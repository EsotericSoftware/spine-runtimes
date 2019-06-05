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

		override public function apply(skeleton : Skeleton, lastTime : Number, time : Number, firedEvents : Vector.<Event>, alpha : Number, blend : MixBlend, direction : MixDirection) : void {
			var frames : Vector.<Number> = this.frames;
			var slot : Slot = skeleton.slots[slotIndex];
			if (!slot.bone.active) return;

			if (time < frames[0]) {
				switch (blend) {
				case MixBlend.setup:
					slot.color.setFromColor(slot.data.color);
					return;
				case MixBlend.first:
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
				if (blend == MixBlend.setup) {
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
