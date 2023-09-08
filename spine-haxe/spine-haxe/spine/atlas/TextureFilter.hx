package spine.atlas;

import openfl.Vector;

class TextureFilter {
	public static var nearest(default, never):TextureFilter = new TextureFilter(0, "nearest");
	public static var linear(default, never):TextureFilter = new TextureFilter(1, "linear");
	public static var mipMap(default, never):TextureFilter = new TextureFilter(2, "mipMap");
	public static var mipMapNearestNearest(default, never):TextureFilter = new TextureFilter(3, "mipMapNearestNearest");
	public static var mipMapLinearNearest(default, never):TextureFilter = new TextureFilter(4, "mipMapLinearNearest");
	public static var mipMapNearestLinear(default, never):TextureFilter = new TextureFilter(5, "mipMapNearestLinear");
	public static var mipMapLinearLinear(default, never):TextureFilter = new TextureFilter(6, "mipMapLinearLinear");

	public static var values(default, never):Vector<TextureFilter> = Vector.ofArray([
		nearest,
		linear,
		mipMap,
		mipMapNearestNearest,
		mipMapLinearNearest,
		mipMapNearestLinear,
		mipMapLinearLinear
	]);

	public var ordinal(default, null):Int;
	public var name(default, null):String;

	public function new(ordinal:Int, name:String) {
		this.ordinal = ordinal;
		this.name = name;
	}

	public static function fromName(name:String):TextureFilter {
		for (value in values) {
			if (value.name == name.toLowerCase())
				return value;
		}
		return null;
	}
}
