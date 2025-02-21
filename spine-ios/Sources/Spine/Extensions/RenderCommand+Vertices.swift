import SpineShadersStructs
import Foundation
import simd

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
            let position = SIMD2<Float>(positions[xIndex], positions[yIndex])
            let uv = SIMD2<Float>(uvs[xIndex], uvs[yIndex])
            let color = extractRGBA(from: colors[index])
            let vertex = SpineVertex(
                position: position,
                color: color,
                uv: uv
            )
            vertices.append(vertex)
        }
        
        return vertices
    }

    private func extractRGBA(from color: Int32) -> SIMD4<Float> {
        guard color != -1 else {
            return SIMD4<Float>(1.0, 1.0, 1.0, 1.0)
        }
        let alpha = Float((color >> 24) & 0xFF) / 255.0
        let red = Float((color >> 16) & 0xFF) / 255.0
        let green = Float((color >> 8) & 0xFF) / 255.0
        let blue = Float(color & 0xFF) / 255.0
        return SIMD4<Float>(red, green, blue, alpha)
    }
}
