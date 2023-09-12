package spine;

import openfl.Vector;

class SequenceMode {
	public static var hold(default, never):SequenceMode = new SequenceMode("hold", 0);
	public static var once(default, never):SequenceMode = new SequenceMode("once", 1);
	public static var loop(default, never):SequenceMode = new SequenceMode("loop", 2);
	public static var pingpong(default, never):SequenceMode = new SequenceMode("pingpong", 3);
	public static var onceReverse(default, never):SequenceMode = new SequenceMode("onceReverse", 4);
	public static var loopReverse(default, never):SequenceMode = new SequenceMode("loopReverse", 5);
	public static var pingpongReverse(default, never):SequenceMode = new SequenceMode("pingpongReverse", 6);

	public static var values(default, never):Vector<SequenceMode> = Vector.ofArray([hold, once, loop, pingpong, onceReverse, loopReverse, pingpongReverse]);

	public var name(default, null):String;
	public var value:Int;

	public function new(name:String, value:Int) {
		this.name = name;
		this.value = value;
	}

	public static function fromName(name:String):SequenceMode {
		for (value in values) {
			if (value.name == name)
				return value;
		}
		return null;
	}
}
