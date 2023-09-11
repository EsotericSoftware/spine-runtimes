package spine;

import openfl.Vector;

class BlendMode {
	public static var normal(default, never):BlendMode = new BlendMode(0, "normal");
	public static var additive(default, never):BlendMode = new BlendMode(1, "additive");
	public static var multiply(default, never):BlendMode = new BlendMode(2, "multiply");
	public static var screen(default, never):BlendMode = new BlendMode(3, "screen");

	public static var values(default, never):Vector<BlendMode> = Vector.ofArray([normal, additive, multiply, screen]);

	public var ordinal(default, null):Int;
	public var name(default, null):String;

	public function new(ordinal:Int, name:String) {
		this.ordinal = ordinal;
		this.name = name;
	}

	public static function fromName(name:String):BlendMode {
		for (value in values) {
			if (value.name == name)
				return value;
		}
		return null;
	}
}
