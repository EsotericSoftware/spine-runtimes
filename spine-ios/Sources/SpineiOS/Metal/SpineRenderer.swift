/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

import Foundation
import MetalKit
import SpineC
import SpineShadersStructs
import SpineSwift

protocol SpineRendererDelegate: AnyObject {
    func spineRendererWillUpdate(_ spineRenderer: SpineRenderer)
    func spineRenderer(_ spineRenderer: SpineRenderer, needsUpdate delta: TimeInterval)
    func spineRendererDidUpdate(_ spineRenderer: SpineRenderer)

    func spineRendererWillDraw(_ spineRenderer: SpineRenderer)
    func spineRendererDidDraw(_ spineRenderer: SpineRenderer)

    func spineRendererDidUpdate(_ spineRenderer: SpineRenderer, scaleX: CGFloat, scaleY: CGFloat, offsetX: CGFloat, offsetY: CGFloat, size: CGSize)
}

protocol SpineRendererDataSource: AnyObject {
    func isPlaying(_ spineRenderer: SpineRenderer) -> Bool
    func renderCommands(_ spineRenderer: SpineRenderer) -> [RenderCommand]
}

internal final class SpineRenderer: NSObject, MTKViewDelegate {

    private let device: MTLDevice
    private let textures: [MTLTexture]
    private let samplerStates: [MTLSamplerState]
    private let commandQueue: MTLCommandQueue

    private var sizeInPoints: CGSize = .zero
    private var viewPortSize = vector_uint2(0, 0)
    private var backingScale = CGSize(width: 1, height: 1)
    private var transform = SpineTransform(
        translation: vector_float2(0, 0),
        scale: vector_float2(1, 1),
        offset: vector_float2(0, 0)
    )
    internal var lastDraw: CFTimeInterval = 0
    internal var waitUntilCompleted = false
    private var pipelineStatesByBlendMode = [Int: MTLRenderPipelineState]()

    private static let numberOfBuffers = 3
    private static let defaultBufferSize = 32 * 1024  // 32KB

    private var buffers = [MTLBuffer]()
    private let bufferingSemaphore = DispatchSemaphore(value: SpineRenderer.numberOfBuffers)
    private var currentBufferIndex: Int = 0

    weak var dataSource: SpineRendererDataSource?
    weak var delegate: SpineRendererDelegate?

    internal init(
        device: MTLDevice,
        commandQueue: MTLCommandQueue,
        pixelFormat: MTLPixelFormat,
        atlas: Atlas,
        atlasPages: [SpineUIImage],
        pma: Bool,
        textureFilter: SpineTextureFilter
    ) throws {
        self.device = device
        self.commandQueue = commandQueue

        let bundle: Bundle
        #if SWIFT_PACKAGE  // SPM
            bundle = .module
        #else  // CocoaPods
            let bundleURL = Bundle(for: SpineRenderer.self).url(forResource: "SpineBundle", withExtension: "bundle")
            bundle = Bundle(url: bundleURL!)!
        #endif

        let defaultLibrary = try device.makeDefaultLibrary(bundle: bundle)
        let textureLoader = MTKTextureLoader(device: device)
        var textures = [MTLTexture]()
        var samplerStates = [MTLSamplerState]()
        let pages = atlas.pages
        guard atlasPages.count == pages.count else {
            throw SpineError("The number of atlas page images doesn't match the number of atlas pages")
        }
        for (index, image) in atlasPages.enumerated() {
#if canImport(UIKit)
            guard let cgImage = image.cgImage else {
                throw SpineError("Couldn't get a CGImage for atlas page \(index)")
            }
#elseif canImport(AppKit)
            guard let cgImage = image.cgImage(forProposedRect: nil, context: nil, hints: [:]) else {
                throw SpineError("Couldn't get a CGImage for atlas page \(index)")
            }
#endif
            guard let page = pages[index] else {
                throw SpineError("Couldn't get atlas page \(index)")
            }
            let minFilter = Self.minFilter(for: textureFilter, atlasFilter: page.minFilter)
            let magFilter = Self.magFilter(for: textureFilter, atlasFilter: page.magFilter)
            textures.append(
                try textureLoader.newTexture(
                    cgImage: cgImage,
                    options: [
                        .textureUsage: NSNumber(value: MTLTextureUsage.shaderRead.rawValue),
                        .SRGB: false,
                        .generateMipmaps: NSNumber(value: Self.usesMipmaps(minFilter)),
                    ]
                )
            )

            let samplerDescriptor = MTLSamplerDescriptor()
            samplerDescriptor.minFilter = Self.metalMinFilter(minFilter)
            samplerDescriptor.magFilter = Self.metalMagFilter(magFilter)
            samplerDescriptor.mipFilter = Self.metalMipFilter(minFilter)
            samplerDescriptor.sAddressMode = Self.metalAddressMode(page.uWrap)
            samplerDescriptor.tAddressMode = Self.metalAddressMode(page.vWrap)
            guard let samplerState = device.makeSamplerState(descriptor: samplerDescriptor) else {
                throw SpineError("Couldn't create texture sampler state")
            }
            samplerStates.append(samplerState)
        }
        self.textures = textures
        self.samplerStates = samplerStates

        let blendModes: [BlendMode] = [
            .normal,
            .additive,
            .multiply,
            .screen,
        ]
        for blendMode in blendModes {
            let descriptor = MTLRenderPipelineDescriptor()
            descriptor.vertexFunction = defaultLibrary.makeFunction(name: "vertexShader")
            descriptor.fragmentFunction = defaultLibrary.makeFunction(name: "fragmentShader")
            descriptor.colorAttachments[0].pixelFormat = pixelFormat
            descriptor.colorAttachments[0].apply(
                blendMode: blendMode,
                with: pma
            )
            pipelineStatesByBlendMode[Int(blendMode.rawValue)] = try device.makeRenderPipelineState(descriptor: descriptor)
        }

        super.init()

        increaseBuffersSize(to: SpineRenderer.defaultBufferSize)
    }

