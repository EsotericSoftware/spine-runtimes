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
	import spine.Slot;

	public class DrawOrderTimeline implements Timeline {
		public var frames : Vector.<Number>; // time, ...
		public var drawOrders : Vector.<Vector.<int>>;

		public function DrawOrderTimeline(frameCount : int) {
			frames = new Vector.<Number>(frameCount, true);
			drawOrders = new Vector.<Vector.<int>>(frameCount, true);
		}

		public function get frameCount() : int {
			return frames.length;
		}

		public function getPropertyId() : int {
			return TimelineType.drawOrder.ordinal << 24;
		}

		/** Sets the time and value of the specified keyframe. */
		public function setFrame(frameIndex : int, time : Number, drawOrder : Vector.<int>) : void {
			frames[frameIndex] = time;
			drawOrders[frameIndex] = drawOrder;
		}

		public function apply(skeleton : Skeleton, lastTime : Number, time : Number, firedEvents : Vector.<Event>, alpha : Number, pose : MixPose, direction : MixDirection) : void {
			if (direction == MixDirection.Out && pose == MixPose.setup) {
				for (var ii : int = 0, n : int = skeleton.slots.length; ii < n; ii++)
					skeleton.drawOrder[ii] = skeleton.slots[ii];
				return;
			}

			var drawOrder : Vector.<Slot> = skeleton.drawOrder;
			var slots : Vector.<Slot> = skeleton.slots;
			var slot : Slot;
			var i : int = 0;
			if (time < frames[0]) {
				if (pose == MixPose.setup) {
					for each (slot in slots)
						drawOrder[i++] = slot;
				}
				return;
			}

			var frameIndex : int;
			if (time >= frames[int(frames.length - 1)]) // Time is after last frame.
				frameIndex = frames.length - 1;
			else
				frameIndex = Animation.binarySearch1(frames, time) - 1;

			var drawOrderToSetupIndex : Vector.<int> = drawOrders[frameIndex];
			i = 0;
			if (!drawOrderToSetupIndex) {
				for each (slot in slots)
					drawOrder[i++] = slot;
			} else {
				for each (var setupIndex : int in drawOrderToSetupIndex)
					drawOrder[i++] = slots[setupIndex];
			}
		}
	}
}