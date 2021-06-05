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

	public class RGBTimeline extends CurveTimeline implements SlotTimeline {
		static internal const ENTRIES : Number = 4;
		static internal const R : Number = 1;
		static internal const G : Number = 2;
		static internal const B : Number = 3;

		private var slotIndex : int;

		public function RGBTimeline (frameCount : Number, bezierCount : Number, slotIndex : Number) {
			super(frameCount, bezierCount, [
				Property.rgb + "|" + slotIndex
			]);
			this.slotIndex = slotIndex;
		}

		public override function getFrameEntries() : int {
			return ENTRIES;
		}

		public function getSlotIndex() : int {
			return slotIndex;
		}

		/** Sets the time in seconds, red, green, and blue for the specified key frame. */
		public function setFrame (frame: Number, time: Number, r: Number, g: Number, b: Number) : void {
			frame <<= 2;
			frames[frame] = time;
			frames[frame + R] = r;
			frames[frame + G] = g;
			frames[frame + B] = b;
		}

		public override function apply (skeleton : Skeleton, lastTime : Number, time : Number, events : Vector.<Event>, alpha : Number, blend : MixBlend, direction : MixDirection) : void {
			var slot : Slot = skeleton.slots[slotIndex];
			if (!slot.bone.active) return;

			var frames : Vector.<Number> = this.frames;
			var color : Color = slot.color, setup : Color;
			if (time < frames[0]) {
				setup = slot.data.color;
				switch (blend) {
				case MixBlend.setup:
					color.r = setup.r;
					color.g = setup.g;
					color.b = setup.b;
					return;
				case MixBlend.first:
					color.r += (setup.r - color.r) * alpha;
					color.g += (setup.g - color.g) * alpha;
					color.b += (setup.b - color.b) * alpha;
				}
				return;
			}

			var r : Number = 0, g : Number = 0, b : Number = 0;
			var i : int = search(frames, time, ENTRIES);
			var curveType : Number = curves[i / ENTRIES];
			switch (curveType) {
			case LINEAR:
				var before : Number = frames[i];
				r = frames[i + R];
				g = frames[i + G];
				b = frames[i + B];
				var t : Number = (time - before) / (frames[i + ENTRIES] - before);
				r += (frames[i + ENTRIES + R] - r) * t;
				g += (frames[i + ENTRIES + G] - g) * t;
				b += (frames[i + ENTRIES + B] - b) * t;
				break;
			case STEPPED:
				r = frames[i + R];
				g = frames[i + G];
				b = frames[i + B];
				break;
			default:
				r = getBezierValue(time, i, R, curveType - BEZIER);
				g = getBezierValue(time, i, G, curveType + BEZIER_SIZE - BEZIER);
				b = getBezierValue(time, i, B, curveType + BEZIER_SIZE * 2 - BEZIER);
			}
			if (alpha == 1) {
				color.r = r;
				color.g = g;
				color.b = b;
			} else {
				if (blend == MixBlend.setup) {
					setup = slot.data.color;
					color.r = setup.r;
					color.g = setup.g;
					color.b = setup.b;
				}
				color.r += (r - color.r) * alpha;
				color.g += (g - color.g) * alpha;
				color.b += (b - color.b) * alpha;
			}
		}
	}
}
