package spine.atlas;

class TextureAtlasPage {
	public var name:String;
	public var format:Format;
	public var minFilter:TextureFilter = TextureFilter.nearest;
	public var magFilter:TextureFilter = TextureFilter.nearest;
	public var uWrap:TextureWrap = TextureWrap.clampToEdge;
	public var vWrap:TextureWrap = TextureWrap.clampToEdge;
	public var width = 0;
	public var height = 0;
	public var pma = false;
	public var texture:Dynamic;
	public var regions = new Array<TextureAtlasRegion>();

	public function new(name:String) {
		this.name = name;
	}
}
