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

package com.esotericsoftware.spine.utils;

import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;
import com.badlogic.gdx.utils.ShortArray;

import com.esotericsoftware.spine.Skeleton;
import com.esotericsoftware.spine.Slot;
import com.esotericsoftware.spine.attachments.ClippingAttachment;

public class SkeletonClipping {
	private Triangulator triangulator;
	private final FloatArray clippingPolygon = new FloatArray(0);
	private final Array<FloatArray> clippingPolygons = new Array(true, 1, FloatArray[]::new);
	private final FloatArray clipOutput = new FloatArray(0);
	private final FloatArray clippedVertices = new FloatArray(0);
	private final FloatArray clippedUvs = new FloatArray(0);
	private final ShortArray clippedTriangles = new ShortArray(0);
	private final FloatArray inverseVertices = new FloatArray(0);
	private final FloatArray scratch = new FloatArray(0);

	private ClippingAttachment clipAttachment;
	private boolean inverse;

	public void clipStart (Skeleton skeleton, Slot slot, ClippingAttachment clip) {
		if (clipAttachment != null) return;
		int n = clip.getWorldVerticesLength();
		if (n < 6) return; // Do not port.
		clipAttachment = clip;
		inverse = clip.getInverse();

		clip.computeWorldVertices(skeleton, slot, 0, n, clippingPolygon.setSize(n), 0, 2);
		boolean convex = makeClockwise(clippingPolygon);
		if (convex || inverse || clip.getConvex()) {
			if (!convex) makeConvex(clippingPolygon);
			clippingPolygon.add(clippingPolygon.items[0], clippingPolygon.items[1]);
			clippingPolygons.add(clippingPolygon);
		} else {
			if (triangulator == null) triangulator = new Triangulator();
			clippingPolygons.addAll(triangulator.decompose(clippingPolygon, triangulator.triangulate(clippingPolygon)));
		}
	}

	public void clipEnd (Slot slot) {
		if (clipAttachment != null && clipAttachment.getEndSlot() == slot.getData()) clipEnd();
	}

	public void clipEnd () {
		if (clipAttachment == null) return;
		clipAttachment = null;
		clippingPolygons.clear();
	}

	public boolean isClipping () {
		return clipAttachment != null;
	}

