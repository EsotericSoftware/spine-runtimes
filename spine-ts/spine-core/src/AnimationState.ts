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

/** biome-ignore-all lint/style/noNonNullAssertion: reference runtime expects some nullable to not be null */

import { Animation, AttachmentTimeline, DrawOrderFolderTimeline, DrawOrderTimeline, EventTimeline, RotateTimeline, Timeline } from "./Animation.js";
import type { AnimationStateData } from "./AnimationStateData.js";
import type { Event } from "./Event.js";
import type { Skeleton } from "./Skeleton.js";
import type { Slot } from "./Slot.js";
import { MathUtils, Pool, StringSet, Utils } from "./Utils.js";


/** Applies animations over time, queues animations for later playback, mixes (crossfading) between animations, and applies
 * multiple animations on top of each other (layering).
 *
 * See [Applying Animations](http://esotericsoftware.com/spine-applying-animations#AnimationState-API) in the Spine Runtimes Guide. */
export class AnimationState {
	static readonly emptyAnimation = new Animation("<empty>", [], 0);

	/** The AnimationStateData to look up mix durations. */
	data: AnimationStateData;

	/** The list of tracks that have had animations. May contain null entries for tracks that currently have no animation. */
	readonly tracks = [] as (TrackEntry | null)[];

	/** Multiplier for the delta time when the animation state is updated, causing time for all animations and mixes to play slower
	 * or faster. Defaults to 1.
	 *
	 * See {@link TrackEntry.timeScale} to affect a single animation. */
	timeScale = 1;
	unkeyedState = 0;

	readonly events = [] as Event[];
	readonly listeners = [] as AnimationStateListener[];
	queue = new EventQueue(this);
	propertyIds = new StringSet();
	animationsChanged = false;

	trackEntryPool = new Pool<TrackEntry>(() => new TrackEntry());

	constructor (data: AnimationStateData) {
		this.data = data;
	}

