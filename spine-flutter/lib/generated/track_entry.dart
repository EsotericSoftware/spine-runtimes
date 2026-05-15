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
import 'animation.dart';
import 'animation_state.dart';
import 'interpolation.dart';

/// State for the playback of an animation
class TrackEntry {
  final Pointer<spine_track_entry_wrapper> _ptr;

  TrackEntry.fromPointer(this._ptr);

  /// Get the native pointer for FFI calls
  Pointer get nativePtr => _ptr;

  factory TrackEntry() {
    final ptr = SpineBindings.bindings.spine_track_entry_create();
    return TrackEntry.fromPointer(ptr);
  }

  void dispose() {
    SpineBindings.bindings.spine_track_entry_dispose(_ptr);
  }

  /// The index of the track where this entry is either current or queued.
  int get trackIndex {
    final result = SpineBindings.bindings.spine_track_entry_get_track_index(_ptr);
    return result;
  }

  /// The animation to apply for this track entry.
  Animation get animation {
    final result = SpineBindings.bindings.spine_track_entry_get_animation(_ptr);
    return Animation.fromPointer(result);
  }

  /// Sets the animation for this track entry.
  set animation(Animation value) {
    SpineBindings.bindings.spine_track_entry_set_animation(_ptr, value.nativePtr.cast());
  }

  TrackEntry? get previous {
    final result = SpineBindings.bindings.spine_track_entry_get_previous(_ptr);
    return result.address == 0 ? null : TrackEntry.fromPointer(result);
  }

  /// If true, the animation will repeat. If false, it will not, instead its
  /// last frame is applied if played beyond its duration.
  bool get loop {
    final result = SpineBindings.bindings.spine_track_entry_get_loop(_ptr);
    return result;
  }

  set loop(bool value) {
    SpineBindings.bindings.spine_track_entry_set_loop(_ptr, value);
  }

  /// When true, timelines in this animation that support additive have their
  /// values added to the setup or current pose values instead of replacing
  /// them. Additive can be set for a new track entry only before
  /// AnimationState::apply() is next called.
  bool get additive {
    final result = SpineBindings.bindings.spine_track_entry_get_additive(_ptr);
    return result;
  }

  set additive(bool value) {
    SpineBindings.bindings.spine_track_entry_set_additive(_ptr, value);
  }

  bool get reverse {
    final result = SpineBindings.bindings.spine_track_entry_get_reverse(_ptr);
    return result;
  }

  set reverse(bool value) {
    SpineBindings.bindings.spine_track_entry_set_reverse(_ptr, value);
  }

  bool get shortestRotation {
    final result = SpineBindings.bindings.spine_track_entry_get_shortest_rotation(_ptr);
    return result;
  }

  set shortestRotation(bool value) {
    SpineBindings.bindings.spine_track_entry_set_shortest_rotation(_ptr, value);
  }

  /// Seconds to postpone playing the animation. Must be >= 0. When this track
  /// entry is the current track entry, delay postpones incrementing the track
  /// time. When this track entry is queued, delay is the time from the start of
  /// the previous animation to when this track entry will become the current
  /// track entry (ie when the previous track entry's track time >= this track
  /// entry's delay).
  ///
  /// Time scale affects the delay.
  ///
  /// When passing delay < = 0 to AnimationState::addAnimation(int, Animation,
  /// bool, float), this delay is set using a mix duration from
  /// AnimationStateData. To change the mix duration afterward, use
  /// setMixDuration(float, float) so this delay is adjusted.
  double get delay {
    final result = SpineBindings.bindings.spine_track_entry_get_delay(_ptr);
    return result;
  }

  set delay(double value) {
    SpineBindings.bindings.spine_track_entry_set_delay(_ptr, value);
  }

  /// The time in seconds this track entry has been the current track entry,
  /// starting at 0 and increasing forever. Compare to getAnimationTime(), which
  /// is always between animationStart and animationEnd.
  ///
  /// The track time can be set to start the animation at a time other than 0,
  /// without affecting looping. When doing so, animationLast can be set to the
  /// same value to avoid firing events from the start of the animation.
  double get trackTime {
    final result = SpineBindings.bindings.spine_track_entry_get_track_time(_ptr);
    return result;
  }

