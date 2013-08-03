package spine.flash {
import spine.AnimationState;
import spine.AnimationStateData;
import spine.SkeletonData;

public class SkeletonAnimation extends SkeletonSprite {
	public var states:Vector.<AnimationState> = new Vector.<AnimationState>();

	public function SkeletonAnimation (skeletonData:SkeletonData) {
		super(skeletonData);
		addAnimationState();
	}

	override public function advanceTime (time:Number) : void {
		for each (var state:AnimationState in states) {
			state.update(time);
			state.apply(skeleton);
		}
		skeleton.updateWorldTransform();
		super.advanceTime(time);
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
