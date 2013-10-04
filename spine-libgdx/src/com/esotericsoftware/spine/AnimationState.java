/******************************************************************************
 * Spine Runtime Software License - Version 1.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Essential, Professional, Enterprise, or Education License must
 *    be purchased from Esoteric Software and the license must remain valid:
 *    http://esotericsoftware.com/
 * 2. Redistributions of source code must retain this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer.
 * 3. Redistributions in binary form must reproduce this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer, in the documentation and/or other materials provided with the
 *    distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine;

import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.Pool.Poolable;
import com.badlogic.gdx.utils.Pools;

/** Stores state for an animation and automatically mixes between animations. */
public class AnimationState {
	private final AnimationStateData data;
	private Array<TrackEntry> tracks = new Array();
	private final Array<Event> events = new Array();
	private final Array<AnimationStateListener> listeners = new Array();
	private float timeScale = 1;

	public AnimationState (AnimationStateData data) {
		if (data == null) throw new IllegalArgumentException("data cannot be null.");
		this.data = data;
	}

	public void update (float delta) {
		delta *= timeScale;
		for (int i = 0, n = tracks.size; i < n; i++) {
			TrackEntry current = tracks.get(i);
			if (current == null) continue;

			float trackDelta = delta * current.timeScale;
			float time = current.time + trackDelta;
			float endTime = current.endTime;

			current.time = time;
			if (current.previous != null) {
				current.previous.time += trackDelta;
				current.mixTime += trackDelta;
			}

			// Check if completed the animation or a loop iteration.
			if (current.loop ? (current.lastTime % endTime > time % endTime) : (current.lastTime < endTime && time >= endTime)) {
				int count = (int)(time / endTime);
				if (current.listener != null) current.listener.complete(i, count);
				for (int ii = 0, nn = listeners.size; ii < nn; ii++)
					listeners.get(ii).complete(i, count);
			}

			TrackEntry next = current.next;
			if (next != null) {
				if (time - trackDelta > next.delay) setCurrent(i, next);
			} else {
				// End non-looping animation when it reaches its end time and there is no next entry.
				if (!current.loop && current.lastTime >= current.endTime) clearTrack(i);
			}
		}
	}

	public void apply (Skeleton skeleton) {
		Array<Event> events = this.events;
		int listenerCount = listeners.size;

		for (int i = 0, n = tracks.size; i < n; i++) {
			TrackEntry current = tracks.get(i);
			if (current == null) continue;

			events.size = 0;

			float time = current.time;
			boolean loop = current.loop;
			if (!loop && time > current.endTime) time = current.endTime;

			TrackEntry previous = current.previous;
			if (previous == null)
				current.animation.apply(skeleton, current.lastTime, time, loop, events);
			else {
				float previousTime = previous.time;
				if (!previous.loop && previousTime > previous.endTime) previousTime = previous.endTime;
				previous.animation.apply(skeleton, previousTime, previousTime, previous.loop, null);

				float alpha = current.mixTime / current.mixDuration;
				if (alpha >= 1) {
					alpha = 1;
					Pools.free(previous);
					current.previous = null;
				}
				current.animation.mix(skeleton, current.lastTime, time, loop, events, alpha);
			}

			for (int ii = 0, nn = events.size; ii < nn; ii++) {
				Event event = events.get(ii);
				if (current.listener != null) current.listener.event(i, event);
				for (int iii = 0; iii < listenerCount; iii++)
					listeners.get(iii).event(i, event);
			}

			current.lastTime = current.time;
		}
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

		if (current.listener != null) current.listener.end(trackIndex);
		for (int i = 0, n = listeners.size; i < n; i++)
			listeners.get(i).end(trackIndex);

		tracks.set(trackIndex, null);
		freeAll(current);
		if (current.previous != null) Pools.free(current.previous);
	}

