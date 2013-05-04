package spine.atlas {

public interface TextureLoader {
	function load (page:AtlasPage, path:String) : void;
	function unload (texture:Object) : void;
}

}
