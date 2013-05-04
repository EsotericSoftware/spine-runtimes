package spine.animation {
import spine.Bone;
import spine.Skeleton;

public class ScaleTimeline extends TranslateTimeline {
	public function ScaleTimeline (frameCount:int) {
		super(frameCount);
	}

	override public function apply (skeleton:Skeleton, time:Number, alpha:Number) : void {
		if (time < frames[0])
			return; // Time is before first frame.

		var bone:Bone = skeleton.bones[boneIndex];
		if (time >= frames[frames.length - 3]) { // Time is after last frame.
			bone.scaleX += (bone.data.scaleX - 1 + frames[frames.length - 2] - bone.scaleX) * alpha;
			bone.scaleY += (bone.data.scaleY - 1 + frames[frames.length - 1] - bone.scaleY) * alpha;
			return;
		}

		// Interpolate between the last frame and the current frame.
		var frameIndex:int = Animation.binarySearch(frames, time, 3);
		var lastFrameX:Number = frames[frameIndex - 2];
		var lastFrameY:Number = frames[frameIndex - 1];
		var frameTime:Number = frames[frameIndex];
		var percent:Number = 1 - (time - frameTime) / (frames[frameIndex + LAST_FRAME_TIME] - frameTime);
		percent = getCurvePercent(frameIndex / 3 - 1, percent < 0 ? 0 : (percent > 1 ? 1 : percent));

		bone.scaleX += (bone.data.scaleX - 1 + lastFrameX + (frames[frameIndex + FRAME_X] - lastFrameX) * percent - bone.scaleX) * alpha;
		bone.scaleY += (bone.data.scaleY - 1 + lastFrameY + (frames[frameIndex + FRAME_Y] - lastFrameY) * percent - bone.scaleY) * alpha;
	}
}

}
