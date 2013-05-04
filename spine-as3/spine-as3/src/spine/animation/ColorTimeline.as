package spine.animation {
import spine.Skeleton;
import spine.Slot;

public class ColorTimeline extends CurveTimeline {
	static private const LAST_FRAME_TIME:int = -5;
	static private const FRAME_R:int = 1;
	static private const FRAME_G:int = 2;
	static private const FRAME_B:int = 3;
	static private const FRAME_A:int = 4;

	public var slotIndex:int;
	public var frames:Vector.<Number> = new Vector.<Number>(); // time, r, g, b, a, ...

	public function ColorTimeline (frameCount:int) {
		super(frameCount);
		frames.length = frameCount * 5;
	}

	/** Sets the time and value of the specified keyframe. */
	public function setFrame (frameIndex:int, time:Number, r:Number, g:Number, b:Number, a:Number) : void {
		frameIndex *= 5;
		frames[frameIndex] = time;
		frames[frameIndex + 1] = r;
		frames[frameIndex + 2] = g;
		frames[frameIndex + 3] = b;
		frames[frameIndex + 4] = a;
	}

	override public function apply (skeleton:Skeleton, time:Number, alpha:Number) : void {
		if (time < frames[0])
			return; // Time is before first frame.

		var slot:Slot = skeleton.slots[slotIndex];

		if (time >= frames[frames.length - 5]) { // Time is after last frame.
			var i:int = frames.length - 1;
			slot.r = frames[i - 3];
			slot.g = frames[i - 2];
			slot.b = frames[i - 1];
			slot.a = frames[i];
			return;
		}

		// Interpolate between the last frame and the current frame.
		var frameIndex:int = Animation.binarySearch(frames, time, 5);
		var lastFrameR:Number = frames[frameIndex - 4];
		var lastFrameG:Number = frames[frameIndex - 3];
		var lastFrameB:Number = frames[frameIndex - 2];
		var lastFrameA:Number = frames[frameIndex - 1];
		var frameTime:Number = frames[frameIndex];
		var percent:Number = 1 - (time - frameTime) / (frames[frameIndex + LAST_FRAME_TIME] - frameTime);
		percent = getCurvePercent(frameIndex / 5 - 1, percent < 0 ? 0 : (percent > 1 ? 1 : percent));

		var r:Number = lastFrameR + (frames[frameIndex + FRAME_R] - lastFrameR) * percent;
		var g:Number = lastFrameG + (frames[frameIndex + FRAME_G] - lastFrameG) * percent;
		var b:Number = lastFrameB + (frames[frameIndex + FRAME_B] - lastFrameB) * percent;
		var a:Number = lastFrameA + (frames[frameIndex + FRAME_A] - lastFrameA) * percent;
		if (alpha < 1) {
			slot.r += (r - slot.r) * alpha;
			slot.g += (g - slot.g) * alpha;
			slot.b += (b - slot.b) * alpha;
			slot.a += (a - slot.a) * alpha;
		} else {
			slot.r = r;
			slot.g = g;
			slot.b = b;
			slot.a = a;
		}
	}
}

}
