package spine.animation;

import openfl.Vector;
import spine.Bone;
import spine.Event;
import spine.Skeleton;

class RotateTimeline extends CurveTimeline1 implements BoneTimeline {
	public var boneIndex:Int = 0;

	public function new(frameCount:Int, bezierCount:Int, boneIndex:Int) {
		super(frameCount, bezierCount, Vector.ofArray([Property.rotate + "|" + boneIndex]));
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
					bone.rotation = bone.data.rotation;
				case MixBlend.first:
					bone.rotation += (bone.data.rotation - bone.rotation) * alpha;
			}
			return;
		}

		var r:Float = getCurveValue(time);
		if (Math.abs(r) == 360)
			r = 0;
		switch (blend) {
			case MixBlend.setup:
				bone.rotation = bone.data.rotation + r * alpha;
			case MixBlend.first, MixBlend.replace:
				r += bone.data.rotation - bone.rotation;
				bone.rotation += r * alpha;
			case MixBlend.add:
				bone.rotation += r * alpha;
		}
	}
}
