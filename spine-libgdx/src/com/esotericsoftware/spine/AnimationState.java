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

import com.badlogic.gdx.math.MathUtils;

/** Automatically mixes between animations as they change. */
public class AnimationState {
	private final AnimationStateData data;
	Animation current, previous;
	float currentTime, previousTime;
	boolean currentLoop, previousLoop;
	float mixTime, mixDuration;

	public AnimationState (AnimationStateData data) {
		this.data = data;
	}

	public void apply (Skeleton skeleton) {
		if (current == null) return;
		if (previous != null) {
			previous.apply(skeleton, previousTime, previousLoop);
			float alpha = MathUtils.clamp(mixTime / mixDuration, 0, 1);
			current.mix(skeleton, currentTime, currentLoop, alpha);
			if (alpha == 1) previous = null;
		} else {
			current.apply(skeleton, currentTime, currentLoop);
		}
	}

	public void update (float delta) {
		currentTime += delta;
		previousTime += delta;
		mixTime += delta;
	}

	/** Set the current animation. */
	public void setAnimation (Animation animation, boolean loop) {
		setAnimation(animation, loop, 0);
	}

	/** Set the current animation.
	 * @param time The time within the animation to start. */
	public void setAnimation (Animation animation, boolean loop, float time) {
		previous = null;
		if (animation != null && current != null) {
			mixDuration = data.getMixing(current, animation);
			if (mixDuration > 0) {
				mixTime = 0;
				previous = current;
			}
		}
		current = animation;
		currentLoop = loop;
		currentTime = time;
	}

	/** @return May be null. */
	public Animation getAnimation () {
		return current;
	}

	/** Returns the time within the current animation. */
	public float getTime () {
		return currentTime;
	}

	public AnimationStateData getData () {
		return data;
	}
}
