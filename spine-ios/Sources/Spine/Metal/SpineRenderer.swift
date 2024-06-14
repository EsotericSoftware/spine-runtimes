import Foundation
import MetalKit
import SpineShadersStructs
import Spine
import SpineCppLite

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
    private let commandQueue: MTLCommandQueue
    
    private var sizeInPoints: CGSize = .zero
    private var viewPortSize = vector_uint2(0, 0)
    private var transform = SpineTransform(
        translation: vector_float2(0, 0),
        scale: vector_float2(1, 1),
        offset: vector_float2(0, 0)
    )
    internal var lastDraw: CFTimeInterval = 0
    internal var waitUntilCompleted = false
    private var pipelineStatesByBlendMode = [Int: MTLRenderPipelineState]()
    
    private static let numberOfBuffers = 3
    private static let defaultBufferSize = 32 * 1024 // 32KB
    
    private var buffers = [MTLBuffer]()
    private let bufferingSemaphore = DispatchSemaphore(value: SpineRenderer.numberOfBuffers)
    private var currentBufferIndex: Int = 0
    
    weak var dataSource: SpineRendererDataSource?
    weak var delegate: SpineRendererDelegate?
    
    internal init(
        device: MTLDevice,
        commandQueue: MTLCommandQueue,
        pixelFormat: MTLPixelFormat,
        atlasPages: [UIImage],
        pma: Bool
    ) throws {
        self.device = device
        self.commandQueue = commandQueue
        
        let bundle: Bundle
        #if SWIFT_PACKAGE // SPM
        bundle = .module
        #else // CocoaPods
        bundle = Bundle(for: SpineRenderer.self)
        #endif
        
        let defaultLibrary = try device.makeDefaultLibrary(bundle: bundle)
        let textureLoader = MTKTextureLoader(device: device)
        textures = try atlasPages
            .compactMap { $0.cgImage }
            .map {
                try textureLoader.newTexture(
                    cgImage: $0,
                    options: [
                        .textureUsage: NSNumber(value: MTLTextureUsage.shaderRead.rawValue),
                        .SRGB: false,
                    ]
                )
            }
        
        let blendModes = [
            SPINE_BLEND_MODE_NORMAL,
            SPINE_BLEND_MODE_ADDITIVE,
            SPINE_BLEND_MODE_MULTIPLY,
            SPINE_BLEND_MODE_SCREEN
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
        
        sizeInPoints = CGSize(width: size.width / UIScreen.main.scale, height: size.height / UIScreen.main.scale)
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
              let renderEncoder = commandBuffer.makeRenderCommandEncoder(descriptor: renderPassDescriptor) else {
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
    
    private func setTransform(bounds: CGRect, mode: Spine.ContentMode, alignment: Spine.Alignment) {
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
            scale: vector_float2(Float(scaleX * UIScreen.main.scale), Float(scaleY * UIScreen.main.scale)),
            offset: vector_float2(Float(offsetX * UIScreen.main.scale), Float(offsetY * UIScreen.main.scale))
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
            
            let textureIndex = Int(renderCommand.atlasPage)
            if textures.indices.contains(textureIndex) {
                renderEncoder.setFragmentTexture(
                    textures[textureIndex],
                    index: Int(SpineTextureIndexBaseColor.rawValue)
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
    
    private func getPipelineState(blendMode: BlendMode) -> MTLRenderPipelineState? {
        pipelineStatesByBlendMode[Int(blendMode.rawValue)]
    }
    
    private func increaseBuffersSize(to size: Int) {
        buffers = (0 ..< SpineRenderer.numberOfBuffers).map { _ in
            device.makeBuffer(length: size, options: .storageModeShared)!
        }
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
