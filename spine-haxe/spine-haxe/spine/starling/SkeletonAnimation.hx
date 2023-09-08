package spine.starling;

import starling.core.Starling;
import spine.animation.AnimationState;
import spine.animation.AnimationStateData;
import spine.SkeletonData;
import starling.animation.IAnimatable;

class SkeletonAnimation extends SkeletonSprite implements IAnimatable {
	public var state:AnimationState;

	private var functionUpdate:Void->Void;

	public function new(skeletonData:SkeletonData, stateData:AnimationStateData = null) {
		super(skeletonData);
		state = new AnimationState(stateData != null ? stateData : new AnimationStateData(skeletonData));
	}

	public function advanceTime(time:Float):Void {
		var stage = Starling.current.stage;
		skeleton.update(time);
		state.update(time);
		state.apply(skeleton);
		skeleton.updateWorldTransform();
		this.setRequiresRedraw();
		if (this.functionUpdate != null)
			this.functionUpdate();
	}

	public function setFunctionAnimationUpdate(functionUpdate:Void->Void):Void {
		this.functionUpdate = functionUpdate;
	}
}
