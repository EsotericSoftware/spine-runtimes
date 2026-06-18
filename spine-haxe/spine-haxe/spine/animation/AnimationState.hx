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

import haxe.ds.StringMap;
import spine.animation.EventTimeline;
import spine.animation.Listeners.EventListeners;
import spine.Event;
import spine.Interpolation;
import spine.Pool;
import spine.Skeleton;

/**
 * Applies animations over time, queues animations for later playback, mixes (crossfading) between animations, and applies
 * multiple animations on top of each other (layering).
 *
 * @see https://esotericsoftware.com/spine-applying-animations/ Applying Animations in the Spine Runtimes Guide
 */
class AnimationState {
	public static inline var CURRENT:Int = 0;
	public static inline var SETUP:Int = 1;
	public static inline var FIRST:Int = 2;
	public static inline var MODE:Int = 3;
	public static inline var HOLD:Int = 4;

	public static inline var ATTACH_SETUP:Int = 1;
	public static inline var ATTACH_RETAIN:Int = 2;

	private static var emptyAnimation:Animation = new Animation("<empty>", new Array<Timeline>(), 0);

	public var data:AnimationStateData;
	public var tracks:Array<TrackEntry> = new Array<TrackEntry>();

	private var events:Array<Event> = new Array<Event>();

	public var onStart:Listeners = new Listeners();
	public var onInterrupt:Listeners = new Listeners();
	public var onEnd:Listeners = new Listeners();
	public var onDispose:Listeners = new Listeners();
	public var onComplete:Listeners = new Listeners();
	public var onEvent:EventListeners = new EventListeners();

	private var queue:EventQueue;
	private var propertyIDs:StringMap<TrackEntry> = new StringMap<TrackEntry>();

	public var animationsChanged:Bool = false;
	public var timeScale:Float = 1;
	public var trackEntryPool:Pool<TrackEntry>;

	private var unkeyedState:Int = 0;

	/**
	 * Creates an uninitialized AnimationState. The animation state data must be set before use.
	 */
	public function new(data:AnimationStateData) {
		if (data == null)
			throw new SpineException("data can not be null");
		this.data = data;
		this.queue = new EventQueue(this);
		this.trackEntryPool = new Pool(function():Dynamic {
			return new TrackEntry();
		});
	}

	/**
	 * Increments each track entry spine.animation.TrackEntry.getTrackTime(), setting queued animations as current if needed.
	 */
	public function update(delta:Float):Void {
		delta *= timeScale;
		for (i in 0...tracks.length) {
			var current:TrackEntry = tracks[i];
			if (current == null)
				continue;

			current.animationLast = current.nextAnimationLast;
			current.trackLast = current.nextTrackLast;

			var currentDelta:Float = delta * current.timeScale;

			if (current.delay > 0) {
				current.delay -= currentDelta;
				if (current.delay > 0)
					continue;
				currentDelta = -current.delay;
				current.delay = 0;
			}

			var next:TrackEntry = current.next;
			if (next != null) {
				// When the next entry's delay is passed, change to the next entry, preserving leftover time.
				var nextTime:Float = current.trackLast - next.delay;
				if (nextTime >= 0) {
					next.delay = 0;
					next.trackTime += current.timeScale == 0 ? 0 : (nextTime / current.timeScale + delta) * next.timeScale;
					current.trackTime += currentDelta;
					setTrack(i, next, true);
					while (next.mixingFrom != null) {
						next.mixTime += delta;
						next = next.mixingFrom;
					}
					continue;
				}
			} else if (current.trackLast >= current.trackEnd && current.mixingFrom == null) {
				// Clear the track when there is no next entry, the track end time is reached, and there is no mixingFrom.
				tracks[i] = null;
				queue.end(current);
				clearNext(current);
				continue;
			}

			if (current.mixingFrom != null && updateMixingFrom(current, delta)) {
				// End mixing from entries once all have completed.
				var from:TrackEntry = current.mixingFrom;
				current.mixingFrom = null;
				if (from != null)
					from.mixingTo = null;
				while (from != null) {
					queue.end(from);
					from = from.mixingFrom;
				}
			}

			current.trackTime += currentDelta;
		}

		queue.drain();
	}

