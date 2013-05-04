package spine.starling {

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

	public function updateVertices () : void {
		onVertexDataChanged();
	}

	override public function get tinted () : Boolean {
		return true;
	}
}

}
