package spine.animation;

import openfl.Vector;
import spine.Bone;
import spine.Event;
import spine.Skeleton;

class TranslateYTimeline extends CurveTimeline1 implements BoneTimeline {
	public var boneIndex:Int = 0;

	public function new(frameCount:Int, bezierCount:Int, boneIndex:Int) {
		super(frameCount, bezierCount, Vector.ofArray([Property.y + "|" + boneIndex]));
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
					bone.y = bone.data.y;
				case MixBlend.first:
					bone.y += (bone.data.y - bone.y) * alpha;
			}
			return;
		}

		var y:Float = getCurveValue(time);
		switch (blend) {
			case MixBlend.setup:
				bone.y = bone.data.y + y * alpha;
			case MixBlend.first, MixBlend.replace:
				bone.y += (bone.data.y + y - bone.y) * alpha;
			case MixBlend.add:
				bone.y += y * alpha;
		}
	}
}
