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
	private float currentTime, previousTime;
	private boolean currentLoop, previousLoop;
	private float mixTime, mixDuration;
	private Array<QueueEntry> queue = new Array();

	public AnimationState (AnimationStateData data) {
		if (data == null) throw new IllegalArgumentException("data cannot be null.");
		this.data = data;
	}

	public void update (float delta) {
		currentTime += delta;
		previousTime += delta;
		mixTime += delta;

		if (queue.size > 0) {
			QueueEntry entry = queue.first();
			if (currentTime >= entry.delay) {
				setAnimationInternal(entry.animation, entry.loop);
				Pools.free(entry);
				queue.removeIndex(0);
			}
		}
	}

	public void apply (Skeleton skeleton) {
		if (current == null) return;
		if (previous != null) {
			previous.apply(skeleton, previousTime, previousLoop);
			float alpha = mixTime / mixDuration;
			if (alpha >= 1) {
				alpha = 1;
				previous = null;
			}
			current.mix(skeleton, currentTime, currentLoop, alpha);
		} else
			current.apply(skeleton, currentTime, currentLoop);
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

	private void setAnimationInternal (Animation animation, boolean loop) {
		previous = null;
		if (animation != null && current != null) {
			mixDuration = data.getMix(current, animation);
			if (mixDuration > 0) {
				mixTime = 0;
				previous = current;
				previousTime = currentTime;
				previousLoop = currentLoop;
			}
		}
		current = animation;
		currentLoop = loop;
		currentTime = 0;
	}

	/** @see #setAnimation(Animation, boolean) */
	public void setAnimation (String animationName, boolean loop) {
		Animation animation = data.getSkeletonData().findAnimation(animationName);
		if (animation == null) throw new IllegalArgumentException("Animation not found: " + animationName);
		setAnimation(animation, loop);
	}

	/** Set the current animation. Any queued animations are cleared and the current animation time is set to 0.
	 * @param animation May be null. */
	public void setAnimation (Animation animation, boolean loop) {
		clearQueue();
		setAnimationInternal(animation, loop);
	}

	/** @see #addAnimation(Animation, boolean) */
	public void addAnimation (String animationName, boolean loop) {
		addAnimation(animationName, loop, 0);
	}

	/** @see #addAnimation(Animation, boolean, float) */
	public void addAnimation (String animationName, boolean loop, float delay) {
		Animation animation = data.getSkeletonData().findAnimation(animationName);
		if (animation == null) throw new IllegalArgumentException("Animation not found: " + animationName);
		addAnimation(animation, loop, delay);
	}

	/** Adds an animation to be played delay seconds after the current or last queued animation, taking into account any mix
	 * duration. */
	public void addAnimation (Animation animation, boolean loop) {
		addAnimation(animation, loop, 0);
	}

	/** Adds an animation to be played delay seconds after the current or last queued animation.
	 * @param delay May be <= 0 to use duration of previous animation minus any mix duration plus the negative delay. */
	public void addAnimation (Animation animation, boolean loop, float delay) {
		QueueEntry entry = Pools.obtain(QueueEntry.class);
		entry.animation = animation;
		entry.loop = loop;

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
	}
}
