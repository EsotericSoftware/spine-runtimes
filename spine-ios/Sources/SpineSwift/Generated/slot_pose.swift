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

/// Stores a slot's pose.
@objc(SpineSlotPose)
@objcMembers
public class SlotPose: NSObject {
    public let _ptr: UnsafeMutableRawPointer

    public init(fromPointer ptr: spine_slot_pose) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        super.init()
    }

    public override convenience init() {
        let ptr = spine_slot_pose_create()
        self.init(fromPointer: ptr!)
    }

    /// The color used to tint the slot's attachment. If a dark color is set, this is used as the
    /// light color for two color tinting.
    public var color: Color {
        let result = spine_slot_pose_get_color(_ptr.assumingMemoryBound(to: spine_slot_pose_wrapper.self))
        return Color(fromPointer: result!)
    }

    /// The dark color used to tint the slot's attachment for two color tinting. The dark color's
    /// alpha is not used.
    public var darkColor: Color {
        let result = spine_slot_pose_get_dark_color(_ptr.assumingMemoryBound(to: spine_slot_pose_wrapper.self))
        return Color(fromPointer: result!)
    }

    /// Returns true if this slot has a dark color.
    public var hasDarkColor: Bool {
        get {
            let result = spine_slot_pose_has_dark_color(_ptr.assumingMemoryBound(to: spine_slot_pose_wrapper.self))
            return result
        }
        set {
            spine_slot_pose_set_has_dark_color(_ptr.assumingMemoryBound(to: spine_slot_pose_wrapper.self), newValue)
        }
    }

    /// The current attachment for the slot, or null if the slot has no attachment.
    public var attachment: Attachment? {
        get {
            let result = spine_slot_pose_get_attachment(_ptr.assumingMemoryBound(to: spine_slot_pose_wrapper.self))
            guard let ptr = result else { return nil }
            let rtti = spine_attachment_get_rtti(ptr)
            let rttiClassName = String(cString: spine_rtti_get_class_name(rtti)!)
            switch rttiClassName {
            case "BoundingBoxAttachment":
                let castedPtr = spine_attachment_cast_to_bounding_box_attachment(ptr)
                return BoundingBoxAttachment(fromPointer: castedPtr!)
            case "ClippingAttachment":
                let castedPtr = spine_attachment_cast_to_clipping_attachment(ptr)
                return ClippingAttachment(fromPointer: castedPtr!)
            case "MeshAttachment":
                let castedPtr = spine_attachment_cast_to_mesh_attachment(ptr)
                return MeshAttachment(fromPointer: castedPtr!)
            case "PathAttachment":
                let castedPtr = spine_attachment_cast_to_path_attachment(ptr)
                return PathAttachment(fromPointer: castedPtr!)
            case "PointAttachment":
                let castedPtr = spine_attachment_cast_to_point_attachment(ptr)
                return PointAttachment(fromPointer: castedPtr!)
            case "RegionAttachment":
                let castedPtr = spine_attachment_cast_to_region_attachment(ptr)
                return RegionAttachment(fromPointer: castedPtr!)
            default:
                fatalError("Unknown concrete type: \(rttiClassName) for abstract class Attachment")
            }
        }
        set {
            spine_slot_pose_set_attachment(
                _ptr.assumingMemoryBound(to: spine_slot_pose_wrapper.self), newValue?._ptr.assumingMemoryBound(to: spine_attachment_wrapper.self))
        }
    }

    /// The index of the texture region to display when the slot's attachment has a Sequence. -1
    /// represents the Sequence::getSetupIndex().
    public var sequenceIndex: Int32 {
        get {
            let result = spine_slot_pose_get_sequence_index(_ptr.assumingMemoryBound(to: spine_slot_pose_wrapper.self))
            return result
        }
        set {
            spine_slot_pose_set_sequence_index(_ptr.assumingMemoryBound(to: spine_slot_pose_wrapper.self), newValue)
        }
    }

    /// Values to deform the slot's attachment. For an unweighted mesh, the entries are local
    /// positions for each vertex. For a weighted mesh, the entries are an offset for each vertex
    /// which will be added to the mesh's local vertex positions.
    ///
    /// See VertexAttachment::computeWorldVertices() and DeformTimeline.
    public var deform: ArrayFloat {
        let result = spine_slot_pose_get_deform(_ptr.assumingMemoryBound(to: spine_slot_pose_wrapper.self))
        return ArrayFloat(fromPointer: result!)
    }

    public func set(_ pose: SlotPose) {
        spine_slot_pose_set(
            _ptr.assumingMemoryBound(to: spine_slot_pose_wrapper.self), pose._ptr.assumingMemoryBound(to: spine_slot_pose_wrapper.self))
    }

    public func dispose() {
        spine_slot_pose_dispose(_ptr.assumingMemoryBound(to: spine_slot_pose_wrapper.self))
    }
}
