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

/// Stores the setup pose for a TransformConstraint.
///
/// See https://esotericsoftware.com/spine-transform-constraints Transform constraints in the Spine
/// User Guide.
@objc(SpineTransformConstraintData)
@objcMembers
public class TransformConstraintData: PosedData, ConstraintData {
    @nonobjc
    public init(fromPointer ptr: spine_transform_constraint_data) {
        super.init(fromPointer: UnsafeMutableRawPointer(ptr).assumingMemoryBound(to: spine_posed_data_wrapper.self))
    }

    public convenience init(_ name: String) {
        let ptr = spine_transform_constraint_data_create(name)
        self.init(fromPointer: ptr!)
    }

    public var rtti: Rtti {
        let result = spine_transform_constraint_data_get_rtti(_ptr.assumingMemoryBound(to: spine_transform_constraint_data_wrapper.self))
        return Rtti(fromPointer: result!)
    }

    /// The bones that will be modified by this transform constraint.
    public var bones: ArrayBoneData {
        let result = spine_transform_constraint_data_get_bones(_ptr.assumingMemoryBound(to: spine_transform_constraint_data_wrapper.self))
        return ArrayBoneData(fromPointer: result!)
    }

    /// The bone whose world transform will be copied to the constrained bones.
    public var source: BoneData {
        get {
            let result = spine_transform_constraint_data_get_source(_ptr.assumingMemoryBound(to: spine_transform_constraint_data_wrapper.self))
        return BoneData(fromPointer: result!)
        }
        set {
            spine_transform_constraint_data_set_source(_ptr.assumingMemoryBound(to: spine_transform_constraint_data_wrapper.self), newValue._ptr.assumingMemoryBound(to: spine_bone_data_wrapper.self))
        }
    }

    /// An offset added to the constrained bone rotation.
    public var offsetRotation: Float {
        get {
            let result = spine_transform_constraint_data_get_offset_rotation(_ptr.assumingMemoryBound(to: spine_transform_constraint_data_wrapper.self))
        return result
        }
        set {
            spine_transform_constraint_data_set_offset_rotation(_ptr.assumingMemoryBound(to: spine_transform_constraint_data_wrapper.self), newValue)
        }
    }

    /// An offset added to the constrained bone X translation.
    public var offsetX: Float {
        get {
            let result = spine_transform_constraint_data_get_offset_x(_ptr.assumingMemoryBound(to: spine_transform_constraint_data_wrapper.self))
        return result
        }
        set {
            spine_transform_constraint_data_set_offset_x(_ptr.assumingMemoryBound(to: spine_transform_constraint_data_wrapper.self), newValue)
        }
    }

    /// An offset added to the constrained bone Y translation.
    public var offsetY: Float {
        get {
            let result = spine_transform_constraint_data_get_offset_y(_ptr.assumingMemoryBound(to: spine_transform_constraint_data_wrapper.self))
        return result
        }
        set {
            spine_transform_constraint_data_set_offset_y(_ptr.assumingMemoryBound(to: spine_transform_constraint_data_wrapper.self), newValue)
        }
    }

    /// An offset added to the constrained bone scaleX.
    public var offsetScaleX: Float {
        get {
            let result = spine_transform_constraint_data_get_offset_scale_x(_ptr.assumingMemoryBound(to: spine_transform_constraint_data_wrapper.self))
        return result
        }
        set {
            spine_transform_constraint_data_set_offset_scale_x(_ptr.assumingMemoryBound(to: spine_transform_constraint_data_wrapper.self), newValue)
        }
    }

    /// An offset added to the constrained bone scaleY.
    public var offsetScaleY: Float {
        get {
            let result = spine_transform_constraint_data_get_offset_scale_y(_ptr.assumingMemoryBound(to: spine_transform_constraint_data_wrapper.self))
        return result
        }
        set {
            spine_transform_constraint_data_set_offset_scale_y(_ptr.assumingMemoryBound(to: spine_transform_constraint_data_wrapper.self), newValue)
        }
    }

    /// An offset added to the constrained bone shearY.
    public var offsetShearY: Float {
        get {
            let result = spine_transform_constraint_data_get_offset_shear_y(_ptr.assumingMemoryBound(to: spine_transform_constraint_data_wrapper.self))
        return result
        }
        set {
            spine_transform_constraint_data_set_offset_shear_y(_ptr.assumingMemoryBound(to: spine_transform_constraint_data_wrapper.self), newValue)
        }
    }

