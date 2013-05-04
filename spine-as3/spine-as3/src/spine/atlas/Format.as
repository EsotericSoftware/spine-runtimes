package spine.atlas {

public class Format {
	public static const alpha:Format = new Format(0, "alpha");
	public static const intensity:Format = new Format(1, "intensity");
	public static const luminanceAlpha:Format = new Format(2, "luminanceAlpha");
	public static const rgb565:Format = new Format(3, "rgb565");
	public static const rgba4444:Format = new Format(4, "rgba4444");
	public static const rgb888:Format = new Format(5, "rgb888");
	public static const rgba8888:Format = new Format(6, "rgba8888");

	public var ordinal:int;
	public var name:String;

	public function Format (ordinal:int, name:String) {
		this.ordinal = ordinal;
		this.name = name;
	}
}

}
