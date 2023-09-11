package spine.animation;

/** The base class for a {@link CurveTimeline} that sets one property. */
import openfl.Vector;

class CurveTimeline1 extends CurveTimeline {
	private static inline var ENTRIES:Int = 2;
	private static inline var VALUE:Int = 1;

	/** @param bezierCount The maximum number of Bezier curves. See {@link #shrink(Int)}.
	 * @param propertyIds Unique identifiers for the properties the timeline modifies. */
	public function new(frameCount:Int, bezierCount:Int, propertyIds:Vector<String>) {
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
}
