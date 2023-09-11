package spine.atlas;

import openfl.Vector;

class TextureWrap {
	public static var mirroredRepeat(default, never):TextureWrap = new TextureWrap(0, "mirroredRepeat");
	public static var clampToEdge(default, never):TextureWrap = new TextureWrap(1, "clampToEdge");
	public static var repeat(default, never):TextureWrap = new TextureWrap(2, "repeat");

	public static var values(default, never):Vector<TextureWrap> = Vector.ofArray([mirroredRepeat, clampToEdge, repeat]);

	public var ordinal(default, null):Int;
	public var name(default, null):String;

	public function new(ordinal:Int, name:String) {
		this.ordinal = ordinal;
		this.name = name;
	}

	public static function fromName(name:String):TextureWrap {
		for (value in values) {
			if (value.name == name)
				return value;
		}
		return null;
	}
}
