package spine;

class EventData {
	private var _name:String;

	public var intValue:Int = 0;
	public var floatValue:Float = 0;
	public var stringValue:String;
	public var audioPath:String;
	public var volume:Float = 1;
	public var balance:Float = 0;

	public function new(name:String) {
		if (name == null)
			throw new SpineException("name cannot be null.");
		_name = name;
	}

	public var name(get, never):String;

	private function get_name():String {
		return _name;
	}

	public function toString():String {
		return _name;
	}
}
