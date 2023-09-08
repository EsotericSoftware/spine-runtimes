package spine.animation;

/** The base class for a {@link CurveTimeline} which sets two properties. */
import openfl.Vector;

class CurveTimeline2 extends CurveTimeline {
	private static inline var ENTRIES:Int = 3;
	private static inline var VALUE1:Int = 1;
	private static inline var VALUE2:Int = 2;

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
	public function setFrame(frame:Int, time:Float, value1:Float, value2:Float):Void {
		frame *= ENTRIES;
		frames[frame] = time;
		frames[frame + VALUE1] = value1;
		frames[frame + VALUE2] = value2;
	}
}
