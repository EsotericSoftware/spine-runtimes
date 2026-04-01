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
import 'skeleton.dart';
import 'transform_constraint_base.dart';
import 'transform_constraint_data.dart';

/// Adjusts the world transform of the constrained bones to match that of the
/// source bone.
///
/// See https://esotericsoftware.com/spine-transform-constraints Transform
/// constraints in the Spine User Guide.
class TransformConstraint extends TransformConstraintBase {
  final Pointer<spine_transform_constraint_wrapper> _ptr;

  TransformConstraint.fromPointer(this._ptr)
      : super.fromPointer(SpineBindings.bindings.spine_transform_constraint_cast_to_transform_constraint_base(_ptr));

  /// Get the native pointer for FFI calls
  @override
  Pointer get nativePtr => _ptr;

  factory TransformConstraint(TransformConstraintData data, Skeleton skeleton) {
    final ptr =
        SpineBindings.bindings.spine_transform_constraint_create(data.nativePtr.cast(), skeleton.nativePtr.cast());
    return TransformConstraint.fromPointer(ptr);
  }

  @override
  void dispose() {
    SpineBindings.bindings.spine_transform_constraint_dispose(_ptr);
  }

  TransformConstraint copy(Skeleton skeleton) {
    final result = SpineBindings.bindings.spine_transform_constraint_copy(_ptr, skeleton.nativePtr.cast());
    return TransformConstraint.fromPointer(result);
  }

  /// The bones that will be modified by this transform constraint.
  ArrayBonePose get bones {
    final result = SpineBindings.bindings.spine_transform_constraint_get_bones(_ptr);
    return ArrayBonePose.fromPointer(result);
  }

  /// The bone whose world transform will be matched by the constrained bones.
  Bone get source {
    final result = SpineBindings.bindings.spine_transform_constraint_get_source(_ptr);
    return Bone.fromPointer(result);
  }

  set source(Bone value) {
    SpineBindings.bindings.spine_transform_constraint_set_source(_ptr, value.nativePtr.cast());
  }
}
