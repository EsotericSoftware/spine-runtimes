package spine;

import openfl.Vector;

class PositionMode {
	public static var fixed(default, never):PositionMode = new PositionMode("fixed");
	public static var percent(default, never):PositionMode = new PositionMode("percent");

	public static var values(default, never):Vector<PositionMode> = Vector.ofArray([fixed, percent]);

	public var name(default, null):String;

	public function new(name:String) {
		this.name = name;
	}

	public static function fromName(name:String):PositionMode {
		for (value in values) {
			if (value.name == name)
				return value;
		}
		return null;
	}
}
