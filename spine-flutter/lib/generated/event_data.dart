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
import 'event.dart';

/// Stores the setup pose values for an Event.
class EventData {
  final Pointer<spine_event_data_wrapper> _ptr;

  EventData.fromPointer(this._ptr);

  /// Get the native pointer for FFI calls
  Pointer get nativePtr => _ptr;

  factory EventData(String name) {
    final ptr = SpineBindings.bindings.spine_event_data_create(name.toNativeUtf8().cast<Char>());
    return EventData.fromPointer(ptr);
  }

  void dispose() {
    SpineBindings.bindings.spine_event_data_dispose(_ptr);
  }

  /// The name of the event, unique across all events in the skeleton.
  String get name {
    final result = SpineBindings.bindings.spine_event_data_get_name(_ptr);
    return result.cast<Utf8>().toDartString();
  }

  /// Path to an audio file relative to the audio folder as defined in Spine.
  String get audioPath {
    final result = SpineBindings.bindings.spine_event_data_get_audio_path(_ptr);
    return result.cast<Utf8>().toDartString();
  }

  set audioPath(String value) {
    SpineBindings.bindings.spine_event_data_set_audio_path(_ptr, value.toNativeUtf8().cast<Char>());
  }

  /// The setup values that are shared by all events with this data.
  Event get getSetupPose {
    final result = SpineBindings.bindings.spine_event_data_get_setup_pose_1(_ptr);
    return Event.fromPointer(result);
  }

  Event get setupPose2 {
    final result = SpineBindings.bindings.spine_event_data_get_setup_pose_2(_ptr);
    return Event.fromPointer(result);
  }
}