  set trackTime(double value) {
    SpineBindings.bindings.spine_track_entry_set_track_time(_ptr, value);
  }

  /// The track time in seconds when this animation will be removed from the
  /// track. Defaults to the highest possible float value, meaning the animation
  /// will be applied until a new animation is set or the track is cleared. If
  /// the track end time is reached, no other animations are queued for
  /// playback, and mixing from any previous animations is complete, then the
  /// properties keyed by the animation are set to the setup pose and the track
  /// is cleared.
  ///
  /// It may be desired to use AnimationState::addEmptyAnimation(int, float,
  /// float) rather than have the animation abruptly cease being applied.
  double get trackEnd {
    final result = SpineBindings.bindings.spine_track_entry_get_track_end(_ptr);
    return result;
  }

  set trackEnd(double value) {
    SpineBindings.bindings.spine_track_entry_set_track_end(_ptr, value);
  }

  /// The time in seconds for the first frame of this animation, both initially
  /// and after looping. Defaults to 0.
  ///
  /// When setting the animation start time, animationLast can be set to the
  /// same value to avoid firing events from the start of the animation.
  double get animationStart {
    final result = SpineBindings.bindings.spine_track_entry_get_animation_start(_ptr);
    return result;
  }

  set animationStart(double value) {
    SpineBindings.bindings.spine_track_entry_set_animation_start(_ptr, value);
  }

  /// The time in seconds for the last frame of this animation. Past this time,
  /// non-looping animations hold the pose at this time while looping animations
  /// will loop back to animationStart. Defaults to the animation duration.
  double get animationEnd {
    final result = SpineBindings.bindings.spine_track_entry_get_animation_end(_ptr);
    return result;
  }

  set animationEnd(double value) {
    SpineBindings.bindings.spine_track_entry_set_animation_end(_ptr, value);
  }

  /// The time in seconds this animation was last applied. Some timelines use
  /// this for one-time triggers. Eg, when this animation is applied, event
  /// timelines will fire all events between the animation last time (exclusive)
  /// and animation time (inclusive). Defaults to -1 to ensure triggers on frame
  /// 0 happen the first time this animation is applied.
  double get animationLast {
    final result = SpineBindings.bindings.spine_track_entry_get_animation_last(_ptr);
    return result;
  }

  set animationLast(double value) {
    SpineBindings.bindings.spine_track_entry_set_animation_last(_ptr, value);
  }

  /// Uses the track time to compute animationTime, which is always between
  /// animationStart and animationEnd. When trackTime is 0, animationTime is
  /// equal to animationStart.
  double get animationTime {
    final result = SpineBindings.bindings.spine_track_entry_get_animation_time(_ptr);
    return result;
  }

  /// Multiplier for the delta time when this track entry is updated, causing
  /// time for this animation to pass slower or faster. Defaults to 1.
  ///
  /// Values < 0 are not supported. To play an animation in reverse, use
  /// reverse.
  ///
  /// mixTime is not affected by track entry time scale, so mixDuration may need
  /// to be adjusted to match the animation speed.
  ///
  /// When using AnimationState::addAnimation(int, Animation, bool, float) with
  /// a delay < = 0, delay is set using the mix duration from
  /// AnimationStateData, assuming time scale to be 1. If the time scale is not
  /// 1, the delay may need to be adjusted.
  ///
  /// See AnimationState::getTimeScale() for affecting all animations.
  double get timeScale {
    final result = SpineBindings.bindings.spine_track_entry_get_time_scale(_ptr);
    return result;
  }

  set timeScale(double value) {
    SpineBindings.bindings.spine_track_entry_set_time_scale(_ptr, value);
  }

  /// Values less than 1 mix this animation with the last skeleton pose.
  /// Defaults to 1, which overwrites the last skeleton pose with this
  /// animation.
  ///
  /// Typically track 0 is used to completely pose the skeleton, then alpha can
  /// be used on higher tracks. It doesn't make sense to use alpha on track 0 if
  /// the skeleton pose is from the last frame render.
  double get alpha {
    final result = SpineBindings.bindings.spine_track_entry_get_alpha(_ptr);
    return result;
  }

