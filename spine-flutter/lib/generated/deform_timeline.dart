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
import 'bounding_box_attachment.dart';
import 'clipping_attachment.dart';
import 'mesh_attachment.dart';
import 'path_attachment.dart';
import 'slot_curve_timeline.dart';
import 'vertex_attachment.dart';

/// Changes a slot's deform to deform a VertexAttachment.
class DeformTimeline extends SlotCurveTimeline {
  final Pointer<spine_deform_timeline_wrapper> _ptr;

  DeformTimeline.fromPointer(this._ptr)
      : super.fromPointer(SpineBindings.bindings.spine_deform_timeline_cast_to_slot_curve_timeline(_ptr));

  /// Get the native pointer for FFI calls
  @override
  Pointer get nativePtr => _ptr;

  factory DeformTimeline(int frameCount, int bezierCount, int slotIndex, VertexAttachment attachment) {
    final ptr = SpineBindings.bindings
        .spine_deform_timeline_create(frameCount, bezierCount, slotIndex, attachment.nativePtr.cast());
    return DeformTimeline.fromPointer(ptr);
  }

  void dispose() {
    SpineBindings.bindings.spine_deform_timeline_dispose(_ptr);
  }

  /// Sets the time and vertices for the specified frame.
  void setFrame(int frameIndex, double time, ArrayFloat vertices) {
    SpineBindings.bindings.spine_deform_timeline_set_frame(_ptr, frameIndex, time, vertices.nativePtr.cast());
  }

  /// The attachment whose vertices will be deformed.
  VertexAttachment get attachment {
    final result = SpineBindings.bindings.spine_deform_timeline_get_attachment(_ptr);
    final rtti = SpineBindings.bindings.spine_vertex_attachment_get_rtti(result);
    final className = SpineBindings.bindings.spine_rtti_get_class_name(rtti).cast<Utf8>().toDartString();
    switch (className) {
      case 'BoundingBoxAttachment':
        final castedPtr = SpineBindings.bindings.spine_vertex_attachment_cast_to_bounding_box_attachment(result);
        return BoundingBoxAttachment.fromPointer(castedPtr);
      case 'ClippingAttachment':
        final castedPtr = SpineBindings.bindings.spine_vertex_attachment_cast_to_clipping_attachment(result);
        return ClippingAttachment.fromPointer(castedPtr);
      case 'MeshAttachment':
        final castedPtr = SpineBindings.bindings.spine_vertex_attachment_cast_to_mesh_attachment(result);
        return MeshAttachment.fromPointer(castedPtr);
      case 'PathAttachment':
        final castedPtr = SpineBindings.bindings.spine_vertex_attachment_cast_to_path_attachment(result);
        return PathAttachment.fromPointer(castedPtr);
      default:
        throw UnsupportedError('Unknown concrete type: $className for abstract class VertexAttachment');
    }
  }

  set attachment(VertexAttachment value) {
    SpineBindings.bindings.spine_deform_timeline_set_attachment(_ptr, value.nativePtr.cast());
  }

  double getCurvePercent(double time, int frame) {
    final result = SpineBindings.bindings.spine_deform_timeline_get_curve_percent(_ptr, time, frame);
    return result;
  }
}
