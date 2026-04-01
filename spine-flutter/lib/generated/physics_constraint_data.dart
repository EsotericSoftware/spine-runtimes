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

import 'package:universal_ffi/ffi.dart';
import 'package:universal_ffi/ffi_utils.dart';
import 'spine_dart_bindings_generated.dart';
import '../spine_bindings.dart';
import 'rtti.dart';
import 'bone_data.dart';
import 'constraint.dart';
import 'constraint_data.dart';
import 'ik_constraint.dart';
import 'path_constraint.dart';
import 'physics_constraint.dart';
import 'physics_constraint_pose.dart';
import 'posed_data.dart';
import 'skeleton.dart';
import 'slider.dart';
import 'transform_constraint.dart';

/// Stores the setup pose for a PhysicsConstraint.
///
/// See https://esotericsoftware.com/spine-physics-constraints Physics
/// constraints in the Spine User Guide.
class PhysicsConstraintData extends PosedData implements ConstraintData {
  final Pointer<spine_physics_constraint_data_wrapper> _ptr;

  PhysicsConstraintData.fromPointer(this._ptr)
      : super.fromPointer(SpineBindings.bindings.spine_physics_constraint_data_cast_to_posed_data(_ptr));

  /// Get the native pointer for FFI calls
  @override
  Pointer get nativePtr => _ptr;

  factory PhysicsConstraintData(String name) {
    final ptr = SpineBindings.bindings.spine_physics_constraint_data_create(name.toNativeUtf8().cast<Char>());
    return PhysicsConstraintData.fromPointer(ptr);
  }

  @override
  void dispose() {
    SpineBindings.bindings.spine_physics_constraint_data_dispose(_ptr);
  }

  @override
  Rtti get rtti {
    final result = SpineBindings.bindings.spine_physics_constraint_data_get_rtti(_ptr);
    return Rtti.fromPointer(result);
  }

  @override
  Constraint createMethod(Skeleton skeleton) {
    final result = SpineBindings.bindings.spine_physics_constraint_data_create_method(_ptr, skeleton.nativePtr.cast());
    final rtti = SpineBindings.bindings.spine_constraint_get_rtti(result);
    final className = SpineBindings.bindings.spine_rtti_get_class_name(rtti).cast<Utf8>().toDartString();
    switch (className) {
      case 'IkConstraint':
        final castedPtr = SpineBindings.bindings.spine_constraint_cast_to_ik_constraint(result);
        return IkConstraint.fromPointer(castedPtr);
      case 'PathConstraint':
        final castedPtr = SpineBindings.bindings.spine_constraint_cast_to_path_constraint(result);
        return PathConstraint.fromPointer(castedPtr);
      case 'PhysicsConstraint':
        final castedPtr = SpineBindings.bindings.spine_constraint_cast_to_physics_constraint(result);
        return PhysicsConstraint.fromPointer(castedPtr);
      case 'Slider':
        final castedPtr = SpineBindings.bindings.spine_constraint_cast_to_slider(result);
        return Slider.fromPointer(castedPtr);
      case 'TransformConstraint':
        final castedPtr = SpineBindings.bindings.spine_constraint_cast_to_transform_constraint(result);
        return TransformConstraint.fromPointer(castedPtr);
      default:
        throw UnsupportedError('Unknown concrete type: $className for abstract class Constraint');
    }
  }

  /// The bone constrained by this physics constraint.
  BoneData get bone {
    final result = SpineBindings.bindings.spine_physics_constraint_data_get_bone(_ptr);
    return BoneData.fromPointer(result);
  }

  set bone(BoneData value) {
    SpineBindings.bindings.spine_physics_constraint_data_set_bone(_ptr, value.nativePtr.cast());
  }

  /// The time in milliseconds required to advanced the physics simulation one
  /// step.
  double get step {
    final result = SpineBindings.bindings.spine_physics_constraint_data_get_step(_ptr);
    return result;
  }

  set step(double value) {
    SpineBindings.bindings.spine_physics_constraint_data_set_step(_ptr, value);
  }

  /// Physics influence on x translation, 0-1.
  double get x {
    final result = SpineBindings.bindings.spine_physics_constraint_data_get_x(_ptr);
    return result;
  }

  set x(double value) {
    SpineBindings.bindings.spine_physics_constraint_data_set_x(_ptr, value);
  }

  /// Physics influence on y translation, 0-1.
  double get y {
    final result = SpineBindings.bindings.spine_physics_constraint_data_get_y(_ptr);
    return result;
  }

