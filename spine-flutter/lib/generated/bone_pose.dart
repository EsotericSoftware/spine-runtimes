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
import 'rtti.dart';
import 'bone_local.dart';
import 'physics.dart';
import 'skeleton.dart';
import 'update.dart';

/// The applied local pose and world transform for a bone. This is the bone's
/// unconstrained pose with constraints applied and the world transform computed
/// by Skeleton::updateWorldTransform(Physics) and
/// updateWorldTransform(Skeleton).
///
/// If the world transform is changed, call updateLocalTransform(Skeleton)
/// before using the local transform. The local transform may be needed by other
/// code (eg to apply another constraint).
///
/// After changing the world transform, call updateWorldTransform(Skeleton) on
/// every descendant bone. It may be more convenient to modify the local
/// transform instead, then call Skeleton::updateWorldTransform(Physics) to
/// update the world transforms for all bones and apply constraints.
class BonePose extends BoneLocal implements Update {
  final Pointer<spine_bone_pose_wrapper> _ptr;

  BonePose.fromPointer(this._ptr) : super.fromPointer(SpineBindings.bindings.spine_bone_pose_cast_to_bone_local(_ptr));

  /// Get the native pointer for FFI calls
  @override
  Pointer get nativePtr => _ptr;

  factory BonePose() {
    final ptr = SpineBindings.bindings.spine_bone_pose_create();
    return BonePose.fromPointer(ptr);
  }

  @override
  void dispose() {
    SpineBindings.bindings.spine_bone_pose_dispose(_ptr);
  }

  @override
  Rtti get rtti {
    final result = SpineBindings.bindings.spine_bone_pose_get_rtti(_ptr);
    return Rtti.fromPointer(result);
  }

  /// Called by Skeleton::updateCache() to compute the world transform, if
  /// needed.
  @override
  void update(Skeleton skeleton, Physics physics) {
    SpineBindings.bindings.spine_bone_pose_update(_ptr, skeleton.nativePtr.cast(), physics.value);
  }

  /// Computes the world transform using the parent bone's world transform and
  /// this applied local pose. Child bones are not updated.
  ///
  /// See World transforms in the Spine Runtimes Guide.
  void updateWorldTransform(Skeleton skeleton) {
    SpineBindings.bindings.spine_bone_pose_update_world_transform(_ptr, skeleton.nativePtr.cast());
  }

  /// Computes the local transform values from the world transform.
  ///
  /// Some information is ambiguous in the world transform, such as -1,-1 scale
  /// versus 180 rotation. The local transform after calling this method is
  /// equivalent to the local transform used to compute the world transform, but
  /// may not be identical.
  void updateLocalTransform(Skeleton skeleton) {
    SpineBindings.bindings.spine_bone_pose_update_local_transform(_ptr, skeleton.nativePtr.cast());
  }

  /// If the world transform has been modified by constraints and the local
  /// transform no longer matches, updateLocalTransform() is called. Call this
  /// after Skeleton::updateWorldTransform(Physics) before using the applied
  /// local transform.
  void validateLocalTransform(Skeleton skeleton) {
    SpineBindings.bindings.spine_bone_pose_validate_local_transform(_ptr, skeleton.nativePtr.cast());
  }

  void modifyLocal(Skeleton skeleton) {
    SpineBindings.bindings.spine_bone_pose_modify_local(_ptr, skeleton.nativePtr.cast());
  }

  void modifyWorld(int update) {
    SpineBindings.bindings.spine_bone_pose_modify_world(_ptr, update);
  }

  void resetWorld(int update) {
    SpineBindings.bindings.spine_bone_pose_reset_world(_ptr, update);
  }

  /// The world transform [a b][c d] x-axis x component.
  double get a {
    final result = SpineBindings.bindings.spine_bone_pose_get_a(_ptr);
    return result;
  }

  set a(double value) {
    SpineBindings.bindings.spine_bone_pose_set_a(_ptr, value);
  }

  /// The world transform [a b][c d] y-axis x component.
  double get b {
    final result = SpineBindings.bindings.spine_bone_pose_get_b(_ptr);
    return result;
  }

  set b(double value) {
    SpineBindings.bindings.spine_bone_pose_set_b(_ptr, value);
  }

  /// The world transform [a b][c d] x-axis y component.
  double get c {
    final result = SpineBindings.bindings.spine_bone_pose_get_c(_ptr);
    return result;
  }

  set c(double value) {
    SpineBindings.bindings.spine_bone_pose_set_c(_ptr, value);
  }

  /// The world transform [a b][c d] y-axis y component.
  double get d {
    final result = SpineBindings.bindings.spine_bone_pose_get_d(_ptr);
    return result;
  }

  set d(double value) {
    SpineBindings.bindings.spine_bone_pose_set_d(_ptr, value);
  }

  /// The world X position.
  double get worldX {
    final result = SpineBindings.bindings.spine_bone_pose_get_world_x(_ptr);
    return result;
  }

  set worldX(double value) {
    SpineBindings.bindings.spine_bone_pose_set_world_x(_ptr, value);
  }

  /// The world Y position.
  double get worldY {
    final result = SpineBindings.bindings.spine_bone_pose_get_world_y(_ptr);
    return result;
  }

  set worldY(double value) {
    SpineBindings.bindings.spine_bone_pose_set_world_y(_ptr, value);
  }

  /// The world rotation for the X axis, calculated using a and c. This is the
  /// direction the bone is pointing.
  double get worldRotationX {
    final result = SpineBindings.bindings.spine_bone_pose_get_world_rotation_x(_ptr);
    return result;
  }

  /// The world rotation for the Y axis, calculated using b and d.
  double get worldRotationY {
    final result = SpineBindings.bindings.spine_bone_pose_get_world_rotation_y(_ptr);
    return result;
  }

  /// The magnitude (always positive) of the world scale X, calculated using a
  /// and c.
  double get worldScaleX {
    final result = SpineBindings.bindings.spine_bone_pose_get_world_scale_x(_ptr);
    return result;
  }

  /// The magnitude (always positive) of the world scale Y, calculated using b
  /// and d.
  double get worldScaleY {
    final result = SpineBindings.bindings.spine_bone_pose_get_world_scale_y(_ptr);
    return result;
  }

  /// Transforms a world rotation to a local rotation.
  double worldToLocalRotation(double worldRotation) {
    final result = SpineBindings.bindings.spine_bone_pose_world_to_local_rotation(_ptr, worldRotation);
    return result;
  }

  /// Transforms a local rotation to a world rotation.
  double localToWorldRotation(double localRotation) {
    final result = SpineBindings.bindings.spine_bone_pose_local_to_world_rotation(_ptr, localRotation);
    return result;
  }

  /// Rotates the world transform the specified amount.
  void rotateWorld(double degrees) {
    SpineBindings.bindings.spine_bone_pose_rotate_world(_ptr, degrees);
  }

  static Rtti rttiStatic() {
    final result = SpineBindings.bindings.spine_bone_pose_rtti();
    return Rtti.fromPointer(result);
  }
}
