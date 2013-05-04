package spine {

public class SlotData {
	internal var _name:String;
	internal var _boneData:BoneData;
	public var r:Number = 1;
	public var g:Number = 1;
	public var b:Number = 1;
	public var a:Number = 1;
	public var attachmentName:String;

	public function SlotData (name:String, boneData:BoneData) {
		if (name == null)
			throw new ArgumentError("name cannot be null.");
		if (boneData == null)
			throw new ArgumentError("boneData cannot be null.");
		_name = name;
		_boneData = boneData;
	}

	public function get name () : String {
		return _name;
	}

	public function get boneData () : BoneData {
		return _boneData;
	}

	public function toString () : String {
		return _name;
	}
}

}
