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
        
        for i in 0..<indices.count {
            let index = Int(indices[i])
            
            let xIndex = 2 * index
            let yIndex = xIndex + 1
            
            let positionX = positions[xIndex]
            let positionY = positions[yIndex]
            let uvX = uvs[xIndex]
            let uvY = uvs[yIndex]
            
            let vertex = AAPLVertex(
                position: vector_float2(positionX, positionY),
                color: vector_float4(1.0, 1.0, 1.0, 1.0), // TODO: Correct Color
                uv: vector_float2(uvX, uvY)
            )
            vertices.append(vertex)
        }
        
        return vertices
    }
}
