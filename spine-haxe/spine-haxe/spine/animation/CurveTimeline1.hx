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

/** The base class for a spine.animation.CurveTimeline that sets one property. */
abstract class CurveTimeline1 extends CurveTimeline {
	private static inline var ENTRIES:Int = 2;
	private static inline var VALUE:Int = 1;

	/** @param frameCount The number of frames in the timeline.
	 * @param bezierCount The maximum number of Bezier curves. See spine.animation.CurveTimeline.shrink().
	 * @param propertyIds Unique identifiers for the properties the timeline modifies. */
	public function new(frameCount:Int, bezierCount:Int, propertyId:String) {
		super(frameCount, bezierCount, propertyId);
	}

	public override function getFrameEntries():Int {
		return ENTRIES;
	}

	/** Sets the time and value for the specified frame.
	 * @param frame Between 0 and frameCount, inclusive.
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
			default:
				return getBezierValue(time, i, VALUE, curveType - CurveTimeline.BEZIER);
		}
	}

	public function getRelativeValue(time:Float, alpha:Float, fromSetup:Bool, add:Bool, current:Float, setup:Float):Float {
		if (time < frames[0])
			return fromSetup ? setup : current;
		var value:Float = getCurveValue(time);
		return fromSetup ? setup + value * alpha : current + (add ? value : value + setup - current) * alpha;
	}

	public function getAbsoluteValue(time:Float, alpha:Float, fromSetup:Bool, add:Bool, current:Float, setup:Float):Float {
		if (time < frames[0])
			return fromSetup ? setup : current;
		var value:Float = getCurveValue(time);
		return fromSetup ? setup + (add ? value : value - setup) * alpha : current + (add ? value : value - current) * alpha;
	}

	public function getAbsoluteValue2(time:Float, alpha:Float, fromSetup:Bool, add:Bool, current:Float, setup:Float, value:Float):Float {
		if (time < frames[0])
			return fromSetup ? setup : current;
		return fromSetup ? setup + (add ? value : value - setup) * alpha : current + (add ? value : value - current) * alpha;
	}

	public function getScaleValue(time:Float, alpha:Float, fromSetup:Bool, add:Bool, out:Bool, current:Float, setup:Float):Float {
		if (time < frames[0])
			return fromSetup ? setup : current;
		var value:Float = getCurveValue(time) * setup;
		if (alpha == 1 && !add)
			return value;
		var base:Float = fromSetup ? setup : current;
		if (add)
			return base + (value - setup) * alpha;
		if (out)
			return base + (Math.abs(value) * MathUtils.signum(base) - base) * alpha;
		base = Math.abs(base) * MathUtils.signum(value);
		return base + (value - base) * alpha;
	}
}
