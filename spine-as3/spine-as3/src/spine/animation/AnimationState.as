/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

package spine.animation {
	import spine.*;
	import flash.utils.Dictionary;

	public class AnimationState {
		static private var SUBSEQUENT : int = 0;
		static private var FIRST : int = 1;
		static private var HOLD_SUBSEQUENT : int = 2;
		static private var HOLD_FIRST : int = 3;
		static private var HOLD_MIX : int = 4;
		static private var SETUP : int = 1;
		static private var CURRENT : int = 2;

		static private var emptyAnimation : Animation = new Animation("<empty>", new Vector.<Timeline>(), 0);

		public var data : AnimationStateData;
		public var tracks : Vector.<TrackEntry> = new Vector.<TrackEntry>();
		internal var events : Vector.<Event> = new Vector.<Event>();
		public var onStart : Listeners = new Listeners();
		public var onInterrupt : Listeners = new Listeners();
		public var onEnd : Listeners = new Listeners();
		public var onDispose : Listeners = new Listeners();
		public var onComplete : Listeners = new Listeners();
		public var onEvent : Listeners = new Listeners();
		internal var queue : EventQueue;
		internal var propertyIDs : StringSet = new StringSet();
		internal var mixingTo : Vector.<TrackEntry> = new Vector.<TrackEntry>();
		internal var animationsChanged : Boolean;
		public var timeScale : Number = 1;
		internal var trackEntryPool : Pool;
		internal var unkeyedState : int = 0;

		public function AnimationState(data : AnimationStateData) {
			if (data == null) throw new ArgumentError("data can not be null");
			this.data = data;
			this.queue = new EventQueue(this);
			this.trackEntryPool = new Pool(function() : Object {
				return new TrackEntry();
			});
		}

		public function update(delta : Number) : void {
			delta *= timeScale;
			for (var i : int = 0, n : int = tracks.length; i < n; i++) {
				var current : TrackEntry = tracks[i];
				if (current == null) continue;

				current.animationLast = current.nextAnimationLast;
				current.trackLast = current.nextTrackLast;

				var currentDelta : Number = delta * current.timeScale;

				if (current.delay > 0) {
					current.delay -= currentDelta;
					if (current.delay > 0) continue;
					currentDelta = -current.delay;
					current.delay = 0;
				}

				var next : TrackEntry = current.next;
				if (next != null) {
					// When the next entry's delay is passed, change to the next entry, preserving leftover time.
					var nextTime : Number = current.trackLast - next.delay;
					if (nextTime >= 0) {
						next.delay = 0;
						next.trackTime += current.timeScale == 0 ? 0 : (nextTime / current.timeScale + delta) * next.timeScale;
						current.trackTime += currentDelta;
						setCurrent(i, next, true);
						while (next.mixingFrom != null) {
							next.mixTime += delta;
							next = next.mixingFrom;
						}
						continue;
					}
				} else {
					// Clear the track when there is no next entry, the track end time is reached, and there is no mixingFrom.
					if (current.trackLast >= current.trackEnd && current.mixingFrom == null) {
						tracks[i] = null;
						queue.end(current);
						clearNext(current);
						continue;
					}
				}
				if (current.mixingFrom != null && updateMixingFrom(current, delta)) {
					// End mixing from entries once all have completed.
					var from : TrackEntry = current.mixingFrom;
					current.mixingFrom = null;
					if (from != null) from.mixingTo = null;
					while (from != null) {
						queue.end(from);
						from = from.mixingFrom;
					}
				}

				current.trackTime += currentDelta;
			}

			queue.drain();
		}

		private function updateMixingFrom(to : TrackEntry, delta : Number) : Boolean {
			var from : TrackEntry = to.mixingFrom;
			if (from == null) return true;

			var finished : Boolean = updateMixingFrom(from, delta);

			from.animationLast = from.nextAnimationLast;
			from.trackLast = from.nextTrackLast;

			// Require mixTime > 0 to ensure the mixing from entry was applied at least once.
			if (to.mixTime > 0 && to.mixTime >= to.mixDuration) {
				// Require totalAlpha == 0 to ensure mixing is complete, unless mixDuration == 0 (the transition is a single frame).
				if (from.totalAlpha == 0 || to.mixDuration == 0) {
					to.mixingFrom = from.mixingFrom;
					if (from.mixingFrom != null) from.mixingFrom.mixingTo = to;
					to.interruptAlpha = from.interruptAlpha;
					queue.end(from);
				}
				return finished;
			}

			from.trackTime += delta * from.timeScale;
			to.mixTime += delta;
			return false;
		}

		public function apply(skeleton : Skeleton) : Boolean {
			if (skeleton == null) throw new ArgumentError("skeleton cannot be null.");
			if (animationsChanged) _animationsChanged();

			var events : Vector.<Event> = this.events;
			var applied : Boolean = false;

			for (var i : int = 0, n : int = tracks.length; i < n; i++) {
				var current : TrackEntry = tracks[i];
				if (current == null || current.delay > 0) continue;
				applied = true;
				var blend : MixBlend = i == 0 ? MixBlend.first : current.mixBlend;

				// Apply mixing from entries first.
				var mix : Number = current.alpha;
				if (current.mixingFrom != null)
					mix *= applyMixingFrom(current, skeleton, blend);
				else if (current.trackTime >= current.trackEnd && current.next == null)
					mix = 0;

				// Apply current entry.
				var animationLast : Number = current.animationLast, animationTime : Number = current.getAnimationTime(), applyTime : Number = animationTime;
				var applyEvents : Vector.<Event> = events;
				if (current.reverse) {
					applyTime = current.animation.duration - applyTime;
					applyEvents = null;
				}
				var timelines : Vector.<Timeline> = current.animation.timelines;
				var timelineCount : int = timelines.length;
				var ii : int = 0;
				var timeline : Timeline;
				if ((i == 0 && mix == 1) || blend == MixBlend.add) {
					for (ii = 0; ii < timelineCount; ii++) {
						timeline = timelines[ii];
						if (timeline is AttachmentTimeline)
							applyAttachmentTimeline(AttachmentTimeline(timeline), skeleton, applyTime, blend, true);
						else
							timeline.apply(skeleton, animationLast, applyTime, applyEvents, mix, blend, MixDirection.mixIn);
					}
				} else {
					var timelineMode : Vector.<int> = current.timelineMode;

					var firstFrame : Boolean = current.timelinesRotation.length != timelineCount << 1;
					if (firstFrame) current.timelinesRotation.length = timelineCount << 1;

					for (ii = 0; ii < timelineCount; ii++) {
						timeline = timelines[ii];
						var timelineBlend : MixBlend = timelineMode[ii] == SUBSEQUENT ? blend : MixBlend.setup;
						if (timeline is RotateTimeline)
							applyRotateTimeline(RotateTimeline(timeline), skeleton, applyTime, mix, timelineBlend, current.timelinesRotation, ii << 1, firstFrame);
						else if (timeline is AttachmentTimeline)
							applyAttachmentTimeline(AttachmentTimeline(timeline), skeleton, applyTime, timelineBlend, true);
						else
							timeline.apply(skeleton, animationLast, applyTime, applyEvents, mix, timelineBlend, MixDirection.mixIn);
					}
				}
				queueEvents(current, animationTime);
				events.length = 0;
				current.nextAnimationLast = animationTime;
				current.nextTrackLast = current.trackTime;
			}

			// Set slots attachments to the setup pose, if needed. This occurs if an animation that is mixing out sets attachments so
			// subsequent timelines see any deform, but the subsequent timelines don't set an attachment (eg they are also mixing out or
			// the time is before the first key).
			var setupState : int = unkeyedState + SETUP;
			var slots : Vector.<Slot> = skeleton.slots;
			for (var si : int = 0, sn : int = skeleton.slots.length; si < sn; si++) {
				var slot : Slot = slots[si];
				if (slot.attachmentState == setupState) {
					var attachmentName : String = slot.data.attachmentName;
					slot.attachment = attachmentName == null ? null : skeleton.getAttachmentForSlotIndex(slot.data.index, attachmentName);
				}
			}
			this.unkeyedState += 2; // Increasing after each use avoids the need to reset attachmentState for every slot.

			queue.drain();
			return applied;
		}

		private function applyMixingFrom(to : TrackEntry, skeleton : Skeleton, blend : MixBlend) : Number {
			var from : TrackEntry = to.mixingFrom;
			if (from.mixingFrom != null) applyMixingFrom(from, skeleton, blend);

			var mix : Number = 0;
			if (to.mixDuration == 0) { // Single frame mix to undo mixingFrom changes.
				mix = 1;
				if (blend == MixBlend.first) blend = MixBlend.setup;
			} else {
				mix = to.mixTime / to.mixDuration;
				if (mix > 1) mix = 1;
				if (blend != MixBlend.first) blend = from.mixBlend;
			}

			var attachments : Boolean = mix < from.attachmentThreshold, drawOrder : Boolean = mix < from.drawOrderThreshold;
			var timelines : Vector.<Timeline> = from.animation.timelines;
			var timelineCount : int = timelines.length;
			var alphaHold : Number = from.alpha * to.interruptAlpha, alphaMix : Number = alphaHold * (1 - mix);
			var animationLast : Number = from.animationLast, animationTime : Number = from.getAnimationTime(), applyTime : Number = animationTime;
			var events : Vector.<Event> = null;
			if (from.reverse)
				applyTime = from.animation.duration - applyTime;
			else if (mix < from.eventThreshold)
				events = this.events;

			var i : int = 0;
			if (blend == MixBlend.add) {
				for (i = 0; i < timelineCount; i++)
					timelines[i].apply(skeleton, animationLast, applyTime, events, alphaMix, blend, MixDirection.mixOut);
			} else {
				var timelineMode : Vector.<int> = from.timelineMode;
				var timelineHoldMix : Vector.<TrackEntry> = from.timelineHoldMix;

				var firstFrame : Boolean = from.timelinesRotation.length != timelineCount << 1;
				if (firstFrame) from.timelinesRotation.length = timelineCount << 1;
				var timelinesRotation : Vector.<Number> = from.timelinesRotation;

				from.totalAlpha = 0;
				for (i = 0; i < timelineCount; i++) {
					var timeline : Timeline = timelines[i];
					var direction : MixDirection = MixDirection.mixOut;
					var timelineBlend: MixBlend;
					var alpha : Number = 0;
					switch (timelineMode[i]) {
					case SUBSEQUENT:
						if (!drawOrder && timeline is DrawOrderTimeline) continue;
						timelineBlend = blend;
						alpha = alphaMix;
						break;
					case FIRST:
						timelineBlend = MixBlend.setup;
						alpha = alphaMix;
						break;
					case HOLD_SUBSEQUENT:
						timelineBlend = blend;
						alpha = alphaHold;
						break;
					case HOLD_FIRST:
						timelineBlend = MixBlend.setup;
						alpha = alphaHold;
						break;
					default:
						timelineBlend = MixBlend.setup;
						var holdMix : TrackEntry = timelineHoldMix[i];
						alpha = alphaHold * Math.max(0, 1 - holdMix.mixTime / holdMix.mixDuration);
						break;
					}
					from.totalAlpha += alpha;
					if (timeline is RotateTimeline)
						applyRotateTimeline(RotateTimeline(timeline), skeleton, applyTime, alpha, timelineBlend, timelinesRotation, i << 1, firstFrame);
					else if (timeline is AttachmentTimeline) {
						applyAttachmentTimeline(AttachmentTimeline(timeline), skeleton, applyTime, timelineBlend, attachments);
					} else {
						if (drawOrder && timeline is DrawOrderTimeline && timelineBlend == MixBlend.setup) direction = MixDirection.mixIn;
						timeline.apply(skeleton, animationLast, applyTime, events, alpha, timelineBlend, direction);
					}
				}
			}

			if (to.mixDuration > 0) queueEvents(from, animationTime);
			this.events.length = 0;
			from.nextAnimationLast = animationTime;
			from.nextTrackLast = from.trackTime;

			return mix;
		}

		private function applyAttachmentTimeline (timeline: AttachmentTimeline, skeleton: Skeleton, time: Number, blend: MixBlend, attachments: Boolean) : void {
			var slot : Slot = skeleton.slots[timeline.getSlotIndex()];
			if (!slot.bone.active) return;

			var frames : Vector.<Number> = timeline.frames;
			if (time < frames[0]) { // Time is before first frame.
				if (blend == MixBlend.setup || blend == MixBlend.first)
					setAttachment(skeleton, slot, slot.data.attachmentName, attachments);
			} else
				setAttachment(skeleton, slot, timeline.attachmentNames[Timeline.search1(frames, time)], attachments);

			// If an attachment wasn't set (ie before the first frame or attachments is false), set the setup attachment later.
			if (slot.attachmentState <= unkeyedState) slot.attachmentState = unkeyedState + SETUP;
		}

		private function setAttachment (skeleton: Skeleton, slot: Slot, attachmentName: String, attachments: Boolean) : void {
			slot.attachment = attachmentName == null ? null : skeleton.getAttachmentForSlotIndex(slot.data.index, attachmentName);
			if (attachments) slot.attachmentState = unkeyedState + CURRENT;
		}

		private function applyRotateTimeline(timeline : RotateTimeline, skeleton : Skeleton, time : Number, alpha : Number, blend : MixBlend, timelinesRotation : Vector.<Number>, i : int, firstFrame : Boolean) : void {
			if (firstFrame) timelinesRotation[i] = 0;

			if (alpha == 1) {
				timeline.apply(skeleton, 0, time, null, 1, blend, MixDirection.mixIn);
				return;
			}

			var bone : Bone = skeleton.bones[timeline.getBoneIndex()];
			if (!bone.active) return;
			var frames : Vector.<Number> = timeline.frames;
			var r1 : Number, r2 : Number;
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
			var total : Number, diff : Number = r2 - r1;
			diff -= (16384 - int((16384.499999999996 - diff / 360))) * 360;
			if (diff == 0) {
				total = timelinesRotation[i];
			} else {
				var lastTotal : Number, lastDiff : Number;
				if (firstFrame) {
					lastTotal = 0;
					lastDiff = diff;
				} else {
					lastTotal = timelinesRotation[i]; // Angle and direction of mix, including loops.
					lastDiff = timelinesRotation[i + 1]; // Difference between bones.
				}
				var current : Boolean = diff > 0, dir : Boolean = lastTotal >= 0;
				// Detect cross at 0 (not 180).
				if (MathUtils.signum(lastDiff) != MathUtils.signum(diff) && Math.abs(lastDiff) <= 90) {
					// A cross after a 360 rotation is a loop.
					if (Math.abs(lastTotal) > 180) lastTotal += 360 * MathUtils.signum(lastTotal);
					dir = current;
				}
				total = diff + lastTotal - lastTotal % 360; // Store loops as part of lastTotal.
				if (dir != current) total += 360 * MathUtils.signum(lastTotal);
				timelinesRotation[i] = total;
			}
			timelinesRotation[i + 1] = diff;
			bone.rotation = r1 + total * alpha;
		}

		private function queueEvents(entry : TrackEntry, animationTime : Number) : void {
			var animationStart : Number = entry.animationStart, animationEnd : Number = entry.animationEnd;
			var duration : Number = animationEnd - animationStart;
			var trackLastWrapped : Number = entry.trackLast % duration;

			// Queue events before complete.
			var events : Vector.<Event> = this.events;
			var event : Event;
			var i : int = 0, n : int = events.length;
			for (; i < n; i++) {
				event = events[i];
				if (event.time < trackLastWrapped) break;
				if (event.time > animationEnd) continue; // Discard events outside animation start/end.
				queue.event(entry, event);
			}

			// Queue complete if completed a loop iteration or the animation.
			var complete:Boolean;
			if (entry.loop)
				complete = duration == 0 || trackLastWrapped > entry.trackTime % duration;
			else
				complete = animationTime >= animationEnd && entry.animationLast < animationEnd;
			if (complete) queue.complete(entry);

			// Queue events after complete.
			for (; i < n; i++) {
				event = events[i];
				if (event.time < animationStart) continue; // Discard events outside animation start/end.
				queue.event(entry, event);
			}
		}

		public function clearTracks() : void {
			var oldTrainDisabled : Boolean = queue.drainDisabled;
			queue.drainDisabled = true;
			for (var i : int = 0, n : int = tracks.length; i < n; i++)
				clearTrack(i);
			tracks.length = 0;
			queue.drainDisabled = oldTrainDisabled;
			queue.drain();
		}

		public function clearTrack(trackIndex : int) : void {
			if (trackIndex >= tracks.length) return;
			var current : TrackEntry = tracks[trackIndex];
			if (current == null) return;

			queue.end(current);

			clearNext(current);

			var entry : TrackEntry = current;
			while (true) {
				var from : TrackEntry = entry.mixingFrom;
				if (from == null) break;
				queue.end(from);
				entry.mixingFrom = null;
				entry.mixingTo = null;
				entry = from;
			}

			tracks[current.trackIndex] = null;

			queue.drain();
		}

		private function setCurrent(index : int, current : TrackEntry, interrupt : Boolean) : void {
			var from : TrackEntry = expandToIndex(index);
			tracks[index] = current;
			current.previous = null;

			if (from != null) {
				if (interrupt) queue.interrupt(from);
				current.mixingFrom = from;
				from.mixingTo = current;
				current.mixTime = 0;

				// Store the interrupted mix percentage.
				if (from.mixingFrom != null && from.mixDuration > 0)
					current.interruptAlpha *= Math.min(1, from.mixTime / from.mixDuration);

				from.timelinesRotation.length = 0; // Reset rotation for mixing out, in case entry was mixed in.
			}

			queue.start(current);
		}

		public function setAnimationByName(trackIndex : int, animationName : String, loop : Boolean) : TrackEntry {
			var animation : Animation = data.skeletonData.findAnimation(animationName);
			if (animation == null) throw new ArgumentError("Animation not found: " + animationName);
			return setAnimation(trackIndex, animation, loop);
		}

		public function setAnimation(trackIndex : int, animation : Animation, loop : Boolean) : TrackEntry {
			if (animation == null) throw new ArgumentError("animation cannot be null.");
			var interrupt : Boolean = true;
			var current : TrackEntry = expandToIndex(trackIndex);
			if (current != null) {
				if (current.nextTrackLast == -1) {
					// Don't mix from an entry that was never applied.
					tracks[trackIndex] = current.mixingFrom;
					queue.interrupt(current);
					queue.end(current);
					clearNext(current);
					current = current.mixingFrom;
					interrupt = false;
				} else
					clearNext(current);
			}
			var entry : TrackEntry = trackEntry(trackIndex, animation, loop, current);
			setCurrent(trackIndex, entry, interrupt);
			queue.drain();
			return entry;
		}

		public function addAnimationByName(trackIndex : int, animationName : String, loop : Boolean, delay : Number) : TrackEntry {
			var animation : Animation = data.skeletonData.findAnimation(animationName);
			if (animation == null) throw new ArgumentError("Animation not found: " + animationName);
			return addAnimation(trackIndex, animation, loop, delay);
		}

		public function addAnimation(trackIndex : int, animation : Animation, loop : Boolean, delay : Number) : TrackEntry {
			if (animation == null) throw new ArgumentError("animation cannot be null.");

			var last : TrackEntry = expandToIndex(trackIndex);
			if (last != null) {
				while (last.next != null)
					last = last.next;
			}

			var entry : TrackEntry = trackEntry(trackIndex, animation, loop, last);

			if (last == null) {
				setCurrent(trackIndex, entry, true);
				queue.drain();
			} else {
				last.next = entry;
				entry.previous = last;
				if (delay <= 0) delay += last.getTrackComplete() - entry.mixDuration;
			}

			entry.delay = delay;
			return entry;
		}

		public function setEmptyAnimation(trackIndex : int, mixDuration : Number) : TrackEntry {
			var entry : TrackEntry = setAnimation(trackIndex, emptyAnimation, false);
			entry.mixDuration = mixDuration;
			entry.trackEnd = mixDuration;
			return entry;
		}

		public function addEmptyAnimation(trackIndex : int, mixDuration : Number, delay : Number) : TrackEntry {
			var entry : TrackEntry = addAnimation(trackIndex, emptyAnimation, false, delay <= 0 ? 1 : delay);
			entry.mixDuration = mixDuration;
			entry.trackEnd = mixDuration;
			if (delay <= 0 && entry.previous != null) entry.delay = entry.previous.getTrackComplete() - entry.mixDuration + delay;
			return entry;
		}

		public function setEmptyAnimations(mixDuration : Number) : void {
			var oldDrainDisabled : Boolean = queue.drainDisabled;
			queue.drainDisabled = true;
			for (var i : int = 0, n : int = tracks.length; i < n; i++) {
				var current : TrackEntry = tracks[i];
				if (current != null) setEmptyAnimation(current.trackIndex, mixDuration);
			}
			queue.drainDisabled = oldDrainDisabled;
			queue.drain();
		}

		private function expandToIndex(index : int) : TrackEntry {
			if (index < tracks.length) return tracks[index];
			tracks.length = index + 1;
			return null;
		}

		private function trackEntry(trackIndex : int, animation : Animation, loop : Boolean, last : TrackEntry) : TrackEntry {
			var entry : TrackEntry = TrackEntry(trackEntryPool.obtain());
			entry.trackIndex = trackIndex;
			entry.animation = animation;
			entry.loop = loop;
			entry.holdPrevious = false;

			entry.eventThreshold = 0;
			entry.attachmentThreshold = 0;
			entry.drawOrderThreshold = 0;

			entry.animationStart = 0;
			entry.animationEnd = animation.duration;
			entry.animationLast = -1;
			entry.nextAnimationLast = -1;

			entry.delay = 0;
			entry.trackTime = 0;
			entry.trackLast = -1;
			entry.nextTrackLast = -1;
			entry.trackEnd = int.MAX_VALUE;
			entry.timeScale = 1;

			entry.alpha = 1;
			entry.interruptAlpha = 1;
			entry.mixTime = 0;
			entry.mixDuration = last == null ? 0 : data.getMix(last.animation, animation);
			entry.mixBlend = MixBlend.replace;
			return entry;
		}

		/** Removes the {@link TrackEntry#getNext() next entry} and all entries after it for the specified entry. */
		public function clearNext(entry : TrackEntry) : void {
			var next : TrackEntry = entry.next;
			while (next != null) {
				queue.dispose(next);
				next = next.next;
			}
			entry.next = null;
		}

		private function _animationsChanged() : void {
			animationsChanged = false;

			propertyIDs.clear();
			var tracks = this.tracks;
			for (var i : int = 0, n : int = tracks.length; i < n; i++) {
				var entry : TrackEntry = tracks[i];
				if (!entry) continue;
				while (entry.mixingFrom)
					entry = entry.mixingFrom;
				do {
					if (!entry.mixingTo || entry.mixBlend != MixBlend.add) computeHold(entry);
					entry = entry.mixingTo;
				} while (entry);
			}
		}

		private function computeHold (entry: TrackEntry) : void {
			var to : TrackEntry = entry.mixingTo;
			var timelines : Vector.<Timeline> = entry.animation.timelines;
			var timelinesCount : int = entry.animation.timelines.length;
			var timelineMode : Vector.<int> = entry.timelineMode;
			timelineMode.length = timelinesCount;
			entry.timelineHoldMix.length = 0;
			var timelineHoldMix : Vector.<TrackEntry> = entry.timelineHoldMix;
			timelineHoldMix.length = timelinesCount;
			var propertyIDs : StringSet = this.propertyIDs;

			var i : int;

			if (to != null && to.holdPrevious) {
				for (i = 0; i < timelinesCount; i++)
					timelineMode[i] = propertyIDs.addAll(timelines[i].propertyIds) ? HOLD_FIRST : HOLD_SUBSEQUENT;
				return;
			}

			outer:
			for (i = 0; i < timelinesCount; i++) {
				var timeline : Timeline = timelines[i];
				var ids : Vector.<String> = timeline.propertyIds;
				if (!propertyIDs.addAll(ids))
					timelineMode[i] = SUBSEQUENT;
				else if (to == null || timeline is AttachmentTimeline || timeline is DrawOrderTimeline
					|| timeline is EventTimeline || !to.animation.hasTimeline(ids)) {
					timelineMode[i] = FIRST;
				} else {
					for (var next : TrackEntry = to.mixingTo; next != null; next = next.mixingTo) {
						if (next.animation.hasTimeline(ids)) continue;
						if (entry.mixDuration > 0) {
							timelineMode[i] = HOLD_MIX;
							timelineHoldMix[i] = next;
							continue outer;
						}
						break;
					}
					timelineMode[i] = HOLD_FIRST;
				}
			}
		}

		public function getCurrent(trackIndex : int) : TrackEntry {
			if (trackIndex >= tracks.length) return null;
			return tracks[trackIndex];
		}

		public function clearListeners() : void {
			onStart.listeners.length = 0;
			onInterrupt.listeners.length = 0;
			onEnd.listeners.length = 0;
			onDispose.listeners.length = 0;
			onComplete.listeners.length = 0;
			onEvent.listeners.length = 0;
		}

		public function clearListenerNotifications() : void {
			queue.clear();
		}
	}
}

import flash.utils.Dictionary;

class StringSet {
	private var entries : Dictionary = new Dictionary();
	private var size : int = 0;

	public function add (value : String): Boolean {
		var contains : Boolean = entries[value];
		entries[value] = true;
		if (!contains) {
			size++;
			return true;
		}
		return false;
	}

	public function addAll (values : Vector.<String>) : Boolean {
		var oldSize : int = size;
		for (var i : int = 0, n : int = values.length; i < n; i++)
			add(values[i]);
		return oldSize != size;
	}

	public function contains (value : String) : Boolean {
		return entries[value];
	}

	public function clear () : void {
		entries = new Dictionary();
		size = 0;
	}
}
