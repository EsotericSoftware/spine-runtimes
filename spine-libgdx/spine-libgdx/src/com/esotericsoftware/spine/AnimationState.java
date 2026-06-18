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

package com.esotericsoftware.spine;

import com.badlogic.gdx.math.Interpolation;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;
import com.badlogic.gdx.utils.IntArray;
import com.badlogic.gdx.utils.LongMap;
import com.badlogic.gdx.utils.Null;
import com.badlogic.gdx.utils.Pool;
import com.badlogic.gdx.utils.Pool.Poolable;
import com.badlogic.gdx.utils.SnapshotArray;

import com.esotericsoftware.spine.Animation.AttachmentTimeline;
import com.esotericsoftware.spine.Animation.DrawOrderFolderTimeline;
import com.esotericsoftware.spine.Animation.DrawOrderTimeline;
import com.esotericsoftware.spine.Animation.EventTimeline;
import com.esotericsoftware.spine.Animation.MixFrom;
import com.esotericsoftware.spine.Animation.RotateTimeline;
import com.esotericsoftware.spine.Animation.Timeline;

/** Applies animations over time, queues animations for later playback, mixes (crossfading) between animations, and applies
 * multiple animations on top of each other (layering).
 * <p>
 * See <a href='https://esotericsoftware.com/spine-applying-animations#AnimationState-API'>Applying Animations</a> in the Spine
 * Runtimes Guide. */
public class AnimationState {
	static private final int CURRENT = 0, SETUP = 1, FIRST = 2, MODE = 3, HOLD = 4, ATTACH_SETUP = 1, ATTACH_RETAIN = 2;

	static final Animation emptyAnimation = new Animation("<empty>");
	static {
		emptyAnimation.setTimelines(new Array(true, 0, Timeline[]::new), new IntArray(0));
	}

	private AnimationStateData data;
	final Array<TrackEntry> tracks = new Array(true, 4, TrackEntry[]::new);
	private final Array<Event> events = new Array(true, 4, Event[]::new);
	final SnapshotArray<AnimationStateListener> listeners = new SnapshotArray(true, 16, AnimationStateListener[]::new);
	private final EventQueue queue = new EventQueue();
	private final LongMap<TrackEntry> propertyIds = new LongMap();
	boolean animationsChanged;
	private float timeScale = 1;
	private int unkeyedState;

	final Pool<TrackEntry> trackEntryPool = new Pool() {
		protected Object newObject () {
			return new TrackEntry();
		}
	};

	/** Creates an uninitialized AnimationState. The animation state data must be set before use. */
	public AnimationState () {
	}

	public AnimationState (AnimationStateData data) {
		if (data == null) throw new IllegalArgumentException("data cannot be null.");
		this.data = data;
	}

