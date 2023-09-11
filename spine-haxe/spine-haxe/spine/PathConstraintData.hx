package spine;

import openfl.Vector;

class PathConstraintData extends ConstraintData {
	private var _bones:Vector<BoneData> = new Vector<BoneData>();

	public var target:SlotData;
	public var positionMode:PositionMode = PositionMode.fixed;
	public var spacingMode:SpacingMode = SpacingMode.fixed;
	public var rotateMode:RotateMode = RotateMode.chain;
	public var offsetRotation:Float = 0;
	public var position:Float = 0;
	public var spacing:Float = 0;
	public var mixRotate:Float = 0;
	public var mixX:Float = 0;
	public var mixY:Float = 0;

	public function new(name:String) {
		super(name, 0, false);
	}

	public var bones(get, never):Vector<BoneData>;

	private function get_bones():Vector<BoneData> {
		return _bones;
	}
}
