package spine;

import openfl.errors.ArgumentError;

class SlotData {
	private var _index:Int;
	private var _name:String;
	private var _boneData:BoneData;

	public var color:Color = new Color(1, 1, 1, 1);
	public var darkColor:Color = null;
	public var attachmentName:String;
	public var blendMode:BlendMode;

	public function new(index:Int, name:String, boneData:BoneData) {
		if (index < 0)
			throw new ArgumentError("index must be >= 0.");
		if (name == null)
			throw new ArgumentError("name cannot be null.");
		if (boneData == null)
			throw new ArgumentError("boneData cannot be null.");
		_index = index;
		_name = name;
		_boneData = boneData;
	}

	public var index(get, never):Int;

	private function get_index():Int {
		return _index;
	}

	public var name(get, never):String;

	private function get_name():String {
		return _name;
	}

	public var boneData(get, never):BoneData;

	private function get_boneData():BoneData {
		return _boneData;
	}

	public function toString():String {
		return _name;
	}
}
