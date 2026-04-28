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

/// SkeletonClipping wrapper
@objc(SpineSkeletonClipping)
@objcMembers
public class SkeletonClipping: NSObject {
    public let _ptr: UnsafeMutableRawPointer

    public init(fromPointer ptr: spine_skeleton_clipping) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        super.init()
    }

    public override convenience init() {
        let ptr = spine_skeleton_clipping_create()
        self.init(fromPointer: ptr!)
    }

    public var isClipping: Bool {
        let result = spine_skeleton_clipping_is_clipping(_ptr.assumingMemoryBound(to: spine_skeleton_clipping_wrapper.self))
        return result
    }

    public var clippedVertices: ArrayFloat {
        let result = spine_skeleton_clipping_get_clipped_vertices(_ptr.assumingMemoryBound(to: spine_skeleton_clipping_wrapper.self))
        return ArrayFloat(fromPointer: result!)
    }

    public var clippedTriangles: ArrayUnsignedShort {
        let result = spine_skeleton_clipping_get_clipped_triangles(_ptr.assumingMemoryBound(to: spine_skeleton_clipping_wrapper.self))
        return ArrayUnsignedShort(fromPointer: result!)
    }

    public var clippedUVs: ArrayFloat {
        let result = spine_skeleton_clipping_get_clipped_u_vs(_ptr.assumingMemoryBound(to: spine_skeleton_clipping_wrapper.self))
        return ArrayFloat(fromPointer: result!)
    }

    public func clipStart(_ skeleton: Skeleton, _ slot: Slot, _ clip: ClippingAttachment?) -> Int {
        let result = spine_skeleton_clipping_clip_start(
            _ptr.assumingMemoryBound(to: spine_skeleton_clipping_wrapper.self), skeleton._ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self),
            slot._ptr.assumingMemoryBound(to: spine_slot_wrapper.self), clip?._ptr.assumingMemoryBound(to: spine_clipping_attachment_wrapper.self))
        return result
    }

    public func clipEnd(_ slot: Slot) {
        spine_skeleton_clipping_clip_end_1(
            _ptr.assumingMemoryBound(to: spine_skeleton_clipping_wrapper.self), slot._ptr.assumingMemoryBound(to: spine_slot_wrapper.self))
    }

    public func clipEnd2() {
        spine_skeleton_clipping_clip_end_2(_ptr.assumingMemoryBound(to: spine_skeleton_clipping_wrapper.self))
    }

    public func clipTriangles(_ vertices: ArrayFloat, _ triangles: ArrayUnsignedShort, _ uvs: ArrayFloat, _ stride: Int) -> Bool {
        let result = spine_skeleton_clipping_clip_triangles_3(
            _ptr.assumingMemoryBound(to: spine_skeleton_clipping_wrapper.self), vertices._ptr.assumingMemoryBound(to: spine_array_float_wrapper.self),
            triangles._ptr.assumingMemoryBound(to: spine_array_unsigned_short_wrapper.self),
            uvs._ptr.assumingMemoryBound(to: spine_array_float_wrapper.self), stride)
        return result
    }

    public func dispose() {
        spine_skeleton_clipping_dispose(_ptr.assumingMemoryBound(to: spine_skeleton_clipping_wrapper.self))
    }
}
