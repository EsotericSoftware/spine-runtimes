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

package spine.animation {

import spine.Event;
import spine.Skeleton;

public class AnimationState {
	private var _data:AnimationStateData;
	private var _tracks:Vector.<TrackEntry> = new Vector.<TrackEntry>();
	private var _events:Vector.<Event> = new Vector.<Event>();
	public var onStart:Function, onEnd:Function, onComplete:Function, onEvent:Function;
	public var timeScale:Number = 1;

	public function AnimationState (data:AnimationStateData) {
		if (!data) throw new ArgumentError("data cannot be null.");
		_data = data;
	}

	public function update (delta:Number) : void {
		delta *= timeScale;
		for (var i:int = 0; i < _tracks.length; i++) {
			var current:TrackEntry = _tracks[i];
			if (!current) continue;
			
			var trackDelta:Number = delta * current.timeScale;		
			current.time += trackDelta;
			if (current.previous) {
				current.previous.time += trackDelta;
				current.mixTime += trackDelta;
			}

			var next:TrackEntry = current.next;
			if (next) {
				if (current.lastTime >= next.delay) setCurrent(i, next);
			} else {
				// End non-looping animation when it reaches its end time and there is no next entry.
				if (!current.loop && current.lastTime >= current.endTime) clearTrack(i);
			}
		}
	}

	public function apply (skeleton:Skeleton) : void {
		for (var i:int = 0; i < _tracks.length; i++) {
			var current:TrackEntry = _tracks[i];
			if (!current) continue;
			
			_events.length = 0;
			
			var time:Number = current.time;
			var lastTime:Number = current.lastTime;
			var endTime:Number = current.endTime;
			var loop:Boolean = current.loop;
			if (!loop && time > endTime) time = endTime;
			
			var previous:TrackEntry = current.previous;
			if (!previous)
				current.animation.apply(skeleton, current.lastTime, time, loop, _events);
			else {
				var previousTime:Number = previous.time;
				if (!previous.loop && previousTime > previous.endTime) previousTime = previous.endTime;
				previous.animation.apply(skeleton, previousTime, previousTime, previous.loop, null);
				
				var alpha:Number = current.mixTime / current.mixDuration;
				if (alpha >= 1) {
					alpha = 1;
					current.previous = null;
				}
				current.animation.mix(skeleton, current.lastTime, time, loop, _events, alpha);
			}
			
			for each (var event:Event in _events) {
				if (current.onEvent != null) current.onEvent(i, event);
				if (onEvent != null) onEvent(i, event);
			}

			// Check if completed the animation or a loop iteration.
			if (loop ? (lastTime % endTime > time % endTime) : (lastTime < endTime && time >= endTime)) {
				var count:int = (int)(time / endTime);
				if (current.onComplete != null) current.onComplete(i, count);
				if (onComplete != null) onComplete(i, count);
			}

			current.lastTime = current.time;
		}
	}

	public function clearTracks () : void {
		for (var i:int = 0, n:int = _tracks.length; i < n; i++)
			clearTrack(i);
		_tracks.length = 0; 
	}
	
	public function clearTrack (trackIndex:int) : void {
		if (trackIndex >= _tracks.length) return;
		var current:TrackEntry = _tracks[trackIndex];
		if (!current) return;
		
		if (current.onEnd != null) current.onEnd(trackIndex);
		if (onEnd != null) onEnd(trackIndex);

		_tracks[trackIndex] = null;
	}
	
	private function expandToIndex (index:int) : TrackEntry {
		if (index < _tracks.length) return _tracks[index];
		while (index >= _tracks.length)
			_tracks.push(null);
		return null;
	}
	
	private function setCurrent (index:int, entry:TrackEntry) : void {
		var current:TrackEntry = expandToIndex(index);
		if (current) {
			current.previous = null;
			
			if (current.onEnd != null) current.onEnd(index);
			if (onEnd != null) onEnd(index);
			
			entry.mixDuration = _data.getMix(current.animation, entry.animation);
			if (entry.mixDuration > 0) {
				entry.mixTime = 0;
				entry.previous = current;
			}
		}
		
		_tracks[index] = entry;
		
		if (entry.onStart != null) entry.onStart(index);
		if (onStart != null) onStart(index);
	}
	
	public function setAnimationByName (trackIndex:int, animationName:String, loop:Boolean) : TrackEntry {
		var animation:Animation = _data._skeletonData.findAnimation(animationName);
		if (!animation) throw new ArgumentError("Animation not found: " + animationName);
		return setAnimation(trackIndex, animation, loop);
	}
	
	/** Set the current animation. Any queued animations are cleared. */
	public function setAnimation (trackIndex:int, animation:Animation, loop:Boolean) : TrackEntry {
		var entry:TrackEntry = new TrackEntry();
		entry.animation = animation;
		entry.loop = loop;
		entry.endTime = animation.duration;
		setCurrent(trackIndex, entry);
		return entry;
	}
	
	public function addAnimationByName (trackIndex:int, animationName:String, loop:Boolean, delay:Number) : TrackEntry {
		var animation:Animation = _data._skeletonData.findAnimation(animationName);
		if (!animation) throw new ArgumentError("Animation not found: " + animationName);
		return addAnimation(trackIndex, animation, loop, delay);
	}
	
	/** Adds an animation to be played delay seconds after the current or last queued animation.
	 * @param delay May be <= 0 to use duration of previous animation minus any mix duration plus the negative delay. */
	public function addAnimation (trackIndex:int, animation:Animation, loop:Boolean, delay:Number) : TrackEntry {
		var entry:TrackEntry = new TrackEntry();
		entry.animation = animation;
		entry.loop = loop;
		entry.endTime = animation.duration;
		
		var last:TrackEntry = expandToIndex(trackIndex);
		if (last) {
			while (last.next)
				last = last.next;
			last.next = entry;
		} else
			_tracks[trackIndex] = entry;
		
		if (delay <= 0) {
			if (last) {
				if (last.time < last.endTime) delay += last.endTime - last.time;
				delay -= _data.getMix(last.animation, animation);
			} else
				delay = 0;
		}
		entry.delay = delay;
		
		return entry;
	}
	
	/** May be null. */
	public function getCurrent (trackIndex:int) : TrackEntry {
		if (trackIndex >= _tracks.length) return null;
		return _tracks[trackIndex];
	}

	public function toString () : String {
		var buffer:String = "";
		for each (var entry:TrackEntry in _tracks) {
			if (!entry) continue;
			if (buffer.length > 0) buffer += ", ";
			buffer += entry.toString();
		}
		if (buffer.length == 0) return "<none>";
		return buffer;
	}
}

}