	/** Increments each track entry {@link TrackEntry.trackTime}, setting queued animations as current if needed. */
	update (delta: number) {
		delta *= this.timeScale;
		const tracks = this.tracks;
		for (let i = 0, n = tracks.length; i < n; i++) {
			const current = tracks[i];
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
				const nextTime = current.trackLast - next.delay;
				if (nextTime >= 0) {
					next.delay = 0;
					next.trackTime += current.timeScale === 0 ? 0 : (nextTime / current.timeScale + delta) * next.timeScale;
					current.trackTime += currentDelta;
					this.setTrack(i, next, true);
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
		const from = to.mixingFrom;
		if (!from) return true;

		const finished = this.updateMixingFrom(from, delta);

		from.animationLast = from.nextAnimationLast;
		from.trackLast = from.nextTrackLast;

		// The from entry was applied at least once and the mix is complete.
		if (to.nextTrackLast !== -1 && to.mixTime >= to.mixDuration) {
			// Mixing is complete for all entries before the from entry or the mix is instantaneous.
			if (from.totalAlpha === 0 || to.mixDuration === 0) {
				to.mixingFrom = from.mixingFrom;
				if (from.mixingFrom != null) from.mixingFrom.mixingTo = to;
				if (from.totalAlpha === 0) {
					for (let next = to; next.mixingTo != null; next = next.mixingTo)
						next.keepHold = true;
				}
				this.queue.end(from);
			}
			return finished;
		}

		from.trackTime += delta * from.timeScale;
		to.mixTime += delta;
		return false;
	}

	/** Poses the skeleton using the track entry animations. The animation state is not changed, so can be applied to multiple
	 * skeletons to pose them identically.
	 * @returns True if any animations were applied. */
	apply (skeleton: Skeleton): boolean {
		if (!skeleton) throw new Error("skeleton cannot be null.");
		if (this.animationsChanged) this._animationsChanged();

		const events = this.events;
		const tracks = this.tracks;
		let applied = false;

		for (let i = 0, n = tracks.length; i < n; i++) {
			const current = tracks[i];
			if (!current || current.delay > 0) continue;
			applied = true;

			// Apply mixing from entries first.
			let alpha = current.alpha;
			if (current.mixingFrom)
				alpha *= this.applyMixingFrom(current, skeleton);
			else if (current.trackTime >= current.trackEnd && !current.next)
				alpha = 0;

			// Apply current entry.
			let animationLast = current.animationLast, animationTime = current.getAnimationTime(), applyTime = animationTime;
			let applyEvents: Event[] | null = events;
			if (current.reverse) {
				applyTime = current.animation!.duration - applyTime;
				applyEvents = null;
			}
			const timelines = current.animation!.timelines;
			const timelineCount = timelines.length;
			if ((i === 0 && alpha === 1)) {
				for (let ii = 0; ii < timelineCount; ii++) {
					// Fixes issue #302 on IOS9 where mix, blend sometimes became undefined and caused assets
					// to sometimes stop rendering when using color correction, as their RGBA values become NaN.
					// (https://github.com/pixijs/pixi-spine/issues/302)
					Utils.webkit602BugfixHelper(alpha);
					const timeline = timelines[ii];
					if (timeline instanceof AttachmentTimeline)
						this.applyAttachmentTimeline(timeline, skeleton, applyTime, true, false, true);
					else
						timeline.apply(skeleton, animationLast, applyTime, applyEvents, alpha, true, false, false, false);
				}
			} else {
				const timelineMode = current.timelineMode;
				const attachments = alpha >= current.alphaAttachmentThreshold;
				const add = current.additive, shortestRotation = add || current.shortestRotation;
				const firstFrame = !shortestRotation && current.timelinesRotation.length !== timelineCount << 1;
				if (firstFrame) current.timelinesRotation.length = timelineCount << 1;

				for (let ii = 0; ii < timelineCount; ii++) {
					const timeline = timelines[ii];
					const fromSetup = (timelineMode[ii] & FIRST) !== 0;
					if (!shortestRotation && timeline instanceof RotateTimeline) {
						this.applyRotateTimeline(timeline, skeleton, applyTime, alpha, fromSetup, current.timelinesRotation, ii << 1, firstFrame);
					} else if (timeline instanceof AttachmentTimeline) {
						this.applyAttachmentTimeline(timeline, skeleton, applyTime, fromSetup, false, attachments);
					} else {
						// This fixes the WebKit 602 specific issue described at https://esotericsoftware.com/forum/d/10109-ios-10-disappearing-graphics
						Utils.webkit602BugfixHelper(alpha);
						timeline.apply(skeleton, animationLast, applyTime, applyEvents, alpha, fromSetup, add, false, false);
					}
				}
			}
			if (current.reverse) this.eventsReverse(current, animationLast, animationTime);
			this.queueEvents(current, animationTime);
			events.length = 0;
			current.nextAnimationLast = animationTime;
			current.nextTrackLast = current.trackTime;
		}

		// Set slots attachments to the setup pose, if needed. This occurs if an animation that is mixing out sets attachments so
		// subsequent timelines see any deform, but the subsequent timelines don't set an attachment (eg they are also mixing out or
		// the time is before the first key).
		const setupState = this.unkeyedState + SETUP;
		const slots = skeleton.slots;
		for (let i = 0, n = skeleton.slots.length; i < n; i++) {
			const slot = slots[i];
			if (slot.attachmentState === setupState) {
				const attachmentName = slot.data.attachmentName;
				slot.pose.setAttachment(!attachmentName ? null : skeleton.getAttachment(slot.data.index, attachmentName));
			}
		}
		this.unkeyedState += 2; // Increasing after each use avoids the need to reset attachmentState for every slot.

		this.queue.drain();
		return applied;
	}

	applyMixingFrom (to: TrackEntry, skeleton: Skeleton) {
		const from = to.mixingFrom!;
		const fromMix = from.mixingFrom !== null ? this.applyMixingFrom(from, skeleton) : 1;
		const mix: number = to.mixDuration === 0 ? 1 : Math.min(1, to.mixTime / to.mixDuration);

		const a = from.alpha * fromMix, keep = 1 - mix * to.alpha;
		const alphaMix = a * (1 - mix), alphaHold = keep > 0 ? alphaMix / keep : a;

		const timelines = from.animation!.timelines;
		const timelineCount = timelines.length;
		const timelineMode = from.timelineMode;
		const timelineHoldMix = from.timelineHoldMix;

		const attachments = mix < from.mixAttachmentThreshold, drawOrder = mix < from.mixDrawOrderThreshold;
		const add = from.additive, shortestRotation = add || from.shortestRotation;
		const firstFrame = !shortestRotation && from.timelinesRotation.length !== timelineCount << 1;
		if (firstFrame) from.timelinesRotation.length = timelineCount << 1;
		const timelinesRotation = from.timelinesRotation;

		let animationLast = from.animationLast, animationTime = from.getAnimationTime(), applyTime = animationTime;
		let events = null;
		if (from.reverse)
			applyTime = from.animation!.duration - applyTime;
		else if (mix < from.eventThreshold) //
			events = this.events;

		from.totalAlpha = 0;

		for (let i = 0; i < timelineCount; i++) {
			const timeline = timelines[i];
			const mode = timelineMode[i];
			let alpha = 0;
			if ((mode & HOLD) !== 0) {
				const holdMix = timelineHoldMix[i];
				alpha = holdMix == null ? alphaHold : alphaHold * Math.max(0, 1 - holdMix.mixTime / holdMix.mixDuration);
			} else {
				if (!drawOrder && timeline instanceof DrawOrderTimeline) continue;
				alpha = alphaMix;
			}
			from.totalAlpha += alpha;
			const fromSetup = (mode & FIRST) !== 0;
			if (!shortestRotation && timeline instanceof RotateTimeline) {
				this.applyRotateTimeline(timeline, skeleton, applyTime, alpha, fromSetup, timelinesRotation, i << 1, firstFrame);
			} else if (timeline instanceof AttachmentTimeline)
				this.applyAttachmentTimeline(timeline, skeleton, applyTime, fromSetup, true,
					attachments && alpha >= from.alphaAttachmentThreshold);
			else {
				const out = !drawOrder || !(timeline instanceof DrawOrderTimeline) || !fromSetup;
				timeline.apply(skeleton, animationLast, applyTime, events, alpha, fromSetup, add, out, false);
			}
		}

		if (from.reverse && mix < from.eventThreshold) this.eventsReverse(from, animationLast, animationTime);
		if (to.mixDuration > 0) this.queueEvents(from, animationTime);
		this.events.length = 0;

		from.nextAnimationLast = animationTime;
		from.nextTrackLast = from.trackTime;
		return mix;
	}

	/** Applies the attachment timeline and sets {@link Slot.attachmentState}.
	 * @param attachments False when: 1) the attachment timeline is mixing out, 2) mix < attachmentThreshold, and 3) the timeline
	 * is not the last timeline to set the slot's attachment. In that case the timeline is applied only so subsequent
	 * timelines see any deform. */
	applyAttachmentTimeline (timeline: AttachmentTimeline, skeleton: Skeleton, time: number, fromSetup: boolean,
		out: boolean, attachments: boolean) {
		const slot = skeleton.slots[timeline.slotIndex];
		if (!slot.bone.active) return;

		if (out || time < timeline.frames[0]) {
			if (fromSetup) this.setAttachment(skeleton, slot, slot.data.attachmentName, attachments);
		} else
			this.setAttachment(skeleton, slot, timeline.attachmentNames[Timeline.search(timeline.frames, time)], attachments);

		// If an attachment wasn't set (ie before the first frame or attachments is false), set the setup attachment later.
		if (slot.attachmentState <= this.unkeyedState) slot.attachmentState = this.unkeyedState + SETUP;
	}

	setAttachment (skeleton: Skeleton, slot: Slot, attachmentName: string | null, attachments: boolean) {
		slot.pose.setAttachment(!attachmentName ? null : skeleton.getAttachment(slot.data.index, attachmentName));
		if (attachments) slot.attachmentState = this.unkeyedState + CURRENT;
	}

	/** Applies the rotate timeline, mixing with the current pose while keeping the same rotation direction chosen as the shortest
	 * the first time the mixing was applied. */
	applyRotateTimeline (timeline: RotateTimeline, skeleton: Skeleton, time: number, alpha: number, fromSetup: boolean,
		timelinesRotation: Array<number>, i: number, firstFrame: boolean) {

		if (firstFrame) timelinesRotation[i] = 0;

		if (alpha === 1) {
			timeline.apply(skeleton, 0, time, null, 1, fromSetup, false, false, false);
			return;
		}

		const bone = skeleton.bones[timeline.boneIndex];
		if (!bone.active) return;
		const pose = bone.pose, setup = bone.data.setupPose;
		const frames = timeline.frames;
		if (time < frames[0]) { // Time is before first frame.
			if (fromSetup) pose.rotation = setup.rotation;
			return;
		}
		const r1 = fromSetup ? setup.rotation : pose.rotation;
		const r2 = setup.rotation + timeline.getCurveValue(time);

		// Mix between rotations using the direction of the shortest route on the first frame while detecting crosses.
		let total = 0, diff = r2 - r1;
		diff -= Math.ceil(diff / 360 - 0.5) * 360;
		if (diff === 0) {
			total = timelinesRotation[i];
		} else {
			let lastTotal = 0, lastDiff = 0;
			if (firstFrame) {
				lastTotal = 0;
				lastDiff = diff;
			} else {
				lastTotal = timelinesRotation[i];
				lastDiff = timelinesRotation[i + 1];
			}
			const loops = lastTotal - lastTotal % 360;
			total = diff + loops;
			let current = diff >= 0, dir = lastTotal >= 0;
			if (Math.abs(lastDiff) <= 90 && MathUtils.signum(lastDiff) !== MathUtils.signum(diff)) {
				if (Math.abs(lastTotal - loops) > 180) {
					total += 360 * MathUtils.signum(lastTotal);
					dir = current;
				} else if (loops !== 0)
					total -= 360 * MathUtils.signum(lastTotal);
				else
					dir = current;
			}
			if (dir !== current) total += 360 * MathUtils.signum(lastTotal);
			timelinesRotation[i] = total;
		}
		timelinesRotation[i + 1] = diff;
		pose.rotation = r1 + total * alpha;
	}

	queueEvents (entry: TrackEntry, animationTime: number) {
		const animationStart = entry.animationStart, animationEnd = entry.animationEnd, duration = animationEnd - animationStart;
		const reverse = entry.reverse;
		let split = entry.trackLast % duration;
		if (reverse) split = duration - split;

		// Queue events before complete.
		const events = this.events;
		let i = 0, n = events.length;
		for (; i < n; i++) {
			const event = events[i];
			if ((event.time < split) !== reverse) break; // java: if (event.time < split ^ reverse) break;
			if (event.time >= animationStart && event.time <= animationEnd) this.queue.event(entry, event);
		}

		// Queue complete if completed a loop iteration or the animation.
		let complete = false;
		if (entry.loop) {
			if (duration === 0)
				complete = true;
			else {
				const cycles = Math.floor(entry.trackTime / duration);
				complete = cycles > 0 && cycles > Math.floor(entry.trackLast / duration);
			}
		} else
			complete = animationTime >= animationEnd && entry.animationLast < animationEnd;
		if (complete) this.queue.complete(entry);

		// Queue events after complete.
		for (; i < n; i++) {
			const event = events[i];
			if (event.time >= animationStart && event.time <= animationEnd) this.queue.event(entry, event);
		}
	}

	private eventsReverse (entry: TrackEntry, animationLast: number, animationTime: number) {
		const duration = entry.animation!.duration, from = duration - animationLast, to = duration - animationTime;
		const timelines = entry.animation!.timelines;
		for (let i = 0, n = entry.animation!.timelines.length; i < n; i++) {
			const eventTimeline = timelines[i];
			if (!(eventTimeline instanceof EventTimeline)) continue;
			const timelineEvents = eventTimeline.events;
			const frames = eventTimeline.frames;
			const frameCount = frames.length;
			if (from >= to) { // from -> to
				for (let ii = 0; ii < frameCount; ii++) {
					if (frames[ii] < to) continue;
					if (frames[ii] >= from) break;
					this.events.push(timelineEvents[ii]);
				}
			} else {
				for (let ii = 0; ii < frameCount; ii++) { // from -> 0
					if (frames[ii] >= from) break;
					this.events.push(timelineEvents[ii]);
				}
				let ii = 0; // end -> to
				for (; ii < frameCount; ii++)
					if (frames[ii] >= to) break;
				for (; ii < frameCount; ii++)
					this.events.push(timelineEvents[ii]);
			}
		}
	}

	/** Removes all animations from all tracks, leaving skeletons in their current pose.
	 *
	 * Usually you want to use {@link setEmptyAnimations} to mix the skeletons back to the setup pose, rather than leaving
	 * them in their current pose. */
	clearTracks () {
		const oldDrainDisabled = this.queue.drainDisabled;
		this.queue.drainDisabled = true;
		for (let i = 0, n = this.tracks.length; i < n; i++)
			this.clearTrack(i);
		this.tracks.length = 0;
		this.queue.drainDisabled = oldDrainDisabled;
		this.queue.drain();
	}

	/** Removes all animations from the track, leaving skeletons in their current pose.
	 *
	 * Usually you want to use {@link setEmptyAnimation} to mix the skeletons back to the setup pose, rather than
	 * leaving them in their current pose. */
	clearTrack (trackIndex: number) {
		if (trackIndex >= this.tracks.length) return;
		const current = this.tracks[trackIndex];
		if (!current) return;

		this.queue.end(current);

		this.clearNext(current);

		let entry = current;
		while (true) {
			const from = entry.mixingFrom;
			if (!from) break;
			this.queue.end(from);
			entry.mixingFrom = null;
			entry.mixingTo = null;
			entry = from;
		}

		this.tracks[current.trackIndex] = null;

		this.queue.drain();
	}

	setTrack (index: number, current: TrackEntry, interrupt: boolean) {
		const from = this.expandToIndex(index);
		this.tracks[index] = current;
		current.previous = null;

		if (from) {
			if (interrupt) this.queue.interrupt(from);
			current.mixingFrom = from;
			from.mixingTo = current;
			current.mixTime = 0;

			from.timelinesRotation.length = 0; // Reset rotation for mixing out, in case entry was mixed in.
		}

		this.queue.start(current);
	}

	/** Sets an animation by name.
	 *
	 * See {@link setAnimation}. */
	setAnimation (trackIndex: number, animationName: string, loop?: boolean): TrackEntry;

	/** Sets the current animation for a track, discarding any queued animations.
	 *
	 * If the formerly current track entry is for the same animation and was never applied to a skeleton, it is replaced (not mixed
	 * from).
	 * @param loop If true, the animation will repeat. If false it will not, instead its last frame is applied if played beyond its
	 *           duration. In either case {@link TrackEntry.trackEnd} determines when the track is cleared.
	 * @return A track entry to allow further customization of animation playback. References to the track entry must not be kept
	 *         after the {@link AnimationStateListener.dispose} event occurs. */
	setAnimation (trackIndex: number, animation: Animation, loop?: boolean): TrackEntry;

	setAnimation (trackIndex: number, animationNameOrAnimation: string | Animation, loop = false): TrackEntry {
		if (typeof animationNameOrAnimation === "string")
			return this.setAnimation1(trackIndex, animationNameOrAnimation, loop);
		return this.setAnimation2(trackIndex, animationNameOrAnimation, loop);
	}

	private setAnimation1 (trackIndex: number, animationName: string, loop: boolean = false) {
		const animation = this.data.skeletonData.findAnimation(animationName);
		if (!animation) throw new Error(`Animation not found: ${animationName}`);
		return this.setAnimation2(trackIndex, animation, loop);
	}

	/** Sets the current animation for a track, discarding any queued animations.
	 *
	 * If the formerly current track entry is for the same animation and was never applied to a skeleton, it is replaced (not mixed
	 * from).
	 * @param loop If true, the animation will repeat. If false it will not, instead its last frame is applied if played beyond its
	 *           duration. In either case {@link TrackEntry.getTrackEnd} determines when the track is cleared.
	 * @return A track entry to allow further customization of animation playback. References to the track entry must not be kept
	 *         after the {@link AnimationStateListener.dispose} event occurs. */
	private setAnimation2 (trackIndex: number, animation: Animation, loop: boolean = false) {
		if (trackIndex < 0) throw new Error("trackIndex must be >= 0.");
		if (!animation) throw new Error("animation cannot be null.");
		let interrupt = true;
		let current = this.expandToIndex(trackIndex);
		if (current) {
			if (current.nextTrackLast === -1 && current.animation === animation) {
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
		const entry = this.trackEntry(trackIndex, animation, loop, current);
		this.setTrack(trackIndex, entry, interrupt);
		this.queue.drain();
		return entry;
	}

	/** Queues an animation by name.
	 *
	 * See {@link addAnimation}. */
	addAnimation (trackIndex: number, animationName: string, loop?: boolean, delay?: number): TrackEntry;

	/** Adds an animation to be played after the current or last queued animation for a track. If the track has no entries, this is
	 * equivalent to calling {@link setAnimation}.
	 * @param delay If > 0, sets {@link TrackEntry.delay}. If <= 0, the delay set is the duration of the previous track entry
	 *           minus any mix duration (from {@link data}) plus the specified `delay` (ie the mix ends at (when
	 *           `delay` = 0) or before (when `delay` < 0) the previous track entry duration). If the
	 *           previous entry is looping, its next loop completion is used instead of its duration.
	 * @return A track entry to allow further customization of animation playback. References to the track entry must not be kept
	 *         after the {@link AnimationStateListener.dispose} event occurs. */
	addAnimation (trackIndex: number, animation: Animation, loop?: boolean, delay?: number): TrackEntry;

	addAnimation (trackIndex: number, animationNameOrAnimation: string | Animation, loop = false, delay: number = 0): TrackEntry {
		if (typeof animationNameOrAnimation === "string")
			return this.addAnimation1(trackIndex, animationNameOrAnimation, loop, delay);
		return this.addAnimation2(trackIndex, animationNameOrAnimation, loop, delay);
	}

	private addAnimation1 (trackIndex: number, animationName: string, loop: boolean = false, delay: number = 0) {
		const animation = this.data.skeletonData.findAnimation(animationName);
		if (!animation) throw new Error(`Animation not found: ${animationName}`);
		return this.addAnimation2(trackIndex, animation, loop, delay);
	}

	private addAnimation2 (trackIndex: number, animation: Animation, loop: boolean = false, delay: number = 0) {
		if (!animation) throw new Error("animation cannot be null.");

		let last = this.expandToIndex(trackIndex);
		if (last) {
			while (last.next)
				last = last.next;
		}

		const entry = this.trackEntry(trackIndex, animation, loop, last);

		if (!last) {
			this.setTrack(trackIndex, entry, true);
			this.queue.drain();
			if (delay < 0) delay = 0;
		} else {
			last.next = entry;
			entry.previous = last;
			if (delay <= 0) delay = Math.max(delay + last.getTrackComplete() - entry.mixDuration, 0);
		}

		entry.delay = delay;
		return entry;
	}

	/** Sets an empty animation for a track, discarding any queued animations, and sets the track entry's
	 * {@link TrackEntry.mixduration}. An empty animation has no timelines and serves as a placeholder for mixing in or out.
	 *
	 * Mixing out is done by setting an empty animation with a mix duration using either {@link setEmptyAnimation},
	 * {@link setEmptyAnimations}, or {@link addEmptyAnimation}. Mixing to an empty animation causes
	 * the previous animation to be applied less and less over the mix duration. Properties keyed in the previous animation
	 * transition to the value from lower tracks or to the setup pose value if no lower tracks key the property. A mix duration of
	 * 0 still needs to be applied one more time to mix out, so the properties it was animating are reverted.
	 *
	 * Mixing in is done by first setting an empty animation, then adding an animation using
	 * {@link addAnimation} with the desired delay (an empty animation has a duration of 0) and on
	 * the returned track entry, set the {@link TrackEntry.setMixDuration}. Mixing from an empty animation causes the new
	 * animation to be applied more and more over the mix duration. Properties keyed in the new animation transition from the value
	 * from lower tracks or from the setup pose value if no lower tracks key the property to the value keyed in the new animation.
	 *
	 * See <a href='https://esotericsoftware.com/spine-applying-animations#Empty-animations'>Empty animations</a> in the Spine
	 * Runtimes Guide. */
	setEmptyAnimation (trackIndex: number, mixDuration: number = 0) {
		const entry = this.setAnimation(trackIndex, AnimationState.emptyAnimation, false);
		entry.mixDuration = mixDuration;
		entry.trackEnd = mixDuration;
		return entry;
	}

	/** Adds an empty animation to be played after the current or last queued animation for a track, and sets the track entry's
	 * {@link TrackEntry.mixDuration}. If the track has no entries, it is equivalent to calling
	 * {@link setEmptyAnimation}.
	 *
	 * See {@link setEmptyAnimation} and
	 * <a href='https://esotericsoftware.com/spine-applying-animations#Empty-animations'>Empty animations</a> in the Spine Runtimes
	 * Guide.
	 * @param delay If > 0, sets {@link TrackEntry.delay}. If <= 0, the delay set is the duration of the previous track entry minus
	 *           any mix duration plus the specified `delay` (ie the mix ends at (when `delay` = 0) or before
	 *           (when `delay` < 0) the previous track entry duration). If the previous entry is looping, its next loop
	 *           completion is used instead of its duration.
	 * @return A track entry to allow further customization of animation playback. References to the track entry must not be kept
	 *         after the {@link AnimationStateListener.dispose} event occurs. */
	addEmptyAnimation (trackIndex: number, mixDuration: number = 0, delay: number = 0) {
		const entry = this.addAnimation(trackIndex, AnimationState.emptyAnimation, false, delay);
		if (delay <= 0) entry.delay = Math.max(entry.delay + entry.mixDuration - mixDuration, 0);
		entry.mixDuration = mixDuration;
		entry.trackEnd = mixDuration;
		return entry;
	}

	/** Sets an empty animation for every track, discarding any queued animations, and mixes to it over the specified mix duration.
	 *
	 * See <a href='https://esotericsoftware.com/spine-applying-animations#Empty-animations'>Empty animations</a> in the Spine
	 * Runtimes Guide. */
	setEmptyAnimations (mixDuration: number = 0) {
		const oldDrainDisabled = this.queue.drainDisabled;
		this.queue.drainDisabled = true;
		for (let i = 0, n = this.tracks.length; i < n; i++) {
			const current = this.tracks[i];
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
		const entry = this.trackEntryPool.obtain();
		entry.reset();
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
		entry.trackEnd = Number.MAX_VALUE;
		entry.timeScale = 1;

		entry.alpha = 1;
		entry.mixTime = 0;
		entry.mixDuration = !last ? 0 : this.data.getMix(last.animation!, animation);
		entry.totalAlpha = 0;
		entry.keepHold = false;
		return entry;
	}

	/** Removes {@link TrackEntry.next} and all entries after it for the specified entry. */
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

		const tracks = this.tracks;
		for (let i = 0, n = tracks.length; i < n; i++) {
			let entry = tracks[i];
			if (!entry) continue;
			while (entry.mixingFrom)
				entry = entry.mixingFrom;
			do {
				this.computeHold(entry);
				entry = entry.mixingTo;
			} while (entry);
		}
		this.propertyIds.clear();
	}

	computeHold (entry: TrackEntry) {
		const timelines = entry.animation!.timelines;
		const timelinesCount = entry.animation!.timelines.length;
		const timelineMode = entry.timelineMode;
		timelineMode.length = timelinesCount;
		const timelineHoldMix = entry.timelineHoldMix;
		timelineHoldMix.length = 0;
		const propertyIds = this.propertyIds;
		const add = entry.additive, keepHold = entry.keepHold;
		const to = entry.mixingTo;

		outer:
		for (let i = 0; i < timelinesCount; i++) {
			const timeline = timelines[i];
			const ids = timeline.propertyIds;
			const first = propertyIds.addAll(ids)
				&& !(timeline instanceof DrawOrderFolderTimeline && propertyIds.contains(DrawOrderTimeline.propertyID));

			if (add && timeline.additive) {
				timelineMode[i] = first ? FIRST : SUBSEQUENT;
				continue;
			}

			for (let from = entry.mixingFrom; from != null; from = from.mixingFrom) {
				if (from.animation!.hasTimeline(ids)) {
					// An earlier entry on this track keys this property, isolating it from lower tracks.
					timelineMode[i] = SUBSEQUENT;
					continue outer;
				}
			}

			// Hold if the next entry will overwrite this property.
			let mode: number;
			if (to === null || timeline.instant || (to.additive && timeline.additive) || !to.animation?.hasTimeline(ids))
				mode = first ? FIRST : SUBSEQUENT;
			else {
				mode = first ? HOLD_FIRST : HOLD;
				// Find next entry that doesn't overwrite this property. Its mix fades out the hold, instead of it ending abruptly.
				for (let next = to.mixingTo; next != null; next = next.mixingTo) {
					if ((next.additive && timeline.additive) || !next.animation?.hasTimeline(ids)) {
						if (next.mixDuration > 0) timelineHoldMix[i] = next;
						break;
					}
				}
			}
			if (keepHold) mode = (mode & ~HOLD) | (timelineMode[i] & HOLD);
			timelineMode[i] = mode;
		}
	}

	/** Returns the track entry for the animation currently playing on the track, or null if no animation is currently playing. */
	getTrack (trackIndex: number) {
		if (trackIndex < 0) throw new Error("trackIndex must be >= 0.");
		if (trackIndex >= this.tracks.length) return null;
		return this.tracks[trackIndex];
	}

	/** Returns the track entry for the animation currently playing on the track, or null if no animation is currently playing.
	 * @deprecated Use {@link getTrack}. */
	getCurrent (trackIndex: number) {
		return this.getTrack(trackIndex);
	}

	/** Adds a listener to receive events for all track entries. */
	addListener (listener: AnimationStateListener) {
		if (!listener) throw new Error("listener cannot be null.");
		this.listeners.push(listener);
	}

	/** Removes the listener added with {@link addListener}. */
	removeListener (listener: AnimationStateListener) {
		const index = this.listeners.indexOf(listener);
		if (index >= 0) this.listeners.splice(index, 1);
	}

	/** Removes all listeners added with {@link addListener}. */
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
 * References to a track entry must not be kept after the {@link AnimationStateListener.dispose} event occurs. */
export class TrackEntry {
	/** The animation to apply for this track entry. */
	animation: Animation | null = null;

	previous: TrackEntry | null = null;

	/** The animation queued to start after this animation, or null. `next` makes up a linked list. */
	next: TrackEntry | null = null;

	/** The track entry for the previous animation when mixing to this animation, or null if no mixing is currently occurring.
	 * When mixing from multiple animations, `mixingFrom` makes up a doubly linked list. */
	mixingFrom: TrackEntry | null = null;

	/** The track entry for the next animation when mixing from this animation, or null if no mixing is currently occurring.
	 * When mixing to multiple animations, `mixingTo` makes up a doubly linked list. */
	mixingTo: TrackEntry | null = null;

	/** The listener for events generated by this track entry, or null.
	 *
	 * A track entry returned from {@link AnimationState.setAnimation} is already the current animation
	 * for the track, so the callback for listener {@link AnimationStateListener.start} will not be called. */
	listener: AnimationStateListener | null = null;

	/** The index of the track where this track entry is either current or queued.
	 *
	 * See {@link AnimationState.getTrack}. */
	trackIndex = 0;

	/** If true, the animation will repeat. If false it will not, instead its last frame is applied if played beyond its
	 * duration. */
	loop = false;

	/** When true, timelines in this animation that support additive have their values added to the setup or current pose values
	 * instead of replacing them. Additive can be set for a new track entry only before {@link AnimationState.apply}
	 * is next called. */
	additive = false;

	/** If true, the animation will be applied in reverse. */
	reverse = false;

	/** If true, mixing rotation between tracks always uses the shortest rotation direction. If the rotation is animated, the
	 * shortest rotation direction may change during the mix.
	 *
	 * If false, the shortest rotation direction is remembered when the mix starts and the same direction is used for the rest
	 * of the mix. Defaults to false.
	 *
	 * See {@link resetRotationDirections}. */
	shortestRotation = false;

	keepHold = false;

	/** When the mix percentage ({@link mixTime} / {@link mixDuration}) is less than the `eventThreshold`, event
	 * timelines are applied while this animation is being mixed out. Defaults to 0, so event timelines are not applied while
	 * this animation is being mixed out. */
	eventThreshold = 0;

	/** When the mix percentage ({@link mixTime} / {@link mixDuration}) is less than the `mixAttachmentThreshold`,
	 * attachment timelines are applied while this animation is being mixed out. Defaults to 0, so attachment timelines are not
	 * applied while this animation is being mixed out. */
	mixAttachmentThreshold = 0;

	/** When the computed alpha is greater than `alphaAttachmentThreshold`, attachment timelines are applied. The
	 * computed alpha includes {@link alpha} and the mix percentage. Defaults to 0, so attachment timelines are always
	 * applied. */
	alphaAttachmentThreshold = 0;

	/** When the mix percentage ({@link mixTime} / {@link mixDuration}) is less than the `mixDrawOrderThreshold`,
	 * draw order timelines are applied while this animation is being mixed out. Defaults to 0, so draw order timelines are not
	 * applied while this animation is being mixed out. */
	mixDrawOrderThreshold = 0;

	/** The time in seconds for the first frame of this animation, both initially and after looping. Defaults to 0.
	 *
	 * When setting `animationStart` time, {@link animationLast} can be set to the same value to avoid firing events
	 * from the start of the animation. */
	animationStart = 0;

	/** The time in seconds for the last frame of this animation. Past this time, non-looping animations hold the pose at this
	 * time while looping animations will loop back to {@link animationStart}. Defaults to the {@link Animation.duration}. */
	animationEnd = 0;


	/** The time in seconds this animation was last applied. Some timelines use this for one-time triggers. For example, when
	 * this animation is applied, event timelines will fire all events between the `animationLast` time (exclusive)
	 * and `animationTime` (inclusive). Defaults to -1 to ensure triggers on frame 0 happen the first time this
	 * animation is applied. */
	animationLast = 0;

	nextAnimationLast = 0;

	/** Seconds to postpone playing the animation. Must be >= 0. When this track entry is the current track entry,
	 * `delay` postpones incrementing the {@link trackTime}. When this track entry is queued, `delay` is
	 * the time from the start of the previous animation to when this track entry will become the current track entry (ie when
	 * the previous track entry {@link trackTime} >= this track entry's `delay`).
	 *
	 * {@link timeScale} affects the delay.
	 *
	 * When passing `delay` <= 0 to {@link AnimationState.addAnimation} this
	 * `delay` is set using a mix duration from {@link AnimationStateData}. To change the {@link mixDuration}
	 * afterward, use {@link setMixDuration} so this `delay` is adjusted. */
	delay = 0;

	/** The time in seconds this track entry has been the current track entry, starting at 0 and increasing forever. Compare to
	 * {@link getAnimationTime}, which is always between {@link animationStart} and {@link animationEnd}.
	 *
	 * The track time can be set to start the animation at a time other than 0, without affecting looping. When doing so,
	 * {@link animationLast} can be set to the same value to avoid firing events from the start of the animation.
	 *
	 * To set the time an animation starts and loops, use {@link animationStart} and {@link animationEnd}. */
	trackTime = 0;

	trackLast = 0; nextTrackLast = 0;

	/** The track time in seconds when this animation will be removed from the track. Defaults to the highest possible float
	 * value, meaning the animation will be applied until a new animation is set or the track is cleared. If the track end time
	 * is reached, no other animations are queued for playback, and mixing from any previous animations is complete, then the
	 * properties keyed by the animation are set to the setup pose and the track is cleared.
	 *
	 * Usually you want to use {@link AnimationState.addEmptyAnimation} rather than have the animation
	 * abruptly cease being applied. */
	trackEnd = 0;

	/** Multiplier for the delta time when this track entry is updated, causing time for this animation to pass slower or
	 * faster. Defaults to 1.
	 *
	 * Values < 0 are not supported. To play an animation in reverse, use {@link reverse}.
	 *
	 * {@link mixTime} is not affected by track entry time scale, so {@link mixDuration} may need to be adjusted to match the
	 * animation speed.
	 *
	 * When using {@link AnimationState.addAnimation} with a `delay` <= 0, the
	 * {@link delay} is set using the mix duration from {@link AnimationState.data}, assuming time scale to be 1. If the time
	 * scale is not 1, the delay may need to be adjusted.
	 *
	 * See {@link AnimationState.timeScale} to affect all animations. */
	timeScale = 0;

	/** Values < 1 mix this animation with the skeleton's current pose (either the setup pose or the pose from lower tracks).
	 * Defaults to 1, which overwrites the skeleton's current pose with this animation.
	 *
	 * Alpha should be 1 on track 0.
	 *
	 * See {@link getAlphaAttachmentThreshold}. */
	alpha = 0;

	/** Seconds elapsed from 0 to the {@link mixDuration} when mixing from the previous animation to this animation. May
	 * be slightly more than `mixDuration` when the mix is complete. */
	mixTime = 0;

	/** Seconds for mixing from the previous animation to this animation. Defaults to the value provided by
	 * {@link AnimationStateData.getMix} based on the animation before this animation (if any).
	 *
	 * A mix duration of 0 still needs to be applied one more time to mix out, so the the properties it was animating are
	 * reverted. A mix duration of 0 can be set at any time to end the mix on the next
	 * {@link AnimationState.update | update}.
	 *
	 * The `mixDuration` can be set manually rather than use the value from
	 * {@link AnimationStateData.getMix}. In that case, the `mixDuration` can be set for a new
	 * track entry only before {@link AnimationState.update} is next called.
	 *
	 * When using {@link AnimationState.addAnimation} with a `delay` <= 0, the
	 * {@link getDelay} is set using the mix duration from {@link AnimationState.data}. If `mixDuration` is set
	 * afterward, the delay needs to be adjusted:
	 *
	 * <pre>
	 * entry.mixDuration = 0.25;<br>
	 * entry.delay = entry.previous.getTrackComplete() - entry.mixDuration + 0;
	 * </pre>
	 *
	 * Alternatively, use {@link setMixDuration} to set both the mix duration and recompute the delay:<br>
	 *
	 * <pre>
	  entry.setMixDuration(0.25f, 0); // mixDuration, delay
	 * </pre>
	 */
	mixDuration = 0;

	totalAlpha = 0;

	/** Sets both {@link getMixDuration} and {@link getDelay}.
	 * @param delay If > 0, sets {@link getDelay}. If <= 0, the delay set is the duration of the previous track entry minus
	 *           the specified mix duration plus the specified `delay` (ie the mix ends at (when `delay` =
	 *           0) or before (when `delay` < 0) the previous track entry duration). If the previous entry is
	 *           looping, its next loop completion is used instead of its duration. */
	setMixDuration (mixDuration: number, delay?: number) {
		this.mixDuration = mixDuration;
		if (delay !== undefined) {
			if (delay <= 0) delay = this.previous == null ? 0 : Math.max(delay + this.previous.getTrackComplete() - mixDuration, 0);
			this.delay = delay;
		}
	}

	/** For each timeline:
	 * - Bit 0, FIRST: 0 = mix from current pose, 1 = mix from setup pose. Timeline is first to set the property.
	 * - Bit 1, HOLD: 0 = mix out using alphaMix, 1 = apply full alpha to prevent dipping. Timeline is first on its track to
	 * set the property and the next entry (mixingTo) also sets it. When held, timelineHoldMix's mix controls how the hold fades
	 * out (for 3+ entry chains where the chain eventually stops setting the property). */
	timelineMode = [] as number[];
	timelineHoldMix = [] as TrackEntry[];
	timelinesRotation = [] as number[];

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

	/** Uses {@link trackTime} to compute the `animationTime`, which is always between {@link animationStart} and
	 * {@link animationEnd}. When `trackTime` is 0, `animationTime` is equal to the
	 * `animationStart` time. */
	getAnimationTime () {
		if (!this.loop) return Math.min(this.trackTime + this.animationStart, this.animationEnd);
		const duration = this.animationEnd - this.animationStart;
		if (duration === 0) return this.animationStart;
		return (this.trackTime % duration) + this.animationStart;
	}

	setAnimationLast (animationLast: number) {
		this.animationLast = animationLast;
		this.nextAnimationLast = animationLast;
	}

	/** Returns true if at least one loop has been completed.
	 *
	 * See {@link AnimationStateListener.complete}. */
	isComplete () {
		return this.trackTime >= this.animationEnd - this.animationStart;
	}

	/** When {@link shortestRotation} is false, this clears the directions for mixing this entry's rotation. This can be useful
	 * to avoid bones rotating the long way around when using {@link getAlpha} and starting animations on other tracks.
	 *
	 * Mixing involves finding a rotation between two others. There are two possible solutions: the short or the long way
	 * around. When the two rotations change over time, which direction is the short or long way can also change. If the short
	 * way was always chosen, bones flip to the other side when that direction became the long way. TrackEntry chooses the short
	 * way the first time it is applied and remembers that direction. Resetting that direction makes it choose a new short way
	 * on the next apply. */
	resetRotationDirections () {
		this.timelinesRotation.length = 0;
	}

	/** If this track entry is non-looping, this is the track time in seconds when {@link animationEnd} is reached, or the
	 * current {@link trackTime} if it has already been reached.
	 *
	 * If this track entry is looping, this is the track time when this animation will reach its next {@link animationEnd} (the
	 * next loop completion). */
	getTrackComplete () {
		const duration = this.animationEnd - this.animationStart;
		if (duration !== 0) {
			if (this.loop) return duration * (1 + ((this.trackTime / duration) | 0)); // Completion of next loop.
			if (this.trackTime < duration) return duration; // Before duration.
		}
		return this.trackTime; // Next update.
	}

	/** Returns true if this track entry has been applied at least once.
	 *
	 * See {@link AnimationState.apply}. */
	wasApplied () {
		return this.nextTrackLast !== -1;
	}

	/** Returns true if there is a {@link next} track entry and it will become the current track entry during the next
	 * {@link AnimationState.update}. */
	isNextReady () {
		return this.next != null && this.nextTrackLast - this.next.delay >= 0;
	}
}

export class EventQueue {
	objects: Array<EventType | TrackEntry | Event> = [];
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
		if (this.drainDisabled) return; // Not reentrant.
		this.drainDisabled = true;

		const listeners = this.animState.listeners;

		for (let i = 0; i < this.objects.length; i += 2) {
			const objects = this.objects;
			const type = objects[i] as EventType;
			const entry = objects[i + 1] as TrackEntry;
			switch (type) {
				case EventType.start:
					if (entry.listener?.start) entry.listener.start(entry);
					for (let ii = 0; ii < listeners.length; ii++) {
						const listener = listeners[ii];
						if (listener.start) listener.start(entry);
					}
					break;
				case EventType.interrupt:
					if (entry.listener?.interrupt) entry.listener.interrupt(entry);
					for (let ii = 0; ii < listeners.length; ii++) {
						const listener = listeners[ii];
						if (listener.interrupt) listener.interrupt(entry);
					}
					break;
				// biome-ignore lint/suspicious/noFallthroughSwitchClause: reference runtime does fall through
				case EventType.end:
					if (entry.listener?.end) entry.listener.end(entry);
					for (let ii = 0; ii < listeners.length; ii++) {
						const listener = listeners[ii];
						if (listener.end) listener.end(entry);
					}
				// Fall through.
				case EventType.dispose:
					if (entry.listener?.dispose) entry.listener.dispose(entry);
					for (let ii = 0; ii < listeners.length; ii++) {
						const listener = listeners[ii];
						if (listener.dispose) listener.dispose(entry);
					}
					this.animState.trackEntryPool.free(entry);
					break;
				case EventType.complete:
					if (entry.listener?.complete) entry.listener.complete(entry);
					for (let ii = 0; ii < listeners.length; ii++) {
						const listener = listeners[ii];
						if (listener.complete) listener.complete(entry);
					}
					break;
				case EventType.event: {
					const event = objects[i++ + 2] as Event;
					if (entry.listener?.event) entry.listener.event(entry, event);
					for (let ii = 0; ii < listeners.length; ii++) {
						const listener = listeners[ii];
						if (listener.event) listener.event(entry, event);
					}
					break;
				}
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
 * TrackEntry events are collected during {@link AnimationState.update} and {@link AnimationState.apply} and
 * fired only after those methods are finished.
 *
 * See {@link TrackEntry.listener} and
 * {@link AnimationState.addListener}. */
export interface AnimationStateListener {
	/** Invoked when this entry has been set as the current entry. {@link end} will occur when this entry will no
	 * longer be applied.
	 *
	 * When this event is triggered by calling {@link AnimationState.setAnimation}, take care not to
	 * call {@link AnimationState.update} until after the TrackEntry has been configured. */
	start?: (entry: TrackEntry) => void;

	/** Invoked when another entry has replaced this entry as the current entry. This entry may continue being applied for
	 * mixing. */
	interrupt?: (entry: TrackEntry) => void;

	/** Invoked when this entry will never be applied again. This only occurs if this entry has previously been set as the
	 * current entry ({@link start} was invoked). */
	end?: (entry: TrackEntry) => void;

	/** Invoked when this entry will be disposed. This may occur without the entry ever being set as the current entry.
	 * References to the entry should not be kept after dispose is called, as it may be destroyed or reused. */
	dispose?: (entry: TrackEntry) => void;

	/** Invoked every time this entry's animation completes a loop. This may occur during mixing (after
	 * {@link interrupt}).
	 *
	 * If this entry's {@link TrackEntry.mixingTo} is not null, this entry is mixing out (it is not the current entry).
	 *
	 * Because this event is triggered at the end of {@link AnimationState.apply}, any animations set in response to
	 * the event won't be applied until the next time the AnimationState is applied. */
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

export const SUBSEQUENT = 0;
export const FIRST = 1;
export const HOLD = 2;
export const HOLD_FIRST = 3;

export const SETUP = 1;
export const CURRENT = 2;
