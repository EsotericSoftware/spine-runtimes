/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine.animation {
	import spine.Event;
	import spine.Skeleton;
	import flash.utils.Dictionary;

	public class Animation {
		internal var _name : String;
		public var _timelines : Vector.<Timeline>;
		internal var _timelineIds : Dictionary = new Dictionary();
		public var duration : Number;

		public function Animation(name : String, timelines : Vector.<Timeline>, duration : Number) {
			if (name == null) throw new ArgumentError("name cannot be null.");
			if (timelines == null) throw new ArgumentError("timelines cannot be null.");
			_name = name;
			_timelines = timelines;
			for (var i : Number = 0; i < timelines.length; i++)
				_timelineIds[timelines[i].getPropertyId()] = true;
			this.duration = duration;
		}
		
		public function hasTimeline(id: Number) : Boolean {
			return _timelineIds[id] == true;
		}

		public function get timelines() : Vector.<Timeline> {
			return _timelines;
		}

		/** Poses the skeleton at the specified time for this animation. */
		public function apply(skeleton : Skeleton, lastTime : Number, time : Number, loop : Boolean, events : Vector.<Event>, alpha : Number, blend : MixBlend, direction : MixDirection) : void {
			if (skeleton == null) throw new ArgumentError("skeleton cannot be null.");

			if (loop && duration != 0) {
				time %= duration;
				if (lastTime > 0) lastTime %= duration;
			}

			for (var i : int = 0, n : int = timelines.length; i < n; i++)
				timelines[i].apply(skeleton, lastTime, time, events, alpha, blend, direction);
		}

		public function get name() : String {
			return _name;
		}

		public function toString() : String {
			return _name;
		}

		/** @param target After the first and before the last entry. */
		static public function binarySearch(values : Vector.<Number>, target : Number, step : int) : int {
			var low : int = 0;
			var high : int = values.length / step - 2;
			if (high == 0)
				return step;
			var current : int = high >>> 1;
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

		/** @param target After the first and before the last entry. */
		static public function binarySearch1(values : Vector.<Number>, target : Number) : int {
			var low : int = 0;
			var high : int = values.length - 2;
			if (high == 0)
				return 1;
			var current : int = high >>> 1;
			while (true) {
				if (values[int(current + 1)] <= target)
					low = current + 1;
				else
					high = current;
				if (low == high)
					return low + 1;
				current = (low + high) >>> 1;
			}
			return 0; // Can't happen.
		}

		static public function linearSearch(values : Vector.<Number>, target : Number, step : int) : int {
			for (var i : int = 0, last : int = values.length - step; i <= last; i += step)
				if (values[i] > target)
					return i;
			return -1;
		}
	}
}
