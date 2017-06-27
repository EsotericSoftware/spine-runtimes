/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

module spine {
	export class AnimationState {
		static emptyAnimation = new Animation("<empty>", [], 0);
		static SUBSEQUENT = 0;
		static FIRST = 1;
		static DIP = 2;
		static DIP_MIX = 3;

		data: AnimationStateData;
		tracks = new Array<TrackEntry>();
		events = new Array<Event>();
		listeners = new Array<AnimationStateListener2>();
		queue = new EventQueue(this);
		propertyIDs = new IntSet();
		mixingTo = new Array<TrackEntry>();
		animationsChanged = false;
		timeScale = 1;

		trackEntryPool = new Pool<TrackEntry>(() => new TrackEntry());

		constructor (data: AnimationStateData) {
			this.data = data;
		}

		update (delta: number) {
			delta *= this.timeScale;
			let tracks = this.tracks;
			for (let i = 0, n = tracks.length; i < n; i++) {
				let current = tracks[i];
				if (current == null) continue;

				current.animationLast = current.nextAnimationLast;
				current.trackLast = current.nextTrackLast;

				let currentDelta = delta * current.timeScale;

				if (current.delay > 0) {
					current.delay -= currentDelta;
					if (current.delay > 0) continue;
					currentDelta = -current.delay;
					current.delay = 0;
				}

				let next = current.next;
				if (next != null) {
					// When the next entry's delay is passed, change to the next entry, preserving leftover time.
					let nextTime = current.trackLast - next.delay;
					if (nextTime >= 0) {
						next.delay = 0;
						next.trackTime = nextTime + delta * next.timeScale;
						current.trackTime += currentDelta;
						this.setCurrent(i, next, true);
						while (next.mixingFrom != null) {
							next.mixTime += currentDelta;
							next = next.mixingFrom;
						}
						continue;
					}
				} else if (current.trackLast >= current.trackEnd && current.mixingFrom == null) {
					tracks[i] = null;
					this.queue.end(current);
					this.disposeNext(current);
					continue;
				}
				if (current.mixingFrom != null && this.updateMixingFrom(current, delta)) {
					// End mixing from entries once all have completed.
					let from = current.mixingFrom;
					current.mixingFrom = null;
					while (from != null) {
						this.queue.end(from);
						from = from.mixingFrom;
					}
				}

				current.trackTime += currentDelta;
			}

			this.queue.drain();
		}

		updateMixingFrom (to: TrackEntry, delta: number): boolean {
			let from = to.mixingFrom;
			if (from == null) return true;

			let finished = this.updateMixingFrom(from, delta);

			// Require mixTime > 0 to ensure the mixing from entry was applied at least once.
			if (to.mixTime > 0 && (to.mixTime >= to.mixDuration || to.timeScale == 0)) {
				if (from.totalAlpha == 0) {
					to.mixingFrom = from.mixingFrom;
					to.interruptAlpha = from.interruptAlpha;
					this.queue.end(from);
				}
				return finished;
			}

			from.animationLast = from.nextAnimationLast;
			from.trackLast = from.nextTrackLast;
			from.trackTime += delta * from.timeScale;
			to.mixTime += delta * to.timeScale;
			return false;
		}

		apply (skeleton: Skeleton) : boolean {
			if (skeleton == null) throw new Error("skeleton cannot be null.");
			if (this.animationsChanged) this._animationsChanged();

			let events = this.events;
			let tracks = this.tracks;
			let applied = false;

			for (let i = 0, n = tracks.length; i < n; i++) {
				let current = tracks[i];
				if (current == null || current.delay > 0) continue;
				applied = true;
				let currentPose = i == 0 ? MixPose.current : MixPose.currentLayered;

				// Apply mixing from entries first.
				let mix = current.alpha;
				if (current.mixingFrom != null)
					mix *= this.applyMixingFrom(current, skeleton, currentPose);
				else if (current.trackTime >= current.trackEnd && current.next == null)
					mix = 0;

				// Apply current entry.
				let animationLast = current.animationLast, animationTime = current.getAnimationTime();
				let timelineCount = current.animation.timelines.length;
				let timelines = current.animation.timelines;
				if (mix == 1) {
					for (let ii = 0; ii < timelineCount; ii++)
						timelines[ii].apply(skeleton, animationLast, animationTime, events, 1, MixPose.setup, MixDirection.in);
				} else {
					let timelineData = current.timelineData;

					let firstFrame = current.timelinesRotation.length == 0;
					if (firstFrame) Utils.setArraySize(current.timelinesRotation, timelineCount << 1, null);
					let timelinesRotation = current.timelinesRotation;

					for (let ii = 0; ii < timelineCount; ii++) {
						let timeline = timelines[ii];
						let pose = timelineData[ii] >= AnimationState.FIRST ? MixPose.setup : currentPose;
						if (timeline instanceof RotateTimeline) {
							this.applyRotateTimeline(timeline, skeleton, animationTime, mix, pose, timelinesRotation, ii << 1, firstFrame);
						} else
							timeline.apply(skeleton, animationLast, animationTime, events, mix, pose, MixDirection.in);
					}
				}
				this.queueEvents(current, animationTime);
				events.length = 0;
				current.nextAnimationLast = animationTime;
				current.nextTrackLast = current.trackTime;
			}

			this.queue.drain();
			return applied;
		}

