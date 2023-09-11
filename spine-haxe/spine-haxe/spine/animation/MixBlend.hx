package spine.animation;

class MixBlend {
	public var ordinal:Int = 0;

	public function new(ordinal:Int) {
		this.ordinal = ordinal;
	}

	public static var setup(default, never):MixBlend = new MixBlend(0);
	public static var first(default, never):MixBlend = new MixBlend(1);
	public static var replace(default, never):MixBlend = new MixBlend(2);
	public static var add(default, never):MixBlend = new MixBlend(3);
}
