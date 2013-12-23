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

public class Animation {
	internal var _name:String;
	private var _timelines:Vector.<Timeline>;
	public var duration:Number;

	public function Animation (name:String, timelines:Vector.<Timeline>, duration:Number) {
		if (name == null)
			throw new ArgumentError("name cannot be null.");
		if (timelines == null)
			throw new ArgumentError("timelines cannot be null.");
		_name = name;
		_timelines = timelines;
		this.duration = duration;
	}

	public function get timelines () : Vector.<Timeline> {
		return _timelines;
	}

	/** Poses the skeleton at the specified time for this animation. */
	public function apply (skeleton:Skeleton, lastTime:Number, time:Number, loop:Boolean, events:Vector.<Event>) : void {
		if (skeleton == null)
			throw new ArgumentError("skeleton cannot be null.");

		if (loop && duration != 0) {
			time %= duration;
			lastTime %= duration;
		}

		for (var i:int = 0, n:int = timelines.length; i < n; i++)
			timelines[i].apply(skeleton, lastTime, time, events, 1);
	}

	/** Poses the skeleton at the specified time for this animation mixed with the current pose.
	 * @param alpha The amount of this animation that affects the current pose. */
	public function mix (skeleton:Skeleton, lastTime:Number, time:Number, loop:Boolean, events:Vector.<Event>, alpha:Number) : void {
		if (skeleton == null)
			throw new ArgumentError("skeleton cannot be null.");

		if (loop && duration != 0) {
			time %= duration;
			lastTime %= duration;
		}

		for (var i:int = 0, n:int = timelines.length; i < n; i++)
			timelines[i].apply(skeleton, lastTime, time, events, alpha);
	}

	public function get name () : String {
		return _name;
	}

	public function toString () : String {
		return _name;
	}

	/** @param target After the first and before the last entry. */
	static public function binarySearch (values:Vector.<Number>, target:Number, step:int) : int {
		var low:int = 0;
		var high:int = values.length / step - 2;
		if (high == 0)
			return step;
		var current:int = high >>> 1;
		while (true) {
			if (values[int((current + 1) * step)] <= target)
				low = current + 1;
			else
				high = current;
			if (low == high)
				return (low + 1) * step;
			current = (low + high) >>> 1;
		}
		return 0; // Can't happen.
	}

	static public function linearSearch (values:Vector.<Number>, target:Number, step:int) : int {
		for (var i:int = 0, last:int = values.length - step; i <= last; i += step)
			if (values[i] > target)
				return i;
		return -1;
	}
}

}
