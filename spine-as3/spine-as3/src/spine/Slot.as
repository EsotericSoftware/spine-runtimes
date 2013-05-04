package spine {
import spine.attachments.Attachment;

public class Slot {
	internal var _data:SlotData;
	internal var _bone:Bone;
	internal var _skeleton:Skeleton;
	public var r:Number;
	public var g:Number;
	public var b:Number;
	public var a:Number;
	internal var _attachment:Attachment;
	private var _attachmentTime:Number;

	public function Slot (data:SlotData, skeleton:Skeleton, bone:Bone) {
		if (data == null)
			throw new ArgumentError("data cannot be null.");
		if (skeleton == null)
			throw new ArgumentError("skeleton cannot be null.");
		if (bone == null)
			throw new ArgumentError("bone cannot be null.");
		_data = data;
		_skeleton = skeleton;
		_bone = bone;
		setToBindPose();
	}

	public function get data () : SlotData {
		return _data;
	}

	public function get skeleton () : Skeleton {
		return _skeleton;
	}

	public function get bone () : Bone {
		return _bone;
	}

	/** @return May be null. */
	public function get attachment () : Attachment {
		return _attachment;
	}

	/** Sets the attachment and resets {@link #getAttachmentTime()}.
	 * @param attachment May be null. */
	public function set attachment (attachment:Attachment) : void {
		_attachment = attachment;
		_attachmentTime = _skeleton.time;
	}

	public function set attachmentTime (time:Number) : void {
		_attachmentTime = skeleton.time - time;
	}

	/** Returns the time since the attachment was set. */
	public function get attachmentTime () : Number {
		return skeleton.time - _attachmentTime;
	}

	public function setToBindPose () : void {
		var slotIndex:int = skeleton.data.slots.indexOf(data);
		r = _data.r;
		g = _data.g;
		b = _data.b;
		a = _data.a;
		attachment = _data.attachmentName == null ? null : skeleton.getAttachmentForSlotIndex(slotIndex, data.attachmentName);
	}

	public function toString () : String {
		return _data.name;
	}
}

}
