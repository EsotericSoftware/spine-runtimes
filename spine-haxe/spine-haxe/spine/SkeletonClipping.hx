package spine;

import openfl.Vector;
import spine.attachments.ClippingAttachment;

class SkeletonClipping {
	private var triangulator:Triangulator = new Triangulator();
	private var clippingPolygon:Vector<Float> = new Vector<Float>();
	private var clipOutput:Vector<Float> = new Vector<Float>();

	public var clippedVertices:Vector<Float> = new Vector<Float>();
	public var clippedUvs:Vector<Float> = new Vector<Float>();
	public var clippedTriangles:Vector<Int> = new Vector<Int>();

	private var scratch:Vector<Float> = new Vector<Float>();

	private var clipAttachment:ClippingAttachment;
	private var clippingPolygons:Vector<Vector<Float>>;

	public function new() {}

	public function clipStart(slot:Slot, clip:ClippingAttachment):Int {
		if (clipAttachment != null)
			return 0;
		clipAttachment = clip;
		clippingPolygon.length = clip.worldVerticesLength;
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
		clippedVertices.length = 0;
		clippedUvs.length = 0;
		clippedTriangles.length = 0;
		clippingPolygon.length = 0;
		clipOutput.length = 0;
	}

	public function isClipping():Bool {
		return clipAttachment != null;
	}

	public function clipTriangles(vertices:Vector<Float>, triangles:Vector<Int>, trianglesLength:Float, uvs:Vector<Float>):Void {
		var polygonsCount:Int = clippingPolygons.length;
		var index:Int = 0;
		clippedVertices.length = 0;
		clippedUvs.length = 0;
		clippedTriangles.length = 0;
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
				var clippedVerticesItems:Vector<Float>;
				var clippedUvsItems:Vector<Float>;
				var clippedTrianglesItems:Vector<Int>;
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
					var clipOutputItems:Vector<Float> = clipOutput;
					clippedVerticesItems = clippedVertices;
					clippedVerticesItems.length = s + clipOutputLength;
					clippedUvsItems = clippedUvs;
					clippedUvsItems.length = s + clipOutputLength;

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
					clippedTrianglesItems.length = s + 3 * (clipOutputCount - 2);
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
					break;
				}
			}

			i += 3;
		}
	}

	/** Clips the input triangle against the convex, clockwise clipping area. If the triangle lies entirely within the clipping
	 * area, false is returned. The clipping area must duplicate the first vertex at the end of the vertices list. */
	public function clip(x1:Float, y1:Float, x2:Float, y2:Float, x3:Float, y3:Float, clippingArea:Vector<Float>, output:Vector<Float>):Bool {
		var originalOutput:Vector<Float> = output;
		var clipped:Bool = false;

		// Avoid copy at the end.
		var input:Vector<Float> = null;
		if (clippingArea.length % 4 >= 2) {
			input = output;
			output = scratch;
		} else {
			input = scratch;
		}

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

		var clippingVertices:Vector<Float> = clippingArea;
		var clippingVerticesLast:Int = clippingArea.length - 4;
		var c0:Float, c2:Float, s:Float, ua:Float;
		var i:Int = 0;
		var n:Int = 0;
		while (true) {
			var edgeX:Float = clippingVertices[i],
				edgeY:Float = clippingVertices[i + 1];
			var edgeX2:Float = clippingVertices[i + 2],
				edgeY2:Float = clippingVertices[i + 3];
			var deltaX:Float = edgeX - edgeX2, deltaY:Float = edgeY - edgeY2;

			var inputVertices:Vector<Float> = input;
			var inputVerticesLength:Int = input.length - 2,
				outputStart:Int = output.length;
			var ii:Int = 0;
			while (ii < inputVerticesLength) {
				var inputX:Float = inputVertices[ii],
					inputY:Float = inputVertices[ii + 1];
				var inputX2:Float = inputVertices[ii + 2],
					inputY2:Float = inputVertices[ii + 3];
				var side2:Bool = deltaX * (inputY2 - edgeY2) - deltaY * (inputX2 - edgeX2) > 0;
				if (deltaX * (inputY - edgeY2) - deltaY * (inputX - edgeX2) > 0) {
					if (side2) {
						// v1 inside, v2 inside
						output.push(inputX2);
						output.push(inputY2);
						ii += 2;
						continue;
					}
					// v1 inside, v2 outside
					c0 = inputY2 - inputY;
					c2 = inputX2 - inputX;
					s = c0 * (edgeX2 - edgeX) - c2 * (edgeY2 - edgeY);
					if (Math.abs(s) > 0.000001) {
						ua = (c2 * (edgeY - inputY) - c0 * (edgeX - inputX)) / s;
						output.push(edgeX + (edgeX2 - edgeX) * ua);
						output.push(edgeY + (edgeY2 - edgeY) * ua);
					} else {
						output.push(edgeX);
						output.push(edgeY);
					}
				} else if (side2) {
					// v1 outside, v2 inside
					c0 = inputY2 - inputY;
					c2 = inputX2 - inputX;
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

				ii += 2;
			}

			if (outputStart == output.length) {
				// All edges outside.
				originalOutput.length = 0;
				return true;
			}

			output.push(output[0]);
			output.push(output[1]);

			if (i == clippingVerticesLast)
				break;
			var temp:Vector<Float> = output;
			output = input;
			output.length = 0;
			input = temp;

			i += 2;
		}

		if (originalOutput != output) {
			originalOutput.length = 0;
			n = output.length - 2;
			for (i in 0...n) {
				originalOutput[i] = output[i];
			}
		} else {
			originalOutput.length = originalOutput.length - 2;
		}

		return clipped;
	}

	public static function makeClockwise(polygon:Vector<Float>):Void {
		var vertices:Vector<Float> = polygon;
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
