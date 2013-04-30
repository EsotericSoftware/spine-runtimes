package spine {
import spine.AnimationState;
import spine.AnimationStateData;
import spine.SkeletonData;

import starling.events.EnterFrameEvent;

public class SkeletonAnimationSprite extends SkeletonSprite {
	public var states:Vector.<AnimationState> = new Vector.<AnimationState>();

	public function SkeletonAnimationSprite (skeletonData:SkeletonData) {
		super(skeletonData);
		addAnimationState();
	}

	override protected function onEnterFrame (event:EnterFrameEvent) : void {
		super.onEnterFrame(event);

		var deltaTime:Number = event.passedTime * timeScale;
		for each (var state:AnimationState in states) {
			state.update(deltaTime);
			state.apply(skeleton);
		}
		skeleton.updateWorldTransform();
	}

	public function addAnimationState (stateData:AnimationStateData = null) : void {
		if (!stateData)
			stateData = new AnimationStateData(skeleton.data);
		states.push(new AnimationState(stateData));
	}

	public function setAnimationStateData (stateData:AnimationStateData, stateIndex:int = 0) : void {
		if (stateIndex < 0 || stateIndex >= states.length)
			throw new ArgumentError("stateIndex out of range.");
		if (!stateData)
			throw new ArgumentError("stateData cannot be null.");
		states[stateIndex] = new AnimationState(stateData);
	}

	public function setMix (fromAnimation:String, toAnimation:String, duration:Number, stateIndex:int = 0) : void {
		if (stateIndex < 0 || stateIndex >= states.length)
			throw new ArgumentError("stateIndex out of range.");
		states[stateIndex].data.setMixByName(fromAnimation, toAnimation, duration);
	}

	public function setAnimation (name:String, loop:Boolean, stateIndex:int = 0) : void {
		if (stateIndex < 0 || stateIndex >= states.length)
			throw new ArgumentError("stateIndex out of range.");
		states[stateIndex].setAnimationByName(name, loop);
	}

	public function addAnimation (name:String, loop:Boolean, delay:Number = 0, stateIndex:int = 0) : void {
		if (stateIndex < 0 || stateIndex >= states.length)
			throw new ArgumentError("stateIndex out of range.");
		states[stateIndex].addAnimationByName(name, loop, delay);
	}

	public function clearAnimation (stateIndex:int = 0) : void {
		if (stateIndex < 0 || stateIndex >= states.length)
			throw new ArgumentError("stateIndex out of range.");
		states[stateIndex].clearAnimation();
	}
}

}
