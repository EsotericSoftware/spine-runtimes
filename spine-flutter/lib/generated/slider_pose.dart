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

/// Stores a pose for a slider.
class SliderPose {
  final Pointer<spine_slider_pose_wrapper> _ptr;

  SliderPose.fromPointer(this._ptr);

  /// Get the native pointer for FFI calls
  Pointer get nativePtr => _ptr;

  factory SliderPose() {
    final ptr = SpineBindings.bindings.spine_slider_pose_create();
    return SliderPose.fromPointer(ptr);
  }

  void dispose() {
    SpineBindings.bindings.spine_slider_pose_dispose(_ptr);
  }

  void set(SliderPose pose) {
    SpineBindings.bindings.spine_slider_pose_set(_ptr, pose.nativePtr.cast());
  }

  /// The time in SliderData::getAnimation() to apply the animation.
  double get time {
    final result = SpineBindings.bindings.spine_slider_pose_get_time(_ptr);
    return result;
  }

  set time(double value) {
    SpineBindings.bindings.spine_slider_pose_set_time(_ptr, value);
  }

  /// A percentage that controls the mix between the constrained and
  /// unconstrained poses.
  double get mix {
    final result = SpineBindings.bindings.spine_slider_pose_get_mix(_ptr);
    return result;
  }

  set mix(double value) {
    SpineBindings.bindings.spine_slider_pose_set_mix(_ptr, value);
  }
}