	/**
	 * Returns true when all mixing from entries are complete.
	 */
	private function updateMixingFrom(to:TrackEntry, delta:Float):Bool {
		var from:TrackEntry = to.mixingFrom;
		if (from == null)
			return true;

		var finished:Bool = updateMixingFrom(from, delta);

		from.animationLast = from.nextAnimationLast;
		from.trackLast = from.nextTrackLast;

		// The from entry was applied at least once and the mix is complete.
		if (to.nextTrackLast != -1 && to.mixTime >= to.mixDuration) {
			// Mixing is complete for all entries before the from entry or the mix is instantaneous.
			if (from.totalAlpha == 0 || to.mixDuration == 0) {
				to.mixingFrom = from.mixingFrom;
				if (from.mixingFrom != null)
					from.mixingFrom.mixingTo = to;
				if (from.totalAlpha == 0) {
					var next = to;
					while (next.mixingTo != null) {
						next.keepHold = true;
						next = next.mixingTo;
					}
				}
				queue.end(from);
			}
			return finished;
		}

		from.trackTime += delta * from.timeScale;
		to.mixTime += delta;
		return false;
	}

	/**
	 * Poses the skeleton using the track entry animations. The animation state is not changed, so can be applied to multiple
	 * skeletons to pose them identically.
	 * @return True if any animations were applied.
	 */
	public function apply(skeleton:Skeleton):Bool {
		if (skeleton == null)
			throw new SpineException("skeleton cannot be null.");
		if (animationsChanged)
			_animationsChanged();
		var applied:Bool = false;

		for (i in 0...tracks.length) {
			var current:TrackEntry = tracks[i];
			if (current == null || current.delay > 0)
				continue;
			applied = true;

			// Apply mixing from entries first.
			var alpha:Float = current.alpha;
			if (current.mixingFrom != null) {
				alpha *= applyMixingFrom(current, skeleton);
			} else if (current.trackTime >= current.trackEnd && current.next == null) {
				alpha = 0;
			}

			// Apply current entry.
			var animationLast:Float = current.animationLast,
				animationTime:Float = current.getAnimationTime(),
				applyTime:Float = animationTime;
			var applyEvents:Array<Event> = events;
			if (current.reverse) {
				applyTime = current.animation.duration - applyTime;
				applyEvents = null;
			}
			var timelines:Array<Timeline> = current.animation.timelines;
			var timelineCount:Int = timelines.length;
			if (i == 0 && alpha == 1) {
				for (timeline in timelines) {
					if (Std.isOfType(timeline, AttachmentTimeline))
						applyAttachmentTimeline(cast(timeline, AttachmentTimeline), skeleton, applyTime, MixFrom.setup, true);
					else
						timeline.apply(skeleton, animationLast, applyTime, applyEvents, alpha, MixFrom.setup, false, false, false);
				}
			} else {
				var timelineMode:Array<Int> = current.timelineMode;
				var retainAttachments:Bool = alpha >= current.alphaAttachmentThreshold;
				var add = current.additive,
					shortestRotation = add || current.shortestRotation;
				var firstFrame:Bool = !shortestRotation && current.timelinesRotation.length != timelineCount << 1;
				if (firstFrame)
					current.timelinesRotation.resize(timelineCount << 1);

				for (ii in 0...timelineCount) {
					var timeline:Timeline = timelines[ii];
					var from:MixFrom = timelineMode[ii] & MODE;
					if (!shortestRotation && Std.isOfType(timeline, RotateTimeline)) {
						applyRotateTimeline(cast(timeline, RotateTimeline), skeleton, applyTime, alpha, from, current.timelinesRotation, ii << 1, firstFrame);
					} else if (Std.isOfType(timeline, AttachmentTimeline)) {
						applyAttachmentTimeline(cast(timeline, AttachmentTimeline), skeleton, applyTime, from, retainAttachments);
					} else {
						timeline.apply(skeleton, animationLast, applyTime, applyEvents, alpha, from, add, false, false);
					}
				}
			}
			if (current.reverse)
				eventsReverse(current, animationLast, animationTime);
			queueEvents(current, animationTime);
			events.resize(0);
			current.nextAnimationLast = animationTime;
			current.nextTrackLast = current.trackTime;
		}

		// Set slot attachments to the setup pose if they were set temporarily to apply deform timelines.
		var setupState:Int = unkeyedState + ATTACH_SETUP;
		for (slot in skeleton.slots) {
			if (slot.attachmentState == setupState) {
				var attachmentName:String = slot.data.attachmentName;
				slot.pose.attachment = attachmentName == null ? null : skeleton.getAttachmentForSlotIndex(slot.data.index, attachmentName);
			}
		}
		unkeyedState += 2; // Reset.

		queue.drain();
		return applied;
	}

