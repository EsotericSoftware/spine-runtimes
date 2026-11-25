/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

package spine;

import spine.attachments.ClippingAttachment;

class SkeletonClipping {
	private var triangulator:Triangulator = null;
	private var clippingPolygon = new Array<Float>();
	private var clippingPolygons = new Array<Array<Float>>();
	private var clipOutput = new Array<Float>();

	public var clippedVertices = new Array<Float>();
	public var clippedUvs = new Array<Float>();
	public var clippedTriangles = new Array<Int>();
	public var inverseVertices = new Array<Float>();

	private var scratch = new Array<Float>();
	private var inverse:Bool = false;

	private var clipAttachment:ClippingAttachment;

	public function new() {}

	public function clipStart(skeleton:Skeleton, slot:Slot, clip:ClippingAttachment):Void {
		if (clipAttachment != null)
			return;
		var n = clip.worldVerticesLength;
		if (n < 6)
			return;
		clipAttachment = clip;
		inverse = clip.inverse;

		clippingPolygon.resize(n);
		clip.computeWorldVertices(skeleton, slot, 0, n, clippingPolygon, 0, 2);
		var convex = makeClockwise(clippingPolygon);

		if (convex || inverse || clip.convex) {
			if (!convex)
				makeConvex(clippingPolygon);
			clippingPolygon.push(clippingPolygon[0]);
			clippingPolygon.push(clippingPolygon[1]);
			clippingPolygons.push(clippingPolygon);
		} else {
			if (triangulator == null)
				triangulator = new Triangulator();
			var decomposed = triangulator.decompose(clippingPolygon, triangulator.triangulate(clippingPolygon));
			for (polygon in decomposed) {
				clippingPolygons.push(polygon);
			}
		}
	}

	public function clipEnd(?slot:Slot):Void {
		if (clipAttachment == null || (slot != null && clipAttachment.endSlot != slot.data))
			return;
		clipAttachment = null;
		clippingPolygons.resize(0);
	}

	public function isClipping():Bool {
		return clipAttachment != null;
	}

