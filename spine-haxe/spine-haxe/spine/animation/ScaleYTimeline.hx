package spine.animation;

import openfl.Vector;
import spine.Bone;
import spine.Event;
import spine.MathUtils;
import spine.Skeleton;

class ScaleYTimeline extends CurveTimeline1 implements BoneTimeline {
	private var boneIndex:Int = 0;

	public function new(frameCount:Int, bezierCount:Int, boneIndex:Int) {
		super(frameCount, bezierCount, Vector.ofArray([Property.scaleY + "|" + boneIndex]));
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
					bone.scaleY = bone.data.scaleY;
				case MixBlend.first:
					bone.scaleY += (bone.data.scaleY - bone.scaleY) * alpha;
			}
			return;
		}

		var y:Float = getCurveValue(time) * bone.data.scaleY;
		if (alpha == 1) {
			if (blend == MixBlend.add)
				bone.scaleY += y - bone.data.scaleY;
			else
				bone.scaleY = y;
		} else {
			// Mixing out uses sign of setup or current pose, else use sign of key.
			var by:Float = 0;
			if (direction == MixDirection.mixOut) {
				switch (blend) {
					case MixBlend.setup:
						by = bone.data.scaleY;
						bone.scaleY = by + (Math.abs(y) * MathUtils.signum(by) - by) * alpha;
					case MixBlend.first, MixBlend.replace:
						by = bone.scaleY;
						bone.scaleY = by + (Math.abs(y) * MathUtils.signum(by) - by) * alpha;
					case MixBlend.add:
						bone.scaleY = (y - bone.data.scaleY) * alpha;
				}
			} else {
				switch (blend) {
					case MixBlend.setup:
						by = Math.abs(bone.data.scaleY) * MathUtils.signum(y);
						bone.scaleY = by + (y - by) * alpha;
					case MixBlend.first, MixBlend.replace:
						by = Math.abs(bone.scaleY) * MathUtils.signum(y);
						bone.scaleY = by + (y - by) * alpha;
					case MixBlend.add:
						bone.scaleY += (y - bone.data.scaleY) * alpha;
				}
			}
		}
	}
}
