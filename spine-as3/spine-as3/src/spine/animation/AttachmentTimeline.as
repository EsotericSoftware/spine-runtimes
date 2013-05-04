package spine.animation {
import spine.Skeleton;

public class AttachmentTimeline implements Timeline {
	public var slotIndex:int;
	private var _frameCount:int;
	public var frames:Vector.<Number> = new Vector.<Number>(); // time, ...
	public var attachmentNames:Vector.<String> = new Vector.<String>();

	public function AttachmentTimeline (frameCount:int) {
		_frameCount = frameCount;
		frames.length = frameCount;
		attachmentNames.length = frameCount;
	}

	public function get frameCount () : int {
		return _frameCount;
	}

	/** Sets the time and value of the specified keyframe. */
	public function setFrame (frameIndex:int, time:Number, attachmentName:String) : void {
		frames[frameIndex] = time;
		attachmentNames[frameIndex] = attachmentName;
	}

	public function apply (skeleton:Skeleton, time:Number, alpha:Number) : void {
		if (time < frames[0])
			return; // Time is before first frame.

		var frameIndex:int;
		if (time >= frames[frames.length - 1]) // Time is after last frame.
			frameIndex = frames.length - 1;
		else
			frameIndex = Animation.binarySearch(frames, time, 1) - 1;

		var attachmentName:String = attachmentNames[frameIndex];
		skeleton.slots[slotIndex].attachment = attachmentName == null ? null : skeleton.getAttachmentForSlotIndex(slotIndex, attachmentName);
	}
}

}
