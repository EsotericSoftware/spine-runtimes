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

package com.esotericsoftware.spine;

import static com.esotericsoftware.spine.Animation.RotateTimeline.*;

import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;
import com.badlogic.gdx.utils.IntArray;
import com.badlogic.gdx.utils.IntSet;
import com.badlogic.gdx.utils.Pool;
import com.badlogic.gdx.utils.Pool.Poolable;

import com.esotericsoftware.spine.Animation.AttachmentTimeline;
import com.esotericsoftware.spine.Animation.DrawOrderTimeline;
import com.esotericsoftware.spine.Animation.MixBlend;
import com.esotericsoftware.spine.Animation.MixDirection;
import com.esotericsoftware.spine.Animation.RotateTimeline;
import com.esotericsoftware.spine.Animation.Timeline;

/** Applies animations over time, queues animations for later playback, mixes (crossfading) between animations, and applies
 * multiple animations on top of each other (layering).
 * <p>
 * See <a href='http://esotericsoftware.com/spine-applying-animations/'>Applying Animations</a> in the Spine Runtimes Guide. */
public class AnimationState {
	static private final Animation emptyAnimation = new Animation("<empty>", new Array(0), 0);

	/** 1) A previously applied timeline has set this property.<br>
	 * Result: Mix from the current pose to the timeline pose. */
	static private final int SUBSEQUENT = 0;
	/** 1) This is the first timeline to set this property.<br>
	 * 2) The next track entry applied after this one does not have a timeline to set this property.<br>
	 * Result: Mix from the setup pose to the timeline pose. */
	static private final int FIRST = 1;
	/** 1) This is the first timeline to set this property.<br>
	 * 2) The next track entry to be applied does have a timeline to set this property.<br>
	 * 3) The next track entry after that one does not have a timeline to set this property.<br>
	 * Result: Mix from the setup pose to the timeline pose, but do not mix out. This avoids "dipping" when crossfading animations
	 * that key the same property. A subsequent timeline will set this property using a mix. */
	static private final int HOLD = 2;
	/** 1) This is the first timeline to set this property.<br>
	 * 2) The next track entry to be applied does have a timeline to set this property.<br>
	 * 3) The next track entry after that one does have a timeline to set this property.<br>
	 * 4) timelineHoldMix stores the first subsequent track entry that does not have a timeline to set this property.<br>
	 * Result: The same as HOLD except the mix percentage from the timelineHoldMix track entry is used. This handles when more than
	 * 2 track entries in a row have a timeline that sets the same property.<br>
	 * Eg, A -> B -> C -> D where A, B, and C have a timeline setting same property, but D does not. When A is applied, to avoid
	 * "dipping" A is not mixed out, however D (the first entry that doesn't set the property) mixing in is used to mix out A
	 * (which affects B and C). Without using D to mix out, A would be applied fully until mixing completes, then snap into
	 * place. */
	static private final int HOLD_MIX = 3;

	private AnimationStateData data;
	final Array<TrackEntry> tracks = new Array();
	private final Array<Event> events = new Array();
	final Array<AnimationStateListener> listeners = new Array();
	private final EventQueue queue = new EventQueue();
	private final IntSet propertyIDs = new IntSet();
	boolean animationsChanged;
	private float timeScale = 1;

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

