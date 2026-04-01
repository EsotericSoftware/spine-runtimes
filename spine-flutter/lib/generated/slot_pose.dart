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

/// Stores a slot's pose.
class SlotPose {
  final Pointer<spine_slot_pose_wrapper> _ptr;

  SlotPose.fromPointer(this._ptr);

  /// Get the native pointer for FFI calls
  Pointer get nativePtr => _ptr;

  factory SlotPose() {
    final ptr = SpineBindings.bindings.spine_slot_pose_create();
    return SlotPose.fromPointer(ptr);
  }

  void dispose() {
    SpineBindings.bindings.spine_slot_pose_dispose(_ptr);
  }

  void set(SlotPose pose) {
    SpineBindings.bindings.spine_slot_pose_set(_ptr, pose.nativePtr.cast());
  }

  /// The color used to tint the slot's attachment. If a dark color is set, this
  /// is used as the light color for two color tinting.
  Color get color {
    final result = SpineBindings.bindings.spine_slot_pose_get_color(_ptr);
    return Color.fromPointer(result);
  }

  /// The dark color used to tint the slot's attachment for two color tinting.
  /// The dark color's alpha is not used.
  Color get darkColor {
    final result = SpineBindings.bindings.spine_slot_pose_get_dark_color(_ptr);
    return Color.fromPointer(result);
  }

  /// Returns true if this slot has a dark color.
  bool get hasDarkColor {
    final result = SpineBindings.bindings.spine_slot_pose_has_dark_color(_ptr);
    return result;
  }

  set hasDarkColor(bool value) {
    SpineBindings.bindings.spine_slot_pose_set_has_dark_color(_ptr, value);
  }

  /// The current attachment for the slot, or null if the slot has no
  /// attachment.
  Attachment? get attachment {
    final result = SpineBindings.bindings.spine_slot_pose_get_attachment(_ptr);
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

  /// Sets the slot's attachment and, if the attachment changed, resets
  /// sequenceIndex and clears the deform. The deform is not cleared if the old
  /// attachment has the same VertexAttachment::getTimelineAttachment() as the
  /// specified attachment.
  set attachment(Attachment? value) {
    SpineBindings.bindings.spine_slot_pose_set_attachment(_ptr, value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  /// The index of the texture region to display when the slot's attachment has
  /// a Sequence. -1 represents the Sequence::getSetupIndex().
  int get sequenceIndex {
    final result = SpineBindings.bindings.spine_slot_pose_get_sequence_index(_ptr);
    return result;
  }

  set sequenceIndex(int value) {
    SpineBindings.bindings.spine_slot_pose_set_sequence_index(_ptr, value);
  }

  /// Values to deform the slot's attachment. For an unweighted mesh, the
  /// entries are local positions for each vertex. For a weighted mesh, the
  /// entries are an offset for each vertex which will be added to the mesh's
  /// local vertex positions.
  ///
  /// See VertexAttachment::computeWorldVertices() and DeformTimeline.
  ArrayFloat get deform {
    final result = SpineBindings.bindings.spine_slot_pose_get_deform(_ptr);
    return ArrayFloat.fromPointer(result);
  }
}