    func mtkView(_ view: MTKView, drawableSizeWillChange size: CGSize) {
        guard let spineView = view as? SpineUIView else { return }

        backingScale = CGSize(
            width: spineView.bounds.width > 0 && size.width > 0 ? size.width / spineView.bounds.width : 1,
            height: spineView.bounds.height > 0 && size.height > 0 ? size.height / spineView.bounds.height : 1
        )
        sizeInPoints = CGSize(width: size.width / backingScale.width, height: size.height / backingScale.height)
        viewPortSize = vector_uint2(UInt32(size.width), UInt32(size.height))
        setTransform(
            bounds: spineView.computedBounds,
            mode: spineView.mode,
            alignment: spineView.alignment
        )
    }

    func draw(in view: MTKView) {
        guard dataSource?.isPlaying(self) ?? false else {
            lastDraw = CACurrentMediaTime()
            return
        }

        callNeedsUpdate()

        // Tripple Buffering
        // Source: https://developer.apple.com/library/archive/documentation/3DDrawing/Conceptual/MTLBestPracticesGuide/TripleBuffering.html#//apple_ref/doc/uid/TP40016642-CH5-SW1
        bufferingSemaphore.wait()
        currentBufferIndex = (currentBufferIndex + 1) % SpineRenderer.numberOfBuffers

        guard let renderCommands = dataSource?.renderCommands(self),
            let commandBuffer = commandQueue.makeCommandBuffer(),
            let renderPassDescriptor = view.currentRenderPassDescriptor,
            let renderEncoder = commandBuffer.makeRenderCommandEncoder(descriptor: renderPassDescriptor)
        else {
            // this can happen if,
            // - CAMetalLayer is configured with drawable timeout, and CAMetalLayer is run out of Drawable
            // - CAMetalLayer is added to the window with frame size of zero or incorrect layout constraint -> currentRenderPassDescriptor is null
            bufferingSemaphore.signal()
            return
        }

        delegate?.spineRendererWillDraw(self)
        draw(renderCommands: renderCommands, renderEncoder: renderEncoder, in: view)
        delegate?.spineRendererDidDraw(self)

        renderEncoder.endEncoding()
        view.currentDrawable.flatMap {
            commandBuffer.present($0)
        }
        commandBuffer.addCompletedHandler { [bufferingSemaphore] _ in
            bufferingSemaphore.signal()
        }
        commandBuffer.commit()
        if waitUntilCompleted {
            commandBuffer.waitUntilCompleted()
        }
    }

