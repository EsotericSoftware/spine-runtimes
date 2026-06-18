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
import 'color.dart';
import 'mix_from.dart';
import 'skeleton.dart';

/// Stores a list of timelines to animate a skeleton's pose over time.
///
/// See Applying Animations in the Spine Runtimes Guide.
class Animation {
  final Pointer<spine_animation_wrapper> _ptr;

  Animation.fromPointer(this._ptr);

  /// Get the native pointer for FFI calls
  Pointer get nativePtr => _ptr;

  /// Creates a new animation. The timelines must be set before use.
  factory Animation(String name) {
    final ptr = SpineBindings.bindings.spine_animation_create(name.toNativeUtf8().cast<Char>());
    return Animation.fromPointer(ptr);
  }

  void dispose() {
    SpineBindings.bindings.spine_animation_dispose(_ptr);
  }

  /// If this list or the timelines it contains are modified, the timelines and
  /// bones must be set again to recompute the animation's bone indices and
  /// timeline property IDs.
  ///
  /// See setTimelines().
  ArrayTimeline get timelines {
    final result = SpineBindings.bindings.spine_animation_get_timelines(_ptr);
    return ArrayTimeline.fromPointer(result);
  }

  /// Sets the timelines and bone indices.
  void setTimelines(ArrayTimeline timelines, ArrayInt bones) {
    SpineBindings.bindings.spine_animation_set_timelines(_ptr, timelines.nativePtr.cast(), bones.nativePtr.cast());
  }

  /// Returns true if this animation contains a timeline with any of the
  /// specified property IDs.
  bool hasTimeline(ArrayPropertyId ids) {
    final result = SpineBindings.bindings.spine_animation_has_timeline(_ptr, ids.nativePtr.cast());
    return result;
  }

  /// The duration of the animation in seconds, which is usually the highest
  /// time of all frames in the timeline. The duration is used to know when it
  /// has completed and when it should loop back to the start.
  double get duration {
    final result = SpineBindings.bindings.spine_animation_get_duration(_ptr);
    return result;
  }

  set duration(double value) {
    SpineBindings.bindings.spine_animation_set_duration(_ptr, value);
  }

  /// Applies the animation's timelines to the specified skeleton.
  ///
  /// See Timeline::apply() and Applying Animations in the Spine Runtimes Guide.
  ///
  /// [skeleton] The skeleton the animation is applied to. This provides access to the bones, slots, and other skeleton components the timelines may change.
  /// [lastTime] The last time in seconds this animation was applied. Some timelines trigger only at discrete times, in which case all keys are triggered between lastTime (exclusive) and time (inclusive). Pass -1 the first time an animation is applied to ensure frame 0 is triggered.
  /// [time] The time in seconds the skeleton is being posed for. Timelines find the frame before and after this time and interpolate between the frame values.
  /// [loop] True if time beyond the animation duration repeats the animation, else the last frame is used.
  /// [events] If any events are fired, they are added to this list. Can be NULL to ignore fired events or if no timelines fire events.
  /// [alpha] 0 applies setup or current values (depending on from), 1 uses timeline values, and intermediate values interpolate between them. Adjusting alpha over time can mix an animation in or out.
  /// [from] If true, alpha transitions between setup and timeline values, setup values are used before the first frame (current values are not used). If false, alpha transitions between current and timeline values, no change is made before the first frame.
  /// [add] If true, for timelines that support it, their values are added to the setup or current values (depending on from).
  /// [out] True when the animation is mixing out, else it is mixing in. Used by timelines that perform instant transitions.
  /// [appliedPose] True to modify getAppliedPose(), else the unconstrained pose is modified.
  void apply(Skeleton skeleton, double lastTime, double time, bool loop, ArrayEvent? events, double alpha, MixFrom from,
      bool add, bool out, bool appliedPose) {
    SpineBindings.bindings.spine_animation_apply(_ptr, skeleton.nativePtr.cast(), lastTime, time, loop,
        events?.nativePtr.cast() ?? Pointer.fromAddress(0), alpha, from.value, add, out, appliedPose);
  }

  /// The animation's name, which is unique across all animations in the
  /// skeleton.
  String get name {
    final result = SpineBindings.bindings.spine_animation_get_name(_ptr);
    return result.cast<Utf8>().toDartString();
  }

  /// The Skeleton::getBones() indices affected by this animation.
  ///
  /// See setTimelines() and BoneTimeline::getBoneIndex().
  ArrayInt get bones {
    final result = SpineBindings.bindings.spine_animation_get_bones(_ptr);
    return ArrayInt.fromPointer(result);
  }

  /// The color of the animation as it was in Spine, or a default color if
  /// nonessential data was not exported.
  Color get color {
    final result = SpineBindings.bindings.spine_animation_get_color(_ptr);
    return Color.fromPointer(result);
  }

  /// [target] After the first and before the last entry.
  static int search(ArrayFloat values, double target) {
    final result = SpineBindings.bindings.spine_animation_search_1(values.nativePtr.cast(), target);
    return result;
  }

  static int search2(ArrayFloat values, double target, int step) {
    final result = SpineBindings.bindings.spine_animation_search_2(values.nativePtr.cast(), target, step);
    return result;
  }
}
