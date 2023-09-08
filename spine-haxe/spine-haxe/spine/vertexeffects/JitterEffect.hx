package spine.vertexeffects;

import spine.MathUtils;
import spine.Skeleton;
import spine.Vertex;
import spine.VertexEffect;

class JitterEffect implements VertexEffect {
	public var jitterX:Float = 0;
	public var jitterY:Float = 0;

	public function new(jitterX:Float, jitterY:Float) {
		this.jitterX = jitterX;
		this.jitterY = jitterY;
	}

	public function begin(skeleton:Skeleton):Void {}

	public function transform(vertex:Vertex):Void {
		vertex.x += MathUtils.randomTriangular(-jitterX, jitterY);
		vertex.y += MathUtils.randomTriangular(-jitterX, jitterY);
	}

	public function end():Void {}
}
