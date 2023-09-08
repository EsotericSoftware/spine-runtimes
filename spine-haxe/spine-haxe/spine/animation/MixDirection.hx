package spine.animation;

class MixDirection {
	public var ordinal:Int = 0;

	public function new(ordinal:Int) {
		this.ordinal = ordinal;
	}

	public static var mixIn(default, never):MixDirection = new MixDirection(0);
	public static var mixOut(default, never):MixDirection = new MixDirection(1);
}