		applyMixingFrom (to: TrackEntry, skeleton: Skeleton, currentPose: MixPose) {
			let from = to.mixingFrom;
			if (from.mixingFrom != null) this.applyMixingFrom(from, skeleton, currentPose);

			let mix = 0;
			if (to.mixDuration == 0) // Single frame mix to undo mixingFrom changes.
				mix = 1;
			else {
				mix = to.mixTime / to.mixDuration;
				if (mix > 1) mix = 1;
			}

			let events = mix < from.eventThreshold ? this.events : null;
			let attachments = mix < from.attachmentThreshold, drawOrder = mix < from.drawOrderThreshold;
			let animationLast = from.animationLast, animationTime = from.getAnimationTime();
			let timelineCount = from.animation.timelines.length;
			let timelines = from.animation.timelines;
			let timelineData = from.timelineData;
			let timelineDipMix = from.timelineDipMix;

			let firstFrame = from.timelinesRotation.length == 0;
			if (firstFrame) Utils.setArraySize(from.timelinesRotation, timelineCount << 1, null);
			let timelinesRotation = from.timelinesRotation;

			let pose: MixPose;
			let alphaDip = from.alpha * to.interruptAlpha, alphaMix = alphaDip * (1 - mix), alpha = 0;
			from.totalAlpha = 0;
			for (var i = 0; i < timelineCount; i++) {
				let timeline = timelines[i];
				switch (timelineData[i]) {
				case AnimationState.SUBSEQUENT:
					if (!attachments && timeline instanceof AttachmentTimeline) continue;
					if (!drawOrder && timeline instanceof DrawOrderTimeline) continue;
					pose = currentPose;
					alpha = alphaMix;
					break;
				case AnimationState.FIRST:
					pose = MixPose.setup
					alpha = alphaMix;
					break;
				case AnimationState.DIP:
					pose = MixPose.setup;
					alpha = alphaDip;
					break;
				default:
					pose = MixPose.setup;
					alpha = alphaDip;
					let dipMix = timelineDipMix[i];
					alpha *= Math.max(0, 1 - dipMix.mixTime / dipMix.mixDuration);
					break;
				}
				from.totalAlpha += alpha;
				if (timeline instanceof RotateTimeline)
					this.applyRotateTimeline(timeline, skeleton, animationTime, alpha, pose, timelinesRotation, i << 1, firstFrame);
				else {
					timeline.apply(skeleton, animationLast, animationTime, events, alpha, pose, MixDirection.out);
				}
			}

			if (to.mixDuration > 0) this.queueEvents(from, animationTime);
			this.events.length = 0;
			from.nextAnimationLast = animationTime;
			from.nextTrackLast = from.trackTime;

			return mix;
		}