	private function clipTrianglesNoRender(vertices:Array<Float>, triangles:Array<Int>, trianglesLength:Float):Bool {
		var clippedVerticesItems = clippedVertices;
		clippedVertices.resize(0);
		var clippedTrianglesItems = clippedTriangles;
		clippedTriangles.resize(0);
		var index:Int = 0;

		if (inverse) {
			var polygon = clippingPolygons[0];
			var i:Int = 0;
			while (i < trianglesLength) {
				var t:Int = triangles[i] << 1;
				var x1:Float = vertices[t], y1:Float = vertices[t + 1];
				t = triangles[i + 1] << 1;
				var x2:Float = vertices[t], y2:Float = vertices[t + 1];
				t = triangles[i + 2] << 1;
				var x3:Float = vertices[t], y3:Float = vertices[t + 1];
				clipInverse(x1, y1, x2, y2, x3, y3, polygon);

				var iv = inverseVertices;
				var offset:Int = 0;
				var nn:Int = inverseVertices.length;
				while (offset < nn) {
					var polygonSize:Int = Std.int(iv[offset]);
					offset++;
					var vertexCount:Int = polygonSize >> 1;
					var s:Int = clippedVerticesItems.length;

					clippedVerticesItems.resize(s + polygonSize);
					var src:Int = offset;
					var dst:Int = s;
					while (src < offset + polygonSize) {
						clippedVerticesItems[dst] = iv[src];
						src++;
						dst++;
					}

					s = clippedTrianglesItems.length;
					clippedTrianglesItems.resize(s + 3 * (vertexCount - 2));
					var ii:Int = 1;
					while (ii < vertexCount - 1) {
						clippedTrianglesItems[s] = index;
						clippedTrianglesItems[s + 1] = index + ii;
						clippedTrianglesItems[s + 2] = index + ii + 1;
						s += 3;
						ii++;
					}
					index += vertexCount;
					offset += polygonSize;
				}

				i += 3;
			}
			return true;
		}

		var polygonsCount:Int = clippingPolygons.length;
		var clipOutputItems:Array<Float> = null;
		var i:Int = 0;
		while (i < trianglesLength) {
			var t:Int = triangles[i] << 1;
			var x1:Float = vertices[t], y1:Float = vertices[t + 1];
			t = triangles[i + 1] << 1;
			var x2:Float = vertices[t], y2:Float = vertices[t + 1];
			t = triangles[i + 2] << 1;
			var x3:Float = vertices[t], y3:Float = vertices[t + 1];

			for (p in 0...polygonsCount) {
				var s:Int = clippedVerticesItems.length;
				if (this.clip(x1, y1, x2, y2, x3, y3, clippingPolygons[p])) {
					clipOutputItems = clipOutput;
					var clipOutputLength:Int = clipOutput.length;
					if (clipOutputLength == 0)
						continue;
					var clipOutputCount:Int = clipOutputLength >> 1;

					clippedVerticesItems.resize(s + clipOutputLength);
					var ii:Int = 0;
					while (ii < clipOutputLength) {
						clippedVerticesItems[s] = clipOutputItems[ii];
						clippedVerticesItems[s + 1] = clipOutputItems[ii + 1];
						s += 2;
						ii += 2;
					}

					s = clippedTrianglesItems.length;
					clippedTrianglesItems.resize(s + 3 * (clipOutputCount - 2));
					clipOutputCount--;
					for (ii in 1...clipOutputCount) {
						clippedTrianglesItems[s] = index;
						clippedTrianglesItems[s + 1] = (index + ii);
						clippedTrianglesItems[s + 2] = (index + ii + 1);
						s += 3;
					}
					index += clipOutputCount + 1;
				} else {
					clippedVerticesItems.resize(s + 3 * 2);
					clippedVerticesItems[s] = x1;
					clippedVerticesItems[s + 1] = y1;
					clippedVerticesItems[s + 2] = x2;
					clippedVerticesItems[s + 3] = y2;
					clippedVerticesItems[s + 4] = x3;
					clippedVerticesItems[s + 5] = y3;

					s = clippedTrianglesItems.length;
					clippedTrianglesItems.resize(s + 3);
					clippedTrianglesItems[s] = index;
					clippedTrianglesItems[s + 1] = (index + 1);
					clippedTrianglesItems[s + 2] = (index + 2);
					index += 3;
					break;
				}
			}

			i += 3;
		}
		return clipOutputItems != null;
	}