	public boolean clipTriangles (float[] vertices, short[] triangles, int trianglesLength) {
		FloatArray clippedVertices = this.clippedVertices;
		clippedVertices.size = 0;
		ShortArray clippedTriangles = this.clippedTriangles;
		clippedTriangles.size = 0;
		short index = 0;

		if (inverse) {
			FloatArray polygon = clippingPolygons.items[0];
			for (int i = 0; i < trianglesLength; i += 3) {
				int t = triangles[i] << 1;
				float x1 = vertices[t], y1 = vertices[t + 1];
				t = triangles[i + 1] << 1;
				float x2 = vertices[t], y2 = vertices[t + 1];
				t = triangles[i + 2] << 1;
				float x3 = vertices[t], y3 = vertices[t + 1];
				clipInverse(x1, y1, x2, y2, x3, y3, polygon);

				float[] iv = inverseVertices.items;
				for (int offset = 0, nn = inverseVertices.size; offset < nn;) {
					int polygonSize = (int)iv[offset++];
					int vertexCount = polygonSize >> 1, s = clippedVertices.size;

					float[] cv = clippedVertices.setSize(s + polygonSize);
					System.arraycopy(iv, offset, cv, s, polygonSize);

					s = clippedTriangles.size;
					short[] ct = clippedTriangles.setSize(s + 3 * (vertexCount - 2));
					for (int ii = 1; ii < vertexCount - 1; ii++, s += 3) {
						ct[s] = index;
						ct[s + 1] = (short)(index + ii);
						ct[s + 2] = (short)(index + ii + 1);
					}
					index += vertexCount;
					offset += polygonSize;
				}
			}
			return true;
		}

		FloatArray clipOutput = this.clipOutput;
		FloatArray[] polygons = clippingPolygons.items;
		int polygonsCount = clippingPolygons.size;
		float[] clipOutputItems = null;
		for (int i = 0; i < trianglesLength; i += 3) {
			int t = triangles[i] << 1;
			float x1 = vertices[t], y1 = vertices[t + 1];
			t = triangles[i + 1] << 1;
			float x2 = vertices[t], y2 = vertices[t + 1];
			t = triangles[i + 2] << 1;
			float x3 = vertices[t], y3 = vertices[t + 1];
			for (int p = 0; p < polygonsCount; p++) {
				int s = clippedVertices.size;
				if (clip(x1, y1, x2, y2, x3, y3, polygons[p])) {
					clipOutputItems = clipOutput.items;
					int clipOutputLength = clipOutput.size;
					if (clipOutputLength == 0) continue;
					int clipOutputCount = clipOutputLength >> 1;

					float[] cv = clippedVertices.setSize(s + clipOutputLength);
					System.arraycopy(clipOutputItems, 0, cv, s, clipOutputLength);

					s = clippedTriangles.size;
					short[] ct = clippedTriangles.setSize(s + 3 * (clipOutputCount - 2));
					for (int ii = 1, nn = clipOutputCount - 1; ii < nn; ii++, s += 3) {
						ct[s] = index;
						ct[s + 1] = (short)(index + ii);
						ct[s + 2] = (short)(index + ii + 1);
					}
					index += clipOutputCount;
				} else {
					float[] cv = clippedVertices.setSize(s + 3 * 2);
					cv[s] = x1;
					cv[s + 1] = y1;
					cv[s + 2] = x2;
					cv[s + 3] = y2;
					cv[s + 4] = x3;
					cv[s + 5] = y3;

					s = clippedTriangles.size;
					short[] ct = clippedTriangles.setSize(s + 3);
					ct[s] = index;
					ct[s + 1] = (short)(index + 1);
					ct[s + 2] = (short)(index + 2);
					index += 3;
					break;
				}
			}
		}
		return clipOutputItems != null;
	}