	private function applyMixingFrom(to:TrackEntry, skeleton:Skeleton):Float {
		var from:TrackEntry = to.mixingFrom;
		var fromMix:Float = from.mixingFrom != null ? applyMixingFrom(from, skeleton) : 1;
		var mix:Float = to.mix();

		var a = from.alpha * fromMix, keep = 1 - mix * to.alpha;
		var alphaMix = a * (1 - mix),
			alphaHold = keep > 0 ? alphaMix / keep : a;

		var timelines:Array<Timeline> = from.animation.timelines;
		var timelineCount:Int = timelines.length;
		var timelineMode:Array<Int> = from.timelineMode;
		var timelineHoldMix:Array<TrackEntry> = from.timelineHoldMix;

		var retainAttachments:Bool = mix < from.mixAttachmentThreshold,
			drawOrder:Bool = mix < from.mixDrawOrderThreshold;
		var add = from.additive,
			shortestRotation = add || from.shortestRotation;
		var firstFrame:Bool = !shortestRotation && from.timelinesRotation.length != timelineCount << 1;
		if (firstFrame)
			from.timelinesRotation.resize(timelineCount << 1);
		var timelinesRotation:Array<Float> = from.timelinesRotation;

		var animationLast:Float = from.animationLast,
			animationTime:Float = from.getAnimationTime(),
			applyTime:Float = animationTime;
		var applyEvents:Array<Event> = null;
		if (from.reverse)
			applyTime = from.animation.duration - applyTime;
		else if (mix < from.eventThreshold)
			applyEvents = events;

		from.totalAlpha = 0;

		for (i in 0...timelineCount) {
			var timeline:Timeline = timelines[i];
			var mode = timelineMode[i];
			var mixFrom:MixFrom = mode & MODE;
			var alpha:Float = 0;
			if ((mode & HOLD) != 0) {
				var holdMix:TrackEntry = timelineHoldMix[i];
				alpha = holdMix == null ? alphaHold : alphaHold * (1 - holdMix.mix());
			} else {
				if (!drawOrder && Std.isOfType(timeline, DrawOrderTimeline) && mixFrom == MixFrom.current)
					continue;
				alpha = alphaMix;
			}
			from.totalAlpha += alpha;
			if (!shortestRotation && Std.isOfType(timeline, RotateTimeline)) {
				applyRotateTimeline(cast(timeline, RotateTimeline), skeleton, applyTime, alpha, mixFrom, timelinesRotation, i << 1, firstFrame);
			} else if (Std.isOfType(timeline, AttachmentTimeline)) {
				applyAttachmentTimeline(cast(timeline, AttachmentTimeline), skeleton, applyTime,
					mixFrom, retainAttachments && alpha >= from.alphaAttachmentThreshold);
			} else {
				var out = !drawOrder || !Std.isOfType(timeline, DrawOrderTimeline) || mixFrom == MixFrom.current;
				timeline.apply(skeleton, animationLast, applyTime, applyEvents, alpha, mixFrom, add, out, false);
			}
		}

		if (from.reverse && mix < from.eventThreshold)
			eventsReverse(from, animationLast, animationTime);
		if (to.mixDuration > 0)
			queueEvents(from, animationTime);
		events.resize(0);
		from.nextAnimationLast = animationTime;
		from.nextTrackLast = from.trackTime;

		return mix;
	}

	/**
	 * Applies the attachment timeline and sets spine.Slot.attachmentState.
	 * @param retain True if the attachment remains after apply, false if temporary for deform timelines.
	 */
	public function applyAttachmentTimeline(timeline:AttachmentTimeline, skeleton:Skeleton, time:Float, from:MixFrom, retain:Bool) {
		var slot = skeleton.slots[timeline.slotIndex];
		if (!slot.bone.active)
			return;
		if (!retain && slot.attachmentState == this.unkeyedState + ATTACH_RETAIN)
			return;

		var setup:Bool = time < timeline.frames[0];
		var name:String = null;
		if (!setup) {
			name = timeline.attachmentNames[Timeline.search1(timeline.frames, time)];
			setup = !retain && name == null;
		}
		if (setup) {
			if (from == MixFrom.current)
				return;
			name = slot.data.attachmentName;
		}
		slot.pose.attachment = name == null ? null : skeleton.getAttachmentForSlotIndex(slot.data.index, name);
		if (retain)
			slot.attachmentState = this.unkeyedState + ATTACH_RETAIN;
		else if (!setup)
			slot.attachmentState = this.unkeyedState + ATTACH_SETUP;
	}