	/** Increments each track entry {@link TrackEntry#trackTime}, setting queued animations as current if needed. */
	public void update (float delta) {
		delta *= timeScale;
		TrackEntry[] tracks = this.tracks.items;
		for (int i = 0, n = this.tracks.size; i < n; i++) {
			TrackEntry current = tracks[i];
			if (current == null) continue;

			current.animationLast = current.nextAnimationLast;
			current.trackLast = current.nextTrackLast;

			float currentDelta = delta * current.timeScale;

			if (current.delay > 0) {
				current.delay -= currentDelta;
				if (current.delay > 0) continue;
				currentDelta = -current.delay;
				current.delay = 0;
			}

			TrackEntry next = current.next;
			if (next != null) {
				// When the next entry's delay is passed, change to the next entry, preserving leftover time.
				float nextTime = current.trackLast - next.delay;
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
				TrackEntry from = current.mixingFrom;
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

	/** Returns true when all mixing from entries are complete. */
	private boolean updateMixingFrom (TrackEntry to, float delta) {
		TrackEntry from = to.mixingFrom;
		if (from == null) return true;

		boolean finished = updateMixingFrom(from, delta);

		from.animationLast = from.nextAnimationLast;
		from.trackLast = from.nextTrackLast;

		// The from entry was applied at least once and the mix is complete.
		if (to.nextTrackLast != -1 && to.mixTime >= to.mixDuration) {
			// Mixing is complete for all entries before the from entry or the mix is instantaneous.
			if (from.totalAlpha == 0 || to.mixDuration == 0) {
				to.mixingFrom = from.mixingFrom;
				if (from.mixingFrom != null) from.mixingFrom.mixingTo = to;
				if (from.totalAlpha == 0) {
					for (TrackEntry next = to; next.mixingTo != null; next = next.mixingTo)
						next.keepHold = true;
				}
				queue.end(from);
			}
			return finished;
		}

		from.trackTime += delta * from.timeScale;
		to.mixTime += delta;
		return false;
	}

	/** Poses the skeleton using the track entry animations. The animation state is not changed, so can be applied to multiple
	 * skeletons to pose them identically.
	 * @return True if any animations were applied. */
	public boolean apply (Skeleton skeleton) {
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");
		if (animationsChanged) animationsChanged();

		Array<Event> events = this.events;
		boolean applied = false;
		TrackEntry[] tracks = this.tracks.items;
		for (int i = 0, n = this.tracks.size; i < n; i++) {
			TrackEntry current = tracks[i];
			if (current == null || current.delay > 0) continue;
			applied = true;

			// Apply mixing from entries first.
			float alpha = current.alpha;
			if (current.mixingFrom != null)
				alpha *= applyMixingFrom(current, skeleton);
			else if (current.trackTime >= current.trackEnd && current.next == null) //
				alpha = 0; // Set to setup pose the last time the entry will be applied.

			// Apply current entry.
			float animationLast = current.animationLast, animationTime = current.getAnimationTime(), applyTime = animationTime;
			Array<Event> applyEvents = events;
			if (current.reverse) {
				applyTime = current.animation.duration - applyTime;
				applyEvents = null;
			}
			int timelineCount = current.animation.timelines.size;
			Timeline[] timelines = current.animation.timelines.items;
			if (i == 0 && alpha == 1) {
				for (int ii = 0; ii < timelineCount; ii++) {
					Timeline timeline = timelines[ii];
					if (timeline instanceof AttachmentTimeline attachmentTimeline)
						applyAttachmentTimeline(attachmentTimeline, skeleton, applyTime, MixFrom.setup, true);
					else
						timeline.apply(skeleton, animationLast, applyTime, applyEvents, alpha, MixFrom.setup, false, false, false);
				}
			} else {
				int[] timelineMode = current.timelineMode.items;
				boolean retainAttachments = alpha >= current.alphaAttachmentThreshold;
				boolean add = current.additive, shortestRotation = add || current.shortestRotation;
				boolean firstFrame = !shortestRotation && current.timelinesRotation.size != timelineCount << 1;
				float[] timelinesRotation = firstFrame ? current.timelinesRotation.setSize(timelineCount << 1)
					: current.timelinesRotation.items;
				for (int ii = 0; ii < timelineCount; ii++) {
					Timeline timeline = timelines[ii];
					MixFrom from = MixFrom.values[timelineMode[ii] & MODE];
					if (!shortestRotation && timeline instanceof RotateTimeline rotateTimeline)
						applyRotateTimeline(rotateTimeline, skeleton, applyTime, alpha, from, timelinesRotation, ii << 1, firstFrame);
					else if (timeline instanceof AttachmentTimeline attachmentTimeline)
						applyAttachmentTimeline(attachmentTimeline, skeleton, applyTime, from, retainAttachments);
					else
						timeline.apply(skeleton, animationLast, applyTime, applyEvents, alpha, from, add, false, false);
				}
			}
			if (current.reverse) eventsReverse(current, animationLast, animationTime);
			queueEvents(current, animationTime);
			events.clear();
			current.nextAnimationLast = animationTime;
			current.nextTrackLast = current.trackTime;
		}

		// Set slot attachments to the setup pose if they were set temporarily to apply deform timelines.
		int setupState = unkeyedState + ATTACH_SETUP;
		Slot[] slots = skeleton.slots.items;
		for (int i = 0, n = skeleton.slots.size; i < n; i++) {
			var slot = slots[i];
			if (slot.attachmentState == setupState) {
				String attachmentName = slot.data.attachmentName;
				slot.pose.setAttachment(attachmentName == null ? null : skeleton.getAttachment(slot.data.index, attachmentName));
			}
		}
		unkeyedState += 2; // Reset.

		queue.drain();
		return applied;
	}

	private float applyMixingFrom (TrackEntry to, Skeleton skeleton) {
		TrackEntry from = to.mixingFrom;
		float fromMix = from.mixingFrom != null ? applyMixingFrom(from, skeleton) : 1;
		float mix = to.mix();

		float a = from.alpha * fromMix, keep = 1 - mix * to.alpha;
		float alphaMix = a * (1 - mix), alphaHold = keep > 0 ? alphaMix / keep : a;

		int timelineCount = from.animation.timelines.size;
		Timeline[] timelines = from.animation.timelines.items;
		int[] timelineMode = from.timelineMode.items;
		TrackEntry[] timelineHoldMix = from.timelineHoldMix.items;

		boolean retainAttachments = mix < from.mixAttachmentThreshold, drawOrder = mix < from.mixDrawOrderThreshold;
		boolean add = from.additive, shortestRotation = add || from.shortestRotation;
		boolean firstFrame = !shortestRotation && from.timelinesRotation.size != timelineCount << 1;
		float[] timelinesRotation = firstFrame ? from.timelinesRotation.setSize(timelineCount << 1) : from.timelinesRotation.items;

		float animationLast = from.animationLast, animationTime = from.getAnimationTime(), applyTime = animationTime;
		Array<Event> events = null;
		if (from.reverse)
			applyTime = from.animation.duration - applyTime;
		else if (mix < from.eventThreshold) //
			events = this.events;

		from.totalAlpha = 0;
		for (int i = 0; i < timelineCount; i++) {
			Timeline timeline = timelines[i];
			int mode = timelineMode[i];
			MixFrom mixFrom = MixFrom.values[mode & MODE];
			float alpha;
			if ((mode & HOLD) != 0) {
				TrackEntry holdMix = timelineHoldMix[i];
				alpha = holdMix == null ? alphaHold : alphaHold * (1 - holdMix.mix());
			} else {
				if (!drawOrder && timeline instanceof DrawOrderTimeline && mixFrom == MixFrom.current) continue;
				alpha = alphaMix;
			}
			from.totalAlpha += alpha;
			if (!shortestRotation && timeline instanceof RotateTimeline rotateTimeline) {
				applyRotateTimeline(rotateTimeline, skeleton, applyTime, alpha, mixFrom, timelinesRotation, i << 1, firstFrame);
			} else if (timeline instanceof AttachmentTimeline attachmentTimeline)
				applyAttachmentTimeline(attachmentTimeline, skeleton, applyTime, mixFrom,
					retainAttachments && alpha >= from.alphaAttachmentThreshold);
			else {
				boolean out = !drawOrder || !(timeline instanceof DrawOrderTimeline) || mixFrom == MixFrom.current;
				timeline.apply(skeleton, animationLast, applyTime, events, alpha, mixFrom, add, out, false);
			}
		}

		if (from.reverse && mix < from.eventThreshold) eventsReverse(from, animationLast, animationTime);
		if (to.mixDuration > 0) queueEvents(from, animationTime);
		this.events.clear();

		from.nextAnimationLast = animationTime;
		from.nextTrackLast = from.trackTime;
		return mix;
	}

	/** Applies the attachment timeline and sets {@link Slot#attachmentState}.
	 * @param retain True if the attachment remains after apply, false if temporary for deform timelines. */
	private void applyAttachmentTimeline (AttachmentTimeline timeline, Skeleton skeleton, float time, MixFrom from,
		boolean retain) {

		Slot slot = skeleton.slots.items[timeline.slotIndex];
		if (!slot.bone.active) return;
		if (!retain && slot.attachmentState == unkeyedState + ATTACH_RETAIN) return;

		boolean setup = time < timeline.frames[0];
		String name = null;
		if (!setup) {
			name = timeline.attachmentNames[Timeline.search(timeline.frames, time)];
			setup = !retain && name == null;
		}
		if (setup) {
			if (from == MixFrom.current) return;
			name = slot.data.attachmentName;
		}
		slot.pose.setAttachment(name == null ? null : skeleton.getAttachment(slot.data.index, name));
		if (retain)
			slot.attachmentState = unkeyedState + ATTACH_RETAIN;
		else if (!setup) //
			slot.attachmentState = unkeyedState + ATTACH_SETUP;
	}

	/** Applies the rotate timeline, mixing with the current pose while keeping the same rotation direction chosen as the shortest
	 * the first time the mixing was applied. */
	private void applyRotateTimeline (RotateTimeline timeline, Skeleton skeleton, float time, float alpha, MixFrom from,
		float[] timelinesRotation, int i, boolean firstFrame) {

		if (firstFrame) timelinesRotation[i] = 0;

		if (alpha == 1) {
			timeline.apply(skeleton, 0, time, null, 1, from, false, false, false);
			return;
		}

		Bone bone = skeleton.bones.items[timeline.boneIndex];
		if (!bone.active) return;
		BonePose pose = bone.pose, setup = bone.data.setupPose;
		float[] frames = timeline.frames;
		float r1, r2;
		if (time < frames[0]) {
			switch (from) {
			case setup -> {
				pose.rotation = setup.rotation;
				return;
			}
			case current -> {
				return;
			}
			}
			r1 = pose.rotation;
			r2 = setup.rotation;
		} else {
			r1 = from == MixFrom.setup ? setup.rotation : pose.rotation;
			r2 = setup.rotation + timeline.getCurveValue(time);
		}

		// Mix between rotations using the direction of the shortest route on the first frame.
		float total, diff = r2 - r1;
		diff -= (float)Math.ceil(diff / 360 - 0.5f) * 360;
		if (diff == 0)
			total = timelinesRotation[i];
		else {
			float lastTotal, lastDiff;
			if (firstFrame) {
				lastTotal = 0;
				lastDiff = diff;
			} else {
				lastTotal = timelinesRotation[i];
				lastDiff = timelinesRotation[i + 1];
			}
			float loops = lastTotal - lastTotal % 360;
			total = diff + loops;
			boolean current = diff >= 0, dir = lastTotal >= 0;
			if (Math.abs(lastDiff) <= 90 && Math.signum(lastDiff) != Math.signum(diff)) {
				if (Math.abs(lastTotal - loops) > 180) {
					total += 360 * Math.signum(lastTotal);
					dir = current;
				} else if (loops != 0)
					total -= 360 * Math.signum(lastTotal);
				else
					dir = current;
			}
			if (dir != current) total += 360 * Math.signum(lastTotal);
			timelinesRotation[i] = total;
		}
		timelinesRotation[i + 1] = diff;
		pose.rotation = r1 + total * alpha;
	}

	private void queueEvents (TrackEntry entry, float animationTime) {
		float animationStart = entry.animationStart, animationEnd = entry.animationEnd, duration = animationEnd - animationStart;
		boolean reverse = entry.reverse;
		float split = entry.trackLast % duration;
		if (reverse) split = duration - split;

		// Queue events before complete.
		Event[] events = this.events.items;
		int i = 0, n = this.events.size;
		for (; i < n; i++) {
			Event event = events[i];
			if (event.time < split ^ reverse) break;
			if (event.time >= animationStart && event.time <= animationEnd) queue.event(entry, event);
		}

		// Queue complete if completed a loop iteration or the animation.
		boolean complete;
		if (entry.loop) {
			if (duration == 0)
				complete = true;
			else {
				int cycles = (int)(entry.trackTime / duration);
				complete = cycles > 0 && cycles > (int)(entry.trackLast / duration);
			}
		} else
			complete = animationTime >= animationEnd && entry.animationLast < animationEnd;
		if (complete) queue.complete(entry);

		// Queue events after complete.
		for (; i < n; i++) {
			Event event = events[i];
			if (event.time >= animationStart && event.time <= animationEnd) queue.event(entry, event);
		}
	}

	private void eventsReverse (TrackEntry entry, float animationLast, float animationTime) {
		float duration = entry.animation.duration, from = duration - animationLast, to = duration - animationTime;
		Timeline[] timelines = entry.animation.timelines.items;
		for (int i = 0, n = entry.animation.timelines.size; i < n; i++) {
			if (!(timelines[i] instanceof EventTimeline eventTimeline)) continue;
			Event[] timelineEvents = eventTimeline.getEvents();
			float[] frames = eventTimeline.frames;
			int frameCount = frames.length;
			if (from >= to) { // from -> to
				for (int ii = 0; ii < frameCount; ii++) {
					if (frames[ii] < to) continue;
					if (frames[ii] >= from) break;
					events.add(timelineEvents[ii]);
				}
			} else {
				for (int ii = 0; ii < frameCount; ii++) { // from -> 0
					if (frames[ii] >= from) break;
					events.add(timelineEvents[ii]);
				}
				int ii = 0; // end -> to
				for (; ii < frameCount; ii++)
					if (frames[ii] >= to) break;
				for (; ii < frameCount; ii++)
					events.add(timelineEvents[ii]);
			}
		}
	}

	/** Removes all animations from all tracks, leaving skeletons in their current pose.
	 * <p>
	 * Usually you want to use {@link #setEmptyAnimations(float)} to mix the skeletons back to the setup pose, rather than leaving
	 * them in their current pose. */
	public void clearTracks () {
		boolean oldDrainDisabled = queue.drainDisabled;
		queue.drainDisabled = true;
		for (int i = 0, n = tracks.size; i < n; i++)
			clearTrack(i);
		tracks.clear();
		queue.drainDisabled = oldDrainDisabled;
		queue.drain();
	}

	/** Removes all animations from the track, leaving skeletons in their current pose.
	 * <p>
	 * Usually you want to use {@link #setEmptyAnimation(int, float)} to mix the skeletons back to the setup pose, rather than
	 * leaving them in their current pose. */
	public void clearTrack (int trackIndex) {
		if (trackIndex < 0) throw new IllegalArgumentException("trackIndex must be >= 0.");
		if (trackIndex >= tracks.size) return;
		TrackEntry current = tracks.items[trackIndex];
		if (current == null) return;

		queue.end(current);

		clearNext(current);

		TrackEntry entry = current;
		while (true) {
			TrackEntry from = entry.mixingFrom;
			if (from == null) break;
			queue.end(from);
			entry.mixingFrom = null;
			entry.mixingTo = null;
			entry = from;
		}

		tracks.items[current.trackIndex] = null;

		queue.drain();
	}

	private void setTrack (int index, TrackEntry current, boolean interrupt) {
		TrackEntry from = expandToIndex(index);
		tracks.items[index] = current;
		current.previous = null;

		if (from != null) {
			from.next = null;
			if (interrupt) queue.interrupt(from);
			current.mixingFrom = from;
			from.mixingTo = current;
			current.mixTime = 0;
			from.timelinesRotation.clear(); // Reset rotation for mixing out, in case entry was mixed in.
		}

		queue.start(current);
	}

	/** Sets an animation by name.
	 * <p>
	 * See {@link #setAnimation(int, Animation, boolean)}. */
	public TrackEntry setAnimation (int trackIndex, String animationName, boolean loop) {
		Animation animation = data.skeletonData.findAnimation(animationName);
		if (animation == null) throw new IllegalArgumentException("Animation not found: " + animationName);
		return setAnimation(trackIndex, animation, loop);
	}

	/** Sets the current animation for a track, discarding any queued animations.
	 * <p>
	 * If the formerly current track entry is for the same animation and was never applied to a skeleton, it is replaced (not mixed
	 * from).
	 * @param loop If true, the animation will repeat. If false it will not, instead its last frame is applied if played beyond its
	 *           duration. In either case {@link TrackEntry#trackEnd} determines when the track is cleared.
	 * @return A track entry to allow further customization of animation playback. References to the track entry must not be kept
	 *         after the {@link AnimationStateListener#dispose(TrackEntry)} event occurs. */
	public TrackEntry setAnimation (int trackIndex, Animation animation, boolean loop) {
		if (trackIndex < 0) throw new IllegalArgumentException("trackIndex must be >= 0.");
		if (animation == null) throw new IllegalArgumentException("animation cannot be null.");
		boolean interrupt = true;
		TrackEntry current = expandToIndex(trackIndex);
		if (current != null) {
			if (current.nextTrackLast == -1 && current.animation == animation) {
				// Don't mix from an entry that was never applied.
				tracks.items[trackIndex] = current.mixingFrom;
				queue.interrupt(current);
				queue.end(current);
				clearNext(current);
				current = current.mixingFrom;
				interrupt = false; // mixingFrom is current again, but don't interrupt it twice.
			} else
				clearNext(current);
		}
		TrackEntry entry = trackEntry(trackIndex, animation, loop, current);
		setTrack(trackIndex, entry, interrupt);
		queue.drain();
		return entry;
	}

	/** Queues an animation by name.
	 * <p>
	 * See {@link #addAnimation(int, Animation, boolean, float)}. */
	public TrackEntry addAnimation (int trackIndex, String animationName, boolean loop, float delay) {
		Animation animation = data.skeletonData.findAnimation(animationName);
		if (animation == null) throw new IllegalArgumentException("Animation not found: " + animationName);
		return addAnimation(trackIndex, animation, loop, delay);
	}

	/** Adds an animation to be played after the current or last queued animation for a track. If the track has no entries, this is
	 * equivalent to calling {@link #setAnimation(int, Animation, boolean)}.
	 * @param delay If > 0, sets {@link TrackEntry#delay}. If <= 0, the delay set is the duration of the previous track entry minus
	 *           any mix duration (from {@link #data}) plus the specified <code>delay</code> (ie the mix ends at (when
	 *           <code>delay</code> = 0) or before (when <code>delay</code> < 0) the previous track entry duration). If the
	 *           previous entry is looping, its next loop completion is used instead of its duration.
	 * @return A track entry to allow further customization of animation playback. References to the track entry must not be kept
	 *         after the {@link AnimationStateListener#dispose(TrackEntry)} event occurs. */
	public TrackEntry addAnimation (int trackIndex, Animation animation, boolean loop, float delay) {
		if (trackIndex < 0) throw new IllegalArgumentException("trackIndex must be >= 0.");
		if (animation == null) throw new IllegalArgumentException("animation cannot be null.");

		TrackEntry last = expandToIndex(trackIndex);
		if (last != null) {
			while (last.next != null)
				last = last.next;
		}

		TrackEntry entry = trackEntry(trackIndex, animation, loop, last);

		if (last == null) {
			setTrack(trackIndex, entry, true);
			queue.drain();
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
	 * {@link TrackEntry#mixDuration}. An empty animation has no timelines and serves as a placeholder for mixing in or out.
	 * <p>
	 * Mixing out is done by setting an empty animation with a mix duration using either {@link #setEmptyAnimation(int, float)},
	 * {@link #setEmptyAnimations(float)}, or {@link #addEmptyAnimation(int, float, float)}. Mixing to an empty animation causes
	 * the previous animation to be applied less and less over the mix duration. Properties keyed in the previous animation
	 * transition to the value from lower tracks or to the setup pose value if no lower tracks key the property. A mix duration of
	 * 0 still needs to be applied one more time to mix out, so the properties it was animating are reverted.
	 * <p>
	 * Mixing in is done by first setting an empty animation, then adding an animation using
	 * {@link #addAnimation(int, Animation, boolean, float)} with the desired delay (an empty animation has a duration of 0) and on
	 * the returned track entry set {@link TrackEntry#setMixDuration(float)}. Mixing from an empty animation causes the new
	 * animation to be applied more and more over the mix duration. Properties keyed in the new animation transition from the value
	 * from lower tracks or from the setup pose value if no lower tracks key the property to the value keyed in the new animation.
	 * <p>
	 * See <a href='https://esotericsoftware.com/spine-applying-animations#Empty-animations'>Empty animations</a> in the Spine
	 * Runtimes Guide. */
	public TrackEntry setEmptyAnimation (int trackIndex, float mixDuration) {
		TrackEntry entry = setAnimation(trackIndex, emptyAnimation, false);
		entry.mixDuration = mixDuration;
		entry.trackEnd = mixDuration;
		return entry;
	}

	/** Adds an empty animation to be played after the current or last queued animation for a track, and sets the track entry's
	 * {@link TrackEntry#mixDuration}. If the track has no entries, it is equivalent to calling
	 * {@link #setEmptyAnimation(int, float)}.
	 * <p>
	 * See {@link #setEmptyAnimation(int, float)} and
	 * <a href='https://esotericsoftware.com/spine-applying-animations#Empty-animations'>Empty animations</a> in the Spine Runtimes
	 * Guide.
	 * @param delay If > 0, sets {@link TrackEntry#delay}. If <= 0, the delay set is the duration of the previous track entry minus
	 *           any mix duration plus the specified <code>delay</code> (ie the mix ends at (when <code>delay</code> = 0) or before
	 *           (when <code>delay</code> < 0) the previous track entry duration). If the previous entry is looping, its next loop
	 *           completion is used instead of its duration.
	 * @return A track entry to allow further customization of animation playback. References to the track entry must not be kept
	 *         after the {@link AnimationStateListener#dispose(TrackEntry)} event occurs. */
	public TrackEntry addEmptyAnimation (int trackIndex, float mixDuration, float delay) {
		TrackEntry entry = addAnimation(trackIndex, emptyAnimation, false, delay);
		if (delay <= 0) entry.delay = Math.max(entry.delay + entry.mixDuration - mixDuration, 0);
		entry.mixDuration = mixDuration;
		entry.trackEnd = mixDuration;
		return entry;
	}

	/** Sets an empty animation for every track, discarding any queued animations, and mixes to it over the specified mix duration.
	 * <p>
	 * See <a href='https://esotericsoftware.com/spine-applying-animations#Empty-animations'>Empty animations</a> in the Spine
	 * Runtimes Guide. */
	public void setEmptyAnimations (float mixDuration) {
		boolean oldDrainDisabled = queue.drainDisabled;
		queue.drainDisabled = true;
		TrackEntry[] tracks = this.tracks.items;
		for (int i = 0, n = this.tracks.size; i < n; i++) {
			TrackEntry current = tracks[i];
			if (current != null) setEmptyAnimation(current.trackIndex, mixDuration);
		}
		queue.drainDisabled = oldDrainDisabled;
		queue.drain();
	}

	private TrackEntry expandToIndex (int index) {
		if (index < tracks.size) return tracks.items[index];
		tracks.ensureCapacity(index - tracks.size + 1);
		tracks.size = index + 1;
		return null;
	}

	private TrackEntry trackEntry (int trackIndex, Animation animation, boolean loop, @Null TrackEntry last) {
		TrackEntry entry = trackEntryPool.obtain();
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
		entry.animationEnd = animation.getDuration();
		entry.animationLast = -1;
		entry.nextAnimationLast = -1;

		entry.delay = 0;
		entry.trackTime = 0;
		entry.trackLast = -1;
		entry.nextTrackLast = -1;
		entry.trackEnd = Float.MAX_VALUE;
		entry.timeScale = 1;

		entry.alpha = 1;
		entry.mixTime = 0;
		entry.mixDuration = last == null ? 0 : data.getMix(last.animation, animation);
		entry.mixInterpolation = Interpolation.linear;
		entry.totalAlpha = 0;
		entry.keepHold = false;
		return entry;
	}

	/** Removes {@link TrackEntry#next} and all entries after it for the specified entry. */
	public void clearNext (TrackEntry entry) {
		TrackEntry next = entry.next;
		while (next != null) {
			queue.dispose(next);
			next = next.next;
		}
		entry.next = null;
	}

	void animationsChanged () {
		animationsChanged = false;

		// Process in the order that animations are applied.
		int n = tracks.size;
		TrackEntry[] tracks = this.tracks.items;
		for (int i = 0; i < n; i++) {
			TrackEntry track = tracks[i];
			if (track == null) continue;
			TrackEntry entry = track;
			while (entry.mixingFrom != null) // Move to last entry, then iterate in reverse.
				entry = entry.mixingFrom;
			do {
				computeHold(entry, track);
				entry = entry.mixingTo;
			} while (entry != null);
		}
		propertyIds.clear(2048);
	}

	private void computeHold (TrackEntry entry, TrackEntry track) {
		Timeline[] timelines = entry.animation.timelines.items;
		int timelinesCount = entry.animation.timelines.size;
		int[] timelineMode = entry.timelineMode.setSize(timelinesCount);
		entry.timelineHoldMix.clear();
		TrackEntry[] timelineHoldMix = entry.timelineHoldMix.setSize(timelinesCount);
		boolean add = entry.additive, keepHold = entry.keepHold;
		TrackEntry to = entry.mixingTo;

		for (int i = 0; i < timelinesCount; i++) {
			Timeline timeline = timelines[i];
			long[] ids = timeline.propertyIds;
			int from = from(track, timeline, ids);

			if (add && timeline.additive) {
				timelineMode[i] = from;
				continue;
			}

			// Hold if the next entry will overwrite this property.
			int mode;
			if (to == null || timeline.instant || (to.additive && timeline.additive) || !to.animation.hasTimeline(ids))
				mode = from;
			else {
				mode = from | HOLD;
				// Find next entry that doesn't overwrite this property. Its mix fades out the hold, instead of it ending abruptly.
				for (TrackEntry next = to.mixingTo; next != null; next = next.mixingTo) {
					if ((next.additive && timeline.additive) || !next.animation.hasTimeline(ids)) {
						if (next.mixDuration > 0) timelineHoldMix[i] = next;
						break;
					}
				}
			}
			if (keepHold) mode = (mode & ~HOLD) | (timelineMode[i] & HOLD);
			timelineMode[i] = mode;
		}
	}

	private int from (TrackEntry track, Timeline timeline, long[] ids) {
		LongMap<TrackEntry> propertyIds = this.propertyIds;
		int from = SETUP;
		for (int i = 0, n = ids.length; i < n; i++) {
			TrackEntry owner = propertyIds.putMissing(ids[i], track);
			if (owner != null) {
				if (owner != track) {
					while (++i < n)
						propertyIds.putMissing(ids[i], track);
					return CURRENT;
				}
				from = FIRST;
			}
		}
		if (timeline instanceof DrawOrderFolderTimeline) {
			TrackEntry first = propertyIds.get(DrawOrderTimeline.propertyID);
			if (first != null) return first != track ? CURRENT : FIRST;
		}
		return from;
	}

	/** Returns the track entry for the animation currently playing on the track, or null if no animation is currently playing. */
	public @Null TrackEntry getTrack (int trackIndex) {
		if (trackIndex < 0) throw new IllegalArgumentException("trackIndex must be >= 0.");
		if (trackIndex >= tracks.size) return null;
		return tracks.items[trackIndex];
	}

	/** Adds a listener to receive events for all track entries. */
	public void addListener (AnimationStateListener listener) {
		if (listener == null) throw new IllegalArgumentException("listener cannot be null.");
		listeners.add(listener);
	}

	/** Removes the listener added with {@link #addListener(AnimationStateListener)}. */
	public void removeListener (AnimationStateListener listener) {
		listeners.removeValue(listener, true);
	}

	/** Removes all listeners added with {@link #addListener(AnimationStateListener)}. */
	public void clearListeners () {
		listeners.clear();
	}

	/** Discards all listener notifications that have not yet been delivered. This can be useful to call from an
	 * {@link AnimationStateListener} when it is known that further notifications that may have been already queued for delivery
	 * are not wanted because new animations are being set. */
	public void clearListenerNotifications () {
		queue.clear();
	}

	/** Multiplier for the delta time when the animation state is updated, causing time for all animations and mixes to play slower
	 * or faster. Defaults to 1.
	 * <p>
	 * See {@link TrackEntry#timeScale} to affect a single animation. */
	public float getTimeScale () {
		return timeScale;
	}

	public void setTimeScale (float timeScale) {
		this.timeScale = timeScale;
	}

	/** The {@link AnimationStateData} to look up mix durations. */
	public AnimationStateData getData () {
		return data;
	}

	public void setData (AnimationStateData data) {
		if (data == null) throw new IllegalArgumentException("data cannot be null.");
		this.data = data;
	}

	/** The list of tracks that have had animations. May contain null entries for tracks that currently have no animation. */
	public Array<TrackEntry> getTracks () {
		return tracks;
	}

	public String toString () {
		var buffer = new StringBuilder(64);
		TrackEntry[] tracks = this.tracks.items;
		for (int i = 0, n = this.tracks.size; i < n; i++) {
			TrackEntry entry = tracks[i];
			if (entry == null) continue;
			if (buffer.length() > 0) buffer.append(", ");
			buffer.append(entry.toString());
		}
		if (buffer.length() == 0) return "<none>";
		return buffer.toString();
	}

	/** Stores settings and other state for the playback of an animation on an {@link AnimationState} track.
	 * <p>
	 * References to a track entry must not be kept after the {@link AnimationStateListener#dispose(TrackEntry)} event occurs. */
	static public class TrackEntry implements Poolable {
		Animation animation;
		@Null TrackEntry previous, next, mixingFrom, mixingTo;
		@Null AnimationStateListener listener;
		int trackIndex;
		boolean loop, additive, reverse, shortestRotation, keepHold;
		float eventThreshold, mixAttachmentThreshold, alphaAttachmentThreshold, mixDrawOrderThreshold;
		float animationStart, animationEnd, animationLast, nextAnimationLast;
		float delay, trackTime, trackLast, nextTrackLast, trackEnd, timeScale;
		float alpha, mixTime, mixDuration, totalAlpha;
		Interpolation mixInterpolation = Interpolation.linear;

		/** For each timeline:
		 * <li>Bits 0-1: MixFrom.
		 * <li>Bit 2, HOLD: 0 = mix out using alphaMix, 1 = apply full alpha to prevent dipping. Timeline is first on its track to
		 * set the property and the next entry (mixingTo) also sets it. When held, timelineHoldMix's mix controls how the hold fades
		 * out (for 3+ entry chains where the chain eventually stops setting the property). */
		final IntArray timelineMode = new IntArray();
		final Array<TrackEntry> timelineHoldMix = new Array(true, 8, TrackEntry[]::new);
		final FloatArray timelinesRotation = new FloatArray();

		public void reset () {
			previous = null;
			next = null;
			mixingFrom = null;
			mixingTo = null;
			mixInterpolation = Interpolation.linear;
			animation = null;
			listener = null;
			timelineMode.clear();
			timelineHoldMix.clear();
			timelinesRotation.clear();
		}

		/** The index of the track where this track entry is either current or queued.
		 * <p>
		 * See {@link AnimationState#getTrack(int)}. */
		public int getTrackIndex () {
			return trackIndex;
		}

		/** The animation to apply for this track entry. */
		public Animation getAnimation () {
			return animation;
		}

		public void setAnimation (Animation animation) {
			if (animation == null) throw new IllegalArgumentException("animation cannot be null.");
			this.animation = animation;
		}

		/** If true, the animation will repeat. If false it will not, instead its last frame is applied if played beyond its
		 * duration. */
		public boolean getLoop () {
			return loop;
		}

		public void setLoop (boolean loop) {
			this.loop = loop;
		}

		/** Seconds to postpone playing the animation. Must be >= 0. When this track entry is the current track entry,
		 * <code>delay</code> postpones incrementing the {@link #trackTime}. When this track entry is queued, <code>delay</code> is
		 * the time from the start of the previous animation to when this track entry will become the current track entry (ie when
		 * the previous track entry {@link #trackTime} >= this track entry's <code>delay</code>).
		 * <p>
		 * {@link #timeScale} affects the delay.
		 * <p>
		 * When passing <code>delay</code> <= 0 to {@link AnimationState#addAnimation(int, Animation, boolean, float)} this
		 * <code>delay</code> is set using a mix duration from {@link AnimationStateData}. To change the {@link #mixDuration}
		 * afterward, use {@link #setMixDuration(float, float)} so this <code>delay</code> is adjusted. */
		public float getDelay () {
			return delay;
		}

		public void setDelay (float delay) {
			if (delay < 0) throw new IllegalArgumentException("delay must be >= 0.");
			this.delay = delay;
		}

		/** The time in seconds this track entry has been the current track entry, starting at 0 and increasing forever. Compare to
		 * {@link #getAnimationTime()}, which is always between {@link #animationStart} and {@link #animationEnd}.
		 * <p>
		 * The track time can be set to start the animation at a time other than 0, without affecting looping. When doing so,
		 * {@link #animationLast} can be set to the same value to avoid firing events from the start of the animation.
		 * <p>
		 * To set the time an animation starts and loops, use {@link #animationStart} and {@link #animationEnd}. */
		public float getTrackTime () {
			return trackTime;
		}

		public void setTrackTime (float trackTime) {
			this.trackTime = trackTime;
		}

		/** The track time in seconds when this animation will be removed from the track. Defaults to the highest possible float
		 * value, meaning the animation will be applied until a new animation is set or the track is cleared. If the track end time
		 * is reached, no other animations are queued for playback, and mixing from any previous animations is complete, then the
		 * properties keyed by the animation are set to the setup pose and the track is cleared.
		 * <p>
		 * Usually you want to use {@link AnimationState#addEmptyAnimation(int, float, float)} rather than have the animation
		 * abruptly cease being applied, leaving the current pose. */
		public float getTrackEnd () {
			return trackEnd;
		}

		public void setTrackEnd (float trackEnd) {
			this.trackEnd = trackEnd;
		}

		/** If this track entry is non-looping, this is the track time in seconds when {@link #animationEnd} is reached, or the
		 * current {@link #trackTime} if it has already been reached.
		 * <p>
		 * If this track entry is looping, this is the track time when this animation will reach its next {@link #animationEnd} (the
		 * next loop completion). */
		public float getTrackComplete () {
			float duration = animationEnd - animationStart;
			if (duration != 0) {
				if (loop) return duration * (1 + (int)(trackTime / duration)); // Completion of next loop.
				if (trackTime < duration) return duration; // Before duration.
			}
			return trackTime; // Next update.
		}

		/** The time in seconds for the first frame of this animation, both initially and after looping. Defaults to 0.
		 * <p>
		 * When setting <code>animationStart</code> time, {@link #animationLast} can be set to the same value to avoid firing events
		 * from the start of the animation. */
		public float getAnimationStart () {
			return animationStart;
		}

		public void setAnimationStart (float animationStart) {
			this.animationStart = animationStart;
		}

		/** The time in seconds for the last frame of this animation. Past this time, non-looping animations hold the pose at this
		 * time while looping animations will loop back to {@link #animationStart}. Defaults to the {@link Animation#duration}. */
		public float getAnimationEnd () {
			return animationEnd;
		}

		public void setAnimationEnd (float animationEnd) {
			this.animationEnd = animationEnd;
		}

		/** The time in seconds this animation was last applied. Some timelines use this for one-time triggers. For example, when
		 * this animation is applied, event timelines will fire all events between the <code>animationLast</code> time (exclusive)
		 * and <code>animationTime</code> (inclusive). Defaults to -1 to ensure triggers on frame 0 happen the first time this
		 * animation is applied. */
		public float getAnimationLast () {
			return animationLast;
		}

		public void setAnimationLast (float animationLast) {
			this.animationLast = animationLast;
			nextAnimationLast = animationLast;
		}

		/** Uses {@link #trackTime} to compute the <code>animationTime</code>, which is always between {@link #animationStart} and
		 * {@link #animationEnd}. When <code>trackTime</code> is 0, <code>animationTime</code> is equal to the
		 * <code>animationStart</code> time. */
		public float getAnimationTime () {
			if (!loop) return Math.min(trackTime + animationStart, animationEnd);
			float duration = animationEnd - animationStart;
			if (duration == 0) return animationStart;
			return (trackTime % duration) + animationStart;
		}

		/** Multiplier for the delta time when this track entry is updated, causing time for this animation to pass slower or
		 * faster. Defaults to 1.
		 * <p>
		 * Values < 0 are not supported. To play an animation in reverse, use {@link #reverse}.
		 * <p>
		 * {@link #mixTime} is not affected by track entry time scale, so {@link #mixDuration} may need to be adjusted to match the
		 * animation speed.
		 * <p>
		 * When using {@link AnimationState#addAnimation(int, Animation, boolean, float)} with a <code>delay</code> <= 0, the
		 * {@link #delay} is set using the mix duration from {@link AnimationState#data}, assuming time scale to be 1. If the time
		 * scale is not 1, the delay may need to be adjusted.
		 * <p>
		 * See {@link AnimationState#timeScale} to affect all animations. */
		public float getTimeScale () {
			return timeScale;
		}

		public void setTimeScale (float timeScale) {
			this.timeScale = timeScale;
		}

		/** The listener for events generated by this track entry, or null.
		 * <p>
		 * A track entry returned from {@link AnimationState#setAnimation(int, Animation, boolean)} is already the current animation
		 * for the track, so the callback for {@link AnimationStateListener#start(TrackEntry)} will not be called. */
		public @Null AnimationStateListener getListener () {
			return listener;
		}

		public void setListener (@Null AnimationStateListener listener) {
			this.listener = listener;
		}

		/** Values < 1 mix this animation with the skeleton's current pose (either the setup pose or the pose from lower tracks).
		 * Defaults to 1, which overwrites the skeleton's current pose with this animation.
		 * <p>
		 * Alpha should be 1 on track 0.
		 * <p>
		 * See {@link #alphaAttachmentThreshold}. */
		public float getAlpha () {
			return alpha;
		}

		public void setAlpha (float alpha) {
			this.alpha = alpha;
		}

		/** When the interpolated mix percentage is less than the <code>eventThreshold</code>, event timelines are applied while
		 * this animation is being mixed out. Defaults to 0, so event timelines are not applied while this animation is being mixed
		 * out. */
		public float getEventThreshold () {
			return eventThreshold;
		}

		public void setEventThreshold (float eventThreshold) {
			this.eventThreshold = eventThreshold;
		}

		/** When the computed alpha is greater than <code>alphaAttachmentThreshold</code>, attachment timelines are applied. The
		 * computed alpha includes {@link #alpha} and the interpolated mix percentage. Defaults to 0, so attachment timelines are
		 * always applied. */
		public float getAlphaAttachmentThreshold () {
			return alphaAttachmentThreshold;
		}

		public void setAlphaAttachmentThreshold (float alphaAttachmentThreshold) {
			this.alphaAttachmentThreshold = alphaAttachmentThreshold;
		}

		/** When the interpolated mix percentage is less than the <code>mixAttachmentThreshold</code>, attachment timelines are
		 * applied while this animation is being mixed out. Defaults to 0, so attachment timelines are not applied while this
		 * animation is being mixed out. */
		public float getMixAttachmentThreshold () {
			return mixAttachmentThreshold;
		}

		public void setMixAttachmentThreshold (float mixAttachmentThreshold) {
			this.mixAttachmentThreshold = mixAttachmentThreshold;
		}

		/** When the interpolated mix percentage is less than the <code>mixDrawOrderThreshold</code>, draw order timelines are
		 * applied while this animation is being mixed out. Defaults to 0, so draw order timelines are not applied while this
		 * animation is being mixed out. */
		public float getMixDrawOrderThreshold () {
			return mixDrawOrderThreshold;
		}

		public void setMixDrawOrderThreshold (float mixDrawOrderThreshold) {
			this.mixDrawOrderThreshold = mixDrawOrderThreshold;
		}

		/** The animation queued to start after this animation, or null if there is none. <code>next</code> makes up a doubly linked
		 * list.
		 * <p>
		 * See {@link AnimationState#clearNext(TrackEntry)} to truncate the list. */
		public @Null TrackEntry getNext () {
			return next;
		}

		/** The animation queued to play before this animation, or null. <code>previous</code> makes up a doubly linked list. */
		public @Null TrackEntry getPrevious () {
			return previous;
		}

		/** Returns true if this track entry has been applied at least once.
		 * <p>
		 * See {@link AnimationState#apply(Skeleton)}. */
		public boolean wasApplied () {
			return nextTrackLast != -1;
		}

		/** Returns true if there is a {@link #next} track entry and it will become the current track entry during the next
		 * {@link AnimationState#update(float)}. */
		public boolean isNextReady () {
			return next != null && nextTrackLast - next.delay >= 0;
		}

		/** Returns true if at least one loop has been completed.
		 * <p>
		 * See {@link AnimationStateListener#complete(TrackEntry)}. */
		public boolean isComplete () {
			return trackTime >= animationEnd - animationStart;
		}

		/** Seconds elapsed from 0 to the {@link #mixDuration} when mixing from the previous animation to this animation. May be
		 * slightly more than <code>mixDuration</code> when the mix is complete. */
		public float getMixTime () {
			return mixTime;
		}

		public void setMixTime (float mixTime) {
			this.mixTime = mixTime;
		}

		/** Seconds for mixing from the previous animation to this animation. Defaults to the value provided by
		 * {@link AnimationStateData#getMix(Animation, Animation)} based on the animation before this animation (if any).
		 * <p>
		 * A mix duration of 0 still needs to be applied one more time to mix out, so the properties it was animating are reverted.
		 * A mix duration of 0 can be set at any time to end the mix on the next {@link AnimationState#update(float) update}.
		 * <p>
		 * The <code>mixDuration</code> can be set manually rather than use the value from
		 * {@link AnimationStateData#getMix(Animation, Animation)}. In that case, the <code>mixDuration</code> can be set for a new
		 * track entry only before {@link AnimationState#update(float)} is next called.
		 * <p>
		 * When using {@link AnimationState#addAnimation(int, Animation, boolean, float)} with a <code>delay</code> <= 0, the
		 * {@link #delay} is set using the mix duration from {@link AnimationState#data}. If <code>mixDuration</code> is set
		 * afterward, the delay needs to be adjusted:
		 * 
		 * <pre>
		 * entry.mixDuration = 0.25;<br>
		 * entry.delay = entry.previous.getTrackComplete() - entry.mixDuration + 0;
		 * </pre>
		 * 
		 * Alternatively, use {@link #setMixDuration(float, float)} to set both the mix duration and recompute the delay:<br>
		 * 
		 * <pre>
		 * entry.setMixDuration(0.25f, 0); // mixDuration, delay
		 * </pre>
		 */
		public float getMixDuration () {
			return mixDuration;
		}

		public void setMixDuration (float mixDuration) {
			this.mixDuration = mixDuration;
		}

		/** Sets both {@link #mixDuration} and {@link #delay}.
		 * @param delay If > 0, sets {@link #delay}. If <= 0, the delay set is the duration of the previous track entry minus the
		 *           specified mix duration plus the specified <code>delay</code> (ie the mix ends at (when <code>delay</code> = 0)
		 *           or before (when <code>delay</code> < 0) the previous track entry duration). If the previous entry is looping,
		 *           its next loop completion is used instead of its duration. */
		public void setMixDuration (float mixDuration, float delay) {
			this.mixDuration = mixDuration;
			if (delay <= 0) delay = previous == null ? 0 : Math.max(delay + previous.getTrackComplete() - mixDuration, 0);
			this.delay = delay;
		}

		/** The interpolation to apply to the mix percentage ({@link #mixTime} / {@link #mixDuration}) when mixing from the previous
		 * animation to this animation. Defaults to linear. */
		public Interpolation getMixInterpolation () {
			return mixInterpolation;
		}

		public void setMixInterpolation (Interpolation mixInterpolation) {
			if (mixInterpolation == null) throw new IllegalArgumentException("mixInterpolation cannot be null.");
			this.mixInterpolation = mixInterpolation;
		}

		float mix () {
			if (mixDuration == 0) return 1;
			float mix = mixTime / mixDuration;
			if (mix >= 1) return 1;
			if (mixInterpolation == Interpolation.linear) return mix;
			mix = mixInterpolation.apply(mix);
			if (mix < 0) return 0;
			if (mix > 1) return 1;
			return mix;
		}

		/** When true, timelines in this animation that support additive have their values added to the setup or current pose values
		 * instead of replacing them. Additive can be set for a new track entry only before {@link AnimationState#apply(Skeleton)}
		 * is next called. */
		public boolean getAdditive () {
			return additive;
		}

		public void setAdditive (boolean additive) {
			this.additive = additive;
		}

		/** The track entry for the previous animation when mixing to this animation, or null if no mixing is currently occurring.
		 * When mixing from multiple animations, <code>mixingFrom</code> makes up a doubly linked list. */
		public @Null TrackEntry getMixingFrom () {
			return mixingFrom;
		}

		/** The track entry for the next animation when mixing from this animation, or null if no mixing is currently occurring.
		 * When mixing to multiple animations, <code>mixingTo</code> makes up a doubly linked list. */
		public @Null TrackEntry getMixingTo () {
			return mixingTo;
		}

		public void setShortestRotation (boolean shortestRotation) {
			this.shortestRotation = shortestRotation;
		}

		/** If true, mixing rotation between tracks always uses the shortest rotation direction. If the rotation is animated, the
		 * shortest rotation direction may change during the mix.
		 * <p>
		 * If false, the shortest rotation direction is remembered when the mix starts and the same direction is used for the rest
		 * of the mix. Defaults to false.
		 * <p>
		 * See {@link #resetRotationDirections()}. */
		public boolean getShortestRotation () {
			return shortestRotation;
		}

		/** When {@link #shortestRotation} is false, this clears the directions for mixing this entry's rotation. This can be useful
		 * to avoid bones rotating the long way around when using {@link #alpha} and starting animations on other tracks.
		 * <p>
		 * Mixing involves finding a rotation between two others. There are two possible solutions: the short or the long way
		 * around. When the two rotations change over time, which direction is the short or long way can also change. If the short
		 * way was always chosen, bones flip to the other side when that direction became the long way. TrackEntry chooses the short
		 * way the first time it is applied and remembers that direction. Resetting that direction makes it choose a new short way
		 * on the next apply. */
		public void resetRotationDirections () {
			timelinesRotation.clear();
		}

		public void setReverse (boolean reverse) {
			this.reverse = reverse;
		}

		/** If true, the animation will be applied in reverse. */
		public boolean getReverse () {
			return reverse;
		}

		/** Returns true if this entry is for the empty animation. See {@link AnimationState#setEmptyAnimation(int, float)},
		 * {@link AnimationState#addEmptyAnimation(int, float, float)}, and {@link AnimationState#setEmptyAnimations(float)}. */
		public boolean isEmptyAnimation () {
			return animation == emptyAnimation;
		}

		public String toString () {
			return animation == null ? "<none>" : animation.name;
		}
	}

	class EventQueue {
		private final Array objects = new Array();
		boolean drainDisabled;

		void start (TrackEntry entry) {
			objects.add(EventType.start);
			objects.add(entry);
			animationsChanged = true;
		}

		void interrupt (TrackEntry entry) {
			objects.add(EventType.interrupt);
			objects.add(entry);
		}

		void end (TrackEntry entry) {
			objects.add(EventType.end);
			objects.add(entry);
			animationsChanged = true;
		}

		void dispose (TrackEntry entry) {
			objects.add(EventType.dispose);
			objects.add(entry);
		}

		void complete (TrackEntry entry) {
			objects.add(EventType.complete);
			objects.add(entry);
		}

		void event (TrackEntry entry, Event event) {
			objects.add(EventType.event);
			objects.add(entry);
			objects.add(event);
		}

		void drain () {
			if (drainDisabled) return; // Not reentrant.
			drainDisabled = true;

			SnapshotArray<AnimationStateListener> listenersArray = AnimationState.this.listeners;
			for (int i = 0; i < this.objects.size; i += 2) {
				Object[] objects = this.objects.items;
				var type = (EventType)objects[i];
				var entry = (TrackEntry)objects[i + 1];
				int nn = listenersArray.size;
				AnimationStateListener[] listeners = listenersArray.begin();
				switch (type) {
				case start:
					if (entry.listener != null) entry.listener.start(entry);
					for (int ii = 0; ii < nn; ii++)
						listeners[ii].start(entry);
					break;
				case interrupt:
					if (entry.listener != null) entry.listener.interrupt(entry);
					for (int ii = 0; ii < nn; ii++)
						listeners[ii].interrupt(entry);
					break;
				case end:
					if (entry.listener != null) entry.listener.end(entry);
					for (int ii = 0; ii < nn; ii++)
						listeners[ii].end(entry);
					// Fall through.
				case dispose:
					if (entry.listener != null) entry.listener.dispose(entry);
					for (int ii = 0; ii < nn; ii++)
						listeners[ii].dispose(entry);
					trackEntryPool.free(entry);
					break;
				case complete:
					if (entry.listener != null) entry.listener.complete(entry);
					for (int ii = 0; ii < nn; ii++)
						listeners[ii].complete(entry);
					break;
				case event:
					var event = (Event)objects[i++ + 2];
					if (entry.listener != null) entry.listener.event(entry, event);
					for (int ii = 0; ii < nn; ii++)
						listeners[ii].event(entry, event);
					break;
				}
				listenersArray.end();
			}
			clear();

			drainDisabled = false;
		}

		void clear () {
			objects.clear();
		}
	}

	static private enum EventType {
		start, interrupt, end, dispose, complete, event
	}

	/** The interface to implement for receiving TrackEntry events. It is always safe to call AnimationState methods when receiving
	 * events.
	 * <p>
	 * TrackEntry events are collected during {@link AnimationState#update(float)} and {@link AnimationState#apply(Skeleton)} and
	 * fired only after those methods are finished.
	 * <p>
	 * See {@link TrackEntry#setListener(AnimationStateListener)} and
	 * {@link AnimationState#addListener(AnimationStateListener)}. */
	static public interface AnimationStateListener {
		/** Invoked when this entry has been set as the current entry. {@link #end(TrackEntry)} will occur when this entry will no
		 * longer be applied.
		 * <p>
		 * When this event is triggered by calling {@link AnimationState#setAnimation(int, Animation, boolean)}, take care not to
		 * call {@link AnimationState#update(float)} until after the TrackEntry has been configured. */
		public void start (TrackEntry entry);

		/** Invoked when another entry has replaced this entry as the current entry. This entry may continue being applied for
		 * mixing. */
		public void interrupt (TrackEntry entry);

		/** Invoked when this entry will never be applied again. This only occurs if this entry has previously been set as the
		 * current entry ({@link #start(TrackEntry)} was invoked). */
		public void end (TrackEntry entry);

		/** Invoked when this entry will be disposed. This may occur without the entry ever being set as the current entry.
		 * <p>
		 * References to the entry should not be kept after <code>dispose</code> is called, as it may be destroyed or reused. */
		public void dispose (TrackEntry entry);

		/** Invoked every time this entry's animation completes a loop. This may occur during mixing (after
		 * {@link #interrupt(TrackEntry)}).
		 * <p>
		 * If this entry's {@link TrackEntry#mixingTo} is not null, this entry is mixing out (it is not the current entry).
		 * <p>
		 * Because this event is triggered at the end of {@link AnimationState#apply(Skeleton)}, any animations set in response to
		 * the event won't be applied until the next time the AnimationState is applied. */
		public void complete (TrackEntry entry);

		/** Invoked when this entry's animation triggers an event. This may occur during mixing (after
		 * {@link #interrupt(TrackEntry)}), see {@link TrackEntry#eventThreshold}.
		 * <p>
		 * Because this event is triggered at the end of {@link AnimationState#apply(Skeleton)}, any animations set in response to
		 * the event won't be applied until the next time the AnimationState is applied. */
		public void event (TrackEntry entry, Event event);
	}

	static abstract public class AnimationStateAdapter implements AnimationStateListener {
		public void start (TrackEntry entry) {
		}

		public void interrupt (TrackEntry entry) {
		}

		public void end (TrackEntry entry) {
		}

		public void dispose (TrackEntry entry) {
		}

		public void complete (TrackEntry entry) {
		}

		public void event (TrackEntry entry, Event event) {
		}
	}
}
