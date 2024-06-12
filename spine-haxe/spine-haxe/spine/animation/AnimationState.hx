/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*****************************************************************************/

package spine.animation;

import haxe.ds.StringMap;
import spine.animation.Listeners.EventListeners;
import spine.Event;
import spine.Pool;
import spine.Skeleton;

class AnimationState {
	public static inline var SUBSEQUENT:Int = 0;
	public static inline var FIRST:Int = 1;
	public static inline var HOLD_SUBSEQUENT:Int = 2;
	public static inline var HOLD_FIRST:Int = 3;
	public static inline var HOLD_MIX:Int = 4;
	public static inline var SETUP:Int = 1;
	public static inline var CURRENT:Int = 2;

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
	private var propertyIDs:StringSet = new StringSet();

	public var animationsChanged:Bool = false;
	public var timeScale:Float = 1;
	public var trackEntryPool:Pool<TrackEntry>;

	private var unkeyedState:Int = 0;

	public function new(data:AnimationStateData) {
		if (data == null)
			throw new SpineException("data can not be null");
		this.data = data;
		this.queue = new EventQueue(this);
		this.trackEntryPool = new Pool(function():Dynamic {
			return new TrackEntry();
		});
	}

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
					next.trackTime = current.timeScale == 0 ? 0 : (nextTime / current.timeScale + delta) * next.timeScale;
					current.trackTime += currentDelta;
					setCurrent(i, next, true);
					while (next.mixingFrom != null) {
						next.mixTime += currentDelta;
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

	private function updateMixingFrom(to:TrackEntry, delta:Float):Bool {
		var from:TrackEntry = to.mixingFrom;
		if (from == null)
			return true;

		var finished:Bool = updateMixingFrom(from, delta);

		from.animationLast = from.nextAnimationLast;
		from.trackLast = from.nextTrackLast;

		// Require mixTime > 0 to ensure the mixing from entry was applied at least once.
		if (to.mixTime > 0 && to.mixTime >= to.mixDuration) {
			// Require totalAlpha == 0 to ensure mixing is complete, unless mixDuration == 0 (the transition is a single frame).
			if (from.totalAlpha == 0 || to.mixDuration == 0) {
				to.mixingFrom = from.mixingFrom;
				if (from.mixingFrom != null)
					from.mixingFrom.mixingTo = to;
				to.interruptAlpha = from.interruptAlpha;
				queue.end(from);
			}
			return finished;
		}

		from.trackTime += delta * from.timeScale;
		to.mixTime += delta;
		return false;
	}

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
			var blend:MixBlend = i == 0 ? MixBlend.first : current.mixBlend;

			// Apply mixing from entries first.
			var alpha:Float = current.alpha;
			if (current.mixingFrom != null) {
				alpha *= applyMixingFrom(current, skeleton, blend);
			} else if (current.trackTime >= current.trackEnd && current.next == null) {
				alpha = 0;
			}
			var attachments:Bool = alpha >= current.alphaAttachmentThreshold;

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
			var timeline:Timeline;
			if ((i == 0 && alpha == 1) || blend == MixBlend.add) {
				if (i == 0) attachments = true;
				for (timeline in timelines) {
					if (Std.isOfType(timeline, AttachmentTimeline)) {
						applyAttachmentTimeline(cast(timeline, AttachmentTimeline), skeleton, applyTime, blend, attachments);
					} else {
						timeline.apply(skeleton, animationLast, applyTime, applyEvents, alpha, blend, MixDirection.mixIn);
					}
				}
			} else {
				var timelineMode:Array<Int> = current.timelineMode;

				var shortestRotation = current.shortestRotation;
				var firstFrame:Bool = !shortestRotation && current.timelinesRotation.length != timelineCount << 1;
				if (firstFrame)
					current.timelinesRotation.resize(timelineCount << 1);

				for (ii in 0...timelineCount) {
					var timeline:Timeline = timelines[ii];
					var timelineBlend:MixBlend = timelineMode[ii] == SUBSEQUENT ? blend : MixBlend.setup;
					if (!shortestRotation && Std.isOfType(timeline, RotateTimeline)) {
						this.applyRotateTimeline(cast(timeline, RotateTimeline), skeleton, applyTime, alpha, timelineBlend, current.timelinesRotation, ii << 1,
							firstFrame);
					} else if (Std.isOfType(timeline, AttachmentTimeline)) {
						this.applyAttachmentTimeline(cast(timeline, AttachmentTimeline), skeleton, applyTime, blend, attachments);
					} else {
						timeline.apply(skeleton, animationLast, applyTime, applyEvents, alpha, timelineBlend, MixDirection.mixIn);
					}
				}
			}
			queueEvents(current, animationTime);
			events.resize(0);
			current.nextAnimationLast = animationTime;
			current.nextTrackLast = current.trackTime;
		}

