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

	public class EventTimeline extends Timeline {
		public var events : Vector.<Event>;

		public function EventTimeline(frameCount : int) {
			super(frameCount, [
				Property.event
			]);
			events = new Vector.<Event>(frameCount, true);
		}

		public override function getFrameCount () : int {
			return frames.length;
		}

		/** Sets the time in seconds and the event for the specified key frame. */
		public function setFrame (frame : int, event : Event) : void {
			frames[frame] = event.time;
			events[frame] = event;
		}

		/** Fires events for frames > `lastTime` and <= `time`. */
		public override function apply (skeleton : Skeleton, lastTime : Number, time : Number, firedEvents : Vector.<Event>, alpha : Number, blend : MixBlend, direction : MixDirection) : void {
			if (firedEvents == null) return;

			var frames : Vector.<Number> = this.frames;
			var frameCount : int = frames.length;

			if (lastTime > time) { // Fire events after last time for looped animations.
				apply(skeleton, lastTime, Number.MAX_VALUE, firedEvents, alpha, blend, direction);
				lastTime = -1;
			} else if (lastTime >= frames[frameCount - 1]) // Last time is after last frame.
				return;
			if (time < frames[0]) return; // Time is before first frame.

			var i : int = 0;
			if (lastTime < frames[0])
				i = 0;
			else {
				i = search(frames, lastTime) + 1;
				var frameTime : Number = frames[i];
				while (i > 0) { // Fire multiple events with the same frame.
					if (frames[i - 1] != frameTime) break;
					i--;
				}
			}
			for (; i < frameCount && time >= frames[i]; i++)
				firedEvents.push(events[i]);
		}
	}
}
