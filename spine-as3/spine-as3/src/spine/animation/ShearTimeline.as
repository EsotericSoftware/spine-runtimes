package spine.animation {
	import spine.Event;
	import spine.Skeleton;
	import spine.Bone;

public class ShearTimeline extends TranslateTimeline {
	public function ShearTimeline (frameCount:int) {
		super(frameCount);
	}

	override public function apply (skeleton:Skeleton, lastTime:Number, time:Number, firedEvents:Vector.<Event>, alpha:Number) : void {
		var frames:Vector.<Number> = this.frames;
		if (time < frames[0]) return; // Time is before first frame.

		var bone:Bone = skeleton.bones[boneIndex];
		if (time >= frames[frames.length - ENTRIES]) { // Time is after last frame.
			bone.shearX += (bone.data.shearX + frames[frames.length + PREV_X] - bone.shearX) * alpha;
			bone.shearY += (bone.data.shearY + frames[frames.length + PREV_Y] - bone.shearY) * alpha;
			return;
		}

		// Interpolate between the previous frame and the current frame.
		var frame:int = Animation.binarySearch(frames, time, ENTRIES);
		var prevX:Number = frames[frame + PREV_X];
		var prevY:Number = frames[frame + PREV_Y];
		var frameTime:Number = frames[frame];
		var percent:Number = getCurvePercent(frame / ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

		bone.shearX += (bone.data.shearX + (prevX + (frames[frame + X] - prevX) * percent) - bone.shearX) * alpha;
		bone.shearY += (bone.data.shearY + (prevY + (frames[frame + Y] - prevY) * percent) - bone.shearY) * alpha;
	}
}

}