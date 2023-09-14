package spine.attachments;

class Attachment {
	private var _name:String;

	public function new(name:String) {
		if (name == null) {
			throw new SpineException("name cannot be null.");
		}
		_name = name;
	}

	public var name(get, never):String;

	private function get_name():String {
		return _name;
	}

	public function toString():String {
		return name;
	}

	public function copy():Attachment {
		throw new SpineException("Not implemented");
	}
}
