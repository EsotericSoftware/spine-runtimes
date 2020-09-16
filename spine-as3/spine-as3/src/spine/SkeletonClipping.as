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

package spine {
	import spine.attachments.ClippingAttachment;
	
	public class SkeletonClipping {
		private var triangulator : Triangulator = new Triangulator();
		private var clippingPolygon : Vector.<Number> = new Vector.<Number>();
		private var clipOutput : Vector.<Number> = new Vector.<Number>();
		public var clippedVertices : Vector.<Number> = new Vector.<Number>();
		public var clippedUvs : Vector.<Number> = new Vector.<Number>();
		public var clippedTriangles : Vector.<uint> = new Vector.<uint>();		
		private var scratch : Vector.<Number> = new Vector.<Number>();

		private var clipAttachment: ClippingAttachment;
		private var clippingPolygons: Vector.<Vector.<Number>>;

		public function SkeletonClipping () {
		}

		public function clipStart (slot: Slot, clip: ClippingAttachment) : int {
			if (this.clipAttachment != null) return 0;
			this.clipAttachment = clip;

			var i : int, n : int = clip.worldVerticesLength;			
			var vertices : Vector.<Number> = this.clippingPolygon;
			vertices.length = n;
			clip.computeWorldVertices(slot, 0, n, vertices, 0, 2);
			var clippingPolygon : Vector.<Number> = this.clippingPolygon;
			SkeletonClipping.makeClockwise(clippingPolygon);
			var clippingPolygons : Vector.<Vector.<Number>> = this.clippingPolygons = this.triangulator.decompose(clippingPolygon, triangulator.triangulate(clippingPolygon));
			for (i = 0, n = clippingPolygons.length; i < n; i++) {
				var polygon : Vector.<Number> = clippingPolygons[i];
				SkeletonClipping.makeClockwise(polygon);
				polygon.push(polygon[0]);
				polygon.push(polygon[1]);
			}
			
			return clippingPolygons.length;
		}

		public function clipEndWithSlot (slot: Slot) : void {
			if (this.clipAttachment != null && this.clipAttachment.endSlot == slot.data) this.clipEnd();
		}

		public function clipEnd () : void {
			if (this.clipAttachment == null) return;
			this.clipAttachment = null;
			this.clippingPolygons = null;
			this.clippedVertices.length = 0;
			this.clippedTriangles.length = 0;
			this.clippingPolygon.length = 0;
		}

		public function isClipping (): Boolean {
			return this.clipAttachment != null;
		}

		public function clipTriangles (vertices: Vector.<Number>, triangles: Vector.<uint>, trianglesLength: Number, uvs: Vector.<Number>) : void {

			var clipOutput : Vector.<Number> = this.clipOutput, clippedVertices : Vector.<Number> = this.clippedVertices,  clippedUvs : Vector.<Number> = this.clippedUvs;
			var clippedTriangles : Vector.<uint> = this.clippedTriangles;
			var polygons : Vector.<Vector.<Number>> = this.clippingPolygons;
			var polygonsCount : int = this.clippingPolygons.length;

			var index : int = 0;
			clippedVertices.length = 0;
			clippedUvs.length = 0;
			clippedTriangles.length = 0;
			outer:
			for (var i : int = 0; i < trianglesLength; i += 3) {
				var vertexOffset : int = triangles[i] << 1;
				var x1 : Number = vertices[vertexOffset], y1 : Number = vertices[vertexOffset + 1];
				var u1 : Number = uvs[vertexOffset], v1 : Number = uvs[vertexOffset + 1];

				vertexOffset = triangles[i + 1] << 1;
				var x2 : Number = vertices[vertexOffset], y2 : Number = vertices[vertexOffset + 1];
				var u2 : Number = uvs[vertexOffset], v2 : Number = uvs[vertexOffset + 1];

				vertexOffset = triangles[i + 2] << 1;
				var x3 : Number = vertices[vertexOffset], y3 : Number = vertices[vertexOffset + 1];
				var u3 : Number = uvs[vertexOffset], v3 : Number = uvs[vertexOffset + 1];

				for (var p : int = 0; p < polygonsCount; p++) {
					var s : int = clippedVertices.length;
					var clippedVerticesItems : Vector.<Number>;
					var clippedUvsItems : Vector.<Number>;
					var clippedTrianglesItems : Vector.<uint>;
					if (this.clip(x1, y1, x2, y2, x3, y3, polygons[p], clipOutput)) {
						var clipOutputLength : int = clipOutput.length;
						if (clipOutputLength == 0) continue;
						var d0 : Number = y2 - y3, d1 : Number = x3 - x2, d2 : Number = x1 - x3, d4 : Number = y3 - y1;
						var d : Number = 1 / (d0 * d2 + d1 * (y1 - y3));
						
						var clipOutputCount : int = clipOutputLength >> 1;
						var clipOutputItems : Vector.<Number> = this.clipOutput;
						clippedVerticesItems = clippedVertices;
						clippedVerticesItems.length = s + clipOutputLength;
						clippedUvsItems = clippedUvs;
						clippedUvsItems.length = s + clipOutputLength;
						
						var ii : int;
						for (ii = 0; ii < clipOutputLength; ii += 2) {
							var x : Number = clipOutputItems[ii], y : Number = clipOutputItems[ii + 1];
							clippedVerticesItems[s] = x;
							clippedVerticesItems[s + 1] = y;
							var c0 : Number = x - x3, c1 : Number = y - y3;
							var a : Number = (d0 * c0 + d1 * c1) * d;
							var b : Number = (d4 * c0 + d2 * c1) * d;
							var c : Number = 1 - a - b;
							clippedUvsItems[s] = u1 * a + u2 * b + u3 * c;
							clippedUvsItems[s + 1] = v1 * a + v2 * b + v3 * c;
							s += 2;
						}

						s = clippedTriangles.length;
						clippedTrianglesItems = clippedTriangles;
						clippedTrianglesItems.length = s + 3 * (clipOutputCount - 2);
						clipOutputCount--;
						for (ii = 1; ii < clipOutputCount; ii++) {
							clippedTrianglesItems[s] = index;
							clippedTrianglesItems[s + 1] = (index + ii);
							clippedTrianglesItems[s + 2] = (index + ii + 1);
							s += 3;
						}
						index += clipOutputCount + 1;

					} else {
						clippedVerticesItems = clippedVertices;
						clippedVerticesItems.length = s + 3 * 2;
						clippedVerticesItems[s] = x1;
						clippedVerticesItems[s + 1] = y1;
						clippedVerticesItems[s + 2] = x2;
						clippedVerticesItems[s + 3] = y2;
						clippedVerticesItems[s + 4] = x3;
						clippedVerticesItems[s + 5] = y3;
						
						clippedUvsItems = clippedUvs;
						clippedUvsItems.length = s + 3 * 2;
						clippedUvsItems[s] = u1;
						clippedUvsItems[s + 1] = v1;
						clippedUvsItems[s + 2] = u2;
						clippedUvsItems[s + 3] = v2;						
						clippedUvsItems[s + 4] = u3;						
						clippedUvsItems[s + 5] = v3;

						s = clippedTriangles.length;
						clippedTrianglesItems = clippedTriangles;
						clippedTrianglesItems.length = s + 3;
						clippedTrianglesItems[s] = index;
						clippedTrianglesItems[s + 1] = (index + 1);
						clippedTrianglesItems[s + 2] = (index + 2);
						index += 3;
						continue outer;
					}
				}
			}
		}

		/** Clips the input triangle against the convex, clockwise clipping area. If the triangle lies entirely within the clipping
		 * area, false is returned. The clipping area must duplicate the first vertex at the end of the vertices list. */
		public function clip (x1: Number, y1: Number, x2: Number, y2: Number, x3: Number, y3: Number, clippingArea: Vector.<Number>, output: Vector.<Number>) : Boolean {
			var originalOutput : Vector.<Number> = output;
			var clipped : Boolean = false;

			// Avoid copy at the end.
			var input: Vector.<Number> = null;
			if (clippingArea.length % 4 >= 2) {
				input = output;
				output = this.scratch;
			} else
				input = this.scratch;

			input.length = 0;
			input.push(x1);
			input.push(y1);
			input.push(x2);
			input.push(y2);
			input.push(x3);
			input.push(y3);
			input.push(x1);
			input.push(y1);
			output.length = 0;

			var clippingVertices : Vector.<Number> = clippingArea;
			var clippingVerticesLast : int = clippingArea.length - 4;
			var c0 : Number, c2 : Number, s : Number, ua : Number;
			var i : int, n : int;
			for (i = 0;; i += 2) {
				var edgeX : Number = clippingVertices[i], edgeY : Number = clippingVertices[i + 1];
				var edgeX2 : Number = clippingVertices[i + 2], edgeY2 : Number = clippingVertices[i + 3];
				var deltaX : Number = edgeX - edgeX2, deltaY : Number = edgeY - edgeY2;

				var inputVertices : Vector.<Number> = input;
				var inputVerticesLength : int = input.length - 2, outputStart : int = output.length;
				for (var ii : int = 0; ii < inputVerticesLength; ii += 2) {
					var inputX : Number = inputVertices[ii], inputY : Number = inputVertices[ii + 1];
					var inputX2 : Number = inputVertices[ii + 2], inputY2 : Number = inputVertices[ii + 3];
					var side2 : Boolean = deltaX * (inputY2 - edgeY2) - deltaY * (inputX2 - edgeX2) > 0;
					if (deltaX * (inputY - edgeY2) - deltaY * (inputX - edgeX2) > 0) {
						if (side2) { // v1 inside, v2 inside
							output.push(inputX2);
							output.push(inputY2);
							continue;
						}
						// v1 inside, v2 outside
						c0 = inputY2 - inputY; c2 = inputX2 - inputX;
						s = c0 * (edgeX2 - edgeX) - c2 * (edgeY2 - edgeY);
						if (Math.abs(s) > 0.000001) {
							ua = (c2 * (edgeY - inputY) - c0 * (edgeX - inputX)) / s;
							output.push(edgeX + (edgeX2 - edgeX) * ua);
							output.push(edgeY + (edgeY2 - edgeY) * ua);
						} else {
							output.push(edgeX);
							output.push(edgeY);
						}
					} else if (side2) { // v1 outside, v2 inside
						c0 = inputY2 - inputY, c2 = inputX2 - inputX;
						s = c0 * (edgeX2 - edgeX) - c2 * (edgeY2 - edgeY);
						if (Math.abs(s) > 0.000001) {
							ua = (c2 * (edgeY - inputY) - c0 * (edgeX - inputX)) / s;
							output.push(edgeX + (edgeX2 - edgeX) * ua);
							output.push(edgeY + (edgeY2 - edgeY) * ua);
						} else {
							output.push(edgeX);
							output.push(edgeY);
						}
						output.push(inputX2);
						output.push(inputY2);
					}
					clipped = true;
				}

				if (outputStart == output.length) { // All edges outside.
					originalOutput.length = 0;
					return true;
				}

				output.push(output[0]);
				output.push(output[1]);

				if (i == clippingVerticesLast) break;
				var temp : Vector.<Number> = output;
				output = input;
				output.length = 0;
				input = temp;
			}

			if (originalOutput != output) {
				originalOutput.length = 0;
				for (i = 0, n = output.length - 2; i < n; i++)
					originalOutput[i] = output[i];
			} else
				originalOutput.length = originalOutput.length - 2;

			return clipped;
		}

		public static function makeClockwise (polygon: Vector.<Number>) : void {
			var vertices : Vector.<Number> = polygon;
			var verticeslength : int = polygon.length;

			var area : Number = vertices[verticeslength - 2] * vertices[1] - vertices[0] * vertices[verticeslength - 1];
			var p1x : Number = 0, p1y : Number = 0, p2x : Number = 0, p2y : Number = 0;
			var i : int, n : int;
			for (i = 0, n = verticeslength - 3; i < n; i += 2) {
				p1x = vertices[i];
				p1y = vertices[i + 1];
				p2x = vertices[i + 2];
				p2y = vertices[i + 3];
				area += p1x * p2y - p2x * p1y;
			}
			if (area < 0) return;

			var lastX : int;
			for (i = 0, lastX = verticeslength - 2, n = verticeslength >> 1; i < n; i += 2) {
				var x : Number = vertices[i], y : Number = vertices[i + 1];
				var other : int = lastX - i;
				vertices[i] = vertices[other];
				vertices[i + 1] = vertices[other + 1];
				vertices[other] = x;
				vertices[other + 1] = y;
			}
		}
	}
}
