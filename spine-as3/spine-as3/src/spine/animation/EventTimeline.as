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
	import spine.Event;
	import spine.Skeleton;

	public class EventTimeline implements Timeline {
		public var frames : Vector.<Number>; // time, ...
		public var events : Vector.<Event>;

		public function EventTimeline(frameCount : int) {
			frames = new Vector.<Number>(frameCount, true);
			events = new Vector.<Event>(frameCount, true);
		}

		public function get frameCount() : int {
			return frames.length;
		}

		public function getPropertyId() : int {
			return TimelineType.event.ordinal << 24;
		}

		/** Sets the time and value of the specified keyframe. */
		public function setFrame(frameIndex : int, event : Event) : void {
			frames[frameIndex] = event.time;
			events[frameIndex] = event;
		}

		/** Fires events for frames > lastTime and <= time. */
		public function apply(skeleton : Skeleton, lastTime : Number, time : Number, firedEvents : Vector.<Event>, alpha : Number, blend : MixBlend, direction : MixDirection) : void {
			if (!firedEvents) return;

			if (lastTime > time) { // Fire events after last time for looped animations.
				apply(skeleton, lastTime, int.MAX_VALUE, firedEvents, alpha, blend, direction);
				lastTime = -1;
			} else if (lastTime >= frames[int(frameCount - 1)]) // Last time is after last frame.
				return;
			if (time < frames[0]) return; // Time is before first frame.

			var frame : int;
			if (lastTime < frames[0])
				frame = 0;
			else {
				frame = Animation.binarySearch1(frames, lastTime);
				var frameTime : Number = frames[frame];
				while (frame > 0) { // Fire multiple events with the same frame.
					if (frames[int(frame - 1)] != frameTime) break;
					frame--;
				}
			}
			for (; frame < frameCount && time >= frames[frame]; frame++)
				firedEvents[firedEvents.length] = events[frame];
		}
	}
}
