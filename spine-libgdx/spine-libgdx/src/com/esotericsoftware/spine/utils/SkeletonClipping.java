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

package com.esotericsoftware.spine.utils;

import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;
import com.badlogic.gdx.utils.ShortArray;

import com.esotericsoftware.spine.Slot;
import com.esotericsoftware.spine.attachments.ClippingAttachment;

public class SkeletonClipping {
	private final Triangulator triangulator = new Triangulator();
	private final FloatArray clippingPolygon = new FloatArray();
	private final FloatArray clipOutput = new FloatArray(128);
	private final FloatArray clippedVertices = new FloatArray(128);
	private final ShortArray clippedTriangles = new ShortArray(128);
	private final FloatArray scratch = new FloatArray();

	private ClippingAttachment clipAttachment;
	private Array<FloatArray> clippingPolygons;

	public void clipStart (Slot slot, ClippingAttachment clip) {
		if (clipAttachment != null) return;
		int n = clip.getWorldVerticesLength();
		if (n < 6) return;
		clipAttachment = clip;

		float[] vertices = clippingPolygon.setSize(n);
		clip.computeWorldVertices(slot, 0, n, vertices, 0, 2);
		makeClockwise(clippingPolygon);
		ShortArray triangles = triangulator.triangulate(clippingPolygon);
		clippingPolygons = triangulator.decompose(clippingPolygon, triangles);
		for (FloatArray polygon : clippingPolygons) {
			makeClockwise(polygon);
			polygon.add(polygon.items[0]);
			polygon.add(polygon.items[1]);
		}
	}

	public void clipEnd (Slot slot) {
		if (clipAttachment != null && clipAttachment.getEndSlot() == slot.getData()) clipEnd();
	}

	public void clipEnd () {
		if (clipAttachment == null) return;
		clipAttachment = null;
		clippingPolygons = null;
		clippedVertices.clear();
		clippedTriangles.clear();
		clippingPolygon.clear();
	}

	public boolean isClipping () {
		return clipAttachment != null;
	}

	public void clipTriangles (float[] vertices, short[] triangles, int trianglesLength) {
		FloatArray clipOutput = this.clipOutput, clippedVertices = this.clippedVertices;
		ShortArray clippedTriangles = this.clippedTriangles;
		Object[] polygons = clippingPolygons.items;
		int polygonsCount = clippingPolygons.size;

		short index = 0;
		clippedVertices.clear();
		clippedTriangles.clear();
		for (int i = 0; i < trianglesLength; i += 3) {
			int vertexOffset = triangles[i] << 1;
			float x1 = vertices[vertexOffset], y1 = vertices[vertexOffset + 1];

			vertexOffset = triangles[i + 1] << 1;
			float x2 = vertices[vertexOffset], y2 = vertices[vertexOffset + 1];

			vertexOffset = triangles[i + 2] << 1;
			float x3 = vertices[vertexOffset], y3 = vertices[vertexOffset + 1];

			for (int p = 0; p < polygonsCount; p++) {
				int s = clippedVertices.size;
				if (clip(x1, y1, x2, y2, x3, y3, (FloatArray)polygons[p], clipOutput)) {
					int clipOutputLength = clipOutput.size;
					if (clipOutputLength == 0) continue;

					int clipOutputCount = clipOutputLength >> 1;
					float[] clipOutputItems = clipOutput.items;
					float[] clippedVerticesItems = clippedVertices.setSize(s + clipOutputCount * 2);
					for (int ii = 0; ii < clipOutputLength; ii += 2, s += 2) {
						float x = clipOutputItems[ii], y = clipOutputItems[ii + 1];
						clippedVerticesItems[s] = x;
						clippedVerticesItems[s + 1] = y;
					}

					s = clippedTriangles.size;
					short[] clippedTrianglesItems = clippedTriangles.setSize(s + 3 * (clipOutputCount - 2));
					clipOutputCount--;
					for (int ii = 1; ii < clipOutputCount; ii++, s += 3) {
						clippedTrianglesItems[s] = index;
						clippedTrianglesItems[s + 1] = (short)(index + ii);
						clippedTrianglesItems[s + 2] = (short)(index + ii + 1);
					}
					index += clipOutputCount + 1;

				} else {
					float[] clippedVerticesItems = clippedVertices.setSize(s + 3 * 2);
					clippedVerticesItems[s] = x1;
					clippedVerticesItems[s + 1] = y1;

					clippedVerticesItems[s + 2] = x2;
					clippedVerticesItems[s + 3] = y2;

					clippedVerticesItems[s + 4] = x3;
					clippedVerticesItems[s + 5] = y3;

					s = clippedTriangles.size;
					short[] clippedTrianglesItems = clippedTriangles.setSize(s + 3);
					clippedTrianglesItems[s] = index;
					clippedTrianglesItems[s + 1] = (short)(index + 1);
					clippedTrianglesItems[s + 2] = (short)(index + 2);
					index += 3;
					break;
				}
			}
		}
	}

