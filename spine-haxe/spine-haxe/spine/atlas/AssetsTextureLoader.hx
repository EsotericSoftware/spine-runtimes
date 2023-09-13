package spine.atlas;

import starling.textures.Texture;
import spine.atlas.TextureAtlasRegion;
import spine.atlas.TextureAtlasPage;
import spine.atlas.TextureLoader;

class AssetsTextureLoader implements TextureLoader {
	private var basePath:String;

	public function new(basePath:String) {
		this.basePath = basePath;
	}

	public function loadPage(page:TextureAtlasPage, path:String) {
		var bitmapData = openfl.utils.Assets.getBitmapData(basePath + "/" + path);
		if (bitmapData == null) {
			throw new SpineException("Could not load atlas page texture " + basePath + "/" + path);
		}
		page.texture = Texture.fromBitmapData(bitmapData);
	}

	public function loadRegion(region:TextureAtlasRegion):Void {
		region.texture = region.page.texture;
	}

	public function unloadPage(page:TextureAtlasPage):Void {
		cast(page.texture, Texture).dispose();
	}
}
