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

/// Stores the setup pose for a PhysicsConstraint.
///
/// See https://esotericsoftware.com/spine-physics-constraints Physics constraints in the Spine User
/// Guide.
@objc(SpinePhysicsConstraintData)
@objcMembers
public class PhysicsConstraintData: PosedData, ConstraintData {
    @nonobjc
    public init(fromPointer ptr: spine_physics_constraint_data) {
        super.init(fromPointer: UnsafeMutableRawPointer(ptr).assumingMemoryBound(to: spine_posed_data_wrapper.self))
    }

    public convenience init(_ name: String) {
        let ptr = spine_physics_constraint_data_create(name)
        self.init(fromPointer: ptr!)
    }

    public var rtti: Rtti {
        let result = spine_physics_constraint_data_get_rtti(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self))
        return Rtti(fromPointer: result!)
    }

    /// The bone constrained by this physics constraint.
    public var bone: BoneData {
        get {
            let result = spine_physics_constraint_data_get_bone(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self))
        return BoneData(fromPointer: result!)
        }
        set {
            spine_physics_constraint_data_set_bone(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self), newValue._ptr.assumingMemoryBound(to: spine_bone_data_wrapper.self))
        }
    }

    /// The time in milliseconds required to advanced the physics simulation one step.
    public var step: Float {
        get {
            let result = spine_physics_constraint_data_get_step(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self))
        return result
        }
        set {
            spine_physics_constraint_data_set_step(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self), newValue)
        }
    }

    /// Physics influence on x translation, 0-1.
    public var x: Float {
        get {
            let result = spine_physics_constraint_data_get_x(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self))
        return result
        }
        set {
            spine_physics_constraint_data_set_x(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self), newValue)
        }
    }

    /// Physics influence on y translation, 0-1.
    public var y: Float {
        get {
            let result = spine_physics_constraint_data_get_y(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self))
        return result
        }
        set {
            spine_physics_constraint_data_set_y(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self), newValue)
        }
    }

    /// Physics influence on rotation, 0-1.
    public var rotate: Float {
        get {
            let result = spine_physics_constraint_data_get_rotate(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self))
        return result
        }
        set {
            spine_physics_constraint_data_set_rotate(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self), newValue)
        }
    }

    /// Physics influence on scaleX, 0-1.
    public var scaleX: Float {
        get {
            let result = spine_physics_constraint_data_get_scale_x(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self))
        return result
        }
        set {
            spine_physics_constraint_data_set_scale_x(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self), newValue)
        }
    }

    /// Physics influence on shearX, 0-1.
    public var shearX: Float {
        get {
            let result = spine_physics_constraint_data_get_shear_x(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self))
        return result
        }
        set {
            spine_physics_constraint_data_set_shear_x(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self), newValue)
        }
    }

    /// Movement greater than the limit will not have a greater affect on physics.
    public var limit: Float {
        get {
            let result = spine_physics_constraint_data_get_limit(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self))
        return result
        }
        set {
            spine_physics_constraint_data_set_limit(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self), newValue)
        }
    }

    /// Determines how BonePose::getScaleY() changes when getScaleX() sets BonePose::getScaleX().
    public var scaleYMode: ScaleYMode {
        get {
            let result = spine_physics_constraint_data_get_scale_y_mode(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self))
        return ScaleYMode(rawValue: Int32(result.rawValue))!
        }
        set {
            spine_physics_constraint_data_set_scale_y_mode(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self), spine_scale_y_mode(rawValue: UInt32(newValue.rawValue)))
        }
    }

    /// True when this constraint's inertia is controlled by global slider timelines.
    public var inertiaGlobal: Bool {
        get {
            let result = spine_physics_constraint_data_get_inertia_global(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self))
        return result
        }
        set {
            spine_physics_constraint_data_set_inertia_global(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self), newValue)
        }
    }

    /// True when this constraint's strength is controlled by global slider timelines.
    public var strengthGlobal: Bool {
        get {
            let result = spine_physics_constraint_data_get_strength_global(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self))
        return result
        }
        set {
            spine_physics_constraint_data_set_strength_global(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self), newValue)
        }
    }

    /// True when this constraint's damping is controlled by global slider timelines.
    public var dampingGlobal: Bool {
        get {
            let result = spine_physics_constraint_data_get_damping_global(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self))
        return result
        }
        set {
            spine_physics_constraint_data_set_damping_global(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self), newValue)
        }
    }

    /// True when this constraint's mass is controlled by global slider timelines.
    public var massGlobal: Bool {
        get {
            let result = spine_physics_constraint_data_get_mass_global(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self))
        return result
        }
        set {
            spine_physics_constraint_data_set_mass_global(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self), newValue)
        }
    }

    /// True when this constraint's wind is controlled by global slider timelines.
    public var windGlobal: Bool {
        get {
            let result = spine_physics_constraint_data_get_wind_global(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self))
        return result
        }
        set {
            spine_physics_constraint_data_set_wind_global(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self), newValue)
        }
    }

    /// True when this constraint's gravity is controlled by global slider timelines.
    public var gravityGlobal: Bool {
        get {
            let result = spine_physics_constraint_data_get_gravity_global(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self))
        return result
        }
        set {
            spine_physics_constraint_data_set_gravity_global(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self), newValue)
        }
    }

    /// True when this constraint's mix is controlled by global slider timelines.
    public var mixGlobal: Bool {
        get {
            let result = spine_physics_constraint_data_get_mix_global(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self))
        return result
        }
        set {
            spine_physics_constraint_data_set_mix_global(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self), newValue)
        }
    }

    /// The setup pose that most animations are relative to.
    public var setupPose: PhysicsConstraintPose {
        let result = spine_physics_constraint_data_get_setup_pose(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self))
        return PhysicsConstraintPose(fromPointer: result!)
    }

    public func createMethod(_ skeleton: Skeleton) -> Constraint {
        let result = spine_physics_constraint_data_create_method(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self), skeleton._ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self))
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
        let result = spine_physics_constraint_data_rtti()
        return Rtti(fromPointer: result!)
    }

    public override func dispose() {
        spine_physics_constraint_data_dispose(_ptr.assumingMemoryBound(to: spine_physics_constraint_data_wrapper.self))
    }
}