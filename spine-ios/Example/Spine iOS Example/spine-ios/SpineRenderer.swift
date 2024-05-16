//
//  SpineRenderer.swift
//  Spine iOS Example
//
//  Created by Denis Andrašec on 17.04.24.
//

import Foundation
import MetalKit
import SpineSharedStructs
import Spine
import SpineWrapper

protocol SpineRendererDelegate: AnyObject {
    func spineRendererWillUpdate(_ spineRenderer: SpineRenderer)
    func spineRenderer(_ spineRenderer: SpineRenderer, needsUpdate delta: TimeInterval)
    func spineRendererDidUpdate(_ spineRenderer: SpineRenderer)
    
    func spineRendererWillDraw(_ spineRenderer: SpineRenderer)
    func spineRendererDidDraw(_ spineRenderer: SpineRenderer)
    
    func spineRendererDidUpdate(_ spineRenderer: SpineRenderer, scaleX: CGFloat, scaleY: CGFloat, offsetX: CGFloat, offsetY: CGFloat)
}

protocol SpineRendererDataSource: AnyObject {
    func isPlaying(_ spineRenderer: SpineRenderer) -> Bool
    func skeletonDrawable(_ spineRenderer: SpineRenderer) -> SkeletonDrawableWrapper
    func renderCommands(_ spineRenderer: SpineRenderer) -> [RenderCommand]
}

final class SpineRenderer: NSObject, MTKViewDelegate {
    private let contentMode: Spine.ContentMode
    private let alignment: Spine.Alignment
    private let boundsProvider: BoundsProvider
    private let device: MTLDevice
    private let textures: [MTLTexture]
    private let pipelineState: MTLRenderPipelineState
    private let commandQueue: MTLCommandQueue
    
    private var sizeInPoints: CGSize = .zero
    private var viewPortSize = vector_uint2(0, 0)
    private var transform = AAPLTransform(
        translation: vector_float2(0, 0),
        scale: vector_float2(1, 1),
        offset: vector_float2(0, 0)
    )
    private var lastDraw: CFTimeInterval = 0
    
    weak var dataSource: SpineRendererDataSource?
    weak var delegate: SpineRendererDelegate?
    
    init(
        mtkView: MTKView,
        atlasPages: [CGImage],
        contentMode: Spine.ContentMode,
        alignment: Spine.Alignment,
        boundsProvider: BoundsProvider
    ) throws {
        let device = mtkView.device!
        self.device = device
        
        let defaultLibrary = device.makeDefaultLibrary()
        let textureLoader = MTKTextureLoader(device: device)
        textures = try atlasPages.map {
            try textureLoader.newTexture(
                cgImage: $0,
                options: [
                    .textureUsage: NSNumber(value: MTLTextureUsage.shaderRead.rawValue),
                    .SRGB: false,
                ]
            )
        }
        self.contentMode = contentMode
        self.alignment = alignment
        self.boundsProvider = boundsProvider
        
        // TODO: Create pipeline state for all blend mode / premultiplied combinations and choose the correct one when drawing vertices
        let pipelineStateDescriptor = MTLRenderPipelineDescriptor()
        pipelineStateDescriptor.vertexFunction = defaultLibrary?.makeFunction(name: "vertexShader")
        pipelineStateDescriptor.fragmentFunction = defaultLibrary?.makeFunction(name: "fragmentShader")
        pipelineStateDescriptor.colorAttachments[0].pixelFormat = mtkView.colorPixelFormat
        pipelineStateDescriptor.colorAttachments[0].apply(
            blendMode: SPINE_BLEND_MODE_NORMAL,
            with: true
        )
        pipelineState = try device.makeRenderPipelineState(descriptor: pipelineStateDescriptor)
        commandQueue = device.makeCommandQueue()!
    }
    
    func mtkView(_ view: MTKView, drawableSizeWillChange size: CGSize) {
        self.sizeInPoints = CGSize(width: size.width / UIScreen.main.scale, height: size.height / UIScreen.main.scale)
        viewPortSize = vector_uint2(UInt32(size.width), UInt32(size.height))
        if let drawable = dataSource?.skeletonDrawable(self) {
            setTransform(
                bounds: boundsProvider.computeBounds(for: drawable),
                contentMode: contentMode,
                alignment: alignment
            )
        }
    }
    
    func draw(in view: MTKView) {
        guard dataSource?.isPlaying(self) ?? false else {
            lastDraw = CACurrentMediaTime()
            return
        }
        
        callNeedsUpdate()
        let renderCommands = dataSource?.renderCommands(self) ?? []
        
        guard let commandBuffer = commandQueue.makeCommandBuffer() else {
            return
        }
        
        guard let renderPassDescriptor = view.currentRenderPassDescriptor else {
            return
        }
        
        guard let renderEncoder = commandBuffer.makeRenderCommandEncoder(descriptor: renderPassDescriptor) else {
            return
        }
        
        delegate?.spineRendererWillDraw(self)
        for renderCommand in renderCommands {
            draw(renderCommand: renderCommand, renderEncoder: renderEncoder, in: view)
        }
        delegate?.spineRendererDidDraw(self)
        
        renderEncoder.endEncoding()
        view.currentDrawable.flatMap {
            commandBuffer.present($0)
        }
        commandBuffer.commit()
    }
    
