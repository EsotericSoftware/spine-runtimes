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

/** Changes the RGB for a slot's spine.Slot.color. */
class RGBTimeline extends SlotCurveTimeline {
	private static inline var ENTRIES:Int = 4;
	private static inline var R:Int = 1;
	private static inline var G:Int = 2;
	private static inline var B:Int = 3;

	public function new(frameCount:Int, bezierCount:Int, slotIndex:Int) {
		super(frameCount, bezierCount, slotIndex, Property.rgb + "|" + slotIndex);
	}

	public override function getFrameEntries():Int {
		return ENTRIES;
	}

	/** Sets the time and color for the specified frame.
	 * @param frame Between 0 and frameCount, inclusive.
	 * @param time The frame time in seconds. */
	public function setFrame(frame:Int, time:Float, r:Float, g:Float, b:Float):Void {
		frame <<= 2;
		frames[frame] = time;
		frames[frame + R] = r;
		frames[frame + G] = g;
		frames[frame + B] = b;
	}

	public function apply1(slot:Slot, pose:SlotPose, time:Float, alpha:Float, from:MixFrom, add:Bool) {
		var color = pose.color;
		var r:Float = 0, g:Float = 0, b:Float = 0;
		if (time < frames[0]) {
			var setup = slot.data.setupPose.color;
			if (from == MixFrom.setup) {
				color.r = setup.r;
				color.g = setup.g;
				color.b = setup.b;
			} else if (from == MixFrom.first) {
				color.r += (setup.r - color.r) * alpha;
				color.g += (setup.g - color.g) * alpha;
				color.b += (setup.b - color.b) * alpha;
			}
			return;
		}

		var i:Int = Timeline.search(frames, time, ENTRIES);
		var curveType:Int = Std.int(curves[Std.int(i / ENTRIES)]);
		switch (curveType) {
			case CurveTimeline.LINEAR:
				var before:Float = frames[i];
				r = frames[i + R];
				g = frames[i + G];
				b = frames[i + B];
				var t:Float = (time - before) / (frames[i + ENTRIES] - before);
				r += (frames[i + ENTRIES + R] - r) * t;
				g += (frames[i + ENTRIES + G] - g) * t;
				b += (frames[i + ENTRIES + B] - b) * t;
			case CurveTimeline.STEPPED:
				r = frames[i + R];
				g = frames[i + G];
				b = frames[i + B];
			default:
				r = getBezierValue(time, i, R, curveType - CurveTimeline.BEZIER);
				g = getBezierValue(time, i, G, curveType + CurveTimeline.BEZIER_SIZE - CurveTimeline.BEZIER);
				b = getBezierValue(time, i, B, curveType + CurveTimeline.BEZIER_SIZE * 2 - CurveTimeline.BEZIER);
		}
		if (alpha != 1) {
			if (from == MixFrom.setup) {
				var setup = slot.data.setupPose.color;
				r = setup.r + (r - setup.r) * alpha;
				g = setup.g + (g - setup.g) * alpha;
				b = setup.b + (b - setup.b) * alpha;
			} else {
				r = color.r + (r - color.r) * alpha;
				g = color.g + (g - color.g) * alpha;
				b = color.b + (b - color.b) * alpha;
			}
		}
		color.r = r < 0 ? 0 : (r > 1 ? 1 : r);
		color.g = g < 0 ? 0 : (g > 1 ? 1 : g);
		color.b = b < 0 ? 0 : (b > 1 ? 1 : b);
	}
}
