/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*****************************************************************************/

package spine;

class Triangulator {
	private var convexPolygons:Array<Array<Float>> = new Array<Array<Float>>();
	private var convexPolygonsIndices:Array<Array<Int>> = new Array<Array<Int>>();
	private var indicesArray:Array<Int> = new Array<Int>();
	private var isConcaveArray:Array<Bool> = new Array<Bool>();
	private var triangles:Array<Int> = new Array<Int>();
	private var polygonPool:Pool<Array<Float>> = new Pool(function():Dynamic {
		return new Array<Float>();
	});
	private var polygonIndicesPool:Pool<Array<Int>> = new Pool(function():Dynamic {
		return new Array<Int>();
	});

	public function new() {}

	public function triangulate(vertices:Array<Float>):Array<Int> {
		var vertexCount:Int = vertices.length >> 1;

		indicesArray.resize(0);
		for (i in 0...vertexCount) {
			indicesArray.push(i);
		}

		isConcaveArray.resize(0);
		for (i in 0...vertexCount) {
			isConcaveArray.push(isConcave(i, vertexCount, vertices, indicesArray));
		}

		triangles.resize(0);

		while (vertexCount > 3) {
			// Find ear tip.
			var previous:Int = vertexCount - 1, next:Int = 1;
			var i:Int = 0;
			while (true) {
				if (!isConcaveArray[i]) {
					var p1:Int = indicesArray[previous] << 1,
						p2:Int = indicesArray[i] << 1,
						p3:Int = indicesArray[next] << 1;
					var p1x:Float = vertices[p1], p1y:Float = vertices[p1 + 1];
					var p2x:Float = vertices[p2], p2y:Float = vertices[p2 + 1];
					var p3x:Float = vertices[p3], p3y:Float = vertices[p3 + 1];
					var ii:Int = (next + 1) % vertexCount;
					while (ii != previous) {
						if (!isConcaveArray[ii]) {
							ii = (ii + 1) % vertexCount;
							continue;
						}
						var v:Int = indicesArray[ii] << 1;
						var vx:Float = vertices[v];
						var vy:Float = vertices[v + 1];
						if (positiveArea(p3x, p3y, p1x, p1y, vx, vy)) {
							if (positiveArea(p1x, p1y, p2x, p2y, vx, vy)) {
								if (positiveArea(p2x, p2y, p3x, p3y, vx, vy)) {
									break;
								}
							}
						}
						ii = (ii + 1) % vertexCount;
					}
					break;
				}

				if (next == 0) {
					do {
						if (!isConcaveArray[i])
							break;
						i--;
					} while (i > 0);
					break;
				}

				previous = i;
				i = next;
				next = (next + 1) % vertexCount;
			}

			// Cut ear tip.
			triangles.push(indicesArray[(vertexCount + i - 1) % vertexCount]);
			triangles.push(indicesArray[i]);
			triangles.push(indicesArray[(i + 1) % vertexCount]);
			indicesArray.splice(i, 1);
			isConcaveArray.splice(i, 1);
			vertexCount--;

			var previousIndex:Int = (vertexCount + i - 1) % vertexCount;
			var nextIndex:Int = i == vertexCount ? 0 : i;
			isConcaveArray[previousIndex] = isConcave(previousIndex, vertexCount, vertices, indicesArray);
			isConcaveArray[nextIndex] = isConcave(nextIndex, vertexCount, vertices, indicesArray);
		}

		if (vertexCount == 3) {
			triangles.push(indicesArray[2]);
			triangles.push(indicesArray[0]);
			triangles.push(indicesArray[1]);
		}

		return triangles;
	}

	public function decompose(vertices:Array<Float>, triangles:Array<Int>):Array<Array<Float>> {
		for (i in 0...convexPolygons.length) {
			this.polygonPool.free(convexPolygons[i]);
		}
		convexPolygons.resize(0);

		for (i in 0...convexPolygonsIndices.length) {
			this.polygonIndicesPool.free(convexPolygonsIndices[i]);
		}
		convexPolygonsIndices.resize(0);

		var polygonIndices:Array<Int> = polygonIndicesPool.obtain();
		polygonIndices.resize(0);

		var polygon:Array<Float> = polygonPool.obtain();
		polygon.resize(0);

		// Merge subsequent triangles if they form a triangle fan.
		var fanBaseIndex:Int = -1, lastWinding:Int = 0;
		var x1:Float, y1:Float, x2:Float, y2:Float, x3:Float, y3:Float;
		var winding1:Int, winding2:Int, o:Int;
		var i:Int = 0;
		while (i < triangles.length) {
			var t1:Int = triangles[i] << 1,
				t2:Int = triangles[i + 1] << 1,
				t3:Int = triangles[i + 2] << 1;
			x1 = vertices[t1];
			y1 = vertices[t1 + 1];
			x2 = vertices[t2];
			y2 = vertices[t2 + 1];
			x3 = vertices[t3];
			y3 = vertices[t3 + 1];

			// If the base of the last triangle is the same as this triangle, check if they form a convex polygon (triangle fan).
			var merged:Bool = false;
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
				polygon = polygonPool.obtain();
				polygon.resize(0);
				polygon.push(x1);
				polygon.push(y1);
				polygon.push(x2);
				polygon.push(y2);
				polygon.push(x3);
				polygon.push(y3);
				polygonIndices = polygonIndicesPool.obtain();
				polygonIndices.resize(0);
				polygonIndices.push(t1);
				polygonIndices.push(t2);
				polygonIndices.push(t3);
				lastWinding = Triangulator.winding(x1, y1, x2, y2, x3, y3);
				fanBaseIndex = t1;
			}

			i += 3;
		}