		applyRotateTimeline (timeline: Timeline, skeleton: Skeleton, time: number, alpha: number, pose: MixPose,
			timelinesRotation: Array<number>, i: number, firstFrame: boolean) {

			if (firstFrame) timelinesRotation[i] = 0;

			if (alpha == 1) {
				timeline.apply(skeleton, 0, time, null, 1, pose, MixDirection.in);
				return;
			}

			let rotateTimeline = timeline as RotateTimeline;
			let frames = rotateTimeline.frames;
			let bone = skeleton.bones[rotateTimeline.boneIndex];
			if (time < frames[0]) {
				if (pose == MixPose.setup) bone.rotation = bone.data.rotation;
				return;
			}

			let r2 = 0;
			if (time >= frames[frames.length - RotateTimeline.ENTRIES]) // Time is after last frame.
				r2 = bone.data.rotation + frames[frames.length + RotateTimeline.PREV_ROTATION];
			else {
				// Interpolate between the previous frame and the current frame.
				let frame = Animation.binarySearch(frames, time, RotateTimeline.ENTRIES);
				let prevRotation = frames[frame + RotateTimeline.PREV_ROTATION];
				let frameTime = frames[frame];
				let percent = rotateTimeline.getCurvePercent((frame >> 1) - 1,
					1 - (time - frameTime) / (frames[frame + RotateTimeline.PREV_TIME] - frameTime));

				r2 = frames[frame + RotateTimeline.ROTATION] - prevRotation;
				r2 -= (16384 - ((16384.499999999996 - r2 / 360) | 0)) * 360;
				r2 = prevRotation + r2 * percent + bone.data.rotation;
				r2 -= (16384 - ((16384.499999999996 - r2 / 360) | 0)) * 360;
			}

			// Mix between rotations using the direction of the shortest route on the first frame while detecting crosses.
			let r1 = pose == MixPose.setup ? bone.data.rotation : bone.rotation;
			let total = 0, diff = r2 - r1;
			if (diff == 0) {
				total = timelinesRotation[i];
			} else {
				diff -= (16384 - ((16384.499999999996 - diff / 360) | 0)) * 360;
				let lastTotal = 0, lastDiff = 0;
				if (firstFrame) {
					lastTotal = 0;
					lastDiff = diff;
				} else {
					lastTotal = timelinesRotation[i]; // Angle and direction of mix, including loops.
					lastDiff = timelinesRotation[i + 1]; // Difference between bones.
				}
				let current = diff > 0, dir = lastTotal >= 0;
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
			r1 += total * alpha;
			bone.rotation = r1 - (16384 - ((16384.499999999996 - r1 / 360) | 0)) * 360;
		}

		queueEvents (entry: TrackEntry, animationTime: number) {
			let animationStart = entry.animationStart, animationEnd = entry.animationEnd;
			let duration = animationEnd - animationStart;
			let trackLastWrapped = entry.trackLast % duration;

			// Queue events before complete.
			let events = this.events;
			let i = 0, n = events.length;
			for (; i < n; i++) {
				let event = events[i];
				if (event.time < trackLastWrapped) break;
				if (event.time > animationEnd) continue; // Discard events outside animation start/end.
				this.queue.event(entry, event);
			}

			// Queue complete if completed a loop iteration or the animation.
			if (entry.loop ? (trackLastWrapped > entry.trackTime % duration)
				: (animationTime >= animationEnd && entry.animationLast < animationEnd)) {
				this.queue.complete(entry);
			}

			// Queue events after complete.
			for (; i < n; i++) {
				let event = events[i];
				if (event.time < animationStart) continue; // Discard events outside animation start/end.
				this.queue.event(entry, events[i]);
			}
		}

		clearTracks () {
			let oldDrainDisabled = this.queue.drainDisabled;
			this.queue.drainDisabled = true;
			for (let i = 0, n = this.tracks.length; i < n; i++)
				this.clearTrack(i);
			this.tracks.length = 0;
			this.queue.drainDisabled = oldDrainDisabled;
			this.queue.drain();
		}

		clearTrack (trackIndex: number) {
			if (trackIndex >= this.tracks.length) return;
			let current = this.tracks[trackIndex];
			if (current == null) return;

			this.queue.end(current);

			this.disposeNext(current);

			let entry = current;
			while (true) {
				let from = entry.mixingFrom;
				if (from == null) break;
				this.queue.end(from);
				entry.mixingFrom = null;
				entry = from;
			}

			this.tracks[current.trackIndex] = null;

			this.queue.drain();
		}

		setCurrent (index: number, current: TrackEntry, interrupt: boolean) {
			let from = this.expandToIndex(index);
			this.tracks[index] = current;

			if (from != null) {
				if (interrupt) this.queue.interrupt(from);
				current.mixingFrom = from;
				current.mixTime = 0;

				// Store the interrupted mix percentage.
				if (from.mixingFrom != null && from.mixDuration > 0)
					current.interruptAlpha *= Math.min(1, from.mixTime / from.mixDuration);

				from.timelinesRotation.length = 0; // Reset rotation for mixing out, in case entry was mixed in.
			}

			this.queue.start(current);
		}

		setAnimation (trackIndex: number, animationName: string, loop: boolean) {
			let animation = this.data.skeletonData.findAnimation(animationName);
			if (animation == null) throw new Error("Animation not found: " + animationName);
			return this.setAnimationWith(trackIndex, animation, loop);
		}

		setAnimationWith (trackIndex: number, animation: Animation, loop: boolean) {
			if (animation == null) throw new Error("animation cannot be null.");
			let interrupt = true;
			let current = this.expandToIndex(trackIndex);
			if (current != null) {
				if (current.nextTrackLast == -1) {
					// Don't mix from an entry that was never applied.
					this.tracks[trackIndex] = current.mixingFrom;
					this.queue.interrupt(current);
					this.queue.end(current);
					this.disposeNext(current);
					current = current.mixingFrom;
					interrupt = false;
				} else
					this.disposeNext(current);
			}
			let entry = this.trackEntry(trackIndex, animation, loop, current);
			this.setCurrent(trackIndex, entry, interrupt);
			this.queue.drain();
			return entry;
		}

		addAnimation (trackIndex: number, animationName: string, loop: boolean, delay: number) {
			let animation = this.data.skeletonData.findAnimation(animationName);
			if (animation == null) throw new Error("Animation not found: " + animationName);
			return this.addAnimationWith(trackIndex, animation, loop, delay);
		}

		addAnimationWith (trackIndex: number, animation: Animation, loop: boolean, delay: number) {
			if (animation == null) throw new Error("animation cannot be null.");

			let last = this.expandToIndex(trackIndex);
			if (last != null) {
				while (last.next != null)
					last = last.next;
			}

			let entry = this.trackEntry(trackIndex, animation, loop, last);

			if (last == null) {
				this.setCurrent(trackIndex, entry, true);
				this.queue.drain();
			} else {
				last.next = entry;
				if (delay <= 0) {
					let duration = last.animationEnd - last.animationStart;
					if (duration != 0)
						delay += duration * (1 + ((last.trackTime / duration) | 0)) - this.data.getMix(last.animation, animation);
					else
						delay = 0;
				}
			}

			entry.delay = delay;
			return entry;
		}

		setEmptyAnimation (trackIndex: number, mixDuration: number) {
			let entry = this.setAnimationWith(trackIndex, AnimationState.emptyAnimation, false);
			entry.mixDuration = mixDuration;
			entry.trackEnd = mixDuration;
			return entry;
		}

		addEmptyAnimation (trackIndex: number, mixDuration: number, delay: number) {
			if (delay <= 0) delay -= mixDuration;
			let entry = this.addAnimationWith(trackIndex, AnimationState.emptyAnimation, false, delay);
			entry.mixDuration = mixDuration;
			entry.trackEnd = mixDuration;
			return entry;
		}

		setEmptyAnimations (mixDuration: number) {
			let oldDrainDisabled = this.queue.drainDisabled;
			this.queue.drainDisabled = true;
			for (let i = 0, n = this.tracks.length; i < n; i++) {
				let current = this.tracks[i];
				if (current != null) this.setEmptyAnimation(current.trackIndex, mixDuration);
			}
			this.queue.drainDisabled = oldDrainDisabled;
			this.queue.drain();
		}

		expandToIndex (index: number) {
			if (index < this.tracks.length) return this.tracks[index];
			Utils.ensureArrayCapacity(this.tracks, index - this.tracks.length + 1, null);
			this.tracks.length = index + 1;
			return null;
		}

		trackEntry (trackIndex: number, animation: Animation, loop: boolean, last: TrackEntry) {
			let entry = this.trackEntryPool.obtain();
			entry.trackIndex = trackIndex;
			entry.animation = animation;
			entry.loop = loop;

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
			entry.trackEnd = Number.MAX_VALUE;
			entry.timeScale = 1;

			entry.alpha = 1;
			entry.interruptAlpha = 1;
			entry.mixTime = 0;
			entry.mixDuration = last == null ? 0 : this.data.getMix(last.animation, animation);
			return entry;
		}

		disposeNext (entry: TrackEntry) {
			let next = entry.next;
			while (next != null) {
				this.queue.dispose(next);
				next = next.next;
			}
			entry.next = null;
		}

		_animationsChanged () {
			this.animationsChanged = false;

			let propertyIDs = this.propertyIDs;
			propertyIDs.clear();
			let mixingTo = this.mixingTo;

			for (var i = 0, n = this.tracks.length; i < n; i++) {
				let entry = this.tracks[i];
				if (entry != null) entry.setTimelineData(null, mixingTo, propertyIDs);
			}
		}

		getCurrent (trackIndex: number) {
			if (trackIndex >= this.tracks.length) return null;
			return this.tracks[trackIndex];
		}

		addListener (listener: AnimationStateListener2) {
			if (listener == null) throw new Error("listener cannot be null.");
			this.listeners.push(listener);
		}

		/** Removes the listener added with {@link #addListener(AnimationStateListener)}. */
		removeListener (listener: AnimationStateListener2) {
			let index = this.listeners.indexOf(listener);
			if (index >= 0) this.listeners.splice(index, 1);
		}

		clearListeners () {
			this.listeners.length = 0;
		}

		clearListenerNotifications () {
			this.queue.clear();
		}
	}

