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

/// Adjusts the world transform of the constrained bones to match that of the source bone.
///
/// See https://esotericsoftware.com/spine-transform-constraints Transform constraints in the Spine
/// User Guide.
@objc(SpineTransformConstraint)
@objcMembers
public class TransformConstraint: TransformConstraintBase {
    @nonobjc
    public init(fromPointer ptr: spine_transform_constraint) {
        super.init(fromPointer: UnsafeMutableRawPointer(ptr).assumingMemoryBound(to: spine_transform_constraint_base_wrapper.self))
    }

    public convenience init(_ data: TransformConstraintData, _ skeleton: Skeleton) {
        let ptr = spine_transform_constraint_create(data._ptr.assumingMemoryBound(to: spine_transform_constraint_data_wrapper.self), skeleton._ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
        self.init(fromPointer: ptr!)
    }

    /// The bones that will be modified by this transform constraint.
    public var bones: ArrayBonePose {
        let result = spine_transform_constraint_get_bones(_ptr.assumingMemoryBound(to: spine_transform_constraint_wrapper.self))
        return ArrayBonePose(fromPointer: result!)
    }

    /// The bone whose world transform will be matched by the constrained bones.
    public var source: Bone {
        get {
            let result = spine_transform_constraint_get_source(_ptr.assumingMemoryBound(to: spine_transform_constraint_wrapper.self))
        return Bone(fromPointer: result!)
        }
        set {
            spine_transform_constraint_set_source(_ptr.assumingMemoryBound(to: spine_transform_constraint_wrapper.self), newValue._ptr.assumingMemoryBound(to: spine_bone_wrapper.self))
        }
    }

    public func copyAttachment(_ skeleton: Skeleton) -> TransformConstraint {
        let result = spine_transform_constraint_copy(_ptr.assumingMemoryBound(to: spine_transform_constraint_wrapper.self), skeleton._ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
        return TransformConstraint(fromPointer: result!)
    }

    public override func dispose() {
        spine_transform_constraint_dispose(_ptr.assumingMemoryBound(to: spine_transform_constraint_wrapper.self))
    }
}