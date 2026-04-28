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

/// IkConstraint wrapper
@objc(SpineIkConstraint)
@objcMembers
public class IkConstraint: IkConstraintBase {
    @nonobjc
    public init(fromPointer ptr: spine_ik_constraint) {
        super.init(fromPointer: UnsafeMutableRawPointer(ptr).assumingMemoryBound(to: spine_ik_constraint_base_wrapper.self))
    }

    public convenience init(_ data: IkConstraintData, _ skeleton: Skeleton) {
        let ptr = spine_ik_constraint_create(
            data._ptr.assumingMemoryBound(to: spine_ik_constraint_data_wrapper.self),
            skeleton._ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
        self.init(fromPointer: ptr!)
    }

    public var bones: ArrayBonePose {
        let result = spine_ik_constraint_get_bones(_ptr.assumingMemoryBound(to: spine_ik_constraint_wrapper.self))
        return ArrayBonePose(fromPointer: result!)
    }

    public var target: Bone {
        get {
            let result = spine_ik_constraint_get_target(_ptr.assumingMemoryBound(to: spine_ik_constraint_wrapper.self))
            return Bone(fromPointer: result!)
        }
        set {
            spine_ik_constraint_set_target(
                _ptr.assumingMemoryBound(to: spine_ik_constraint_wrapper.self), newValue._ptr.assumingMemoryBound(to: spine_bone_wrapper.self))
        }
    }

    public func copyAttachment(_ skeleton: Skeleton) -> IkConstraint {
        let result = spine_ik_constraint_copy(
            _ptr.assumingMemoryBound(to: spine_ik_constraint_wrapper.self), skeleton._ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
        return IkConstraint(fromPointer: result!)
    }

    /// Adjusts the local rotation of the bone so the world position of the tip is as close to the
    /// target position as possible. The target is specified in the world coordinate system.
    public static func apply(
        _ skeleton: Skeleton, _ bone: BonePose, _ targetX: Float, _ targetY: Float, _ compress: Bool, _ stretch: Bool, _ scaleY: ScaleY, _ mix: Float
    ) {
        spine_ik_constraint_apply_1(
            skeleton._ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), bone._ptr.assumingMemoryBound(to: spine_bone_pose_wrapper.self),
            targetX, targetY, compress, stretch, spine_scale_y(rawValue: UInt32(scaleY.rawValue)), mix)
    }

    /// Adjusts the parent and child bone rotations so the tip of the child is as close to the
    /// target position as possible. The target is specified in the world coordinate system.
    ///
    /// - Parameter child: A direct descendant of the parent bone.
    public static func apply2(
        _ skeleton: Skeleton, _ parent: BonePose, _ child: BonePose, _ targetX: Float, _ targetY: Float, _ bendDirection: Int32, _ stretch: Bool,
        _ scaleY: ScaleY, _ softness: Float, _ mix: Float
    ) {
        spine_ik_constraint_apply_2(
            skeleton._ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), parent._ptr.assumingMemoryBound(to: spine_bone_pose_wrapper.self),
            child._ptr.assumingMemoryBound(to: spine_bone_pose_wrapper.self), targetX, targetY, bendDirection, stretch,
            spine_scale_y(rawValue: UInt32(scaleY.rawValue)), softness, mix)
    }

    public override func dispose() {
        spine_ik_constraint_dispose(_ptr.assumingMemoryBound(to: spine_ik_constraint_wrapper.self))
    }
}