	/**
	 * Applies the rotate timeline, mixing with the current pose while keeping the same rotation direction chosen as the shortest
	 * the first time the mixing was applied.
	 */
	public function applyRotateTimeline(timeline:RotateTimeline, skeleton:Skeleton, time:Float, alpha:Float, from:MixFrom, timelinesRotation:Array<Float>,
			i:Int, firstFrame:Bool) {
		if (firstFrame)
			timelinesRotation[i] = 0;

		if (alpha == 1) {
			timeline.apply(skeleton, 0, time, null, 1, from, false, false, false);
			return;
		}

		var bone = skeleton.bones[timeline.boneIndex];
		if (!bone.active)
			return;
		var pose = bone.pose, setup = bone.data.setupPose;
		var frames = timeline.frames;
		var r1:Float, r2:Float;
		if (time < frames[0]) {
			if (from == MixFrom.setup) {
				pose.rotation = setup.rotation;
				return;
			}
			if (from == MixFrom.current)
				return;
			r1 = pose.rotation;
			r2 = setup.rotation;
		} else {
			r1 = from == MixFrom.setup ? setup.rotation : pose.rotation;
			r2 = setup.rotation + timeline.getCurveValue(time);
		}

		// Mix between rotations using the direction of the shortest route on the first frame while detecting crosses.
		var total:Float = 0, diff:Float = r2 - r1;
		diff -= Math.ceil(diff / 360 - 0.5) * 360;
		if (diff == 0) {
			total = timelinesRotation[i];
		} else {
			var lastTotal:Float = 0, lastDiff:Float = 0;
			if (firstFrame) {
				lastTotal = 0;
				lastDiff = diff;
			} else {
				lastTotal = timelinesRotation[i];
				lastDiff = timelinesRotation[i + 1];
			}
			var loops:Float = lastTotal - lastTotal % 360;
			total = diff + loops;
			var current = diff >= 0, dir = lastTotal >= 0;
			if (Math.abs(lastDiff) <= 90 && MathUtils.signum(lastDiff) != MathUtils.signum(diff)) {
				if (Math.abs(lastTotal - loops) > 180) {
					total += 360 * MathUtils.signum(lastTotal);
					dir = current;
				} else if (loops != 0)
					total -= 360 * MathUtils.signum(lastTotal);
				else
					dir = current;
			}
			if (dir != current)
				total += 360 * MathUtils.signum(lastTotal);
			timelinesRotation[i] = total;
		}
		timelinesRotation[i + 1] = diff;
		pose.rotation = r1 + total * alpha;
	}

	private function queueEvents(entry:TrackEntry, animationTime:Float):Void {
		var animationStart:Float = entry.animationStart,
			animationEnd:Float = entry.animationEnd;
		var duration:Float = animationEnd - animationStart;
		var reverse:Bool = entry.reverse;
		var split:Float = entry.trackLast % duration;
		if (reverse)
			split = duration - split;

		// Queue events before complete.
		var event:Event;
		var i:Int = 0;
		var n:Int = events.length;
		while (i < n) {
			event = events[i];
			if (event == null) {
				i++;
				continue;
			}
			if ((event.time < split) != reverse)
				break;
			if (event.time >= animationStart && event.time <= animationEnd)
				queue.event(entry, event);
			i++;
		}

		// Queue complete if completed a loop iteration or the animation.
		var complete = false;
		if (entry.loop) {
			if (duration == 0)
				complete = true;
			else {
				var cycles:Float = Math.floor(entry.trackTime / duration);
				complete = cycles > 0 && cycles > Math.floor(entry.trackLast / duration);
			}
		} else
			complete = animationTime >= animationEnd && entry.animationLast < animationEnd;
		if (complete)
			queue.complete(entry);

		// Queue events after complete.
		while (i < n) {
			event = events[i++];
			if (event == null)
				continue;
			if (event.time >= animationStart && event.time <= animationEnd)
				queue.event(entry, event);
		}
	}

