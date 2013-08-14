/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
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
 ******************************************************************************/

package com.esotericsoftware.spine;

import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.Pools;

/** Stores state for an animation and automatically mixes between animations. */
public class AnimationState {
	private final AnimationStateData data;
	private Animation current, previous;
	private float currentTime, currentLastTime, previousTime;
	private boolean currentLoop, previousLoop;
	private AnimationStateListener currentListener;
	private float mixTime, mixDuration;
	private final Array<QueueEntry> queue = new Array();
	private final Array<Event> events = new Array();
	private final Array<AnimationStateListener> listeners = new Array();

	public AnimationState (AnimationStateData data) {
		if (data == null) throw new IllegalArgumentException("data cannot be null.");
		this.data = data;
	}

	public void update (float delta) {
		currentLastTime = currentTime;
		currentTime += delta;
		previousTime += delta;
		mixTime += delta;

		if (current != null) {
			float duration = current.getDuration();
			if (currentLoop ? (currentLastTime % duration > currentTime % duration)
				: (currentLastTime < duration && currentTime >= duration)) {
				int count = (int)(currentTime / duration);
				if (currentListener != null) currentListener.complete(count);
				for (int i = 0, n = listeners.size; i < n; i++)
					listeners.get(i).complete(count);
			}
		}

		if (queue.size > 0) {
			QueueEntry entry = queue.first();
			if (currentTime >= entry.delay) {
				setAnimationInternal(entry.animation, entry.loop, entry.listener);
				Pools.free(entry);
				queue.removeIndex(0);
			}
		}
	}

	public void apply (Skeleton skeleton) {
		if (current == null) return;

		Array<Event> events = this.events;
		events.clear();

		if (previous != null) {
			previous.apply(skeleton, Float.MAX_VALUE, previousTime, previousLoop, null);
			float alpha = mixTime / mixDuration;
			if (alpha >= 1) {
				alpha = 1;
				previous = null;
			}
			current.mix(skeleton, currentLastTime, currentTime, currentLoop, events, alpha);
		} else
			current.apply(skeleton, currentLastTime, currentTime, currentLoop, events);

		int listenerCount = listeners.size;
		for (int i = 0, n = events.size; i < n; i++) {
			Event event = events.get(i);
			if (currentListener != null) currentListener.event(event);
			for (int ii = 0; ii < listenerCount; ii++)
				listeners.get(ii).event(event);
		}
	}

	public void clearAnimation () {
		previous = null;
		current = null;
		clearQueue();
	}

	private void clearQueue () {
		Pools.freeAll(queue);
		queue.clear();
	}

	private void setAnimationInternal (Animation animation, boolean loop, AnimationStateListener listener) {
		previous = null;
		if (current != null) {
			if (currentListener != null) currentListener.end();
			for (int i = 0, n = listeners.size; i < n; i++)
				listeners.get(i).end();

			if (animation != null) {
				mixDuration = data.getMix(current, animation);
				if (mixDuration > 0) {
					mixTime = 0;
					previous = current;
					previousTime = currentTime;
					previousLoop = currentLoop;
				}
			}
		}
		current = animation;
		currentLoop = loop;
		currentTime = 0;
		currentListener = listener;

		if (currentListener != null) currentListener.start();
		for (int i = 0, n = listeners.size; i < n; i++)
			listeners.get(i).start();
	}

	/** @see #setAnimation(Animation, boolean, AnimationStateListener) */
	public void setAnimation (String animationName, boolean loop) {
		setAnimation(animationName, loop, null);
	}

	/** @see #setAnimation(Animation, boolean, AnimationStateListener) */
	public void setAnimation (String animationName, boolean loop, AnimationStateListener listener) {
		Animation animation = data.getSkeletonData().findAnimation(animationName);
		if (animation == null) throw new IllegalArgumentException("Animation not found: " + animationName);
		setAnimation(animation, loop, listener);
	}

	/** @see #setAnimation(Animation, boolean, AnimationStateListener) */
	public void setAnimation (Animation animation, boolean loop) {
		setAnimation(animation, loop, null);
	}

	/** Set the current animation. Any queued animations are cleared and the current animation time is set to 0. The specified
	 * listener receives events only for this animation.
	 * @param animation May be null.
	 * @param listener May be null. */
	public void setAnimation (Animation animation, boolean loop, AnimationStateListener listener) {
		clearQueue();
		setAnimationInternal(animation, loop, listener);
	}

	/** @see #addAnimation(Animation, boolean) */
	public void addAnimation (String animationName, boolean loop) {
		addAnimation(animationName, loop, 0, null);
	}

	/** @see #addAnimation(Animation, boolean, float, AnimationStateListener) */
	public void addAnimation (String animationName, boolean loop, float delay, AnimationStateListener listener) {
		Animation animation = data.getSkeletonData().findAnimation(animationName);
		if (animation == null) throw new IllegalArgumentException("Animation not found: " + animationName);
		addAnimation(animation, loop, delay, listener);
	}

	/** Adds an animation to be played delay seconds after the current or last queued animation, taking into account any mix
	 * duration. */
	public void addAnimation (Animation animation, boolean loop) {
		addAnimation(animation, loop, 0, null);
	}

	/** Adds an animation to be played delay seconds after the current or last queued animation.
	 * @param delay May be <= 0 to use duration of previous animation minus any mix duration plus the negative delay.
	 * @param listener May be null. */
	public void addAnimation (Animation animation, boolean loop, float delay, AnimationStateListener listener) {
		QueueEntry entry = Pools.obtain(QueueEntry.class);
		entry.animation = animation;
		entry.loop = loop;
		entry.listener = listener;

		if (delay <= 0) {
			Animation previousAnimation = queue.size == 0 ? current : queue.peek().animation;
			if (previousAnimation != null)
				delay = previousAnimation.getDuration() - data.getMix(previousAnimation, animation) + delay;
			else
				delay = 0;
		}
		entry.delay = delay;

		queue.add(entry);
	}

	/** @return May be null. */
	public Animation getAnimation () {
		return current;
	}

	/** Returns the time within the current animation. */
	public float getTime () {
		return currentTime;
	}

	public void setTime (float time) {
		currentTime = time;
	}

	/** Returns true if no animation is set or if the current time is greater than the animation duration, regardless of looping. */
	public boolean isComplete () {
		return current == null || currentTime >= current.getDuration();
	}

	/** Adds a listener to receive events for all animations. */
	public void addListener (AnimationStateListener listener) {
		if (listener == null) throw new IllegalArgumentException("listener cannot be null.");
		listeners.add(listener);
	}

	public void removeListener (AnimationStateListener listener) {
		listeners.removeValue(listener, true);
	}

	public AnimationStateData getData () {
		return data;
	}

	public String toString () {
		return (current != null && current.getName() != null) ? current.getName() : super.toString();
	}

	static private class QueueEntry {
		Animation animation;
		boolean loop;
		float delay;
		AnimationStateListener listener;
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
