/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*****************************************************************************/

package spine.animation;

import spine.animation.Listeners.EventListeners;
import spine.Poolable;

/** Stores settings and other state for the playback of an animation on an spine.animation.AnimationState track.
 *
 * References to a track entry must not be kept after the AnimationStateListener.dispose(TrackEntry) event occurs. */
class TrackEntry implements Poolable {
	/** The animation to apply for this track entry. */
	public var animation:Animation;

	/** The animation queued to start after this animation, or null if there is none. next makes up a doubly linked
	 * list.
	 *
	 * See spine.animation.AnimationState.clearNext(TrackEntry) to truncate the list. */
	public var next:TrackEntry;

	/** The animation queued to play before this animation, or null. previous makes up a doubly linked list. */
	public var previous:TrackEntry;

	/** The track entry for the previous animation when mixing from the previous animation to this animation, or null if no
	 * mixing is currently occurring. When mixing from multiple animations, mixingFrom makes up a linked list. */
	public var mixingFrom:TrackEntry;

	/** The track entry for the next animation when mixing from this animation to the next animation, or null if no mixing is
	 * currently occurring. When mixing to multiple animations, mixingTo makes up a linked list. */
	public var mixingTo:TrackEntry;

	public var onStart:Listeners = new Listeners();
	public var onInterrupt:Listeners = new Listeners();
	public var onEnd:Listeners = new Listeners();
	public var onDispose:Listeners = new Listeners();
	public var onComplete:Listeners = new Listeners();
	public var onEvent:EventListeners = new EventListeners();

	/** The index of the track where this track entry is either current or queued.
	 *
	 * See spine.animation.AnimationState.getCurrent(int). */
	public var trackIndex:Int = 0;

	/** If true, the animation will repeat. If false it will not, instead its last frame is applied if played beyond its
	 * duration. */
	public var loop:Bool = false;

	/** If true, the animation will be applied in reverse. Events are not fired when an animation is applied in reverse. */
	public var reverse:Bool = false;

	/** When true, timelines in this animation that support additive have their values added to the setup or current pose values
	 * rather than replacing them. */
	public var additive:Bool = false;

	public var keepHold:Bool = false;

	/** When the mix percentage (TrackEntry.getMixTime() / TrackEntry.getMixDuration()) is less than the
	 * eventThreshold, event timelines are applied while this animation is being mixed out. Defaults to 0, so event
	 * timelines are not applied while this animation is being mixed out. */
	public var eventThreshold:Float = 0;

	/** When the mix percentage (TrackEntry.getMixTime() / TrackEntry.getMixDuration()) is less than the
	 * mixAttachmentThreshold, attachment timelines are applied while this animation is being mixed out. Defaults
	 * to 0, so attachment timelines are not applied while this animation is being mixed out. */
	public var mixAttachmentThreshold:Float = 0;

	/** When TrackEntry.getAlpha() is greater than alphaAttachmentThreshold, attachment timelines are applied.
	 * Defaults to 0, so attachment timelines are always applied. */
	public var alphaAttachmentThreshold:Float = 0;

	/** When the mix percentage (TrackEntry.getMixTime() / TrackEntry.getMixDuration()) is less than the
	 * mixDrawOrderThreshold, draw order timelines are applied while this animation is being mixed out. Defaults to
	 * 0, so draw order timelines are not applied while this animation is being mixed out. */
	public var mixDrawOrderThreshold:Float = 0;

	/** Seconds when this animation starts, both initially and after looping. Defaults to 0.
	 *
	 * When changing the animationStart time, it often makes sense to set TrackEntry.getAnimationLast() to the same
	 * value to prevent timeline keys before the start time from triggering. */
	public var animationStart:Float = 0;

	/** Seconds for the last frame of this animation. Non-looping animations won't play past this time. Looping animations will
	 * loop back to TrackEntry.getAnimationStart() at this time. Defaults to the animation spine.animation.Animation.duration. */
	public var animationEnd:Float = 0;

	/** The time in seconds this animation was last applied. Some timelines use this for one-time triggers. Eg, when this
	 * animation is applied, event timelines will fire all events between the animationLast time (exclusive) and
	 * animationTime (inclusive). Defaults to -1 to ensure triggers on frame 0 happen the first time this animation
	 * is applied. */
	public var animationLast:Float = 0;

	public var nextAnimationLast:Float = 0;

