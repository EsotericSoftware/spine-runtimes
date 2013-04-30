package spine {

import starling.display.Image;
import starling.textures.Texture;
import starling.utils.VertexData;

public class SkeletonImage extends Image {
	public function SkeletonImage (texture:Texture) {
		super(texture);
	}

	public function get vertexData () : VertexData {
		return mVertexData;
	}

	override public function get tinted () : Boolean {
		return true;
	}
}

}
