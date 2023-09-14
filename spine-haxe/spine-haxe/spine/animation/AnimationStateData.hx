package spine.animation;

import openfl.utils.Object;
import spine.SkeletonData;

class AnimationStateData {
	private var _skeletonData:SkeletonData;
	private var animationToMixTime:Object = new Object();

	public var defaultMix:Float = 0;

	public function new(skeletonData:SkeletonData) {
		_skeletonData = skeletonData;
	}

	public var skeletonData(get, never):SkeletonData;

	private function get_skeletonData():SkeletonData {
		return _skeletonData;
	}

	public function setMixByName(fromName:String, toName:String, duration:Float):Void {
		var from:Animation = _skeletonData.findAnimation(fromName);
		if (from == null)
			throw new SpineException("Animation not found: " + fromName);
		var to:Animation = _skeletonData.findAnimation(toName);
		if (to == null)
			throw new SpineException("Animation not found: " + toName);
		setMix(from, to, duration);
	}

	public function setMix(from:Animation, to:Animation, duration:Float):Void {
		if (from == null)
			throw new SpineException("from cannot be null.");
		if (to == null)
			throw new SpineException("to cannot be null.");
		animationToMixTime[from.name + ":" + to.name] = duration;
	}

	public function getMix(from:Animation, to:Animation):Float {
		var time:Object = animationToMixTime[from.name + ":" + to.name];
		if (time == null)
			return defaultMix;
		return cast(time, Float);
	}
}