	public boolean clipTriangles (float[] vertices, short[] triangles, int trianglesLength, float[] uvs, float light, float dark,
		boolean twoColor, int stride) {
		FloatArray clippedVertices = this.clippedVertices;
		clippedVertices.size = 0;
		ShortArray clippedTriangles = this.clippedTriangles;
		clippedTriangles.size = 0;
		short index = 0;

		if (inverse) {
			FloatArray polygon = clippingPolygons.items[0];
			for (int i = 0; i < trianglesLength; i += 3) {
				int t0 = triangles[i], t1 = triangles[i + 1], t2 = triangles[i + 2];
				float x1 = vertices[t0 * stride], y1 = vertices[t0 * stride + 1];
				float x2 = vertices[t1 * stride], y2 = vertices[t1 * stride + 1];
				float x3 = vertices[t2 * stride], y3 = vertices[t2 * stride + 1];
				clipInverse(x1, y1, x2, y2, x3, y3, polygon);
				int nn = inverseVertices.size;
				if (nn == 0) continue;

				float u1 = uvs[t0 <<= 1], v1 = uvs[t0 + 1];
				float u2 = uvs[t1 <<= 1], v2 = uvs[t1 + 1];
				float u3 = uvs[t2 <<= 1], v3 = uvs[t2 + 1];
				float d0 = y2 - y3, d1 = x3 - x2, d2 = x1 - x3, d4 = y3 - y1, d = 1 / (d0 * d2 + d1 * (y1 - y3));
				float[] iv = inverseVertices.items;
				for (int offset = 0; offset < nn;) {
					int polygonSize = (int)iv[offset++];
					int vertexCount = polygonSize >> 1;

					int s = clippedVertices.size;
					float[] cv = clippedVertices.setSize(s + vertexCount * stride);
					for (int ii = 0; ii < polygonSize; ii += 2, s += 2) {
						float x = iv[offset + ii], y = iv[offset + ii + 1];
						cv[s] = x;
						cv[s + 1] = y;
						cv[s + 2] = light;
						if (twoColor) {
							cv[s + 3] = dark;
							s += 4;
						} else
							s += 3;
						float c0 = x - x3, c1 = y - y3, a = (d0 * c0 + d1 * c1) * d, b = (d4 * c0 + d2 * c1) * d, c = 1 - a - b;
						cv[s] = u1 * a + u2 * b + u3 * c;
						cv[s + 1] = v1 * a + v2 * b + v3 * c;
					}

					s = clippedTriangles.size;
					short[] ct = clippedTriangles.setSize(s + 3 * (vertexCount - 2));
					for (int ii = 1; ii < vertexCount - 1; ii++, s += 3) {
						ct[s] = index;
						ct[s + 1] = (short)(index + ii);
						ct[s + 2] = (short)(index + ii + 1);
					}
					index += vertexCount;
					offset += polygonSize;
				}
			}
			return true;
		}

		FloatArray clipOutput = this.clipOutput;
		FloatArray[] polygons = clippingPolygons.items;
		int polygonsCount = clippingPolygons.size;
		float[] clipOutputItems = null;
		for (int i = 0; i < trianglesLength; i += 3) {
			int t = triangles[i];
			float x1 = vertices[t * stride], y1 = vertices[t * stride + 1];
			float u1 = uvs[t << 1], v1 = uvs[(t << 1) + 1];
			t = triangles[i + 1];
			float x2 = vertices[t * stride], y2 = vertices[t * stride + 1];
			float u2 = uvs[t << 1], v2 = uvs[(t << 1) + 1];
			t = triangles[i + 2];
			float x3 = vertices[t * stride], y3 = vertices[t * stride + 1];
			float u3 = uvs[t << 1], v3 = uvs[(t << 1) + 1];
			float d0 = y2 - y3, d1 = x3 - x2, d2 = x1 - x3, d4 = y3 - y1, d = 1 / (d0 * d2 + d1 * (y1 - y3));
			for (int p = 0; p < polygonsCount; p++) {
				int s = clippedVertices.size;
				if (clip(x1, y1, x2, y2, x3, y3, polygons[p])) {
					clipOutputItems = clipOutput.items;
					int clipOutputLength = clipOutput.size;
					if (clipOutputLength == 0) continue;
					int clipOutputCount = clipOutputLength >> 1;

					float[] cv = clippedVertices.setSize(s + clipOutputCount * stride);
					for (int ii = 0; ii < clipOutputLength; ii += 2, s += 2) {
						float x = clipOutputItems[ii], y = clipOutputItems[ii + 1];
						cv[s] = x;
						cv[s + 1] = y;
						cv[s + 2] = light;
						if (twoColor) {
							cv[s + 3] = dark;
							s += 4;
						} else
							s += 3;

						float c0 = x - x3, c1 = y - y3, a = (d0 * c0 + d1 * c1) * d, b = (d4 * c0 + d2 * c1) * d, c = 1 - a - b;
						cv[s] = u1 * a + u2 * b + u3 * c;
						cv[s + 1] = v1 * a + v2 * b + v3 * c;
					}

					s = clippedTriangles.size;
					short[] ct = clippedTriangles.setSize(s + 3 * (clipOutputCount - 2));
					clipOutputCount--;
					for (int ii = 1; ii < clipOutputCount; ii++, s += 3) {
						ct[s] = index;
						ct[s + 1] = (short)(index + ii);
						ct[s + 2] = (short)(index + ii + 1);
					}
					index += clipOutputCount + 1;
				} else {
					float[] cv = clippedVertices.setSize(s + 3 * stride);
					cv[s] = x1;
					cv[s + 1] = y1;
					cv[s + 2] = light;
					if (!twoColor) {
						cv[s + 3] = u1;
						cv[s + 4] = v1;

						cv[s + 5] = x2;
						cv[s + 6] = y2;
						cv[s + 7] = light;
						cv[s + 8] = u2;
						cv[s + 9] = v2;

						cv[s + 10] = x3;
						cv[s + 11] = y3;
						cv[s + 12] = light;
						cv[s + 13] = u3;
						cv[s + 14] = v3;
					} else {
						cv[s + 3] = dark;
						cv[s + 4] = u1;
						cv[s + 5] = v1;

						cv[s + 6] = x2;
						cv[s + 7] = y2;
						cv[s + 8] = light;
						cv[s + 9] = dark;
						cv[s + 10] = u2;
						cv[s + 11] = v2;

						cv[s + 12] = x3;
						cv[s + 13] = y3;
						cv[s + 14] = light;
						cv[s + 15] = dark;
						cv[s + 16] = u3;
						cv[s + 17] = v3;
					}

					s = clippedTriangles.size;
					short[] ct = clippedTriangles.setSize(s + 3);
					ct[s] = index;
					ct[s + 1] = (short)(index + 1);
					ct[s + 2] = (short)(index + 2);
					index += 3;
					break;
				}
			}
		}
		return clipOutputItems != null;
	}

