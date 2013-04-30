package spine {

public class BoneData {
	internal var _parent:BoneData;
	internal var _name:String;
	public var length:Number;
	public var x:Number;
	public var y:Number;
	public var rotation:Number;
	public var scaleX:Number = 1;
	public var scaleY:Number = 1;

	/** @param parent May be null. */
	public function BoneData (name:String, parent:BoneData) {
		if (name == null)
			throw new ArgumentError("name cannot be null.");
		_name = name;
		_parent = parent;
	}

	/** @return May be null. */
	public function get parent () : BoneData {
		return _parent;
	}

	public function get name () : String {
		return _name;
	}

	public function toString () : String {
		return _name;
	}
}

}
