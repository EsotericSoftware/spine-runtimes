/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*****************************************************************************/

package spine.animation;

class RGB2Timeline extends CurveTimeline implements SlotTimeline {
	private static inline var ENTRIES:Int = 7;
	private static inline var R:Int = 1;
	private static inline var G:Int = 2;
	private static inline var B:Int = 3;
	private static inline var R2:Int = 4;
	private static inline var G2:Int = 5;
	private static inline var B2:Int = 6;

	private var slotIndex:Int = 0;

	public function new(frameCount:Int, bezierCount:Int, slotIndex:Int) {
		super(frameCount, bezierCount, [Property.rgb + "|" + slotIndex, Property.rgb2 + "|" + slotIndex]);
		this.slotIndex = slotIndex;
	}

	public override function getFrameEntries():Int {
		return ENTRIES;
	}

	public function getSlotIndex():Int {
		return slotIndex;
	}

	/** Sets the time in seconds, light, and dark colors for the specified key frame. */
	public function setFrame(frame:Int, time:Float, r:Float, g:Float, b:Float, r2:Float, g2:Float, b2:Float):Void {
		frame *= ENTRIES;
		frames[frame] = time;
		frames[frame + R] = r;
		frames[frame + G] = g;
		frames[frame + B] = b;
		frames[frame + R2] = r2;
		frames[frame + G2] = g2;
		frames[frame + B2] = b2;
	}

	public override function apply(skeleton:Skeleton, lastTime:Float, time:Float, events:Array<Event>, alpha:Float, blend:MixBlend,
			direction:MixDirection):Void {
		var slot:Slot = skeleton.slots[slotIndex];
		if (!slot.bone.active)
			return;

		var light:Color = slot.color, dark:Color = slot.darkColor;
		var setupLight:Color, setupDark:Color;
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

		var r:Float = 0, g:Float = 0, b:Float = 0, a:Float = 0, r2:Float = 0, g2:Float = 0, b2:Float = 0;
		var i:Int = Timeline.search(frames, time, ENTRIES);
		var curveType:Int = Std.int(curves[Std.int(i / ENTRIES)]);
		switch (curveType) {
			case CurveTimeline.LINEAR:
				var before:Float = frames[i];
				r = frames[i + R];
				g = frames[i + G];
				b = frames[i + B];
				r2 = frames[i + R2];
				g2 = frames[i + G2];
				b2 = frames[i + B2];
				var t:Float = (time - before) / (frames[i + ENTRIES] - before);
				r += (frames[i + ENTRIES + R] - r) * t;
				g += (frames[i + ENTRIES + G] - g) * t;
				b += (frames[i + ENTRIES + B] - b) * t;
				r2 += (frames[i + ENTRIES + R2] - r2) * t;
				g2 += (frames[i + ENTRIES + G2] - g2) * t;
				b2 += (frames[i + ENTRIES + B2] - b2) * t;
			case CurveTimeline.STEPPED:
				r = frames[i + R];
				g = frames[i + G];
				b = frames[i + B];
				r2 = frames[i + R2];
				g2 = frames[i + G2];
				b2 = frames[i + B2];
			default:
				r = getBezierValue(time, i, R, curveType - CurveTimeline.BEZIER);
				g = getBezierValue(time, i, G, curveType + CurveTimeline.BEZIER_SIZE - CurveTimeline.BEZIER);
				b = getBezierValue(time, i, B, curveType + CurveTimeline.BEZIER_SIZE * 2 - CurveTimeline.BEZIER);
				r2 = getBezierValue(time, i, R2, curveType + CurveTimeline.BEZIER_SIZE * 3 - CurveTimeline.BEZIER);
				g2 = getBezierValue(time, i, G2, curveType + CurveTimeline.BEZIER_SIZE * 4 - CurveTimeline.BEZIER);
				b2 = getBezierValue(time, i, B2, curveType + CurveTimeline.BEZIER_SIZE * 5 - CurveTimeline.BEZIER);
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