	public boolean clipTrianglesUnpacked (float[] vertices, int vertexStart, short[] triangles, int trianglesLength, float[] uvs) {
		FloatArray clippedVertices = this.clippedVertices;
		clippedVertices.size = 0;
		ShortArray clippedTriangles = this.clippedTriangles;
		clippedTriangles.size = 0;
		FloatArray clippedUvs = this.clippedUvs;
		clippedUvs.size = 0;
		short index = 0;

		if (inverse) {
			FloatArray polygon = clippingPolygons.items[0];
			for (int i = 0; i < trianglesLength; i += 3) {
				int t0 = triangles[i] << 1, t1 = triangles[i + 1] << 1, t2 = triangles[i + 2] << 1;
				float x1 = vertices[vertexStart + t0], y1 = vertices[vertexStart + t0 + 1];
				float x2 = vertices[vertexStart + t1], y2 = vertices[vertexStart + t1 + 1];
				float x3 = vertices[vertexStart + t2], y3 = vertices[vertexStart + t2 + 1];
				clipInverse(x1, y1, x2, y2, x3, y3, polygon);
				int nn = inverseVertices.size;
				if (nn == 0) continue;

				float u1 = uvs[t0], v1 = uvs[t0 + 1];
				float u2 = uvs[t1], v2 = uvs[t1 + 1];
				float u3 = uvs[t2], v3 = uvs[t2 + 1];
				float d0 = y2 - y3, d1 = x3 - x2, d2 = x1 - x3, d4 = y3 - y1, d = 1 / (d0 * d2 + d1 * (y1 - y3));
				float[] iv = inverseVertices.items;
				for (int offset = 0; offset < nn;) {
					int polygonSize = (int)iv[offset++];
					int vertexCount = polygonSize >> 1;

					int s = clippedVertices.size;
					float[] cv = clippedVertices.setSize(s + polygonSize);
					float[] cu = clippedUvs.setSize(s + polygonSize);
					for (int ii = 0; ii < polygonSize; ii += 2, s += 2) {
						float x = iv[offset + ii], y = iv[offset + ii + 1];
						cv[s] = x;
						cv[s + 1] = y;
						float c0 = x - x3, c1 = y - y3, a = (d0 * c0 + d1 * c1) * d, b = (d4 * c0 + d2 * c1) * d, c = 1 - a - b;
						cu[s] = u1 * a + u2 * b + u3 * c;
						cu[s + 1] = v1 * a + v2 * b + v3 * c;
					}

					s = clippedTriangles.size;
					short[] ct = clippedTriangles.setSize(s + 3 * (vertexCount - 2));
					for (int ii = 1; ii < vertexCount - 1; ii++, s += 3) {
						ct[s] = index;
						ct[s + 1] = (short)(index + ii);
						ct[s + 2] = (short)(index + ii + 1);
					}
					index += vertexCount;
					offset += polygonSize;
				}
			}
			return true;
		}

		FloatArray clipOutput = this.clipOutput;
		FloatArray[] polygons = clippingPolygons.items;
		int polygonsCount = clippingPolygons.size;
		float[] clipOutputItems = null;
		for (int i = 0; i < trianglesLength; i += 3) {
			int t = triangles[i] << 1;
			float x1 = vertices[vertexStart + t], y1 = vertices[vertexStart + t + 1];
			float u1 = uvs[t], v1 = uvs[t + 1];
			t = triangles[i + 1] << 1;
			float x2 = vertices[vertexStart + t], y2 = vertices[vertexStart + t + 1];
			float u2 = uvs[t], v2 = uvs[t + 1];
			t = triangles[i + 2] << 1;
			float x3 = vertices[vertexStart + t], y3 = vertices[vertexStart + t + 1];
			float u3 = uvs[t], v3 = uvs[t + 1];
			float d0 = y2 - y3, d1 = x3 - x2, d2 = x1 - x3, d4 = y3 - y1, d = 1 / (d0 * d2 + d1 * (y1 - y3));
			for (int p = 0; p < polygonsCount; p++) {
				int s = clippedVertices.size;
				if (clip(x1, y1, x2, y2, x3, y3, polygons[p])) {
					clipOutputItems = clipOutput.items;
					int clipOutputLength = clipOutput.size;
					if (clipOutputLength == 0) continue;
					int clipOutputCount = clipOutputLength >> 1;

					float[] cv = clippedVertices.setSize(s + clipOutputCount * 2), cu = clippedUvs.setSize(s + clipOutputCount * 2);
					for (int ii = 0; ii < clipOutputLength; ii += 2, s += 2) {
						float x = clipOutputItems[ii], y = clipOutputItems[ii + 1];
						cv[s] = x;
						cv[s + 1] = y;

						float c0 = x - x3, c1 = y - y3, a = (d0 * c0 + d1 * c1) * d, b = (d4 * c0 + d2 * c1) * d, c = 1 - a - b;
						cu[s] = u1 * a + u2 * b + u3 * c;
						cu[s + 1] = v1 * a + v2 * b + v3 * c;
					}

					s = clippedTriangles.size;
					short[] ct = clippedTriangles.setSize(s + 3 * (clipOutputCount - 2));
					clipOutputCount--;
					for (int ii = 1; ii < clipOutputCount; ii++, s += 3) {
						ct[s] = index;
						ct[s + 1] = (short)(index + ii);
						ct[s + 2] = (short)(index + ii + 1);
					}
					index += clipOutputCount + 1;
				} else {
					float[] cv = clippedVertices.setSize(s + 3 * 2);
					cv[s] = x1;
					cv[s + 1] = y1;
					cv[s + 2] = x2;
					cv[s + 3] = y2;
					cv[s + 4] = x3;
					cv[s + 5] = y3;

					float[] cu = clippedUvs.setSize(s + 3 * 2);
					cu[s] = u1;
					cu[s + 1] = v1;
					cu[s + 2] = u2;
					cu[s + 3] = v2;
					cu[s + 4] = u3;
					cu[s + 5] = v3;

					s = clippedTriangles.size;
					short[] ct = clippedTriangles.setSize(s + 3);
					ct[s] = index;
					ct[s + 1] = (short)(index + 1);
					ct[s + 2] = (short)(index + 2);
					index += 3;
					break;
				}
			}
		}
		return clipOutputItems != null;
	}

