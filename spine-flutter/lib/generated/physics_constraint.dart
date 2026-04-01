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
import 'spine_dart_bindings_generated.dart';
import '../spine_bindings.dart';
import 'bone_pose.dart';
import 'physics_constraint_base.dart';
import 'physics_constraint_data.dart';
import 'skeleton.dart';

/// PhysicsConstraint wrapper
class PhysicsConstraint extends PhysicsConstraintBase {
  final Pointer<spine_physics_constraint_wrapper> _ptr;

  PhysicsConstraint.fromPointer(this._ptr)
      : super.fromPointer(SpineBindings.bindings.spine_physics_constraint_cast_to_physics_constraint_base(_ptr));

  /// Get the native pointer for FFI calls
  @override
  Pointer get nativePtr => _ptr;

  factory PhysicsConstraint(PhysicsConstraintData data, Skeleton skeleton) {
    final ptr =
        SpineBindings.bindings.spine_physics_constraint_create(data.nativePtr.cast(), skeleton.nativePtr.cast());
    return PhysicsConstraint.fromPointer(ptr);
  }

  @override
  void dispose() {
    SpineBindings.bindings.spine_physics_constraint_dispose(_ptr);
  }

  PhysicsConstraint copy(Skeleton skeleton) {
    final result = SpineBindings.bindings.spine_physics_constraint_copy(_ptr, skeleton.nativePtr.cast());
    return PhysicsConstraint.fromPointer(result);
  }

  /// Resets all physics state that was the result of previous movement. Use
  /// this after moving a bone to prevent physics from reacting to the movement.
  void reset(Skeleton skeleton) {
    SpineBindings.bindings.spine_physics_constraint_reset(_ptr, skeleton.nativePtr.cast());
  }

  /// Translates the physics constraint so the next update() forces are applied
  /// as if the bone moved an additional amount in world space.
  void translate(double x, double y) {
    SpineBindings.bindings.spine_physics_constraint_translate(_ptr, x, y);
  }

  /// Rotates the physics constraint so the next update() forces are applied as
  /// if the bone rotated around the specified point in world space.
  void rotate(double x, double y, double degrees) {
    SpineBindings.bindings.spine_physics_constraint_rotate(_ptr, x, y, degrees);
  }

  /// The bone constrained by this physics constraint.
  BonePose get bone {
    final result = SpineBindings.bindings.spine_physics_constraint_get_bone(_ptr);
    return BonePose.fromPointer(result);
  }

  set bone(BonePose value) {
    SpineBindings.bindings.spine_physics_constraint_set_bone(_ptr, value.nativePtr.cast());
  }
}
