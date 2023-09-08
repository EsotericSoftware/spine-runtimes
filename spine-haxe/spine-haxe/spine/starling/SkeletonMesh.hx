package spine.starling;

import starling.display.Mesh;
import starling.rendering.IndexData;
import starling.rendering.VertexData;
import starling.styles.MeshStyle;
import starling.textures.Texture;

class SkeletonMesh extends Mesh {
	public function new(texture:Texture, vertexData:VertexData = null, indexData:IndexData = null, style:MeshStyle = null) {
		super(vertexData == null ? new VertexData() : vertexData, indexData == null ? new IndexData() : indexData, style);
		this.texture = texture;
	}

	public function getVertexData():VertexData {
		return this.vertexData;
	}

	public function getIndexData():IndexData {
		return this.indexData;
	}
}
