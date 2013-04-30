package spine.atlas {

public class TextureFilter {
	public static const nearest:TextureFilter = new TextureFilter(0, "nearest");
	public static const linear:TextureFilter = new TextureFilter(1, "linear");
	public static const mipMap:TextureFilter = new TextureFilter(2, "mipMap");
	public static const mipMapNearestNearest:TextureFilter = new TextureFilter(3, "mipMapNearestNearest");
	public static const mipMapLinearNearest:TextureFilter = new TextureFilter(4, "mipMapLinearNearest");
	public static const mipMapNearestLinear:TextureFilter = new TextureFilter(5, "mipMapNearestLinear");
	public static const mipMapLinearLinear:TextureFilter = new TextureFilter(6, "mipMapLinearLinear");

	public var ordinal:int;
	public var name:String;

	public function TextureFilter (ordinal:int, name:String) {
		this.ordinal = ordinal;
		this.name = name;
	}
}

}