	private function eventsReverse(entry:TrackEntry, animationLast:Float, animationTime:Float):Void {
		var duration:Float = entry.animation.duration,
			from:Float = duration - animationLast,
			to:Float = duration - animationTime;
		var timelines:Array<Timeline> = entry.animation.timelines;
		for (i in 0...entry.animation.timelines.length) {
			var timeline:Timeline = timelines[i];
			if (!Std.isOfType(timeline, EventTimeline))
				continue;
			var eventTimeline:EventTimeline = cast(timeline, EventTimeline);
			var timelineEvents:Array<Event> = eventTimeline.events;
			var frames = eventTimeline.frames;
			var frameCount:Int = frames.length;
			if (from >= to) { // from -> to
				for (ii in 0...frameCount) {
					if (frames[ii] < to)
						continue;
					if (frames[ii] >= from)
						break;
					events.push(timelineEvents[ii]);
				}
			} else {
				var ii:Int = 0;
				while (ii < frameCount) { // from -> 0
					if (frames[ii] >= from)
						break;
					events.push(timelineEvents[ii]);
					ii++;
				}
				ii = 0; // end -> to
				while (ii < frameCount) {
					if (frames[ii] >= to)
						break;
					ii++;
				}
				while (ii < frameCount) {
					events.push(timelineEvents[ii]);
					ii++;
				}
			}
		}
	}

	/**
	 * Removes all animations from all tracks, leaving skeletons in their current pose.
	 *
	 * It may be desired to use spine.animation.AnimationState.setEmptyAnimations() to mix the skeletons back to the setup pose,
	 * rather than leaving them in their current pose.
	 */
	public function clearTracks():Void {
		var oldTrainDisabled:Bool = queue.drainDisabled;
		queue.drainDisabled = true;
		for (i in 0...tracks.length) {
			clearTrack(i);
		}
		tracks.resize(0);
		queue.drainDisabled = oldTrainDisabled;
		queue.drain();
	}

	/**
	 * Removes all animations from the track, leaving skeletons in their current pose.
	 *
	 * It may be desired to use spine.animation.AnimationState.setEmptyAnimation() to mix the skeletons back to the setup pose,
	 * rather than leaving them in their current pose.
	 */
	public function clearTrack(trackIndex:Int):Void {
		if (trackIndex < 0)
			throw new SpineException("trackIndex must be >= 0.");
		if (trackIndex >= tracks.length)
			return;
		var current:TrackEntry = tracks[trackIndex];
		if (current == null)
			return;

		queue.end(current);
		clearNext(current);

		var entry:TrackEntry = current;
		while (true) {
			var from:TrackEntry = entry.mixingFrom;
			if (from == null)
				break;
			queue.end(from);
			entry.mixingFrom = null;
			entry.mixingTo = null;
			entry = from;
		}

		tracks[current.trackIndex] = null;

		queue.drain();
	}

	private function setTrack(index:Int, current:TrackEntry, interrupt:Bool):Void {
		var from:TrackEntry = expandToIndex(index);
		tracks[index] = current;
		current.previous = null;

		if (from != null) {
			from.next = null;
			if (interrupt)
				queue.interrupt(from);
			current.mixingFrom = from;
			from.mixingTo = current;
			current.mixTime = 0;

			from.timelinesRotation.resize(0); // Reset rotation for mixing out, in case entry was mixed in.
		}

		queue.start(current);
	}

	/**
	 * Sets an animation by name.
	 *
	 * See spine.animation.AnimationState.setAnimation().
	 */
	public function setAnimationByName(trackIndex:Int, animationName:String, loop:Bool):TrackEntry {
		var animation:Animation = data.skeletonData.findAnimation(animationName);
		if (animation == null)
			throw new SpineException("Animation not found: " + animationName);
		return setAnimation(trackIndex, animation, loop);
	}