	public function clipTriangles(vertices:Array<Float>, triangles:Array<Int>, trianglesLength:Float, uvs:Array<Float> = null):Bool {
		if (uvs == null) {
			return clipTrianglesNoRender(vertices, triangles, trianglesLength);
		}

		var clippedVerticesItems = clippedVertices;
		clippedVertices.resize(0);
		clippedUvs.resize(0);
		var clippedTrianglesItems = clippedTriangles;
		clippedTriangles.resize(0);
		var index:Int = 0;

		if (inverse) {
			var polygon = clippingPolygons[0];
			var i:Int = 0;
			while (i < trianglesLength) {
				var t:Int = triangles[i] << 1;
				var x1:Float = vertices[t], y1:Float = vertices[t + 1];
				var u1:Float = uvs[t], v1:Float = uvs[t + 1];
				t = triangles[i + 1] << 1;
				var x2:Float = vertices[t], y2:Float = vertices[t + 1];
				var u2:Float = uvs[t], v2:Float = uvs[t + 1];
				t = triangles[i + 2] << 1;
				var x3:Float = vertices[t], y3:Float = vertices[t + 1];
				var u3:Float = uvs[t], v3:Float = uvs[t + 1];
				clipInverse(x1, y1, x2, y2, x3, y3, polygon);
				var nn:Int = inverseVertices.length;
				if (nn == 0) {
					i += 3;
					continue;
				}

				var d0:Float = y2 - y3,
					d1:Float = x3 - x2,
					d2:Float = x1 - x3,
					d4:Float = y3 - y1;
				var d:Float = 1 / (d0 * d2 + d1 * (y1 - y3));
				var iv = inverseVertices;
				var offset:Int = 0;
				while (offset < nn) {
					var polygonSize:Int = Std.int(iv[offset]);
					offset++;
					var vertexCount:Int = polygonSize >> 1;

					var s:Int = clippedVerticesItems.length;
					clippedVerticesItems.resize(s + polygonSize);
					var clippedUvsItems = clippedUvs;
					clippedUvsItems.resize(s + polygonSize);
					var ii:Int = 0;
					while (ii < polygonSize) {
						var x:Float = iv[offset + ii],
							y:Float = iv[offset + ii + 1];
						clippedVerticesItems[s] = x;
						clippedVerticesItems[s + 1] = y;
						var c0:Float = x - x3, c1:Float = y - y3;
						var a:Float = (d0 * c0 + d1 * c1) * d;
						var b:Float = (d4 * c0 + d2 * c1) * d;
						var c:Float = 1 - a - b;
						clippedUvsItems[s] = u1 * a + u2 * b + u3 * c;
						clippedUvsItems[s + 1] = v1 * a + v2 * b + v3 * c;
						s += 2;
						ii += 2;
					}

					s = clippedTrianglesItems.length;
					clippedTrianglesItems.resize(s + 3 * (vertexCount - 2));
					ii = 1;
					while (ii < vertexCount - 1) {
						clippedTrianglesItems[s] = index;
						clippedTrianglesItems[s + 1] = index + ii;
						clippedTrianglesItems[s + 2] = index + ii + 1;
						s += 3;
						ii++;
					}
					index += vertexCount;
					offset += polygonSize;
				}

				i += 3;
			}
			return true;
		}

		var polygonsCount:Int = clippingPolygons.length;
		var clipOutputItems:Array<Float> = null;
		var i:Int = 0;
		while (i < trianglesLength) {
			var vertexOffset:Int = triangles[i] << 1;
			var x1 = vertices[vertexOffset], y1 = vertices[vertexOffset + 1];
			var u1 = uvs[vertexOffset], v1 = uvs[vertexOffset + 1];

			vertexOffset = triangles[i + 1] << 1;
			var x2 = vertices[vertexOffset], y2 = vertices[vertexOffset + 1];
			var u2 = uvs[vertexOffset], v2 = uvs[vertexOffset + 1];

			vertexOffset = triangles[i + 2] << 1;
			var x3 = vertices[vertexOffset], y3 = vertices[vertexOffset + 1];
			var u3 = uvs[vertexOffset], v3 = uvs[vertexOffset + 1];

			var d0:Float = 0, d1:Float = 0, d2:Float = 0, d4:Float = 0;
			var d:Float = 0;

			for (p in 0...polygonsCount) {
				var s:Int = clippedVerticesItems.length;
				if (this.clip(x1, y1, x2, y2, x3, y3, clippingPolygons[p])) {
					clipOutputItems = clipOutput;
					var clipOutputLength:Int = clipOutput.length;
					if (clipOutputLength == 0)
						continue;

					var clipOutputCount:Int = clipOutputLength >> 1;
					if (d == 0) {
						d0 = y2 - y3;
						d1 = x3 - x2;
						d2 = x1 - x3;
						d4 = y3 - y1;
						d = 1 / (d0 * d2 - d1 * d4);
					}
					clippedVerticesItems.resize(s + clipOutputLength);
					var clippedUvsItems = clippedUvs;
					clippedUvsItems.resize(s + clipOutputLength);

					var ii:Int = 0;
					while (ii < clipOutputLength) {
						var x = clipOutputItems[ii],
							y = clipOutputItems[ii + 1];
						clippedVerticesItems[s] = x;
						clippedVerticesItems[s + 1] = y;
						var c0 = x - x3, c1 = y - y3;
						var a = (d0 * c0 + d1 * c1) * d;
						var b = (d4 * c0 + d2 * c1) * d;
						var c = 1 - a - b;
						clippedUvsItems[s] = u1 * a + u2 * b + u3 * c;
						clippedUvsItems[s + 1] = v1 * a + v2 * b + v3 * c;
						s += 2;

						ii += 2;
					}

					s = clippedTrianglesItems.length;
					clippedTrianglesItems.resize(s + 3 * (clipOutputCount - 2));
					clipOutputCount--;
					for (ii in 1...clipOutputCount) {
						clippedTrianglesItems[s] = index;
						clippedTrianglesItems[s + 1] = (index + ii);
						clippedTrianglesItems[s + 2] = (index + ii + 1);
						s += 3;
					}
					index += clipOutputCount + 1;
				} else {
					clippedVerticesItems.resize(s + 3 * 2);
					clippedVerticesItems[s] = x1;
					clippedVerticesItems[s + 1] = y1;
					clippedVerticesItems[s + 2] = x2;
					clippedVerticesItems[s + 3] = y2;
					clippedVerticesItems[s + 4] = x3;
					clippedVerticesItems[s + 5] = y3;

					var clippedUvsItems = clippedUvs;
					clippedUvsItems.resize(s + 3 * 2);
					clippedUvsItems[s] = u1;
					clippedUvsItems[s + 1] = v1;
					clippedUvsItems[s + 2] = u2;
					clippedUvsItems[s + 3] = v2;
					clippedUvsItems[s + 4] = u3;
					clippedUvsItems[s + 5] = v3;

					s = clippedTrianglesItems.length;
					clippedTrianglesItems.resize(s + 3);
					clippedTrianglesItems[s] = index;
					clippedTrianglesItems[s + 1] = (index + 1);
					clippedTrianglesItems[s + 2] = (index + 2);
					index += 3;
					break;
				}
			}

			i += 3;
		}
		return clipOutputItems != null;
	}

