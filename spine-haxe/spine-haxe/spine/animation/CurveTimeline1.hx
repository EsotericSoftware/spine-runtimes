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

/** The base class for a {@link CurveTimeline} that sets one property. */
class CurveTimeline1 extends CurveTimeline {
	private static inline var ENTRIES:Int = 2;
	private static inline var VALUE:Int = 1;

	/** @param bezierCount The maximum number of Bezier curves. See {@link #shrink(Int)}.
	 * @param propertyIds Unique identifiers for the properties the timeline modifies. */
	public function new(frameCount:Int, bezierCount:Int, propertyIds:Array<String>) {
		super(frameCount, bezierCount, propertyIds);
	}

	public override function getFrameEntries():Int {
		return ENTRIES;
	}

	/** Sets the time and values for the specified frame.
	 * @param frame Between 0 and <code>frameCount</code>, inclusive.
	 * @param time The frame time in seconds. */
	public function setFrame(frame:Int, time:Float, value1:Float):Void {
		frame <<= 1;
		frames[frame] = time;
		frames[frame + VALUE] = value1;
	}

	/** Returns the interpolated value for the specified time. */
	public function getCurveValue(time:Float):Float {
		var i:Int = frames.length - 2;
		var ii:Int = 2;
		while (ii <= i) {
			if (frames[ii] > time) {
				i = ii - 2;
				break;
			}
			ii += 2;
		}

		var curveType:Int = Std.int(curves[i >> 1]);
		switch (curveType) {
			case CurveTimeline.LINEAR:
				var before:Float = frames[i], value:Float = frames[i + VALUE];
				return value + (time - before) / (frames[i + ENTRIES] - before) * (frames[i + ENTRIES + VALUE] - value);
			case CurveTimeline.STEPPED:
				return frames[i + VALUE];
		}
		return getBezierValue(time, i, VALUE, curveType - CurveTimeline.BEZIER);
	}

	public function getRelativeValue (time:Float, alpha:Float, blend: MixBlend, current:Float, setup:Float):Float {
		if (time < frames[0]) {
			switch (blend) {
				case MixBlend.setup:
					return setup;
				case MixBlend.first:
					return current + (setup - current) * alpha;
			}
			return current;
		}
		var value:Float = getCurveValue(time);
		switch (blend) {
			case MixBlend.setup:
				return setup + value * alpha;
			case MixBlend.first, MixBlend.replace:
				value += setup - current;
		}
		return current + value * alpha;
	}

	public function getAbsoluteValue (time:Float, alpha:Float, blend: MixBlend, current:Float, setup:Float):Float {
		if (time < frames[0]) {
			switch (blend) {
				case MixBlend.setup:
					return setup;
				case MixBlend.first:
					return current + (setup - current) * alpha;
			}
			return current;
		}
		var value:Float = getCurveValue(time);
		if (blend == MixBlend.setup) return setup + (value - setup) * alpha;
		return current + (value - current) * alpha;
	}

	public function getAbsoluteValue2 (time:Float, alpha:Float, blend: MixBlend, current:Float, setup:Float, value:Float):Float {
		if (time < frames[0]) {
			switch (blend) {
				case MixBlend.setup:
					return setup;
				case MixBlend.first:
					return current + (setup - current) * alpha;
			}
			return current;
		}
		if (blend == MixBlend.setup) return setup + (value - setup) * alpha;
		return current + (value - current) * alpha;
	}

	public function getScaleValue (time:Float, alpha:Float, blend: MixBlend, direction: MixDirection, current:Float, setup:Float):Float {
		var frames:Array<Float> = frames;
		if (time < frames[0]) {
			switch (blend) {
				case MixBlend.setup:
					return setup;
				case MixBlend.first:
					return current + (setup - current) * alpha;
			}
			return current;
		}
		var value:Float = getCurveValue(time) * setup;
		if (alpha == 1) {
			if (blend == MixBlend.add) return current + value - setup;
			return value;
		}
		// Mixing out uses sign of setup or current pose, else use sign of key.
		if (direction == MixDirection.mixOut) {
			switch (blend) {
				case MixBlend.setup:
					return setup + (Math.abs(value) * MathUtils.signum(setup) - setup) * alpha;
				case MixBlend.first, MixBlend.replace:
					return current + (Math.abs(value) * MathUtils.signum(current) - current) * alpha;
			}
		} else {
			var s:Float = 0;
			switch (blend) {
				case MixBlend.setup:
					s = Math.abs(setup) * MathUtils.signum(value);
					return s + (value - s) * alpha;
				case MixBlend.first, MixBlend.replace:
					s = Math.abs(current) * MathUtils.signum(value);
					return s + (value - s) * alpha;
			}
		}
		return current + (value - setup) * alpha;
	}
}
