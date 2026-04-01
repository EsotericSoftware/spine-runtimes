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
import 'animation.dart';
import 'skeleton_data.dart';

/// Stores mix (crossfade) durations to be applied when AnimationState
/// animations are changed on the same track.
class AnimationStateData {
  final Pointer<spine_animation_state_data_wrapper> _ptr;

  AnimationStateData.fromPointer(this._ptr);

  /// Get the native pointer for FFI calls
  Pointer get nativePtr => _ptr;

  factory AnimationStateData(SkeletonData skeletonData) {
    final ptr = SpineBindings.bindings.spine_animation_state_data_create(skeletonData.nativePtr.cast());
    return AnimationStateData.fromPointer(ptr);
  }

  void dispose() {
    SpineBindings.bindings.spine_animation_state_data_dispose(_ptr);
  }

  /// The SkeletonData to look up animations when they are specified by name.
  SkeletonData get skeletonData {
    final result = SpineBindings.bindings.spine_animation_state_data_get_skeleton_data(_ptr);
    return SkeletonData.fromPointer(result);
  }

  /// The mix duration to use when no mix duration has been specifically defined
  /// between two animations.
  double get defaultMix {
    final result = SpineBindings.bindings.spine_animation_state_data_get_default_mix(_ptr);
    return result;
  }

  set defaultMix(double value) {
    SpineBindings.bindings.spine_animation_state_data_set_default_mix(_ptr, value);
  }

  /// Returns the mix duration to use when changing from the specified animation
  /// to the other on the same track, or the default mix if no mix duration has
  /// been set.
  double getMix(Animation from, Animation to) {
    final result =
        SpineBindings.bindings.spine_animation_state_data_get_mix(_ptr, from.nativePtr.cast(), to.nativePtr.cast());
    return result;
  }

  /// Removes all mixes and sets the default mix to 0.
  void clear() {
    SpineBindings.bindings.spine_animation_state_data_clear(_ptr);
  }

  /// Sets a mix duration by animation names.
  void setMix(String fromName, String toName, double duration) {
    SpineBindings.bindings.spine_animation_state_data_set_mix_1(
        _ptr, fromName.toNativeUtf8().cast<Char>(), toName.toNativeUtf8().cast<Char>(), duration);
  }

  /// Sets a mix duration when changing from the specified animation to the
  /// other. See TrackEntry.MixDuration.
  void setMix2(Animation from, Animation to, double duration) {
    SpineBindings.bindings
        .spine_animation_state_data_set_mix_2(_ptr, from.nativePtr.cast(), to.nativePtr.cast(), duration);
  }
}
