package spine.atlas {

public class TextureWrap {
	public static const mirroredRepeat:TextureWrap = new TextureWrap(0, "mirroredRepeat");
	public static const clampToEdge:TextureWrap = new TextureWrap(1, "clampToEdge");
	public static const repeat:TextureWrap = new TextureWrap(2, "repeat");

	public var ordinal:int;
	public var name:String;

	public function TextureWrap (ordinal:int, name:String) {
		this.ordinal = ordinal;
		this.name = name;
	}

	static public function fromString (name:String) : TextureWrap {
		switch (name.toLowerCase()) {
		case "mirroredRepeat":
			return mirroredRepeat;
		case "clampToEdge":
			return clampToEdge;
		case "repeat":
			return repeat;
		}
		throw new ArgumentError("Unknown texture wrap: " + name);
	}
}

}