	/**
	 * Clips the input triangle against the convex, clockwise clipping area. If the triangle lies entirely within the clipping
	 * area, false is returned. The clipping area must duplicate the first vertex at the end of the vertices list.
	 */
	private function clip(x1:Float, y1:Float, x2:Float, y2:Float, x3:Float, y3:Float, polygon:Array<Float>):Bool {
		var originalOutput:Array<Float> = clipOutput;
		var clipped:Bool = false;

		// Avoid copy at the end.
		var input:Array<Float> = null;
		var output:Array<Float> = null;
		if (polygon.length % 4 >= 2) {
			input = clipOutput;
			output = scratch;
		} else {
			input = scratch;
			output = clipOutput;
		}

		input.resize(8);
		input[0] = x1;
		input[1] = y1;
		input[2] = x2;
		input[3] = y2;
		input[4] = x3;
		input[5] = y3;
		input[6] = x1;
		input[7] = y1;
		output.resize(0);

		var last:Int = polygon.length - 4;
		var i:Int = 0;
		while (true) {
			var edgeX:Float = polygon[i], edgeY:Float = polygon[i + 1];
			var ex:Float = edgeX - polygon[i + 2],
				ey:Float = edgeY - polygon[i + 3];

			var outputStart:Int = output.length;
			x1 = input[0];
			y1 = input[1];
			var s1:Float = ey * (edgeX - x1) - ex * (edgeY - y1);
			var ii:Int = 2;
			var nn:Int = input.length - 2;
			while (ii <= nn) {
				x2 = input[ii];
				y2 = input[ii + 1];
				var s2:Float = ey * (edgeX - x2) - ex * (edgeY - y2);
				if (s1 > 0) {
					if (s2 > 0) {
						// v1 in, v2 in
						output.push(x2);
						output.push(y2);
					} else {
						// v1 in, v2 out
						var ix:Float = x2 - x1, iy:Float = y2 - y1;
						var t:Float = s1 / (ix * ey - iy * ex);
						if (t >= 0 && t <= 1) {
							output.push(x1 + ix * t);
							output.push(y1 + iy * t);
							clipped = true;
						} else {
							output.push(x2);
							output.push(y2);
						}
					}
				} else if (s2 > 0) {
					// v1 out, v2 in
					var ix:Float = x2 - x1, iy:Float = y2 - y1;
					var t:Float = s1 / (ix * ey - iy * ex);
					if (t >= 0 && t <= 1) {
						output.push(x1 + ix * t);
						output.push(y1 + iy * t);
						output.push(x2);
						output.push(y2);
						clipped = true;
					} else {
						output.push(x2);
						output.push(y2);
					}
				} else {
					// v1 out, v2 out
					clipped = true;
				}
				x1 = x2;
				y1 = y2;
				s1 = s2;
				ii += 2;
			}

			if (outputStart == output.length) {
				// All outside.
				originalOutput.resize(0);
				return true;
			}

			output.push(output[0]);
			output.push(output[1]);

			if (i == last)
				break;
			var temp:Array<Float> = output;
			output = input;
			output.resize(0);
			input = temp;

			i += 2;
		}

		if (originalOutput != output) {
			originalOutput.resize(0);
			var n:Int = output.length - 2;
			for (i in 0...n) {
				originalOutput[i] = output[i];
			}
		} else {
			originalOutput.resize(originalOutput.length - 2);
		}

		return clipped;
	}

