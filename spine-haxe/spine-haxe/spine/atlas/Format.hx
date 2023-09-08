package spine.atlas;

import openfl.Vector;

class Format {
	public static var alpha(default, never):Format = new Format(0, "alpha");
	public static var intensity(default, never):Format = new Format(1, "intensity");
	public static var luminanceAlpha(default, never):Format = new Format(2, "luminanceAlpha");
	public static var rgb565(default, never):Format = new Format(3, "rgb565");
	public static var rgba4444(default, never):Format = new Format(4, "rgba4444");
	public static var rgb888(default, never):Format = new Format(5, "rgb888");
	public static var rgba8888(default, never):Format = new Format(6, "rgba8888");

	public static var values(default, never):Vector<Format> = Vector.ofArray([alpha, intensity, luminanceAlpha, rgb565, rgba4444, rgb888, rgba8888]);

	public var ordinal(default, null):Int;
	public var name(default, null):String;

	public function new(ordinal:Int, name:String) {
		this.ordinal = ordinal;
		this.name = name;
	}

	public static function fromName(name:String):Format {
		for (value in values) {
			if (value.name == name)
				return value;
		}
		return null;
	}
}
