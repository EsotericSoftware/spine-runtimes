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

	public class RGBATimeline extends CurveTimeline implements SlotTimeline {
		static internal const ENTRIES : Number = 5;
		static internal const R : Number = 1;
		static internal const G : Number = 2;
		static internal const B : Number = 3;
		static internal const A : Number = 4;

		private var slotIndex : int;

		public function RGBATimeline (frameCount : Number, bezierCount : Number, slotIndex : Number) {
			super(frameCount, bezierCount, [
				Property.rgb + "|" + slotIndex,
				Property.alpha + "|" + slotIndex
			]);
			this.slotIndex = slotIndex;
		}

		public override function getFrameEntries() : int {
			return ENTRIES;
		}

		public function getSlotIndex() : int {
			return slotIndex;
		}

		/** Sets the time in seconds, red, green, blue, and alpha for the specified key frame. */
		public function setFrame (frame: Number, time: Number, r: Number, g: Number, b: Number, a: Number) : void {
			frame *= ENTRIES;
			frames[frame] = time;
			frames[frame + R] = r;
			frames[frame + G] = g;
			frames[frame + B] = b;
			frames[frame + A] = a;
		}

		public override function apply (skeleton : Skeleton, lastTime : Number, time : Number, events : Vector.<Event>, alpha : Number, blend : MixBlend, direction : MixDirection) : void {
			var slot : Slot = skeleton.slots[slotIndex];
			if (!slot.bone.active) return;

			var frames : Vector.<Number> = this.frames;
			var color : Color = slot.color;
			if (time < frames[0]) {
				var setup : Color = slot.data.color;
				switch (blend) {
				case MixBlend.setup:
					color.setFromColor(setup);
					return;
				case MixBlend.first:
					color.add((setup.r - color.r) * alpha, (setup.g - color.g) * alpha, (setup.b - color.b) * alpha,
						(setup.a - color.a) * alpha);
				}
				return;
			}

			var r : Number = 0, g : Number = 0, b : Number = 0, a : Number = 0;
			var i : int = search(frames, time, ENTRIES);
			var curveType : Number = curves[i / ENTRIES];
			switch (curveType) {
			case LINEAR:
				var before : Number = frames[i];
				r = frames[i + R];
				g = frames[i + G];
				b = frames[i + B];
				a = frames[i + A];
				var t : Number = (time - before) / (frames[i + ENTRIES] - before);
				r += (frames[i + ENTRIES + R] - r) * t;
				g += (frames[i + ENTRIES + G] - g) * t;
				b += (frames[i + ENTRIES + B] - b) * t;
				a += (frames[i + ENTRIES + A] - a) * t;
				break;
			case STEPPED:
				r = frames[i + R];
				g = frames[i + G];
				b = frames[i + B];
				a = frames[i + A];
				break;
			default:
				r = getBezierValue(time, i, R, curveType - BEZIER);
				g = getBezierValue(time, i, G, curveType + BEZIER_SIZE - BEZIER);
				b = getBezierValue(time, i, B, curveType + BEZIER_SIZE * 2 - BEZIER);
				a = getBezierValue(time, i, A, curveType + BEZIER_SIZE * 3 - BEZIER);
			}
			if (alpha == 1)
				color.set(r, g, b, a);
			else {
				if (blend == MixBlend.setup) color.setFromColor(slot.data.color);
				color.add((r - color.r) * alpha, (g - color.g) * alpha, (b - color.b) * alpha, (a - color.a) * alpha);
			}
		}
	}
}
