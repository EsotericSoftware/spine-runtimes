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
import 'rtti.dart';
import 'bounding_box_attachment.dart';
import 'clipping_attachment.dart';
import 'mesh_attachment.dart';
import 'path_attachment.dart';
import 'point_attachment.dart';
import 'region_attachment.dart';

/// The base class for all attachments. Multiple Skeleton instances, slots, or
/// skins can use the same attachments.
abstract class Attachment {
  final Pointer<spine_attachment_wrapper> _ptr;

  Attachment.fromPointer(this._ptr);

  /// Get the native pointer for FFI calls
  Pointer get nativePtr => _ptr;

  Rtti get rtti {
    final result = SpineBindings.bindings.spine_attachment_get_rtti(_ptr);
    return Rtti.fromPointer(result);
  }

  String get name {
    final result = SpineBindings.bindings.spine_attachment_get_name(_ptr);
    return result.cast<Utf8>().toDartString();
  }

  Attachment copy() {
    final result = SpineBindings.bindings.spine_attachment_copy(_ptr);
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

  Attachment? get timelineAttachment {
    final result = SpineBindings.bindings.spine_attachment_get_timeline_attachment(_ptr);
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

  set timelineAttachment(Attachment? value) {
    SpineBindings.bindings
        .spine_attachment_set_timeline_attachment(_ptr, value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  int get refCount {
    final result = SpineBindings.bindings.spine_attachment_get_ref_count(_ptr);
    return result;
  }

  void reference() {
    SpineBindings.bindings.spine_attachment_reference(_ptr);
  }

  void dereference() {
    SpineBindings.bindings.spine_attachment_dereference(_ptr);
  }

  static Rtti rttiStatic() {
    final result = SpineBindings.bindings.spine_attachment_rtti();
    return Rtti.fromPointer(result);
  }
}
