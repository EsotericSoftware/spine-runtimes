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

/// Stores the setup pose for a PathConstraint.
///
/// See https://esotericsoftware.com/spine-path-constraints Path constraints in the Spine User
/// Guide.
@objc(SpinePathConstraintData)
@objcMembers
public class PathConstraintData: PosedData, ConstraintData {
    @nonobjc
    public init(fromPointer ptr: spine_path_constraint_data) {
        super.init(fromPointer: UnsafeMutableRawPointer(ptr).assumingMemoryBound(to: spine_posed_data_wrapper.self))
    }

    public convenience init(_ name: String) {
        let ptr = spine_path_constraint_data_create(name)
        self.init(fromPointer: ptr!)
    }

    public var rtti: Rtti {
        let result = spine_path_constraint_data_get_rtti(_ptr.assumingMemoryBound(to: spine_path_constraint_data_wrapper.self))
        return Rtti(fromPointer: result!)
    }

    /// The bones that will be modified by this path constraint.
    public var bones: ArrayBoneData {
        let result = spine_path_constraint_data_get_bones(_ptr.assumingMemoryBound(to: spine_path_constraint_data_wrapper.self))
        return ArrayBoneData(fromPointer: result!)
    }

    /// The slot whose path attachment will be used to constrained the bones.
    public var slot: SlotData {
        get {
            let result = spine_path_constraint_data_get_slot(_ptr.assumingMemoryBound(to: spine_path_constraint_data_wrapper.self))
        return SlotData(fromPointer: result!)
        }
        set {
            spine_path_constraint_data_set_slot(_ptr.assumingMemoryBound(to: spine_path_constraint_data_wrapper.self), newValue._ptr.assumingMemoryBound(to: spine_slot_data_wrapper.self))
        }
    }

    /// The mode for positioning the first bone on the path.
    public var positionMode: PositionMode {
        get {
            let result = spine_path_constraint_data_get_position_mode(_ptr.assumingMemoryBound(to: spine_path_constraint_data_wrapper.self))
        return PositionMode(rawValue: Int32(result.rawValue))!
        }
        set {
            spine_path_constraint_data_set_position_mode(_ptr.assumingMemoryBound(to: spine_path_constraint_data_wrapper.self), spine_position_mode(rawValue: UInt32(newValue.rawValue)))
        }
    }

    /// The mode for positioning the bones after the first bone on the path.
    public var spacingMode: SpacingMode {
        get {
            let result = spine_path_constraint_data_get_spacing_mode(_ptr.assumingMemoryBound(to: spine_path_constraint_data_wrapper.self))
        return SpacingMode(rawValue: Int32(result.rawValue))!
        }
        set {
            spine_path_constraint_data_set_spacing_mode(_ptr.assumingMemoryBound(to: spine_path_constraint_data_wrapper.self), spine_spacing_mode(rawValue: UInt32(newValue.rawValue)))
        }
    }

    /// The mode for adjusting the rotation of the bones.
    public var rotateMode: RotateMode {
        get {
            let result = spine_path_constraint_data_get_rotate_mode(_ptr.assumingMemoryBound(to: spine_path_constraint_data_wrapper.self))
        return RotateMode(rawValue: Int32(result.rawValue))!
        }
        set {
            spine_path_constraint_data_set_rotate_mode(_ptr.assumingMemoryBound(to: spine_path_constraint_data_wrapper.self), spine_rotate_mode(rawValue: UInt32(newValue.rawValue)))
        }
    }

    /// An offset added to the constrained bone rotation.
    public var offsetRotation: Float {
        get {
            let result = spine_path_constraint_data_get_offset_rotation(_ptr.assumingMemoryBound(to: spine_path_constraint_data_wrapper.self))
        return result
        }
        set {
            spine_path_constraint_data_set_offset_rotation(_ptr.assumingMemoryBound(to: spine_path_constraint_data_wrapper.self), newValue)
        }
    }

    /// The setup pose that most animations are relative to.
    public var setupPose: PathConstraintPose {
        let result = spine_path_constraint_data_get_setup_pose(_ptr.assumingMemoryBound(to: spine_path_constraint_data_wrapper.self))
        return PathConstraintPose(fromPointer: result!)
    }

    public func createMethod(_ skeleton: Skeleton) -> Constraint {
        let result = spine_path_constraint_data_create_method(_ptr.assumingMemoryBound(to: spine_path_constraint_data_wrapper.self), skeleton._ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
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
        let result = spine_path_constraint_data_rtti()
        return Rtti(fromPointer: result!)
    }

    public override func dispose() {
        spine_path_constraint_data_dispose(_ptr.assumingMemoryBound(to: spine_path_constraint_data_wrapper.self))
    }
}