package spine;

import openfl.Vector;

class RotateMode {
	public static var tangent(default, never):RotateMode = new RotateMode("tangent");
	public static var chain(default, never):RotateMode = new RotateMode("chain");
	public static var chainScale(default, never):RotateMode = new RotateMode("chainScale");

	public static var values(default, never):Vector<RotateMode> = Vector.ofArray([tangent, chain, chainScale]);

	public var name(default, null):String;

	public function new(name:String) {
		this.name = name;
	}

	public static function fromName(name:String):RotateMode {
		for (value in values) {
			if (value.name == name)
				return value;
		}
		return null;
	}
}
