package spine {
import spine.animation.Animation;

public class AnimationStateData {
	private var _skeletonData:SkeletonData;
	private var animationToMixTime:Object = new Object();
	public var defaultMix:Number;

	public function AnimationStateData (skeletonData:SkeletonData) {
		_skeletonData = skeletonData;
	}

	public function get skeletonData () : SkeletonData {
		return _skeletonData;
	}

	public function setMixByName (fromName:String, toName:String, duration:Number) : void {
		var from:Animation = _skeletonData.findAnimation(fromName);
		if (from == null)
			throw new ArgumentError("Animation not found: " + fromName);
		var to:Animation = _skeletonData.findAnimation(toName);
		if (to == null)
			throw new ArgumentError("Animation not found: " + toName);
		setMix(from, to, duration);
	}

	public function setMix (from:Animation, to:Animation, duration:Number) : void {
		if (from == null)
			throw new ArgumentError("from cannot be null.");
		if (to == null)
			throw new ArgumentError("to cannot be null.");
		animationToMixTime[from.name + ":" + to.name] = duration;
	}

	public function getMix (from:Animation, to:Animation) : Number {
		var time:Object = animationToMixTime[from.name + ":" + to.name];
		if (time == null)
			return defaultMix;
		return time as Number;
	}
}

}