	private function clipInverse(x1:Float, y1:Float, x2:Float, y2:Float, x3:Float, y3:Float, polygon:Array<Float>):Void {
		inverseVertices.resize(0);
		var vLast:Int = polygon.length - 4;

		// Avoid copy at the end.
		var input:Array<Float> = null;
		var output:Array<Float> = null;
		if (polygon.length % 4 >= 2) {
			input = clipOutput;
			output = scratch;
		} else {
			input = scratch;
			output = clipOutput;
		}

		input.resize(8);
		input[0] = x1;
		input[1] = y1;
		input[2] = x2;
		input[3] = y2;
		input[4] = x3;
		input[5] = y3;
		input[6] = x1;
		input[7] = y1;
		output.resize(0);

		var i:Int = 0;
		while (true) {
			var edgeX:Float = polygon[i], edgeY:Float = polygon[i + 1];
			var ex:Float = edgeX - polygon[i + 2],
				ey:Float = edgeY - polygon[i + 3];
			var outputStart:Int = output.length;
			var fragmentStart:Int = inverseVertices.length;
			inverseVertices.push(0);

			x1 = input[0];
			y1 = input[1];
			var s1:Float = ey * (edgeX - x1) - ex * (edgeY - y1);
			var ii:Int = 2;
			var nn:Int = input.length - 2;
			while (ii <= nn) {
				x2 = input[ii];
				y2 = input[ii + 1];
				var s2:Float = ey * (edgeX - x2) - ex * (edgeY - y2);
				if (s1 > 0) {
					if (s2 > 0) {
						// v1 in, v2 in
						output.push(x2);
						output.push(y2);
					} else {
						// v1 in, v2 out
						var ix:Float = x2 - x1, iy:Float = y2 - y1;
						var t:Float = s1 / (ix * ey - iy * ex);
						if (t >= 0 && t <= 1) {
							var cx:Float = x1 + ix * t, cy:Float = y1 + iy * t;
							output.push(cx);
							output.push(cy);
							inverseVertices.push(cx);
							inverseVertices.push(cy);
							inverseVertices.push(x2);
							inverseVertices.push(y2);
						} else {
							output.push(x2);
							output.push(y2);
						}
					}
				} else if (s2 > 0) {
					// v1 out, v2 in
					var ix:Float = x2 - x1, iy:Float = y2 - y1;
					var t:Float = s1 / (ix * ey - iy * ex);
					if (t >= 0 && t <= 1) {
						var cx:Float = x1 + ix * t, cy:Float = y1 + iy * t;
						inverseVertices.push(cx);
						inverseVertices.push(cy);
						output.push(cx);
						output.push(cy);
						output.push(x2);
						output.push(y2);
					} else {
						output.push(x2);
						output.push(y2);
					}
				} else {
					// v1 out, v2 out
					inverseVertices.push(x2);
					inverseVertices.push(y2);
				}
				x1 = x2;
				y1 = y2;
				s1 = s2;
				ii += 2;
			}

			var fragmentSize:Int = inverseVertices.length - fragmentStart - 1;
			if (fragmentSize >= 6)
				inverseVertices[fragmentStart] = fragmentSize;
			else
				inverseVertices.resize(fragmentStart); // Degenerate.

			if (outputStart == output.length)
				break; // All outside.

			output.push(output[0]);
			output.push(output[1]);

			if (i == vLast)
				break;
			var temp:Array<Float> = output;
			output = input;
			output.resize(0);
			input = temp;

			i += 2;
		}
	}

