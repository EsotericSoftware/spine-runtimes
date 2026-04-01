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
import 'attachment.dart';
import 'bone_pose.dart';
import 'color.dart';

/// An attachment which is a single point and a rotation. This can be used to
/// spawn projectiles, particles, etc. A bone can be used in similar ways, but a
/// PointAttachment is slightly less expensive to compute and can be hidden,
/// shown, and placed in a skin.
///
/// See https://esotericsoftware.com/spine-points for Point Attachments in the
/// Spine User Guide.
class PointAttachment extends Attachment {
  final Pointer<spine_point_attachment_wrapper> _ptr;

  PointAttachment.fromPointer(this._ptr)
      : super.fromPointer(SpineBindings.bindings.spine_point_attachment_cast_to_attachment(_ptr));

  /// Get the native pointer for FFI calls
  @override
  Pointer get nativePtr => _ptr;

  factory PointAttachment(String name) {
    final ptr = SpineBindings.bindings.spine_point_attachment_create(name.toNativeUtf8().cast<Char>());
    return PointAttachment.fromPointer(ptr);
  }

  void dispose() {
    SpineBindings.bindings.spine_point_attachment_dispose(_ptr);
  }

  /// The local x position.
  double get x {
    final result = SpineBindings.bindings.spine_point_attachment_get_x(_ptr);
    return result;
  }

  set x(double value) {
    SpineBindings.bindings.spine_point_attachment_set_x(_ptr, value);
  }

  /// The local y position.
  double get y {
    final result = SpineBindings.bindings.spine_point_attachment_get_y(_ptr);
    return result;
  }

  set y(double value) {
    SpineBindings.bindings.spine_point_attachment_set_y(_ptr, value);
  }

  /// The local rotation in degrees, counter clockwise.
  double get rotation {
    final result = SpineBindings.bindings.spine_point_attachment_get_rotation(_ptr);
    return result;
  }

  set rotation(double value) {
    SpineBindings.bindings.spine_point_attachment_set_rotation(_ptr, value);
  }

  Color get color {
    final result = SpineBindings.bindings.spine_point_attachment_get_color(_ptr);
    return Color.fromPointer(result);
  }

  /// Computes the world rotation from the local rotation.
  double computeWorldRotation(BonePose bone) {
    final result = SpineBindings.bindings.spine_point_attachment_compute_world_rotation(_ptr, bone.nativePtr.cast());
    return result;
  }
}
