/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine;

import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.BooleanArray;
import com.badlogic.gdx.utils.IntArray;
import com.badlogic.gdx.utils.IntSet;
import com.badlogic.gdx.utils.Pool;
import com.badlogic.gdx.utils.Pool.Poolable;
import com.esotericsoftware.spine.Animation.AttachmentTimeline;
import com.esotericsoftware.spine.Animation.DrawOrderTimeline;
import com.esotericsoftware.spine.Animation.Timeline;

/** Stores state for applying one or more animations over time and automatically mixes (crossfades) when animations change. */
public class AnimationState {
	private AnimationStateData data;
	private final Array<TrackEntry> tracks = new Array();
	private final Array<Event> events = new Array();
	private final Array<AnimationStateListener> listeners = new Array();
	private final Pool<TrackEntry> trackEntryPool = new Pool() {
		protected Object newObject () {
			return new TrackEntry();
		}
	};
	private final EventQueue queue = new EventQueue(listeners, trackEntryPool);
	private final IntSet usage = new IntSet();
	private float timeScale = 1;

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
		for (int i = 0; i < tracks.size; i++) {
			TrackEntry current = tracks.get(i);
			if (current == null) continue;

			float currentDelta = delta * current.timeScale;

			if (current.delay > 0) {
				current.delay -= currentDelta;
				if (current.delay > 0) continue;
				currentDelta = -current.delay;
				current.delay = 0;
			}

			TrackEntry next = current.next;
			if (next != null) {
				// When the next entry's delay is passed, change to the next entry.
				float nextTime = current.trackLast - next.delay;
				if (nextTime >= 0) {
					next.delay = 0;
					next.trackTime = nextTime + delta * next.timeScale;
					current.trackTime += currentDelta;
					setCurrent(i, next);
					if (next.mixingFrom != null) next.mixTime += currentDelta;
					continue;
				}
			} else if (current.trackLast >= current.trackEnd) {
				// Clear the track when the end time is reached and there is no next entry.
				clearTrack(i);
				continue;
			}

			current.trackTime += currentDelta;
			if (current.mixingFrom != null) {
				float mixingFromDelta = delta * current.mixingFrom.timeScale;
				current.mixingFrom.trackTime += mixingFromDelta;
				current.mixTime += mixingFromDelta;
			}
		}
	}

	/** Poses the skeleton using the track entry animations. */
	public void apply (Skeleton skeleton) {
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");

		Array<Event> events = this.events;

		for (int i = 0; i < tracks.size; i++) {
			TrackEntry current = tracks.get(i);
			if (current == null) continue;
			if (current.delay > 0) continue;

			float mix = current.alpha;
			if (current.mixingFrom != null) {
				if (current.mixDuration == 0)
					mix = 1;
				else {
					mix *= current.mixTime / current.mixDuration;
					if (mix > 1) mix = 1;
				}
				applyMixingFrom(current.mixingFrom, skeleton, mix);
				if (mix == 1) {
					queue.end(current.mixingFrom);
					current.mixingFrom = null;
					updateSetupPose();
				}
			}

			float animationLast = current.animationLast, animationTime = current.getAnimationTime();
			Array<Timeline> timelines = current.animation.timelines;
			BooleanArray setupPose = current.setupPose;
			for (int ii = 0, n = timelines.size; ii < n; ii++)
				timelines.get(ii).apply(skeleton, animationLast, animationTime, events, mix, setupPose.get(ii), false);
			queueEvents(current, animationTime);
			current.animationLast = animationTime;
			current.trackLast = current.trackTime;
		}

		queue.drain();
	}

	private void applyMixingFrom (TrackEntry entry, Skeleton skeleton, float mix) {
		Array<Event> events = mix < entry.eventThreshold ? this.events : null;
		boolean attachments = mix < entry.attachmentThreshold, drawOrder = mix < entry.drawOrderThreshold;

		float animationLast = entry.animationLast, animationTime = entry.getAnimationTime();
		Array<Timeline> timelines = entry.animation.timelines;
		BooleanArray setupPose = entry.setupPose;
		float alphaFull = entry.alpha, alphaMix = entry.alpha * (1 - mix);
		if (attachments && drawOrder) {
			for (int i = 0, n = timelines.size; i < n; i++) {
				Timeline timeline = timelines.get(i);
				if (setupPose.get(i))
					timeline.apply(skeleton, animationLast, animationTime, events, alphaMix, true, true);
				else
					timeline.apply(skeleton, animationLast, animationTime, events, alphaFull, false, false);
			}
		} else {
			for (int i = 0, n = timelines.size; i < n; i++) {
				Timeline timeline = timelines.get(i);
				if (!attachments && timeline instanceof AttachmentTimeline) continue;
				if (!drawOrder && timeline instanceof DrawOrderTimeline) continue;
				if (setupPose.get(i))
					timeline.apply(skeleton, animationLast, animationTime, events, alphaMix, true, true);
				else
					timeline.apply(skeleton, animationLast, animationTime, events, alphaFull, false, false);
			}
		}

		queueEvents(entry, animationTime);
		entry.animationLast = animationTime;
		entry.trackLast = entry.trackTime;
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

	public void clearTracks () {
		for (int i = 0, n = tracks.size; i < n; i++)
			clearTrack(i);
		tracks.clear();
	}

	public void clearTrack (int trackIndex) {
		if (trackIndex >= tracks.size) return;
		TrackEntry current = tracks.get(trackIndex);
		if (current == null) return;
		freeAll(current.next);

		queue.end(current);
		if (current.mixingFrom != null) queue.end(current.mixingFrom);
		queue.drain();

		tracks.set(trackIndex, null);
	}

	/** @param entry May be null. */
	private void freeAll (TrackEntry entry) {
		while (entry != null) {
			TrackEntry next = entry.next;
			trackEntryPool.free(entry);
			entry = next;
		}
	}

	private TrackEntry expandToIndex (int index) {
		if (index < tracks.size) return tracks.get(index);
		tracks.ensureCapacity(index - tracks.size + 1);
		tracks.size = index + 1;
		return null;
	}

	private void setCurrent (int index, TrackEntry entry) {
		TrackEntry current = expandToIndex(index);
		tracks.set(index, entry);

		queue.start(entry);

		if (current != null) {
			TrackEntry mixingFrom = current.mixingFrom;
			current.mixingFrom = null;

			queue.interrupt(current);

			// If a mix is in progress, mix from the closest animation.
			if (mixingFrom != null && current.mixTime / current.mixDuration < 0.5f) {
				entry.mixingFrom = mixingFrom;
				mixingFrom = current;
			} else
				entry.mixingFrom = current;

			if (mixingFrom != null) queue.end(mixingFrom);
		}

		queue.drain();

		updateSetupPose();
	}

	private void updateSetupPose () {
		usage.clear();
		int i = 0, n = tracks.size;
		for (; i < n; i++) {
			TrackEntry entry = tracks.get(i);
			if (entry == null) continue;
			if (entry.mixingFrom != null) {
				updateFirstSetupPose(entry.mixingFrom);
				updateSetupPose(entry);
			} else
				updateFirstSetupPose(entry);
			i++;
			break;
		}
		for (; i < n; i++) {
			TrackEntry entry = tracks.get(i);
			if (entry == null) continue;
			if (entry.mixingFrom != null) updateSetupPose(entry.mixingFrom);
			updateSetupPose(entry);
		}
	}

	private void updateFirstSetupPose (TrackEntry entry) {
		IntSet usage = this.usage;
		BooleanArray setupPose = entry.setupPose;
		setupPose.clear();
		Array<Timeline> timelines = entry.animation.timelines;
		for (int ii = 0, nn = timelines.size; ii < nn; ii++) {
			Timeline timeline = timelines.get(ii);
			usage.add(timeline.getId());
			setupPose.add(true);
		}
	}

	private void updateSetupPose (TrackEntry entry) {
		IntSet usage = this.usage;
		BooleanArray setupPose = entry.setupPose;
		setupPose.clear();
		Array<Timeline> timelines = entry.animation.timelines;
		for (int ii = 0, nn = timelines.size; ii < nn; ii++) {
			Timeline timeline = timelines.get(ii);
			int id = timeline.getId();
			if (usage.contains(id))
				setupPose.add(false);
			else {
				usage.add(id);
				setupPose.add(true);
			}
		}
	}

	/** @see #setAnimation(int, Animation, boolean) */
	public TrackEntry setAnimation (int trackIndex, String animationName, boolean loop) {
		Animation animation = data.getSkeletonData().findAnimation(animationName);
		if (animation == null) throw new IllegalArgumentException("Animation not found: " + animationName);
		return setAnimation(trackIndex, animation, loop);
	}

	/** Sets the current animation for a track. If the track is empty, the new animation is made the current animation immediately.
	 * Otherwise, any queued animations are discarded and the new animation is queued to become the current animation the next time
	 * {@link #update(float)} is called.
	 * @return A track entry to allow further customization of animation playback. References to the track entry must not be kept
	 *         after {@link AnimationStateListener#end(TrackEntry)}. */
	public TrackEntry setAnimation (int trackIndex, Animation animation, boolean loop) {
		if (animation == null) throw new IllegalArgumentException("animation cannot be null.");
		TrackEntry current = expandToIndex(trackIndex);
		TrackEntry entry = trackEntry(trackIndex, animation, loop, current);
		if (current == null)
			setCurrent(trackIndex, entry);
		else {
			freeAll(current.next);
			if (current.trackLast == -1) // If current was never applied, replace it.
				setCurrent(trackIndex, entry);
			else {
				current.next = entry;
				entry.delay = current.trackLast;
			}
		}
		return entry;
	}

	/** {@link #addAnimation(int, Animation, boolean, float)} */
	public TrackEntry addAnimation (int trackIndex, String animationName, boolean loop, float delay) {
		Animation animation = data.getSkeletonData().findAnimation(animationName);
		if (animation == null) throw new IllegalArgumentException("Animation not found: " + animationName);
		return addAnimation(trackIndex, animation, loop, delay);
	}

	/** Adds an animation to be played after the current or last queued animation for a track.
	 * @param delay Seconds to begin this animation after the start of the previous animation. May be <= 0 to use duration of the
	 *           previous animation minus any mix duration plus the negative delay.
	 * @return A track entry to allow further customization of animation playback. References to the track entry must not be kept
	 *         after {@link AnimationStateListener#end(TrackEntry)}. */
	public TrackEntry addAnimation (int trackIndex, Animation animation, boolean loop, float delay) {
		if (animation == null) throw new IllegalArgumentException("animation cannot be null.");

		TrackEntry last = expandToIndex(trackIndex);
		if (last != null) {
			while (last.next != null)
				last = last.next;
		}

		TrackEntry entry = trackEntry(trackIndex, animation, loop, last);

		if (last == null)
			setCurrent(trackIndex, entry);
		else {
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

	/** @param last May be null. */
	private TrackEntry trackEntry (int trackIndex, Animation animation, boolean loop, TrackEntry last) {
		TrackEntry entry = trackEntryPool.obtain();
		entry.trackIndex = trackIndex;
		entry.animation = animation;
		entry.loop = loop;

		entry.eventThreshold = 0;
		entry.attachmentThreshold = 1;
		entry.drawOrderThreshold = 0;

		entry.delay = 0;
		entry.animationStart = 0;
		entry.animationEnd = animation.getDuration();
		entry.animationLast = -1;
		entry.trackTime = 0;
		entry.trackEnd = loop ? Integer.MAX_VALUE : entry.animationEnd;
		entry.trackLast = -1;
		entry.timeScale = 1;

		entry.alpha = 1;

		entry.mixTime = 0;
		entry.mixDuration = last == null ? 0 : data.getMix(last.animation, animation);
		return entry;
	}

	/** The track entry for the animation currently playing on the track, or null. */
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
		float delay, trackTime, trackLast, trackEnd, animationStart, animationEnd, animationLast, timeScale;
		float alpha;
		float mixTime, mixDuration;
		final BooleanArray setupPose = new BooleanArray();

		public void reset () {
			next = null;
			mixingFrom = null;
			animation = null;
			listener = null;
			setupPose.clear();
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
		 * {@link #getAnimationTime()} and can be set to start the animation at a time other than 0. */
		public float getTrackTime () {
			return trackTime;
		}

		public void setTrackTime (float trackTime) {
			this.trackTime = trackTime;
		}

		/** The track time in seconds when this animation will be removed from the track. If the track end time is reached and no
		 * other animations are queued for playback, the track is cleared. Defaults to the animation duration for non-looping
		 * animations and to {@link Integer#MAX_VALUE} for looping animations. */
		public float getTrackEnd () {
			return trackEnd;
		}

		public void setTrackEnd (float trackEnd) {
			this.trackEnd = trackEnd;
		}

		/** Seconds when this animation starts, both initially and after looping. Defaults to 0.
		 * <p>
		 * When changing the animation start time, it often makes sense to also change {@link #getAnimationLast()} to control when
		 * timelines will trigger. */
		public float getAnimationStart () {
			return animationStart;
		}

		public void setAnimationStart (float animationStart) {
			this.animationStart = animationStart;
		}

		/** Seconds for the last frame of this animation. Non-looping animations won't play past this time. Looping animation will
		 * loop back to {@link #getAnimationStart()} at this time. Defaults to the animation duration. */
		public float getAnimationEnd () {
			return animationEnd;
		}

		public void setAnimationEnd (float animationEnd) {
			this.animationEnd = animationEnd;
		}

		/** The time in seconds this animation was last applied. Some timelines use this for one-time triggers. Eg, when this
		 * animation is applied, event timelines will fire all events between lastTime (exclusive) and time (inclusive). Defaults to
		 * -1 to ensure triggers on frame 0 happen the first time this animation is applied. */
		public float getAnimationLast () {
			return animationLast;
		}

		public void setAnimationLast (float animationLast) {
			this.animationLast = animationLast;
		}

		/** Uses the {@link #getTrackTime() track time} to compute the animation time between the {@link #getAnimationStart()
		 * animation start} and {@link #getAnimationEnd() animation end}. When the track time is 0, the animation time is equal to
		 * the animation start time. */
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

		/** Values < 1 mix this animation with the skeleton pose. Defaults to 1, which overwrites the skeleton pose with this
		 * animation.
		 * <p>
		 * Typically track 0 is used to completely pose the skeleton, then alpha can be used on higher tracks. Generally it doesn't
		 * make sense to use alpha on track 0, since the skeleton pose is probably from the last frame render. */
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

		/** @param next May be null. */
		public void setNext (TrackEntry next) {
			this.next = next;
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
		 * The mix duration must be set before this track entry becomes the current track entry. */
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

	static private class EventQueue {
		static private final int START = 0, EVENT = 1, COMPLETE = 2, INTERRUPT = 3, END = 4;

		private final Array<AnimationStateListener> listeners;
		private final Pool<TrackEntry> trackEntryPool;
		private final Array objects = new Array();
		private final IntArray eventTypes = new IntArray(); // If > 0 it's loop count for a complete event.
		private boolean draining;

		public EventQueue (Array<AnimationStateListener> listeners, Pool<TrackEntry> trackEntryPool) {
			this.listeners = listeners;
			this.trackEntryPool = trackEntryPool;
		}

		public void start (TrackEntry entry) {
			objects.add(entry);
			eventTypes.add(START);
		}

		public void event (TrackEntry entry, Event event) {
			objects.add(entry);
			objects.add(event);
			eventTypes.add(EVENT);
		}

		public void complete (TrackEntry entry) {
			objects.add(entry);
			eventTypes.add(COMPLETE);
		}

		public void interrupt (TrackEntry entry) {
			objects.add(entry);
			eventTypes.add(INTERRUPT);
		}

		public void end (TrackEntry entry) {
			objects.add(entry);
			eventTypes.add(END);
		}

		public void drain () {
			if (draining) return; // Not reentrant.
			draining = true;

			Array objects = this.objects;
			IntArray eventTypes = this.eventTypes;
			Array<AnimationStateListener> listeners = this.listeners;
			for (int e = 0, o = 0; e < eventTypes.size; e++, o++) {
				TrackEntry entry = (TrackEntry)objects.get(o);
				int eventType = eventTypes.get(e);
				switch (eventType) {
				case START:
					if (entry.listener != null) entry.listener.end(entry);
					for (int i = 0; i < listeners.size; i++)
						listeners.get(i).start(entry);
					break;
				case EVENT:
					Event event = (Event)objects.get(++o);
					if (entry.listener != null) entry.listener.event(entry, event);
					for (int i = 0; i < listeners.size; i++)
						listeners.get(i).event(entry, event);
					break;
				case INTERRUPT:
					if (entry.listener != null) entry.listener.end(entry);
					for (int i = 0; i < listeners.size; i++)
						listeners.get(i).interrupt(entry);
					break;
				case END:
					if (entry.listener != null) entry.listener.end(entry);
					for (int i = 0; i < listeners.size; i++)
						listeners.get(i).end(entry);
					trackEntryPool.free(entry);
					break;
				default:
					if (entry.listener != null) entry.listener.complete(entry);
					for (int i = 0; i < listeners.size; i++)
						listeners.get(i).complete(entry);
				}
			}
			clear();

			draining = false;
		}

		public void clear () {
			objects.clear();
			eventTypes.clear();
		}
	}

	static public interface AnimationStateListener {
		/** Invoked just after this animation is set as the current animation. */
		public void start (TrackEntry entry);

		/** Invoked just after another animation is set as the current animation. The animation may continue being applied if there
		 * is a mix duration. */
		public void interrupt (TrackEntry entry);

		/** Invoked when this animation will no longer be applied. After this method returns, no references to the track entry
		 * should be kept because it may be reused. */
		public void end (TrackEntry entry);

		/** Invoked every time this animation completes a loop. */
		public void complete (TrackEntry entry);

		/** Invoked when this animation triggers an event. */
		public void event (TrackEntry entry, Event event);
	}

	static public abstract class AnimationStateAdapter implements AnimationStateListener {
		public void event (TrackEntry entry, Event event) {
		}

		public void complete (TrackEntry entry) {
		}

		public void start (TrackEntry entry) {
		}

		public void interrupt (TrackEntry entry) {
		}

		public void end (TrackEntry entry) {
		}
	}
}
