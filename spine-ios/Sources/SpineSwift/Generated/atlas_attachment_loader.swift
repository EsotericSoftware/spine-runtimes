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

/// An AttachmentLoader that configures attachments using texture regions from an Atlas.
///
/// See https://esotericsoftware.com/spine-loading-skeleton-data#JSON-and-binary-data Loading
/// skeleton data in the Spine Runtimes Guide.
@objc(SpineAtlasAttachmentLoader)
@objcMembers
public class AtlasAttachmentLoader: NSObject, AttachmentLoader {
    public let _ptr: UnsafeMutableRawPointer

    public init(fromPointer ptr: spine_atlas_attachment_loader) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        super.init()
    }

    public convenience init(_ atlas: Atlas) {
        let ptr = spine_atlas_attachment_loader_create(atlas._ptr.assumingMemoryBound(to: spine_atlas_wrapper.self))
        self.init(fromPointer: ptr!)
    }

    public func newRegionAttachment(_ skin: Skin, _ name: String, _ path: String, _ sequence: Sequence?) -> RegionAttachment? {
        let result = spine_atlas_attachment_loader_new_region_attachment(_ptr.assumingMemoryBound(to: spine_atlas_attachment_loader_wrapper.self), skin._ptr.assumingMemoryBound(to: spine_skin_wrapper.self), name, path, sequence?._ptr.assumingMemoryBound(to: spine_sequence_wrapper.self))
        return result.map { RegionAttachment(fromPointer: $0) }
    }

    public func newMeshAttachment(_ skin: Skin, _ name: String, _ path: String, _ sequence: Sequence?) -> MeshAttachment? {
        let result = spine_atlas_attachment_loader_new_mesh_attachment(_ptr.assumingMemoryBound(to: spine_atlas_attachment_loader_wrapper.self), skin._ptr.assumingMemoryBound(to: spine_skin_wrapper.self), name, path, sequence?._ptr.assumingMemoryBound(to: spine_sequence_wrapper.self))
        return result.map { MeshAttachment(fromPointer: $0) }
    }

    public func newBoundingBoxAttachment(_ skin: Skin, _ name: String) -> BoundingBoxAttachment? {
        let result = spine_atlas_attachment_loader_new_bounding_box_attachment(_ptr.assumingMemoryBound(to: spine_atlas_attachment_loader_wrapper.self), skin._ptr.assumingMemoryBound(to: spine_skin_wrapper.self), name)
        return result.map { BoundingBoxAttachment(fromPointer: $0) }
    }

    public func newPathAttachment(_ skin: Skin, _ name: String) -> PathAttachment? {
        let result = spine_atlas_attachment_loader_new_path_attachment(_ptr.assumingMemoryBound(to: spine_atlas_attachment_loader_wrapper.self), skin._ptr.assumingMemoryBound(to: spine_skin_wrapper.self), name)
        return result.map { PathAttachment(fromPointer: $0) }
    }

    public func newPointAttachment(_ skin: Skin, _ name: String) -> PointAttachment? {
        let result = spine_atlas_attachment_loader_new_point_attachment(_ptr.assumingMemoryBound(to: spine_atlas_attachment_loader_wrapper.self), skin._ptr.assumingMemoryBound(to: spine_skin_wrapper.self), name)
        return result.map { PointAttachment(fromPointer: $0) }
    }

    public func newClippingAttachment(_ skin: Skin, _ name: String) -> ClippingAttachment? {
        let result = spine_atlas_attachment_loader_new_clipping_attachment(_ptr.assumingMemoryBound(to: spine_atlas_attachment_loader_wrapper.self), skin._ptr.assumingMemoryBound(to: spine_skin_wrapper.self), name)
        return result.map { ClippingAttachment(fromPointer: $0) }
    }

    public func findRegion(_ name: String) -> AtlasRegion? {
        let result = spine_atlas_attachment_loader_find_region(_ptr.assumingMemoryBound(to: spine_atlas_attachment_loader_wrapper.self), name)
        return result.map { AtlasRegion(fromPointer: $0) }
    }

    public func dispose() {
        spine_atlas_attachment_loader_dispose(_ptr.assumingMemoryBound(to: spine_atlas_attachment_loader_wrapper.self))
    }
}