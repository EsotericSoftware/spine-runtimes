package spine;

import openfl.errors.ArgumentError;
import openfl.Vector;
import spine.attachments.Attachment;
import spine.attachments.VertexAttachment;

class Slot {
	private var _data:SlotData;
	private var _bone:Bone;

	public var color:Color;
	public var darkColor:Color;

	private var _attachment:Attachment;
	private var _attachmentTime:Float = 0;

	public var attachmentState:Int = 0;
	public var deform:Vector<Float> = new Vector<Float>();

	public function new(data:SlotData, bone:Bone) {
		if (data == null)
			throw new ArgumentError("data cannot be null.");
		if (bone == null)
			throw new ArgumentError("bone cannot be null.");
		_data = data;
		_bone = bone;
		this.color = new Color(1, 1, 1, 1);
		this.darkColor = data.darkColor == null ? null : new Color(1, 1, 1, 1);
		setToSetupPose();
	}

	public var data(get, never):SlotData;

	private function get_data():SlotData {
		return _data;
	}

	public var bone(get, never):Bone;

	private function get_bone():Bone {
		return _bone;
	}

	public var skeleton(get, never):Skeleton;

	private function get_skeleton():Skeleton {
		return _bone.skeleton;
	}

	/** @return May be null. */
	public var attachment(get, set):Attachment;

	private function get_attachment():Attachment {
		return _attachment;
	}

	/** Sets the slot's attachment and, if the attachment changed, resets {@link #attachmentTime} and clears the {@link #deform}.
	 * The deform is not cleared if the old attachment has the same {@link VertexAttachment#getDeformAttachment()} as the specified attachment.
	 * @param attachment May be null. */
	public function set_attachment(attachmentNew:Attachment):Attachment {
		if (attachment == attachmentNew)
			return attachmentNew;
		if (!Std.isOfType(attachmentNew, VertexAttachment)
			|| !Std.isOfType(attachment, VertexAttachment)
			|| cast(attachmentNew, VertexAttachment).deformAttachment != cast(attachment, VertexAttachment).deformAttachment) {
			deform = new Vector<Float>();
		}
		_attachment = attachmentNew;
		_attachmentTime = skeleton.time;
		return attachmentNew;
	}

	public var attachmentTime(get, set):Float;

	private function set_attachmentTime(time:Float):Float {
		_attachmentTime = skeleton.time - time;
		return _attachmentTime;
	}

	/** Returns the time since the attachment was set. */
	private function get_attachmentTime():Float {
		return skeleton.time - _attachmentTime;
	}

	public function setToSetupPose():Void {
		color.setFromColor(data.color);
		if (darkColor != null)
			darkColor.setFromColor(data.darkColor);
		if (_data.attachmentName == null) {
			attachment = null;
		} else {
			_attachment = null;
			attachment = skeleton.getAttachmentForSlotIndex(data.index, data.attachmentName);
		}
	}

	public function toString():String {
		return _data.name != null ? _data.name : "Slot?";
	}
}