	/** Increments each track entry {@link TrackEntry#getTrackTime()}, setting queued animations as current if needed. */
	public void update (float delta) {
		delta *= timeScale;
		for (int i = 0, n = tracks.size; i < n; i++) {
			TrackEntry current = tracks.get(i);
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
					next.trackTime = nextTime + delta * next.timeScale;
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
				tracks.set(i, null);
				queue.end(current);
				disposeNext(current);
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

		// Require mixTime > 0 to ensure the mixing from entry was applied at least once.
		if (to.mixTime > 0 && (to.mixTime >= to.mixDuration || to.timeScale == 0)) {
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
		to.mixTime += delta * to.timeScale;
		return false;
	}

	/** Poses the skeleton using the track entry animations. There are no side effects other than invoking listeners, so the
	 * animation state can be applied to multiple skeletons to pose them identically.
	 * @return True if any animations were applied. */
	public boolean apply (Skeleton skeleton) {
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");
		if (animationsChanged) animationsChanged();

		Array<Event> events = this.events;
		boolean applied = false;
		for (int i = 0, n = tracks.size; i < n; i++) {
			TrackEntry current = tracks.get(i);
			if (current == null || current.delay > 0) continue;
			applied = true;

			// Track 0 animations aren't for layering, so do not show the previously applied animations before the first key.
			MixBlend blend = i == 0 ? MixBlend.first : current.mixBlend;

			// Apply mixing from entries first.
			float mix = current.alpha;
			if (current.mixingFrom != null)
				mix *= applyMixingFrom(current, skeleton, blend);
			else if (current.trackTime >= current.trackEnd && current.next == null) //
				mix = 0; // Set to setup pose the last time the entry will be applied.

			// Apply current entry.
			float animationLast = current.animationLast, animationTime = current.getAnimationTime();
			int timelineCount = current.animation.timelines.size;
			Object[] timelines = current.animation.timelines.items;
			if (i == 0 && (mix == 1 || blend == MixBlend.add)) {
				for (int ii = 0; ii < timelineCount; ii++)
					((Timeline)timelines[ii]).apply(skeleton, animationLast, animationTime, events, mix, blend, MixDirection.in);
			} else {
				int[] timelineMode = current.timelineMode.items;

				boolean firstFrame = current.timelinesRotation.size == 0;
				if (firstFrame) current.timelinesRotation.setSize(timelineCount << 1);
				float[] timelinesRotation = current.timelinesRotation.items;

				for (int ii = 0; ii < timelineCount; ii++) {
					Timeline timeline = (Timeline)timelines[ii];
					MixBlend timelineBlend = timelineMode[ii] == SUBSEQUENT ? blend : MixBlend.setup;
					if (timeline instanceof RotateTimeline) {
						applyRotateTimeline(timeline, skeleton, animationTime, mix, timelineBlend, timelinesRotation, ii << 1,
							firstFrame);
					} else
						timeline.apply(skeleton, animationLast, animationTime, events, mix, timelineBlend, MixDirection.in);
				}
			}
			queueEvents(current, animationTime);
			events.clear();
			current.nextAnimationLast = animationTime;
			current.nextTrackLast = current.trackTime;
		}

		queue.drain();
		return applied;
	}

	private float applyMixingFrom (TrackEntry to, Skeleton skeleton, MixBlend blend) {
		TrackEntry from = to.mixingFrom;
		if (from.mixingFrom != null) applyMixingFrom(from, skeleton, blend);

		float mix;
		if (to.mixDuration == 0) { // Single frame mix to undo mixingFrom changes.
			mix = 1;
			if (blend == MixBlend.first) blend = MixBlend.setup; // Tracks >0 are transparent and can't reset to setup pose.
		} else {
			mix = to.mixTime / to.mixDuration;
			if (mix > 1) mix = 1;
			if (blend != MixBlend.first) blend = from.mixBlend; // Track 0 ignores track mix blend.
		}

		Array<Event> events = mix < from.eventThreshold ? this.events : null;
		boolean attachments = mix < from.attachmentThreshold, drawOrder = mix < from.drawOrderThreshold;
		float animationLast = from.animationLast, animationTime = from.getAnimationTime();
		int timelineCount = from.animation.timelines.size;
		Object[] timelines = from.animation.timelines.items;
		float alphaHold = from.alpha * to.interruptAlpha, alphaMix = alphaHold * (1 - mix);

		if (blend == MixBlend.add) {
			for (int i = 0; i < timelineCount; i++)
				((Timeline)timelines[i]).apply(skeleton, animationLast, animationTime, events, alphaMix, blend, MixDirection.out);
		} else {
			int[] timelineMode = from.timelineMode.items;
			Object[] timelineHoldMix = from.timelineHoldMix.items;

			boolean firstFrame = from.timelinesRotation.size == 0;
			if (firstFrame) from.timelinesRotation.setSize(timelineCount << 1);
			float[] timelinesRotation = from.timelinesRotation.items;

			from.totalAlpha = 0;
			for (int i = 0; i < timelineCount; i++) {
				Timeline timeline = (Timeline)timelines[i];
				MixBlend timelineBlend;
				float alpha;
				switch (timelineMode[i]) {
				case SUBSEQUENT:
					if (!attachments && timeline instanceof AttachmentTimeline) continue;
					if (!drawOrder && timeline instanceof DrawOrderTimeline) continue;
					timelineBlend = blend;
					alpha = alphaMix;
					break;
				case FIRST:
					timelineBlend = MixBlend.setup;
					alpha = alphaMix;
					break;
				case HOLD:
					timelineBlend = MixBlend.setup;
					alpha = alphaHold;
					break;
				default:
					timelineBlend = MixBlend.setup;
					TrackEntry holdMix = (TrackEntry)timelineHoldMix[i];
					alpha = alphaHold * Math.max(0, 1 - holdMix.mixTime / holdMix.mixDuration);
					break;
				}
				from.totalAlpha += alpha;
				if (timeline instanceof RotateTimeline) {
					applyRotateTimeline(timeline, skeleton, animationTime, alpha, timelineBlend, timelinesRotation, i << 1,
						firstFrame);
				} else
					timeline.apply(skeleton, animationLast, animationTime, events, alpha, timelineBlend, MixDirection.out);
			}
		}

		if (to.mixDuration > 0) queueEvents(from, animationTime);
		this.events.clear();
		from.nextAnimationLast = animationTime;
		from.nextTrackLast = from.trackTime;

		return mix;
	}

	private void applyRotateTimeline (Timeline timeline, Skeleton skeleton, float time, float alpha, MixBlend blend,
		float[] timelinesRotation, int i, boolean firstFrame) {

		if (firstFrame) timelinesRotation[i] = 0;

		if (alpha == 1) {
			timeline.apply(skeleton, 0, time, null, 1, blend, MixDirection.in);
			return;
		}

		RotateTimeline rotateTimeline = (RotateTimeline)timeline;
		Bone bone = skeleton.bones.get(rotateTimeline.boneIndex);
		float[] frames = rotateTimeline.frames;
		if (time < frames[0]) { // Time is before first frame.
			if (blend == MixBlend.setup) bone.rotation = bone.data.rotation;
			return;
		}

		float r2;
		if (time >= frames[frames.length - ENTRIES]) // Time is after last frame.
			r2 = bone.data.rotation + frames[frames.length + PREV_ROTATION];
		else {
			// Interpolate between the previous frame and the current frame.
			int frame = Animation.binarySearch(frames, time, ENTRIES);
			float prevRotation = frames[frame + PREV_ROTATION];
			float frameTime = frames[frame];
			float percent = rotateTimeline.getCurvePercent((frame >> 1) - 1,
				1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

			r2 = frames[frame + ROTATION] - prevRotation;
			r2 -= (16384 - (int)(16384.499999999996 - r2 / 360)) * 360;
			r2 = prevRotation + r2 * percent + bone.data.rotation;
			r2 -= (16384 - (int)(16384.499999999996 - r2 / 360)) * 360;
		}

		// Mix between rotations using the direction of the shortest route on the first frame.
		float r1 = blend == MixBlend.setup ? bone.data.rotation : bone.rotation;
		float total, diff = r2 - r1;
		diff -= (16384 - (int)(16384.499999999996 - diff / 360)) * 360;
		if (diff == 0)
			total = timelinesRotation[i];
		else {
			float lastTotal, lastDiff;
			if (firstFrame) {
				lastTotal = 0;
				lastDiff = diff;
			} else {
				lastTotal = timelinesRotation[i]; // Angle and direction of mix, including loops.
				lastDiff = timelinesRotation[i + 1]; // Difference between bones.
			}
			boolean current = diff > 0, dir = lastTotal >= 0;
			// Detect cross at 0 (not 180).
			if (Math.signum(lastDiff) != Math.signum(diff) && Math.abs(lastDiff) <= 90) {
				// A cross after a 360 rotation is a loop.
				if (Math.abs(lastTotal) > 180) lastTotal += 360 * Math.signum(lastTotal);
				dir = current;
			}
			total = diff + lastTotal - lastTotal % 360; // Store loops as part of lastTotal.
			if (dir != current) total += 360 * Math.signum(lastTotal);
			timelinesRotation[i] = total;
		}
		timelinesRotation[i + 1] = diff;
		r1 += total * alpha;
		bone.rotation = r1 - (16384 - (int)(16384.499999999996 - r1 / 360)) * 360;
	}

	private void queueEvents (TrackEntry entry, float animationTime) {
		float animationStart = entry.animationStart, animationEnd = entry.animationEnd;
		float duration = animationEnd - animationStart;
		float trackLastWrapped = entry.trackLast % duration;

		// Queue events before complete.
		Array<Event> events = this.events;
		int i = 0, n = events.size;
		for (; i < n; i++) {
			Event event = events.get(i);
			if (event.time < trackLastWrapped) break;
			if (event.time > animationEnd) continue; // Discard events outside animation start/end.
			queue.event(entry, event);
		}

		// Queue complete if completed a loop iteration or the animation.
		boolean complete;
		if (entry.loop)
			complete = duration == 0 || trackLastWrapped > entry.trackTime % duration;
		else
			complete = animationTime >= animationEnd && entry.animationLast < animationEnd;
		if (complete) queue.complete(entry);

		// Queue events after complete.
		for (; i < n; i++) {
			Event event = events.get(i);
			if (event.time < animationStart) continue; // Discard events outside animation start/end.
			queue.event(entry, events.get(i));
		}
	}

	/** Removes all animations from all tracks, leaving skeletons in their previous pose.
	 * <p>
	 * It may be desired to use {@link AnimationState#setEmptyAnimations(float)} to mix the skeletons back to the setup pose,
	 * rather than leaving them in their previous pose. */
	public void clearTracks () {
		boolean oldDrainDisabled = queue.drainDisabled;
		queue.drainDisabled = true;
		for (int i = 0, n = tracks.size; i < n; i++)
			clearTrack(i);
		tracks.clear();
		queue.drainDisabled = oldDrainDisabled;
		queue.drain();
	}

	/** Removes all animations from the track, leaving skeletons in their previous pose.
	 * <p>
	 * It may be desired to use {@link AnimationState#setEmptyAnimation(int, float)} to mix the skeletons back to the setup pose,
	 * rather than leaving them in their previous pose. */
	public void clearTrack (int trackIndex) {
		if (trackIndex >= tracks.size) return;
		TrackEntry current = tracks.get(trackIndex);
		if (current == null) return;

		queue.end(current);

		disposeNext(current);

		TrackEntry entry = current;
		while (true) {
			TrackEntry from = entry.mixingFrom;
			if (from == null) break;
			queue.end(from);
			entry.mixingFrom = null;
			entry.mixingTo = null;
			entry = from;
		}

		tracks.set(current.trackIndex, null);

		queue.drain();
	}

	private void setCurrent (int index, TrackEntry current, boolean interrupt) {
		TrackEntry from = expandToIndex(index);
		tracks.set(index, current);

		if (from != null) {
			if (interrupt) queue.interrupt(from);
			current.mixingFrom = from;
			from.mixingTo = current;
			current.mixTime = 0;

			// Store the interrupted mix percentage.
			if (from.mixingFrom != null && from.mixDuration > 0)
				current.interruptAlpha *= Math.min(1, from.mixTime / from.mixDuration);

			from.timelinesRotation.clear(); // Reset rotation for mixing out, in case entry was mixed in.
		}

		queue.start(current);
	}

	/** Sets an animation by name.
	 * <p>
	 * {@link #setAnimation(int, Animation, boolean)}. */
	public TrackEntry setAnimation (int trackIndex, String animationName, boolean loop) {
		Animation animation = data.skeletonData.findAnimation(animationName);
		if (animation == null) throw new IllegalArgumentException("Animation not found: " + animationName);
		return setAnimation(trackIndex, animation, loop);
	}

	/** Sets the current animation for a track, discarding any queued animations.
	 * @param loop If true, the animation will repeat. If false it will not, instead its last frame is applied if played beyond its
	 *           duration. In either case {@link TrackEntry#getTrackEnd()} determines when the track is cleared.
	 * @return A track entry to allow further customization of animation playback. References to the track entry must not be kept
	 *         after the {@link AnimationStateListener#dispose(TrackEntry)} event occurs. */
	public TrackEntry setAnimation (int trackIndex, Animation animation, boolean loop) {
		if (animation == null) throw new IllegalArgumentException("animation cannot be null.");
		boolean interrupt = true;
		TrackEntry current = expandToIndex(trackIndex);
		if (current != null) {
			if (current.nextTrackLast == -1) {
				// Don't mix from an entry that was never applied.
				tracks.set(trackIndex, current.mixingFrom);
				queue.interrupt(current);
				queue.end(current);
				disposeNext(current);
				current = current.mixingFrom;
				interrupt = false; // mixingFrom is current again, but don't interrupt it twice.
			} else
				disposeNext(current);
		}
		TrackEntry entry = trackEntry(trackIndex, animation, loop, current);
		setCurrent(trackIndex, entry, interrupt);
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

	/** Adds an animation to be played after the current or last queued animation for a track. If the track is empty, it is
	 * equivalent to calling {@link #setAnimation(int, Animation, boolean)}.
	 * @param delay Seconds to begin this animation after the start of the previous animation. If <= 0, uses the duration of the
	 *           previous track entry minus any mix duration plus the specified <code>delay</code>. If the previous entry is
	 *           looping, its next loop completion is used instead of the duration.
	 * @return A track entry to allow further customization of animation playback. References to the track entry must not be kept
	 *         after the {@link AnimationStateListener#dispose(TrackEntry)} event occurs. */
	public TrackEntry addAnimation (int trackIndex, Animation animation, boolean loop, float delay) {
		if (animation == null) throw new IllegalArgumentException("animation cannot be null.");

		TrackEntry last = expandToIndex(trackIndex);
		if (last != null) {
			while (last.next != null)
				last = last.next;
		}

		TrackEntry entry = trackEntry(trackIndex, animation, loop, last);

		if (last == null) {
			setCurrent(trackIndex, entry, true);
			queue.drain();
		} else {
			last.next = entry;
			if (delay <= 0) {
				float duration = last.animationEnd - last.animationStart;
				if (duration != 0) {
					if (last.loop)
						delay += duration * (1 + (int)(last.trackTime / duration)); // Completion of next loop.
					else
						delay += Math.max(duration, last.trackTime); // After duration, else next update.
					delay -= data.getMix(last.animation, animation);
				} else
					delay = last.trackTime; // Next update.
			}
		}

		entry.delay = delay;
		return entry;
	}

	/** Sets an empty animation for a track, discarding any queued animations, and sets the track entry's
	 * {@link TrackEntry#getMixDuration()}. An empty animation has no timelines and serves as a placeholder for mixing in or out.
	 * <p>
	 * Mixing out is done by setting an empty animation with a mix duration using either {@link #setEmptyAnimation(int, float)},
	 * {@link #setEmptyAnimations(float)}, or {@link #addEmptyAnimation(int, float, float)}. Mixing to an empty animation causes
	 * the previous animation to be applied less and less over the mix duration. Properties keyed in the previous animation
	 * transition to the value from lower tracks or to the setup pose value if no lower tracks key the property. A mix duration of
	 * 0 still mixes out over one frame.
	 * <p>
	 * Mixing in is done by first setting an empty animation, then adding an animation using
	 * {@link #addAnimation(int, Animation, boolean, float)} and on the returned track entry, set the
	 * {@link TrackEntry#setMixDuration(float)}. Mixing from an empty animation causes the new animation to be applied more and
	 * more over the mix duration. Properties keyed in the new animation transition from the value from lower tracks or from the
	 * setup pose value if no lower tracks key the property to the value keyed in the new animation. */
	public TrackEntry setEmptyAnimation (int trackIndex, float mixDuration) {
		TrackEntry entry = setAnimation(trackIndex, emptyAnimation, false);
		entry.mixDuration = mixDuration;
		entry.trackEnd = mixDuration;
		return entry;
	}

	/** Adds an empty animation to be played after the current or last queued animation for a track, and sets the track entry's
	 * {@link TrackEntry#getMixDuration()}. If the track is empty, it is equivalent to calling
	 * {@link #setEmptyAnimation(int, float)}.
	 * <p>
	 * See {@link #setEmptyAnimation(int, float)}.
	 * @param delay Seconds to begin this animation after the start of the previous animation. If <= 0, uses the duration of the
	 *           previous track entry minus any mix duration plus the specified <code>delay</code>. If the previous entry is
	 *           looping, its next loop completion is used instead of the duration.
	 * @return A track entry to allow further customization of animation playback. References to the track entry must not be kept
	 *         after the {@link AnimationStateListener#dispose(TrackEntry)} event occurs. */
	public TrackEntry addEmptyAnimation (int trackIndex, float mixDuration, float delay) {
		if (delay <= 0) delay -= mixDuration;
		TrackEntry entry = addAnimation(trackIndex, emptyAnimation, false, delay);
		entry.mixDuration = mixDuration;
		entry.trackEnd = mixDuration;
		return entry;
	}

	/** Sets an empty animation for every track, discarding any queued animations, and mixes to it over the specified mix
	 * duration. */
	public void setEmptyAnimations (float mixDuration) {
		boolean oldDrainDisabled = queue.drainDisabled;
		queue.drainDisabled = true;
		for (int i = 0, n = tracks.size; i < n; i++) {
			TrackEntry current = tracks.get(i);
			if (current != null) setEmptyAnimation(current.trackIndex, mixDuration);
		}
		queue.drainDisabled = oldDrainDisabled;
		queue.drain();
	}

	private TrackEntry expandToIndex (int index) {
		if (index < tracks.size) return tracks.get(index);
		tracks.ensureCapacity(index - tracks.size + 1);
		tracks.size = index + 1;
		return null;
	}

	/** @param last May be null. */
	private TrackEntry trackEntry (int trackIndex, Animation animation, boolean loop, TrackEntry last) {
		TrackEntry entry = trackEntryPool.obtain();
		entry.trackIndex = trackIndex;
		entry.animation = animation;
		entry.loop = loop;
		entry.holdPrevious = false;

		entry.eventThreshold = 0;
		entry.attachmentThreshold = 0;
		entry.drawOrderThreshold = 0;

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
		entry.interruptAlpha = 1;
		entry.mixTime = 0;
		entry.mixDuration = last == null ? 0 : data.getMix(last.animation, animation);
		return entry;
	}

	private void disposeNext (TrackEntry entry) {
		TrackEntry next = entry.next;
		while (next != null) {
			queue.dispose(next);
			next = next.next;
		}
		entry.next = null;
	}

	private void animationsChanged () {
		animationsChanged = false;

		propertyIDs.clear(2048);

		for (int i = 0, n = tracks.size; i < n; i++) {
			TrackEntry entry = tracks.get(i);
			if (entry == null) continue;
			// Move to last entry, then iterate in reverse (the order animations are applied).
			while (entry.mixingFrom != null)
				entry = entry.mixingFrom;
			do {
				if (entry.mixingTo == null || entry.mixBlend != MixBlend.add) setTimelineModes(entry);
				entry = entry.mixingTo;
			} while (entry != null);
		}
	}

	private void setTimelineModes (TrackEntry entry) {
		TrackEntry to = entry.mixingTo;
		Object[] timelines = entry.animation.timelines.items;
		int timelinesCount = entry.animation.timelines.size;
		int[] timelineMode = entry.timelineMode.setSize(timelinesCount);
		entry.timelineHoldMix.clear();
		Object[] timelineHoldMix = entry.timelineHoldMix.setSize(timelinesCount);
		IntSet propertyIDs = this.propertyIDs;

		if (to != null && to.holdPrevious) {
			for (int i = 0; i < timelinesCount; i++) {
				propertyIDs.add(((Timeline)timelines[i]).getPropertyId());
				timelineMode[i] = HOLD;
			}
			return;
		}

		outer:
		for (int i = 0; i < timelinesCount; i++) {
			int id = ((Timeline)timelines[i]).getPropertyId();
			if (!propertyIDs.add(id))
				timelineMode[i] = SUBSEQUENT;
			else if (to == null || !hasTimeline(to, id))
				timelineMode[i] = FIRST;
			else {
				for (TrackEntry next = to.mixingTo; next != null; next = next.mixingTo) {
					if (hasTimeline(next, id)) continue;
					if (next.mixDuration > 0) {
						timelineMode[i] = HOLD_MIX;
						timelineHoldMix[i] = next;
						continue outer;
					}
					break;
				}
				timelineMode[i] = HOLD;
			}
		}
	}

	private boolean hasTimeline (TrackEntry entry, int id) {
		Object[] timelines = entry.animation.timelines.items;
		for (int i = 0, n = entry.animation.timelines.size; i < n; i++)
			if (((Timeline)timelines[i]).getPropertyId() == id) return true;
		return false;
	}

	/** Returns the track entry for the animation currently playing on the track, or null if no animation is currently playing. */
	public TrackEntry getCurrent (int trackIndex) {
		if (trackIndex >= tracks.size) return null;
		return tracks.get(trackIndex);
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

	/** Multiplier for the delta time when the animation state is updated, causing time for all animations to play slower or
	 * faster. Defaults to 1.
	 * <p>
	 * See TrackEntry {@link TrackEntry#getTimeScale()} for affecting a single animation. */
	public float getTimeScale () {
		return timeScale;
	}

	public void setTimeScale (float timeScale) {
		this.timeScale = timeScale;
	}

	/** The AnimationStateData to look up mix durations. */
	public AnimationStateData getData () {
		return data;
	}

	public void setData (AnimationStateData data) {
		if (data == null) throw new IllegalArgumentException("data cannot be null.");
		this.data = data;
	}

	/** The list of tracks that currently have animations, which may contain null entries. */
	public Array<TrackEntry> getTracks () {
		return tracks;
	}

	public String toString () {
		StringBuilder buffer = new StringBuilder(64);
		for (int i = 0, n = tracks.size; i < n; i++) {
			TrackEntry entry = tracks.get(i);
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
		TrackEntry next, mixingFrom, mixingTo;
		AnimationStateListener listener;
		int trackIndex;
		boolean loop, holdPrevious;
		float eventThreshold, attachmentThreshold, drawOrderThreshold;
		float animationStart, animationEnd, animationLast, nextAnimationLast;
		float delay, trackTime, trackLast, nextTrackLast, trackEnd, timeScale;
		float alpha, mixTime, mixDuration, interruptAlpha, totalAlpha;
		MixBlend mixBlend = MixBlend.replace;
		final IntArray timelineMode = new IntArray();
		final Array<TrackEntry> timelineHoldMix = new Array();
		final FloatArray timelinesRotation = new FloatArray();

		public void reset () {
			next = null;
			mixingFrom = null;
			mixingTo = null;
			animation = null;
			listener = null;
			timelineMode.clear();
			timelineHoldMix.clear();
			timelinesRotation.clear();
		}

		/** The index of the track where this track entry is either current or queued.
		 * <p>
		 * See {@link AnimationState#getCurrent(int)}. */
		public int getTrackIndex () {
			return trackIndex;
		}

		/** The animation to apply for this track entry. */
		public Animation getAnimation () {
			return animation;
		}

		public void setAnimation (Animation animation) {
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

		/** Seconds to postpone playing the animation. When a track entry is the current track entry, <code>delay</code> postpones
		 * incrementing the {@link #getTrackTime()}. When a track entry is queued, <code>delay</code> is the time from the start of
		 * the previous animation to when the track entry will become the current track entry. */
		public float getDelay () {
			return delay;
		}

		public void setDelay (float delay) {
			this.delay = delay;
		}

		/** Current time in seconds this track entry has been the current track entry. The track time determines
		 * {@link #getAnimationTime()}. The track time can be set to start the animation at a time other than 0, without affecting
		 * looping. */
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
		 * It may be desired to use {@link AnimationState#addEmptyAnimation(int, float, float)} rather than have the animation
		 * abruptly cease being applied. */
		public float getTrackEnd () {
			return trackEnd;
		}

		public void setTrackEnd (float trackEnd) {
			this.trackEnd = trackEnd;
		}

		/** Seconds when this animation starts, both initially and after looping. Defaults to 0.
		 * <p>
		 * When changing the <code>animationStart</code> time, it often makes sense to set {@link #getAnimationLast()} to the same
		 * value to prevent timeline keys before the start time from triggering. */
		public float getAnimationStart () {
			return animationStart;
		}

		public void setAnimationStart (float animationStart) {
			this.animationStart = animationStart;
		}

		/** Seconds for the last frame of this animation. Non-looping animations won't play past this time. Looping animations will
		 * loop back to {@link #getAnimationStart()} at this time. Defaults to the animation {@link Animation#duration}. */
		public float getAnimationEnd () {
			return animationEnd;
		}

		public void setAnimationEnd (float animationEnd) {
			this.animationEnd = animationEnd;
		}

		/** The time in seconds this animation was last applied. Some timelines use this for one-time triggers. Eg, when this
		 * animation is applied, event timelines will fire all events between the <code>animationLast</code> time (exclusive) and
		 * <code>animationTime</code> (inclusive). Defaults to -1 to ensure triggers on frame 0 happen the first time this animation
		 * is applied. */
		public float getAnimationLast () {
			return animationLast;
		}

		public void setAnimationLast (float animationLast) {
			this.animationLast = animationLast;
			nextAnimationLast = animationLast;
		}

		/** Uses {@link #getTrackTime()} to compute the <code>animationTime</code>, which is between {@link #getAnimationStart()}
		 * and {@link #getAnimationEnd()}. When the <code>trackTime</code> is 0, the <code>animationTime</code> is equal to the
		 * <code>animationStart</code> time. */
		public float getAnimationTime () {
			if (loop) {
				float duration = animationEnd - animationStart;
				if (duration == 0) return animationStart;
				return (trackTime % duration) + animationStart;
			}
			return Math.min(trackTime + animationStart, animationEnd);
		}

		/** Multiplier for the delta time when the animation state is updated, causing time for this animation to pass slower or
		 * faster. Defaults to 1.
		 * <p>
		 * If <code>timeScale</code> is 0, any {@link #getMixDuration()} will be ignored.
		 * <p>
		 * See AnimationState {@link AnimationState#getTimeScale()} for affecting all animations. */
		public float getTimeScale () {
			return timeScale;
		}

		public void setTimeScale (float timeScale) {
			this.timeScale = timeScale;
		}

		/** The listener for events generated by this track entry, or null.
		 * <p>
		 * A track entry returned from {@link AnimationState#setAnimation(int, Animation, boolean)} is already the current animation
		 * for the track, so the track entry listener {@link AnimationStateListener#start(TrackEntry)} will not be called. */
		public AnimationStateListener getListener () {
			return listener;
		}

		/** @param listener May be null. */
		public void setListener (AnimationStateListener listener) {
			this.listener = listener;
		}

		/** Values < 1 mix this animation with the setup pose or the skeleton's previous pose. Defaults to 1, which overwrites the
		 * skeleton's previous pose with this animation.
		 * <p>
		 * Typically track 0 is used to completely pose the skeleton, then alpha can be used on higher tracks. It doesn't make sense
		 * to use alpha on track 0 if the skeleton pose is from the last frame render. */
		public float getAlpha () {
			return alpha;
		}

		public void setAlpha (float alpha) {
			this.alpha = alpha;
		}

		/** When the mix percentage ({@link #getMixTime()} / {@link #getMixDuration()}) is less than the
		 * <code>eventThreshold</code>, event timelines for the animation being mixed out will be applied. Defaults to 0, so event
		 * timelines are not applied for an animation being mixed out. */
		public float getEventThreshold () {
			return eventThreshold;
		}

		public void setEventThreshold (float eventThreshold) {
			this.eventThreshold = eventThreshold;
		}

		/** When the mix percentage ({@link #getMixTime()} / {@link #getMixDuration()}) is less than the
		 * <code>attachmentThreshold</code>, attachment timelines for the animation being mixed out will be applied. Defaults to 0,
		 * so attachment timelines are not applied for an animation being mixed out. */
		public float getAttachmentThreshold () {
			return attachmentThreshold;
		}

		public void setAttachmentThreshold (float attachmentThreshold) {
			this.attachmentThreshold = attachmentThreshold;
		}

		/** When the mix percentage ({@link #getMixTime()} / {@link #getMixDuration()}) is less than the
		 * <code>drawOrderThreshold</code>, draw order timelines for the animation being mixed out will be applied. Defaults to 0,
		 * so draw order timelines are not applied for an animation being mixed out. */
		public float getDrawOrderThreshold () {
			return drawOrderThreshold;
		}

		public void setDrawOrderThreshold (float drawOrderThreshold) {
			this.drawOrderThreshold = drawOrderThreshold;
		}

		/** The animation queued to start after this animation, or null. <code>next</code> makes up a linked list. */
		public TrackEntry getNext () {
			return next;
		}

		/** Returns true if at least one loop has been completed.
		 * <p>
		 * See {@link AnimationStateListener#complete(TrackEntry)}. */
		public boolean isComplete () {
			return trackTime >= animationEnd - animationStart;
		}

		/** Seconds from 0 to the {@link #getMixDuration()} when mixing from the previous animation to this animation. May be
		 * slightly more than <code>mixDuration</code> when the mix is complete. */
		public float getMixTime () {
			return mixTime;
		}

		public void setMixTime (float mixTime) {
			this.mixTime = mixTime;
		}

		/** Seconds for mixing from the previous animation to this animation. Defaults to the value provided by AnimationStateData
		 * {@link AnimationStateData#getMix(Animation, Animation)} based on the animation before this animation (if any).
		 * <p>
		 * The <code>mixDuration</code> can be set manually rather than use the value from
		 * {@link AnimationStateData#getMix(Animation, Animation)}. In that case, the <code>mixDuration</code> can be set for a new
		 * track entry only before {@link AnimationState#update(float)} is first called.
		 * <p>
		 * When using {@link AnimationState#addAnimation(int, Animation, boolean, float)} with a <code>delay</code> <= 0, note the
		 * {@link #getDelay()} is set using the mix duration from the {@link AnimationStateData}. */
		public float getMixDuration () {
			return mixDuration;
		}

		public void setMixDuration (float mixDuration) {
			this.mixDuration = mixDuration;
		}

		/** Controls how properties keyed in the animation are mixed with lower tracks. Defaults to {@link MixBlend#replace}, which
		 * replaces the values from the lower tracks with the animation values. {@link MixBlend#add} adds the animation values to
		 * the values from the lower tracks.
		 * <p>
		 * The <code>mixBlend</code> can be set for a new track entry only before {@link AnimationState#apply(Skeleton)} is first
		 * called. */
		public MixBlend getMixBlend () {
			return mixBlend;
		}

		public void setMixBlend (MixBlend mixBlend) {
			this.mixBlend = mixBlend;
		}

		/** The track entry for the previous animation when mixing from the previous animation to this animation, or null if no
		 * mixing is currently occuring. When mixing from multiple animations, <code>mixingFrom</code> makes up a linked list. */
		public TrackEntry getMixingFrom () {
			return mixingFrom;
		}

		public void setHoldPrevious (boolean holdPrevious) {
			this.holdPrevious = holdPrevious;
		}

		/** If true, when mixing from the previous animation to this animation, the previous animation is applied as normal instead
		 * of being mixed out.
		 * <p>
		 * When mixing between animations that key the same property, if a lower track also keys that property then the value will
		 * briefly dip toward the lower track value during the mix. This happens because the first animation mixes from 100% to 0%
		 * while the second animation mixes from 0% to 100%. Setting <code>holdPrevious</code> to true applies the first animation
		 * at 100% during the mix so the lower track value is overwritten. Such dipping does not occur on the lowest track which
		 * keys the property, only when a higher track also keys the property.
		 * <p>
		 * Snapping will occur if <code>holdPrevious</code> is true and this animation does not key all the same properties as the
		 * previous animation. */
		public boolean getHoldPrevious () {
			return holdPrevious;
		}

		/** Resets the rotation directions for mixing this entry's rotate timelines. This can be useful to avoid bones rotating the
		 * long way around when using {@link #alpha} and starting animations on other tracks.
		 * <p>
		 * Mixing with {@link MixBlend#replace} involves finding a rotation between two others, which has two possible solutions:
		 * the short way or the long way around. The two rotations likely change over time, so which direction is the short or long
		 * way also changes. If the short way was always chosen, bones would flip to the other side when that direction became the
		 * long way. TrackEntry chooses the short way the first time it is applied and remembers that direction. */
		public void resetRotationDirections () {
			timelinesRotation.clear();
		}

		public String toString () {
			return animation == null ? "<none>" : animation.name;
		}
	}

	class EventQueue {
		private final Array objects = new Array();
		boolean drainDisabled;

		public void start (TrackEntry entry) {
			objects.add(EventType.start);
			objects.add(entry);
			animationsChanged = true;
		}

		public void interrupt (TrackEntry entry) {
			objects.add(EventType.interrupt);
			objects.add(entry);
		}

		public void end (TrackEntry entry) {
			objects.add(EventType.end);
			objects.add(entry);
			animationsChanged = true;
		}

		public void dispose (TrackEntry entry) {
			objects.add(EventType.dispose);
			objects.add(entry);
		}

		public void complete (TrackEntry entry) {
			objects.add(EventType.complete);
			objects.add(entry);
		}

		public void event (TrackEntry entry, Event event) {
			objects.add(EventType.event);
			objects.add(entry);
			objects.add(event);
		}

		public void drain () {
			if (drainDisabled) return; // Not reentrant.
			drainDisabled = true;

			Array objects = this.objects;
			Array<AnimationStateListener> listeners = AnimationState.this.listeners;
			for (int i = 0; i < objects.size; i += 2) {
				EventType type = (EventType)objects.get(i);
				TrackEntry entry = (TrackEntry)objects.get(i + 1);
				switch (type) {
				case start:
					if (entry.listener != null) entry.listener.start(entry);
					for (int ii = 0; ii < listeners.size; ii++)
						listeners.get(ii).start(entry);
					break;
				case interrupt:
					if (entry.listener != null) entry.listener.interrupt(entry);
					for (int ii = 0; ii < listeners.size; ii++)
						listeners.get(ii).interrupt(entry);
					break;
				case end:
					if (entry.listener != null) entry.listener.end(entry);
					for (int ii = 0; ii < listeners.size; ii++)
						listeners.get(ii).end(entry);
					// Fall through.
				case dispose:
					if (entry.listener != null) entry.listener.dispose(entry);
					for (int ii = 0; ii < listeners.size; ii++)
						listeners.get(ii).dispose(entry);
					trackEntryPool.free(entry);
					break;
				case complete:
					if (entry.listener != null) entry.listener.complete(entry);
					for (int ii = 0; ii < listeners.size; ii++)
						listeners.get(ii).complete(entry);
					break;
				case event:
					Event event = (Event)objects.get(i++ + 2);
					if (entry.listener != null) entry.listener.event(entry, event);
					for (int ii = 0; ii < listeners.size; ii++)
						listeners.get(ii).event(entry, event);
					break;
				}
			}
			clear();

			drainDisabled = false;
		}

		public void clear () {
			objects.clear();
		}
	}

	static private enum EventType {
		start, interrupt, end, dispose, complete, event
	}

	/** The interface to implement for receiving TrackEntry events.
	 * <p>
	 * See TrackEntry {@link TrackEntry#setListener(AnimationStateListener)} and AnimationState
	 * {@link AnimationState#addListener(AnimationStateListener)}. */
	static public interface AnimationStateListener {
		/** Invoked when this entry has been set as the current entry. */
		public void start (TrackEntry entry);

		/** Invoked when another entry has replaced this entry as the current entry. This entry may continue being applied for
		 * mixing. */
		public void interrupt (TrackEntry entry);

		/** Invoked when this entry is no longer the current entry and will never be applied again. */
		public void end (TrackEntry entry);

		/** Invoked when this entry will be disposed. This may occur without the entry ever being set as the current entry.
		 * References to the entry should not be kept after <code>dispose</code> is called, as it may be destroyed or reused. */
		public void dispose (TrackEntry entry);

		/** Invoked every time this entry's animation completes a loop. */
		public void complete (TrackEntry entry);

		/** Invoked when this entry's animation triggers an event. */
		public void event (TrackEntry entry, Event event);
	}

	static public abstract class AnimationStateAdapter implements AnimationStateListener {
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