		// Set slots attachments to the setup pose, if needed. This occurs if an animation that is mixing out sets attachments so
		// subsequent timelines see any deform, but the subsequent timelines don't set an attachment (eg they are also mixing out or
		// the time is before the first key).
		var setupState:Int = unkeyedState + SETUP;
		for (slot in skeleton.slots) {
			if (slot.attachmentState == setupState) {
				var attachmentName:String = slot.data.attachmentName;
				slot.attachment = attachmentName == null ? null : skeleton.getAttachmentForSlotIndex(slot.data.index, attachmentName);
			}
		}
		unkeyedState += 2; // Increasing after each use avoids the need to reset attachmentState for every slot.

		queue.drain();
		return applied;
	}

	private function applyMixingFrom(to:TrackEntry, skeleton:Skeleton, blend:MixBlend):Float {
		var from:TrackEntry = to.mixingFrom;
		if (from.mixingFrom != null)
			applyMixingFrom(from, skeleton, blend);

		var mix:Float = 0;
		if (to.mixDuration == 0) // Single frame mix to undo mixingFrom changes.
		{
			mix = 1;
			if (blend == MixBlend.first)
				blend = MixBlend.setup;
		} else {
			mix = to.mixTime / to.mixDuration;
			if (mix > 1)
				mix = 1;
			if (blend != MixBlend.first)
				blend = from.mixBlend;
		}

		var attachments:Bool = mix < from.mixAttachmentThreshold,
			drawOrder:Bool = mix < from.mixDrawOrderThreshold;
		var timelineCount:Int = from.animation.timelines.length;
		var timelines:Array<Timeline> = from.animation.timelines;
		var alphaHold:Float = from.alpha * to.interruptAlpha,
			alphaMix:Float = alphaHold * (1 - mix);
		var animationLast:Float = from.animationLast,
			animationTime:Float = from.getAnimationTime(),
			applyTime:Float = animationTime;
		var applyEvents:Array<Event> = null;
		if (from.reverse) {
			applyTime = from.animation.duration - applyTime;
		} else if (mix < from.eventThreshold) {
			applyEvents = events;
		}

		if (blend == MixBlend.add) {
			for (timeline in timelines) {
				timeline.apply(skeleton, animationLast, applyTime, applyEvents, alphaMix, blend, MixDirection.mixOut);
			}
		} else {
			var timelineMode:Array<Int> = from.timelineMode;
			var timelineHoldMix:Array<TrackEntry> = from.timelineHoldMix;
			var shortestRotation = from.shortestRotation;

			var firstFrame:Bool = !shortestRotation && from.timelinesRotation.length != timelineCount << 1;
			if (firstFrame)
				from.timelinesRotation.resize(timelineCount << 1);
			var timelinesRotation:Array<Float> = from.timelinesRotation;

			from.totalAlpha = 0;
			for (i in 0...timelineCount) {
				var timeline:Timeline = timelines[i];
				var direction:MixDirection = MixDirection.mixOut;
				var timelineBlend:MixBlend;
				var alpha:Float = 0;
				switch (timelineMode[i]) {
					case SUBSEQUENT:
						if (!drawOrder && Std.isOfType(timeline, DrawOrderTimeline))
							continue;
						timelineBlend = blend;
						alpha = alphaMix;
					case FIRST:
						timelineBlend = MixBlend.setup;
						alpha = alphaMix;
					case HOLD_SUBSEQUENT:
						timelineBlend = blend;
						alpha = alphaHold;
					case HOLD_FIRST:
						timelineBlend = MixBlend.setup;
						alpha = alphaHold;
					default:
						timelineBlend = MixBlend.setup;
						var holdMix:TrackEntry = timelineHoldMix[i];
						alpha = alphaHold * Math.max(0, 1 - holdMix.mixTime / holdMix.mixDuration);
				}
				from.totalAlpha += alpha;

				if (!shortestRotation && Std.isOfType(timeline, RotateTimeline)) {
					applyRotateTimeline(cast(timeline, RotateTimeline), skeleton, applyTime, alpha, timelineBlend, from.timelinesRotation, i << 1, firstFrame);
				} else if (Std.isOfType(timeline, AttachmentTimeline)) {
					applyAttachmentTimeline(cast(timeline, AttachmentTimeline), skeleton, applyTime, timelineBlend, attachments && alpha >= from.alphaAttachmentThreshold);
				} else {
					if (drawOrder && Std.isOfType(timeline, DrawOrderTimeline) && timelineBlend == MixBlend.setup)
						direction = MixDirection.mixIn;
					timeline.apply(skeleton, animationLast, applyTime, events, alpha, timelineBlend, direction);
				}
			}
		}

		if (to.mixDuration > 0)
			queueEvents(from, animationTime);
		events.resize(0);
		from.nextAnimationLast = animationTime;
		from.nextTrackLast = from.trackTime;

		return mix;
	}

	public function applyAttachmentTimeline(timeline:AttachmentTimeline, skeleton:Skeleton, time:Float, blend:MixBlend, attachments:Bool) {
		var slot = skeleton.slots[timeline.slotIndex];
		if (!slot.bone.active)
			return;

		if (time < timeline.frames[0]) { // Time is before first frame.
			if (blend == MixBlend.setup || blend == MixBlend.first)
				this.setAttachment(skeleton, slot, slot.data.attachmentName, attachments);
		} else
			this.setAttachment(skeleton, slot, timeline.attachmentNames[Timeline.search1(timeline.frames, time)], attachments);

		// If an attachment wasn't set (ie before the first frame or attachments is false), set the setup attachment later.
		if (slot.attachmentState <= this.unkeyedState)
			slot.attachmentState = this.unkeyedState + SETUP;
	}

	public function applyRotateTimeline(timeline:RotateTimeline, skeleton:Skeleton, time:Float, alpha:Float, blend:MixBlend, timelinesRotation:Array<Float>,
			i:Int, firstFrame:Bool) {
		if (firstFrame)
			timelinesRotation[i] = 0;

		if (alpha == 1) {
			timeline.apply(skeleton, 0, time, null, 1, blend, MixDirection.mixIn);
			return;
		}

		var bone = skeleton.bones[timeline.boneIndex];
		if (!bone.active)
			return;
		var frames = timeline.frames;
		var r1:Float = 0, r2:Float = 0;
		if (time < frames[0]) {
			switch (blend) {
				case MixBlend.setup:
					bone.rotation = bone.data.rotation;
				default:
					return;
				case MixBlend.first:
					r1 = bone.rotation;
					r2 = bone.data.rotation;
			}
		} else {
			r1 = blend == MixBlend.setup ? bone.data.rotation : bone.rotation;
			r2 = bone.data.rotation + timeline.getCurveValue(time);
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
		bone.rotation = r1 + total * alpha;
	}

	private function setAttachment(skeleton:Skeleton, slot:Slot, attachmentName:String, attachments:Bool):Void {
		slot.attachment = attachmentName == null ? null : skeleton.getAttachmentForSlotIndex(slot.data.index, attachmentName);
		if (attachments)
			slot.attachmentState = unkeyedState + CURRENT;
	}

	private function queueEvents(entry:TrackEntry, animationTime:Float):Void {
		var animationStart:Float = entry.animationStart,
			animationEnd:Float = entry.animationEnd;
		var duration:Float = animationEnd - animationStart;
		var trackLastWrapped:Float = entry.trackLast % duration;

		// Queue events before complete.
		var event:Event;
		var i:Int = 0;
		var n:Int = events.length;
		while (i < n) {
			event = events[i++];
			if (event == null)
				continue;
			if (event.time < trackLastWrapped)
				break;
			if (event.time > animationEnd)
				continue; // Discard events outside animation start/end.
			queue.event(entry, event);
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
		if (complete) queue.complete(entry);

		// Queue events after complete.
		while (i < n) {
			event = events[i++];
			if (event == null)
				continue;
			if (event.time < animationStart)
				continue; // Discard events outside animation start/end.
			queue.event(entry, event);
		}
	}

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

	public function clearTrack(trackIndex:Int):Void {
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

	private function setCurrent(index:Int, current:TrackEntry, interrupt:Bool):Void {
		var from:TrackEntry = expandToIndex(index);
		tracks[index] = current;

		if (from != null) {
			if (interrupt)
				queue.interrupt(from);
			current.mixingFrom = from;
			from.mixingTo = current;
			current.mixTime = 0;

			// Store the interrupted mix percentage.
			if (from.mixingFrom != null && from.mixDuration > 0) {
				current.interruptAlpha *= Math.min(1, from.mixTime / from.mixDuration);
			}

			from.timelinesRotation.resize(0); // Reset rotation for mixing out, in case entry was mixed in.
		}

		queue.start(current);
	}

	public function setAnimationByName(trackIndex:Int, animationName:String, loop:Bool):TrackEntry {
		var animation:Animation = data.skeletonData.findAnimation(animationName);
		if (animation == null)
			throw new SpineException("Animation not found: " + animationName);
		return setAnimation(trackIndex, animation, loop);
	}

	public function setAnimation(trackIndex:Int, animation:Animation, loop:Bool):TrackEntry {
		if (animation == null)
			throw new SpineException("animation cannot be null.");
		var interrupt:Bool = true;
		var current:TrackEntry = expandToIndex(trackIndex);
		if (current != null) {
			if (current.nextTrackLast == -1) {
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
		setCurrent(trackIndex, entry, interrupt);
		queue.drain();
		return entry;
	}

	public function addAnimationByName(trackIndex:Int, animationName:String, loop:Bool, delay:Float):TrackEntry {
		var animation:Animation = data.skeletonData.findAnimation(animationName);
		if (animation == null)
			throw new SpineException("Animation not found: " + animationName);
		return addAnimation(trackIndex, animation, loop, delay);
	}

	public function addAnimation(trackIndex:Int, animation:Animation, loop:Bool, delay:Float):TrackEntry {
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
			setCurrent(trackIndex, entry, true);
			queue.drain();
		} else {
			last.next = entry;
			entry.previous = last;
			if (delay <= 0)
				delay += last.getTrackComplete() - entry.mixDuration;
		}

		entry.delay = delay;
		return entry;
	}

	public function setEmptyAnimation(trackIndex:Int, mixDuration:Float):TrackEntry {
		var entry:TrackEntry = setAnimation(trackIndex, emptyAnimation, false);
		entry.mixDuration = mixDuration;
		entry.trackEnd = mixDuration;
		return entry;
	}

	public function addEmptyAnimation(trackIndex:Int, mixDuration:Float, delay:Float):TrackEntry {
		var entry:TrackEntry = addAnimation(trackIndex, emptyAnimation, false, delay);
		if (delay <= 0)
			entry.delay += entry.mixDuration - mixDuration;
		entry.mixDuration = mixDuration;
		entry.trackEnd = mixDuration;
		return entry;
	}

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
		entry.holdPrevious = false;

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
		entry.interruptAlpha = 1;
		entry.totalAlpha = 0;
		entry.mixBlend = MixBlend.replace;
		return entry;
	}

	/** Removes the {@link TrackEntry#getNext() next entry} and all entries after it for the specified entry. */
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

		propertyIDs.clear();
		var entry:TrackEntry = null;
		for (i in 0...tracks.length) {
			entry = tracks[i];
			if (entry == null)
				continue;
			while (entry.mixingFrom != null) {
				entry = entry.mixingFrom;
			}
			do {
				if (entry.mixingTo == null || entry.mixBlend != MixBlend.add)
					computeHold(entry);
				entry = entry.mixingTo;
			} while (entry != null);
		}
	}

	private function computeHold(entry:TrackEntry):Void {
		var to:TrackEntry = entry.mixingTo;
		var timelines:Array<Timeline> = entry.animation.timelines;
		var timelinesCount:Int = entry.animation.timelines.length;
		var timelineMode:Array<Int> = entry.timelineMode;
		timelineMode.resize(timelinesCount);
		entry.timelineHoldMix.resize(0);
		var timelineHoldMix:Array<TrackEntry> = entry.timelineHoldMix;
		timelineHoldMix.resize(timelinesCount);

		if (to != null && to.holdPrevious) {
			for (i in 0...timelinesCount) {
				timelineMode[i] = propertyIDs.addAll(timelines[i].propertyIds) ? HOLD_FIRST : HOLD_SUBSEQUENT;
			}
			return;
		}

		var continueOuter:Bool;
		for (i in 0...timelinesCount) {
			continueOuter = false;
			var timeline:Timeline = timelines[i];
			var ids:Array<String> = timeline.propertyIds;
			if (!propertyIDs.addAll(ids)) {
				timelineMode[i] = SUBSEQUENT;
			} else if (to == null
				|| Std.isOfType(timeline, AttachmentTimeline)
				|| Std.isOfType(timeline, DrawOrderTimeline)
				|| Std.isOfType(timeline, EventTimeline)
				|| !to.animation.hasTimeline(ids)) {
				timelineMode[i] = FIRST;
			} else {
				var next:TrackEntry = to.mixingTo;
				while (next != null) {
					if (next.animation.hasTimeline(ids)) {
						next = next.mixingTo;
						continue;
					}
					if (entry.mixDuration > 0) {
						timelineMode[i] = HOLD_MIX;
						timelineHoldMix[i] = next;
						continueOuter = true;
						break;
					}
					break;
				}
				if (continueOuter)
					continue;
				timelineMode[i] = HOLD_FIRST;
			}
		}
	}

	public function getCurrent(trackIndex:Int):TrackEntry {
		if (trackIndex >= tracks.length)
			return null;
		return tracks[trackIndex];
	}

	public var fHasEndListener(get, never):Bool;

	private function get_fHasEndListener():Bool {
		return onComplete.listeners.length > 0 || onEnd.listeners.length > 0;
	}

	public function clearListeners():Void {
		onStart.listeners.resize(0);
		onInterrupt.listeners.resize(0);
		onEnd.listeners.resize(0);
		onDispose.listeners.resize(0);
		onComplete.listeners.resize(0);
		onEvent.listeners.resize(0);
	}

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
