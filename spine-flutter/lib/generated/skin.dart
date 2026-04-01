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
import 'arrays.dart';
import 'attachment.dart';
import 'bounding_box_attachment.dart';
import 'clipping_attachment.dart';
import 'color.dart';
import 'mesh_attachment.dart';
import 'path_attachment.dart';
import 'point_attachment.dart';
import 'region_attachment.dart';

/// Stores attachments by slot index and placeholder name. Multiple Skeleton
/// instances can use the same skins. See SkeletonData::getDefaultSkin,
/// Skeleton::getSkin, and http://esotericsoftware.com/spine-runtime-skins in
/// the Spine Runtimes Guide.
class Skin {
  final Pointer<spine_skin_wrapper> _ptr;

  Skin.fromPointer(this._ptr);

  /// Get the native pointer for FFI calls
  Pointer get nativePtr => _ptr;

  factory Skin(String name) {
    final ptr = SpineBindings.bindings.spine_skin_create(name.toNativeUtf8().cast<Char>());
    return Skin.fromPointer(ptr);
  }

  void dispose() {
    SpineBindings.bindings.spine_skin_dispose(_ptr);
  }

  /// Adds an attachment to the skin for the specified slot index and
  /// placeholder name. If the placeholder name already exists for the slot, the
  /// previous value is replaced.
  void setAttachment(int slotIndex, String placeholderName, Attachment? attachment) {
    SpineBindings.bindings.spine_skin_set_attachment(_ptr, slotIndex, placeholderName.toNativeUtf8().cast<Char>(),
        attachment?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// Returns the attachment for the specified slot index and placeholder name,
  /// or NULL.
  Attachment? getAttachment(int slotIndex, String placeholderName) {
    final result =
        SpineBindings.bindings.spine_skin_get_attachment(_ptr, slotIndex, placeholderName.toNativeUtf8().cast<Char>());
    if (result.address == 0) return null;
    final rtti = SpineBindings.bindings.spine_attachment_get_rtti(result);
    final className = SpineBindings.bindings.spine_rtti_get_class_name(rtti).cast<Utf8>().toDartString();
    switch (className) {
      case 'BoundingBoxAttachment':
        final castedPtr = SpineBindings.bindings.spine_attachment_cast_to_bounding_box_attachment(result);
        return BoundingBoxAttachment.fromPointer(castedPtr);
      case 'ClippingAttachment':
        final castedPtr = SpineBindings.bindings.spine_attachment_cast_to_clipping_attachment(result);
        return ClippingAttachment.fromPointer(castedPtr);
      case 'MeshAttachment':
        final castedPtr = SpineBindings.bindings.spine_attachment_cast_to_mesh_attachment(result);
        return MeshAttachment.fromPointer(castedPtr);
      case 'PathAttachment':
        final castedPtr = SpineBindings.bindings.spine_attachment_cast_to_path_attachment(result);
        return PathAttachment.fromPointer(castedPtr);
      case 'PointAttachment':
        final castedPtr = SpineBindings.bindings.spine_attachment_cast_to_point_attachment(result);
        return PointAttachment.fromPointer(castedPtr);
      case 'RegionAttachment':
        final castedPtr = SpineBindings.bindings.spine_attachment_cast_to_region_attachment(result);
        return RegionAttachment.fromPointer(castedPtr);
      default:
        throw UnsupportedError('Unknown concrete type: $className for abstract class Attachment');
    }
  }

  /// Removes the attachment from the skin.
  void removeAttachment(int slotIndex, String placeholderName) {
    SpineBindings.bindings.spine_skin_remove_attachment(_ptr, slotIndex, placeholderName.toNativeUtf8().cast<Char>());
  }

  /// Finds the attachments for a given slot. The results are added to the
  /// passed array of Attachments.
  ///
  /// [slotIndex] The target slotIndex. To find the slot index, use SkeletonData::findSlot and SlotData::getIndex.
  /// [attachments] Found Attachments will be added to this array.
  void findAttachmentsForSlot(int slotIndex, ArrayAttachment attachments) {
    SpineBindings.bindings.spine_skin_find_attachments_for_slot(_ptr, slotIndex, attachments.nativePtr.cast());
  }

  String get name {
    final result = SpineBindings.bindings.spine_skin_get_name(_ptr);
    return result.cast<Utf8>().toDartString();
  }

  /// Adds all attachments, bones, and constraints from the specified skin to
  /// this skin.
  void addSkin(Skin other) {
    SpineBindings.bindings.spine_skin_add_skin(_ptr, other.nativePtr.cast());
  }

  /// Adds all attachments, bones, and constraints from the specified skin to
  /// this skin. Attachments are deep copied.
  void copySkin(Skin other) {
    SpineBindings.bindings.spine_skin_copy_skin(_ptr, other.nativePtr.cast());
  }

  ArrayBoneData get bones {
    final result = SpineBindings.bindings.spine_skin_get_bones(_ptr);
    return ArrayBoneData.fromPointer(result);
  }

  ArrayConstraintData get constraints {
    final result = SpineBindings.bindings.spine_skin_get_constraints(_ptr);
    return ArrayConstraintData.fromPointer(result);
  }

  Color get color {
    final result = SpineBindings.bindings.spine_skin_get_color(_ptr);
    return Color.fromPointer(result);
  }
}
