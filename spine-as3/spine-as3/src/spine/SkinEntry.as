package spine {
	import spine.attachments.Attachment;
	
	public class SkinEntry {
		public var slotIndex : int;
		public var name : String;
		public var attachment : Attachment;
		
		public function SkinEntry(slotIndex : int, name: String, attachment: Attachment) {
			this.slotIndex = slotIndex;
			this.name = name;
			this.attachment = attachment;
		}
	}
}