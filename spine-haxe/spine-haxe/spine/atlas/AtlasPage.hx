package spine.atlas;

class AtlasPage {
	public var name:String;
	public var format:Format;
	public var minFilter:TextureFilter = TextureFilter.nearest;
	public var magFilter:TextureFilter = TextureFilter.nearest;
	public var uWrap:TextureWrap = TextureWrap.clampToEdge;
	public var vWrap:TextureWrap = TextureWrap.clampToEdge;
	public var width:Int = 0;
	public var height:Int = 0;
	public var pma:Bool = false;
	public var rendererObject:Dynamic;

	public function new() {}
}
