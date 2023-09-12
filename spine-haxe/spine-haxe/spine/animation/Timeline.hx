package spine.animation;

import spine.Event;
import spine.Skeleton;
import openfl.Vector;

class Timeline {
	public var propertyIds:Vector<String>;
	public var frames:Vector<Float>;

	public function new(frameCount:Int, propertyIds:Vector<String>) {
		this.propertyIds = propertyIds;
		frames = new Vector<Float>(frameCount * getFrameEntries(), true);
	}

	public function getFrameEntries():Int {
		return 1;
	}

	public function getFrameCount():Int {
		return Std.int(frames.length / getFrameEntries());
	}

	public function getDuration():Float {
		return frames[frames.length - getFrameEntries()];
	}

	public function apply(skeleton:Skeleton, lastTime:Float, time:Float, events:Vector<Event>, alpha:Float, blend:MixBlend, direction:MixDirection):Void {
		throw new SpineException("Timeline implementations must override apply()");
	}

	public static function search1(frames:Vector<Float>, time:Float):Int {
		var n:Int = frames.length;
		for (i in 1...n) {
			if (frames[i] > time)
				return i - 1;
		}
		return n - 1;
	}

	public static function search(values:Vector<Float>, time:Float, step:Int):Int {
		var n:Int = values.length;
		var i:Int = step;
		while (i < n) {
			if (values[i] > time)
				return i - step;
			i += step;
		}
		return n - step;
	}

	public function toString():String {
		return "Timeline " + Type.getClassName(Type.getClass(this));
	}
}
