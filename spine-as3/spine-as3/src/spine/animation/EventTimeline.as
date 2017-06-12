/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
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
		public function apply(skeleton : Skeleton, lastTime : Number, time : Number, firedEvents : Vector.<Event>, alpha : Number, pose : MixPose, direction : MixDirection) : void {
			if (!firedEvents) return;

			if (lastTime > time) { // Fire events after last time for looped animations.
				apply(skeleton, lastTime, int.MAX_VALUE, firedEvents, alpha, pose, direction);
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