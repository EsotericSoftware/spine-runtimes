package spine;

import openfl.Vector;

class IkConstraintData extends ConstraintData {
	public var bones:Vector<BoneData> = new Vector<BoneData>();
	public var target:BoneData;
	public var mix:Float = 1;
	public var bendDirection:Int = 1;
	public var compress:Bool = false;
	public var stretch:Bool = false;
	public var uniform:Bool = false;
	public var softness:Float = 0;

	public function new(name:String) {
		super(name, 0, false);
	}
}
