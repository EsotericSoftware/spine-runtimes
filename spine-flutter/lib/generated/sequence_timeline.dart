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
import 'bounding_box_attachment.dart';
import 'clipping_attachment.dart';
import 'mesh_attachment.dart';
import 'path_attachment.dart';
import 'point_attachment.dart';
import 'region_attachment.dart';
import 'sequence_mode.dart';
import 'slot_timeline.dart';
import 'timeline.dart';

/// Changes the sequence index for an attachment's Sequence.
class SequenceTimeline extends Timeline implements SlotTimeline {
  final Pointer<spine_sequence_timeline_wrapper> _ptr;

  SequenceTimeline.fromPointer(this._ptr)
      : super.fromPointer(SpineBindings.bindings.spine_sequence_timeline_cast_to_timeline(_ptr));

  /// Get the native pointer for FFI calls
  @override
  Pointer get nativePtr => _ptr;

  factory SequenceTimeline(int frameCount, int slotIndex, Attachment attachment) {
    final ptr =
        SpineBindings.bindings.spine_sequence_timeline_create(frameCount, slotIndex, attachment.nativePtr.cast());
    return SequenceTimeline.fromPointer(ptr);
  }

  void dispose() {
    SpineBindings.bindings.spine_sequence_timeline_dispose(_ptr);
  }

  /// Sets the time, mode, index, and frame time for the specified frame.
  ///
  /// [frame] Between 0 and frameCount, inclusive.
  /// [delay] Seconds between frames.
  void setFrame(int frame, double time, SequenceMode mode, int index, double delay) {
    SpineBindings.bindings.spine_sequence_timeline_set_frame(_ptr, frame, time, mode.value, index, delay);
  }

  /// The attachment for which the sequence index will be set.
  ///
  /// See Attachment::getTimelineAttachment().
  Attachment get attachment {
    final result = SpineBindings.bindings.spine_sequence_timeline_get_attachment(_ptr);
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

  @override
  int get slotIndex {
    final result = SpineBindings.bindings.spine_sequence_timeline_get_slot_index(_ptr);
    return result;
  }

  @override
  set slotIndex(int value) {
    SpineBindings.bindings.spine_sequence_timeline_set_slot_index(_ptr, value);
  }
}
