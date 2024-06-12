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

import spine.attachments.ClippingAttachment;

class SkeletonClipping {
	private var triangulator:Triangulator = new Triangulator();
	private var clippingPolygon:Array<Float> = new Array<Float>();
	private var clipOutput:Array<Float> = new Array<Float>();

	public var clippedVertices:Array<Float> = new Array<Float>();
	public var clippedUvs:Array<Float> = new Array<Float>();
	public var clippedTriangles:Array<Int> = new Array<Int>();

	private var scratch:Array<Float> = new Array<Float>();

	private var clipAttachment:ClippingAttachment;
	private var clippingPolygons:Array<Array<Float>>;

	public function new() {}

	public function clipStart(slot:Slot, clip:ClippingAttachment):Int {
		if (clipAttachment != null)
			return 0;
		clipAttachment = clip;
		clippingPolygon.resize(clip.worldVerticesLength);
		clip.computeWorldVertices(slot, 0, clippingPolygon.length, clippingPolygon, 0, 2);
		SkeletonClipping.makeClockwise(clippingPolygon);
		clippingPolygons = triangulator.decompose(clippingPolygon, triangulator.triangulate(clippingPolygon));
		for (polygon in clippingPolygons) {
			SkeletonClipping.makeClockwise(polygon);
			polygon.push(polygon[0]);
			polygon.push(polygon[1]);
		}
		return clippingPolygons.length;
	}

	public function clipEndWithSlot(slot:Slot):Void {
		if (clipAttachment != null && clipAttachment.endSlot == slot.data)
			clipEnd();
	}

	public function clipEnd():Void {
		if (clipAttachment == null)
			return;
		clipAttachment = null;
		clippingPolygons = null;
		clippedVertices.resize(0);
		clippedUvs.resize(0);
		clippedTriangles.resize(0);
		clippingPolygon.resize(0);
		clipOutput.resize(0);
	}

	public function isClipping():Bool {
		return clipAttachment != null;
	}