	public void clipTriangles (float[] vertices, short[] triangles, int trianglesLength, float[] uvs, float light, float dark,
		boolean twoColor) {

		FloatArray clipOutput = this.clipOutput, clippedVertices = this.clippedVertices;
		ShortArray clippedTriangles = this.clippedTriangles;
		Object[] polygons = clippingPolygons.items;
		int polygonsCount = clippingPolygons.size;
		int vertexSize = twoColor ? 6 : 5;

		short index = 0;
		clippedVertices.clear();
		clippedTriangles.clear();
		for (int i = 0; i < trianglesLength; i += 3) {
			int vertexOffset = triangles[i] << 1;
			float x1 = vertices[vertexOffset], y1 = vertices[vertexOffset + 1];
			float u1 = uvs[vertexOffset], v1 = uvs[vertexOffset + 1];

			vertexOffset = triangles[i + 1] << 1;
			float x2 = vertices[vertexOffset], y2 = vertices[vertexOffset + 1];
			float u2 = uvs[vertexOffset], v2 = uvs[vertexOffset + 1];

			vertexOffset = triangles[i + 2] << 1;
			float x3 = vertices[vertexOffset], y3 = vertices[vertexOffset + 1];
			float u3 = uvs[vertexOffset], v3 = uvs[vertexOffset + 1];

			for (int p = 0; p < polygonsCount; p++) {
				int s = clippedVertices.size;
				if (clip(x1, y1, x2, y2, x3, y3, (FloatArray)polygons[p], clipOutput)) {
					int clipOutputLength = clipOutput.size;
					if (clipOutputLength == 0) continue;
					float d0 = y2 - y3, d1 = x3 - x2, d2 = x1 - x3, d4 = y3 - y1;
					float d = 1 / (d0 * d2 + d1 * (y1 - y3));

					int clipOutputCount = clipOutputLength >> 1;
					float[] clipOutputItems = clipOutput.items;
					float[] clippedVerticesItems = clippedVertices.setSize(s + clipOutputCount * vertexSize);
					for (int ii = 0; ii < clipOutputLength; ii += 2, s += 2) {
						float x = clipOutputItems[ii], y = clipOutputItems[ii + 1];
						clippedVerticesItems[s] = x;
						clippedVerticesItems[s + 1] = y;
						clippedVerticesItems[s + 2] = light;
						if (twoColor) {
							clippedVerticesItems[s + 3] = dark;
							s += 4;
						} else
							s += 3;
						float c0 = x - x3, c1 = y - y3;
						float a = (d0 * c0 + d1 * c1) * d;
						float b = (d4 * c0 + d2 * c1) * d;
						float c = 1 - a - b;
						clippedVerticesItems[s] = u1 * a + u2 * b + u3 * c;
						clippedVerticesItems[s + 1] = v1 * a + v2 * b + v3 * c;
					}

					s = clippedTriangles.size;
					short[] clippedTrianglesItems = clippedTriangles.setSize(s + 3 * (clipOutputCount - 2));
					clipOutputCount--;
					for (int ii = 1; ii < clipOutputCount; ii++, s += 3) {
						clippedTrianglesItems[s] = index;
						clippedTrianglesItems[s + 1] = (short)(index + ii);
						clippedTrianglesItems[s + 2] = (short)(index + ii + 1);
					}
					index += clipOutputCount + 1;

				} else {
					float[] clippedVerticesItems = clippedVertices.setSize(s + 3 * vertexSize);
					clippedVerticesItems[s] = x1;
					clippedVerticesItems[s + 1] = y1;
					clippedVerticesItems[s + 2] = light;
					if (!twoColor) {
						clippedVerticesItems[s + 3] = u1;
						clippedVerticesItems[s + 4] = v1;

						clippedVerticesItems[s + 5] = x2;
						clippedVerticesItems[s + 6] = y2;
						clippedVerticesItems[s + 7] = light;
						clippedVerticesItems[s + 8] = u2;
						clippedVerticesItems[s + 9] = v2;

						clippedVerticesItems[s + 10] = x3;
						clippedVerticesItems[s + 11] = y3;
						clippedVerticesItems[s + 12] = light;
						clippedVerticesItems[s + 13] = u3;
						clippedVerticesItems[s + 14] = v3;
					} else {
						clippedVerticesItems[s + 3] = dark;
						clippedVerticesItems[s + 4] = u1;
						clippedVerticesItems[s + 5] = v1;

						clippedVerticesItems[s + 6] = x2;
						clippedVerticesItems[s + 7] = y2;
						clippedVerticesItems[s + 8] = light;
						clippedVerticesItems[s + 9] = dark;
						clippedVerticesItems[s + 10] = u2;
						clippedVerticesItems[s + 11] = v2;

						clippedVerticesItems[s + 12] = x3;
						clippedVerticesItems[s + 13] = y3;
						clippedVerticesItems[s + 14] = light;
						clippedVerticesItems[s + 15] = dark;
						clippedVerticesItems[s + 16] = u3;
						clippedVerticesItems[s + 17] = v3;
					}

					s = clippedTriangles.size;
					short[] clippedTrianglesItems = clippedTriangles.setSize(s + 3);
					clippedTrianglesItems[s] = index;
					clippedTrianglesItems[s + 1] = (short)(index + 1);
					clippedTrianglesItems[s + 2] = (short)(index + 2);
					index += 3;
					break;
				}
			}
		}
	}

