package spine;

interface VertexEffect {
	function begin(skeleton:Skeleton):Void;

	function transform(vertex:Vertex):Void;

	function end():Void;
}
