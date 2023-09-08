package spine;

import openfl.Vector;

class TransformMode {
	public static var normal(default, never):TransformMode = new TransformMode("normal");
	public static var onlyTranslation(default, never):TransformMode = new TransformMode("onlyTranslation");
	public static var noRotationOrReflection(default, never):TransformMode = new TransformMode("noRotationOrReflection");
	public static var noScale(default, never):TransformMode = new TransformMode("noScale");
	public static var noScaleOrReflection(default, never):TransformMode = new TransformMode("noScaleOrReflection");

	public static var values:Vector<TransformMode> = Vector.ofArray([normal, onlyTranslation, noRotationOrReflection, noScale, noScaleOrReflection]);

	public var name(default, null):String;

	public function new(name:String) {
		this.name = name;
	}

	public static function fromName(name:String):TransformMode {
		for (value in values) {
			if (value.name == name)
				return value;
		}
		return null;
	}
}
