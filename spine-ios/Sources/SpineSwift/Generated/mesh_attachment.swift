//
// Spine Runtimes License Agreement
// Last updated April 5, 2025. Replaces all prior versions.
//
// Copyright (c) 2013-2025, Esoteric Software LLC
//
// Integration of the Spine Runtimes into software or otherwise creating
// derivative works of the Spine Runtimes is permitted under the terms and
// conditions of Section 2 of the Spine Editor License Agreement:
// http://esotericsoftware.com/spine-editor-license
//
// Otherwise, it is permitted to integrate the Spine Runtimes into software
// or otherwise create derivative works of the Spine Runtimes (collectively,
// "Products"), provided that each user of the Products must obtain their own
// Spine Editor license and redistribution of the Products in any form must
// include this license and copyright notice.
//
// THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
// BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

// AUTO GENERATED FILE, DO NOT EDIT.

import Foundation
import SpineC

/// Attachment that displays a texture region using a mesh.
@objc(SpineMeshAttachment)
@objcMembers
public class MeshAttachment: VertexAttachment {
    @nonobjc
    public init(fromPointer ptr: spine_mesh_attachment) {
        super.init(fromPointer: UnsafeMutableRawPointer(ptr).assumingMemoryBound(to: spine_vertex_attachment_wrapper.self))
    }

    public convenience init(_ name: String, _ sequence: Sequence?) {
        let ptr = spine_mesh_attachment_create(name, sequence?._ptr.assumingMemoryBound(to: spine_sequence_wrapper.self))
        self.init(fromPointer: ptr!)
    }

    public var regionUVs: ArrayFloat {
        get {
            let result = spine_mesh_attachment_get_region_u_vs(_ptr.assumingMemoryBound(to: spine_mesh_attachment_wrapper.self))
            return ArrayFloat(fromPointer: result!)
        }
        set {
            spine_mesh_attachment_set_region_u_vs(
                _ptr.assumingMemoryBound(to: spine_mesh_attachment_wrapper.self),
                newValue._ptr.assumingMemoryBound(to: spine_array_float_wrapper.self))
        }
    }

    public var triangles: ArrayUnsignedShort {
        get {
            let result = spine_mesh_attachment_get_triangles(_ptr.assumingMemoryBound(to: spine_mesh_attachment_wrapper.self))
            return ArrayUnsignedShort(fromPointer: result!)
        }
        set {
            spine_mesh_attachment_set_triangles(
                _ptr.assumingMemoryBound(to: spine_mesh_attachment_wrapper.self),
                newValue._ptr.assumingMemoryBound(to: spine_array_unsigned_short_wrapper.self))
        }
    }

    public var hullLength: Int32 {
        get {
            let result = spine_mesh_attachment_get_hull_length(_ptr.assumingMemoryBound(to: spine_mesh_attachment_wrapper.self))
            return result
        }
        set {
            spine_mesh_attachment_set_hull_length(_ptr.assumingMemoryBound(to: spine_mesh_attachment_wrapper.self), newValue)
        }
    }

    public var sequence: Sequence {
        let result = spine_mesh_attachment_get_sequence(_ptr.assumingMemoryBound(to: spine_mesh_attachment_wrapper.self))
        return Sequence(fromPointer: result!)
    }

    public var path: String {
        get {
            let result = spine_mesh_attachment_get_path(_ptr.assumingMemoryBound(to: spine_mesh_attachment_wrapper.self))
            return String(cString: result!)
        }
        set {
            spine_mesh_attachment_set_path(_ptr.assumingMemoryBound(to: spine_mesh_attachment_wrapper.self), newValue)
        }
    }

    public var color: Color {
        let result = spine_mesh_attachment_get_color(_ptr.assumingMemoryBound(to: spine_mesh_attachment_wrapper.self))
        return Color(fromPointer: result!)
    }

    /// The parent mesh if this is a linked mesh, else NULL. A linked mesh shares the bones,
    /// vertices, regionUVs, triangles, hullLength, edges, width, and height with the parent mesh,
    /// but may have a different name or path, and therefore a different texture region.
    public var parentMesh: MeshAttachment? {
        get {
            let result = spine_mesh_attachment_get_parent_mesh(_ptr.assumingMemoryBound(to: spine_mesh_attachment_wrapper.self))
            return result.map { MeshAttachment(fromPointer: $0) }
        }
        set {
            spine_mesh_attachment_set_parent_mesh(
                _ptr.assumingMemoryBound(to: spine_mesh_attachment_wrapper.self),
                newValue?._ptr.assumingMemoryBound(to: spine_mesh_attachment_wrapper.self))
        }
    }

    /// Vertex index pairs describing edges for controlling triangulation, or empty if nonessential
    /// data was not exported. Mesh triangles do not cross edges. Triangulation is not performed at
    /// runtime.
    public var edges: ArrayUnsignedShort {
        get {
            let result = spine_mesh_attachment_get_edges(_ptr.assumingMemoryBound(to: spine_mesh_attachment_wrapper.self))
            return ArrayUnsignedShort(fromPointer: result!)
        }
        set {
            spine_mesh_attachment_set_edges(
                _ptr.assumingMemoryBound(to: spine_mesh_attachment_wrapper.self),
                newValue._ptr.assumingMemoryBound(to: spine_array_unsigned_short_wrapper.self))
        }
    }

    public var width: Float {
        get {
            let result = spine_mesh_attachment_get_width(_ptr.assumingMemoryBound(to: spine_mesh_attachment_wrapper.self))
            return result
        }
        set {
            spine_mesh_attachment_set_width(_ptr.assumingMemoryBound(to: spine_mesh_attachment_wrapper.self), newValue)
        }
    }

    public var height: Float {
        get {
            let result = spine_mesh_attachment_get_height(_ptr.assumingMemoryBound(to: spine_mesh_attachment_wrapper.self))
            return result
        }
        set {
            spine_mesh_attachment_set_height(_ptr.assumingMemoryBound(to: spine_mesh_attachment_wrapper.self), newValue)
        }
    }

    public func updateSequence() {
        spine_mesh_attachment_update_sequence(_ptr.assumingMemoryBound(to: spine_mesh_attachment_wrapper.self))
    }

    public func newLinkedMesh() -> MeshAttachment {
        let result = spine_mesh_attachment_new_linked_mesh(_ptr.assumingMemoryBound(to: spine_mesh_attachment_wrapper.self))
        return MeshAttachment(fromPointer: result!)
    }

    /// Computes UVs for a mesh attachment.
    ///
    /// - Parameter uvs: Output array for the computed UVs, same length as regionUVs.
    public static func computeUVs(_ region: TextureRegion?, _ regionUVs: ArrayFloat, _ uvs: ArrayFloat) {
        spine_mesh_attachment_compute_u_vs(
            region?._ptr.assumingMemoryBound(to: spine_texture_region_wrapper.self),
            regionUVs._ptr.assumingMemoryBound(to: spine_array_float_wrapper.self), uvs._ptr.assumingMemoryBound(to: spine_array_float_wrapper.self))
    }

    public func dispose() {
        spine_mesh_attachment_dispose(_ptr.assumingMemoryBound(to: spine_mesh_attachment_wrapper.self))
    }
}