  set alpha(double value) {
    SpineBindings.bindings.spine_track_entry_set_alpha(_ptr, value);
  }

  /// When the interpolated mix percentage is less than the event threshold,
  /// event timelines for the animation being mixed out will be applied.
  /// Defaults to 0, so event timelines are not applied for an animation being
  /// mixed out.
  double get eventThreshold {
    final result = SpineBindings.bindings.spine_track_entry_get_event_threshold(_ptr);
    return result;
  }

  set eventThreshold(double value) {
    SpineBindings.bindings.spine_track_entry_set_event_threshold(_ptr, value);
  }

  /// When the interpolated mix percentage is less than the attachment
  /// threshold, attachment timelines for the animation being mixed out will be
  /// applied. Defaults to 0, so attachment timelines are not applied for an
  /// animation being mixed out.
  double get mixAttachmentThreshold {
    final result = SpineBindings.bindings.spine_track_entry_get_mix_attachment_threshold(_ptr);
    return result;
  }

  set mixAttachmentThreshold(double value) {
    SpineBindings.bindings.spine_track_entry_set_mix_attachment_threshold(_ptr, value);
  }

  /// When the computed alpha is greater than alphaAttachmentThreshold,
  /// attachment timelines are applied. The computed alpha includes alpha and
  /// the interpolated mix percentage. Defaults to 0, so attachment timelines
  /// are always applied.
  double get alphaAttachmentThreshold {
    final result = SpineBindings.bindings.spine_track_entry_get_alpha_attachment_threshold(_ptr);
    return result;
  }

  set alphaAttachmentThreshold(double value) {
    SpineBindings.bindings.spine_track_entry_set_alpha_attachment_threshold(_ptr, value);
  }

  /// When the interpolated mix percentage is less than the draw order
  /// threshold, draw order timelines for the animation being mixed out will be
  /// applied. Defaults to 0, so draw order timelines are not applied for an
  /// animation being mixed out.
  double get mixDrawOrderThreshold {
    final result = SpineBindings.bindings.spine_track_entry_get_mix_draw_order_threshold(_ptr);
    return result;
  }

  set mixDrawOrderThreshold(double value) {
    SpineBindings.bindings.spine_track_entry_set_mix_draw_order_threshold(_ptr, value);
  }

  /// The animation queued to start after this animation, or NULL.
  TrackEntry? get next {
    final result = SpineBindings.bindings.spine_track_entry_get_next(_ptr);
    return result.address == 0 ? null : TrackEntry.fromPointer(result);
  }

  /// Returns true if at least one loop has been completed.
  bool get isComplete {
    final result = SpineBindings.bindings.spine_track_entry_is_complete(_ptr);
    return result;
  }

  /// Seconds from 0 to the mix duration when mixing from the previous animation
  /// to this animation. May be slightly more than mixDuration when the mix is
  /// complete.
  double get mixTime {
    final result = SpineBindings.bindings.spine_track_entry_get_mix_time(_ptr);
    return result;
  }

  set mixTime(double value) {
    SpineBindings.bindings.spine_track_entry_set_mix_time(_ptr, value);
  }

  /// Seconds for mixing from the previous animation to this animation. Defaults
  /// to the value provided by AnimationStateData based on the animation before
  /// this animation (if any).
  ///
  /// The mix duration can be set manually rather than use the value from
  /// AnimationStateData.GetMix. In that case, the mixDuration must be set
  /// before AnimationState.update(float) is next called.
  ///
  /// When using AnimationState::addAnimation(int, Animation, bool, float) with
  /// a delay less than or equal to 0, note the Delay is set using the mix
  /// duration from the AnimationStateData
  double get mixDuration {
    final result = SpineBindings.bindings.spine_track_entry_get_mix_duration(_ptr);
    return result;
  }

