/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

import { Animation, MixBlend, AttachmentTimeline, MixDirection, RotateTimeline, DrawOrderTimeline, Timeline, EventTimeline } from "./Animation";
import { AnimationStateData } from "./AnimationStateData";
import { Skeleton } from "./Skeleton";
import { Slot } from "./Slot";
import { StringSet, Pool, Utils, MathUtils } from "./Utils";
import { Event } from "./Event";


/** Applies animations over time, queues animations for later playback, mixes (crossfading) between animations, and applies
 * multiple animations on top of each other (layering).
 *
 * See [Applying Animations](http://esotericsoftware.com/spine-applying-animations/) in the Spine Runtimes Guide. */
export class AnimationState {
	static _emptyAnimation = new Animation("<empty>", [], 0);
	private static emptyAnimation (): Animation {
		return AnimationState._emptyAnimation;
	}

	/** The AnimationStateData to look up mix durations. */
	data: AnimationStateData;

	/** The list of tracks that currently have animations, which may contain null entries. */
	tracks = new Array<TrackEntry | null>();

	/** Multiplier for the delta time when the animation state is updated, causing time for all animations and mixes to play slower
	 * or faster. Defaults to 1.
	 *
	 * See TrackEntry {@link TrackEntry#timeScale} for affecting a single animation. */
	timeScale = 1;
	unkeyedState = 0;

	events = new Array<Event>();
	listeners = new Array<AnimationStateListener>();
	queue = new EventQueue(this);
	propertyIDs = new StringSet();
	animationsChanged = false;

	trackEntryPool = new Pool<TrackEntry>(() => new TrackEntry());

	constructor (data: AnimationStateData) {
		this.data = data;
	}

	/** Increments each track entry {@link TrackEntry#trackTime()}, setting queued animations as current if needed. */
	update (delta: number) {
		delta *= this.timeScale;
		let tracks = this.tracks;
		for (let i = 0, n = tracks.length; i < n; i++) {
			let current = tracks[i];
			if (!current) continue;

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
			if (next) {
				// When the next entry's delay is passed, change to the next entry, preserving leftover time.
				let nextTime = current.trackLast - next.delay;
				if (nextTime >= 0) {
					next.delay = 0;
					next.trackTime += current.timeScale == 0 ? 0 : (nextTime / current.timeScale + delta) * next.timeScale;
					current.trackTime += currentDelta;
					this.setCurrent(i, next, true);
					while (next.mixingFrom) {
						next.mixTime += delta;
						next = next.mixingFrom;
					}
					continue;
				}
			} else if (current.trackLast >= current.trackEnd && !current.mixingFrom) {
				tracks[i] = null;
				this.queue.end(current);
				this.clearNext(current);
				continue;
			}
			if (current.mixingFrom && this.updateMixingFrom(current, delta)) {
				// End mixing from entries once all have completed.
				let from: TrackEntry | null = current.mixingFrom;
				current.mixingFrom = null;
				if (from) from.mixingTo = null;
				while (from) {
					this.queue.end(from);
					from = from.mixingFrom;
				}
			}

			current.trackTime += currentDelta;
		}

		this.queue.drain();
	}

	/** Returns true when all mixing from entries are complete. */
	updateMixingFrom (to: TrackEntry, delta: number): boolean {
		let from = to.mixingFrom;
		if (!from) return true;

		let finished = this.updateMixingFrom(from, delta);

		from.animationLast = from.nextAnimationLast;
		from.trackLast = from.nextTrackLast;

		// Require mixTime > 0 to ensure the mixing from entry was applied at least once.
		if (to.mixTime > 0 && to.mixTime >= to.mixDuration) {
			// Require totalAlpha == 0 to ensure mixing is complete, unless mixDuration == 0 (the transition is a single frame).
			if (from.totalAlpha == 0 || to.mixDuration == 0) {
				to.mixingFrom = from.mixingFrom;
				if (from.mixingFrom) from.mixingFrom.mixingTo = to;
				to.interruptAlpha = from.interruptAlpha;
				this.queue.end(from);
			}
			return finished;
		}

		from.trackTime += delta * from.timeScale;
		to.mixTime += delta;
		return false;
	}

