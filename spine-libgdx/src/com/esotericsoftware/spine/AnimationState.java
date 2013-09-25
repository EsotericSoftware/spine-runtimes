/******************************************************************************
 * Spine Runtime Software License - Version 1.0
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Single User License or Spine Professional License must be
 *    purchased from Esoteric Software and the license must remain valid:
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
	private QueuedAnimation current, previous;
	private float mixTime, mixDuration;
	private final Array<QueuedAnimation> queue = new Array();
	private final Array<AnimationStateListener> listeners = new Array();
	private final Array<Event> events = new Array();

	public AnimationState (AnimationStateData data) {
		if (data == null) throw new IllegalArgumentException("data cannot be null.");
		this.data = data;
	}

	public void update (float delta) {
		QueuedAnimation current = this.current;
		if (current == null) return;

		float time = current.time;
		float duration = current.endTime;

		current.time = time + delta;
		if (previous != null) {
			previous.time += delta;
			mixTime += delta;
		}

		// Check if completed the animation or a loop iteration.
		if (current.loop ? (current.lastTime % duration > time % duration) : (current.lastTime < duration && time >= duration)) {
			int count = (int)(time / duration);
			if (current.listener != null) current.listener.complete(count);
			for (int i = 0, n = listeners.size; i < n; i++)
				listeners.get(i).complete(count);
		}

		if (queue.size > 0) {
			QueuedAnimation entry = queue.first();
			if (time >= entry.delay) {
				if (entry.animation == null)
					clearAnimation();
				else {
					setAnimationEntry(entry);
					queue.removeIndex(0);
				}
			}
		}
	}

	public void apply (Skeleton skeleton) {
		QueuedAnimation current = this.current;
		if (current == null) return;

		Array<Event> events = this.events;
		events.size = 0;

		QueuedAnimation previous = this.previous;
		if (previous != null) {
			previous.animation.apply(skeleton, Integer.MAX_VALUE, previous.time, previous.loop, null);
			float alpha = mixTime / mixDuration;
			if (alpha >= 1) {
				alpha = 1;
				Pools.free(previous);
				this.previous = null;
			}
			current.animation.mix(skeleton, current.lastTime, current.time, current.loop, events, alpha);
		} else
			current.animation.apply(skeleton, current.lastTime, current.time, current.loop, events);

		int listenerCount = listeners.size;
		for (int i = 0, n = events.size; i < n; i++) {
			Event event = events.get(i);
			if (current.listener != null) current.listener.event(event);
			for (int ii = 0; ii < listenerCount; ii++)
				listeners.get(ii).event(event);
		}

		current.lastTime = current.time;
	}

	public void clearAnimation () {
		if (previous != null) {
			Pools.free(previous);
			previous = null;
		}
		if (current != null) {
			Pools.free(current);
			current = null;
		}
		clearQueue();
	}

	private void clearQueue () {
		Pools.freeAll(queue);
		queue.clear();
	}

	private void setAnimationEntry (QueuedAnimation entry) {
		if (previous != null) {
			Pools.free(previous);
			previous = null;
		}

		QueuedAnimation current = this.current;
		if (current != null) {
			if (current.listener != null) current.listener.end();
			for (int i = 0, n = listeners.size; i < n; i++)
				listeners.get(i).end();

			mixDuration = data.getMix(current.animation, entry.animation);
			if (mixDuration > 0) {
				mixTime = 0;
				previous = current;
			} else
				Pools.free(current);
		}
		this.current = entry;

		if (entry != null && entry.listener != null) entry.listener.start();
		for (int i = 0, n = listeners.size; i < n; i++)
			listeners.get(i).start();
	}

	/** @see #setAnimation(Animation, boolean) */
	public QueuedAnimation setAnimation (String animationName, boolean loop) {
		Animation animation = data.getSkeletonData().findAnimation(animationName);
		if (animation == null) throw new IllegalArgumentException("Animation not found: " + animationName);
		return setAnimation(animation, loop);
	}

	/** Set the current animation. Any queued animations are cleared. */
	public QueuedAnimation setAnimation (Animation animation, boolean loop) {
		clearQueue();

		QueuedAnimation entry = Pools.obtain(QueuedAnimation.class);
		entry.animation = animation;
		entry.loop = loop;
		entry.time = 0;
		entry.endTime = animation.getDuration();
		setAnimationEntry(entry);
		return entry;
	}

	/** @see #addAnimation(Animation, boolean) */
	public QueuedAnimation addAnimation (String animationName, boolean loop) {
		return addAnimation(animationName, loop, 0);
	}

	/** @see #addAnimation(Animation, boolean, float) */
	public QueuedAnimation addAnimation (String animationName, boolean loop, float delay) {
		Animation animation = data.getSkeletonData().findAnimation(animationName);
		if (animation == null) throw new IllegalArgumentException("Animation not found: " + animationName);
		return addAnimation(animation, loop, delay);
	}

	/** Adds an animation to be played delay seconds after the current or last queued animation, taking into account any mix
	 * duration.
	 * @param animation May be null to queue clearing the AnimationState. */
	public QueuedAnimation addAnimation (Animation animation, boolean loop) {
		return addAnimation(animation, loop, 0);
	}

	/** Adds an animation to be played delay seconds after the current or last queued animation.
	 * @param animation May be null to queue clearing the AnimationState.
	 * @param delay May be <= 0 to use duration of previous animation minus any mix duration plus the negative delay. */
	public QueuedAnimation addAnimation (Animation animation, boolean loop, float delay) {
		QueuedAnimation entry = Pools.obtain(QueuedAnimation.class);
		entry.animation = animation;
		entry.loop = loop;
		entry.time = 0;
		entry.endTime = animation != null ? animation.getDuration() : 0;

		if (delay <= 0) {
			QueuedAnimation previousEntry = queue.size > 0 ? queue.peek() : current;
			if (previousEntry != null) {
				delay += previousEntry.endTime;
				if (animation != null) delay += -data.getMix(previousEntry.animation, animation);
			} else
				delay = 0;
		}
		entry.delay = delay;

		queue.add(entry);
		return entry;
	}

	public Array<QueuedAnimation> getQueue () {
		return queue;
	}

	/** @return May be null. */
	public QueuedAnimation getCurrent () {
		return current;
	}

	/** @return May be null. */
	public Animation getAnimation () {
		return current != null ? current.animation : null;
	}

	/** Returns the time within the current animation. */
	public float getTime () {
		return current != null ? current.time : 0;
	}

	public void setTime (float time) {
		if (current != null) current.setTime(time);
	}

	/** Returns true if no animation is set or if the current time is greater than the animation duration, regardless of looping. */
	public boolean isComplete () {
		return current == null || current.time >= current.endTime;
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

	public AnimationStateData getData () {
		return data;
	}

	public String toString () {
		if (current == null || current.animation == null) return "<none>";
		return current.animation.getName();
	}

	/** A queued animation. */
	static public class QueuedAnimation implements Poolable {
		Animation animation;
		boolean loop;
		float delay, time, lastTime, endTime;
		AnimationStateListener listener;

		public void reset () {
			animation = null;
			listener = null;
		}

		public Animation getAnimation () {
			return animation;
		}

		public boolean getLoop () {
			return loop;
		}

		public void setLoop (boolean loop) {
			this.loop = loop;
		}

		public float getTime () {
			return time;
		}

		public void setTime (float time) {
			this.time = time;
			if (lastTime < time) lastTime = time;
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

		public float getDelay () {
			return delay;
		}

		public void setDelay (float delay) {
			this.delay = delay;
		}
	}

	static public interface AnimationStateListener {
		/** Invoked when the current animation triggers an event. */
		public void event (Event event);

		/** Invoked when the current animation has completed.
		 * @param loopCount The number of times the animation reached the end. */
		public void complete (int loopCount);

		/** Invoked just after the current animation is set. */
		public void start ();

		/** Invoked just before the current animation is replaced. */
		public void end ();
	}

	static public abstract class AnimationStateAdapter implements AnimationStateListener {
		public void event (Event event) {
		}

		public void complete (int loopCount) {
		}

		public void start () {
		}

		public void end () {
		}
	}
}
