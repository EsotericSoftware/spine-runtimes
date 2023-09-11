package spine.animation;

import openfl.Vector;
import spine.Bone;
import spine.Event;
import spine.Skeleton;

class TranslateXTimeline extends CurveTimeline1 implements BoneTimeline {
	public var boneIndex:Int = 0;

	public function new(frameCount:Int, bezierCount:Int, boneIndex:Int) {
		super(frameCount, bezierCount, Vector.ofArray([Property.x + "|" + boneIndex]));
		this.boneIndex = boneIndex;
	}

	public function getBoneIndex():Int {
		return boneIndex;
	}

	public override function apply(skeleton:Skeleton, lastTime:Float, time:Float, events:Vector<Event>, alpha:Float, blend:MixBlend,
			direction:MixDirection):Void {
		var bone:Bone = skeleton.bones[boneIndex];
		if (!bone.active)
			return;

		if (time < frames[0]) {
			switch (blend) {
				case MixBlend.setup:
					bone.x = bone.data.x;
				case MixBlend.first:
					bone.x += (bone.data.x - bone.x) * alpha;
			}
			return;
		}

		var x:Float = getCurveValue(time);
		switch (blend) {
			case MixBlend.setup:
				bone.x = bone.data.x + x * alpha;
			case MixBlend.first, MixBlend.replace:
				bone.x += (bone.data.x + x - bone.x) * alpha;
			case MixBlend.add:
				bone.x += x * alpha;
		}
	}
}