    private func setTransform(bounds: CGRect, mode: SpineContentMode, alignment: SpineAlignment) {
        let x = -bounds.minX - bounds.width / 2.0
        let y = -bounds.minY - bounds.height / 2.0

        var scaleX: CGFloat = 1.0
        var scaleY: CGFloat = 1.0

        switch mode {
        case .fit:
            scaleX = min(sizeInPoints.width / bounds.width, sizeInPoints.height / bounds.height)
            scaleY = scaleX
        case .fill:
            scaleX = max(sizeInPoints.width / bounds.width, sizeInPoints.height / bounds.height)
            scaleY = scaleX
        }

        let offsetX = abs(sizeInPoints.width - bounds.width * scaleX) / 2 * alignment.x
        let offsetY = abs(sizeInPoints.height - bounds.height * scaleY) / 2 * alignment.y

        transform = SpineTransform(
            translation: vector_float2(Float(x), Float(y)),
            scale: vector_float2(Float(scaleX * backingScale.width), Float(scaleY * backingScale.height)),
            offset: vector_float2(Float(offsetX * backingScale.width), Float(offsetY * backingScale.height))
        )

        delegate?.spineRendererDidUpdate(
            self,
            scaleX: scaleX,
            scaleY: scaleY,
            offsetX: x + offsetX / scaleX,
            offsetY: y + offsetY / scaleY,
            size: sizeInPoints
        )
    }

    private func callNeedsUpdate() {
        if lastDraw == 0 {
            lastDraw = CACurrentMediaTime()
        }
        let delta = CACurrentMediaTime() - lastDraw
        delegate?.spineRendererWillUpdate(self)
        delegate?.spineRenderer(self, needsUpdate: delta)
        lastDraw = CACurrentMediaTime()
        delegate?.spineRendererDidUpdate(self)
    }

    private func draw(renderCommands: [RenderCommand], renderEncoder: MTLRenderCommandEncoder, in view: MTKView) {
        let allVertices = renderCommands.map { renderCommand in
            Array(renderCommand.getVertices())
        }
        let vertices = allVertices.flatMap { $0 }
        let verticesSize = MemoryLayout<SpineVertex>.stride * vertices.count

        guard verticesSize > 0 else {
            return
        }

        var vertexBuffer = buffers[currentBufferIndex]
        var vertexBufferSize = vertexBuffer.length

        if vertexBufferSize < verticesSize {
            increaseBuffersSize(to: verticesSize)
            vertexBuffer = buffers[currentBufferIndex]
        }

        renderEncoder.setViewport(
            MTLViewport(
                originX: 0.0,
                originY: 0.0,
                width: Double(viewPortSize.x),
                height: Double(viewPortSize.y),
                znear: 0.0,
                zfar: 1.0
            )
        )

        memcpy(vertexBuffer.contents(), vertices, verticesSize)

        renderEncoder.setVertexBuffer(
            vertexBuffer,
            offset: 0,
            index: Int(SpineVertexInputIndexVertices.rawValue)
        )
        renderEncoder.setVertexBytes(
            &transform,
            length: MemoryLayout.size(ofValue: transform),
            index: Int(SpineVertexInputIndexTransform.rawValue)
        )
        renderEncoder.setVertexBytes(
            &viewPortSize,
            length: MemoryLayout.size(ofValue: viewPortSize),
            index: Int(SpineVertexInputIndexViewportSize.rawValue)
        )

        // Buffer Bindings
        // https://developer.apple.com/library/archive/documentation/3DDrawing/Conceptual/MTLBestPracticesGuide/BufferBindings.html#//apple_ref/doc/uid/TP40016642-CH28-SW3
        var vertexStart = 0
        for (index, renderCommand) in renderCommands.enumerated() {
            guard let pipelineState = getPipelineState(blendMode: renderCommand.blendMode) else {
                continue
            }
            renderEncoder.setRenderPipelineState(pipelineState)

            let vertices = allVertices[index]

            // When using spine_atlas_load, texture is actually the atlas page index cast as a pointer
            let textureIndex = Int(bitPattern: renderCommand.texture)
            if textures.indices.contains(textureIndex), samplerStates.indices.contains(textureIndex) {
                renderEncoder.setFragmentTexture(
                    textures[textureIndex],
                    index: Int(SpineTextureIndexBaseColor.rawValue)
                )
                renderEncoder.setFragmentSamplerState(
                    samplerStates[textureIndex],
                    index: Int(SpineSamplerIndexTexture.rawValue)
                )
            }

            renderEncoder.drawPrimitives(
                type: .triangle,
                vertexStart: vertexStart,
                vertexCount: vertices.count
            )
            vertexStart += vertices.count
        }
    }