	private boolean clip (float x1, float y1, float x2, float y2, float x3, float y3, FloatArray polygon) {
		FloatArray originalOutput = clipOutput;
		boolean clipped = false;

		FloatArray input, output; // Avoid copy at the end.
		if (polygon.size % 4 >= 2) {
			input = clipOutput;
			output = scratch;
		} else {
			input = scratch;
			output = clipOutput;
		}

		float[] v = polygon.items, iv = input.setSize(8);
		iv[0] = x1;
		iv[1] = y1;
		iv[2] = x2;
		iv[3] = y2;
		iv[4] = x3;
		iv[5] = y3;
		iv[6] = x1;
		iv[7] = y1;
		output.size = 0;

		int last = polygon.size - 4;
		for (int i = 0;; i += 2) {
			float edgeX = v[i], edgeY = v[i + 1], ex = edgeX - v[i + 2], ey = edgeY - v[i + 3];
			int outputStart = output.size;
			iv = input.items;
			for (int ii = 0, nn = input.size - 2; ii < nn;) {
				x1 = iv[ii];
				y1 = iv[ii + 1];
				ii += 2;
				x2 = iv[ii];
				y2 = iv[ii + 1];
				boolean s2 = ey * (edgeX - x2) > ex * (edgeY - y2);
				float s1 = ey * (edgeX - x1) - ex * (edgeY - y1);
				if (s1 > 0) {
					if (s2) // v1 in, v2 in
						output.add(x2, y2);
					else { // v1 in, v2 out
						float ix = x2 - x1, iy = y2 - y1, t = s1 / (ix * ey - iy * ex);
						if (t >= 0 && t <= 1) {
							output.add(x1 + ix * t, y1 + iy * t);
							clipped = true;
						} else
							output.add(x2, y2);
					}
				} else if (s2) { // v1 out, v2 in
					float ix = x2 - x1, iy = y2 - y1, t = s1 / (ix * ey - iy * ex);
					if (t >= 0 && t <= 1) {
						output.add(x1 + ix * t, y1 + iy * t, x2, y2);
						clipped = true;
					} else
						output.add(x2, y2);
				} else // v1 out, v2 out
					clipped = true;
			}
			if (outputStart == output.size) { // All outside.
				originalOutput.size = 0;
				return true;
			}

			output.add(output.items[0], output.items[1]);

			if (i == last) break;
			FloatArray temp = output;
			output = input;
			output.size = 0;
			input = temp;
		}

		if (originalOutput != output) {
			originalOutput.size = 0;
			originalOutput.addAll(output.items, 0, output.size - 2);
		} else
			originalOutput.setSize(originalOutput.size - 2);

		return clipped;
	}