	/** Poses the skeleton using the track entry animations. There are no side effects other than invoking listeners, so the
	 * animation state can be applied to multiple skeletons to pose them identically.
	 * @returns True if any animations were applied. */
	apply (skeleton: Skeleton): boolean {
		if (!skeleton) throw new Error("skeleton cannot be null.");
		if (this.animationsChanged) this._animationsChanged();

		let events = this.events;
		let tracks = this.tracks;
		let applied = false;

		for (let i = 0, n = tracks.length; i < n; i++) {
			let current = tracks[i];
			if (!current || current.delay > 0) continue;
			applied = true;
			let blend: MixBlend = i == 0 ? MixBlend.first : current.mixBlend;

			// Apply mixing from entries first.
			let mix = current.alpha;
			if (current.mixingFrom)
				mix *= this.applyMixingFrom(current, skeleton, blend);
			else if (current.trackTime >= current.trackEnd && !current.next)
				mix = 0;

			// Apply current entry.
			let animationLast = current.animationLast, animationTime = current.getAnimationTime(), applyTime = animationTime;
			let applyEvents: Event[] | null = events;
			if (current.reverse) {
				applyTime = current.animation!.duration - applyTime;
				applyEvents = null;
			}
			let timelines = current.animation!.timelines;
			let timelineCount = timelines.length;
			if ((i == 0 && mix == 1) || blend == MixBlend.add) {
				for (let ii = 0; ii < timelineCount; ii++) {
					// Fixes issue #302 on IOS9 where mix, blend sometimes became undefined and caused assets
					// to sometimes stop rendering when using color correction, as their RGBA values become NaN.
					// (https://github.com/pixijs/pixi-spine/issues/302)
					Utils.webkit602BugfixHelper(mix, blend);
					var timeline = timelines[ii];
					if (timeline instanceof AttachmentTimeline)
						this.applyAttachmentTimeline(timeline, skeleton, applyTime, blend, true);
					else
						timeline.apply(skeleton, animationLast, applyTime, applyEvents, mix, blend, MixDirection.mixIn);
				}
			} else {
				let timelineMode = current.timelineMode;

				let shortestRotation = current.shortestRotation;
				let firstFrame = !shortestRotation && current.timelinesRotation.length != timelineCount << 1;
				if (firstFrame) current.timelinesRotation.length = timelineCount << 1;

				for (let ii = 0; ii < timelineCount; ii++) {
					let timeline = timelines[ii];
					let timelineBlend = timelineMode[ii] == SUBSEQUENT ? blend : MixBlend.setup;
					if (!shortestRotation && timeline instanceof RotateTimeline) {
						this.applyRotateTimeline(timeline, skeleton, applyTime, mix, timelineBlend, current.timelinesRotation, ii << 1, firstFrame);
					} else if (timeline instanceof AttachmentTimeline) {
						this.applyAttachmentTimeline(timeline, skeleton, applyTime, blend, true);
					} else {
						// This fixes the WebKit 602 specific issue described at http://esotericsoftware.com/forum/iOS-10-disappearing-graphics-10109
						Utils.webkit602BugfixHelper(mix, blend);
						timeline.apply(skeleton, animationLast, applyTime, applyEvents, mix, timelineBlend, MixDirection.mixIn);
					}
				}
			}
			this.queueEvents(current, animationTime);
			events.length = 0;
			current.nextAnimationLast = animationTime;
			current.nextTrackLast = current.trackTime;
		}

		// Set slots attachments to the setup pose, if needed. This occurs if an animation that is mixing out sets attachments so
		// subsequent timelines see any deform, but the subsequent timelines don't set an attachment (eg they are also mixing out or
		// the time is before the first key).
		var setupState = this.unkeyedState + SETUP;
		var slots = skeleton.slots;
		for (var i = 0, n = skeleton.slots.length; i < n; i++) {
			var slot = slots[i];
			if (slot.attachmentState == setupState) {
				var attachmentName = slot.data.attachmentName;
				slot.setAttachment(!attachmentName ? null : skeleton.getAttachment(slot.data.index, attachmentName));
			}
		}
		this.unkeyedState += 2; // Increasing after each use avoids the need to reset attachmentState for every slot.

		this.queue.drain();
		return applied;
	}

	applyMixingFrom (to: TrackEntry, skeleton: Skeleton, blend: MixBlend) {
		let from = to.mixingFrom!;
		if (from.mixingFrom) this.applyMixingFrom(from, skeleton, blend);

		let mix = 0;
		if (to.mixDuration == 0) { // Single frame mix to undo mixingFrom changes.
			mix = 1;
			if (blend == MixBlend.first) blend = MixBlend.setup;
		} else {
			mix = to.mixTime / to.mixDuration;
			if (mix > 1) mix = 1;
			if (blend != MixBlend.first) blend = from.mixBlend;
		}

		let attachments = mix < from.attachmentThreshold, drawOrder = mix < from.drawOrderThreshold;
		let timelines = from.animation!.timelines;
		let timelineCount = timelines.length;
		let alphaHold = from.alpha * to.interruptAlpha, alphaMix = alphaHold * (1 - mix);
		let animationLast = from.animationLast, animationTime = from.getAnimationTime(), applyTime = animationTime;
		let events = null;
		if (from.reverse)
			applyTime = from.animation!.duration - applyTime;
		else if (mix < from.eventThreshold)
			events = this.events;

		if (blend == MixBlend.add) {
			for (let i = 0; i < timelineCount; i++)
				timelines[i].apply(skeleton, animationLast, applyTime, events, alphaMix, blend, MixDirection.mixOut);
		} else {
			let timelineMode = from.timelineMode;
			let timelineHoldMix = from.timelineHoldMix;

			let shortestRotation = from.shortestRotation;
			let firstFrame = !shortestRotation && from.timelinesRotation.length != timelineCount << 1;
			if (firstFrame) from.timelinesRotation.length = timelineCount << 1;

			from.totalAlpha = 0;
			for (let i = 0; i < timelineCount; i++) {
				let timeline = timelines[i];
				let direction = MixDirection.mixOut;
				let timelineBlend: MixBlend;
				let alpha = 0;
				switch (timelineMode[i]) {
					case SUBSEQUENT:
						if (!drawOrder && timeline instanceof DrawOrderTimeline) continue;
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
						let holdMix = timelineHoldMix[i];
						alpha = alphaHold * Math.max(0, 1 - holdMix.mixTime / holdMix.mixDuration);
						break;
				}
				from.totalAlpha += alpha;

				if (!shortestRotation && timeline instanceof RotateTimeline)
					this.applyRotateTimeline(timeline, skeleton, applyTime, alpha, timelineBlend, from.timelinesRotation, i << 1, firstFrame);
				else if (timeline instanceof AttachmentTimeline)
					this.applyAttachmentTimeline(timeline, skeleton, applyTime, timelineBlend, attachments);
				else {
					// This fixes the WebKit 602 specific issue described at http://esotericsoftware.com/forum/iOS-10-disappearing-graphics-10109
					Utils.webkit602BugfixHelper(alpha, blend);
					if (drawOrder && timeline instanceof DrawOrderTimeline && timelineBlend == MixBlend.setup)
						direction = MixDirection.mixIn;
					timeline.apply(skeleton, animationLast, applyTime, events, alpha, timelineBlend, direction);
				}
			}
		}

		if (to.mixDuration > 0) this.queueEvents(from, animationTime);
		this.events.length = 0;
		from.nextAnimationLast = animationTime;
		from.nextTrackLast = from.trackTime;

		return mix;
	}