		if (polygon.length > 0) {
			convexPolygons.push(polygon);
			convexPolygonsIndices.push(polygonIndices);
		}

		// Go through the list of polygons and try to merge the remaining triangles with the found triangle fans.
		i = 0;
		var n:Int = convexPolygons.length;
		while (i < n) {
			polygonIndices = convexPolygonsIndices[i];
			if (polygonIndices.length == 0) {
				i++;
				continue;
			}
			var firstIndex:Int = polygonIndices[0];
			var lastIndex:Int = polygonIndices[polygonIndices.length - 1];

			polygon = convexPolygons[i];
			o = polygon.length - 4;
			var prevPrevX:Float = polygon[o], prevPrevY:Float = polygon[o + 1];
			var prevX:Float = polygon[o + 2], prevY:Float = polygon[o + 3];
			var firstX:Float = polygon[0], firstY:Float = polygon[1];
			var secondX:Float = polygon[2], secondY:Float = polygon[3];
			var currWinding:Int = Triangulator.winding(prevPrevX, prevPrevY, prevX, prevY, firstX, firstY);

			var ii:Int = 0;
			while (ii < n) {
				if (ii == i) {
					ii++;
					continue;
				}
				var otherIndices:Array<Int> = convexPolygonsIndices[ii];
				if (otherIndices.length != 3) {
					ii++;
					continue;
				}
				var otherFirstIndex:Int = otherIndices[0];
				var otherSecondIndex:Int = otherIndices[1];
				var otherLastIndex:Int = otherIndices[2];

				var otherPoly:Array<Float> = convexPolygons[ii];
				x3 = otherPoly[otherPoly.length - 2];
				y3 = otherPoly[otherPoly.length - 1];

				if (otherFirstIndex != firstIndex || otherSecondIndex != lastIndex) {
					ii++;
					continue;
				}
				winding1 = Triangulator.winding(prevPrevX, prevPrevY, prevX, prevY, x3, y3);
				winding2 = Triangulator.winding(x3, y3, firstX, firstY, secondX, secondY);
				if (winding1 == currWinding && winding2 == currWinding) {
					otherPoly.resize(0);
					otherIndices.resize(0);
					polygon.push(x3);
					polygon.push(y3);
					polygonIndices.push(otherLastIndex);
					prevPrevX = prevX;
					prevPrevY = prevY;
					prevX = x3;
					prevY = y3;
					ii = 0;
				}

				ii++;
			}

			i++;
		}

		// Remove empty polygons that resulted from the merge step above.
		i = convexPolygons.length - 1;
		while (i >= 0) {
			polygon = convexPolygons[i];
			if (polygon.length == 0) {
				convexPolygons.splice(i, 1);
				this.polygonPool.free(polygon);
				polygonIndices = convexPolygonsIndices[i];
				convexPolygonsIndices.splice(i, 1);
				this.polygonIndicesPool.free(polygonIndices);
			}

			i--;
		}

		return convexPolygons;
	}

	private static function isConcave(index:Int, vertexCount:Int, vertices:Array<Float>, indices:Array<Int>):Bool {
		var previous:Int = indices[(vertexCount + index - 1) % vertexCount] << 1;
		var current:Int = indices[index] << 1;
		var next:Int = indices[(index + 1) % vertexCount] << 1;
		return !positiveArea(vertices[previous], vertices[previous + 1], vertices[current], vertices[current + 1], vertices[next], vertices[next + 1]);
	}

	private static function positiveArea(p1x:Float, p1y:Float, p2x:Float, p2y:Float, p3x:Float, p3y:Float):Bool {
		return p1x * (p3y - p2y) + p2x * (p1y - p3y) + p3x * (p2y - p1y) >= 0;
	}

	private static function winding(p1x:Float, p1y:Float, p2x:Float, p2y:Float, p3x:Float, p3y:Float):Int {
		var px:Float = p2x - p1x, py:Float = p2y - p1y;
		return p3x * py - p3y * px + px * p1y - p1x * py >= 0 ? 1 : -1;
	}
}