	private void clipInverse (float x1, float y1, float x2, float y2, float x3, float y3, FloatArray polygon) {
		inverseVertices.size = 0;
		inverseVertices.ensureCapacity(polygon.size * 3);
		int vLast = polygon.size - 4;

		FloatArray input, output; // Avoid copy at the end.
		if (polygon.size % 4 >= 2) {
			input = clipOutput;
			output = scratch;
		} else {
			input = scratch;
			output = clipOutput;
		}

		float[] v = polygon.items, iv = input.setSize(8);
		iv[0] = x1;
		iv[1] = y1;
		iv[2] = x2;
		iv[3] = y2;
		iv[4] = x3;
		iv[5] = y3;
		iv[6] = x1;
		iv[7] = y1;
		output.size = 0;

		for (int i = 0;; i += 2) {
			float edgeX = v[i], edgeY = v[i + 1], ex = edgeX - v[i + 2], ey = edgeY - v[i + 3];
			int outputStart = output.size, fragmentStart = inverseVertices.size;
			inverseVertices.add(0);
			iv = input.items;
			for (int ii = 0, nn = input.size - 2; ii < nn;) {
				x1 = iv[ii];
				y1 = iv[ii + 1];
				ii += 2;
				x2 = iv[ii];
				y2 = iv[ii + 1];
				boolean s2 = ey * (edgeX - x2) > ex * (edgeY - y2);
				float s1 = ey * (edgeX - x1) - ex * (edgeY - y1);
				if (s1 > 0) {
					if (s2) // v1 in, v2 in
						output.add(x2, y2);
					else {
						// v1 in, v2 out
						float ix = x2 - x1, iy = y2 - y1, t = s1 / (ix * ey - iy * ex);
						if (t >= 0 && t <= 1) {
							float cx = x1 + ix * t, cy = y1 + iy * t;
							output.add(cx, cy);
							inverseVertices.add(cx, cy, x2, y2);
						} else
							output.add(x2, y2);
					}
				} else if (s2) { // v1 out, v2 in
					float dx = x2 - x1, dy = y2 - y1, t = s1 / (dx * ey - dy * ex);
					if (t >= 0 && t <= 1) {
						float cx = x1 + dx * t, cy = y1 + dy * t;
						inverseVertices.add(cx, cy);
						output.add(cx, cy, x2, y2);
					} else
						output.add(x2, y2);
				} else // v1 out, v2 out
					inverseVertices.add(x2, y2);
			}

			int fragmentSize = inverseVertices.size - fragmentStart - 1;
			if (fragmentSize >= 6)
				inverseVertices.items[fragmentStart] = fragmentSize;
			else
				inverseVertices.size = fragmentStart; // Degenerate.

			if (outputStart == output.size) break; // All outside.

			output.add(output.items[0], output.items[1]);

			if (i == vLast) break;
			FloatArray temp = output;
			output = input;
			output.size = 0;
			input = temp;
		}
	}

