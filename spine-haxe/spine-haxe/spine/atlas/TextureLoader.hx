package spine.atlas;

interface TextureLoader {
	function loadPage(page:TextureAtlasPage, path:String):Void;

	function loadRegion(region:TextureAtlasRegion):Void;

	function unloadPage(page:TextureAtlasPage):Void;
}
