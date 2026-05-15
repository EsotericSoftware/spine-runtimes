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

import Foundation
import SpineC

/// State for the playback of an animation
@objc(SpineTrackEntry)
@objcMembers
public class TrackEntry: NSObject {
    public let _ptr: UnsafeMutableRawPointer

    public init(fromPointer ptr: spine_track_entry) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        super.init()
    }

    public override convenience init() {
        let ptr = spine_track_entry_create()
        self.init(fromPointer: ptr!)
    }

    /// The index of the track where this entry is either current or queued.
    public var trackIndex: Int32 {
        let result = spine_track_entry_get_track_index(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result
    }

    /// The animation to apply for this track entry.
    public var animation: Animation {
        get {
            let result = spine_track_entry_get_animation(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return Animation(fromPointer: result!)
        }
        set {
            spine_track_entry_set_animation(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self), newValue._ptr.assumingMemoryBound(to: spine_animation_wrapper.self))
        }
    }

    public var previous: TrackEntry? {
        let result = spine_track_entry_get_previous(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result.map { TrackEntry(fromPointer: $0) }
    }

    /// If true, the animation will repeat. If false, it will not, instead its last frame is applied
    /// if played beyond its duration.
    public var loop: Bool {
        get {
            let result = spine_track_entry_get_loop(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result
        }
        set {
            spine_track_entry_set_loop(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self), newValue)
        }
    }

    /// When true, timelines in this animation that support additive have their values added to the
    /// setup or current pose values instead of replacing them. Additive can be set for a new track
    /// entry only before AnimationState::apply() is next called.
    public var additive: Bool {
        get {
            let result = spine_track_entry_get_additive(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result
        }
        set {
            spine_track_entry_set_additive(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self), newValue)
        }
    }

    public var reverse: Bool {
        get {
            let result = spine_track_entry_get_reverse(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result
        }
        set {
            spine_track_entry_set_reverse(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self), newValue)
        }
    }

    public var shortestRotation: Bool {
        get {
            let result = spine_track_entry_get_shortest_rotation(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result
        }
        set {
            spine_track_entry_set_shortest_rotation(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self), newValue)
        }
    }

    /// Seconds to postpone playing the animation. Must be >= 0. When this track entry is the
    /// current track entry, delay postpones incrementing the track time. When this track entry is
    /// queued, delay is the time from the start of the previous animation to when this track entry
    /// will become the current track entry (ie when the previous track entry's track time >= this
    /// track entry's delay).
    ///
    /// Time scale affects the delay.
///
    /// When passing delay < = 0 to AnimationState::addAnimation(int, Animation, bool, float), this
    /// delay is set using a mix duration from AnimationStateData. To change the mix duration
    /// afterward, use setMixDuration(float, float) so this delay is adjusted.
    public var delay: Float {
        get {
            let result = spine_track_entry_get_delay(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result
        }
        set {
            spine_track_entry_set_delay(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self), newValue)
        }
    }

    /// The time in seconds this track entry has been the current track entry, starting at 0 and
    /// increasing forever. Compare to getAnimationTime(), which is always between animationStart
    /// and animationEnd.
    ///
    /// The track time can be set to start the animation at a time other than 0, without affecting
    /// looping. When doing so, animationLast can be set to the same value to avoid firing events
    /// from the start of the animation.
    public var trackTime: Float {
        get {
            let result = spine_track_entry_get_track_time(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result
        }
        set {
            spine_track_entry_set_track_time(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self), newValue)
        }
    }

    /// The track time in seconds when this animation will be removed from the track. Defaults to
    /// the highest possible float value, meaning the animation will be applied until a new
    /// animation is set or the track is cleared. If the track end time is reached, no other
    /// animations are queued for playback, and mixing from any previous animations is complete,
    /// then the properties keyed by the animation are set to the setup pose and the track is
    /// cleared.
    ///
    /// It may be desired to use AnimationState::addEmptyAnimation(int, float, float) rather than
    /// have the animation abruptly cease being applied.
    public var trackEnd: Float {
        get {
            let result = spine_track_entry_get_track_end(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result
        }
        set {
            spine_track_entry_set_track_end(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self), newValue)
        }
    }

    /// The time in seconds for the first frame of this animation, both initially and after looping.
    /// Defaults to 0.
    ///
    /// When setting the animation start time, animationLast can be set to the same value to avoid
    /// firing events from the start of the animation.
    public var animationStart: Float {
        get {
            let result = spine_track_entry_get_animation_start(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result
        }
        set {
            spine_track_entry_set_animation_start(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self), newValue)
        }
    }

    /// The time in seconds for the last frame of this animation. Past this time, non-looping
    /// animations hold the pose at this time while looping animations will loop back to
    /// animationStart. Defaults to the animation duration.
    public var animationEnd: Float {
        get {
            let result = spine_track_entry_get_animation_end(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result
        }
        set {
            spine_track_entry_set_animation_end(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self), newValue)
        }
    }

    /// The time in seconds this animation was last applied. Some timelines use this for one-time
    /// triggers. Eg, when this animation is applied, event timelines will fire all events between
    /// the animation last time (exclusive) and animation time (inclusive). Defaults to -1 to ensure
    /// triggers on frame 0 happen the first time this animation is applied.
    public var animationLast: Float {
        get {
            let result = spine_track_entry_get_animation_last(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result
        }
        set {
            spine_track_entry_set_animation_last(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self), newValue)
        }
    }

    /// Uses the track time to compute animationTime, which is always between animationStart and
    /// animationEnd. When trackTime is 0, animationTime is equal to animationStart.
    public var animationTime: Float {
        let result = spine_track_entry_get_animation_time(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result
    }

    /// Multiplier for the delta time when this track entry is updated, causing time for this
    /// animation to pass slower or faster. Defaults to 1.
    ///
    /// Values < 0 are not supported. To play an animation in reverse, use reverse.
///
    /// mixTime is not affected by track entry time scale, so mixDuration may need to be adjusted to
    /// match the animation speed.
///
    /// When using AnimationState::addAnimation(int, Animation, bool, float) with a delay < = 0,
    /// delay is set using the mix duration from AnimationStateData, assuming time scale to be 1. If
    /// the time scale is not 1, the delay may need to be adjusted.
///
    /// See AnimationState::getTimeScale() for affecting all animations.
    public var timeScale: Float {
        get {
            let result = spine_track_entry_get_time_scale(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result
        }
        set {
            spine_track_entry_set_time_scale(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self), newValue)
        }
    }

    /// Values less than 1 mix this animation with the last skeleton pose. Defaults to 1, which
    /// overwrites the last skeleton pose with this animation.
    ///
    /// Typically track 0 is used to completely pose the skeleton, then alpha can be used on higher
    /// tracks. It doesn't make sense to use alpha on track 0 if the skeleton pose is from the last
    /// frame render.
    public var alpha: Float {
        get {
            let result = spine_track_entry_get_alpha(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result
        }
        set {
            spine_track_entry_set_alpha(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self), newValue)
        }
    }

    /// When the interpolated mix percentage is less than the event threshold, event timelines for
    /// the animation being mixed out will be applied. Defaults to 0, so event timelines are not
    /// applied for an animation being mixed out.
    public var eventThreshold: Float {
        get {
            let result = spine_track_entry_get_event_threshold(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result
        }
        set {
            spine_track_entry_set_event_threshold(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self), newValue)
        }
    }

    /// When the interpolated mix percentage is less than the attachment threshold, attachment
    /// timelines for the animation being mixed out will be applied. Defaults to 0, so attachment
    /// timelines are not applied for an animation being mixed out.
    public var mixAttachmentThreshold: Float {
        get {
            let result = spine_track_entry_get_mix_attachment_threshold(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result
        }
        set {
            spine_track_entry_set_mix_attachment_threshold(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self), newValue)
        }
    }

    /// When the computed alpha is greater than alphaAttachmentThreshold, attachment timelines are
    /// applied. The computed alpha includes alpha and the interpolated mix percentage. Defaults to
    /// 0, so attachment timelines are always applied.
    public var alphaAttachmentThreshold: Float {
        get {
            let result = spine_track_entry_get_alpha_attachment_threshold(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result
        }
        set {
            spine_track_entry_set_alpha_attachment_threshold(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self), newValue)
        }
    }

    /// When the interpolated mix percentage is less than the draw order threshold, draw order
    /// timelines for the animation being mixed out will be applied. Defaults to 0, so draw order
    /// timelines are not applied for an animation being mixed out.
    public var mixDrawOrderThreshold: Float {
        get {
            let result = spine_track_entry_get_mix_draw_order_threshold(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result
        }
        set {
            spine_track_entry_set_mix_draw_order_threshold(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self), newValue)
        }
    }

    /// The animation queued to start after this animation, or NULL.
    public var next: TrackEntry? {
        let result = spine_track_entry_get_next(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result.map { TrackEntry(fromPointer: $0) }
    }

    /// Returns true if at least one loop has been completed.
    public var isComplete: Bool {
        let result = spine_track_entry_is_complete(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result
    }

    /// Seconds from 0 to the mix duration when mixing from the previous animation to this
    /// animation. May be slightly more than mixDuration when the mix is complete.
    public var mixTime: Float {
        get {
            let result = spine_track_entry_get_mix_time(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result
        }
        set {
            spine_track_entry_set_mix_time(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self), newValue)
        }
    }

    /// Seconds for mixing from the previous animation to this animation. Defaults to the value
    /// provided by AnimationStateData based on the animation before this animation (if any).
    ///
    /// The mix duration can be set manually rather than use the value from
    /// AnimationStateData.GetMix. In that case, the mixDuration must be set before
    /// AnimationState.update(float) is next called.
///
    /// When using AnimationState::addAnimation(int, Animation, bool, float) with a delay less than
    /// or equal to 0, note the Delay is set using the mix duration from the AnimationStateData
    public var mixDuration: Float {
        let result = spine_track_entry_get_mix_duration(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result
    }

    /// The interpolation to apply to the mix percentage (mix time / mix duration) when mixing from
    /// the previous animation to this animation. Defaults to linear.
    public var mixInterpolation: Interpolation {
        get {
            let result = spine_track_entry_get_mix_interpolation(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return Interpolation(fromPointer: result!)
        }
        set {
            spine_track_entry_set_mix_interpolation(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self), newValue._ptr.assumingMemoryBound(to: spine_interpolation_wrapper.self))
        }
    }

    /// The track entry for the previous animation when mixing to this animation, or NULL if no
    /// mixing is currently occurring. When mixing from multiple animations, MixingFrom makes up a
    /// doubly linked list with MixingTo.
    public var mixingFrom: TrackEntry? {
        let result = spine_track_entry_get_mixing_from(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result.map { TrackEntry(fromPointer: $0) }
    }

    /// The track entry for the next animation when mixing from this animation, or NULL if no mixing
    /// is currently occurring. When mixing to multiple animations, MixingTo makes up a doubly
    /// linked list with MixingFrom.
    public var mixingTo: TrackEntry? {
        let result = spine_track_entry_get_mixing_to(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result.map { TrackEntry(fromPointer: $0) }
    }

    public var trackComplete: Float {
        let result = spine_track_entry_get_track_complete(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result
    }

    /// Returns true if this entry is for the empty animation.
    public var isEmptyAnimation: Bool {
        let result = spine_track_entry_is_empty_animation(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result
    }

    /// Returns true if this track entry has been applied at least once.
    ///
    /// See AnimationState::apply(Skeleton).
    public var wasApplied: Bool {
        let result = spine_track_entry_was_applied(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result
    }

    /// Returns true if there is a next track entry that is ready to become the current track entry
    /// during the next AnimationState::update(float)}
    public var isNextReady: Bool {
        let result = spine_track_entry_is_next_ready(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result
    }

    /// The AnimationState this track entry belongs to. May be NULL if TrackEntry is directly
    /// instantiated.
    public var animationState: AnimationState? {
        get {
            let result = spine_track_entry_get_animation_state(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result.map { AnimationState(fromPointer: $0) }
        }
        set {
            spine_track_entry_set_animation_state(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self), newValue?._ptr.assumingMemoryBound(to: spine_animation_state_wrapper.self))
        }
    }

    public var rendererObject: UnsafeMutableRawPointer? {
        let result = spine_track_entry_get_renderer_object(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        return result
    }

    public var setMixDuration: Float {
        get { fatalError("Setter-only property") }
        set(newValue) {
            spine_track_entry_set_mix_duration_1(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self), newValue)
        }
    }

    /// Resets the rotation directions for mixing this entry's rotate timelines. This can be useful
    /// to avoid bones rotating the long way around when using alpha and starting animations on
    /// other tracks.
    ///
    /// Mixing involves finding a rotation between two others, which has two possible solutions: the
    /// short way or the long way around. The two rotations likely change over time, so which
    /// direction is the short or long way also changes. If the short way was always chosen, bones
    /// would flip to the other side when that direction became the long way. TrackEntry chooses the
    /// short way the first time it is applied and remembers that direction.
    public func resetRotationDirections() {
        spine_track_entry_reset_rotation_directions(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
    }

    /// Sets both mixDuration and delay.
    ///
    /// - Parameter delay: If > 0, sets delay. If < = 0, the delay set is the duration of the previous track entry minus the specified mix duration plus the specified delay (ie the mix ends at (delay = 0) or before (delay < 0) the previous track entry duration). If the previous entry is looping, its next loop completion is used instead of its duration.
    public func setMixDuration2(_ mixDuration: Float, _ delay: Float) {
        spine_track_entry_set_mix_duration_2(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self), mixDuration, delay)
    }

    public func dispose() {
        spine_track_entry_dispose(_ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
    }
}