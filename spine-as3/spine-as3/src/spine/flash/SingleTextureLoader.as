package spine.flash {
import flash.display.Bitmap;
import flash.display.BitmapData;

import spine.atlas.AtlasPage;
import spine.atlas.TextureLoader;

public class SingleTextureLoader implements TextureLoader {
	private var pageBitmapData:BitmapData;

	/** @param object A Bitmap or BitmapData. */
	public function SingleTextureLoader (object:*) {
		if (object is BitmapData)
			pageBitmapData = BitmapData(object);
		else if (object is Bitmap)
			pageBitmapData = Bitmap(object).bitmapData;
		else
			throw new ArgumentError("object must be a Bitmap or BitmapData.");
	}

	public function load (page:AtlasPage, path:String) : void {
		page.rendererObject = pageBitmapData;
		page.width = pageBitmapData.width;
		page.height = pageBitmapData.height;
	}

	public function unload (texture:Object) : void {
		BitmapData(texture).dispose();
	}
}

}
