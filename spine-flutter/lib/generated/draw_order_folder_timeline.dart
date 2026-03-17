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
import 'timeline.dart';

/// Changes a subset of a skeleton's Skeleton::getDrawOrder().
class DrawOrderFolderTimeline extends Timeline {
  final Pointer<spine_draw_order_folder_timeline_wrapper> _ptr;

  DrawOrderFolderTimeline.fromPointer(this._ptr)
      : super.fromPointer(SpineBindings.bindings.spine_draw_order_folder_timeline_cast_to_timeline(_ptr));

  /// Get the native pointer for FFI calls
  @override
  Pointer get nativePtr => _ptr;

  factory DrawOrderFolderTimeline(int frameCount, ArrayInt slots, int slotCount) {
    final ptr =
        SpineBindings.bindings.spine_draw_order_folder_timeline_create(frameCount, slots.nativePtr.cast(), slotCount);
    return DrawOrderFolderTimeline.fromPointer(ptr);
  }

  void dispose() {
    SpineBindings.bindings.spine_draw_order_folder_timeline_dispose(_ptr);
  }

  /// The Skeleton::getSlots() indices that this timeline affects, in setup
  /// order.
  ArrayInt get slots {
    final result = SpineBindings.bindings.spine_draw_order_folder_timeline_get_slots(_ptr);
    return ArrayInt.fromPointer(result);
  }

  /// Sets the time and draw order for the specified frame.
  ///
  /// [frame] Between 0 and frameCount, inclusive.
  /// [time] The frame time in seconds.
  /// [drawOrder] Ordered getSlots() indices, or null to use setup pose order.
  void setFrame(int frame, double time, ArrayInt? drawOrder) {
    SpineBindings.bindings.spine_draw_order_folder_timeline_set_frame(
        _ptr, frame, time, drawOrder?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }
}
