/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

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