	private function makeClockwise(polygon:Array<Float>):Bool {
		var v:Array<Float> = polygon;
		var n:Int = polygon.length;
		var noCW:Bool = true, noCCW:Bool = true;
		var area:Float = 0,
			prevX:Float = v[n - 2],
			prevY:Float = v[n - 1],
			currX:Float = v[0],
			currY:Float = v[1];
		var i:Int = 2;
		while (i < n) {
			var nextX:Float = v[i], nextY:Float = v[i + 1];
			area += currX * nextY - nextX * currY;
			var cross:Float = (currX - prevX) * (nextY - currY) - (currY - prevY) * (nextX - currX);
			noCCW = noCCW && cross <= 0;
			noCW = noCW && cross >= 0;
			prevX = currX;
			prevY = currY;
			currX = nextX;
			currY = nextY;
			i += 2;
		}
		area += currX * v[1] - v[0] * currY;
		var cross:Float = (currX - prevX) * (v[1] - currY) - (currY - prevY) * (v[0] - currX);
		noCCW = noCCW && cross <= 0;
		noCW = noCW && cross >= 0;
		if (area >= 0) {
			var lastX:Int = n - 2;
			var half:Int = n >> 1;
			i = 0;
			while (i < half) {
				var x:Float = v[i], y:Float = v[i + 1];
				var other:Int = lastX - i;
				v[i] = v[other];
				v[i + 1] = v[other + 1];
				v[other] = x;
				v[other + 1] = y;
				i += 2;
			}
			return noCW;
		}
		return noCCW;
	}

	private function makeConvex(polygon:Array<Float>):Void {
		var n:Int = polygon.length;
		var v:Array<Float> = polygon;
		clipOutput.resize(n);
		var sorted:Array<Float> = clipOutput;
		sorted[0] = v[0];
		sorted[1] = v[1];
		var i:Int = 2;
		while (i < n) {
			var x:Float = v[i], y:Float = v[i + 1];
			var p:Int = i - 2;
			while (p >= 0 && (sorted[p] > x || (sorted[p] == x && sorted[p + 1] > y))) {
				sorted[p + 2] = sorted[p];
				sorted[p + 3] = sorted[p + 1];
				p -= 2;
			}
			sorted[p + 2] = x;
			sorted[p + 3] = y;
			i += 2;
		}
		v[0] = sorted[0];
		v[1] = sorted[1];
		v[2] = sorted[2];
		v[3] = sorted[3];
		var s:Int = 4;
		i = 4;
		while (i < n) {
			var x:Float = sorted[i], y:Float = sorted[i + 1];
			while ((v[s - 2] - v[s - 4]) * (y - v[s - 3]) - (v[s - 1] - v[s - 3]) * (x - v[s - 4]) >= 0) {
				s -= 2;
				if (s == 2)
					break;
			}
			v[s] = x;
			v[s + 1] = y;
			i += 2;
			s += 2;
		}
		v[s] = sorted[n - 4];
		v[s + 1] = sorted[n - 3];
		var t:Int = s;
		s += 2;
		i = n - 6;
		while (i >= 0) {
			var x:Float = sorted[i], y:Float = sorted[i + 1];
			while ((v[s - 2] - v[s - 4]) * (y - v[s - 3]) - (v[s - 1] - v[s - 3]) * (x - v[s - 4]) >= 0) {
				s -= 2;
				if (s == t)
					break;
			}
			v[s] = x;
			v[s + 1] = y;
			i -= 2;
			s += 2;
		}
		polygon.resize(s - 2);
	}
}
