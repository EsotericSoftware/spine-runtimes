package spine.attachments;

import spine.Color;
import spine.SlotData;

class ClippingAttachment extends VertexAttachment {
	public var endSlot:SlotData;
	public var color:Color = new Color(0.2275, 0.2275, 0.2275, 1);

	public function new(name:String) {
		super(name);
	}

	override public function copy():Attachment {
		var copy:ClippingAttachment = new ClippingAttachment(name);
		copyTo(copy);
		copy.endSlot = endSlot;
		copy.color.setFromColor(color);
		return copy;
	}
}
