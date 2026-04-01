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
import 'event_data.dart';

/// Fired by EventTimeline when specific animation times are reached.
///
/// See Timeline::apply(), AnimationStateListener::event(), and
/// https://esotericsoftware.com/spine-events Events in the Spine User Guide.
class Event {
  final Pointer<spine_event_wrapper> _ptr;

  Event.fromPointer(this._ptr);

  /// Get the native pointer for FFI calls
  Pointer get nativePtr => _ptr;

  factory Event(double time, EventData data) {
    final ptr = SpineBindings.bindings.spine_event_create(time, data.nativePtr.cast());
    return Event.fromPointer(ptr);
  }

  void dispose() {
    SpineBindings.bindings.spine_event_dispose(_ptr);
  }

  /// The event's setup pose data.
  EventData get data {
    final result = SpineBindings.bindings.spine_event_get_data(_ptr);
    return EventData.fromPointer(result);
  }

  /// The animation time this event was keyed, or -1 for the setup pose.
  double get time {
    final result = SpineBindings.bindings.spine_event_get_time(_ptr);
    return result;
  }

  /// The integer payload for this event.
  int get intValue {
    final result = SpineBindings.bindings.spine_event_get_int(_ptr);
    return result;
  }

  set intValue(int value) {
    SpineBindings.bindings.spine_event_set_int(_ptr, value);
  }

  /// The float payload for this event.
  double get floatValue {
    final result = SpineBindings.bindings.spine_event_get_float(_ptr);
    return result;
  }

  set floatValue(double value) {
    SpineBindings.bindings.spine_event_set_float(_ptr, value);
  }

  /// The string payload for this event.
  String get stringValue {
    final result = SpineBindings.bindings.spine_event_get_string(_ptr);
    return result.cast<Utf8>().toDartString();
  }

  set stringValue(String value) {
    SpineBindings.bindings.spine_event_set_string(_ptr, value.toNativeUtf8().cast<Char>());
  }

  /// If an audio path is set, the volume for the audio.
  double get volume {
    final result = SpineBindings.bindings.spine_event_get_volume(_ptr);
    return result;
  }

  set volume(double value) {
    SpineBindings.bindings.spine_event_set_volume(_ptr, value);
  }

  /// If an audio path is set, the left/right balance for the audio.
  double get balance {
    final result = SpineBindings.bindings.spine_event_get_balance(_ptr);
    return result;
  }

  set balance(double value) {
    SpineBindings.bindings.spine_event_set_balance(_ptr, value);
  }
}
