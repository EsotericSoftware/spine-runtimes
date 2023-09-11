package spine;

import openfl.errors.ArgumentError;

class Event {
	private var _data:EventData;

	public var time:Float = 0;
	public var intValue:Int = 0;
	public var floatValue:Float = 0;
	public var stringValue:String;
	public var volume:Float = 1;
	public var balance:Float = 0;

	public function new(time:Float, data:EventData) {
		if (data == null)
			throw new ArgumentError("data cannot be null.");
		this.time = time;
		_data = data;
	}

	public var data(get, never):EventData;

	private function get_data():EventData {
		return _data;
	}

	public function toString():String {
		return _data.name != null ? _data.name : "Event?";
	}
}
