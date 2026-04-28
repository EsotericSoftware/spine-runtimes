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
import 'arrays.dart';
import 'bone.dart';
import 'bone_pose.dart';
import 'ik_constraint_base.dart';
import 'ik_constraint_data.dart';
import 'scale_y.dart';
import 'skeleton.dart';

/// IkConstraint wrapper
class IkConstraint extends IkConstraintBase {
  final Pointer<spine_ik_constraint_wrapper> _ptr;

  IkConstraint.fromPointer(this._ptr)
      : super.fromPointer(SpineBindings.bindings.spine_ik_constraint_cast_to_ik_constraint_base(_ptr));

  /// Get the native pointer for FFI calls
  @override
  Pointer get nativePtr => _ptr;

  factory IkConstraint(IkConstraintData data, Skeleton skeleton) {
    final ptr = SpineBindings.bindings.spine_ik_constraint_create(data.nativePtr.cast(), skeleton.nativePtr.cast());
    return IkConstraint.fromPointer(ptr);
  }

  @override
  void dispose() {
    SpineBindings.bindings.spine_ik_constraint_dispose(_ptr);
  }

  IkConstraint copy(Skeleton skeleton) {
    final result = SpineBindings.bindings.spine_ik_constraint_copy(_ptr, skeleton.nativePtr.cast());
    return IkConstraint.fromPointer(result);
  }

  ArrayBonePose get bones {
    final result = SpineBindings.bindings.spine_ik_constraint_get_bones(_ptr);
    return ArrayBonePose.fromPointer(result);
  }

  Bone get target {
    final result = SpineBindings.bindings.spine_ik_constraint_get_target(_ptr);
    return Bone.fromPointer(result);
  }

  set target(Bone value) {
    SpineBindings.bindings.spine_ik_constraint_set_target(_ptr, value.nativePtr.cast());
  }

  /// Adjusts the local rotation of the bone so the world position of the tip is
  /// as close to the target position as possible. The target is specified in
  /// the world coordinate system.
  static void apply(Skeleton skeleton, BonePose bone, double targetX, double targetY, bool compress, bool stretch,
      ScaleY scaleY, double mix) {
    SpineBindings.bindings.spine_ik_constraint_apply_1(
        skeleton.nativePtr.cast(), bone.nativePtr.cast(), targetX, targetY, compress, stretch, scaleY.value, mix);
  }

  /// Adjusts the parent and child bone rotations so the tip of the child is as
  /// close to the target position as possible. The target is specified in the
  /// world coordinate system.
  ///
  /// [child] A direct descendant of the parent bone.
  static void apply2(Skeleton skeleton, BonePose parent, BonePose child, double targetX, double targetY,
      int bendDirection, bool stretch, ScaleY scaleY, double softness, double mix) {
    SpineBindings.bindings.spine_ik_constraint_apply_2(skeleton.nativePtr.cast(), parent.nativePtr.cast(),
        child.nativePtr.cast(), targetX, targetY, bendDirection, stretch, scaleY.value, softness, mix);
  }
}
