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

/// Changes the sequence index for an attachment's Sequence.
@objc(SpineSequenceTimeline)
@objcMembers
public class SequenceTimeline: Timeline, SlotTimeline {
    @nonobjc
    public init(fromPointer ptr: spine_sequence_timeline) {
        super.init(fromPointer: UnsafeMutableRawPointer(ptr).assumingMemoryBound(to: spine_timeline_wrapper.self))
    }

    public convenience init(_ frameCount: Int, _ slotIndex: Int32, _ attachment: Attachment) {
        let ptr = spine_sequence_timeline_create(frameCount, slotIndex, attachment._ptr.assumingMemoryBound(to: spine_attachment_wrapper.self))
        self.init(fromPointer: ptr!)
    }

    /// The attachment for which the sequence index will be set.
    ///
    /// See Attachment::getTimelineAttachment().
    public var attachment: Attachment {
        let result = spine_sequence_timeline_get_attachment(_ptr.assumingMemoryBound(to: spine_sequence_timeline_wrapper.self))
        let rtti = spine_attachment_get_rtti(result!)
        let rttiClassName = String(cString: spine_rtti_get_class_name(rtti)!)
        switch rttiClassName {
        case "BoundingBoxAttachment":
            let castedPtr = spine_attachment_cast_to_bounding_box_attachment(result!)
            return BoundingBoxAttachment(fromPointer: castedPtr!)
        case "ClippingAttachment":
            let castedPtr = spine_attachment_cast_to_clipping_attachment(result!)
            return ClippingAttachment(fromPointer: castedPtr!)
        case "MeshAttachment":
            let castedPtr = spine_attachment_cast_to_mesh_attachment(result!)
            return MeshAttachment(fromPointer: castedPtr!)
        case "PathAttachment":
            let castedPtr = spine_attachment_cast_to_path_attachment(result!)
            return PathAttachment(fromPointer: castedPtr!)
        case "PointAttachment":
            let castedPtr = spine_attachment_cast_to_point_attachment(result!)
            return PointAttachment(fromPointer: castedPtr!)
        case "RegionAttachment":
            let castedPtr = spine_attachment_cast_to_region_attachment(result!)
            return RegionAttachment(fromPointer: castedPtr!)
        default:
            fatalError("Unknown concrete type: \(rttiClassName) for abstract class Attachment")
        }
    }

    public var slotIndex: Int32 {
        get {
            let result = spine_sequence_timeline_get_slot_index(_ptr.assumingMemoryBound(to: spine_sequence_timeline_wrapper.self))
        return result
        }
        set {
            spine_sequence_timeline_set_slot_index(_ptr.assumingMemoryBound(to: spine_sequence_timeline_wrapper.self), newValue)
        }
    }

    /// Sets the time, mode, index, and frame time for the specified frame.
    ///
    /// - Parameter frame: Between 0 and frameCount, inclusive.
    /// - Parameter delay: Seconds between frames.
    public func setFrame(_ frame: Int32, _ time: Float, _ mode: SequenceMode, _ index: Int32, _ delay: Float) {
        spine_sequence_timeline_set_frame(_ptr.assumingMemoryBound(to: spine_sequence_timeline_wrapper.self), frame, time, spine_sequence_mode(rawValue: UInt32(mode.rawValue)), index, delay)
    }

    public func dispose() {
        spine_sequence_timeline_dispose(_ptr.assumingMemoryBound(to: spine_sequence_timeline_wrapper.self))
    }
}