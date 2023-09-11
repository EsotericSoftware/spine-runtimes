package spine;

import spine.attachments.Attachment;

class SkinEntry {
	public var slotIndex:Int = 0;
	public var name:String;
	public var attachment:Attachment;

	public function new(slotIndex:Int, name:String, attachment:Attachment) {
		this.slotIndex = slotIndex;
		this.name = name;
		this.attachment = attachment;
	}
}
