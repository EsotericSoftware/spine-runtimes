package spine.flixel;

import flixel.FlxG;
import flixel.graphics.FlxGraphic;
import openfl.display.BlendMode;

class SpineTexture extends FlxGraphic
{
	public static function from(bitmapData: openfl.display.BitmapData): FlxGraphic {
		return FlxG.bitmap.add(bitmapData);
	}

	public static function toFlixelBlending (blend: spine.BlendMode): BlendMode {
		switch (blend) {
			case spine.BlendMode.normal:
				return BlendMode.NORMAL;

			case spine.BlendMode.additive:
				return BlendMode.ADD;

			case spine.BlendMode.multiply:
				return BlendMode.MULTIPLY;

			case spine.BlendMode.screen:
				return BlendMode.SCREEN;
		}
		return BlendMode.NORMAL;
	}

}
