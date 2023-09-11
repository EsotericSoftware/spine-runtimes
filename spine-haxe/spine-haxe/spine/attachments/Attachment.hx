package spine.attachments;

import openfl.errors.ArgumentError;
import openfl.errors.IllegalOperationError;

class Attachment {
	private var _name:String;

	public function new(name:String) {
		if (name == null) {
			throw new ArgumentError("name cannot be null.");
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
		throw new IllegalOperationError("Not implemented");
	}
}
