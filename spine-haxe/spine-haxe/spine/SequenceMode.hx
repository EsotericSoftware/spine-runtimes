package spine;

class SequenceMode {
	public static var hold(default, never):RotateMode = new SequenceMode("hold");
	public static var once(default, never):RotateMode = new SequenceMode("once");
	public static var loop(default, never):RotateMode = new SequenceMode("loop");
	public static var pingpong(default, never):RotateMode = new SequenceMode("pingpong");
	public static var onceReverse(default, never):RotateMode = new SequenceMode("onceReverse");
	public static var loopReverse(default, never):RotateMode = new SequenceMode("loopReverse");
	public static var pingpongReverse(default, never):RotateMode = new SequenceMode("pingpongReverse");

	public static var values(default, never):Vector<SequenceMode> = Vector.ofArray([tangent, chain, chainScale]);

	public var name(default, null):String;

	public function new(name:String) {
		this.name = name;
	}

	public static function fromName(name:String):SequenceMode {
		for (value in values) {
			if (value.name == name)
				return value;
		}
		return null;
	}
}
