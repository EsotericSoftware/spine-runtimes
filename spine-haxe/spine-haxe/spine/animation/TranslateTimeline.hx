package spine.animation;

import openfl.Vector;
import spine.Bone;
import spine.Event;
import spine.Skeleton;

class TranslateTimeline extends CurveTimeline2 implements BoneTimeline {
	public var boneIndex:Int = 0;

	public function new(frameCount:Int, bezierCount:Int, boneIndex:Int) {
		super(frameCount, bezierCount, Vector.ofArray([Property.x + "|" + boneIndex, Property.y + "|" + boneIndex]));
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
					bone.x = bone.data.x;
					bone.y = bone.data.y;
				case MixBlend.first:
					bone.x += (bone.data.x - bone.x) * alpha;
					bone.y += (bone.data.y - bone.y) * alpha;
			}
			return;
		}

		var x:Float = 0, y:Float = 0;
		var i:Int = Timeline.search(frames, time, CurveTimeline2.ENTRIES);
		var curveType:Int = Std.int(curves[Std.int(i / CurveTimeline2.ENTRIES)]);

		switch (curveType) {
			case CurveTimeline.LINEAR:
				var before:Float = frames[i];
				x = frames[i + CurveTimeline2.VALUE1];
				y = frames[i + CurveTimeline2.VALUE2];
				var t:Float = (time - before) / (frames[i + CurveTimeline2.ENTRIES] - before);
				x += (frames[i + CurveTimeline2.ENTRIES + CurveTimeline2.VALUE1] - x) * t;
				y += (frames[i + CurveTimeline2.ENTRIES + CurveTimeline2.VALUE2] - y) * t;
			case CurveTimeline.STEPPED:
				x = frames[i + CurveTimeline2.VALUE1];
				y = frames[i + CurveTimeline2.VALUE2];
			default:
				x = getBezierValue(time, i, CurveTimeline2.VALUE1, curveType - CurveTimeline.BEZIER);
				y = getBezierValue(time, i, CurveTimeline2.VALUE2, curveType + CurveTimeline.BEZIER_SIZE - CurveTimeline.BEZIER);
		}

		switch (blend) {
			case MixBlend.setup:
				bone.x = bone.data.x + x * alpha;
				bone.y = bone.data.y + y * alpha;
			case MixBlend.first, MixBlend.replace:
				bone.x += (bone.data.x + x - bone.x) * alpha;
				bone.y += (bone.data.y + y - bone.y) * alpha;
			case MixBlend.add:
				bone.x += x * alpha;
				bone.y += y * alpha;
		}
	}
}
