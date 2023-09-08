package spine.atlas;

interface TextureLoader {
	function loadPage(page:AtlasPage, path:String):Void;

	function loadRegion(region:AtlasRegion):Void;

	function unloadPage(page:AtlasPage):Void;
}
