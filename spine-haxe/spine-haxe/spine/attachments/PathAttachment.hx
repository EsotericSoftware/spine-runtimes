package spine.attachments;

import openfl.Vector;
import spine.Color;

class PathAttachment extends VertexAttachment {
	public var lengths:Vector<Float>;
	public var closed:Bool = false;
	public var constantSpeed:Bool = false;
	public var color:Color = new Color(0, 0, 0, 0);

	public function new(name:String) {
		super(name);
	}

	override public function copy():Attachment {
		var copy:PathAttachment = new PathAttachment(name);
		copyTo(copy);
		copy.lengths = lengths.concat();
		copy.closed = closed;
		copy.constantSpeed = constantSpeed;
		return copy;
	}
}