	export class TrackEntry {
		animation: Animation;
		next: TrackEntry; mixingFrom: TrackEntry;
		listener: AnimationStateListener2;
		trackIndex: number;
		loop: boolean;
		eventThreshold: number; attachmentThreshold: number; drawOrderThreshold: number;
		animationStart: number; animationEnd: number; animationLast: number; nextAnimationLast: number;
		delay: number; trackTime: number; trackLast: number; nextTrackLast: number; trackEnd: number; timeScale: number;
		alpha: number; mixTime: number; mixDuration: number; interruptAlpha: number; totalAlpha: number;
		timelineData = new Array<number>();
		timelineDipMix = new Array<TrackEntry>();
		timelinesRotation = new Array<number>();

		reset () {
			this.next = null;
			this.mixingFrom = null;
			this.animation = null;
			this.listener = null;
			this.timelineData.length = 0;
			this.timelineDipMix.length = 0;
			this.timelinesRotation.length = 0;
		}

		setTimelineData (to: TrackEntry, mixingToArray: Array<TrackEntry>, propertyIDs: IntSet) : TrackEntry {
			if (to != null) mixingToArray.push(to);
			let lastEntry = this.mixingFrom != null ? this.mixingFrom.setTimelineData(this, mixingToArray, propertyIDs) : this;
			if (to != null) mixingToArray.pop();

			let mixingTo = mixingToArray;
			let mixingToLast = mixingToArray.length - 1;
			let timelines = this.animation.timelines;
			let timelinesCount = this.animation.timelines.length;
			let timelineData = Utils.setArraySize(this.timelineData, timelinesCount);
			this.timelineDipMix.length = 0;
			let timelineDipMix = Utils.setArraySize(this.timelineDipMix, timelinesCount);

			outer:
			for (var i = 0; i < timelinesCount; i++) {
				let id = timelines[i].getPropertyId();
				if (!propertyIDs.add(id))
					timelineData[i] = AnimationState.SUBSEQUENT;
				else if (to == null || !to.hasTimeline(id))
					timelineData[i] = AnimationState.FIRST;
				else {
					for (var ii = mixingToLast; ii >= 0; ii--) {
						let entry = mixingTo[ii];
						if (!entry.hasTimeline(id)) {
							if (entry.mixDuration > 0) {
								timelineData[i] = AnimationState.DIP_MIX;
								timelineDipMix[i] = entry;
								continue outer;
							}
						}
					}
					timelineData[i] = AnimationState.DIP;
				}
			}
			return lastEntry;
		}

