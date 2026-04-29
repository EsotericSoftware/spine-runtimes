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

/// A node in a skeleton's hierarchy with a transform that affects its children and their
/// attachments. A bone has a number of poses: - getData(): The setup pose data. - getPose(): The
/// unconstrained local pose. Set by animations and application code. - getAppliedPose(): The local
/// pose to use for rendering. Possibly modified by constraints. - World transform: the local pose
/// combined with the parent world transform. Computed on a pose by
/// BonePose::updateWorldTransform(Skeleton) and Skeleton::updateWorldTransform(Physics).
@objc(SpineBone)
@objcMembers
public class Bone: PosedActive, Posed, Update {
    @nonobjc
    public init(fromPointer ptr: spine_bone) {
        super.init(fromPointer: UnsafeMutableRawPointer(ptr).assumingMemoryBound(to: spine_posed_active_wrapper.self))
    }

    /// - Parameter parent: May be NULL.
    public convenience init(_ data: BoneData, _ parent: Bone?) {
        let ptr = spine_bone_create(data._ptr.assumingMemoryBound(to: spine_bone_data_wrapper.self), parent?._ptr.assumingMemoryBound(to: spine_bone_wrapper.self))
        self.init(fromPointer: ptr!)
    }

    /// Copy constructor. Does not copy the child bones.
    public static func from(_ bone: Bone, _ parent: Bone?) -> Bone {
        let ptr = spine_bone_create2(bone._ptr.assumingMemoryBound(to: spine_bone_wrapper.self), parent?._ptr.assumingMemoryBound(to: spine_bone_wrapper.self))
        return Bone(fromPointer: ptr!)
    }

    public var rtti: Rtti {
        let result = spine_bone_get_rtti(_ptr.assumingMemoryBound(to: spine_bone_wrapper.self))
        return Rtti(fromPointer: result!)
    }

    /// The parent bone, or null if this is the root bone.
    public var parent: Bone? {
        let result = spine_bone_get_parent(_ptr.assumingMemoryBound(to: spine_bone_wrapper.self))
        return result.map { Bone(fromPointer: $0) }
    }

    /// The immediate children of this bone.
    public var children: ArrayBone {
        let result = spine_bone_get_children(_ptr.assumingMemoryBound(to: spine_bone_wrapper.self))
        return ArrayBone(fromPointer: result!)
    }

    /// The setup pose data. May be shared with multiple instances.
    public var data: BoneData {
        let result = spine_bone_get_data(_ptr.assumingMemoryBound(to: spine_bone_wrapper.self))
        return BoneData(fromPointer: result!)
    }

    /// The unconstrained pose for this object, set by animations and application code.
    public var pose: BonePose {
        let result = spine_bone_get_pose(_ptr.assumingMemoryBound(to: spine_bone_wrapper.self))
        return BonePose(fromPointer: result!)
    }

    /// The pose to use for rendering. If no constraints modify this pose, this is the same as
    /// getPose(). Otherwise it is a copy of getPose() modified by constraints.
    public var appliedPose: BonePose {
        let result = spine_bone_get_applied_pose(_ptr.assumingMemoryBound(to: spine_bone_wrapper.self))
        return BonePose(fromPointer: result!)
    }

    public var isPoseEqualToApplied: Bool {
        let result = spine_bone_is_pose_equal_to_applied(_ptr.assumingMemoryBound(to: spine_bone_wrapper.self))
        return result
    }

    public static func isYDown() -> Bool {
        let result = spine_bone_is_y_down()
        return result
    }

    public static func setYDown(_ value: Bool) {
        spine_bone_set_y_down(value)
    }

    public func update(_ skeleton: Skeleton, _ physics: Physics) {
        spine_bone_update(_ptr.assumingMemoryBound(to: spine_bone_wrapper.self), skeleton._ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), spine_physics(rawValue: UInt32(physics.rawValue)))
    }

    /// Sets the constrained pose to the unconstrained pose, as a starting point for constraints to
    /// be applied.
    public func resetConstrained() {
        spine_bone_reset_constrained(_ptr.assumingMemoryBound(to: spine_bone_wrapper.self))
    }

    /// Sets the applied pose to the constrained pose, in anticipation of the applied pose being
    /// modified by constraints.
    public func constrained() {
        spine_bone_constrained(_ptr.assumingMemoryBound(to: spine_bone_wrapper.self))
    }

    public static func rttiStatic() -> Rtti {
        let result = spine_bone_rtti()
        return Rtti(fromPointer: result!)
    }

    public override func dispose() {
        spine_bone_dispose(_ptr.assumingMemoryBound(to: spine_bone_wrapper.self))
    }
}