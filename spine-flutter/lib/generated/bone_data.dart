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
import 'bone_pose.dart';
import 'color.dart';
import 'posed_data.dart';

/// BoneData wrapper
class BoneData extends PosedData {
  final Pointer<spine_bone_data_wrapper> _ptr;

  BoneData.fromPointer(this._ptr) : super.fromPointer(SpineBindings.bindings.spine_bone_data_cast_to_posed_data(_ptr));

  /// Get the native pointer for FFI calls
  @override
  Pointer get nativePtr => _ptr;

  factory BoneData(int index, String name, BoneData? parent) {
    final ptr = SpineBindings.bindings.spine_bone_data_create(
        index, name.toNativeUtf8().cast<Char>(), parent?.nativePtr.cast() ?? Pointer.fromAddress(0));
    return BoneData.fromPointer(ptr);
  }

  @override
  void dispose() {
    SpineBindings.bindings.spine_bone_data_dispose(_ptr);
  }

  /// The Skeleton::getBones() index for this bone.
  int get index {
    final result = SpineBindings.bindings.spine_bone_data_get_index(_ptr);
    return result;
  }

  /// The parent bone, or NULL if this bone is the root.
  BoneData? get parent {
    final result = SpineBindings.bindings.spine_bone_data_get_parent(_ptr);
    return result.address == 0 ? null : BoneData.fromPointer(result);
  }

  double get length {
    final result = SpineBindings.bindings.spine_bone_data_get_length(_ptr);
    return result;
  }

  set length(double value) {
    SpineBindings.bindings.spine_bone_data_set_length(_ptr, value);
  }

  Color get color {
    final result = SpineBindings.bindings.spine_bone_data_get_color(_ptr);
    return Color.fromPointer(result);
  }

  /// The bone icon name as it was in Spine, or empty if nonessential data was
  /// not exported.
  String get icon {
    final result = SpineBindings.bindings.spine_bone_data_get_icon(_ptr);
    return result.cast<Utf8>().toDartString();
  }

  set icon(String value) {
    SpineBindings.bindings.spine_bone_data_set_icon(_ptr, value.toNativeUtf8().cast<Char>());
  }

  /// The bone icon's display size scale, or 1 if nonessential data was not
  /// exported.
  double get iconSize {
    final result = SpineBindings.bindings.spine_bone_data_get_icon_size(_ptr);
    return result;
  }

  set iconSize(double value) {
    SpineBindings.bindings.spine_bone_data_set_icon_size(_ptr, value);
  }

  /// The bone icon's display rotation in degrees, or 0 if nonessential data was
  /// not exported.
  double get iconRotation {
    final result = SpineBindings.bindings.spine_bone_data_get_icon_rotation(_ptr);
    return result;
  }

  set iconRotation(double value) {
    SpineBindings.bindings.spine_bone_data_set_icon_rotation(_ptr, value);
  }

  bool get visible {
    final result = SpineBindings.bindings.spine_bone_data_get_visible(_ptr);
    return result;
  }

  set visible(bool value) {
    SpineBindings.bindings.spine_bone_data_set_visible(_ptr, value);
  }

  /// The setup pose that most animations are relative to.
  BonePose get setupPose {
    final result = SpineBindings.bindings.spine_bone_data_get_setup_pose(_ptr);
    return BonePose.fromPointer(result);
  }
}
