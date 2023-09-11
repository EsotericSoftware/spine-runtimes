package spine;

import openfl.Vector;

class TransformConstraintData extends ConstraintData {
	private var _bones:Vector<BoneData> = new Vector<BoneData>();

	public var target:BoneData;
	public var mixRotate:Float = 0;
	public var mixX:Float = 0;
	public var mixY:Float = 0;
	public var mixScaleX:Float = 0;
	public var mixScaleY:Float = 0;
	public var mixShearY:Float = 0;
	public var offsetRotation:Float = 0;
	public var offsetX:Float = 0;
	public var offsetY:Float = 0;
	public var offsetScaleX:Float = 0;
	public var offsetScaleY:Float = 0;
	public var offsetShearY:Float = 0;
	public var relative:Bool = false;
	public var local:Bool = false;

	public function new(name:String) {
		super(name, 0, false);
	}

	public var bones(get, never):Vector<BoneData>;

	private function get_bones():Vector<BoneData> {
		return _bones;
	}
}