	/** Seconds to postpone playing the animation. Must be >= 0. When this track entry is the current track entry,
	 * delay postpones incrementing the TrackEntry.getTrackTime(). When this track entry is queued,
	 * delay is the time from the start of the previous animation to when this track entry will become the current
	 * track entry (ie when the previous track entry TrackEntry.getTrackTime() >= this track entry's
	 * delay).
	 *
	 * TrackEntry.getTimeScale() affects the delay.
	 *
	 * When passing delay <= 0 to spine.animation.AnimationState.addAnimation(int, Animation, boolean, float) this
	 * delay is set using a mix duration from spine.animation.AnimationStateData. To change the TrackEntry.getMixDuration()
	 * afterward, use TrackEntry.setMixDuration(float, float) so this delay is adjusted. */
	public var delay(default, set):Float = 0;

	/** Current time in seconds this track entry has been the current track entry. The track time determines
	 * TrackEntry.getAnimationTime(). The track time can be set to start the animation at a time other than 0, without affecting
	 * looping. */
	public var trackTime:Float = 0;

	public var trackLast:Float = 0;
	public var nextTrackLast:Float = 0;

	/** The track time in seconds when this animation will be removed from the track. Defaults to the highest possible float
	 * value, meaning the animation will be applied until a new animation is set or the track is cleared. If the track end time
	 * is reached, no other animations are queued for playback, and mixing from any previous animations is complete, then the
	 * properties keyed by the animation are set to the setup pose and the track is cleared.
	 *
	 * It may be desired to use spine.animation.AnimationState.addEmptyAnimation(int, float, float) rather than have the animation
	 * abruptly cease being applied. */
	public var trackEnd:Float = 0;

	/** Multiplier for the delta time when this track entry is updated, causing time for this animation to pass slower or
	 * faster. Defaults to 1.
	 *
	 * Values < 0 are not supported. To play an animation in reverse, use TrackEntry.getReverse().
	 *
	 * TrackEntry.getMixTime() is not affected by track entry time scale, so TrackEntry.getMixDuration() may need to be adjusted to
	 * match the animation speed.
	 *
	 * When using spine.animation.AnimationState.addAnimation(int, Animation, boolean, float) with a delay <= 0, the
	 * TrackEntry.getDelay() is set using the mix duration from the spine.animation.AnimationStateData, assuming time scale to be 1. If
	 * the time scale is not 1, the delay may need to be adjusted.
	 *
	 * See AnimationState spine.animation.AnimationState.getTimeScale() for affecting all animations. */
	public var timeScale:Float = 0;

	/** Values < 1 mix this animation with the skeleton's current pose (usually the pose resulting from lower tracks). Defaults
	 * to 1, which overwrites the skeleton's current pose with this animation.
	 *
	 * Typically track 0 is used to completely pose the skeleton, then alpha is used on higher tracks. It doesn't make sense to
	 * use alpha on track 0 if the skeleton pose is from the last frame render. */
	public var alpha:Float = 0;

	/** Seconds from 0 to the TrackEntry.getMixDuration() when mixing from the previous animation to this animation. May be
	 * slightly more than mixDuration when the mix is complete. */
	public var mixTime:Float = 0;

	/** Seconds for mixing from the previous animation to this animation. Defaults to the value provided by AnimationStateData
	 * spine.animation.AnimationStateData.getMix(Animation, Animation) based on the animation before this animation (if any).
	 *
	 * A mix duration of 0 still mixes out over one frame to provide the track entry being mixed out a chance to revert the
	 * properties it was animating. A mix duration of 0 can be set at any time to end the mix on the next
	 * spine.animation.AnimationState.update(float) update.
	 *
	 * The mixDuration can be set manually rather than use the value from
	 * spine.animation.AnimationStateData.getMix(Animation, Animation). In that case, the mixDuration can be set for a new
	 * track entry only before spine.animation.AnimationState.update(float) is first called.
	 *
	 * When using spine.animation.AnimationState.addAnimation(int, Animation, boolean, float) with a delay <= 0, the
	 * TrackEntry.getDelay() is set using the mix duration from the spine.animation.AnimationStateData. If mixDuration is set
	 * afterward, the delay may need to be adjusted. For example:
	 * entry.delay = entry.previous.getTrackComplete() - entry.mixDuration;
	 * Alternatively, TrackEntry.setMixDuration(float, float) can be used to recompute the delay:
	 * entry.setMixDuration(0.25f, 0); */
	public var mixDuration:Float = 0;

	public var totalAlpha:Float = 0;