  /// The interpolation to apply to the mix percentage (mix time / mix duration)
  /// when mixing from the previous animation to this animation. Defaults to
  /// linear.
  Interpolation get mixInterpolation {
    final result = SpineBindings.bindings.spine_track_entry_get_mix_interpolation(_ptr);
    return Interpolation.fromPointer(result);
  }

  set mixInterpolation(Interpolation value) {
    SpineBindings.bindings.spine_track_entry_set_mix_interpolation(_ptr, value.nativePtr.cast());
  }

  /// The track entry for the previous animation when mixing to this animation,
  /// or NULL if no mixing is currently occurring. When mixing from multiple
  /// animations, MixingFrom makes up a doubly linked list with MixingTo.
  TrackEntry? get mixingFrom {
    final result = SpineBindings.bindings.spine_track_entry_get_mixing_from(_ptr);
    return result.address == 0 ? null : TrackEntry.fromPointer(result);
  }

  /// The track entry for the next animation when mixing from this animation, or
  /// NULL if no mixing is currently occurring. When mixing to multiple
  /// animations, MixingTo makes up a doubly linked list with MixingFrom.
  TrackEntry? get mixingTo {
    final result = SpineBindings.bindings.spine_track_entry_get_mixing_to(_ptr);
    return result.address == 0 ? null : TrackEntry.fromPointer(result);
  }

  /// Resets the rotation directions for mixing this entry's rotate timelines.
  /// This can be useful to avoid bones rotating the long way around when using
  /// alpha and starting animations on other tracks.
  ///
  /// Mixing involves finding a rotation between two others, which has two
  /// possible solutions: the short way or the long way around. The two
  /// rotations likely change over time, so which direction is the short or long
  /// way also changes. If the short way was always chosen, bones would flip to
  /// the other side when that direction became the long way. TrackEntry chooses
  /// the short way the first time it is applied and remembers that direction.
  void resetRotationDirections() {
    SpineBindings.bindings.spine_track_entry_reset_rotation_directions(_ptr);
  }

  double get trackComplete {
    final result = SpineBindings.bindings.spine_track_entry_get_track_complete(_ptr);
    return result;
  }

  /// Returns true if this entry is for the empty animation.
  bool get isEmptyAnimation {
    final result = SpineBindings.bindings.spine_track_entry_is_empty_animation(_ptr);
    return result;
  }

  /// Returns true if this track entry has been applied at least once.
  ///
  /// See AnimationState::apply(Skeleton).
  bool get wasApplied {
    final result = SpineBindings.bindings.spine_track_entry_was_applied(_ptr);
    return result;
  }

  /// Returns true if there is a next track entry that is ready to become the
  /// current track entry during the next AnimationState::update(float)}
  bool get isNextReady {
    final result = SpineBindings.bindings.spine_track_entry_is_next_ready(_ptr);
    return result;
  }

  /// The AnimationState this track entry belongs to. May be NULL if TrackEntry
  /// is directly instantiated.
  AnimationState? get animationState {
    final result = SpineBindings.bindings.spine_track_entry_get_animation_state(_ptr);
    return result.address == 0 ? null : AnimationState.fromPointer(result);
  }

  set animationState(AnimationState? value) {
    SpineBindings.bindings
        .spine_track_entry_set_animation_state(_ptr, value?.nativePtr.cast() ?? Pointer.fromAddress(0));
  }

  Pointer<Void>? get rendererObject {
    final result = SpineBindings.bindings.spine_track_entry_get_renderer_object(_ptr);
    return result;
  }

  set setMixDuration(double value) {
    SpineBindings.bindings.spine_track_entry_set_mix_duration_1(_ptr, value);
  }

  /// Sets both mixDuration and delay.
  ///
  /// [delay] If > 0, sets delay. If < = 0, the delay set is the duration of the previous track entry minus the specified mix duration plus the specified delay (ie the mix ends at (delay = 0) or before (delay < 0) the previous track entry duration). If the previous entry is looping, its next loop completion is used instead of its duration.
  void setMixDuration2(double mixDuration, double delay) {
    SpineBindings.bindings.spine_track_entry_set_mix_duration_2(_ptr, mixDuration, delay);
  }
}