	/** Sets the current animation for a track, discarding any queued animations.
	 * If the formerly current track entry is for the same animation and was never applied to a skeleton, it is replaced (not mixed
	 * from).
	 * @param loop If true, the animation will repeat. If false it will not, instead its last frame is applied if played beyond its
	 *           duration. In either case spine.animation.TrackEntry.getTrackEnd() determines when the track is cleared.
	 * @return A track entry to allow further customization of animation playback. References to the track entry must not be kept
	 *         after the spine.animation.AnimationStateListener.dispose() event occurs.
	 */
	public function setAnimation(trackIndex:Int, animation:Animation, loop:Bool):TrackEntry {
		if (trackIndex < 0)
			throw new SpineException("trackIndex must be >= 0.");
		if (animation == null)
			throw new SpineException("animation cannot be null.");
		var interrupt:Bool = true;
		var current:TrackEntry = expandToIndex(trackIndex);
		if (current != null) {
			if (current.nextTrackLast == -1 && current.animation == animation) {
				// Don't mix from an entry that was never applied.
				tracks[trackIndex] = current.mixingFrom;
				queue.interrupt(current);
				queue.end(current);
				clearNext(current);
				current = current.mixingFrom;
				interrupt = false;
			} else {
				clearNext(current);
			}
		}
		var entry:TrackEntry = trackEntry(trackIndex, animation, loop, current);
		setTrack(trackIndex, entry, interrupt);
		queue.drain();
		return entry;
	}

	/**
	 * Queues an animation by name.
	 *
	 * See spine.animation.AnimationState.addAnimation().
	 */
	public function addAnimationByName(trackIndex:Int, animationName:String, loop:Bool, delay:Float):TrackEntry {
		var animation:Animation = data.skeletonData.findAnimation(animationName);
		if (animation == null)
			throw new SpineException("Animation not found: " + animationName);
		return addAnimation(trackIndex, animation, loop, delay);
	}

	/** Adds an animation to be played after the current or last queued animation for a track. If the track has no entries, this is
	 * equivalent to calling spine.animation.AnimationState.setAnimation().
	 * @param delay If > 0, sets spine.animation.TrackEntry.getDelay(). If <= 0, the delay set is the duration of the previous track entry
	 *           minus any mix duration (from the spine.animation.AnimationStateData) plus the specified delay (ie the mix
	 *           ends at (delay = 0) or before (delay < 0) the previous track entry duration). If the
	 *           previous entry is looping, its next loop completion is used instead of its duration.
	 * @return A track entry to allow further customization of animation playback. References to the track entry must not be kept
	 *         after the spine.animation.AnimationStateListener.dispose() event occurs.
	 */
	public function addAnimation(trackIndex:Int, animation:Animation, loop:Bool, delay:Float):TrackEntry {
		if (trackIndex < 0)
			throw new SpineException("trackIndex must be >= 0.");
		if (animation == null)
			throw new SpineException("animation cannot be null.");

		var last:TrackEntry = expandToIndex(trackIndex);
		if (last != null) {
			while (last.next != null) {
				last = last.next;
			}
		}

		var entry:TrackEntry = trackEntry(trackIndex, animation, loop, last);

		if (last == null) {
			setTrack(trackIndex, entry, true);
			queue.drain();
			if (delay < 0)
				delay = 0;
		} else {
			last.next = entry;
			entry.previous = last;
			if (delay <= 0)
				delay = Math.max(delay + last.getTrackComplete() - entry.mixDuration, 0);
		}

		entry.delay = delay;
		return entry;
	}

	/**
	 * Sets an empty animation for a track, discarding any queued animations, and sets the track entry's
	 * spine.animation.TrackEntry.getMixDuration(). An empty animation has no timelines and serves as a placeholder for mixing in or out.
	 *
	 * Mixing out is done by setting an empty animation with a mix duration using either spine.animation.AnimationState.setEmptyAnimation(),
	 * spine.animation.AnimationState.setEmptyAnimations(), or spine.animation.AnimationState.addEmptyAnimation(). Mixing to an empty animation causes
	 * the previous animation to be applied less and less over the mix duration. Properties keyed in the previous animation
	 * transition to the value from lower tracks or to the setup pose value if no lower tracks key the property. A mix duration of
	 * 0 still mixes out over one frame.
	 *
	 * Mixing in is done by first setting an empty animation, then adding an animation using
	 * spine.animation.AnimationState.addAnimation() with the desired delay (an empty animation has a duration of 0) and on
	 * the returned track entry, set the spine.animation.TrackEntry.setMixDuration(). Mixing from an empty animation causes the new
	 * animation to be applied more and more over the mix duration. Properties keyed in the new animation transition from the value
	 * from lower tracks or from the setup pose value if no lower tracks key the property to the value keyed in the new
	 * animation.
	 *
	 * See <a href='https://esotericsoftware.com/spine-applying-animations/#Empty-animations'>Empty animations</a> in the Spine
	 * Runtimes Guide. */
	public function setEmptyAnimation(trackIndex:Int, mixDuration:Float):TrackEntry {
		var entry:TrackEntry = setAnimation(trackIndex, emptyAnimation, false);
		entry.mixDuration = mixDuration;
		entry.trackEnd = mixDuration;
		return entry;
	}

