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
	import spine.Color;
	import spine.Event;
	import spine.Skeleton;
	import spine.Slot;

	public class ColorTimeline extends CurveTimeline {
		static public const ENTRIES : int = 5;
		static internal const PREV_TIME : int = -5, PREV_R : int = -4, PREV_G : int = -3, PREV_B : int = -2, PREV_A : int = -1;
		static internal const R : int = 1, G : int = 2, B : int = 3, A : int = 4;
		public var slotIndex : int;
		public var frames : Vector.<Number>; // time, r, g, b, a, ...

		public function ColorTimeline(frameCount : int) {
			super(frameCount);
			frames = new Vector.<Number>(frameCount * 5, true);
		}

		override public function getPropertyId() : int {
			return (TimelineType.color.ordinal << 24) + slotIndex;
		}

		/** Sets the time and value of the specified keyframe. */
		public function setFrame(frameIndex : int, time : Number, r : Number, g : Number, b : Number, a : Number) : void {
			frameIndex *= ENTRIES;
			frames[frameIndex] = time;
			frames[int(frameIndex + R)] = r;
			frames[int(frameIndex + G)] = g;
			frames[int(frameIndex + B)] = b;
			frames[int(frameIndex + A)] = a;
		}

		override public function apply(skeleton : Skeleton, lastTime : Number, time : Number, firedEvents : Vector.<Event>, alpha : Number, pose : MixPose, direction : MixDirection) : void {
			var frames : Vector.<Number> = this.frames;
			var slot : Slot = skeleton.slots[slotIndex];

			if (time < frames[0]) {
				switch (pose) {
				case MixPose.setup:
					slot.color.setFromColor(slot.data.color);
					return;
				case MixPose.current:
					var color : Color = slot.color, setup : Color = slot.data.color;
					color.add((setup.r - color.r) * alpha, (setup.g - color.g) * alpha, (setup.b - color.b) * alpha,
						(setup.a - color.a) * alpha);
				}
				return;
			}

			var r : Number, g : Number, b : Number, a : Number;
			if (time >= frames[frames.length - ENTRIES]) { // Time is after last frame.
				var i : int = frames.length;
				r = frames[i + PREV_R];
				g = frames[i + PREV_G];
				b = frames[i + PREV_B];
				a = frames[i + PREV_A];
			} else {
				// Interpolate between the previous frame and the current frame.
				var frame : int = Animation.binarySearch(frames, time, ENTRIES);
				r = frames[frame + PREV_R];
				g = frames[frame + PREV_G];
				b = frames[frame + PREV_B];
				a = frames[frame + PREV_A];
				var frameTime : Number = frames[frame];
				var percent : Number = getCurvePercent(frame / ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

				r += (frames[frame + R] - r) * percent;
				g += (frames[frame + G] - g) * percent;
				b += (frames[frame + B] - b) * percent;
				a += (frames[frame + A] - a) * percent;
			}
			if (alpha == 1) {
				slot.color.setFrom(r, g, b, a);
			} else {
				if (pose == MixPose.setup) {
					slot.color.setFromColor(slot.data.color);
				}
				slot.color.r += (r - slot.color.r) * alpha;
				slot.color.g += (g - slot.color.g) * alpha;
				slot.color.b += (b - slot.color.b) * alpha;
				slot.color.a += (a - slot.color.a) * alpha;
			}
		}
	}
}