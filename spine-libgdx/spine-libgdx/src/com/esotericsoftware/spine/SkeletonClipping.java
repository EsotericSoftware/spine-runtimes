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

package com.esotericsoftware.spine;

import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;
import com.badlogic.gdx.utils.ShortArray;
import com.esotericsoftware.spine.attachments.ClippingAttachment;
import com.esotericsoftware.spine.utils.Clipper;
import com.esotericsoftware.spine.utils.ConvexDecomposer;

public class SkeletonClipping {
	private ClippingAttachment clipAttachment;
	private Clipper clipper = new Clipper();
	private ConvexDecomposer decomposer = new ConvexDecomposer();
	private FloatArray clippingPolygon = new FloatArray(400);
	private Array<FloatArray> convexClippingPolygons;
	private FloatArray clipOutput = new FloatArray(400);
	private FloatArray clippedVertices = new FloatArray(400);
	private ShortArray clippedTriangles = new ShortArray(400);

	public void clipStart (Slot slot, ClippingAttachment clip) {
		if (clipAttachment != null) return;
		clipAttachment = clip;

		int n = clip.getWorldVerticesLength();
		float[] vertices = this.clippingPolygon.setSize(n);
		clip.computeWorldVertices(slot, 0, n, vertices, 0, 2);
		convexClippingPolygons = decomposer.decompose(clippingPolygon);
		for (FloatArray poly : convexClippingPolygons) {
			Clipper.makeClockwise(poly);
			poly.add(poly.items[0]);
			poly.add(poly.items[1]);
		}
	}

	public void clipEnd () {
		clippedVertices.clear();
		clippedTriangles.clear();
		clippingPolygon.clear();
		convexClippingPolygons = null;
		clipAttachment = null;
	}

	public boolean isClipping () {
		return clipAttachment != null;
	}

	public ClippingAttachment getClippingAttachment () {
		return clipAttachment;
	}

	public void clipTriangles (final float[] vertices, final int verticesLength, final short[] triangles,
		final int trianglesLength, final float uvs[], final float dark, final float light, final boolean twoColor) {
		short idx = 0;
		clippedVertices.clear();
		clippedTriangles.clear();
		for (FloatArray convexClippingPolygon : convexClippingPolygons) {
			for (int i = 0; i < trianglesLength; i += 3) {
				int vertexOffset = triangles[i] << 1;
				float x1 = vertices[vertexOffset];
				float y1 = vertices[vertexOffset + 1];
				float u1 = uvs[vertexOffset];
				float v1 = uvs[vertexOffset + 1];

				vertexOffset = triangles[i + 1] << 1;
				float x2 = vertices[vertexOffset];
				float y2 = vertices[vertexOffset + 1];
				float u2 = uvs[vertexOffset];
				float v2 = uvs[vertexOffset + 1];

				vertexOffset = triangles[i + 2] << 1;
				float x3 = vertices[vertexOffset];
				float y3 = vertices[vertexOffset + 1];
				float u3 = uvs[vertexOffset];
				float v3 = uvs[vertexOffset + 1];

				boolean clipped = clipper.clip(x1, y1, x2, y2, x3, y3, convexClippingPolygon, clipOutput);
				if (clipped) {
					if (clipOutput.size == 0) continue;
					float d0 = y2 - y3;
					float d1 = x3 - x2;
					float d2 = x1 - x3;
					float d3 = y1 - y3;
					float d4 = y3 - y1;

					float denom = 1 / (d0 * d2 + d1 * d3);

					float[] clipVertices = clipOutput.items;
					int s = clippedVertices.size;
					clippedVertices.setSize(s + (clipOutput.size >> 1) * (twoColor ? 6 : 5));
					final float[] clippedVerticesArray = clippedVertices.items;

					for (int j = 0, n = clipOutput.size; j < n; j += 2) {
						float x = clipVertices[j];
						float y = clipVertices[j + 1];

						float c0 = x - x3;
						float c1 = y - y3;
						float a = (d0 * c0 + d1 * c1) * denom;
						float b = (d4 * c0 + d2 * c1) * denom;
						float c = 1.0f - a - b;

						float u = u1 * a + u2 * b + u3 * c;
						float v = v1 * a + v2 * b + v3 * c;
						clippedVerticesArray[s++] = x;
						clippedVerticesArray[s++] = y;
						clippedVerticesArray[s++] = light;
						if (twoColor) clippedVerticesArray[s++] = dark;
						clippedVerticesArray[s++] = u;
						clippedVerticesArray[s++] = v;
					}

					s = clippedTriangles.size;
					clippedTriangles.setSize(s + 3 * ((clipOutput.size >> 1) - 2));
					final short[] clippedTrianglesArray = clippedTriangles.items;

					for (int j = 1, n = (clipOutput.size >> 1) - 1; j < n; j++) {
						clippedTrianglesArray[s++] = idx;
						clippedTrianglesArray[s++] = (short)(idx + j);
						clippedTrianglesArray[s++] = (short)(idx + j + 1);
					}

					idx += clipOutput.size >> 1;
				} else {
					int s = clippedVertices.size;
					clippedVertices.setSize(s + 3 * (twoColor ? 6 : 5));
					final float[] clippedVerticesArray = clippedVertices.items;

					if (!twoColor) {
						clippedVerticesArray[s] = x1;
						clippedVerticesArray[s + 1] = y1;
						clippedVerticesArray[s + 2] = light;
						clippedVerticesArray[s + 3] = u1;
						clippedVerticesArray[s + 4] = v1;

						clippedVerticesArray[s + 5] = x2;
						clippedVerticesArray[s + 6] = y2;
						clippedVerticesArray[s + 7] = light;
						clippedVerticesArray[s + 8] = u2;
						clippedVerticesArray[s + 9] = v2;

						clippedVerticesArray[s + 10] = x3;
						clippedVerticesArray[s + 11] = y3;
						clippedVerticesArray[s + 12] = light;
						clippedVerticesArray[s + 13] = u3;
						clippedVerticesArray[s + 14] = v3;
					} else {
						clippedVerticesArray[s] = x1;
						clippedVerticesArray[s + 1] = y1;
						clippedVerticesArray[s + 2] = light;
						clippedVerticesArray[s + 3] = dark;
						clippedVerticesArray[s + 4] = u1;
						clippedVerticesArray[s + 5] = v1;

						clippedVerticesArray[s + 6] = x2;
						clippedVerticesArray[s + 7] = y2;
						clippedVerticesArray[s + 8] = light;
						clippedVerticesArray[s + 9] = dark;
						clippedVerticesArray[s + 10] = u2;
						clippedVerticesArray[s + 11] = v2;

						clippedVerticesArray[s + 12] = x3;
						clippedVerticesArray[s + 13] = y3;
						clippedVerticesArray[s + 14] = light;
						clippedVerticesArray[s + 15] = dark;
						clippedVerticesArray[s + 16] = u3;
						clippedVerticesArray[s + 17] = v3;
					}

					s = clippedTriangles.size;
					clippedTriangles.setSize(s + 3);
					final short[] clippedTrianglesArray = clippedTriangles.items;
					clippedTrianglesArray[s] = idx++;
					clippedTrianglesArray[s + 1] = idx++;
					clippedTrianglesArray[s + 2] = idx++;
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