	/**
	 * Adds an empty animation to be played after the current or last queued animation for a track, and sets the track entry's
	 * spine.animation.TrackEntry.getMixDuration(). If the track has no entries,, it is equivalent to calling
	 * spine.animation.AnimationState.setEmptyAnimation().
	 *
	 * See spine.animation.AnimationState.setEmptyAnimation() and
	 * <a href='https://esotericsoftware.com/spine-applying-animations/#Empty-animations'>Empty animations</a> in the Spine
	 * Runtimes Guide.
	 * @param delay If > 0, sets spine.animation.TrackEntry.getDelay(). If <= 0, the delay set is the duration of the previous track entry
	 *           minus any mix duration plus the specified delay (ie the mix ends at (delay = 0) or
	 *           before (delay < 0) the previous track entry duration). If the previous entry is looping, its next
	 *           loop completion is used instead of its duration.
	 * @return A track entry to allow further customization of animation playback. References to the track entry must not be kept
	 *         after the spine.animation.AnimationStateListener.dispose() event occurs.
	 */
	public function addEmptyAnimation(trackIndex:Int, mixDuration:Float, delay:Float):TrackEntry {
		var entry:TrackEntry = addAnimation(trackIndex, emptyAnimation, false, delay);
		if (delay <= 0)
			entry.delay = Math.max(entry.delay + entry.mixDuration - mixDuration, 0);
		entry.mixDuration = mixDuration;
		entry.trackEnd = mixDuration;
		return entry;
	}

	/** Sets an empty animation for every track, discarding any queued animations, and mixes to it over the specified mix duration.
	 *
	 * See <a href='https://esotericsoftware.com/spine-applying-animations/#Empty-animations'>Empty animations</a> in the Spine
	 * Runtimes Guide. */
	public function setEmptyAnimations(mixDuration:Float):Void {
		var oldDrainDisabled:Bool = queue.drainDisabled;
		queue.drainDisabled = true;
		for (i in 0...tracks.length) {
			var current:TrackEntry = tracks[i];
			if (current != null)
				setEmptyAnimation(current.trackIndex, mixDuration);
		}
		queue.drainDisabled = oldDrainDisabled;
		queue.drain();
	}

	private function expandToIndex(index:Int):TrackEntry {
		if (index < tracks.length)
			return tracks[index];
		tracks.resize(index + 1);
		return null;
	}

	private function trackEntry(trackIndex:Int, animation:Animation, loop:Bool, last:TrackEntry):TrackEntry {
		var entry:TrackEntry = cast(trackEntryPool.obtain(), TrackEntry);
		entry.trackIndex = trackIndex;
		entry.animation = animation;
		entry.loop = loop;

		entry.additive = false;
		entry.reverse = false;
		entry.shortestRotation = false;

		entry.eventThreshold = 0;
		entry.alphaAttachmentThreshold = 0;
		entry.mixAttachmentThreshold = 0;
		entry.mixDrawOrderThreshold = 0;

		entry.animationStart = 0;
		entry.animationEnd = animation.duration;
		entry.animationLast = -1;
		entry.nextAnimationLast = -1;

		entry.delay = 0;
		entry.trackTime = 0;
		entry.trackLast = -1;
		entry.nextTrackLast = -1;
		entry.trackEnd = 2147483647;
		entry.timeScale = 1;

		entry.alpha = 1;
		entry.mixTime = 0;
		entry.mixDuration = last == null ? 0 : data.getMix(last.animation, animation);
		entry.mixInterpolation = Interpolation.linear;
		entry.totalAlpha = 0;
		entry.keepHold = false;
		return entry;
	}

	/**
	 * Removes the spine.animation.TrackEntry.getNext() next entry and all entries after it for the specified entry.
	 */
	public function clearNext(entry:TrackEntry):Void {
		var next:TrackEntry = entry.next;
		while (next != null) {
			queue.dispose(next);
			next = next.next;
		}
		entry.next = null;
	}

