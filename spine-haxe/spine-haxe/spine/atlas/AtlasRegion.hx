package spine.atlas;

import openfl.Vector;

class AtlasRegion {
	public var page:AtlasPage;
	public var name:String;
	public var x:Int = 0;
	public var y:Int = 0;
	public var width:Int = 0;
	public var height:Int = 0;
	public var u:Float = 0;
	public var v:Float = 0;
	public var u2:Float = 0;
	public var v2:Float = 0;
	public var offsetX:Float = 0;
	public var offsetY:Float = 0;
	public var originalWidth:Int = 0;
	public var originalHeight:Int = 0;
	public var index:Int = 0;
	public var degrees:Int = 0;
	public var splits:Vector<Int>;
	public var pads:Vector<Int>;
	public var rendererObject:Dynamic;
	public var names:Vector<String>;
	public var values:Vector<Vector<Float>>;

	public function new() {}
}
