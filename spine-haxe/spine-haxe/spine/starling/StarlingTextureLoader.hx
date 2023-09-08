package spine.starling;

import openfl.display.Bitmap;
import openfl.display.BitmapData;
import openfl.errors.ArgumentError;
import openfl.utils.Object;
import spine.atlas.AtlasPage;
import spine.atlas.AtlasRegion;
import spine.atlas.TextureLoader;
import starling.display.Image;
import starling.textures.Texture;

class StarlingTextureLoader implements TextureLoader {
	public var bitmapDatasOrTextures:Object = {};
	public var singleBitmapDataOrTexture:Dynamic;

	/** @param bitmaps A Bitmap or BitmapData or Texture for an atlas that has only one page, or for a multi page atlas an object where the
	 * key is the image path and the value is the Bitmap or BitmapData or Texture. */
	public function new(bitmapsOrTextures:Dynamic) {
		if (Std.isOfType(bitmapsOrTextures, BitmapData)) {
			singleBitmapDataOrTexture = cast(bitmapsOrTextures, BitmapData);
			return;
		}
		if (Std.isOfType(bitmapsOrTextures, Bitmap)) {
			singleBitmapDataOrTexture = cast(bitmapsOrTextures, Bitmap).bitmapData;
			return;
		}
		if (Std.isOfType(bitmapsOrTextures, Texture)) {
			singleBitmapDataOrTexture = cast(bitmapsOrTextures, Texture);
			return;
		}

		for (path in Reflect.fields(bitmapsOrTextures)) {
			var object:Dynamic = Reflect.getProperty(bitmapsOrTextures, path);
			var bitmapDataOrTexture:Dynamic;
			if (Std.isOfType(object, BitmapData)) {
				bitmapDataOrTexture = cast(object, BitmapData);
			} else if (Std.isOfType(object, Bitmap)) {
				bitmapDataOrTexture = cast(object, Bitmap).bitmapData;
			} else if (Std.isOfType(object, Texture)) {
				bitmapDataOrTexture = cast(object, Texture);
			} else {
				throw new ArgumentError("Object for path \"" + path + "\" must be a Bitmap, BitmapData or Texture: " + object);
			}
			bitmapDatasOrTextures[path] = bitmapDataOrTexture;
		}
	}

	public function loadPage(page:AtlasPage, path:String):Void {
		var bitmapDataOrTexture:Dynamic = singleBitmapDataOrTexture != null ? singleBitmapDataOrTexture : bitmapDatasOrTextures[path];
		if (bitmapDataOrTexture == null) {
			throw new ArgumentError("BitmapData/Texture not found with name: " + path);
		}
		if (Std.isOfType(bitmapDataOrTexture, BitmapData)) {
			var bitmapData:BitmapData = cast(bitmapDataOrTexture, BitmapData);
			page.rendererObject = Texture.fromBitmapData(bitmapData);
		} else {
			var texture:Texture = cast(bitmapDataOrTexture, Texture);
			page.rendererObject = texture;
		}
	}

	public function loadRegion(region:AtlasRegion):Void {
		var image:Image = new Image(cast(region.page.rendererObject, Texture));
		if (region.degrees == 90) {
			image.setTexCoords(0, region.u, region.v2);
			image.setTexCoords(1, region.u, region.v);
			image.setTexCoords(2, region.u2, region.v2);
			image.setTexCoords(3, region.u2, region.v);
		} else {
			image.setTexCoords(0, region.u, region.v);
			image.setTexCoords(1, region.u2, region.v);
			image.setTexCoords(2, region.u, region.v2);
			image.setTexCoords(3, region.u2, region.v2);
		}
		region.rendererObject = image;
	}

	public function unloadPage(page:AtlasPage):Void {
		cast(page.rendererObject, Texture).dispose();
	}
}
