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
	import spine.Color;
	import spine.Event;
	import spine.Skeleton;
	import spine.Slot;

	public class RGB2Timeline extends CurveTimeline implements SlotTimeline {
		static internal const ENTRIES : Number = 7;
		static internal const R : Number = 1;
		static internal const G : Number = 2;
		static internal const B : Number = 3;
		static internal const R2 : Number = 4;
		static internal const G2 : Number = 5;
		static internal const B2 : Number = 6;

		private var slotIndex : int;

		public function RGB2Timeline (frameCount : Number, bezierCount : Number, slotIndex : Number) {
			super(frameCount, bezierCount, [
				Property.rgb + "|" + slotIndex,
				Property.rgb2 + "|" + slotIndex
			]);
			this.slotIndex = slotIndex;
		}

		public override function getFrameEntries() : int {
			return ENTRIES;
		}

		public function getSlotIndex() : int {
			return slotIndex;
		}

		/** Sets the time in seconds, light, and dark colors for the specified key frame. */
		public function setFrame (frame: Number, time: Number, r: Number, g: Number, b: Number, r2: Number, g2: Number, b2: Number) : void {
			frame *= ENTRIES;
			frames[frame] = time;
			frames[frame + R] = r;
			frames[frame + G] = g;
			frames[frame + B] = b;
			frames[frame + R2] = r2;
			frames[frame + G2] = g2;
			frames[frame + B2] = b2;
		}

		public override function apply (skeleton : Skeleton, lastTime : Number, time : Number, events : Vector.<Event>, alpha : Number, blend : MixBlend, direction : MixDirection) : void {
			var slot : Slot = skeleton.slots[slotIndex];
			if (!slot.bone.active) return;

			var frames : Vector.<Number> = this.frames;
			var light : Color = slot.color, dark : Color = slot.darkColor;
			var setupLight : Color, setupDark : Color;
			if (time < frames[0]) {
				setupLight = slot.data.color;
				setupDark = slot.data.darkColor;
				switch (blend) {
				case MixBlend.setup:
					light.r = setupLight.r;
					light.g = setupLight.g;
					light.b = setupLight.b;
					dark.r = setupDark.r;
					dark.g = setupDark.g;
					dark.b = setupDark.b;
					return;
				case MixBlend.first:
					light.r += (setupLight.r - light.r) * alpha;
					light.g += (setupLight.g - light.g) * alpha;
					light.b += (setupLight.b - light.b) * alpha;
					dark.r += (setupDark.r - dark.r) * alpha;
					dark.g += (setupDark.g - dark.g) * alpha;
					dark.b += (setupDark.b - dark.b) * alpha;
				}
				return;
			}

			var r : Number = 0, g : Number = 0, b : Number = 0, a : Number = 0, r2 : Number = 0, g2 : Number = 0, b2 : Number = 0;
			var i : int = search2(frames, time, ENTRIES);
			var curveType : Number = curves[i / ENTRIES];
			switch (curveType) {
			case LINEAR:
				var before : Number = frames[i];
				r = frames[i + R];
				g = frames[i + G];
				b = frames[i + B];
				r2 = frames[i + R2];
				g2 = frames[i + G2];
				b2 = frames[i + B2];
				var t : Number = (time - before) / (frames[i + ENTRIES] - before);
				r += (frames[i + ENTRIES + R] - r) * t;
				g += (frames[i + ENTRIES + G] - g) * t;
				b += (frames[i + ENTRIES + B] - b) * t;
				r2 += (frames[i + ENTRIES + R2] - r2) * t;
				g2 += (frames[i + ENTRIES + G2] - g2) * t;
				b2 += (frames[i + ENTRIES + B2] - b2) * t;
				break;
			case STEPPED:
				r = frames[i + R];
				g = frames[i + G];
				b = frames[i + B];
				r2 = frames[i + R2];
				g2 = frames[i + G2];
				b2 = frames[i + B2];
				break;
			default:
				r = getBezierValue(time, i, R, curveType - BEZIER);
				g = getBezierValue(time, i, G, curveType + BEZIER_SIZE - BEZIER);
				b = getBezierValue(time, i, B, curveType + BEZIER_SIZE * 2 - BEZIER);
				r2 = getBezierValue(time, i, R2, curveType + BEZIER_SIZE * 3 - BEZIER);
				g2 = getBezierValue(time, i, G2, curveType + BEZIER_SIZE * 4 - BEZIER);
				b2 = getBezierValue(time, i, B2, curveType + BEZIER_SIZE * 5 - BEZIER);
			}

			if (alpha == 1) {
				light.r = r;
				light.g = g;
				light.b = b;
				dark.r = r2;
				dark.g = g2;
				dark.b = b2;
			} else {
				if (blend == MixBlend.setup) {
					setupLight = slot.data.color;
					setupDark = slot.data.darkColor;
					light.r = setupLight.r;
					light.g = setupLight.g;
					light.b = setupLight.b;
					dark.r = setupDark.r;
					dark.g = setupDark.g;
					dark.b = setupDark.b;
				}
				light.r += (r - light.r) * alpha;
				light.g += (g - light.g) * alpha;
				light.b += (b - light.b) * alpha;
				dark.r += (r2 - dark.r) * alpha;
				dark.g += (g2 - dark.g) * alpha;
				dark.b += (b2 - dark.b) * alpha;
			}
		}
	}
}
