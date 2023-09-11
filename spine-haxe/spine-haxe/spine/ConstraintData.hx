package spine;

class ConstraintData {
	public var name:String;
	public var order:Int = 0;
	public var skinRequired:Bool = false;

	function new(name:String, order:Int, skinRequired:Bool) {
		this.name = name;
		this.order = order;
		this.skinRequired = skinRequired;
	}

	public function toString():String {
		return name;
	}
}