    private func setTransform(bounds: CGRect, contentMode: Spine.ContentMode, alignment: Spine.Alignment) {
        let x = -bounds.minX - bounds.width / 2.0// - (alignment.x * bounds.width / 2.0)
        let y = -bounds.minY - bounds.height / 2.0// - (alignment.y * bounds.height / 2.0)
        
        var scaleX: CGFloat = 1.0
        var scaleY: CGFloat = 1.0
        
        switch contentMode {
        case .fit:
            scaleX = min(sizeInPoints.width / bounds.width, sizeInPoints.height / bounds.height)
            scaleY = scaleX
        case .fill:
            scaleX = max(sizeInPoints.width / bounds.width, sizeInPoints.height / bounds.height)
            scaleY = scaleX
        }
        
        let offsetX = abs(sizeInPoints.width - bounds.width) / 2 * alignment.x
        let offsetY = abs(sizeInPoints.height - bounds.height) / 2 * alignment.y
        
//        let offsetX = sizeInPoints.width / 2.0 + (alignment.x * sizeInPoints.width / 2.0)
//        let offsetY = sizeInPoints.height / 2.0 + (alignment.y * sizeInPoints.height / 2.0)
        
        transform = AAPLTransform(
            translation: vector_float2(Float(x), Float(y)),
            scale: vector_float2(Float(scaleX * UIScreen.main.scale), Float(scaleY * UIScreen.main.scale)),
            offset: vector_float2(Float(offsetX * UIScreen.main.scale), Float(offsetY * UIScreen.main.scale))
        )
        
        delegate?.spineRendererDidUpdate(
            self,
            scaleX: scaleX,
            scaleY: scaleY,
            offsetX: x / scaleX + bounds.width / 2.0 + offsetX,
            offsetY: y / scaleY + bounds.height / 2.0 + offsetY
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
    
    private func draw(renderCommand: RenderCommand, renderEncoder: MTLRenderCommandEncoder, in view: MTKView) {
        let vertices = Array(renderCommand.getVertices())
        
        guard !vertices.isEmpty else {
            return
        }
        let verticesBufferSize = MemoryLayout<AAPLVertex>.stride * vertices.count
        
        guard let vertexBuffer = device.makeBuffer(length: verticesBufferSize, options: .storageModeShared) else {
            return
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
        renderEncoder.setRenderPipelineState(pipelineState)
        
        memcpy(vertexBuffer.contents(), vertices, verticesBufferSize)
        
        renderEncoder.setVertexBuffer(
            vertexBuffer,
            offset: 0,
            index: Int(AAPLVertexInputIndexVertices.rawValue)
        )
        renderEncoder.setVertexBytes(
            &transform,
            length: MemoryLayout.size(ofValue: transform),
            index: Int(AAPLVertexInputIndexTransform.rawValue)
        )
        renderEncoder.setVertexBytes(
            &viewPortSize,
            length: MemoryLayout.size(ofValue: viewPortSize),
            index: Int(AAPLVertexInputIndexViewportSize.rawValue)
        )
        
        let textureIndex = Int(renderCommand.atlasPage)
        if textures.indices.contains(textureIndex) {
            renderEncoder.setFragmentTexture(
                textures[textureIndex],
                index: Int(AAPLTextureIndexBaseColor.rawValue)
            )
        }
        
        renderEncoder.drawPrimitives(
            type: .triangle,
            vertexStart: 0,
            vertexCount: vertices.count
        )
    }
}

fileprivate extension MTLRenderPipelineColorAttachmentDescriptor {
    
    func apply(blendMode: BlendMode, with premultipliedAlpha: Bool) {
        isBlendingEnabled = true
        sourceRGBBlendFactor = blendMode.sourceRGBBlendFactor(premultipliedAlpha: premultipliedAlpha)
        destinationRGBBlendFactor = blendMode.destinationRGBBlendFactor
        destinationAlphaBlendFactor = .oneMinusSourceAlpha
    }
}

fileprivate extension BlendMode {
    func sourceRGBBlendFactor(premultipliedAlpha: Bool) -> MTLBlendFactor {
        switch self {
        case SPINE_BLEND_MODE_NORMAL, SPINE_BLEND_MODE_ADDITIVE:
            return premultipliedAlpha ? .one : .sourceAlpha
        case SPINE_BLEND_MODE_MULTIPLY:
            return .destinationColor
        case SPINE_BLEND_MODE_SCREEN:
            return .one
        default:
            return .one // Should never be called
        }
    }
    
    var destinationRGBBlendFactor: MTLBlendFactor {
        switch self {
        case SPINE_BLEND_MODE_NORMAL, SPINE_BLEND_MODE_ADDITIVE:
            return .oneMinusSourceAlpha
        case SPINE_BLEND_MODE_MULTIPLY:
            return .one
        case SPINE_BLEND_MODE_SCREEN:
            return .oneMinusSourceColor
        default:
            return .one // Should never be called
        }
    }
}
