package spine.animation {
import spine.Skeleton;

public class Animation {
	internal var _name:String;
	private var _timelines:Vector.<Timeline>;
	public var duration:Number;

	public function Animation (name:String, timelines:Vector.<Timeline>, duration:Number) {
		if (name == null)
			throw new ArgumentError("name cannot be null.");
		if (timelines == null)
			throw new ArgumentError("timelines cannot be null.");
		_name = name;
		_timelines = timelines;
		this.duration = duration;
	}

	public function get timelines () : Vector.<Timeline> {
		return _timelines;
	}

	/** Poses the skeleton at the specified time for this animation. */
	public function apply (skeleton:Skeleton, time:Number, loop:Boolean) : void {
		if (skeleton == null)
			throw new ArgumentError("skeleton cannot be null.");

		if (loop && duration != 0)
			time %= duration;

		for (var i:int = 0, n:int = timelines.length; i < n; i++)
			timelines[i].apply(skeleton, time, 1);
	}

	/** Poses the skeleton at the specified time for this animation mixed with the current pose.
	 * @param alpha The amount of this animation that affects the current pose. */
	public function mix (skeleton:Skeleton, time:Number, loop:Boolean, alpha:Number) : void {
		if (skeleton == null)
			throw new ArgumentError("skeleton cannot be null.");

		if (loop && duration != 0)
			time %= duration;

		for (var i:int = 0, n:int = timelines.length; i < n; i++)
			timelines[i].apply(skeleton, time, alpha);
	}

	public function get name () : String {
		return _name;
	}

	public function toString () : String {
		return _name;
	}

	/** @param target After the first and before the last entry. */
	static public function binarySearch (values:Vector.<Number>, target:Number, step:int) : int {
		var low:int = 0;
		var high:int = values.length / step - 2;
		if (high == 0)
			return step;
		var current:int = high >>> 1;
		while (true) {
			if (values[(current + 1) * step] <= target)
				low = current + 1;
			else
				high = current;
			if (low == high)
				return (low + 1) * step;
			current = (low + high) >>> 1;
		}
		return 0; // Can't happen.
	}

	static public function linearSearch (values:Vector.<Number>, target:Number, step:int) : int {
		for (var i:int = 0, last:int = values.length - step; i <= last; i += step)
			if (values[i] > target)
				return i;
		return -1;
	}
}

}
