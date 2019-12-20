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
	public class Triangulator {
		private var convexPolygons : Vector.<Vector.<Number>> = new Vector.<Vector.<Number>>();
		private var convexPolygonsIndices : Vector.<Vector.<int>> = new Vector.<Vector.<int>>();
		private var indicesArray : Vector.<int> = new Vector.<int>();
		private var isConcaveArray : Vector.<Boolean> = new Vector.<Boolean>();
		private var triangles : Vector.<int> = new Vector.<int>();
		private var polygonPool : Pool = new Pool(function() : Vector.<Number> {
			return new Vector.<Number>();
		});
		private var polygonIndicesPool : Pool = new Pool(function() : Vector.<int> {
			return new Vector.<int>();
		});
		
		public function Triangulator () {			
		}
		
		public function triangulate(verticesArray : Vector.<Number>) : Vector.<int> {
			var vertices : Vector.<Number> = verticesArray;
			var vertexCount : int = verticesArray.length >> 1;
			var i : int, n : int;

			var indices : Vector.<int> = this.indicesArray;
			indices.length = 0;
			for (i = 0; i < vertexCount; i++)
				indices[i] = i;

			var isConcaveArray : Vector.<Boolean> = this.isConcaveArray;
			isConcaveArray.length = 0;
			for (i = 0, n = vertexCount; i < n; ++i)
				isConcaveArray[i] = isConcave(i, vertexCount, vertices, indices);

			var triangles : Vector.<int> = this.triangles;
			triangles.length = 0;

			while (vertexCount > 3) {
				// Find ear tip.
				var previous : int = vertexCount - 1, next : int = 1;
				i = 0;
				while (true) {
					outer:
					if (!isConcaveArray[i]) {
						var p1 : int = indices[previous] << 1, p2 : int = indices[i] << 1, p3 : int = indices[next] << 1;
						var p1x : Number = vertices[p1], p1y : Number = vertices[p1 + 1];
						var p2x : Number = vertices[p2], p2y : Number = vertices[p2 + 1];
						var p3x : Number = vertices[p3], p3y : Number = vertices[p3 + 1];
						for (var ii : int = (next + 1) % vertexCount; ii != previous; ii = (ii + 1) % vertexCount) {
							if (!isConcaveArray[ii]) continue;
							var v : int = indices[ii] << 1;
							var vx : int = vertices[v], vy : int = vertices[v + 1];
							if (positiveArea(p3x, p3y, p1x, p1y, vx, vy)) {
								if (positiveArea(p1x, p1y, p2x, p2y, vx, vy)) {
									if (positiveArea(p2x, p2y, p3x, p3y, vx, vy)) break outer;
								}
							}
						}
						break;
					}

					if (next == 0) {
						do {
							if (!isConcaveArray[i]) break;
							i--;
						} while (i > 0);
						break;
					}

					previous = i;
					i = next;
					next = (next + 1) % vertexCount;
				}

				// Cut ear tip.
				triangles.push(indices[(vertexCount + i - 1) % vertexCount]);
				triangles.push(indices[i]);
				triangles.push(indices[(i + 1) % vertexCount]);
				indices.splice(i, 1);
				isConcaveArray.splice(i, 1);
				vertexCount--;

				var previousIndex : int = (vertexCount + i - 1) % vertexCount;
				var nextIndex : int = i == vertexCount ? 0 : i;
				isConcaveArray[previousIndex] = isConcave(previousIndex, vertexCount, vertices, indices);
				isConcaveArray[nextIndex] = isConcave(nextIndex, vertexCount, vertices, indices);
			}

			if (vertexCount == 3) {
				triangles.push(indices[2]);
				triangles.push(indices[0]);
				triangles.push(indices[1]);
			}
			
			return triangles;
		}

		public function decompose(verticesArray : Vector.<Number>, triangles : Vector.<int>) : Vector.<Vector.<Number>> {
			var vertices : Vector.<Number> = verticesArray;
			var convexPolygons : Vector.<Vector.<Number>> = this.convexPolygons;
			var i : int, n : int;
			for (i = 0, n = convexPolygons.length; i < n; i++) {
				this.polygonPool.free(convexPolygons[i]);
			}
			convexPolygons.length = 0;

			var convexPolygonsIndices : Vector.<Vector.<int>> = this.convexPolygonsIndices;
			for (i = 0, n = convexPolygonsIndices.length; i < n; i++) {
				this.polygonIndicesPool.free(convexPolygonsIndices[i]);
			}
			convexPolygonsIndices.length = 0;

			var polygonIndices : Vector.<int> = Vector.<int>(this.polygonIndicesPool.obtain());
			polygonIndices.length = 0;

			var polygon : Vector.<Number> = Vector.<Number>(this.polygonPool.obtain());
			polygon.length = 0;

			// Merge subsequent triangles if they form a triangle fan.
			var fanBaseIndex : int = -1, lastWinding : int = 0;
			var x1 : Number, y1 : Number, x2 : Number, y2 : Number, x3 : Number, y3 : Number;
			var winding1 : int, winding2 : int, o : int;
			for (i = 0, n = triangles.length; i < n; i += 3) {
				var t1 : int = triangles[i] << 1, t2 : int = triangles[i + 1] << 1, t3 : int = triangles[i + 2] << 1;
				x1 = vertices[t1];
				y1 = vertices[t1 + 1];
				x2 = vertices[t2];
				y2 = vertices[t2 + 1];
				x3 = vertices[t3];
				y3 = vertices[t3 + 1];

				// If the base of the last triangle is the same as this triangle, check if they form a convex polygon (triangle fan).
				var merged : Boolean = false;
				if (fanBaseIndex == t1) {
					o = polygon.length - 4;
					winding1 = Triangulator.winding(polygon[o], polygon[o + 1], polygon[o + 2], polygon[o + 3], x3, y3);
					winding2 = Triangulator.winding(x3, y3, polygon[0], polygon[1], polygon[2], polygon[3]);
					if (winding1 == lastWinding && winding2 == lastWinding) {
						polygon.push(x3);
						polygon.push(y3);
						polygonIndices.push(t3);
						merged = true;
					}
				}

				// Otherwise make this triangle the new base.
				if (!merged) {
					if (polygon.length > 0) {
						convexPolygons.push(polygon);
						convexPolygonsIndices.push(polygonIndices);
					} else {
						polygonPool.free(polygon);
						polygonIndicesPool.free(polygonIndices);
					}
					polygon = Vector.<Number>(this.polygonPool.obtain());
					polygon.length = 0;
					polygon.push(x1);
					polygon.push(y1);
					polygon.push(x2);
					polygon.push(y2);
					polygon.push(x3);
					polygon.push(y3);
					polygonIndices = Vector.<int>(this.polygonIndicesPool.obtain());
					polygonIndices.length = 0;
					polygonIndices.push(t1);
					polygonIndices.push(t2);
					polygonIndices.push(t3);
					lastWinding = Triangulator.winding(x1, y1, x2, y2, x3, y3);
					fanBaseIndex = t1;
				}
			}

			if (polygon.length > 0) {
				convexPolygons.push(polygon);
				convexPolygonsIndices.push(polygonIndices);
			}

			// Go through the list of polygons and try to merge the remaining triangles with the found triangle fans.
			for (i = 0, n = convexPolygons.length; i < n; i++) {
				polygonIndices = convexPolygonsIndices[i];
				if (polygonIndices.length == 0) continue;
				var firstIndex : int = polygonIndices[0];
				var lastIndex : int = polygonIndices[polygonIndices.length - 1];

				polygon = convexPolygons[i];
				o = polygon.length - 4;
				var prevPrevX : Number = polygon[o], prevPrevY : Number = polygon[o + 1];
				var prevX : Number = polygon[o + 2], prevY : Number = polygon[o + 3];
				var firstX : Number = polygon[0], firstY : Number = polygon[1];
				var secondX : Number = polygon[2], secondY : Number = polygon[3];
				var currWinding : int = Triangulator.winding(prevPrevX, prevPrevY, prevX, prevY, firstX, firstY);

				for (var ii : int = 0; ii < n; ii++) {
					if (ii == i) continue;
					var otherIndices : Vector.<int>= convexPolygonsIndices[ii];
					if (otherIndices.length != 3) continue;
					var otherFirstIndex : int = otherIndices[0];
					var otherSecondIndex : int = otherIndices[1];
					var otherLastIndex : int = otherIndices[2];

					var otherPoly : Vector.<Number>= convexPolygons[ii];
					x3 = otherPoly[otherPoly.length - 2];
					y3 = otherPoly[otherPoly.length - 1];

					if (otherFirstIndex != firstIndex || otherSecondIndex != lastIndex) continue;
					winding1 = Triangulator.winding(prevPrevX, prevPrevY, prevX, prevY, x3, y3);
					winding2 = Triangulator.winding(x3, y3, firstX, firstY, secondX, secondY);
					if (winding1 == currWinding && winding2 == currWinding) {
						otherPoly.length = 0;
						otherIndices.length = 0;
						polygon.push(x3);
						polygon.push(y3);
						polygonIndices.push(otherLastIndex);
						prevPrevX = prevX;
						prevPrevY = prevY;
						prevX = x3;
						prevY = y3;
						ii = 0;
					}
				}
			}

			// Remove empty polygons that resulted from the merge step above.
			for (i = convexPolygons.length - 1; i >= 0; i--) {
				polygon = convexPolygons[i];
				if (polygon.length == 0) {
					convexPolygons.splice(i, 1);
					this.polygonPool.free(polygon);
					polygonIndices = convexPolygonsIndices[i];
					convexPolygonsIndices.splice(i, 1);
					this.polygonIndicesPool.free(polygonIndices);
				}
			}

			return convexPolygons;
		}

		private static function isConcave(index : Number, vertexCount : Number, vertices : Vector.<Number>, indices : Vector.<int>) : Boolean {
			var previous : int = indices[(vertexCount + index - 1) % vertexCount] << 1;
			var current : int = indices[index] << 1;
			var next : int = indices[(index + 1) % vertexCount] << 1;
			return !positiveArea(vertices[previous], vertices[previous + 1], vertices[current], vertices[current + 1], vertices[next], vertices[next + 1]);
		}

		private static function positiveArea(p1x : Number, p1y : Number, p2x : Number, p2y : Number, p3x : Number, p3y : Number) : Boolean {
			return p1x * (p3y - p2y) + p2x * (p1y - p3y) + p3x * (p2y - p1y) >= 0;
		}

		private static function winding(p1x : Number, p1y : Number, p2x : Number, p2y : Number, p3x : Number, p3y : Number) : int {
			var px : Number = p2x - p1x, py : Number = p2y - p1y;
			return p3x * py - p3y * px + px * p1y - p1x * py >= 0 ? 1 : -1;
		}
	}
}