	private function _animationsChanged():Void {
		animationsChanged = false;

		var entry:TrackEntry = null;
		for (i in 0...tracks.length) {
			var track = tracks[i];
			if (track == null)
				continue;
			entry = track;
			while (entry.mixingFrom != null)
				entry = entry.mixingFrom;
			do {
				computeHold(entry, track);
				entry = entry.mixingTo;
			} while (entry != null);
		}
		propertyIDs.clear();
	}

	private function computeHold(entry:TrackEntry, track:TrackEntry):Void {
		var to:TrackEntry = entry.mixingTo;
		var timelines:Array<Timeline> = entry.animation.timelines;
		var timelinesCount:Int = entry.animation.timelines.length;
		var timelineMode:Array<Int> = entry.timelineMode;
		timelineMode.resize(timelinesCount);
		entry.timelineHoldMix.resize(0);
		var timelineHoldMix:Array<TrackEntry> = entry.timelineHoldMix;
		timelineHoldMix.resize(timelinesCount);
		var add = entry.additive, keepHold = entry.keepHold;

		for (i in 0...timelinesCount) {
			var timeline:Timeline = timelines[i];
			var ids:Array<String> = timeline.propertyIds;
			var from = getFrom(track, timeline, ids);

			if (add && timeline.additive) {
				timelineMode[i] = from;
				continue;
			}

			var mode:Int;
			if (to == null || timeline.instant || (to.additive && timeline.additive) || !to.animation.hasTimeline(ids))
				mode = from;
			else {
				mode = from | HOLD;
				var next:TrackEntry = to.mixingTo;
				while (next != null) {
					if ((next.additive && timeline.additive) || !next.animation.hasTimeline(ids)) {
						if (next.mixDuration > 0)
							timelineHoldMix[i] = next;
						break;
					}
					next = next.mixingTo;
				}
			}
			if (keepHold)
				mode = (mode & ~HOLD) | (timelineMode[i] & HOLD);
			timelineMode[i] = mode;
		}
	}

	private function getFrom(track:TrackEntry, timeline:Timeline, ids:Array<String>):Int {
		var from = SETUP;
		var i = 0, n = ids.length;
		while (i < n) {
			var owner = propertyIDs.get(ids[i]);
			if (owner == null) {
				propertyIDs.set(ids[i], track);
			} else {
				if (owner != track) {
					while (++i < n)
						if (!propertyIDs.exists(ids[i]))
							propertyIDs.set(ids[i], track);
					return CURRENT;
				}
				from = FIRST;
			}
			i++;
		}
		if (Std.isOfType(timeline, DrawOrderFolderTimeline)) {
			var first = propertyIDs.get(DrawOrderTimeline.propertyID);
			if (first != null)
				return first != track ? CURRENT : FIRST;
		}
		return from;
	}

	/**
	 * Returns the track entry for the animation currently playing on the track, or null if no animation is currently playing.
	 */
	public function getTrack(trackIndex:Int):TrackEntry {
		if (trackIndex < 0)
			throw new SpineException("trackIndex must be >= 0.");
		if (trackIndex >= tracks.length)
			return null;
		return tracks[trackIndex];
	}

	/**
	 * Removes all listeners added with spine.animation.AnimationState.addListener().
	 */
	public function clearListeners():Void {
		onStart.listeners.resize(0);
		onInterrupt.listeners.resize(0);
		onEnd.listeners.resize(0);
		onDispose.listeners.resize(0);
		onComplete.listeners.resize(0);
		onEvent.listeners.resize(0);
	}

	/**
	 * Discards all listener notifications that have not yet been delivered. This can be useful to call from an
	 * spine.animation.AnimationStateListener when it is known that further notifications that may have been already queued for delivery
	 * are not wanted because new animations are being set.
	 */
	public function clearListenerNotifications():Void {
		queue.clear();
	}
}

class StringSet {
	private var entries:StringMap<Bool> = new StringMap<Bool>();
	private var size:Int = 0;

	public function new() {}

	public function add(value:String):Bool {
		var contains:Bool = entries.exists(value);
		entries.set(value, true);
		if (!contains) {
			size++;
			return true;
		}
		return false;
	}

	public function addAll(values:Array<String>):Bool {
		var oldSize:Int = size;
		for (i in 0...values.length) {
			add(values[i]);
		}
		return oldSize != size;
	}

	public function contains(value:String):Bool {
		return entries.exists(value);
	}

	public function clear():Void {
		entries = new StringMap<Bool>();
		size = 0;
	}
}
