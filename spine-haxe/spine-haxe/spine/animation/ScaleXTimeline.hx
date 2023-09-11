package spine.animation;

import openfl.Vector;
import spine.Bone;
import spine.Event;
import spine.MathUtils;
import spine.Skeleton;

class ScaleXTimeline extends CurveTimeline1 implements BoneTimeline {
	private var boneIndex:Int = 0;

	public function new(frameCount:Int, bezierCount:Int, boneIndex:Int) {
		super(frameCount, bezierCount, Vector.ofArray([Property.scaleX + "|" + boneIndex]));
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
					bone.scaleX = bone.data.scaleX;
				case MixBlend.first:
					bone.scaleX += (bone.data.scaleX - bone.scaleX) * alpha;
			}
			return;
		}

		var x:Float = getCurveValue(time) * bone.data.scaleX;
		if (alpha == 1) {
			if (blend == MixBlend.add)
				bone.scaleX += x - bone.data.scaleX;
			else
				bone.scaleX = x;
		} else {
			// Mixing out uses sign of setup or current pose, else use sign of key.
			var bx:Float = 0;
			if (direction == MixDirection.mixOut) {
				switch (blend) {
					case MixBlend.setup:
						bx = bone.data.scaleX;
						bone.scaleX = bx + (Math.abs(x) * MathUtils.signum(bx) - bx) * alpha;
					case MixBlend.first, MixBlend.replace:
						bx = bone.scaleX;
						bone.scaleX = bx + (Math.abs(x) * MathUtils.signum(bx) - bx) * alpha;
					case MixBlend.add:
						bone.scaleX = (x - bone.data.scaleX) * alpha;
				}
			} else {
				switch (blend) {
					case MixBlend.setup:
						bx = Math.abs(bone.data.scaleX) * MathUtils.signum(x);
						bone.scaleX = bx + (x - bx) * alpha;
					case MixBlend.first, MixBlend.replace:
						bx = Math.abs(bone.scaleX) * MathUtils.signum(x);
						bone.scaleX = bx + (x - bx) * alpha;
					case MixBlend.add:
						bone.scaleX += (x - bone.data.scaleX) * alpha;
				}
			}
		}
	}
}
