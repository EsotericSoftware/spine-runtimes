//
//  RenderCommand+Vertices.swift
//  Spine iOS Example
//
//  Created by Denis Andrašec on 08.05.24.
//

import Spine
import SpineSharedStructs
import Foundation

extension RenderCommand {
    func getVertices() -> [AAPLVertex] {
        var vertices = [AAPLVertex]()
        
        let indices = indices
        let positions = positions
        let uvs = uvs
        let colors = colors
        
        for i in 0..<indices.count {
            let index = Int(indices[i])
            
            let xIndex = 2 * index
            let yIndex = xIndex + 1
            
            let positionX = positions[xIndex]
            let positionY = positions[yIndex]
            let uvX = uvs[xIndex]
            let uvY = uvs[yIndex]
            let color = extractRGBA(from: colors[index])
            
            let vertex = AAPLVertex(
                position: vector_float2(positionX, positionY),
                color: color,
                uv: vector_float2(uvX, uvY)
            )
            vertices.append(vertex)
        }
        
        return vertices
    }
    
    private func extractRGBA(from color: Int32) -> vector_float4 {
        guard color != -1 else {
            return vector_float4(1.0, 1.0, 1.0, 1.0)
        }
        let alpha = (color >> 24) & 0xFF
        let red = (color >> 16) & 0xFF
        let green = (color >> 8) & 0xFF
        let blue = color & 0xFF
                
        return vector_float4(Float(red)/255, Float(green)/255, Float(blue)/255, (Float(alpha)/255))
    }
}
