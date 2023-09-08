package spine;

import openfl.Vector;

class SpacingMode {
	public static var length(default, never):SpacingMode = new SpacingMode("length");
	public static var fixed(default, never):SpacingMode = new SpacingMode("fixed");
	public static var percent(default, never):SpacingMode = new SpacingMode("percent");
	public static var proportional(default, never):SpacingMode = new SpacingMode("proportional");

	public static var values(default, never):Vector<SpacingMode> = Vector.ofArray([length, fixed, percent, proportional]);

	public var name(default, null):String;

	public function new(name:String) {
		this.name = name;
	}

	public static function fromName(name:String):SpacingMode {
		for (value in values) {
			if (value.name == name)
				return value;
		}
		return null;
	}
}
