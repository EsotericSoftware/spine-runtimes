/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine.utils;

import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;
import com.badlogic.gdx.utils.ShortArray;
import com.esotericsoftware.spine.Slot;
import com.esotericsoftware.spine.attachments.ClippingAttachment;

public class SkeletonClipping {
	private final Clipper clipper = new Clipper();
	private final ConvexDecomposer decomposer = new ConvexDecomposer();
	private final FloatArray clippingPolygon = new FloatArray();
	private final FloatArray clipOutput = new FloatArray(400);
	private final FloatArray clippedVertices = new FloatArray(400);
	private final ShortArray clippedTriangles = new ShortArray(400);

	private ClippingAttachment clipAttachment;
	private Array<FloatArray> convexClippingPolygons;

	public void clipStart (Slot slot, ClippingAttachment clip) {
		if (clipAttachment != null) return;
		clipAttachment = clip;

		int n = clip.getWorldVerticesLength();
		float[] vertices = clippingPolygon.setSize(n);
		clip.computeWorldVertices(slot, 0, n, vertices, 0, 2);
		Clipper.makeClockwise(clippingPolygon);
		convexClippingPolygons = decomposer.decompose(clippingPolygon);
		for (FloatArray polygon : convexClippingPolygons) {
			Clipper.makeClockwise(polygon);
			polygon.add(polygon.items[0]);
			polygon.add(polygon.items[1]);
		}
	}

	public void clipEnd (int index) {
		if (clipAttachment == null || clipAttachment.getEndSlot() != index) return;
		clipAttachment = null;
		convexClippingPolygons = null;
		clippedVertices.clear();
		clippedTriangles.clear();
		clippingPolygon.clear();
	}

	public boolean isClipping () {
		return clipAttachment != null;
	}

	public void clipTriangles (float[] vertices, int verticesLength, short[] triangles, int trianglesLength, float[] uvs,
		float light, float dark, boolean twoColor) {

		Clipper clipper = this.clipper;
		FloatArray clipOutput = this.clipOutput, clippedVertices = this.clippedVertices;
		ShortArray clippedTriangles = this.clippedTriangles;
		int vertexSize = twoColor ? 6 : 5;

		short index = 0;
		clippedVertices.clear();
		clippedTriangles.clear();
		for (FloatArray convexClippingPolygon : convexClippingPolygons) {
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

				int s = clippedVertices.size;
				if (clipper.clip(x1, y1, x2, y2, x3, y3, convexClippingPolygon, clipOutput)) {
					if (clipOutput.size == 0) continue;
					float d0 = y2 - y3, d1 = x3 - x2, d2 = x1 - x3, d4 = y3 - y1;
					float d = 1 / (d0 * d2 + d1 * (y1 - y3));

					float[] clipOutputItems = clipOutput.items;
					float[] clippedVerticesItems = clippedVertices.setSize(s + (clipOutput.size >> 1) * vertexSize);
					for (int ii = 0, nn = clipOutput.size; ii < nn; ii += 2) {
						float x = clipOutputItems[ii], y = clipOutputItems[ii + 1];
						float c0 = x - x3, c1 = y - y3;
						float a = (d0 * c0 + d1 * c1) * d;
						float b = (d4 * c0 + d2 * c1) * d;
						float c = 1 - a - b;
						float u = u1 * a + u2 * b + u3 * c;
						float v = v1 * a + v2 * b + v3 * c;
						clippedVerticesItems[s] = x;
						clippedVerticesItems[s + 1] = y;
						clippedVerticesItems[s + 2] = light;
						if (twoColor) {
							clippedVerticesItems[s + 3] = dark;
							clippedVerticesItems[s + 4] = u;
							clippedVerticesItems[s + 5] = v;
							s += 6;
						} else {
							clippedVerticesItems[s + 3] = u;
							clippedVerticesItems[s + 4] = v;
							s += 5;
						}
					}

					s = clippedTriangles.size;
					short[] clippedTrianglesItems = clippedTriangles.setSize(s + 3 * ((clipOutput.size >> 1) - 2));
					for (int ii = 1, nn = (clipOutput.size >> 1) - 1; ii < nn; ii++) {
						clippedTrianglesItems[s] = index;
						clippedTrianglesItems[s + 1] = (short)(index + ii);
						clippedTrianglesItems[s + 2] = (short)(index + ii + 1);
						s += 3;
					}
					index += clipOutput.size >> 1;

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
					clippedTrianglesItems[s] = index++;
					clippedTrianglesItems[s + 1] = index++;
					clippedTrianglesItems[s + 2] = index++;
				}
			}
		}
	}

	public FloatArray getClippedVertices () {
		return clippedVertices;
	}

	public ShortArray getClippedTriangles () {
		return clippedTriangles;
	}
}
