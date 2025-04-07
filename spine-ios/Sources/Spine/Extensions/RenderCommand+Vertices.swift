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

import SpineShadersStructs
import Foundation

extension RenderCommand {
    func getVertices() -> [SpineVertex] {
        var vertices = [SpineVertex]()
        
        let indices = indices
        let numVertices = numVertices
        let positions = positions(numVertices: numVertices)
        let uvs = uvs(numVertices: numVertices)
        let colors = colors(numVertices: numVertices)
        vertices.reserveCapacity(indices.count)
        for i in 0..<indices.count {
            let index = Int(indices[i])
            
            let xIndex = 2 * index
            let yIndex = xIndex + 1
            
            let positionX = positions[xIndex]
            let positionY = positions[yIndex]
            let uvX = uvs[xIndex]
            let uvY = uvs[yIndex]
            let color = extractRGBA(from: colors[index])
            
            let vertex = SpineVertex(
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
