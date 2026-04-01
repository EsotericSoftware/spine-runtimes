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

/// Stores a pose for a physics constraint.
class PhysicsConstraintPose {
  final Pointer<spine_physics_constraint_pose_wrapper> _ptr;

  PhysicsConstraintPose.fromPointer(this._ptr);

  /// Get the native pointer for FFI calls
  Pointer get nativePtr => _ptr;

  factory PhysicsConstraintPose() {
    final ptr = SpineBindings.bindings.spine_physics_constraint_pose_create();
    return PhysicsConstraintPose.fromPointer(ptr);
  }

  void dispose() {
    SpineBindings.bindings.spine_physics_constraint_pose_dispose(_ptr);
  }

  void set(PhysicsConstraintPose pose) {
    SpineBindings.bindings.spine_physics_constraint_pose_set(_ptr, pose.nativePtr.cast());
  }

  /// Controls how much bone movement is converted into physics movement.
  double get inertia {
    final result = SpineBindings.bindings.spine_physics_constraint_pose_get_inertia(_ptr);
    return result;
  }

  set inertia(double value) {
    SpineBindings.bindings.spine_physics_constraint_pose_set_inertia(_ptr, value);
  }

  /// The amount of force used to return properties to the unconstrained value.
  double get strength {
    final result = SpineBindings.bindings.spine_physics_constraint_pose_get_strength(_ptr);
    return result;
  }

  set strength(double value) {
    SpineBindings.bindings.spine_physics_constraint_pose_set_strength(_ptr, value);
  }

  /// Reduces the speed of physics movements, with more of a reduction at higher
  /// speeds.
  double get damping {
    final result = SpineBindings.bindings.spine_physics_constraint_pose_get_damping(_ptr);
    return result;
  }

  set damping(double value) {
    SpineBindings.bindings.spine_physics_constraint_pose_set_damping(_ptr, value);
  }

  /// Determines susceptibility to acceleration.
  double get massInverse {
    final result = SpineBindings.bindings.spine_physics_constraint_pose_get_mass_inverse(_ptr);
    return result;
  }

  set massInverse(double value) {
    SpineBindings.bindings.spine_physics_constraint_pose_set_mass_inverse(_ptr, value);
  }

  /// Applies a constant force along the Skeleton::getWindX(),
  /// Skeleton::getWindY() vector.
  double get wind {
    final result = SpineBindings.bindings.spine_physics_constraint_pose_get_wind(_ptr);
    return result;
  }

  set wind(double value) {
    SpineBindings.bindings.spine_physics_constraint_pose_set_wind(_ptr, value);
  }

  /// Applies a constant force along the Skeleton::getGravityX(),
  /// Skeleton::getGravityY() vector.
  double get gravity {
    final result = SpineBindings.bindings.spine_physics_constraint_pose_get_gravity(_ptr);
    return result;
  }

  set gravity(double value) {
    SpineBindings.bindings.spine_physics_constraint_pose_set_gravity(_ptr, value);
  }

  /// A percentage (0+) that controls the mix between the constrained and
  /// unconstrained poses.
  double get mix {
    final result = SpineBindings.bindings.spine_physics_constraint_pose_get_mix(_ptr);
    return result;
  }

  set mix(double value) {
    SpineBindings.bindings.spine_physics_constraint_pose_set_mix(_ptr, value);
  }
}