  set y(double value) {
    SpineBindings.bindings.spine_physics_constraint_data_set_y(_ptr, value);
  }

  /// Physics influence on rotation, 0-1.
  double get rotate {
    final result = SpineBindings.bindings.spine_physics_constraint_data_get_rotate(_ptr);
    return result;
  }

  set rotate(double value) {
    SpineBindings.bindings.spine_physics_constraint_data_set_rotate(_ptr, value);
  }

  /// Physics influence on scaleX, 0-1.
  double get scaleX {
    final result = SpineBindings.bindings.spine_physics_constraint_data_get_scale_x(_ptr);
    return result;
  }

  set scaleX(double value) {
    SpineBindings.bindings.spine_physics_constraint_data_set_scale_x(_ptr, value);
  }

  /// Physics influence on shearX, 0-1.
  double get shearX {
    final result = SpineBindings.bindings.spine_physics_constraint_data_get_shear_x(_ptr);
    return result;
  }

  set shearX(double value) {
    SpineBindings.bindings.spine_physics_constraint_data_set_shear_x(_ptr, value);
  }

  /// Movement greater than the limit will not have a greater affect on physics.
  double get limit {
    final result = SpineBindings.bindings.spine_physics_constraint_data_get_limit(_ptr);
    return result;
  }

  set limit(double value) {
    SpineBindings.bindings.spine_physics_constraint_data_set_limit(_ptr, value);
  }

  /// True when this constraint's inertia is controlled by global slider
  /// timelines.
  bool get inertiaGlobal {
    final result = SpineBindings.bindings.spine_physics_constraint_data_get_inertia_global(_ptr);
    return result;
  }

  set inertiaGlobal(bool value) {
    SpineBindings.bindings.spine_physics_constraint_data_set_inertia_global(_ptr, value);
  }

  /// True when this constraint's strength is controlled by global slider
  /// timelines.
  bool get strengthGlobal {
    final result = SpineBindings.bindings.spine_physics_constraint_data_get_strength_global(_ptr);
    return result;
  }

  set strengthGlobal(bool value) {
    SpineBindings.bindings.spine_physics_constraint_data_set_strength_global(_ptr, value);
  }

  /// True when this constraint's damping is controlled by global slider
  /// timelines.
  bool get dampingGlobal {
    final result = SpineBindings.bindings.spine_physics_constraint_data_get_damping_global(_ptr);
    return result;
  }

  set dampingGlobal(bool value) {
    SpineBindings.bindings.spine_physics_constraint_data_set_damping_global(_ptr, value);
  }

  /// True when this constraint's mass is controlled by global slider timelines.
  bool get massGlobal {
    final result = SpineBindings.bindings.spine_physics_constraint_data_get_mass_global(_ptr);
    return result;
  }

  set massGlobal(bool value) {
    SpineBindings.bindings.spine_physics_constraint_data_set_mass_global(_ptr, value);
  }

  /// True when this constraint's wind is controlled by global slider timelines.
  bool get windGlobal {
    final result = SpineBindings.bindings.spine_physics_constraint_data_get_wind_global(_ptr);
    return result;
  }

  set windGlobal(bool value) {
    SpineBindings.bindings.spine_physics_constraint_data_set_wind_global(_ptr, value);
  }

  /// True when this constraint's gravity is controlled by global slider
  /// timelines.
  bool get gravityGlobal {
    final result = SpineBindings.bindings.spine_physics_constraint_data_get_gravity_global(_ptr);
    return result;
  }

  set gravityGlobal(bool value) {
    SpineBindings.bindings.spine_physics_constraint_data_set_gravity_global(_ptr, value);
  }

  /// True when this constraint's mix is controlled by global slider timelines.
  bool get mixGlobal {
    final result = SpineBindings.bindings.spine_physics_constraint_data_get_mix_global(_ptr);
    return result;
  }

  set mixGlobal(bool value) {
    SpineBindings.bindings.spine_physics_constraint_data_set_mix_global(_ptr, value);
  }

  /// The setup pose that most animations are relative to.
  PhysicsConstraintPose get setupPose {
    final result = SpineBindings.bindings.spine_physics_constraint_data_get_setup_pose(_ptr);
    return PhysicsConstraintPose.fromPointer(result);
  }

  static Rtti rttiStatic() {
    final result = SpineBindings.bindings.spine_physics_constraint_data_rtti();
    return Rtti.fromPointer(result);
  }
}
