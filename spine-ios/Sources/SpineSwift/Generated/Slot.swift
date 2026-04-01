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

/// Organizes attachments for Skeleton drawOrder purposes and provide a place to store state for an
/// attachment.
///
/// State cannot be stored in an attachment itself because attachments are stateless and may be
/// shared across multiple skeletons.
@objc(SpineSlot)
@objcMembers
public class Slot: NSObject, Posed {
    public let _ptr: UnsafeMutableRawPointer

    public init(fromPointer ptr: spine_slot) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        super.init()
    }

    public convenience init(_ data: SlotData, _ skeleton: Skeleton) {
        let ptr = spine_slot_create(
            data._ptr.assumingMemoryBound(to: spine_slot_data_wrapper.self), skeleton._ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
        self.init(fromPointer: ptr!)
    }

    /// The bone this slot belongs to.
    public var bone: Bone {
        let result = spine_slot_get_bone(_ptr.assumingMemoryBound(to: spine_slot_wrapper.self))
        return Bone(fromPointer: result!)
    }

    /// The setup pose data. May be shared with multiple instances.
    public var data: SlotData {
        let result = spine_slot_get_data(_ptr.assumingMemoryBound(to: spine_slot_wrapper.self))
        return SlotData(fromPointer: result!)
    }

    /// The unconstrained pose for this object, set by animations and application code.
    public var pose: SlotPose {
        let result = spine_slot_get_pose(_ptr.assumingMemoryBound(to: spine_slot_wrapper.self))
        return SlotPose(fromPointer: result!)
    }

    /// The pose to use for rendering. If no constraints modify this pose, this is the same as
    /// getPose(). Otherwise it is a copy of getPose() modified by constraints.
    public var appliedPose: SlotPose {
        let result = spine_slot_get_applied_pose(_ptr.assumingMemoryBound(to: spine_slot_wrapper.self))
        return SlotPose(fromPointer: result!)
    }

    public var isPoseEqualToApplied: Bool {
        let result = spine_slot_is_pose_equal_to_applied(_ptr.assumingMemoryBound(to: spine_slot_wrapper.self))
        return result
    }

    public func setupPose() {
        spine_slot_setup_pose(_ptr.assumingMemoryBound(to: spine_slot_wrapper.self))
    }

    /// Sets the constrained pose to the unconstrained pose, as a starting point for constraints to
    /// be applied.
    public func resetConstrained() {
        spine_slot_reset_constrained(_ptr.assumingMemoryBound(to: spine_slot_wrapper.self))
    }

    /// Sets the applied pose to the constrained pose, in anticipation of the applied pose being
    /// modified by constraints.
    public func constrained() {
        spine_slot_constrained(_ptr.assumingMemoryBound(to: spine_slot_wrapper.self))
    }

    public func dispose() {
        spine_slot_dispose(_ptr.assumingMemoryBound(to: spine_slot_wrapper.self))
    }
}
