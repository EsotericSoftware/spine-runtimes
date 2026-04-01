/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

package spine.animation;

/** Changes a slot's spine.Slot.getColor() and spine.Slot.getDarkColor() for two color tinting. */
class RGBA2Timeline extends SlotCurveTimeline {
	private static inline var ENTRIES:Int = 8;
	private static inline var R:Int = 1;
	private static inline var G:Int = 2;
	private static inline var B:Int = 3;
	private static inline var A:Int = 4;
	private static inline var R2:Int = 5;
	private static inline var G2:Int = 6;
	private static inline var B2:Int = 7;

	public function new(frameCount:Int, bezierCount:Int, slotIndex:Int) {
		super(frameCount, bezierCount, slotIndex, Property.rgb
			+ "|"
			+ slotIndex, Property.alpha
			+ "|"
			+ slotIndex, Property.rgb2
			+ "|"
			+ slotIndex);
	}

	public override function getFrameEntries():Int {
		return ENTRIES;
	}

	/** Sets the time, light color, and dark color for the specified frame.
	 * @param frame Between 0 and frameCount, inclusive.
	 * @param time The frame time in seconds. */
	public function setFrame(frame:Int, time:Float, r:Float, g:Float, b:Float, a:Float, r2:Float, g2:Float, b2:Float):Void {
		frame <<= 3;
		frames[frame] = time;
		frames[frame + R] = r;
		frames[frame + G] = g;
		frames[frame + B] = b;
		frames[frame + A] = a;
		frames[frame + R2] = r2;
		frames[frame + G2] = g2;
		frames[frame + B2] = b2;
	}

	public function apply1(slot:Slot, pose:SlotPose, time:Float, alpha:Float, fromSetup:Bool, add:Bool) {
		var light = pose.color, dark = pose.darkColor;
		var r2:Float = 0, g2:Float = 0, b2:Float = 0;
		if (time < frames[0]) {
			if (fromSetup) {
				var setupPose = slot.data.setupPose;
				light.setFromColor(setupPose.color);
				dark.r = setupPose.darkColor.r;
				dark.g = setupPose.darkColor.g;
				dark.b = setupPose.darkColor.b;
			}
			return;
		}
		{
			var r:Float = 0, g:Float = 0, b:Float = 0, a:Float = 0;
			var i:Int = Timeline.search(frames, time, ENTRIES);
			var curveType:Int = Std.int(curves[i >> 3]);
			switch (curveType) {
				case CurveTimeline.LINEAR:
					var before:Float = frames[i];
					r = frames[i + R];
					g = frames[i + G];
					b = frames[i + B];
					a = frames[i + A];
					r2 = frames[i + R2];
					g2 = frames[i + G2];
					b2 = frames[i + B2];
					var t:Float = (time - before) / (frames[i + ENTRIES] - before);
					r += (frames[i + ENTRIES + R] - r) * t;
					g += (frames[i + ENTRIES + G] - g) * t;
					b += (frames[i + ENTRIES + B] - b) * t;
					a += (frames[i + ENTRIES + A] - a) * t;
					r2 += (frames[i + ENTRIES + R2] - r2) * t;
					g2 += (frames[i + ENTRIES + G2] - g2) * t;
					b2 += (frames[i + ENTRIES + B2] - b2) * t;
				case CurveTimeline.STEPPED:
					r = frames[i + R];
					g = frames[i + G];
					b = frames[i + B];
					a = frames[i + A];
					r2 = frames[i + R2];
					g2 = frames[i + G2];
					b2 = frames[i + B2];
				default:
					r = getBezierValue(time, i, R, curveType - CurveTimeline.BEZIER);
					g = getBezierValue(time, i, G, curveType + CurveTimeline.BEZIER_SIZE - CurveTimeline.BEZIER);
					b = getBezierValue(time, i, B, curveType + CurveTimeline.BEZIER_SIZE * 2 - CurveTimeline.BEZIER);
					a = getBezierValue(time, i, A, curveType + CurveTimeline.BEZIER_SIZE * 3 - CurveTimeline.BEZIER);
					r2 = getBezierValue(time, i, R2, curveType + CurveTimeline.BEZIER_SIZE * 4 - CurveTimeline.BEZIER);
					g2 = getBezierValue(time, i, G2, curveType + CurveTimeline.BEZIER_SIZE * 5 - CurveTimeline.BEZIER);
					b2 = getBezierValue(time, i, B2, curveType + CurveTimeline.BEZIER_SIZE * 6 - CurveTimeline.BEZIER);
			}

			if (alpha == 1) {
				light.set(r, g, b, a);
			} else if (fromSetup) {
				var setupPose = slot.data.setupPose;
				var setup = setupPose.color;
				light.set(setup.r
					+ (r - setup.r) * alpha, setup.g
					+ (g - setup.g) * alpha, setup.b
					+ (b - setup.b) * alpha, setup.a
					+ (a - setup.a) * alpha);
				setup = setupPose.darkColor;
				r2 = setup.r + (r2 - setup.r) * alpha;
				g2 = setup.g + (g2 - setup.g) * alpha;
				b2 = setup.b + (b2 - setup.b) * alpha;
			} else {
				light.add((r - light.r) * alpha, (g - light.g) * alpha, (b - light.b) * alpha, (a - light.a) * alpha);
				r2 = dark.r + (r2 - dark.r) * alpha;
				g2 = dark.g + (g2 - dark.g) * alpha;
				b2 = dark.b + (b2 - dark.b) * alpha;
			}
		}
		dark.r = r2 < 0 ? 0 : (r2 > 1 ? 1 : r2);
		dark.g = g2 < 0 ? 0 : (g2 > 1 ? 1 : g2);
		dark.b = b2 < 0 ? 0 : (b2 > 1 ? 1 : b2);
	}
}
