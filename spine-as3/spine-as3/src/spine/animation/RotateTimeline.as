package spine.animation {
import spine.Bone;
import spine.Skeleton;

public class RotateTimeline extends CurveTimeline {
	static private const LAST_FRAME_TIME:int = -2;
	static private const FRAME_VALUE:int = 1;

	public var boneIndex:int;
	public var frames:Vector.<Number> = new Vector.<Number>(); // time, value, ...

	public function RotateTimeline (frameCount:int) {
		super(frameCount);
		frames.length = frameCount * 2;
	}

	/** Sets the time and angle of the specified keyframe. */
	public function setFrame (frameIndex:int, time:Number, angle:Number) : void {
		frameIndex *= 2;
		frames[frameIndex] = time;
		frames[frameIndex + 1] = angle;
	}

	override public function apply (skeleton:Skeleton, time:Number, alpha:Number) : void {
		if (time < frames[0])
			return; // Time is before first frame.

		var bone:Bone = skeleton.bones[boneIndex];

		var amount:Number;
		if (time >= frames[frames.length - 2]) { // Time is after last frame.
			amount = bone.data.rotation + frames[frames.length - 1] - bone.rotation;
			while (amount > 180)
				amount -= 360;
			while (amount < -180)
				amount += 360;
			bone.rotation += amount * alpha;
			return;
		}

		// Interpolate between the last frame and the current frame.
		var frameIndex:int = Animation.binarySearch(frames, time, 2);
		var lastFrameValue:Number = frames[frameIndex - 1];
		var frameTime:Number = frames[frameIndex];
		var percent:Number = 1 - (time - frameTime) / (frames[frameIndex + LAST_FRAME_TIME] - frameTime);
		percent = getCurvePercent(frameIndex / 2 - 1, percent < 0 ? 0 : (percent > 1 ? 1 : percent));

		amount = frames[frameIndex + FRAME_VALUE] - lastFrameValue;
		while (amount > 180)
			amount -= 360;
		while (amount < -180)
			amount += 360;
		amount = bone.data.rotation + (lastFrameValue + amount * percent) - bone.rotation;
		while (amount > 180)
			amount -= 360;
		while (amount < -180)
			amount += 360;
		bone.rotation += amount * alpha;
	}
}

}