	private boolean makeClockwise (FloatArray polygon) {
		float[] v = polygon.items;
		int n = polygon.size;
		boolean noCW = true, noCCW = true;
		float area = 0, prevX = v[n - 2], prevY = v[n - 1], currX = v[0], currY = v[1];
		for (int i = 2; i < n; i += 2) {
			float nextX = v[i], nextY = v[i + 1];
			area += currX * nextY - nextX * currY;
			float cross = (currX - prevX) * (nextY - currY) - (currY - prevY) * (nextX - currX);
			noCCW &= cross <= 0;
			noCW &= cross >= 0;
			prevX = currX;
			prevY = currY;
			currX = nextX;
			currY = nextY;
		}
		area += currX * v[1] - v[0] * currY;
		float cross = (currX - prevX) * (v[1] - currY) - (currY - prevY) * (v[0] - currX);
		noCCW &= cross <= 0;
		noCW &= cross >= 0;
		if (area >= 0) {
			for (int i = 0, lastX = n - 2, half = n >> 1; i < half; i += 2) {
				float x = v[i], y = v[i + 1];
				int other = lastX - i;
				v[i] = v[other];
				v[i + 1] = v[other + 1];
				v[other] = x;
				v[other + 1] = y;
			}
			return noCW;
		}
		return noCCW;
	}

	private void makeConvex (FloatArray polygon) {
		int n = polygon.size;
		float[] v = polygon.ensureCapacity(n), sorted = clipOutput.setSize(n);
		sorted[0] = v[0];
		sorted[1] = v[1];
		for (int i = 2; i < n; i += 2) {
			float x = v[i], y = v[i + 1];
			int p = i - 2;
			for (; p >= 0 && (sorted[p] > x || (sorted[p] == x && sorted[p + 1] > y)); p -= 2) {
				sorted[p + 2] = sorted[p];
				sorted[p + 3] = sorted[p + 1];
			}
			sorted[p + 2] = x;
			sorted[p + 3] = y;
		}
		v[0] = sorted[0];
		v[1] = sorted[1];
		v[2] = sorted[2];
		v[3] = sorted[3];
		int s = 4;
		for (int i = 4; i < n; i += 2, s += 2) {
			float x = sorted[i], y = sorted[i + 1];
			while ((v[s - 2] - v[s - 4]) * (y - v[s - 3]) - (v[s - 1] - v[s - 3]) * (x - v[s - 4]) >= 0) {
				s -= 2;
				if (s == 2) break;
			}
			v[s] = x;
			v[s + 1] = y;
		}
		v[s] = sorted[n - 4];
		v[s + 1] = sorted[n - 3];
		int t = s;
		s += 2;
		for (int i = n - 6; i >= 0; i -= 2, s += 2) {
			float x = sorted[i], y = sorted[i + 1];
			while ((v[s - 2] - v[s - 4]) * (y - v[s - 3]) - (v[s - 1] - v[s - 3]) * (x - v[s - 4]) >= 0) {
				s -= 2;
				if (s == t) break;
			}
			v[s] = x;
			v[s + 1] = y;
		}
		polygon.size = s - 2;
	}

	public FloatArray getClippedVertices () {
		return clippedVertices;
	}

	/** Returns an empty array unless {@link #clipTrianglesUnpacked(float[], int, short[], int, float[])} was used. **/
	public FloatArray getClippedUvs () {
		return clippedUvs;
	}

	public ShortArray getClippedTriangles () {
		return clippedTriangles;
	}
}
