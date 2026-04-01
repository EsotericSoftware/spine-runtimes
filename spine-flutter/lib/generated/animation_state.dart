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
import 'animation_state_data.dart';
import 'arrays.dart';
import 'skeleton.dart';
import 'track_entry.dart';

/// Applies animations over time, queues animations for later playback, mixes
/// (crossfading) between animations, and applies multiple animations on top of
/// each other (layering).
///
/// See Applying Animations in the Spine Runtimes Guide.
class AnimationState {
  final Pointer<spine_animation_state_wrapper> _ptr;

  AnimationState.fromPointer(this._ptr);

  /// Get the native pointer for FFI calls
  Pointer get nativePtr => _ptr;

  factory AnimationState(AnimationStateData data) {
    final ptr = SpineBindings.bindings.spine_animation_state_create(data.nativePtr.cast());
    return AnimationState.fromPointer(ptr);
  }

  void dispose() {
    SpineBindings.bindings.spine_animation_state_dispose(_ptr);
  }

  /// Increments each track entry's track time, setting queued animations as
  /// current if needed.
  void update(double delta) {
    SpineBindings.bindings.spine_animation_state_update(_ptr, delta);
  }

  /// Poses the skeleton using the track entry animations. The animation state
  /// is not changed, so can be applied to multiple skeletons to pose them
  /// identically.
  ///
  /// Returns True if any animations were applied.
  bool apply(Skeleton skeleton) {
    final result = SpineBindings.bindings.spine_animation_state_apply(_ptr, skeleton.nativePtr.cast());
    return result;
  }

  /// Removes all animations from all tracks, leaving skeletons in their current
  /// pose.
  ///
  /// It may be desired to use AnimationState::setEmptyAnimations(float) to mix
  /// the skeletons back to the setup pose, rather than leaving them in their
  /// current pose.
  void clearTracks() {
    SpineBindings.bindings.spine_animation_state_clear_tracks(_ptr);
  }

  /// Removes all animations from the track, leaving skeletons in their current
  /// pose.
  ///
  /// It may be desired to use AnimationState::setEmptyAnimation(int, float) to
  /// mix the skeletons back to the setup pose, rather than leaving them in
  /// their current pose.
  void clearTrack(int trackIndex) {
    SpineBindings.bindings.spine_animation_state_clear_track(_ptr, trackIndex);
  }

  /// Sets an empty animation for a track, discarding any queued animations, and
  /// sets the track entry's TrackEntry::getMixDuration(). An empty animation
  /// has no timelines and serves as a placeholder for mixing in or out.
  ///
  /// Mixing out is done by setting an empty animation with a mix duration using
  /// either setEmptyAnimation(int, float), setEmptyAnimations(float), or
  /// addEmptyAnimation(int, float, float). Mixing to an empty animation causes
  /// the previous animation to be applied less and less over the mix duration.
  /// Properties keyed in the previous animation transition to the value from
  /// lower tracks or to the setup pose value if no lower tracks key the
  /// property. A mix duration of 0 still mixes out over one frame.
  ///
  /// Mixing in is done by first setting an empty animation, then adding an
  /// animation using addAnimation(int, Animation, bool, float) with the desired
  /// delay (an empty animation has a duration of 0) and on the returned track
  /// entry set TrackEntry::setMixDuration(float). Mixing from an empty
  /// animation causes the new animation to be applied more and more over the
  /// mix duration. Properties keyed in the new animation transition from the
  /// value from lower tracks or from the setup pose value if no lower tracks
  /// key the property to the value keyed in the new animation.
  ///
  /// See Empty animations in the Spine Runtimes Guide.
  TrackEntry setEmptyAnimation(int trackIndex, double mixDuration) {
    final result = SpineBindings.bindings.spine_animation_state_set_empty_animation(_ptr, trackIndex, mixDuration);
    return TrackEntry.fromPointer(result);
  }

  /// Adds an empty animation to be played after the current or last queued
  /// animation for a track, and sets the track entry's
  /// TrackEntry::getMixDuration(). If the track has no entries, it is
  /// equivalent to calling setEmptyAnimation(int, float).
  ///
  /// See setEmptyAnimation(int, float) and Empty animations in the Spine
  /// Runtimes Guide.
  ///
  /// [delay] If > 0, sets TrackEntry::getDelay(). If < = 0, the delay set is the duration of the previous track entry minus any mix duration plus the specified delay (ie the mix ends at ( delay = 0) or before ( delay < 0) the previous track entry duration). If the previous entry is looping, its next loop completion is used instead of its duration.
  ///
  /// Returns A track entry to allow further customization of animation playback. References to the track entry must not be kept after the AnimationStateListener::dispose(TrackEntry) event occurs.
  TrackEntry addEmptyAnimation(int trackIndex, double mixDuration, double delay) {
    final result =
        SpineBindings.bindings.spine_animation_state_add_empty_animation(_ptr, trackIndex, mixDuration, delay);
    return TrackEntry.fromPointer(result);
  }

