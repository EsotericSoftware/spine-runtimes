/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine.animation {
	import spine.SkeletonData;

	public class AnimationStateData {
		internal var _skeletonData : SkeletonData;
		private var animationToMixTime : Object = new Object();
		public var defaultMix : Number = 0;

		public function AnimationStateData(skeletonData : SkeletonData) {
			_skeletonData = skeletonData;
		}

		public function get skeletonData() : SkeletonData {
			return _skeletonData;
		}

		public function setMixByName(fromName : String, toName : String, duration : Number) : void {
			var from : Animation = _skeletonData.findAnimation(fromName);
			if (from == null) throw new ArgumentError("Animation not found: " + fromName);
			var to : Animation = _skeletonData.findAnimation(toName);
			if (to == null) throw new ArgumentError("Animation not found: " + toName);
			setMix(from, to, duration);
		}

		public function setMix(from : Animation, to : Animation, duration : Number) : void {
			if (from == null) throw new ArgumentError("from cannot be null.");
			if (to == null) throw new ArgumentError("to cannot be null.");
			animationToMixTime[from.name + ":" + to.name] = duration;
		}

		public function getMix(from : Animation, to : Animation) : Number {
			var time : Object = animationToMixTime[from.name + ":" + to.name];
			if (time == null) return defaultMix;
			return time as Number;
		}
	}
}
