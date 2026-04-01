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
import 'blend_mode.dart';
import 'bone_data.dart';
import 'posed_data.dart';
import 'slot_pose.dart';

/// Stores the setup pose for a Slot.
class SlotData extends PosedData {
  final Pointer<spine_slot_data_wrapper> _ptr;

  SlotData.fromPointer(this._ptr) : super.fromPointer(SpineBindings.bindings.spine_slot_data_cast_to_posed_data(_ptr));

  /// Get the native pointer for FFI calls
  @override
  Pointer get nativePtr => _ptr;

  factory SlotData(int index, String name, BoneData boneData) {
    final ptr = SpineBindings.bindings
        .spine_slot_data_create(index, name.toNativeUtf8().cast<Char>(), boneData.nativePtr.cast());
    return SlotData.fromPointer(ptr);
  }

  @override
  void dispose() {
    SpineBindings.bindings.spine_slot_data_dispose(_ptr);
  }

  /// The Skeleton::getSlots() index for this slot.
  int get index {
    final result = SpineBindings.bindings.spine_slot_data_get_index(_ptr);
    return result;
  }

  /// The bone this slot belongs to.
  BoneData get boneData {
    final result = SpineBindings.bindings.spine_slot_data_get_bone_data(_ptr);
    return BoneData.fromPointer(result);
  }

  set attachmentName(String value) {
    SpineBindings.bindings.spine_slot_data_set_attachment_name(_ptr, value.toNativeUtf8().cast<Char>());
  }

  /// The name of the attachment that is visible for this slot in the setup
  /// pose, or empty if no attachment is visible.
  String get attachmentName {
    final result = SpineBindings.bindings.spine_slot_data_get_attachment_name(_ptr);
    return result.cast<Utf8>().toDartString();
  }

  /// The blend mode for drawing the slot's attachment.
  BlendMode get blendMode {
    final result = SpineBindings.bindings.spine_slot_data_get_blend_mode(_ptr);
    return BlendMode.fromValue(result);
  }

  set blendMode(BlendMode value) {
    SpineBindings.bindings.spine_slot_data_set_blend_mode(_ptr, value.value);
  }

  /// False if the slot was hidden in Spine and nonessential data was exported.
  /// Does not affect runtime rendering.
  bool get visible {
    final result = SpineBindings.bindings.spine_slot_data_get_visible(_ptr);
    return result;
  }

  set visible(bool value) {
    SpineBindings.bindings.spine_slot_data_set_visible(_ptr, value);
  }

  /// The setup pose that most animations are relative to.
  SlotPose get setupPose {
    final result = SpineBindings.bindings.spine_slot_data_get_setup_pose(_ptr);
    return SlotPose.fromPointer(result);
  }
}
