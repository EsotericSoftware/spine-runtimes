package spine.animation;

import openfl.Vector;
import spine.Bone;
import spine.Event;
import spine.Skeleton;

class ShearXTimeline extends CurveTimeline1 implements BoneTimeline {
	private var boneIndex:Int = 0;

	public function new(frameCount:Int, bezierCount:Int, boneIndex:Int) {
		super(frameCount, bezierCount, Vector.ofArray([Property.shearX + "|" + boneIndex]));
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
					bone.shearX = bone.data.shearX;
				case MixBlend.first:
					bone.shearX += (bone.data.shearX - bone.shearX) * alpha;
			}
			return;
		}

		var x:Float = getCurveValue(time);
		switch (blend) {
			case MixBlend.setup:
				bone.shearX = bone.data.shearX + x * alpha;
			case MixBlend.first, MixBlend.replace:
				bone.shearX += (bone.data.shearX + x - bone.shearX) * alpha;
			case MixBlend.add:
				bone.shearX += x * alpha;
		}
	}
}