		hasTimeline (id: number) : boolean {
			let timelines = this.animation.timelines;
			for (var i = 0, n = timelines.length; i < n; i++)
				if (timelines[i].getPropertyId() == id) return true;
			return false;
		}

		getAnimationTime () {
			if (this.loop) {
				let duration = this.animationEnd - this.animationStart;
				if (duration == 0) return this.animationStart;
				return (this.trackTime % duration) + this.animationStart;
			}
			return Math.min(this.trackTime + this.animationStart, this.animationEnd);
		}

		setAnimationLast(animationLast: number) {
			this.animationLast = animationLast;
			this.nextAnimationLast = animationLast;
		}

		isComplete () {
			return this.trackTime >= this.animationEnd - this.animationStart;
		}

		resetRotationDirections () {
			this.timelinesRotation.length = 0;
		}
	}

	export class EventQueue {
		objects: Array<any> = [];
		drainDisabled = false;
		animState: AnimationState;

		constructor(animState: AnimationState) {
			this.animState = animState;
		}

		start (entry: TrackEntry) {
			this.objects.push(EventType.start);
			this.objects.push(entry);
			this.animState.animationsChanged = true;
		}

		interrupt (entry: TrackEntry) {
			this.objects.push(EventType.interrupt);
			this.objects.push(entry);
		}