	private function clipTrianglesNoRender(vertices:Array<Float>, triangles:Array<Int>, trianglesLength:Float):Void {
		var polygonsCount:Int = clippingPolygons.length;
		var index:Int = 0;
		clippedVertices.resize(0);
		clippedTriangles.resize(0);
		var i:Int = 0;
		while (i < trianglesLength) {
			var vertexOffset:Int = triangles[i] << 1;
			var x1:Float = vertices[vertexOffset],
				y1:Float = vertices[vertexOffset + 1];

			vertexOffset = triangles[i + 1] << 1;
			var x2:Float = vertices[vertexOffset],
				y2:Float = vertices[vertexOffset + 1];

			vertexOffset = triangles[i + 2] << 1;
			var x3:Float = vertices[vertexOffset],
				y3:Float = vertices[vertexOffset + 1];

			for (p in 0...polygonsCount) {
				var s:Int = clippedVertices.length;
				var clippedVerticesItems:Array<Float>;
				var clippedTrianglesItems:Array<Int>;
				if (this.clip(x1, y1, x2, y2, x3, y3, clippingPolygons[p], clipOutput)) {
					var clipOutputLength:Int = clipOutput.length;
					if (clipOutputLength == 0)
						continue;

					var clipOutputCount:Int = clipOutputLength >> 1;
					var clipOutputItems:Array<Float> = clipOutput;
					clippedVerticesItems = clippedVertices;
					clippedVerticesItems.resize(s + clipOutputLength);
					var ii:Int = 0;
					while (ii < clipOutputLength) {
						var x:Float = clipOutputItems[ii],
							y:Float = clipOutputItems[ii + 1];
						clippedVerticesItems[s] = x;
						clippedVerticesItems[s + 1] = y;
						s += 2;
						ii += 2;
					}

					s = clippedTriangles.length;
					clippedTrianglesItems = clippedTriangles;
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
					clippedVerticesItems = clippedVertices;
					clippedVerticesItems.resize(s + 3 * 2);
					clippedVerticesItems[s] = x1;
					clippedVerticesItems[s + 1] = y1;
					clippedVerticesItems[s + 2] = x2;
					clippedVerticesItems[s + 3] = y2;
					clippedVerticesItems[s + 4] = x3;
					clippedVerticesItems[s + 5] = y3;

					s = clippedTriangles.length;
					clippedTrianglesItems = clippedTriangles;
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
	}

	public function clipTriangles(vertices:Array<Float>, triangles:Array<Int>, trianglesLength:Float, uvs:Array<Float> = null):Void {
		if (uvs == null) {
			clipTrianglesNoRender(vertices, triangles, trianglesLength);
			return;
		}

		var polygonsCount:Int = clippingPolygons.length;
		var index:Int = 0;
		clippedVertices.resize(0);
		clippedUvs.resize(0);
		clippedTriangles.resize(0);
		var i:Int = 0;
		while (i < trianglesLength) {
			var vertexOffset:Int = triangles[i] << 1;
			var x1:Float = vertices[vertexOffset],
				y1:Float = vertices[vertexOffset + 1];
			var u1:Float = uvs[vertexOffset], v1:Float = uvs[vertexOffset + 1];

			vertexOffset = triangles[i + 1] << 1;
			var x2:Float = vertices[vertexOffset],
				y2:Float = vertices[vertexOffset + 1];
			var u2:Float = uvs[vertexOffset], v2:Float = uvs[vertexOffset + 1];

			vertexOffset = triangles[i + 2] << 1;
			var x3:Float = vertices[vertexOffset],
				y3:Float = vertices[vertexOffset + 1];
			var u3:Float = uvs[vertexOffset], v3:Float = uvs[vertexOffset + 1];

			for (p in 0...polygonsCount) {
				var s:Int = clippedVertices.length;
				var clippedVerticesItems:Array<Float>;
				var clippedUvsItems:Array<Float>;
				var clippedTrianglesItems:Array<Int>;
				if (this.clip(x1, y1, x2, y2, x3, y3, clippingPolygons[p], clipOutput)) {
					var clipOutputLength:Int = clipOutput.length;
					if (clipOutputLength == 0)
						continue;
					var d0:Float = y2 - y3,
						d1:Float = x3 - x2,
						d2:Float = x1 - x3,
						d4:Float = y3 - y1;
					var d:Float = 1 / (d0 * d2 + d1 * (y1 - y3));

					var clipOutputCount:Int = clipOutputLength >> 1;
					var clipOutputItems:Array<Float> = clipOutput;
					clippedVerticesItems = clippedVertices;
					clippedVerticesItems.resize(s + clipOutputLength);
					clippedUvsItems = clippedUvs;
					clippedUvsItems.resize(s + clipOutputLength);

					var ii:Int = 0;
					while (ii < clipOutputLength) {
						var x:Float = clipOutputItems[ii],
							y:Float = clipOutputItems[ii + 1];
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

					s = clippedTriangles.length;
					clippedTrianglesItems = clippedTriangles;
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
					clippedVerticesItems = clippedVertices;
					clippedVerticesItems.resize(s + 3 * 2);
					clippedVerticesItems[s] = x1;
					clippedVerticesItems[s + 1] = y1;
					clippedVerticesItems[s + 2] = x2;
					clippedVerticesItems[s + 3] = y2;
					clippedVerticesItems[s + 4] = x3;
					clippedVerticesItems[s + 5] = y3;

					clippedUvsItems = clippedUvs;
					clippedUvsItems.resize(s + 3 * 2);
					clippedUvsItems[s] = u1;
					clippedUvsItems[s + 1] = v1;
					clippedUvsItems[s + 2] = u2;
					clippedUvsItems[s + 3] = v2;
					clippedUvsItems[s + 4] = u3;
					clippedUvsItems[s + 5] = v3;

					s = clippedTriangles.length;
					clippedTrianglesItems = clippedTriangles;
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
	}

	/** Clips the input triangle against the convex, clockwise clipping area. If the triangle lies entirely within the clipping
	 * area, false is returned. The clipping area must duplicate the first vertex at the end of the vertices list. */
	public function clip(x1:Float, y1:Float, x2:Float, y2:Float, x3:Float, y3:Float, clippingArea:Array<Float>, output:Array<Float>):Bool {
		var originalOutput:Array<Float> = output;
		var clipped:Bool = false;

		// Avoid copy at the end.
		var input:Array<Float> = null;
		if (clippingArea.length % 4 >= 2) {
			input = output;
			output = scratch;
		} else {
			input = scratch;
		}

		input.resize(0);
		input.push(x1);
		input.push(y1);
		input.push(x2);
		input.push(y2);
		input.push(x3);
		input.push(y3);
		input.push(x1);
		input.push(y1);
		output.resize(0);

		var clippingVerticesLast:Int = clippingArea.length - 4;
		var clippingVertices:Array<Float> = clippingArea;
		var ix:Float, iy:Float, t:Float;
		var i:Int = 0;
		var n:Int = 0;
		while (true) {
			var edgeX:Float = clippingVertices[i],
				edgeY:Float = clippingVertices[i + 1];
			var ex:Float = edgeX - clippingVertices[i + 2],
				ey:Float = edgeY - clippingVertices[i + 3];

			var outputStart:Int = output.length;
			var inputVertices:Array<Float> = input;
			var ii:Int = 0;
			var nn:Int = input.length - 2;
			while (ii < nn) {
				var inputX:Float = inputVertices[ii],
					inputY:Float = inputVertices[ii + 1];
				ii += 2;
				var inputX2:Float = inputVertices[ii],
					inputY2:Float = inputVertices[ii + 1];
				var s2:Bool = ey * (edgeX - inputX2) > ex * (edgeY - inputY2);
				var s1:Float = ey * (edgeX - inputX) - ex * (edgeY - inputY);
				if (s1 > 0) {
					if (s2) {
						// v1 inside, v2 inside
						output.push(inputX2);
						output.push(inputY2);
						continue;
					}
					// v1 inside, v2 outside
					ix = inputX2 - inputX;
					iy = inputY2 - inputY;
					t = s1 / (ix * ey - iy * ex);
					if (t >= 0 && t <= 1) {
						output.push(inputX + ix * t);
						output.push(inputY + iy * t);
					} else {
						output.push(inputX2);
						output.push(inputY2);
						continue;
					}
				} else if (s2) {
					// v1 outside, v2 inside
					ix = inputX2 - inputX;
					iy = inputY2 - inputY;
					t = s1 / (ix * ey - iy * ex);
					if (t >= 0 && t <= 1) {
						output.push(inputX + ix * t);
						output.push(inputY + iy * t);
						output.push(inputX2);
						output.push(inputY2);
					} else {
						output.push(inputX2);
						output.push(inputY2);
						continue;
					}
				}
				clipped = true;
			}

			if (outputStart == output.length) {
				// All edges outside.
				originalOutput.resize(0);
				return true;
			}

			output.push(output[0]);
			output.push(output[1]);

			if (i == clippingVerticesLast)
				break;
			var temp:Array<Float> = output;
			output = input;
			output.resize(0);
			input = temp;

			i += 2;
		}

		if (originalOutput != output) {
			originalOutput.resize(0);
			n = output.length - 2;
			for (i in 0...n) {
				originalOutput[i] = output[i];
			}
		} else {
			originalOutput.resize(originalOutput.length - 2);
		}

		return clipped;
	}

	public static function makeClockwise(polygon:Array<Float>):Void {
		var vertices:Array<Float> = polygon;
		var verticeslength:Int = polygon.length;

		var area:Float = vertices[verticeslength - 2] * vertices[1] - vertices[0] * vertices[verticeslength - 1];
		var p1x:Float = 0, p1y:Float = 0, p2x:Float = 0, p2y:Float = 0;
		var i:Int = 0;
		var n:Int = verticeslength - 3;
		while (i < n) {
			p1x = vertices[i];
			p1y = vertices[i + 1];
			p2x = vertices[i + 2];
			p2y = vertices[i + 3];
			area += p1x * p2y - p2x * p1y;
			i += 2;
		}
		if (area < 0)
			return;

		i = 0;
		n = verticeslength >> 1;
		var lastX:Int = verticeslength - 2;
		while (i < n) {
			var x:Float = vertices[i], y:Float = vertices[i + 1];
			var other:Int = lastX - i;
			vertices[i] = vertices[other];
			vertices[i + 1] = vertices[other + 1];
			vertices[other] = x;
			vertices[other + 1] = y;
			i += 2;
		}
	}
}