    /// Reads the source bone's local transform instead of its world transform.
    public var localSource: Bool {
        get {
            let result = spine_transform_constraint_data_get_local_source(_ptr.assumingMemoryBound(to: spine_transform_constraint_data_wrapper.self))
        return result
        }
        set {
            spine_transform_constraint_data_set_local_source(_ptr.assumingMemoryBound(to: spine_transform_constraint_data_wrapper.self), newValue)
        }
    }

    /// Sets the constrained bones' local transforms instead of their world transforms.
    public var localTarget: Bool {
        get {
            let result = spine_transform_constraint_data_get_local_target(_ptr.assumingMemoryBound(to: spine_transform_constraint_data_wrapper.self))
        return result
        }
        set {
            spine_transform_constraint_data_set_local_target(_ptr.assumingMemoryBound(to: spine_transform_constraint_data_wrapper.self), newValue)
        }
    }

    /// Adds the source bone transform to the constrained bones instead of setting it absolutely.
    public var additive: Bool {
        get {
            let result = spine_transform_constraint_data_get_additive(_ptr.assumingMemoryBound(to: spine_transform_constraint_data_wrapper.self))
        return result
        }
        set {
            spine_transform_constraint_data_set_additive(_ptr.assumingMemoryBound(to: spine_transform_constraint_data_wrapper.self), newValue)
        }
    }

    /// Prevents constrained bones from exceeding the ranged defined by offset and max.
    public var clamp: Bool {
        get {
            let result = spine_transform_constraint_data_get_clamp(_ptr.assumingMemoryBound(to: spine_transform_constraint_data_wrapper.self))
        return result
        }
        set {
            spine_transform_constraint_data_set_clamp(_ptr.assumingMemoryBound(to: spine_transform_constraint_data_wrapper.self), newValue)
        }
    }

    /// The mapping of transform properties to other transform properties.
    public var properties: ArrayFromProperty {
        let result = spine_transform_constraint_data_get_properties(_ptr.assumingMemoryBound(to: spine_transform_constraint_data_wrapper.self))
        return ArrayFromProperty(fromPointer: result!)
    }

    /// The setup pose that most animations are relative to.
    public var setupPose: TransformConstraintPose {
        let result = spine_transform_constraint_data_get_setup_pose(_ptr.assumingMemoryBound(to: spine_transform_constraint_data_wrapper.self))
        return TransformConstraintPose(fromPointer: result!)
    }

    public func createMethod(_ skeleton: Skeleton) -> Constraint {
        let result = spine_transform_constraint_data_create_method(_ptr.assumingMemoryBound(to: spine_transform_constraint_data_wrapper.self), skeleton._ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
        let rtti = spine_constraint_get_rtti(result!)
        let rttiClassName = String(cString: spine_rtti_get_class_name(rtti)!)
        switch rttiClassName {
        case "IkConstraint":
            let castedPtr = spine_constraint_cast_to_ik_constraint(result!)
            return IkConstraint(fromPointer: castedPtr!)
        case "PathConstraint":
            let castedPtr = spine_constraint_cast_to_path_constraint(result!)
            return PathConstraint(fromPointer: castedPtr!)
        case "PhysicsConstraint":
            let castedPtr = spine_constraint_cast_to_physics_constraint(result!)
            return PhysicsConstraint(fromPointer: castedPtr!)
        case "Slider":
            let castedPtr = spine_constraint_cast_to_slider(result!)
            return Slider(fromPointer: castedPtr!)
        case "TransformConstraint":
            let castedPtr = spine_constraint_cast_to_transform_constraint(result!)
            return TransformConstraint(fromPointer: castedPtr!)
        default:
            fatalError("Unknown concrete type: \(rttiClassName) for abstract class Constraint")
        }
    }

    public static func rttiStatic() -> Rtti {
        let result = spine_transform_constraint_data_rtti()
        return Rtti(fromPointer: result!)
    }

    public override func dispose() {
        spine_transform_constraint_data_dispose(_ptr.assumingMemoryBound(to: spine_transform_constraint_data_wrapper.self))
    }
}