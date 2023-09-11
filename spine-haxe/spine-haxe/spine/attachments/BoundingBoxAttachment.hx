package spine.attachments;

import spine.Color;

class BoundingBoxAttachment extends VertexAttachment {
	public var color:Color = new Color(0, 0, 0, 0);

	public function new(name:String) {
		super(name);
	}

	override public function copy():Attachment {
		var copy:BoundingBoxAttachment = new BoundingBoxAttachment(name);
		copyTo(copy);
		return copy;
	}
}
