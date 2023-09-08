package spine.animation;

import openfl.Vector;
import spine.Event;
import spine.Skeleton;
import spine.Slot;

class AttachmentTimeline extends Timeline implements SlotTimeline {
	public var slotIndex:Int = 0;

	/** The attachment name for each key frame. May contain null values to clear the attachment. */
	public var attachmentNames:Vector<String>;

	public function new(frameCount:Int, slotIndex:Int) {
		super(frameCount, Vector.ofArray([Property.attachment + "|" + slotIndex]));
		this.slotIndex = slotIndex;
		attachmentNames = new Vector<String>(frameCount, true);
	}

	public override function getFrameCount():Int {
		return frames.length;
	}

	public function getSlotIndex():Int {
		return slotIndex;
	}

	/** Sets the time in seconds and the attachment name for the specified key frame. */
	public function setFrame(frame:Int, time:Float, attachmentName:String):Void {
		frames[frame] = time;
		attachmentNames[frame] = attachmentName;
	}

	public override function apply(skeleton:Skeleton, lastTime:Float, time:Float, events:Vector<Event>, alpha:Float, blend:MixBlend,
			direction:MixDirection):Void {
		var slot:Slot = skeleton.slots[slotIndex];
		if (!slot.bone.active)
			return;

		if (direction == MixDirection.mixOut) {
			if (blend == MixBlend.setup) {
				setAttachment(skeleton, slot, slot.data.attachmentName);
			}
			return;
		}

		if (time < frames[0]) {
			if (blend == MixBlend.setup || blend == MixBlend.first) {
				setAttachment(skeleton, slot, slot.data.attachmentName);
			}
			return;
		}

		setAttachment(skeleton, slot, attachmentNames[Timeline.search1(frames, time)]);
	}

	private function setAttachment(skeleton:Skeleton, slot:Slot, attachmentName:String):Void {
		slot.attachment = attachmentName == null ? null : skeleton.getAttachmentForSlotIndex(slotIndex, attachmentName);
	}
}
