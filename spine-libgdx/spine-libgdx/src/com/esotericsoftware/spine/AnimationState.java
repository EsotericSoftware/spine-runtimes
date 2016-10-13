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
import com.badlogic.gdx.utils.BooleanArray;
import com.badlogic.gdx.utils.FloatArray;
import com.badlogic.gdx.utils.IntSet;
import com.badlogic.gdx.utils.Pool;
import com.badlogic.gdx.utils.Pool.Poolable;
import com.esotericsoftware.spine.Animation.AttachmentTimeline;
import com.esotericsoftware.spine.Animation.DrawOrderTimeline;
import com.esotericsoftware.spine.Animation.RotateTimeline;
import com.esotericsoftware.spine.Animation.Timeline;

/** Stores state for applying one or more animations over time and mixing (crossfading) between animations.
 * <p>
 * Animations on different tracks are applied sequentially each frame, from lowest to highest track index. This enables animations
 * to be layered, where higher tracks either key only a subset of the skeleton pose or use alpha < 1 to mix with the pose on the
 * lower track. */
public class AnimationState {
	static private final Animation emptyAnimation = new Animation("<empty>", new Array(0), 0);

	private AnimationStateData data;
	private final Array<TrackEntry> tracks = new Array();
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

	/** Increments the track entry times, setting queued animations as current if needed. */
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
					setCurrent(i, next);
					while (next.mixingFrom != null) {
						next.mixTime += currentDelta;
						next = next.mixingFrom;
					}
					continue;
				}
				updateMixingFrom(current, delta);
			} else {
				updateMixingFrom(current, delta);
				// Clear the track when there is no next entry, the track end time is reached, and there is no mixingFrom.
				if (current.trackLast >= current.trackEnd && current.mixingFrom == null) {
					tracks.set(i, null);
					queue.end(current);
					disposeNext(current);
					continue;
				}
			}

			current.trackTime += currentDelta;
		}

		queue.drain();
	}

	private void updateMixingFrom (TrackEntry entry, float delta) {
		TrackEntry from = entry.mixingFrom;
		if (from == null) return;

		if (entry.mixTime >= entry.mixDuration && entry.mixTime > 0) {
			queue.end(from);
			TrackEntry newFrom = from.mixingFrom;
			entry.mixingFrom = newFrom;
			if (newFrom == null) return;
			entry.mixTime = from.mixTime;
			entry.mixDuration = from.mixDuration;
			from = newFrom;
		}

		from.animationLast = from.nextAnimationLast;
		from.trackLast = from.nextTrackLast;
		float mixingFromDelta = delta * from.timeScale;
		from.trackTime += mixingFromDelta;
		entry.mixTime += mixingFromDelta;

		updateMixingFrom(from, delta);
	}

	/** Poses the skeleton using the track entry animations. There are no side effects other than invoking listeners, so the
	 * animation state can be applied to multiple skeletons to pose them identically. */
	public void apply (Skeleton skeleton) {
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");
		if (animationsChanged) animationsChanged();

		Array<Event> events = this.events;

		for (int i = 0; i < tracks.size; i++) {
			TrackEntry current = tracks.get(i);
			if (current == null || current.delay > 0) continue;

			// Apply mixing from entries first.
			float mix = current.alpha;
			if (current.mixingFrom != null) mix = applyMixingFrom(current, skeleton, mix);

			// Apply current entry.
			float animationLast = current.animationLast, animationTime = current.getAnimationTime();
			Array<Timeline> timelines = current.animation.timelines;
			if (mix == 1) {
				for (int ii = 0, n = timelines.size; ii < n; ii++)
					timelines.get(ii).apply(skeleton, animationLast, animationTime, events, 1, false, false);
			} else {
				boolean firstFrame = current.timelinesRotation.size == 0;
				if (firstFrame) current.timelinesRotation.setSize(timelines.size << 1);
				float[] timelinesRotation = current.timelinesRotation.items;
				boolean[] timelinesFirst = current.timelinesFirst.items;
				for (int ii = 0, n = timelines.size; ii < n; ii++) {
					Timeline timeline = timelines.get(ii);
					if (timeline instanceof RotateTimeline) {
						applyRotateTimeline((RotateTimeline)timeline, skeleton, animationLast, animationTime, events, mix,
							timelinesFirst[ii], false, timelinesRotation, ii << 1, firstFrame);
					} else {
						timeline.apply(skeleton, animationLast, animationTime, events, mix, timelinesFirst[ii], false);
					}
				}
			}
			queueEvents(current, animationTime);
			current.nextAnimationLast = animationTime;
			current.nextTrackLast = current.trackTime;
		}

		queue.drain();

		System.out.println();
	}

	private float applyMixingFrom (TrackEntry entry, Skeleton skeleton, float alpha) {
		float mix;
		if (entry.mixDuration == 0) // Single frame mix to undo mixingFrom changes.
			mix = 1;
		else {
			mix = alpha * entry.mixTime / entry.mixDuration;
			if (mix > 1) mix = 1;
		}

		TrackEntry from = entry.mixingFrom;
		if (from.mixingFrom != null) applyMixingFrom(from, skeleton, alpha);

		Array<Event> events = mix < from.eventThreshold ? this.events : null;
		boolean attachments = mix < from.attachmentThreshold, drawOrder = mix < from.drawOrderThreshold;

		float animationLast = from.animationLast, animationTime = from.getAnimationTime();
		Array<Timeline> timelines = from.animation.timelines;
		int timelineCount = timelines.size;
		boolean[] timelinesFirst = from.timelinesFirst.items, timelinesLast = from.timelinesLast.items;
		float alphaFull = from.alpha, alphaMix = alphaFull * (1 - mix);

		boolean firstFrame = from.timelinesRotation.size == 0;
		if (firstFrame) from.timelinesRotation.setSize(timelineCount << 1);
		float[] timelinesRotation = from.timelinesRotation.items;

		System.out.println(entry.mixingFrom + " -> " + entry + ": " + entry.mixTime / entry.mixDuration);

		for (int i = 0; i < timelineCount; i++) {
			Timeline timeline = timelines.get(i);
			boolean setupPose = timelinesFirst[i];
			float a = timelinesLast[i] ? alphaMix : alphaFull;
			if (timeline instanceof RotateTimeline) {
				applyRotateTimeline((RotateTimeline)timeline, skeleton, animationLast, animationTime, events, a, setupPose, setupPose,
					timelinesRotation, i << 1, firstFrame);
			} else {
				if (!setupPose) {
					if (!attachments && timeline instanceof AttachmentTimeline) continue;
					if (!drawOrder && timeline instanceof DrawOrderTimeline) continue;
				}
				timeline.apply(skeleton, animationLast, animationTime, events, a, setupPose, setupPose);
			}
		}

		queueEvents(from, animationTime);
		from.nextAnimationLast = animationTime;
		from.nextTrackLast = from.trackTime;

		return mix;
	}

	/** @param events May be null. */
	private void applyRotateTimeline (RotateTimeline timeline, Skeleton skeleton, float lastTime, float time, Array<Event> events,
		float alpha, boolean setupPose, boolean mixingOut, float[] timelinesRotation, int i, boolean firstFrame) {
		if (alpha == 1) {
			timeline.apply(skeleton, lastTime, time, events, 1, setupPose, setupPose);
			return;
		}

		float[] frames = timeline.frames;
		if (time < frames[0]) return; // Time is before first frame.

		Bone bone = skeleton.bones.get(timeline.boneIndex);

		float r2;
		if (time >= frames[frames.length - ENTRIES]) // Time is after last frame.
			r2 = bone.data.rotation + frames[frames.length + PREV_ROTATION];
		else {
			// Interpolate between the previous frame and the current frame.
			int frame = Animation.binarySearch(frames, time, ENTRIES);
			float prevRotation = frames[frame + PREV_ROTATION];
			float frameTime = frames[frame];
			float percent = timeline.getCurvePercent((frame >> 1) - 1,
				1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

			r2 = frames[frame + ROTATION] - prevRotation;
			r2 -= (16384 - (int)(16384.499999999996 - r2 / 360)) * 360;
			r2 = prevRotation + r2 * percent + bone.data.rotation;
			r2 -= (16384 - (int)(16384.499999999996 - r2 / 360)) * 360;
		}

		// Mix between two rotations using the direction of the shortest route on the first frame while detecting crosses.
		float r1 = setupPose ? bone.data.rotation : bone.rotation;
		float total, diff = r2 - r1;
		if (diff == 0) {
			if (firstFrame) {
				timelinesRotation[i] = 0;
				total = 0;
			} else
				total = timelinesRotation[i];
		} else {
			diff -= (16384 - (int)(16384.499999999996 - diff / 360)) * 360;
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
			total = diff + lastTotal - lastTotal % 360; // Keep loops part of lastTotal.
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
		if (entry.loop ? (trackLastWrapped > entry.trackTime % duration)
			: (animationTime >= animationEnd && entry.animationLast < animationEnd)) {
			queue.complete(entry);
		}

		// Queue events after complete.
		for (; i < n; i++) {
			Event event = events.get(i);
			if (event.time < animationStart) continue; // Discard events outside animation start/end.
			queue.event(entry, events.get(i));
		}
		events.clear();
	}

	/** Removes all animations from all tracks, leaving skeletons in their last pose.
	 * <p>
	 * It may be desired to use {@link AnimationState#setEmptyAnimations(float)} to mix the skeletons back to the setup pose,
	 * rather than leaving them in their last pose. */
	public void clearTracks () {
		queue.drainDisabled = true;
		for (int i = 0, n = tracks.size; i < n; i++)
			clearTrack(i);
		tracks.clear();
		queue.drainDisabled = false;
		queue.drain();
	}

	/** Removes all animations from the track, leaving skeletons in their last pose.
	 * <p>
	 * It may be desired to use {@link AnimationState#setEmptyAnimation(int, float)} to mix the skeletons back to the setup pose,
	 * rather than leaving them in their last pose. */
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
			entry = from;
		}

		tracks.set(current.trackIndex, null);

		queue.drain();
	}

	private void setCurrent (int index, TrackEntry entry) {
		TrackEntry current = expandToIndex(index);
		tracks.set(index, entry);

		if (current != null) {
			queue.interrupt(current);
			entry.mixingFrom = current;
			entry.mixTime = Math.max(0, entry.mixDuration - current.trackTime);
			current.timelinesRotation.clear(); // BOZO - Needed? Recursive?
		}

		queue.start(entry);
	}

	/** @see #setAnimation(int, Animation, boolean) */
	public TrackEntry setAnimation (int trackIndex, String animationName, boolean loop) {
		Animation animation = data.getSkeletonData().findAnimation(animationName);
		if (animation == null) throw new IllegalArgumentException("Animation not found: " + animationName);
		return setAnimation(trackIndex, animation, loop);
	}

	/** Sets the current animation for a track, discarding any queued animations.
	 * @return A track entry to allow further customization of animation playback. References to the track entry must not be kept
	 *         after {@link AnimationStateListener#dispose(TrackEntry)}. */
	public TrackEntry setAnimation (int trackIndex, Animation animation, boolean loop) {
		if (animation == null) throw new IllegalArgumentException("animation cannot be null.");
		TrackEntry current = expandToIndex(trackIndex);
		if (current != null) {
			if (current.nextTrackLast == -1) {
				// Don't mix from an entry that was never applied.
				tracks.set(trackIndex, null);
				queue.interrupt(current);
				queue.end(current);
				disposeNext(current);
				current = null;
			} else
				disposeNext(current);
		}
		TrackEntry entry = trackEntry(trackIndex, animation, loop, current);
		setCurrent(trackIndex, entry);
		queue.drain();
		return entry;
	}

	/** {@link #addAnimation(int, Animation, boolean, float)} */
	public TrackEntry addAnimation (int trackIndex, String animationName, boolean loop, float delay) {
		Animation animation = data.getSkeletonData().findAnimation(animationName);
		if (animation == null) throw new IllegalArgumentException("Animation not found: " + animationName);
		return addAnimation(trackIndex, animation, loop, delay);
	}

	/** Adds an animation to be played after the current or last queued animation for a track.
	 * @param delay Seconds to begin this animation after the start of the previous animation. May be <= 0 to use the animation
	 *           duration of the previous track minus any mix duration plus the negative delay.
	 * @return A track entry to allow further customization of animation playback. References to the track entry must not be kept
	 *         after {@link AnimationStateListener#dispose(TrackEntry)}. */
	public TrackEntry addAnimation (int trackIndex, Animation animation, boolean loop, float delay) {
		if (animation == null) throw new IllegalArgumentException("animation cannot be null.");

		TrackEntry last = expandToIndex(trackIndex);
		if (last != null) {
			while (last.next != null)
				last = last.next;
		}

		TrackEntry entry = trackEntry(trackIndex, animation, loop, last);

		if (last == null) {
			setCurrent(trackIndex, entry);
			queue.drain();
		} else {
			last.next = entry;
			if (delay <= 0) {
				float duration = last.animationEnd - last.animationStart;
				if (duration != 0)
					delay += duration * (1 + (int)(last.trackTime / duration)) - data.getMix(last.animation, animation);
				else
					delay = 0;
			}
		}

		entry.delay = delay;
		return entry;
	}

	/** Sets an empty animation for a track, discarding any queued animations, and mixes to it over the specified mix duration. */
	public TrackEntry setEmptyAnimation (int trackIndex, float mixDuration) {
		TrackEntry entry = setAnimation(trackIndex, emptyAnimation, false);
		entry.mixDuration = mixDuration;
		entry.trackEnd = mixDuration;
		return entry;
	}

	/** Adds an empty animation to be played after the current or last queued animation for a track, and mixes to it over the
	 * specified mix duration.
	 * @param delay Seconds to begin this animation after the start of the previous animation. May be <= 0 to use the animation
	 *           duration of the previous track minus any mix duration plus the negative delay.
	 * @return A track entry to allow further customization of animation playback. References to the track entry must not be kept
	 *         after {@link AnimationStateListener#dispose(TrackEntry)}. */
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
		queue.drainDisabled = true;
		for (int i = 0, n = tracks.size; i < n; i++) {
			TrackEntry current = tracks.get(i);
			if (current != null) setEmptyAnimation(current.trackIndex, mixDuration);
		}
		queue.drainDisabled = false;
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
		entry.trackEnd = loop ? Integer.MAX_VALUE : entry.animationEnd;
		entry.timeScale = 1;

		entry.alpha = 1;
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

		IntSet propertyIDs = this.propertyIDs;

		// Compute timelinesFirst from lowest to highest track entries.
		int i = 0, n = tracks.size;
		propertyIDs.clear();
		for (; i < n; i++) { // Find first non-null entry.
			TrackEntry entry = tracks.get(i);
			if (entry == null) continue;
			setTimelinesFirst(entry);
			i++;
			break;
		}
		for (; i < n; i++) { // Rest of entries.
			TrackEntry entry = tracks.get(i);
			if (entry != null) checkTimelinesFirst(entry);
		}

		// Compute timelinesLast from highest to lowest track entries that have mixingFrom.
		propertyIDs.clear();
		int lowestMixingFrom = n;
		for (i = 0; i < n; i++) { // Find lowest with a mixingFrom entry.
			TrackEntry entry = tracks.get(i);
			if (entry == null) continue;
			if (entry.mixingFrom != null) {
				lowestMixingFrom = i;
				break;
			}
		}
		for (i = n - 1; i >= lowestMixingFrom; i--) {
			TrackEntry entry = tracks.get(i);
			if (entry == null) continue;

			Array<Timeline> timelines = entry.animation.timelines;
			for (int ii = 0, nn = timelines.size; ii < nn; ii++)
				propertyIDs.add(timelines.get(ii).getPropertyId());

			entry = entry.mixingFrom;
			while (entry != null) {
				checkTimelinesUsage(entry, entry.timelinesLast);
				entry = entry.mixingFrom;
			}
		}
	}

	/** From last to first mixingFrom entries, sets timelinesFirst to true on last, calls checkTimelineUsage on rest. */
	private void setTimelinesFirst (TrackEntry entry) {
		if (entry.mixingFrom != null) {
			setTimelinesFirst(entry.mixingFrom);
			checkTimelinesUsage(entry, entry.timelinesFirst);
			return;
		}
		IntSet propertyIDs = this.propertyIDs;
		Array<Timeline> timelines = entry.animation.timelines;
		int n = timelines.size;
		boolean[] usage = entry.timelinesFirst.setSize(n);
		for (int i = 0; i < n; i++) {
			propertyIDs.add(timelines.get(i).getPropertyId());
			usage[i] = true;
		}
	}

	/** From last to first mixingFrom entries, calls checkTimelineUsage. */
	private void checkTimelinesFirst (TrackEntry entry) {
		if (entry.mixingFrom != null) checkTimelinesFirst(entry.mixingFrom);
		checkTimelinesUsage(entry, entry.timelinesFirst);
	}

	private void checkTimelinesUsage (TrackEntry entry, BooleanArray usageArray) {
		IntSet propertyIDs = this.propertyIDs;
		Array<Timeline> timelines = entry.animation.timelines;
		int n = timelines.size;
		boolean[] usage = usageArray.setSize(n);
		for (int i = 0; i < n; i++)
			usage[i] = propertyIDs.add(timelines.get(i).getPropertyId());
	}

	/** Returns the track entry for the animation currently playing on the track, or null. */
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

	public void clearListeners () {
		listeners.clear();
	}

	/** Discards all {@link #addListener(AnimationStateListener) listener} notifications that have not yet been delivered. This can
	 * be useful to call from an {@link AnimationStateListener} when it is known that further notifications that may have been
	 * already queued for delivery are not wanted because new animations are being set. */
	public void clearListenerNotifications () {
		queue.clear();
	}

	/** Multiplier for the delta time when the animation state is updated, causing time for all animations to play slower or
	 * faster. Defaults to 1. */
	public float getTimeScale () {
		return timeScale;
	}

	public void setTimeScale (float timeScale) {
		this.timeScale = timeScale;
	}

	public AnimationStateData getData () {
		return data;
	}

	public void setData (AnimationStateData data) {
		if (data == null) throw new IllegalArgumentException("data cannot be null.");
		this.data = data;
	}

	/** Returns the list of tracks that have animations, which may contain null entries. */
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

	/** State for the playback of an animation. */
	static public class TrackEntry implements Poolable {
		Animation animation;
		TrackEntry next, mixingFrom;
		AnimationStateListener listener;
		int trackIndex;
		boolean loop;
		float eventThreshold, attachmentThreshold, drawOrderThreshold;
		float animationStart, animationEnd, animationLast, nextAnimationLast;
		float delay, trackTime, trackLast, nextTrackLast, trackEnd, timeScale;
		float alpha, mixTime, mixDuration;
		final BooleanArray timelinesFirst = new BooleanArray(), timelinesLast = new BooleanArray();
		final FloatArray timelinesRotation = new FloatArray();

		public void reset () {
			next = null;
			mixingFrom = null;
			animation = null;
			listener = null;
			timelinesFirst.clear();
			timelinesLast.clear();
			timelinesRotation.clear();
		}

		public int getTrackIndex () {
			return trackIndex;
		}

		public Animation getAnimation () {
			return animation;
		}

		public void setAnimation (Animation animation) {
			this.animation = animation;
		}

		public boolean getLoop () {
			return loop;
		}

		public void setLoop (boolean loop) {
			this.loop = loop;
		}

		/** Seconds to postpone playing the animation. When a track entry is the current track entry, delay postpones incrementing
		 * the track time. When a track entry is queued, delay is the time from the start of the previous animation to when the
		 * track entry will become the current track entry. */
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

		/** The track time in seconds when this animation will be removed from the track. Defaults to the animation duration for
		 * non-looping animations and to {@link Integer#MAX_VALUE} for looping animations. If the track end time is reached, no
		 * other animations are queued for playback, and mixing from any previous animations is complete, then the track is cleared,
		 * leaving skeletons in their last pose.
		 * <p>
		 * It may be desired to use {@link AnimationState#addEmptyAnimation(int, float, float)} to mix the skeletons back to the
		 * setup pose, rather than leaving them in their last pose. */
		public float getTrackEnd () {
			return trackEnd;
		}

		public void setTrackEnd (float trackEnd) {
			this.trackEnd = trackEnd;
		}

		/** Seconds when this animation starts, both initially and after looping. Defaults to 0.
		 * <p>
		 * When changing the animation start time, it often makes sense to set {@link #getAnimationLast()} to the same value to
		 * prevent timeline keys before the start time from triggering. */
		public float getAnimationStart () {
			return animationStart;
		}

		public void setAnimationStart (float animationStart) {
			this.animationStart = animationStart;
		}

		/** Seconds for the last frame of this animation. Non-looping animations won't play past this time. Looping animations will
		 * loop back to {@link #getAnimationStart()} at this time. Defaults to the animation duration. */
		public float getAnimationEnd () {
			return animationEnd;
		}

		public void setAnimationEnd (float animationEnd) {
			this.animationEnd = animationEnd;
		}

		/** The time in seconds this animation was last applied. Some timelines use this for one-time triggers. Eg, when this
		 * animation is applied, event timelines will fire all events between the animation last time (exclusive) and animation time
		 * (inclusive). Defaults to -1 to ensure triggers on frame 0 happen the first time this animation is applied. */
		public float getAnimationLast () {
			return animationLast;
		}

		public void setAnimationLast (float animationLast) {
			this.animationLast = animationLast;
			nextAnimationLast = animationLast;
		}

		/** Uses {@link #getTrackTime()} to compute the animation time between {@link #getAnimationStart()} and
		 * {@link #getAnimationEnd()}. When the track time is 0, the animation time is equal to the animation start time. */
		public float getAnimationTime () {
			if (loop) {
				float duration = animationEnd - animationStart;
				if (duration == 0) return animationStart;
				return (trackTime % duration) + animationStart;
			}
			return Math.min(trackTime + animationStart, animationEnd);
		}

		/** Multiplier for the delta time when the animation state is updated, causing time for this animation to play slower or
		 * faster. Defaults to 1. */
		public float getTimeScale () {
			return timeScale;
		}

		public void setTimeScale (float timeScale) {
			this.timeScale = timeScale;
		}

		/** The listener for events generated by this track entry, or null. */
		public AnimationStateListener getListener () {
			return listener;
		}

		/** @param listener May be null. */
		public void setListener (AnimationStateListener listener) {
			this.listener = listener;
		}

		/** Values < 1 mix this animation with the last skeleton pose. Defaults to 1, which overwrites the last skeleton pose with
		 * this animation.
		 * <p>
		 * Typically track 0 is used to completely pose the skeleton, then alpha can be used on higher tracks. It doesn't make sense
		 * to use alpha on track 0 if the skeleton pose is from the last frame render. */
		public float getAlpha () {
			return alpha;
		}

		public void setAlpha (float alpha) {
			this.alpha = alpha;
		}

		/** When the mix percentage (mix time / mix duration) is less than the event threshold, event timelines for the animation
		 * being mixed out will be applied. Defaults to 0, so event timelines are not applied for an animation being mixed out. */
		public float getEventThreshold () {
			return eventThreshold;
		}

		public void setEventThreshold (float eventThreshold) {
			this.eventThreshold = eventThreshold;
		}

		/** When the mix percentage (mix time / mix duration) is less than the attachment threshold, attachment timelines for the
		 * animation being mixed out will be applied. Defaults to 0, so attachment timelines are not applied for an animation being
		 * mixed out. */
		public float getAttachmentThreshold () {
			return attachmentThreshold;
		}

		public void setAttachmentThreshold (float attachmentThreshold) {
			this.attachmentThreshold = attachmentThreshold;
		}

		/** When the mix percentage (mix time / mix duration) is less than the draw order threshold, draw order timelines for the
		 * animation being mixed out will be applied. Defaults to 0, so draw order timelines are not applied for an animation being
		 * mixed out. */
		public float getDrawOrderThreshold () {
			return drawOrderThreshold;
		}

		public void setDrawOrderThreshold (float drawOrderThreshold) {
			this.drawOrderThreshold = drawOrderThreshold;
		}

		/** The animation queued to start after this animation, or null. */
		public TrackEntry getNext () {
			return next;
		}

		/** Returns true if at least one loop has been completed. */
		public boolean isComplete () {
			return trackTime >= animationEnd - animationStart;
		}

		/** Seconds from 0 to the mix duration when mixing from the previous animation to this animation. May be slightly more than
		 * {@link #getMixDuration()}. */
		public float getMixTime () {
			return mixTime;
		}

		public void setMixTime (float mixTime) {
			this.mixTime = mixTime;
		}

		/** Seconds for mixing from the previous animation to this animation. Defaults to the value provided by
		 * {@link AnimationStateData} based on the animation before this animation (if any).
		 * <p>
		 * The mix duration must be set before {@link AnimationState#update(float)} is next called. */
		public float getMixDuration () {
			return mixDuration;
		}

		public void setMixDuration (float mixDuration) {
			this.mixDuration = mixDuration;
		}

		/** The track entry for the previous animation when mixing from the previous animation to this animation, or null if no
		 * mixing is currently occuring. */
		public TrackEntry getMixingFrom () {
			return mixingFrom;
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
					if (entry.listener != null) entry.listener.end(entry);
					for (int ii = 0; ii < listeners.size; ii++)
						listeners.get(ii).start(entry);
					break;
				case interrupt:
					if (entry.listener != null) entry.listener.end(entry);
					for (int ii = 0; ii < listeners.size; ii++)
						listeners.get(ii).interrupt(entry);
					break;
				case end:
					if (entry.listener != null) entry.listener.end(entry);
					for (int ii = 0; ii < listeners.size; ii++)
						listeners.get(ii).end(entry);
					// Fall through.
				case dispose:
					if (entry.listener != null) entry.listener.end(entry);
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

	static public interface AnimationStateListener {
		/** Invoked when this entry has been set as the current entry. */
		public void start (TrackEntry entry);

		/** Invoked when another entry has replaced this entry as the current entry. This entry may continue being applied for
		 * mixing. */
		public void interrupt (TrackEntry entry);

		/** Invoked when this entry is no longer the current entry and will never be applied again. */
		public void end (TrackEntry entry);

		/** Invoked when this entry will be disposed. References to the entry should not be kept after dispose is called, as it may
		 * be destroyed or reused. */
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