	private void freeAll (TrackEntry entry) {
		while (entry != null) {
			TrackEntry next = entry.next;
			Pools.free(entry);
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
		if (current != null) {
			if (current.previous != null) {
				Pools.free(current.previous);
				current.previous = null;
			}

			if (current.listener != null) current.listener.end(index);
			for (int i = 0, n = listeners.size; i < n; i++)
				listeners.get(i).end(index);

			entry.mixDuration = data.getMix(current.animation, entry.animation);
			if (entry.mixDuration > 0) {
				entry.mixTime = 0;
				entry.previous = current;
			} else
				Pools.free(current);
		}

		tracks.set(index, entry);

		if (entry.listener != null) entry.listener.start(index);
		for (int i = 0, n = listeners.size; i < n; i++)
			listeners.get(i).start(index);
	}

	/** @see #setAnimation(int, Animation, boolean) */
	public TrackEntry setAnimation (int trackIndex, String animationName, boolean loop) {
		Animation animation = data.getSkeletonData().findAnimation(animationName);
		if (animation == null) throw new IllegalArgumentException("Animation not found: " + animationName);
		return setAnimation(trackIndex, animation, loop);
	}

	/** Set the current animation. Any queued animations are cleared. */
	public TrackEntry setAnimation (int trackIndex, Animation animation, boolean loop) {
		TrackEntry current = expandToIndex(trackIndex);
		if (current != null) freeAll(current.next);

		TrackEntry entry = Pools.obtain(TrackEntry.class);
		entry.animation = animation;
		entry.loop = loop;
		entry.endTime = animation.getDuration();
		setCurrent(trackIndex, entry);
		return entry;
	}

	/** {@link #addAnimation(int, Animation, boolean, float)} */
	public TrackEntry addAnimation (int trackIndex, String animationName, boolean loop, float delay) {
		Animation animation = data.getSkeletonData().findAnimation(animationName);
		if (animation == null) throw new IllegalArgumentException("Animation not found: " + animationName);
		return addAnimation(trackIndex, animation, loop, delay);
	}

	/** Adds an animation to be played delay seconds after the current or last queued animation.
	 * @param delay May be <= 0 to use duration of previous animation minus any mix duration plus the negative delay. */
	public TrackEntry addAnimation (int trackIndex, Animation animation, boolean loop, float delay) {
		TrackEntry entry = Pools.obtain(TrackEntry.class);
		entry.animation = animation;
		entry.loop = loop;
		entry.endTime = animation.getDuration();

		TrackEntry last = expandToIndex(trackIndex);
		if (last != null) {
			while (last.next != null)
				last = last.next;
			last.next = entry;
		} else
			tracks.set(trackIndex, entry);

		if (delay <= 0) {
			if (last != null)
				delay += last.endTime - data.getMix(last.animation, animation);
			else
				delay = 0;
		}
		entry.delay = delay;

		return entry;
	}

	/** @return May be null. */
	public TrackEntry getCurrent (int trackIndex) {
		if (trackIndex >= tracks.size) return null;
		return tracks.get(trackIndex);
	}

	/** Adds a listener to receive events for all animations. */
	public void addListener (AnimationStateListener listener) {
		if (listener == null) throw new IllegalArgumentException("listener cannot be null.");
		listeners.add(listener);
	}

	/** Removes the listener added with {@link #addListener(AnimationStateListener)}. */
	public void removeListener (AnimationStateListener listener) {
		listeners.removeValue(listener, true);
	}

	public float getTimeScale () {
		return timeScale;
	}

	public void setTimeScale (float timeScale) {
		this.timeScale = timeScale;
	}

	public AnimationStateData getData () {
		return data;
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

	static public class TrackEntry implements Poolable {
		TrackEntry next, previous;
		Animation animation;
		boolean loop;
		float delay, time, lastTime, endTime, timeScale = 1;
		float mixTime, mixDuration;
		AnimationStateListener listener;

		public void reset () {
			next = null;
			previous = null;
			animation = null;
			listener = null;
			timeScale = 1;
			lastTime = 0;
			time = 0;
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

		public float getDelay () {
			return delay;
		}

		public void setDelay (float delay) {
			this.delay = delay;
		}

		public float getTime () {
			return time;
		}

		public void setTime (float time) {
			this.time = time;
		}

		public float getEndTime () {
			return endTime;
		}

		public void setEndTime (float endTime) {
			this.endTime = endTime;
		}

		public AnimationStateListener getListener () {
			return listener;
		}

		public void setListener (AnimationStateListener listener) {
			this.listener = listener;
		}

		public float getLastTime () {
			return lastTime;
		}

		public void setLastTime (float lastTime) {
			this.lastTime = lastTime;
		}

		public float getTimeScale () {
			return timeScale;
		}

		public void setTimeScale (float timeScale) {
			this.timeScale = timeScale;
		}

		public TrackEntry getNext () {
			return next;
		}

		public void setNext (TrackEntry next) {
			this.next = next;
		}

		/** Returns true if the current time is greater than the end time, regardless of looping. */
		public boolean isComplete () {
			return time >= endTime;
		}

		public String toString () {
			return animation == null ? "<none>" : animation.name;
		}
	}

	static public interface AnimationStateListener {
		/** Invoked when the current animation triggers an event. */
		public void event (int trackIndex, Event event);

		/** Invoked when the current animation has completed.
		 * @param loopCount The number of times the animation reached the end. */
		public void complete (int trackIndex, int loopCount);

		/** Invoked just after the current animation is set. */
		public void start (int trackIndex);

		/** Invoked just before the current animation is replaced. */
		public void end (int trackIndex);
	}

	static public abstract class AnimationStateAdapter implements AnimationStateListener {
		public void event (int trackIndex, Event event) {
		}

		public void complete (int trackIndex, int loopCount) {
		}

		public void start (int trackIndex) {
		}

		public void end (int trackIndex) {
		}
	}
}
