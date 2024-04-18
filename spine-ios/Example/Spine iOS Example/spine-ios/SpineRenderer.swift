//
//  SpineRenderer.swift
//  Spine iOS Example
//
//  Created by Denis Andrašec on 17.04.24.
//

import Foundation
import MetalKit
import SpineSharedStructs

final class SpineRenderer: NSObject, MTKViewDelegate {
    
    let mtkView: MTKView
    let renderCommand: RenderCommand
    let device: MTLDevice
    let texture: MTLTexture?
    let pipelineState: MTLRenderPipelineState
    let commandQueue: MTLCommandQueue
    
    var viewPortSize = vector_uint2(0, 0)
    
    init(mtkView: MTKView, renderCommand: RenderCommand, imageURL: URL) throws {
        self.mtkView = mtkView
        self.renderCommand = renderCommand
        
        let device = mtkView.device!
        self.device = device
        
        let defaultLibrary = device.makeDefaultLibrary()
        let textureLoader = MTKTextureLoader(device: device)
        
        texture = try textureLoader.newTexture(
            URL: imageURL,
            options: [
                .textureUsage: NSNumber(value: MTLTextureUsage.shaderRead.rawValue),
                .SRGB: false,
            ]
        )
        
        let pipelineStateDescriptor = MTLRenderPipelineDescriptor()
        pipelineStateDescriptor.vertexFunction = defaultLibrary?.makeFunction(name: "vertexShader")
        pipelineStateDescriptor.fragmentFunction = defaultLibrary?.makeFunction(name: "fragmentShader")
        pipelineStateDescriptor.colorAttachments[0].pixelFormat = mtkView.colorPixelFormat
        pipelineStateDescriptor.colorAttachments[0].apply(
            blendMode: renderCommand.blendMode,
            with: renderCommand.premultipliedAlpha
        )
        
        pipelineState = try device.makeRenderPipelineState(descriptor: pipelineStateDescriptor)
        commandQueue = device.makeCommandQueue()!
    }
    
    func mtkView(_ view: MTKView, drawableSizeWillChange size: CGSize) {
        viewPortSize = vector_uint2(UInt32(size.width), UInt32(size.height))
    }
    
    func draw(in view: MTKView) {
        let vertices = Array(renderCommand.getVertices())
        let verticesBufferSize = MemoryLayout<AAPLVertex>.stride * vertices.count
        
        guard let commandBuffer = commandQueue.makeCommandBuffer() else {
            return
        }
        
        guard let renderPassDescriptor = view.currentRenderPassDescriptor else {
            return
        }
        
        guard let renderEncoder = commandBuffer.makeRenderCommandEncoder(descriptor: renderPassDescriptor) else {
            return
        }
        
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
            &viewPortSize,
            length: MemoryLayout.size(ofValue: viewPortSize),
            index: Int(AAPLVertexInputIndexViewportSize.rawValue)
        )
        
        if let texture {
            renderEncoder.setFragmentTexture(
                texture,
                index: Int(AAPLTextureIndexBaseColor.rawValue)
            )
        }
        
        renderEncoder.drawPrimitives(
            type: .triangle,
            vertexStart: 0,
            vertexCount: vertices.count
        )

        renderEncoder.endEncoding()
        view.currentDrawable.flatMap {
            commandBuffer.present($0)
        }
        commandBuffer.commit()
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
        case .normal, .additive:
            return premultipliedAlpha ? .one : .sourceAlpha
        case .multiply:
            return .destinationColor
        case .screen:
            return .one
        }
    }
    
    var destinationRGBBlendFactor: MTLBlendFactor {
        switch self {
        case .normal, .multiply:
            return .oneMinusSourceAlpha
        case .additive:
            return .one
        case .screen:
            return .oneMinusSourceColor
        }
    }
}
