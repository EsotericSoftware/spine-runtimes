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
	import flash.utils.Dictionary;
	import spine.Poolable;

	public class TrackEntry implements Poolable {
		public var animation : Animation;
		public var next : TrackEntry, mixingFrom : TrackEntry, mixingTo: TrackEntry;
		public var onStart : Listeners = new Listeners();
		public var onInterrupt : Listeners = new Listeners();
		public var onEnd : Listeners = new Listeners();
		public var onDispose : Listeners = new Listeners();
		public var onComplete : Listeners = new Listeners();
		public var onEvent : Listeners = new Listeners();
		public var trackIndex : int;
		public var loop : Boolean, holdPrevious: Boolean;
		public var eventThreshold : Number, attachmentThreshold : Number, drawOrderThreshold : Number;
		public var animationStart : Number, animationEnd : Number, animationLast : Number, nextAnimationLast : Number;
		public var delay : Number, trackTime : Number, trackLast : Number, nextTrackLast : Number, trackEnd : Number, timeScale : Number;
		public var alpha : Number, mixTime : Number, mixDuration : Number, interruptAlpha : Number, totalAlpha : Number = 0;
		public var mixBlend: MixBlend = MixBlend.replace;
		public var timelineMode : Vector.<int> = new Vector.<int>();
		public var timelineHoldMix : Vector.<TrackEntry> = new Vector.<TrackEntry>();
		public var timelinesRotation : Vector.<Number> = new Vector.<Number>();

		public function TrackEntry() {
		}

		public function getAnimationTime() : Number {
			if (loop) {
				var duration : Number = animationEnd - animationStart;
				if (duration == 0) return animationStart;
				return (trackTime % duration) + animationStart;
			}
			return Math.min(trackTime + animationStart, animationEnd);
		}

		public function reset() : void {
			next = null;
			mixingFrom = null;
			mixingTo = null;
			animation = null;
			onStart.listeners.length = 0;
			onInterrupt.listeners.length = 0;
			onEnd.listeners.length = 0;
			onDispose.listeners.length = 0;
			onComplete.listeners.length = 0;
			onEvent.listeners.length = 0;
			timelineMode.length = 0;
			timelineHoldMix.length = 0;
			timelinesRotation.length = 0;
		}		

		public function resetRotationDirection() : void {
			timelinesRotation.length = 0;
		}
	}
}
