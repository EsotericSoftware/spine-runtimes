package spine.animation;

import openfl.Vector;
import spine.Bone;
import spine.Event;
import spine.Skeleton;

class ShearYTimeline extends CurveTimeline1 implements BoneTimeline {
	private var boneIndex:Int = 0;

	public function new(frameCount:Int, bezierCount:Int, boneIndex:Int) {
		super(frameCount, bezierCount, Vector.ofArray([Property.shearY + "|" + boneIndex]));
		this.boneIndex = boneIndex;
	}

	public function getBoneIndex():Int {
		return boneIndex;
	}

	override public function apply(skeleton:Skeleton, lastTime:Float, time:Float, events:Vector<Event>, alpha:Float, blend:MixBlend,
			direction:MixDirection):Void {
		var bone:Bone = skeleton.bones[boneIndex];
		if (!bone.active)
			return;

		if (time < frames[0]) {
			switch (blend) {
				case MixBlend.setup:
					bone.shearY = bone.data.shearY;
				case MixBlend.first:
					bone.shearY += (bone.data.shearY - bone.shearY) * alpha;
			}
			return;
		}

		var y:Float = getCurveValue(time);
		switch (blend) {
			case MixBlend.setup:
				bone.shearY = bone.data.shearY + y * alpha;
			case MixBlend.first, MixBlend.replace:
				bone.shearY += (bone.data.shearY + y - bone.shearY) * alpha;
			case MixBlend.add:
				bone.shearY += y * alpha;
		}
	}
}