    private static func minFilter(for textureFilter: SpineTextureFilter, atlasFilter: TextureFilter) -> TextureFilter {
        switch textureFilter {
        case .atlas: return atlasFilter
        case .nearest: return .nearest
        case .linear: return .linear
        }
    }

    private static func magFilter(for textureFilter: SpineTextureFilter, atlasFilter: TextureFilter) -> TextureFilter {
        switch textureFilter {
        case .atlas: return atlasFilter
        case .nearest: return .nearest
        case .linear: return .linear
        }
    }

    private static func usesMipmaps(_ textureFilter: TextureFilter) -> Bool {
        switch textureFilter {
        case .mipMap, .mipMapNearestNearest, .mipMapLinearNearest, .mipMapNearestLinear, .mipMapLinearLinear:
            return true
        default:
            return false
        }
    }

    private static func metalMinFilter(_ textureFilter: TextureFilter) -> MTLSamplerMinMagFilter {
        switch textureFilter {
        case .linear, .mipMap, .mipMapLinearNearest, .mipMapLinearLinear:
            return .linear
        default:
            return .nearest
        }
    }

    private static func metalMagFilter(_ textureFilter: TextureFilter) -> MTLSamplerMinMagFilter {
        switch textureFilter {
        case .linear, .mipMap, .mipMapNearestLinear, .mipMapLinearLinear:
            return .linear
        default:
            return .nearest
        }
    }

    private static func metalMipFilter(_ textureFilter: TextureFilter) -> MTLSamplerMipFilter {
        switch textureFilter {
        case .mipMapNearestNearest, .mipMapLinearNearest:
            return .nearest
        case .mipMap, .mipMapNearestLinear, .mipMapLinearLinear:
            return .linear
        default:
            return .notMipmapped
        }
    }

    private static func metalAddressMode(_ textureWrap: TextureWrap) -> MTLSamplerAddressMode {
        switch textureWrap {
        case .mirroredRepeat: return .mirrorRepeat
        case .clampToEdge: return .clampToEdge
        case .repeat: return .repeat
        }
    }

    private func getPipelineState(blendMode: BlendMode) -> MTLRenderPipelineState? {
        pipelineStatesByBlendMode[Int(blendMode.rawValue)]
    }

    private func increaseBuffersSize(to size: Int) {
        buffers = (0..<SpineRenderer.numberOfBuffers).map { _ in
            device.makeBuffer(length: size, options: .storageModeShared)!
        }
    }
}

extension BlendMode {
    fileprivate func sourceRGBBlendFactor(premultipliedAlpha: Bool) -> MTLBlendFactor {
        switch self {
        case .normal:
            return premultipliedAlpha ? .one : .sourceAlpha
        case .additive:
            // additvie only needs sourceAlpha multiply if it is not pma
            return premultipliedAlpha ? .one : .sourceAlpha
        case .multiply:
            return .destinationColor
        case .screen:
            return .one
        }
    }

    fileprivate var sourceAlphaBlendFactor: MTLBlendFactor {
        // pma and non-pma has no-relation ship with alpha blending
        switch self {
        case .normal:
            return .one
        case .additive:
            return .one
        case .multiply:
            return .oneMinusSourceAlpha
        case .screen:
            return .oneMinusSourceColor
        }
    }

    fileprivate var destinationRGBBlendFactor: MTLBlendFactor {
        switch self {
        case .normal:
            return .oneMinusSourceAlpha
        case .additive:
            return .one
        case .multiply:
            return .oneMinusSourceAlpha
        case .screen:
            return .oneMinusSourceColor
        }
    }

    fileprivate var destinationAlphaBlendFactor: MTLBlendFactor {
        switch self {
        case .normal:
            return .oneMinusSourceAlpha
        case .additive:
            return .one
        case .multiply:
            return .oneMinusSourceAlpha
        case .screen:
            return .oneMinusSourceColor
        }
    }
}

extension MTLRenderPipelineColorAttachmentDescriptor {

    fileprivate func apply(blendMode: BlendMode, with premultipliedAlpha: Bool) {
        isBlendingEnabled = true
        sourceRGBBlendFactor = blendMode.sourceRGBBlendFactor(premultipliedAlpha: premultipliedAlpha)
        sourceAlphaBlendFactor = blendMode.sourceAlphaBlendFactor
        destinationRGBBlendFactor = blendMode.destinationRGBBlendFactor
        destinationAlphaBlendFactor = blendMode.destinationAlphaBlendFactor
    }
}
