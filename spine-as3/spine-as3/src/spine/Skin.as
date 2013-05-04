package spine {
import spine.attachments.Attachment;

/** Stores attachments by slot index and attachment name. */
public class Skin {
	internal var _name:String;
	private var attachments:Object = new Object();

	public function Skin (name:String) {
		if (name == null)
			throw new ArgumentError("name cannot be null.");
		_name = name;
	}

	public function addAttachment (slotIndex:int, name:String, attachment:Attachment) : void {
		if (attachment == null)
			throw new ArgumentError("attachment cannot be null.");
		attachments[slotIndex + ":" + name] = attachment;
	}

	/** @return May be null. */
	public function getAttachment (slotIndex:int, name:String) : Attachment {
		return attachments[slotIndex + ":" + name];
	}

	public function get name () : String {
		return _name;
	}

	public function toString () : String {
		return _name;
	}

	/** Attach each attachment in this skin if the corresponding attachment in the old skin is currently attached. */
	public function attachAll (skeleton:Skeleton, oldSkin:Skin) : void {
		for each (var key:String in oldSkin.attachments) {
			var colon:int = key.indexOf(":");
			var slotIndex:int = parseInt(key.substring(0, colon));
			var name:String = key.substring(colon + 1);
			var slot:Slot = skeleton.slots[slotIndex];
			if (slot.attachment.name == name) {
				var attachment:Attachment = getAttachment(slotIndex, name);
				if (attachment != null)
					slot.attachment = attachment;
			}
		}
	}
}

}