	/** Clips the input triangle against the convex, clockwise clipping area. If the triangle lies entirely within the clipping
	 * area, false is returned. The clipping area must duplicate the first vertex at the end of the vertices list. */
	boolean clip (float x1, float y1, float x2, float y2, float x3, float y3, FloatArray clippingArea, FloatArray output) {
		FloatArray originalOutput = output;
		boolean clipped = false;

		// Avoid copy at the end.
		FloatArray input = null;
		if (clippingArea.size % 4 >= 2) {
			input = output;
			output = scratch;
		} else
			input = scratch;

		input.clear();
		input.add(x1);
		input.add(y1);
		input.add(x2);
		input.add(y2);
		input.add(x3);
		input.add(y3);
		input.add(x1);
		input.add(y1);
		output.clear();

		int clippingVerticesLast = clippingArea.size - 4;
		float[] clippingVertices = clippingArea.items;
		for (int i = 0;; i += 2) {
			float edgeX = clippingVertices[i], edgeY = clippingVertices[i + 1];
			float ex = edgeX - clippingVertices[i + 2], ey = edgeY - clippingVertices[i + 3];

			int outputStart = output.size;
			float[] inputVertices = input.items;
			for (int ii = 0, nn = input.size - 2; ii < nn;) {
				float inputX = inputVertices[ii], inputY = inputVertices[ii + 1];
				ii += 2;
				float inputX2 = inputVertices[ii], inputY2 = inputVertices[ii + 1];
				boolean s2 = ey * (edgeX - inputX2) > ex * (edgeY - inputY2);
				float s1 = ey * (edgeX - inputX) - ex * (edgeY - inputY);
				if (s1 > 0) {
					if (s2) { // v1 inside, v2 inside
						output.add(inputX2);
						output.add(inputY2);
						continue;
					}
					// v1 inside, v2 outside
					float ix = inputX2 - inputX, iy = inputY2 - inputY, t = s1 / (ix * ey - iy * ex);
					if (t >= 0 && t <= 1) {
						output.add(inputX + ix * t);
						output.add(inputY + iy * t);
					} else {
						output.add(inputX2);
						output.add(inputY2);
						continue;
					}
				} else if (s2) { // v1 outside, v2 inside
					float ix = inputX2 - inputX, iy = inputY2 - inputY, t = s1 / (ix * ey - iy * ex);
					if (t >= 0 && t <= 1) {
						output.add(inputX + ix * t);
						output.add(inputY + iy * t);
						output.add(inputX2);
						output.add(inputY2);
					} else {
						output.add(inputX2);
						output.add(inputY2);
						continue;
					}
				}
				clipped = true;
			}

			if (outputStart == output.size) { // All edges outside.
				originalOutput.clear();
				return true;
			}

			output.add(output.items[0]);
			output.add(output.items[1]);

			if (i == clippingVerticesLast) break;
			FloatArray temp = output;
			output = input;
			output.clear();
			input = temp;
		}

		if (originalOutput != output) {
			originalOutput.clear();
			originalOutput.addAll(output.items, 0, output.size - 2);
		} else
			originalOutput.setSize(originalOutput.size - 2);

		return clipped;
	}

	public FloatArray getClippedVertices () {
		return clippedVertices;
	}

	public ShortArray getClippedTriangles () {
		return clippedTriangles;
	}

	static void makeClockwise (FloatArray polygon) {
		float[] vertices = polygon.items;
		int verticeslength = polygon.size;

		float area = vertices[verticeslength - 2] * vertices[1] - vertices[0] * vertices[verticeslength - 1], p1x, p1y, p2x, p2y;
		for (int i = 0, n = verticeslength - 3; i < n; i += 2) {
			p1x = vertices[i];
			p1y = vertices[i + 1];
			p2x = vertices[i + 2];
			p2y = vertices[i + 3];
			area += p1x * p2y - p2x * p1y;
		}
		if (area < 0) return;

		for (int i = 0, lastX = verticeslength - 2, n = verticeslength >> 1; i < n; i += 2) {
			float x = vertices[i], y = vertices[i + 1];
			int other = lastX - i;
			vertices[i] = vertices[other];
			vertices[i + 1] = vertices[other + 1];
			vertices[other] = x;
			vertices[other + 1] = y;
		}
	}
}