	applyAttachmentTimeline (timeline: AttachmentTimeline, skeleton: Skeleton, time: number, blend: MixBlend, attachments: boolean) {
		var slot = skeleton.slots[timeline.slotIndex];
		if (!slot.bone.active) return;

		if (time < timeline.frames[0]) { // Time is before first frame.
			if (blend == MixBlend.setup || blend == MixBlend.first)
				this.setAttachment(skeleton, slot, slot.data.attachmentName, attachments);
		} else
			this.setAttachment(skeleton, slot, timeline.attachmentNames[Timeline.search1(timeline.frames, time)], attachments);

		// If an attachment wasn't set (ie before the first frame or attachments is false), set the setup attachment later.
		if (slot.attachmentState <= this.unkeyedState) slot.attachmentState = this.unkeyedState + SETUP;
	}

	setAttachment (skeleton: Skeleton, slot: Slot, attachmentName: string | null, attachments: boolean) {
		slot.setAttachment(!attachmentName ? null : skeleton.getAttachment(slot.data.index, attachmentName));
		if (attachments) slot.attachmentState = this.unkeyedState + CURRENT;
	}

	applyRotateTimeline (timeline: RotateTimeline, skeleton: Skeleton, time: number, alpha: number, blend: MixBlend,
		timelinesRotation: Array<number>, i: number, firstFrame: boolean) {

		if (firstFrame) timelinesRotation[i] = 0;

		if (alpha == 1) {
			timeline.apply(skeleton, 0, time, null, 1, blend, MixDirection.mixIn);
			return;
		}

		let bone = skeleton.bones[timeline.boneIndex];
		if (!bone.active) return;
		let frames = timeline.frames;
		let r1 = 0, r2 = 0;
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
		let total = 0, diff = r2 - r1;
		diff -= (16384 - ((16384.499999999996 - diff / 360) | 0)) * 360;
		if (diff == 0) {
			total = timelinesRotation[i];
		} else {
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
		bone.rotation = r1 + total * alpha;
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
		let complete = false;
		if (entry.loop)
			complete = duration == 0 || trackLastWrapped > entry.trackTime % duration;
		else
			complete = animationTime >= animationEnd && entry.animationLast < animationEnd;
		if (complete) this.queue.complete(entry);

		// Queue events after complete.
		for (; i < n; i++) {
			let event = events[i];
			if (event.time < animationStart) continue; // Discard events outside animation start/end.
			this.queue.event(entry, event);
		}
	}

	/** Removes all animations from all tracks, leaving skeletons in their current pose.
	 *
	 * It may be desired to use {@link AnimationState#setEmptyAnimation()} to mix the skeletons back to the setup pose,
	 * rather than leaving them in their current pose. */
	clearTracks () {
		let oldDrainDisabled = this.queue.drainDisabled;
		this.queue.drainDisabled = true;
		for (let i = 0, n = this.tracks.length; i < n; i++)
			this.clearTrack(i);
		this.tracks.length = 0;
		this.queue.drainDisabled = oldDrainDisabled;
		this.queue.drain();
	}

	/** Removes all animations from the track, leaving skeletons in their current pose.
	 *
	 * It may be desired to use {@link AnimationState#setEmptyAnimation()} to mix the skeletons back to the setup pose,
	 * rather than leaving them in their current pose. */
	clearTrack (trackIndex: number) {
		if (trackIndex >= this.tracks.length) return;
		let current = this.tracks[trackIndex];
		if (!current) return;

		this.queue.end(current);

		this.clearNext(current);

		let entry = current;
		while (true) {
			let from = entry.mixingFrom;
			if (!from) break;
			this.queue.end(from);
			entry.mixingFrom = null;
			entry.mixingTo = null;
			entry = from;
		}

		this.tracks[current.trackIndex] = null;

		this.queue.drain();
	}

	setCurrent (index: number, current: TrackEntry, interrupt: boolean) {
		let from = this.expandToIndex(index);
		this.tracks[index] = current;
		current.previous = null;

		if (from) {
			if (interrupt) this.queue.interrupt(from);
			current.mixingFrom = from;
			from.mixingTo = current;
			current.mixTime = 0;

			// Store the interrupted mix percentage.
			if (from.mixingFrom && from.mixDuration > 0)
				current.interruptAlpha *= Math.min(1, from.mixTime / from.mixDuration);

			from.timelinesRotation.length = 0; // Reset rotation for mixing out, in case entry was mixed in.
		}

		this.queue.start(current);
	}

	/** Sets an animation by name.
	  *
	  * See {@link #setAnimationWith()}. */
	setAnimation (trackIndex: number, animationName: string, loop: boolean = false) {
		let animation = this.data.skeletonData.findAnimation(animationName);
		if (!animation) throw new Error("Animation not found: " + animationName);
		return this.setAnimationWith(trackIndex, animation, loop);
	}

	/** Sets the current animation for a track, discarding any queued animations. If the formerly current track entry was never
	 * applied to a skeleton, it is replaced (not mixed from).
	 * @param loop If true, the animation will repeat. If false it will not, instead its last frame is applied if played beyond its
	 *           duration. In either case {@link TrackEntry#trackEnd} determines when the track is cleared.
	 * @returns A track entry to allow further customization of animation playback. References to the track entry must not be kept
	 *         after the {@link AnimationStateListener#dispose()} event occurs. */
	setAnimationWith (trackIndex: number, animation: Animation, loop: boolean = false) {
		if (!animation) throw new Error("animation cannot be null.");
		let interrupt = true;
		let current = this.expandToIndex(trackIndex);
		if (current) {
			if (current.nextTrackLast == -1) {
				// Don't mix from an entry that was never applied.
				this.tracks[trackIndex] = current.mixingFrom;
				this.queue.interrupt(current);
				this.queue.end(current);
				this.clearNext(current);
				current = current.mixingFrom;
				interrupt = false;
			} else
				this.clearNext(current);
		}
		let entry = this.trackEntry(trackIndex, animation, loop, current);
		this.setCurrent(trackIndex, entry, interrupt);
		this.queue.drain();
		return entry;
	}

	/** Queues an animation by name.
	 *
	 * See {@link #addAnimationWith()}. */
	addAnimation (trackIndex: number, animationName: string, loop: boolean = false, delay: number = 0) {
		let animation = this.data.skeletonData.findAnimation(animationName);
		if (!animation) throw new Error("Animation not found: " + animationName);
		return this.addAnimationWith(trackIndex, animation, loop, delay);
	}

	/** Adds an animation to be played after the current or last queued animation for a track. If the track is empty, it is
	 * equivalent to calling {@link #setAnimationWith()}.
	 * @param delay If > 0, sets {@link TrackEntry#delay}. If <= 0, the delay set is the duration of the previous track entry
	 *           minus any mix duration (from the {@link AnimationStateData}) plus the specified `delay` (ie the mix
	 *           ends at (`delay` = 0) or before (`delay` < 0) the previous track entry duration). If the
	 *           previous entry is looping, its next loop completion is used instead of its duration.
	 * @returns A track entry to allow further customization of animation playback. References to the track entry must not be kept
	 *         after the {@link AnimationStateListener#dispose()} event occurs. */
	addAnimationWith (trackIndex: number, animation: Animation, loop: boolean = false, delay: number = 0) {
		if (!animation) throw new Error("animation cannot be null.");

		let last = this.expandToIndex(trackIndex);
		if (last) {
			while (last.next)
				last = last.next;
		}

		let entry = this.trackEntry(trackIndex, animation, loop, last);

		if (!last) {
			this.setCurrent(trackIndex, entry, true);
			this.queue.drain();
		} else {
			last.next = entry;
			entry.previous = last;
			if (delay <= 0) delay += last.getTrackComplete() - entry.mixDuration;
		}

		entry.delay = delay;
		return entry;
	}

	/** Sets an empty animation for a track, discarding any queued animations, and sets the track entry's
	 * {@link TrackEntry#mixduration}. An empty animation has no timelines and serves as a placeholder for mixing in or out.
	 *
	 * Mixing out is done by setting an empty animation with a mix duration using either {@link #setEmptyAnimation()},
	 * {@link #setEmptyAnimations()}, or {@link #addEmptyAnimation()}. Mixing to an empty animation causes
	 * the previous animation to be applied less and less over the mix duration. Properties keyed in the previous animation
	 * transition to the value from lower tracks or to the setup pose value if no lower tracks key the property. A mix duration of
	 * 0 still mixes out over one frame.
	 *
	 * Mixing in is done by first setting an empty animation, then adding an animation using
	 * {@link #addAnimation()} and on the returned track entry, set the
	 * {@link TrackEntry#setMixDuration()}. Mixing from an empty animation causes the new animation to be applied more and
	 * more over the mix duration. Properties keyed in the new animation transition from the value from lower tracks or from the
	 * setup pose value if no lower tracks key the property to the value keyed in the new animation. */
	setEmptyAnimation (trackIndex: number, mixDuration: number = 0) {
		let entry = this.setAnimationWith(trackIndex, AnimationState.emptyAnimation(), false);
		entry.mixDuration = mixDuration;
		entry.trackEnd = mixDuration;
		return entry;
	}

	/** Adds an empty animation to be played after the current or last queued animation for a track, and sets the track entry's
	 * {@link TrackEntry#mixDuration}. If the track is empty, it is equivalent to calling
	 * {@link #setEmptyAnimation()}.
	 *
	 * See {@link #setEmptyAnimation()}.
	 * @param delay If > 0, sets {@link TrackEntry#delay}. If <= 0, the delay set is the duration of the previous track entry
	 *           minus any mix duration plus the specified `delay` (ie the mix ends at (`delay` = 0) or
	 *           before (`delay` < 0) the previous track entry duration). If the previous entry is looping, its next
	 *           loop completion is used instead of its duration.
	 * @return A track entry to allow further customization of animation playback. References to the track entry must not be kept
	 *         after the {@link AnimationStateListener#dispose()} event occurs. */
	addEmptyAnimation (trackIndex: number, mixDuration: number = 0, delay: number = 0) {
		let entry = this.addAnimationWith(trackIndex, AnimationState.emptyAnimation(), false, delay);
		if (delay <= 0) entry.delay += entry.mixDuration - mixDuration;
		entry.mixDuration = mixDuration;
		entry.trackEnd = mixDuration;
		return entry;
	}

	/** Sets an empty animation for every track, discarding any queued animations, and mixes to it over the specified mix
	  * duration. */
	setEmptyAnimations (mixDuration: number = 0) {
		let oldDrainDisabled = this.queue.drainDisabled;
		this.queue.drainDisabled = true;
		for (let i = 0, n = this.tracks.length; i < n; i++) {
			let current = this.tracks[i];
			if (current) this.setEmptyAnimation(current.trackIndex, mixDuration);
		}
		this.queue.drainDisabled = oldDrainDisabled;
		this.queue.drain();
	}

	expandToIndex (index: number) {
		if (index < this.tracks.length) return this.tracks[index];
		Utils.ensureArrayCapacity(this.tracks, index + 1, null);
		this.tracks.length = index + 1;
		return null;
	}

	/** @param last May be null. */
	trackEntry (trackIndex: number, animation: Animation, loop: boolean, last: TrackEntry | null) {
		let entry = this.trackEntryPool.obtain();
		entry.reset();
		entry.trackIndex = trackIndex;
		entry.animation = animation;
		entry.loop = loop;
		entry.holdPrevious = false;

		entry.reverse = false;
		entry.shortestRotation = false;

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
		entry.mixTime = 0;
		entry.mixDuration = !last ? 0 : this.data.getMix(last.animation!, animation);
		entry.interruptAlpha = 1;
		entry.totalAlpha = 0;
		entry.mixBlend = MixBlend.replace;
		return entry;
	}

	/** Removes the {@link TrackEntry#getNext() next entry} and all entries after it for the specified entry. */
	clearNext (entry: TrackEntry) {
		let next = entry.next;
		while (next) {
			this.queue.dispose(next);
			next = next.next;
		}
		entry.next = null;
	}

	_animationsChanged () {
		this.animationsChanged = false;

		this.propertyIDs.clear();
		let tracks = this.tracks;
		for (let i = 0, n = tracks.length; i < n; i++) {
			let entry = tracks[i];
			if (!entry) continue;
			while (entry.mixingFrom)
				entry = entry.mixingFrom;
			do {
				if (!entry.mixingTo || entry.mixBlend != MixBlend.add) this.computeHold(entry);
				entry = entry.mixingTo;
			} while (entry);
		}
	}

	computeHold (entry: TrackEntry) {
		let to = entry.mixingTo;
		let timelines = entry.animation!.timelines;
		let timelinesCount = entry.animation!.timelines.length;
		let timelineMode = entry.timelineMode;
		timelineMode.length = timelinesCount;
		let timelineHoldMix = entry.timelineHoldMix;
		timelineHoldMix.length = 0;
		let propertyIDs = this.propertyIDs;

		if (to && to.holdPrevious) {
			for (let i = 0; i < timelinesCount; i++)
				timelineMode[i] = propertyIDs.addAll(timelines[i].getPropertyIds()) ? HOLD_FIRST : HOLD_SUBSEQUENT;
			return;
		}

		outer:
		for (let i = 0; i < timelinesCount; i++) {
			let timeline = timelines[i];
			let ids = timeline.getPropertyIds();
			if (!propertyIDs.addAll(ids))
				timelineMode[i] = SUBSEQUENT;
			else if (!to || timeline instanceof AttachmentTimeline || timeline instanceof DrawOrderTimeline
				|| timeline instanceof EventTimeline || !to.animation!.hasTimeline(ids)) {
				timelineMode[i] = FIRST;
			} else {
				for (let next = to.mixingTo; next; next = next!.mixingTo) {
					if (next.animation!.hasTimeline(ids)) continue;
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

	/** Returns the track entry for the animation currently playing on the track, or null if no animation is currently playing. */
	getCurrent (trackIndex: number) {
		if (trackIndex >= this.tracks.length) return null;
		return this.tracks[trackIndex];
	}

	/** Adds a listener to receive events for all track entries. */
	addListener (listener: AnimationStateListener) {
		if (!listener) throw new Error("listener cannot be null.");
		this.listeners.push(listener);
	}

	/** Removes the listener added with {@link #addListener()}. */
	removeListener (listener: AnimationStateListener) {
		let index = this.listeners.indexOf(listener);
		if (index >= 0) this.listeners.splice(index, 1);
	}

	/** Removes all listeners added with {@link #addListener()}. */
	clearListeners () {
		this.listeners.length = 0;
	}

	/** Discards all listener notifications that have not yet been delivered. This can be useful to call from an
	 * {@link AnimationStateListener} when it is known that further notifications that may have been already queued for delivery
	 * are not wanted because new animations are being set. */
	clearListenerNotifications () {
		this.queue.clear();
	}
}

/** Stores settings and other state for the playback of an animation on an {@link AnimationState} track.
 *
 * References to a track entry must not be kept after the {@link AnimationStateListener#dispose()} event occurs. */
export class TrackEntry {
	/** The animation to apply for this track entry. */
	animation: Animation | null = null;

	previous: TrackEntry | null = null;

	/** The animation queued to start after this animation, or null. `next` makes up a linked list. */
	next: TrackEntry | null = null;

	/** The track entry for the previous animation when mixing from the previous animation to this animation, or null if no
	 * mixing is currently occuring. When mixing from multiple animations, `mixingFrom` makes up a linked list. */
	mixingFrom: TrackEntry | null = null;

	/** The track entry for the next animation when mixing from this animation to the next animation, or null if no mixing is
	 * currently occuring. When mixing to multiple animations, `mixingTo` makes up a linked list. */
	mixingTo: TrackEntry | null = null;

	/** The listener for events generated by this track entry, or null.
	 *
	 * A track entry returned from {@link AnimationState#setAnimation()} is already the current animation
	 * for the track, so the track entry listener {@link AnimationStateListener#start()} will not be called. */
	listener: AnimationStateListener | null = null;

	/** The index of the track where this track entry is either current or queued.
	 *
	 * See {@link AnimationState#getCurrent()}. */
	trackIndex: number = 0;

	/** If true, the animation will repeat. If false it will not, instead its last frame is applied if played beyond its
	 * duration. */
	loop: boolean = false;

	/** If true, when mixing from the previous animation to this animation, the previous animation is applied as normal instead
	 * of being mixed out.
	 *
	 * When mixing between animations that key the same property, if a lower track also keys that property then the value will
	 * briefly dip toward the lower track value during the mix. This happens because the first animation mixes from 100% to 0%
	 * while the second animation mixes from 0% to 100%. Setting `holdPrevious` to true applies the first animation
	 * at 100% during the mix so the lower track value is overwritten. Such dipping does not occur on the lowest track which
	 * keys the property, only when a higher track also keys the property.
	 *
	 * Snapping will occur if `holdPrevious` is true and this animation does not key all the same properties as the
	 * previous animation. */
	holdPrevious: boolean = false;

	reverse: boolean = false;

	shortestRotation: boolean = false;

	/** When the mix percentage ({@link #mixTime} / {@link #mixDuration}) is less than the
	 * `eventThreshold`, event timelines are applied while this animation is being mixed out. Defaults to 0, so event
	 * timelines are not applied while this animation is being mixed out. */
	eventThreshold: number = 0;

	/** When the mix percentage ({@link #mixtime} / {@link #mixDuration}) is less than the
	 * `attachmentThreshold`, attachment timelines are applied while this animation is being mixed out. Defaults to
	 * 0, so attachment timelines are not applied while this animation is being mixed out. */
	attachmentThreshold: number = 0;

	/** When the mix percentage ({@link #mixTime} / {@link #mixDuration}) is less than the
	 * `drawOrderThreshold`, draw order timelines are applied while this animation is being mixed out. Defaults to 0,
	 * so draw order timelines are not applied while this animation is being mixed out. */
	drawOrderThreshold: number = 0;

	/** Seconds when this animation starts, both initially and after looping. Defaults to 0.
	 *
	 * When changing the `animationStart` time, it often makes sense to set {@link #animationLast} to the same
	 * value to prevent timeline keys before the start time from triggering. */
	animationStart: number = 0;

	/** Seconds for the last frame of this animation. Non-looping animations won't play past this time. Looping animations will
	 * loop back to {@link #animationStart} at this time. Defaults to the animation {@link Animation#duration}. */
	animationEnd: number = 0;


	/** The time in seconds this animation was last applied. Some timelines use this for one-time triggers. Eg, when this
	 * animation is applied, event timelines will fire all events between the `animationLast` time (exclusive) and
	 * `animationTime` (inclusive). Defaults to -1 to ensure triggers on frame 0 happen the first time this animation
	 * is applied. */
	animationLast: number = 0;

	nextAnimationLast: number = 0;

	/** Seconds to postpone playing the animation. When this track entry is the current track entry, `delay`
	 * postpones incrementing the {@link #trackTime}. When this track entry is queued, `delay` is the time from
	 * the start of the previous animation to when this track entry will become the current track entry (ie when the previous
	 * track entry {@link TrackEntry#trackTime} >= this track entry's `delay`).
	 *
	 * {@link #timeScale} affects the delay. */
	delay: number = 0;

	/** Current time in seconds this track entry has been the current track entry. The track time determines
	 * {@link #animationTime}. The track time can be set to start the animation at a time other than 0, without affecting
	 * looping. */
	trackTime: number = 0;

	trackLast: number = 0; nextTrackLast: number = 0;

	/** The track time in seconds when this animation will be removed from the track. Defaults to the highest possible float
	 * value, meaning the animation will be applied until a new animation is set or the track is cleared. If the track end time
	 * is reached, no other animations are queued for playback, and mixing from any previous animations is complete, then the
	 * properties keyed by the animation are set to the setup pose and the track is cleared.
	 *
	 * It may be desired to use {@link AnimationState#addEmptyAnimation()} rather than have the animation
	 * abruptly cease being applied. */
	trackEnd: number = 0;

	/** Multiplier for the delta time when this track entry is updated, causing time for this animation to pass slower or
	 * faster. Defaults to 1.
	 *
	 * {@link #mixTime} is not affected by track entry time scale, so {@link #mixDuration} may need to be adjusted to
	 * match the animation speed.
	 *
	 * When using {@link AnimationState#addAnimation()} with a `delay` <= 0, note the
	 * {@link #delay} is set using the mix duration from the {@link AnimationStateData}, assuming time scale to be 1. If
	 * the time scale is not 1, the delay may need to be adjusted.
	 *
	 * See AnimationState {@link AnimationState#timeScale} for affecting all animations. */
	timeScale: number = 0;

	/** Values < 1 mix this animation with the skeleton's current pose (usually the pose resulting from lower tracks). Defaults
	 * to 1, which overwrites the skeleton's current pose with this animation.
	 *
	 * Typically track 0 is used to completely pose the skeleton, then alpha is used on higher tracks. It doesn't make sense to
	 * use alpha on track 0 if the skeleton pose is from the last frame render. */
	alpha: number = 0;

	/** Seconds from 0 to the {@link #getMixDuration()} when mixing from the previous animation to this animation. May be
	 * slightly more than `mixDuration` when the mix is complete. */
	mixTime: number = 0;

	/** Seconds for mixing from the previous animation to this animation. Defaults to the value provided by AnimationStateData
	 * {@link AnimationStateData#getMix()} based on the animation before this animation (if any).
	 *
	 * A mix duration of 0 still mixes out over one frame to provide the track entry being mixed out a chance to revert the
	 * properties it was animating.
	 *
	 * The `mixDuration` can be set manually rather than use the value from
	 * {@link AnimationStateData#getMix()}. In that case, the `mixDuration` can be set for a new
	 * track entry only before {@link AnimationState#update(float)} is first called.
	 *
	 * When using {@link AnimationState#addAnimation()} with a `delay` <= 0, note the
	 * {@link #delay} is set using the mix duration from the {@link AnimationStateData}, not a mix duration set
	 * afterward. */
	mixDuration: number = 0; interruptAlpha: number = 0; totalAlpha: number = 0;

	/** Controls how properties keyed in the animation are mixed with lower tracks. Defaults to {@link MixBlend#replace}, which
	 * replaces the values from the lower tracks with the animation values. {@link MixBlend#add} adds the animation values to
	 * the values from the lower tracks.
	 *
	 * The `mixBlend` can be set for a new track entry only before {@link AnimationState#apply()} is first
	 * called. */
	mixBlend = MixBlend.replace;
	timelineMode = new Array<number>();
	timelineHoldMix = new Array<TrackEntry>();
	timelinesRotation = new Array<number>();

	reset () {
		this.next = null;
		this.previous = null;
		this.mixingFrom = null;
		this.mixingTo = null;
		this.animation = null;
		this.listener = null;
		this.timelineMode.length = 0;
		this.timelineHoldMix.length = 0;
		this.timelinesRotation.length = 0;
	}

	/** Uses {@link #trackTime} to compute the `animationTime`, which is between {@link #animationStart}
	 * and {@link #animationEnd}. When the `trackTime` is 0, the `animationTime` is equal to the
	 * `animationStart` time. */
	getAnimationTime () {
		if (this.loop) {
			let duration = this.animationEnd - this.animationStart;
			if (duration == 0) return this.animationStart;
			return (this.trackTime % duration) + this.animationStart;
		}
		return Math.min(this.trackTime + this.animationStart, this.animationEnd);
	}

	setAnimationLast (animationLast: number) {
		this.animationLast = animationLast;
		this.nextAnimationLast = animationLast;
	}

	/** Returns true if at least one loop has been completed.
	 *
	 * See {@link AnimationStateListener#complete()}. */
	isComplete () {
		return this.trackTime >= this.animationEnd - this.animationStart;
	}

	/** Resets the rotation directions for mixing this entry's rotate timelines. This can be useful to avoid bones rotating the
	 * long way around when using {@link #alpha} and starting animations on other tracks.
	 *
	 * Mixing with {@link MixBlend#replace} involves finding a rotation between two others, which has two possible solutions:
	 * the short way or the long way around. The two rotations likely change over time, so which direction is the short or long
	 * way also changes. If the short way was always chosen, bones would flip to the other side when that direction became the
	 * long way. TrackEntry chooses the short way the first time it is applied and remembers that direction. */
	resetRotationDirections () {
		this.timelinesRotation.length = 0;
	}

	getTrackComplete () {
		let duration = this.animationEnd - this.animationStart;
		if (duration != 0) {
			if (this.loop) return duration * (1 + ((this.trackTime / duration) | 0)); // Completion of next loop.
			if (this.trackTime < duration) return duration; // Before duration.
		}
		return this.trackTime; // Next update.
	}
}

export class EventQueue {
	objects: Array<any> = [];
	drainDisabled = false;
	animState: AnimationState;

	constructor (animState: AnimationState) {
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
					if (entry.listener && entry.listener.start) entry.listener.start(entry);
					for (let ii = 0; ii < listeners.length; ii++) {
						let listener = listeners[ii];
						if (listener.start) listener.start(entry);
					}
					break;
				case EventType.interrupt:
					if (entry.listener && entry.listener.interrupt) entry.listener.interrupt(entry);
					for (let ii = 0; ii < listeners.length; ii++) {
						let listener = listeners[ii];
						if (listener.interrupt) listener.interrupt(entry);
					}
					break;
				case EventType.end:
					if (entry.listener && entry.listener.end) entry.listener.end(entry);
					for (let ii = 0; ii < listeners.length; ii++) {
						let listener = listeners[ii];
						if (listener.end) listener.end(entry);
					}
				// Fall through.
				case EventType.dispose:
					if (entry.listener && entry.listener.dispose) entry.listener.dispose(entry);
					for (let ii = 0; ii < listeners.length; ii++) {
						let listener = listeners[ii];
						if (listener.dispose) listener.dispose(entry);
					}
					this.animState.trackEntryPool.free(entry);
					break;
				case EventType.complete:
					if (entry.listener && entry.listener.complete) entry.listener.complete(entry);
					for (let ii = 0; ii < listeners.length; ii++) {
						let listener = listeners[ii];
						if (listener.complete) listener.complete(entry);
					}
					break;
				case EventType.event:
					let event = objects[i++ + 2] as Event;
					if (entry.listener && entry.listener.event) entry.listener.event(entry, event);
					for (let ii = 0; ii < listeners.length; ii++) {
						let listener = listeners[ii];
						if (listener.event) listener.event(entry, event);
					}
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

/** The interface to implement for receiving TrackEntry events. It is always safe to call AnimationState methods when receiving
 * events.
 *
 * See TrackEntry {@link TrackEntry#listener} and AnimationState
 * {@link AnimationState#addListener()}. */
export interface AnimationStateListener {
	/** Invoked when this entry has been set as the current entry. */
	start?: (entry: TrackEntry) => void;

	/** Invoked when another entry has replaced this entry as the current entry. This entry may continue being applied for
	 * mixing. */
	interrupt?: (entry: TrackEntry) => void;

	/** Invoked when this entry is no longer the current entry and will never be applied again. */
	end?: (entry: TrackEntry) => void;

	/** Invoked when this entry will be disposed. This may occur without the entry ever being set as the current entry.
	 * References to the entry should not be kept after dispose is called, as it may be destroyed or reused. */
	dispose?: (entry: TrackEntry) => void;

	/** Invoked every time this entry's animation completes a loop. */
	complete?: (entry: TrackEntry) => void;

	/** Invoked when this entry's animation triggers an event. */
	event?: (entry: TrackEntry, event: Event) => void;
}

export abstract class AnimationStateAdapter implements AnimationStateListener {
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

/** 1. A previously applied timeline has set this property.
 *
 * Result: Mix from the current pose to the timeline pose. */
export const SUBSEQUENT = 0;
/** 1. This is the first timeline to set this property.
 * 2. The next track entry applied after this one does not have a timeline to set this property.
 *
 * Result: Mix from the setup pose to the timeline pose. */
export const FIRST = 1;
/** 1) A previously applied timeline has set this property.<br>
 * 2) The next track entry to be applied does have a timeline to set this property.<br>
 * 3) The next track entry after that one does not have a timeline to set this property.<br>
 * Result: Mix from the current pose to the timeline pose, but do not mix out. This avoids "dipping" when crossfading
 * animations that key the same property. A subsequent timeline will set this property using a mix. */
export const HOLD_SUBSEQUENT = 2;
/** 1) This is the first timeline to set this property.<br>
 * 2) The next track entry to be applied does have a timeline to set this property.<br>
 * 3) The next track entry after that one does not have a timeline to set this property.<br>
 * Result: Mix from the setup pose to the timeline pose, but do not mix out. This avoids "dipping" when crossfading animations
 * that key the same property. A subsequent timeline will set this property using a mix. */
export const HOLD_FIRST = 3;
/** 1. This is the first timeline to set this property.
 * 2. The next track entry to be applied does have a timeline to set this property.
 * 3. The next track entry after that one does have a timeline to set this property.
 * 4. timelineHoldMix stores the first subsequent track entry that does not have a timeline to set this property.
 *
 * Result: The same as HOLD except the mix percentage from the timelineHoldMix track entry is used. This handles when more than
 * 2 track entries in a row have a timeline that sets the same property.
 *
 * Eg, A -> B -> C -> D where A, B, and C have a timeline setting same property, but D does not. When A is applied, to avoid
 * "dipping" A is not mixed out, however D (the first entry that doesn't set the property) mixing in is used to mix out A
 * (which affects B and C). Without using D to mix out, A would be applied fully until mixing completes, then snap into
 * place. */
export const HOLD_MIX = 4;

export const SETUP = 1;
export const CURRENT = 2;