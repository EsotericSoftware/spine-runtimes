package spine.atlas;

import openfl.Vector;

class TextureAtlasRegion extends TextureRegion {
	public var page:TextureAtlasPage;
	public var name:String;
	public var x:Int = 0;
	public var y:Int = 0;
	public var index:Int = 0;
	public var splits:Vector<Int>;
	public var pads:Vector<Int>;
	public var names:Vector<String>;
	public var values:Vector<Vector<Float>>;

	public function new(page:TextureAtlasPage, name:String) {
		super();
		this.page = page;
		this.name = name;
		page.regions.push(this);
	}
}
