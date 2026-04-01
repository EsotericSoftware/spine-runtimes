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
import 'animation.dart';
import 'bone_data.dart';
import 'constraint.dart';
import 'constraint_data.dart';
import 'from_property.dart';
import 'from_rotate.dart';
import 'from_scale_x.dart';
import 'from_scale_y.dart';
import 'from_shear_y.dart';
import 'from_x.dart';
import 'from_y.dart';
import 'ik_constraint.dart';
import 'path_constraint.dart';
import 'physics_constraint.dart';
import 'posed_data.dart';
import 'skeleton.dart';
import 'slider.dart';
import 'slider_pose.dart';
import 'transform_constraint.dart';

/// Stores the setup pose for a Slider
class SliderData extends PosedData implements ConstraintData {
  final Pointer<spine_slider_data_wrapper> _ptr;

  SliderData.fromPointer(this._ptr)
      : super.fromPointer(SpineBindings.bindings.spine_slider_data_cast_to_posed_data(_ptr));

  /// Get the native pointer for FFI calls
  @override
  Pointer get nativePtr => _ptr;

  factory SliderData(String name) {
    final ptr = SpineBindings.bindings.spine_slider_data_create(name.toNativeUtf8().cast<Char>());
    return SliderData.fromPointer(ptr);
  }

  @override
  void dispose() {
    SpineBindings.bindings.spine_slider_data_dispose(_ptr);
  }

  @override
  Rtti get rtti {
    final result = SpineBindings.bindings.spine_slider_data_get_rtti(_ptr);
    return Rtti.fromPointer(result);
  }

  /// Creates a slider instance.
  @override
  Constraint createMethod(Skeleton skeleton) {
    final result = SpineBindings.bindings.spine_slider_data_create_method(_ptr, skeleton.nativePtr.cast());
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

  /// The animation the slider will apply.
  Animation get animation {
    final result = SpineBindings.bindings.spine_slider_data_get_animation(_ptr);
    return Animation.fromPointer(result);
  }

  set animation(Animation value) {
    SpineBindings.bindings.spine_slider_data_set_animation(_ptr, value.nativePtr.cast());
  }

  /// When true, the animation is applied by adding it to the current pose
  /// rather than overwriting it.
  bool get additive {
    final result = SpineBindings.bindings.spine_slider_data_get_additive(_ptr);
    return result;
  }

  set additive(bool value) {
    SpineBindings.bindings.spine_slider_data_set_additive(_ptr, value);
  }

  /// When true, the animation repeats after its duration, otherwise the last
  /// frame is used.
  bool get loop {
    final result = SpineBindings.bindings.spine_slider_data_get_loop(_ptr);
    return result;
  }

  set loop(bool value) {
    SpineBindings.bindings.spine_slider_data_set_loop(_ptr, value);
  }

  /// When set, the bone's transform property is used to set the slider's
  /// SliderPose::getTime().
  BoneData? get bone {
    final result = SpineBindings.bindings.spine_slider_data_get_bone(_ptr);
    return result.address == 0 ? null : BoneData.fromPointer(result);
  }

  set bone(BoneData? value) {
    SpineBindings.bindings.spine_slider_data_set_bone(_ptr, value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// When a bone is set, the specified transform property is used to set the
  /// slider's SliderPose::getTime().
  FromProperty? get property {
    final result = SpineBindings.bindings.spine_slider_data_get_property(_ptr);
    if (result.address == 0) return null;
    final rtti = SpineBindings.bindings.spine_from_property_get_rtti(result);
    final className = SpineBindings.bindings.spine_rtti_get_class_name(rtti).cast<Utf8>().toDartString();
    switch (className) {
      case 'FromRotate':
        final castedPtr = SpineBindings.bindings.spine_from_property_cast_to_from_rotate(result);
        return FromRotate.fromPointer(castedPtr);
      case 'FromScaleX':
        final castedPtr = SpineBindings.bindings.spine_from_property_cast_to_from_scale_x(result);
        return FromScaleX.fromPointer(castedPtr);
      case 'FromScaleY':
        final castedPtr = SpineBindings.bindings.spine_from_property_cast_to_from_scale_y(result);
        return FromScaleY.fromPointer(castedPtr);
      case 'FromShearY':
        final castedPtr = SpineBindings.bindings.spine_from_property_cast_to_from_shear_y(result);
        return FromShearY.fromPointer(castedPtr);
      case 'FromX':
        final castedPtr = SpineBindings.bindings.spine_from_property_cast_to_from_x(result);
        return FromX.fromPointer(castedPtr);
      case 'FromY':
        final castedPtr = SpineBindings.bindings.spine_from_property_cast_to_from_y(result);
        return FromY.fromPointer(castedPtr);
      default:
        throw UnsupportedError('Unknown concrete type: $className for abstract class FromProperty');
    }
  }

  set property(FromProperty? value) {
    SpineBindings.bindings.spine_slider_data_set_property(_ptr, value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// When a bone is set, this is the scale of the property value in relation to
  /// the slider time.
  double get scale {
    final result = SpineBindings.bindings.spine_slider_data_get_scale(_ptr);
    return result;
  }

  set scale(double value) {
    SpineBindings.bindings.spine_slider_data_set_scale(_ptr, value);
  }

  /// When a bone is set, the offset is added to the property.
  double get offset {
    final result = SpineBindings.bindings.spine_slider_data_get_offset(_ptr);
    return result;
  }

  set offset(double value) {
    SpineBindings.bindings.spine_slider_data_set_offset(_ptr, value);
  }

  /// When true and a bone is set, the bone's local transform property is read
  /// instead of its world transform.
  bool get local {
    final result = SpineBindings.bindings.spine_slider_data_get_local(_ptr);
    return result;
  }

  set local(bool value) {
    SpineBindings.bindings.spine_slider_data_set_local(_ptr, value);
  }

  /// The setup pose that most animations are relative to.
  SliderPose get setupPose {
    final result = SpineBindings.bindings.spine_slider_data_get_setup_pose(_ptr);
    return SliderPose.fromPointer(result);
  }

  static Rtti rttiStatic() {
    final result = SpineBindings.bindings.spine_slider_data_rtti();
    return Rtti.fromPointer(result);
  }
}
