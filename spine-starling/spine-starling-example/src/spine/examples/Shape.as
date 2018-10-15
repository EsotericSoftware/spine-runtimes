package spine.examples {
	import starling.animation.IAnimatable;
	import starling.textures.Texture;
	import flash.display.BitmapData;
	import flash.geom.Point;
	import spine.starling.SkeletonMesh;

	import starling.display.DisplayObject;
	import starling.rendering.IndexData;
	import starling.rendering.Painter;
	import starling.utils.Color;

	
	public class Shape extends DisplayObject implements IAnimatable {
		private var r: Number = 1, g: Number = 1, b: Number = 1, a: Number = 1;
		private var mesh: SkeletonMesh;
		private var vertices: Vector.<Number>;
		
		public function Shape() {
			var bitmapData: BitmapData = new BitmapData(16, 16, false, 0xffffffff);
			mesh = new SkeletonMesh(Texture.fromBitmapData(bitmapData));			
			setVertices(new <Number>[0, 0, 100, 0, 100, 100, 0, 100]);
			setColor(1, 0, 0, 1);
		}
		
		public function setVertices(vertices: Vector.<Number>): void {
			this.vertices = vertices;
		}
		
		public function setColor(r: Number, g: Number, b: Number, a: Number): void {
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}
		
		override public function render(painter : Painter) : void {
			var indices: IndexData = mesh.getIndexData();
			var idx: int = 0;
			var x:Number = vertices[0], y:Number = vertices[1];			
			for (var i:int = 2; i < vertices.length - 2; i+=2) {
				var x2:Number = vertices[i], y2:Number = vertices[i+1]; 
				var x3:Number = vertices[i+2], y3:Number = vertices[i+3];
				indices.setIndex(idx, idx);
				indices.setIndex(idx+1, idx+1);
				indices.setIndex(idx+2, idx+2);
				mesh.setVertexPosition(idx, x, y);
				mesh.setTexCoords(idx++, 0, 0);
				mesh.setVertexPosition(idx, x2, y2);
				mesh.setTexCoords(idx++, 0, 0);
				mesh.setVertexPosition(idx, x3, y3);
				mesh.setTexCoords(idx++, 0, 0);					
			}			
			indices.numIndices = idx;
			indices.trim();
			mesh.getVertexData().numVertices = idx;
			
			var rgb: uint = Color.rgb(r * 255, g * 255, b * 255);
			var alpha: uint = a * 255;	
			mesh.getVertexData().colorize("color", 0xffffffff, 0xff);
						
			mesh.setVertexDataChanged();
			mesh.setIndexDataChanged();
					
			painter.batchMesh(mesh);			
		}
		
		public function advanceTime(time : Number) : void {
			this.setRequiresRedraw();	
		}
		
		override public function hitTest(localPoint : Point) : DisplayObject {
			return null;
		}
	}
}