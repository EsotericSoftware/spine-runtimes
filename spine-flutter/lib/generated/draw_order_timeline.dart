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

/// Changes the Skeleton::getDrawOrder().
class DrawOrderTimeline extends Timeline {
  final Pointer<spine_draw_order_timeline_wrapper> _ptr;

  DrawOrderTimeline.fromPointer(this._ptr)
      : super.fromPointer(SpineBindings.bindings.spine_draw_order_timeline_cast_to_timeline(_ptr));

  /// Get the native pointer for FFI calls
  @override
  Pointer get nativePtr => _ptr;

  factory DrawOrderTimeline(int frameCount) {
    final ptr = SpineBindings.bindings.spine_draw_order_timeline_create(frameCount);
    return DrawOrderTimeline.fromPointer(ptr);
  }

  void dispose() {
    SpineBindings.bindings.spine_draw_order_timeline_dispose(_ptr);
  }

  /// Sets the time and draw order for the specified frame.
  ///
  /// [frame] Between 0 and frameCount, inclusive.
  /// [time] The frame time in seconds.
  /// [drawOrder] For each slot in Skeleton::getSlots(), the index of the slot in the new draw order. May be null to use setup pose draw order.
  void setFrame(int frame, double time, ArrayInt? drawOrder) {
    SpineBindings.bindings
        .spine_draw_order_timeline_set_frame(_ptr, frame, time, drawOrder?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }
}