	public var timelineMode:Array<Int> = new Array<Int>();
	public var timelineHoldMix:Array<TrackEntry> = new Array<TrackEntry>();
	public var timelinesRotation:Array<Float> = new Array<Float>();

	/** If true, mixing rotation between tracks always uses the shortest rotation direction. If the rotation is animated, the
	 * shortest rotation direction may change during the mix.
	 *
	 * If false, the shortest rotation direction is remembered when the mix starts and the same direction is used for the rest
	 * of the mix. Defaults to false. */
	public var shortestRotation = false;

	function set_delay(delay:Float):Float {
		if (delay < 0)
			throw new SpineException("delay must be >= 0.");
		return this.delay = delay;
	}

	public function new() {}

	/** Uses TrackEntry.getTrackTime() to compute the animationTime. When the trackTime is 0, the
	 * animationTime is equal to the animationStart time.
	 *
	 * The animationTime is between TrackEntry.getAnimationStart() and TrackEntry.getAnimationEnd(), except if this
	 * track entry is non-looping and TrackEntry.getAnimationEnd() is >= to the animation spine.animation.Animation.duration, then
	 * animationTime continues to increase past TrackEntry.getAnimationEnd(). */
	public function getAnimationTime():Float {
		if (loop) {
			var duration:Float = animationEnd - animationStart;
			if (duration == 0)
				return animationStart;
			return (trackTime % duration) + animationStart;
		}
		return Math.min(trackTime + animationStart, animationEnd);
	}

	/** If this track entry is non-looping, the track time in seconds when TrackEntry.getAnimationEnd() is reached, or the current
	 * TrackEntry.getTrackTime() if it has already been reached. If this track entry is looping, the track time when this
	 * animation will reach its next TrackEntry.getAnimationEnd() (the next loop completion). */
	public function getTrackComplete():Float {
		var duration:Float = animationEnd - animationStart;
		if (duration != 0) {
			if (loop)
				return duration * (1 + Std.int(trackTime / duration)); // Completion of next loop.
			if (trackTime < duration)
				return duration; // Before duration.
		}
		return trackTime; // Next update.
	}

	/** Returns true if this track entry has been applied at least once.
	 *
	 * See spine.animation.AnimationState.apply(Skeleton). */
	public function wasApplied() {
		return nextTrackLast != -1;
	}

	/** Returns true if there is a TrackEntry.getNext() track entry and it will become the current track entry during the next
	 * spine.animation.AnimationState.update(float). */
	public function isNextReady():Bool {
		return next != null && nextTrackLast - next.delay >= 0;
	}

	public function reset():Void {
		next = null;
		previous = null;
		mixingFrom = null;
		mixingTo = null;
		animation = null;
		onStart.listeners.resize(0);
		onInterrupt.listeners.resize(0);
		onEnd.listeners.resize(0);
		onDispose.listeners.resize(0);
		onComplete.listeners.resize(0);
		onEvent.listeners.resize(0);
		timelineMode.resize(0);
		timelineHoldMix.resize(0);
		timelinesRotation.resize(0);
	}

	/** Resets the rotation directions for mixing this entry's rotate timelines. This can be useful to avoid bones rotating the
	 * long way around when using TrackEntry.getAlpha() and starting animations on other tracks.
	 *
	 * Mixing involves finding a rotation between two others, which has two possible solutions:
	 * the short way or the long way around. The two rotations likely change over time, so which direction is the short or long
	 * way also changes. If the short way was always chosen, bones would flip to the other side when that direction became the
	 * long way. TrackEntry chooses the short way the first time it is applied and remembers that direction. */
	public function resetRotationDirection():Void {
		timelinesRotation.resize(0);
	}

	/** Sets both TrackEntry.getMixDuration() and TrackEntry.getDelay().
	 * @param mixDuration If > 0, sets TrackEntry.getDelay(). If <= 0, the delay set is the duration of the previous track
	 *           entry minus the specified mix duration plus the specified delay (ie the mix ends at
	 *           (delay = 0) or before (delay < 0) the previous track entry duration). If the previous
	 *           entry is looping, its next loop completion is used instead of its duration. */
	public function setMixDurationWithDelay(mixDuration:Float):Float {
		this.mixDuration = mixDuration;
		if (delay <= 0) {
			if (this.previous != null)
				delay = Math.max(delay + this.previous.getTrackComplete() - mixDuration, 0);
			else
				delay = 0;
		}
		return mixDuration;
	}
}