		end (entry: TrackEntry) {
			this.objects.push(EventType.end);
			this.objects.push(entry);
			this.animState.animationsChanged = true;
		}

		dispose (entry: TrackEntry) {
			this.objects.push(EventType.dispose);
			this.objects.push(entry);
		}

		complete (entry: TrackEntry) {
			this.objects.push(EventType.complete);
			this.objects.push(entry);
		}

		event (entry: TrackEntry, event: Event) {
			this.objects.push(EventType.event);
			this.objects.push(entry);
			this.objects.push(event);
		}

		drain () {
			if (this.drainDisabled) return;
			this.drainDisabled = true;

			let objects = this.objects;
			let listeners = this.animState.listeners;

			for (let i = 0; i < objects.length; i += 2) {
				let type = objects[i] as EventType;
				let entry = objects[i + 1] as TrackEntry;
				switch (type) {
				case EventType.start:
					if (entry.listener != null && entry.listener.start) entry.listener.start(entry);
					for (let ii = 0; ii < listeners.length; ii++)
						if (listeners[ii].start) listeners[ii].start(entry);
					break;
				case EventType.interrupt:
					if (entry.listener != null && entry.listener.interrupt) entry.listener.interrupt(entry);
					for (let ii = 0; ii < listeners.length; ii++)
						if (listeners[ii].interrupt) listeners[ii].interrupt(entry);
					break;
				case EventType.end:
					if (entry.listener != null && entry.listener.end) entry.listener.end(entry);
					for (let ii = 0; ii < listeners.length; ii++)
						if (listeners[ii].end) listeners[ii].end(entry);
					// Fall through.
				case EventType.dispose:
					if (entry.listener != null && entry.listener.dispose) entry.listener.dispose(entry);
					for (let ii = 0; ii < listeners.length; ii++)
						if (listeners[ii].dispose) listeners[ii].dispose(entry);
					this.animState.trackEntryPool.free(entry);
					break;
				case EventType.complete:
					if (entry.listener != null && entry.listener.complete) entry.listener.complete(entry);
					for (let ii = 0; ii < listeners.length; ii++)
						if (listeners[ii].complete) listeners[ii].complete(entry);
					break;
				case EventType.event:
					let event = objects[i++ + 2] as Event;
					if (entry.listener != null && entry.listener.event) entry.listener.event(entry, event);
					for (let ii = 0; ii < listeners.length; ii++)
						if (listeners[ii].event) listeners[ii].event(entry, event);
					break;
				}
			}
			this.clear();

			this.drainDisabled = false;
		}

		clear () {
			this.objects.length = 0;
		}
	}

	export enum EventType {
		start, interrupt, end, dispose, complete, event
	}

	export interface AnimationStateListener2 {
		/** Invoked when this entry has been set as the current entry. */
		start (entry: TrackEntry): void;

		/** Invoked when another entry has replaced this entry as the current entry. This entry may continue being applied for
		 * mixing. */
		interrupt (entry: TrackEntry): void;

		/** Invoked when this entry is no longer the current entry and will never be applied again. */
		end (entry: TrackEntry): void;

		/** Invoked when this entry will be disposed. This may occur without the entry ever being set as the current entry.
		 * References to the entry should not be kept after dispose is called, as it may be destroyed or reused. */
		dispose (entry: TrackEntry): void;

		/** Invoked every time this entry's animation completes a loop. */
		complete (entry: TrackEntry): void;

		/** Invoked when this entry's animation triggers an event. */
		event (entry: TrackEntry, event: Event): void;
	}

	export abstract class AnimationStateAdapter2 implements AnimationStateListener2 {
		start (entry: TrackEntry) {
		}

		interrupt (entry: TrackEntry) {
		}

		end (entry: TrackEntry) {
		}

		dispose (entry: TrackEntry) {
		}

		complete (entry: TrackEntry) {
		}

		event (entry: TrackEntry, event: Event) {
		}
	}
}
