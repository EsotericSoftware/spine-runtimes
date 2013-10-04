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

package spine {
import spine.animation.Animation;

public class AnimationState {
	private var _data:AnimationStateData;
	private var current:Animation;
	private var previous:Animation;
	private var currentTime:Number;
	private var previousTime:Number;
	private var currentLoop:Boolean;
	private var previousLoop:Boolean;
	private var mixTime:Number;
	private var mixDuration:Number;
	private var queue:Vector.<QueueEntry> = new Vector.<QueueEntry>();

	public function AnimationState (data:AnimationStateData) {
		if (data == null)
			throw new ArgumentError("data cannot be null.");
		_data = data;
	}

	public function update (delta:Number) : void {
		currentTime += delta;
		previousTime += delta;
		mixTime += delta;

		if (queue.length > 0) {
			var entry:QueueEntry = queue[0];
			if (currentTime >= entry.delay) {
				setAnimationInternal(entry.animation, entry.loop);
				queue.shift();
			}
		}
	}

	public function apply (skeleton:Skeleton) : void {
		if (!current)
			return;
		if (previous) {
			previous.apply(skeleton, previousTime, previousLoop);
			var alpha:Number = mixTime / mixDuration;
			if (alpha >= 1) {
				alpha = 1;
				previous = null;
			}
			current.mix(skeleton, currentTime, currentLoop, alpha);
		} else
			current.apply(skeleton, currentTime, currentLoop);
	}

	public function clearAnimation () : void {
		previous = null;
		current = null;
		clearQueue();
	}

	private function clearQueue () : void {
		queue.length = 0;
	}

	private function setAnimationInternal (animation:Animation, loop:Boolean) : void {
		previous = null;
		if (animation != null && current != null) {
			mixDuration = _data.getMix(current, animation);
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

	/** @see #setAnimation(Animation, Boolean) */
	public function setAnimationByName (animationName:String, loop:Boolean) : void {
		var animation:Animation = _data.skeletonData.findAnimation(animationName);
		if (animation == null)
			throw new ArgumentError("Animation not found: " + animationName);
		setAnimation(animation, loop);
	}

	/** Set the current animation. Any queued animations are cleared and the current animation time is set to 0.
	 * @param animation May be null. */
	public function setAnimation (animation:Animation, loop:Boolean) : void {
		clearQueue();
		setAnimationInternal(animation, loop);
	}

	/** @see #addAnimation(Animation, Boolean, Number) */
	public function addAnimationByName (animationName:String, loop:Boolean, delay:Number) : void {
		var animation:Animation = _data.skeletonData.findAnimation(animationName);
		if (animation == null)
			throw new ArgumentError("Animation not found: " + animationName);
		addAnimation(animation, loop, delay);
	}

	/** Adds an animation to be played delay seconds after the current or last queued animation.
	 * @param delay May be <= 0 to use duration of previous animation minus any mix duration plus the negative delay. */
	public function addAnimation (animation:Animation, loop:Boolean, delay:Number) : void {
		var entry:QueueEntry = new QueueEntry();
		entry.animation = animation;
		entry.loop = loop;

		if (delay <= 0) {
			var previousAnimation:Animation = queue.length == 0 ? current : queue[queue.length - 1].animation;
			if (previousAnimation != null)
				delay = previousAnimation.duration - _data.getMix(previousAnimation, animation) + delay;
			else
				delay = 0;
		}
		entry.delay = delay;

		queue.push(entry);
	}

	/** @return May be null. */
	public function get animation () : Animation {
		return current;
	}

	/** Returns the time within the current animation. */
	public function get time () : Number {
		return currentTime;
	}

	public function set time (time:Number) : void {
		currentTime = time;
	}

	/** Returns true if no animation is set or if the current time is greater than the animation duration, regardless of looping. */
	public function get isComplete () : Boolean {
		return current == null || currentTime >= current.duration;
	}

	public function get data () : AnimationStateData {
		return _data;
	}

	public function toString () : String {
		return (current != null && current.name != null) ? current.name : super.toString();
	}
}

}

import spine.animation.Animation;

class QueueEntry {
	public var animation:Animation;
	public var loop:Boolean;
	public var delay:Number;
}