  /// Sets an empty animation for every track, discarding any queued animations,
  /// and mixes to it over the specified mix duration.
  ///
  /// See Empty animations in the Spine Runtimes Guide.
  set emptyAnimations(double value) {
    SpineBindings.bindings.spine_animation_state_set_empty_animations(_ptr, value);
  }

  /// Returns The track entry for the animation currently playing on the track, or NULL if no animation is currently playing.
  TrackEntry? getCurrent(int trackIndex) {
    final result = SpineBindings.bindings.spine_animation_state_get_current(_ptr, trackIndex);
    return result.address == 0 ? null : TrackEntry.fromPointer(result);
  }

  /// The AnimationStateData to look up mix durations.
  AnimationStateData get data {
    final result = SpineBindings.bindings.spine_animation_state_get_data(_ptr);
    return AnimationStateData.fromPointer(result);
  }

  /// The list of tracks that have had animations, which may contain null
  /// entries for tracks that currently have no animation.
  ArrayTrackEntry get tracks {
    final result = SpineBindings.bindings.spine_animation_state_get_tracks(_ptr);
    return ArrayTrackEntry.fromPointer(result);
  }

  /// Multiplier for the delta time when the animation state is updated, causing
  /// time for all animations and mixes to play slower or faster. Defaults to 1.
  ///
  /// See TrackEntry::getTimeScale() for affecting a single animation.
  double get timeScale {
    final result = SpineBindings.bindings.spine_animation_state_get_time_scale(_ptr);
    return result;
  }

  set timeScale(double value) {
    SpineBindings.bindings.spine_animation_state_set_time_scale(_ptr, value);
  }

  void disableQueue() {
    SpineBindings.bindings.spine_animation_state_disable_queue(_ptr);
  }

  void enableQueue() {
    SpineBindings.bindings.spine_animation_state_enable_queue(_ptr);
  }

  set manualTrackEntryDisposal(bool value) {
    SpineBindings.bindings.spine_animation_state_set_manual_track_entry_disposal(_ptr, value);
  }

  bool get manualTrackEntryDisposal {
    final result = SpineBindings.bindings.spine_animation_state_get_manual_track_entry_disposal(_ptr);
    return result;
  }

  void disposeTrackEntry(TrackEntry? entry) {
    SpineBindings.bindings
        .spine_animation_state_dispose_track_entry(_ptr, entry?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  Pointer<Void>? get rendererObject {
    final result = SpineBindings.bindings.spine_animation_state_get_renderer_object(_ptr);
    return result;
  }

  /// Sets an animation by name.
  ///
  /// See setAnimation(int, Animation, bool).
  TrackEntry setAnimation(int trackIndex, String animationName, bool loop) {
    final result = SpineBindings.bindings
        .spine_animation_state_set_animation_1(_ptr, trackIndex, animationName.toNativeUtf8().cast<Char>(), loop);
    return TrackEntry.fromPointer(result);
  }

  /// Sets the current animation for a track, discarding any queued animations.
  ///
  /// If the formerly current track entry is for the same animation and was
  /// never applied to a skeleton, it is replaced (not mixed from).
  ///
  /// [loop] If true, the animation will repeat. If false, it will not, instead its last frame is applied if played beyond its duration. In either case TrackEntry.TrackEnd determines when the track is cleared.
  ///
  /// Returns A track entry to allow further customization of animation playback. References to the track entry must not be kept after AnimationState.Dispose.
  TrackEntry setAnimation2(int trackIndex, Animation animation, bool loop) {
    final result = SpineBindings.bindings
        .spine_animation_state_set_animation_2(_ptr, trackIndex, animation.nativePtr.cast(), loop);
    return TrackEntry.fromPointer(result);
  }

  /// Queues an animation by name.
  ///
  /// See addAnimation(int, Animation, bool, float).
  TrackEntry addAnimation(int trackIndex, String animationName, bool loop, double delay) {
    final result = SpineBindings.bindings.spine_animation_state_add_animation_1(
        _ptr, trackIndex, animationName.toNativeUtf8().cast<Char>(), loop, delay);
    return TrackEntry.fromPointer(result);
  }

  /// Adds an animation to be played delay seconds after the current or last
  /// queued animation for a track. If the track has no entries, this is
  /// equivalent to calling setAnimation.
  ///
  /// [delay] Seconds to begin this animation after the start of the previous animation. May be < = 0 to use the animation duration of the previous track minus any mix duration plus the negative delay.
  ///
  /// Returns A track entry to allow further customization of animation playback. References to the track entry must not be kept after AnimationState.Dispose
  TrackEntry addAnimation2(int trackIndex, Animation animation, bool loop, double delay) {
    final result = SpineBindings.bindings
        .spine_animation_state_add_animation_2(_ptr, trackIndex, animation.nativePtr.cast(), loop, delay);
    return TrackEntry.fromPointer(result);
  }
}
