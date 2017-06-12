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

	public class TwoColorTimeline extends CurveTimeline {
		static public const ENTRIES : int = 8;
		static internal const PREV_TIME : int = -8, PREV_R : int = -7, PREV_G : int = -6, PREV_B : int = -5, PREV_A : int = -4;
		static internal const PREV_R2 : int = -3, PREV_G2 : int = -2, PREV_B2 : int = -1;
		static internal const R : int = 1, G : int = 2, B : int = 3, A : int = 4, R2 : int = 5, G2 : int = 6, B2 : int = 7;
		public var slotIndex : int;
		public var frames : Vector.<Number>; // time, r, g, b, a, ...

		public function TwoColorTimeline(frameCount : int) {
			super(frameCount);
			frames = new Vector.<Number>(frameCount * ENTRIES, true);
		}

		override public function getPropertyId() : int {
			return (TimelineType.twoColor.ordinal << 24) + slotIndex;
		}

		/** Sets the time and value of the specified keyframe. */
		public function setFrame(frameIndex : int, time : Number, r : Number, g : Number, b : Number, a : Number, r2 : Number, g2 : Number, b2 : Number) : void {
			frameIndex *= TwoColorTimeline.ENTRIES;
			this.frames[frameIndex] = time;
			this.frames[frameIndex + TwoColorTimeline.R] = r;
			this.frames[frameIndex + TwoColorTimeline.G] = g;
			this.frames[frameIndex + TwoColorTimeline.B] = b;
			this.frames[frameIndex + TwoColorTimeline.A] = a;
			this.frames[frameIndex + TwoColorTimeline.R2] = r2;
			this.frames[frameIndex + TwoColorTimeline.G2] = g2;
			this.frames[frameIndex + TwoColorTimeline.B2] = b2;
		}

		override public function apply(skeleton : Skeleton, lastTime : Number, time : Number, firedEvents : Vector.<Event>, alpha : Number, pose : MixPose, direction : MixDirection) : void {
			var frames : Vector.<Number> = this.frames;
			var slot : Slot = skeleton.slots[slotIndex];
			var light : Color, dark : Color;

			if (time < frames[0]) {
				switch (pose) {
				case MixPose.setup:
					slot.color.setFromColor(slot.data.color);
					slot.darkColor.setFromColor(slot.data.darkColor);
					return;
				case MixPose.current:
					light = slot.color;
					dark = slot.darkColor;
					var setupLight : Color = slot.data.color, setupDark : Color = slot.data.darkColor;
					light.add((setupLight.r - light.r) * alpha, (setupLight.g - light.g) * alpha, (setupLight.b - light.b) * alpha,
						(setupLight.a - light.a) * alpha);
					dark.add((setupDark.r - dark.r) * alpha, (setupDark.g - dark.g) * alpha, (setupDark.b - dark.b) * alpha, 0);
				}
				return;
			}

			var r : Number, g : Number, b : Number, a : Number, r2 : Number, g2 : Number, b2 : Number;
			if (time >= frames[frames.length - ENTRIES]) { // Time is after last frame.
				var i : int = frames.length;
				r = frames[i + PREV_R];
				g = frames[i + PREV_G];
				b = frames[i + PREV_B];
				a = frames[i + PREV_A];
				r2 = frames[i + PREV_R2];
				g2 = frames[i + PREV_G2];
				b2 = frames[i + PREV_B2];
			} else {
				// Interpolate between the previous frame and the current frame.
				var frame : int = Animation.binarySearch(frames, time, ENTRIES);
				r = frames[frame + PREV_R];
				g = frames[frame + PREV_G];
				b = frames[frame + PREV_B];
				a = frames[frame + PREV_A];
				r2 = frames[frame + PREV_R2];
				g2 = frames[frame + PREV_G2];
				b2 = frames[frame + PREV_B2];
				var frameTime : Number = frames[frame];
				var percent : Number = getCurvePercent(frame / ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

				r += (frames[frame + R] - r) * percent;
				g += (frames[frame + G] - g) * percent;
				b += (frames[frame + B] - b) * percent;
				a += (frames[frame + A] - a) * percent;
				r2 += (frames[frame + R2] - r2) * percent;
				g2 += (frames[frame + G2] - g2) * percent;
				b2 += (frames[frame + B2] - b2) * percent;
			}
			if (alpha == 1) {
				slot.color.setFrom(r, g, b, a);
				slot.darkColor.setFrom(r2, g2, b2, 1);
			} else {
				light = slot.color;
				dark = slot.darkColor;
				if (pose == MixPose.setup) {
					light.setFromColor(slot.data.color);
					dark.setFromColor(slot.data.darkColor);
				}
				light.add((r - light.r) * alpha, (g - light.g) * alpha, (b - light.b) * alpha, (a - light.a) * alpha);
				dark.add((r2 - dark.r) * alpha, (g2 - dark.g) * alpha, (b2 - dark.b) * alpha, 0);
			}
		}
	}
}