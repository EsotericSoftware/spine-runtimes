package spine;

class BoneData {
	private var _index:Int;
	private var _name:String;
	private var _parent:BoneData;

	public var length:Float = 0;
	public var x:Float = 0;
	public var y:Float = 0;
	public var rotation:Float = 0;
	public var scaleX:Float = 1;
	public var scaleY:Float = 1;
	public var shearX:Float = 0;
	public var shearY:Float = 0;
	public var transformMode:TransformMode = TransformMode.normal;
	public var skinRequired:Bool = false;
	public var color:Color = new Color(0, 0, 0, 0);

	/** @param parent May be null. */
	public function new(index:Int, name:String, parent:BoneData) {
		if (index < 0)
			throw new SpineException("index must be >= 0");
		if (name == null)
			throw new SpineException("name cannot be null.");
		_index = index;
		_name = name;
		_parent = parent;
	}

	public var index(get, never):Int;

	private function get_index():Int {
		return _index;
	}

	public var name(get, never):String;

	private function get_name():String {
		return _name;
	}

	/** @return May be null. */
	public var parent(get, never):BoneData;

	private function get_parent():BoneData {
		return _parent;
	}

	public function toString():String {
		return _name;
	}
}
