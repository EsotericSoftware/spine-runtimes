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
import com.esotericsoftware.spine.Animation.MixDirection;
import com.esotericsoftware.spine.Animation.MixPose;
import com.esotericsoftware.spine.Animation.RotateTimeline;
import com.esotericsoftware.spine.Animation.Timeline;

/** Applies animations over time, queues animations for later playback, mixes (crossfading) between animations, and applies
 * multiple animations on top of each other (layering).
 * <p>
 * See <a href='http://esotericsoftware.com/spine-applying-animations/'>Applying Animations</a> in the Spine Runtimes Guide. */
public class AnimationState {
	static private final Animation emptyAnimation = new Animation("<empty>", new Array(0), 0);
	static private final int SUBSEQUENT = 0, FIRST = 1, DIP = 2, DIP_MIX = 3;

	private AnimationStateData data;
	final Array<TrackEntry> tracks = new Array();
	private final Array<Event> events = new Array();
	final Array<AnimationStateListener> listeners = new Array();
	private final EventQueue queue = new EventQueue();
	private final IntSet propertyIDs = new IntSet();
	private final Array<TrackEntry> mixingTo = new Array();
	boolean animationsChanged;
	private float timeScale = 1;

	Pool<TrackEntry> trackEntryPool = new Pool() {
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

		// Require mixTime > 0 to ensure the mixing from entry was applied at least once.
		if (to.mixTime > 0 && (to.mixTime >= to.mixDuration || to.timeScale == 0)) {
			if (from.totalAlpha == 0) {
				to.mixingFrom = from.mixingFrom;
				to.interruptAlpha = from.interruptAlpha;
				queue.end(from);
			}
			return finished;
		}

		from.animationLast = from.nextAnimationLast;
		from.trackLast = from.nextTrackLast;
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
			MixPose currentPose = i == 0 ? MixPose.current : MixPose.currentLayered;

			// Apply mixing from entries first.
			float mix = current.alpha;
			if (current.mixingFrom != null)
				mix *= applyMixingFrom(current, skeleton, currentPose);
			else if (current.trackTime >= current.trackEnd && current.next == null) //
				mix = 0; // Set to setup pose the last time the entry will be applied.

			// Apply current entry.
			float animationLast = current.animationLast, animationTime = current.getAnimationTime();
			int timelineCount = current.animation.timelines.size;
			Object[] timelines = current.animation.timelines.items;
			if (mix == 1) {
				for (int ii = 0; ii < timelineCount; ii++)
					((Timeline)timelines[ii]).apply(skeleton, animationLast, animationTime, events, 1, MixPose.setup, MixDirection.in);
			} else {
				int[] timelineData = current.timelineData.items;

				boolean firstFrame = current.timelinesRotation.size == 0;
				if (firstFrame) current.timelinesRotation.setSize(timelineCount << 1);
				float[] timelinesRotation = current.timelinesRotation.items;

				for (int ii = 0; ii < timelineCount; ii++) {
					Timeline timeline = (Timeline)timelines[ii];
					MixPose pose = timelineData[ii] >= FIRST ? MixPose.setup : currentPose;
					if (timeline instanceof RotateTimeline)
						applyRotateTimeline(timeline, skeleton, animationTime, mix, pose, timelinesRotation, ii << 1, firstFrame);
					else
						timeline.apply(skeleton, animationLast, animationTime, events, mix, pose, MixDirection.in);
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

	private float applyMixingFrom (TrackEntry to, Skeleton skeleton, MixPose currentPose) {
		TrackEntry from = to.mixingFrom;
		if (from.mixingFrom != null) applyMixingFrom(from, skeleton, currentPose);

		float mix;
		if (to.mixDuration == 0) // Single frame mix to undo mixingFrom changes.
			mix = 1;
		else {
			mix = to.mixTime / to.mixDuration;
			if (mix > 1) mix = 1;
		}

		Array<Event> events = mix < from.eventThreshold ? this.events : null;
		boolean attachments = mix < from.attachmentThreshold, drawOrder = mix < from.drawOrderThreshold;
		float animationLast = from.animationLast, animationTime = from.getAnimationTime();
		int timelineCount = from.animation.timelines.size;
		Object[] timelines = from.animation.timelines.items;
		int[] timelineData = from.timelineData.items;
		Object[] timelineDipMix = from.timelineDipMix.items;

		boolean firstFrame = from.timelinesRotation.size == 0;
		if (firstFrame) from.timelinesRotation.setSize(timelineCount << 1);
		float[] timelinesRotation = from.timelinesRotation.items;

		MixPose pose;
		float alphaDip = from.alpha * to.interruptAlpha, alphaMix = alphaDip * (1 - mix), alpha;
		from.totalAlpha = 0;
		for (int i = 0; i < timelineCount; i++) {
			Timeline timeline = (Timeline)timelines[i];
			switch (timelineData[i]) {
			case SUBSEQUENT:
				if (!attachments && timeline instanceof AttachmentTimeline) continue;
				if (!drawOrder && timeline instanceof DrawOrderTimeline) continue;
				pose = currentPose;
				alpha = alphaMix;
				break;
			case FIRST:
				pose = MixPose.setup;
				alpha = alphaMix;
				break;
			case DIP:
				pose = MixPose.setup;
				alpha = alphaDip;
				break;
			default:
				pose = MixPose.setup;
				alpha = alphaDip;
				TrackEntry dipMix = (TrackEntry)timelineDipMix[i];
				alpha *= Math.max(0, 1 - dipMix.mixTime / dipMix.mixDuration);
				break;
			}
			from.totalAlpha += alpha;
			if (timeline instanceof RotateTimeline)
				applyRotateTimeline(timeline, skeleton, animationTime, alpha, pose, timelinesRotation, i << 1, firstFrame);
			else
				timeline.apply(skeleton, animationLast, animationTime, events, alpha, pose, MixDirection.out);
		}

		if (to.mixDuration > 0) queueEvents(from, animationTime);
		this.events.clear();
		from.nextAnimationLast = animationTime;
		from.nextTrackLast = from.trackTime;

		return mix;
	}

	private void applyRotateTimeline (Timeline timeline, Skeleton skeleton, float time, float alpha, MixPose pose,
		float[] timelinesRotation, int i, boolean firstFrame) {

		if (firstFrame) timelinesRotation[i] = 0;

		if (alpha == 1) {
			timeline.apply(skeleton, 0, time, null, 1, pose, MixDirection.in);
			return;
		}

		RotateTimeline rotateTimeline = (RotateTimeline)timeline;
		Bone bone = skeleton.bones.get(rotateTimeline.boneIndex);
		float[] frames = rotateTimeline.frames;
		if (time < frames[0]) { // Time is before first frame.
			if (pose == MixPose.setup) bone.rotation = bone.data.rotation;
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
		float r1 = pose == MixPose.setup ? bone.data.rotation : bone.rotation;
		float total, diff = r2 - r1;
		if (diff == 0)
			total = timelinesRotation[i];
		else {
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
	 * @param delay Seconds to begin this animation after the start of the previous animation. May be <= 0 to use the animation
	 *           duration of the previous track minus any mix duration plus the <code>delay</code>.
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
				if (duration != 0)
					delay += duration * (1 + (int)(last.trackTime / duration)) - data.getMix(last.animation, animation);
				else
					delay = 0;
			}
		}

		entry.delay = delay;
		return entry;
	}

	/** Sets an empty animation for a track, discarding any queued animations, and sets the track entry's
	 * {@link TrackEntry#getMixDuration()}.
	 * <p>
	 * Mixing out is done by setting an empty animation. A mix duration of 0 still mixes out over one frame.
	 * <p>
	 * To mix in, first set an empty animation and add an animation using {@link #addAnimation(int, Animation, boolean, float)},
	 * then set the {@link TrackEntry#setMixDuration(float)} on the returned track entry. */
	public TrackEntry setEmptyAnimation (int trackIndex, float mixDuration) {
		TrackEntry entry = setAnimation(trackIndex, emptyAnimation, false);
		entry.mixDuration = mixDuration;
		entry.trackEnd = mixDuration;
		return entry;
	}

	/** Adds an empty animation to be played after the current or last queued animation for a track, and sets the track entry's
	 * {@link TrackEntry#getMixDuration()}. If the track is empty, it is equivalent to calling
	 * {@link #setEmptyAnimation(int, float)}.
	 * @param delay Seconds to begin this animation after the start of the previous animation. May be <= 0 to use the animation
	 *           duration of the previous track minus any mix duration plus <code>delay</code>.
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

		IntSet propertyIDs = this.propertyIDs;
		propertyIDs.clear();
		Array<TrackEntry> mixingTo = this.mixingTo;

		for (int i = 0, n = tracks.size; i < n; i++) {
			TrackEntry entry = tracks.get(i);
			if (entry != null) entry.setTimelineData(null, mixingTo, propertyIDs);
		}
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
		TrackEntry next, mixingFrom;
		AnimationStateListener listener;
		int trackIndex;
		boolean loop;
		float eventThreshold, attachmentThreshold, drawOrderThreshold;
		float animationStart, animationEnd, animationLast, nextAnimationLast;
		float delay, trackTime, trackLast, nextTrackLast, trackEnd, timeScale;
		float alpha, mixTime, mixDuration, interruptAlpha, totalAlpha;
		final IntArray timelineData = new IntArray();
		final Array<TrackEntry> timelineDipMix = new Array();
		final FloatArray timelinesRotation = new FloatArray();

		public void reset () {
			next = null;
			mixingFrom = null;
			animation = null;
			listener = null;
			timelineData.clear();
			timelineDipMix.clear();
			timelinesRotation.clear();
		}

		/** @param to May be null. */
		TrackEntry setTimelineData (TrackEntry to, Array<TrackEntry> mixingToArray, IntSet propertyIDs) {
			if (to != null) mixingToArray.add(to);
			TrackEntry lastEntry = mixingFrom != null ? mixingFrom.setTimelineData(this, mixingToArray, propertyIDs) : this;
			if (to != null) mixingToArray.pop();

			Object[] mixingTo = mixingToArray.items;
			int mixingToLast = mixingToArray.size - 1;
			Object[] timelines = animation.timelines.items;
			int timelinesCount = animation.timelines.size;
			int[] timelineData = this.timelineData.setSize(timelinesCount);
			timelineDipMix.clear();
			Object[] timelineDipMix = this.timelineDipMix.setSize(timelinesCount);
			outer:
			for (int i = 0; i < timelinesCount; i++) {
				int id = ((Timeline)timelines[i]).getPropertyId();
				if (!propertyIDs.add(id))
					timelineData[i] = SUBSEQUENT;
				else if (to == null || !to.hasTimeline(id))
					timelineData[i] = FIRST;
				else {
					for (int ii = mixingToLast; ii >= 0; ii--) {
						TrackEntry entry = (TrackEntry)mixingTo[ii];
						if (!entry.hasTimeline(id)) {
							if (entry.mixDuration > 0) {
								timelineData[i] = DIP_MIX;
								timelineDipMix[i] = entry;
								continue outer;
							}
							break;
						}
					}
					timelineData[i] = DIP;
				}
			}
			return lastEntry;
		}

		private boolean hasTimeline (int id) {
			Object[] timelines = animation.timelines.items;
			for (int i = 0, n = animation.timelines.size; i < n; i++)
				if (((Timeline)timelines[i]).getPropertyId() == id) return true;
			return false;
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
		 * It may be desired to use {@link AnimationState#addEmptyAnimation(int, float, float)} to mix the properties back to the
		 * setup pose over time, rather than have it happen instantly. */
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
		 * {@link AnimationStateData#getMix(Animation, Animation)}. In that case, the <code>mixDuration</code> must be set for a new
		 * track entry before {@link AnimationState#update(float)} is next called.
		 * <p>
		 * When using {@link AnimationState#addAnimation(int, Animation, boolean, float)} with a <code>delay</code> <= 0, note the
		 * {@link #getDelay()} is set using the mix duration from the {@link AnimationStateData}. */
		public float getMixDuration () {
			return mixDuration;
		}

		public void setMixDuration (float mixDuration) {
			this.mixDuration = mixDuration;
		}

		/** The track entry for the previous animation when mixing from the previous animation to this animation, or null if no
		 * mixing is currently occuring. When mixing from multiple animations, <code>mixingFrom</code> makes up a linked list. */
		public TrackEntry getMixingFrom () {
			return mixingFrom;
		}

		/** Resets the rotation directions for mixing this entry's rotate timelines. This can be useful to avoid bones rotating the
		 * long way around when using {@link #alpha} and starting animations on other tracks.
		 * <p>
		 * Mixing involves finding a rotation between two others, which has two possible solutions: the short way or the long way
		 * around. The two rotations likely change over time, so which direction is the short or long way also changes. If the short
		 * way was always chosen, bones would flip to the other side when that direction became the long way. TrackEntry chooses the
		 * short way the first time it is applied and remembers that direction. */
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

	/** The interface which can be implemented to receive TrackEntry events.
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
