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
import com.badlogic.gdx.utils.BooleanArray;
import com.badlogic.gdx.utils.FloatArray;
import com.badlogic.gdx.utils.Pool;
import com.badlogic.gdx.utils.ShortArray;

class Triangulator {
	private final Array<FloatArray> convexPolygons = new Array(false, 8, FloatArray[]::new);
	private final Array<ShortArray> convexPolygonsIndices = new Array(false, 8, ShortArray[]::new);

	private final ShortArray indicesArray = new ShortArray();
	private final BooleanArray isConcaveArray = new BooleanArray();
	private final ShortArray triangles = new ShortArray();

	private final Pool<FloatArray> polygonPool = new Pool() {
		protected FloatArray newObject () {
			return new FloatArray(16);
		}
	};

	private final Pool<ShortArray> polygonIndicesPool = new Pool() {
		protected ShortArray newObject () {
			return new ShortArray(16);
		}
	};

	public ShortArray triangulate (FloatArray verticesArray) {
		float[] vertices = verticesArray.items;
		int vertexCount = verticesArray.size >> 1;

		ShortArray indicesArray = this.indicesArray;
		indicesArray.clear();
		short[] indices = indicesArray.setSize(vertexCount);
		for (short i = 0; i < vertexCount; i++)
			indices[i] = i;

		BooleanArray isConcaveArray = this.isConcaveArray;
		boolean[] isConcave = isConcaveArray.setSize(vertexCount);
		for (int i = 0; i < vertexCount; i++)
			isConcave[i] = isConcave(i, vertexCount, vertices, indices);

		ShortArray triangles = this.triangles;
		triangles.clear();
		triangles.ensureCapacity(Math.max(0, vertexCount - 2) * 3);

		while (vertexCount > 3) {
			// Find ear tip.
			int previous = vertexCount - 1, i = 0, next = 1;
			while (true) {
				outer:
				if (!isConcave[i]) {
					int p1 = indices[previous] << 1, p2 = indices[i] << 1, p3 = indices[next] << 1;
					float p1x = vertices[p1], p1y = vertices[p1 + 1];
					float p2x = vertices[p2], p2y = vertices[p2 + 1];
					float p3x = vertices[p3], p3y = vertices[p3 + 1];
					for (int ii = next + 1 < vertexCount ? next + 1 : 0; ii != previous;) {
						if (isConcave[ii]) {
							int v = indices[ii] << 1;
							float vx = vertices[v], vy = vertices[v + 1];
							if (positiveArea(p3x, p3y, p1x, p1y, vx, vy) //
								&& positiveArea(p1x, p1y, p2x, p2y, vx, vy) //
								&& positiveArea(p2x, p2y, p3x, p3y, vx, vy)) break outer;
						}
						if (++ii == vertexCount) ii = 0;
					}
					break;
				}

				if (next == 0) {
					do {
						if (!isConcave[i]) break;
						i--;
					} while (i > 0);
					previous = i > 0 ? i - 1 : vertexCount - 1;
					next = i + 1 < vertexCount ? i + 1 : 0;
					break;
				}

				previous = i;
				i = next;
				if (++next == vertexCount) next = 0;
			}

			// Cut ear tip.
			triangles.add(indices[previous], indices[i], indices[next]);
			indicesArray.removeIndex(i);
			isConcaveArray.removeIndex(i);
			vertexCount--;

			int previousIndex = i > 0 ? i - 1 : vertexCount - 1;
			int nextIndex = i < vertexCount ? i : 0;
			isConcave[previousIndex] = isConcave(previousIndex, vertexCount, vertices, indices);
			isConcave[nextIndex] = isConcave(nextIndex, vertexCount, vertices, indices);
		}
		if (vertexCount == 3) triangles.add(indices[2], indices[0], indices[1]);
		return triangles;
	}

	public Array<FloatArray> decompose (FloatArray verticesArray, ShortArray triangles) {
		float[] vertices = verticesArray.items;

		Array<FloatArray> convexPolygons = this.convexPolygons;
		polygonPool.freeAll(convexPolygons);
		convexPolygons.clear();

		Array<ShortArray> convexPolygonsIndices = this.convexPolygonsIndices;
		polygonIndicesPool.freeAll(convexPolygonsIndices);
		convexPolygonsIndices.clear();

		ShortArray polygonIndices = polygonIndicesPool.obtain();
		polygonIndices.clear();

		FloatArray polygon = polygonPool.obtain();
		polygon.clear();

		// Merge subsequent triangles if they form a triangle fan.
		int fanBaseIndex = -1, lastWinding = 0;
		short[] trianglesItems = triangles.items;
		for (int i = 0, n = triangles.size; i < n; i += 3) {
			int t1 = trianglesItems[i] << 1, t2 = trianglesItems[i + 1] << 1, t3 = trianglesItems[i + 2] << 1;
			float x1 = vertices[t1], y1 = vertices[t1 + 1];
			float x2 = vertices[t2], y2 = vertices[t2 + 1];
			float x3 = vertices[t3], y3 = vertices[t3 + 1];

			// If the base of the last triangle is the same as this triangle, check if they form a convex polygon (triangle fan).
			if (fanBaseIndex == t1) {
				int o = polygon.size - 4;
				float[] p = polygon.items;
				if (winding(p[o], p[o + 1], p[o + 2], p[o + 3], x3, y3) == lastWinding
					&& winding(x3, y3, p[0], p[1], p[2], p[3]) == lastWinding) {
					polygon.add(x3, y3);
					polygonIndices.add(t3);
					continue;
				}
			}

			// Otherwise make this triangle the new base.
			if (polygon.size > 0) {
				convexPolygons.add(polygon);
				convexPolygonsIndices.add(polygonIndices);
				polygon = polygonPool.obtain();
				polygonIndices = polygonIndicesPool.obtain();
			}
			polygon.clear();
			polygon.add(x1, y1, x2, y2);
			polygon.add(x3, y3);
			polygonIndices.clear();
			polygonIndices.add((short)t1, (short)t2, (short)t3);
			lastWinding = winding(x1, y1, x2, y2, x3, y3);
			fanBaseIndex = t1;
		}

		if (polygon.size > 0) {
			convexPolygons.add(polygon);
			convexPolygonsIndices.add(polygonIndices);
		}

		// Merge remaining triangles with the found triangle fans.
		ShortArray[] convexPolygonsIndicesItems = convexPolygonsIndices.items;
		FloatArray[] convexPolygonsItems = convexPolygons.items;
		for (int i = 0, n = convexPolygons.size; i < n; i++) {
			polygonIndices = convexPolygonsIndicesItems[i];
			if (polygonIndices.size == 0) continue;
			int firstIndex = polygonIndices.items[0];
			int lastIndex = polygonIndices.items[polygonIndices.size - 1];

			polygon = convexPolygonsItems[i];
			int o = polygon.size - 4;
			float[] p = polygon.items;
			float prevPrevX = p[o], prevPrevY = p[o + 1];
			float prevX = p[o + 2], prevY = p[o + 3];
			float firstX = p[0], firstY = p[1];
			float secondX = p[2], secondY = p[3];
			int winding = winding(prevPrevX, prevPrevY, prevX, prevY, firstX, firstY);

			for (int ii = 0; ii < n; ii++) {
				if (ii == i) continue;
				ShortArray otherIndices = convexPolygonsIndicesItems[ii];
				if (otherIndices.size != 3) continue;
				int otherFirstIndex = otherIndices.items[0];
				int otherSecondIndex = otherIndices.items[1];
				int otherLastIndex = otherIndices.items[2];

				FloatArray otherPoly = convexPolygonsItems[ii];
				float x3 = otherPoly.items[otherPoly.size - 2], y3 = otherPoly.items[otherPoly.size - 1];

				if (otherFirstIndex != firstIndex || otherSecondIndex != lastIndex) continue;
				if (winding(prevPrevX, prevPrevY, prevX, prevY, x3, y3) == winding
					&& winding(x3, y3, firstX, firstY, secondX, secondY) == winding) {
					otherPoly.clear();
					otherIndices.clear();
					polygon.add(x3, y3);
					polygonIndices.add(otherLastIndex);
					lastIndex = otherLastIndex;
					prevPrevX = prevX;
					prevPrevY = prevY;
					prevX = x3;
					prevY = y3;
					ii = -1;
				}
			}
		}

		// Remove empty polygons from the merge step above.
		for (int i = convexPolygons.size - 1; i >= 0; i--) {
			polygon = convexPolygonsItems[i];
			if (polygon.size == 0) {
				convexPolygons.removeIndex(i);
				polygonPool.free(polygon);
				polygonIndices = convexPolygonsIndices.removeIndex(i);
				polygonIndicesPool.free(polygonIndices);
			} else
				polygon.add(polygon.items[0], polygon.items[1]);
		}
		return convexPolygons;
	}

	static private boolean isConcave (int index, int vertexCount, float[] vertices, short[] indices) {
		int previous = indices[index > 0 ? index - 1 : vertexCount - 1] << 1;
		int current = indices[index] << 1;
		int next = indices[index + 1 < vertexCount ? index + 1 : 0] << 1;
		return !positiveArea(vertices[previous], vertices[previous + 1], vertices[current], vertices[current + 1], vertices[next],
			vertices[next + 1]);
	}

	static private boolean positiveArea (float p1x, float p1y, float p2x, float p2y, float p3x, float p3y) {
		return p1x * (p3y - p2y) + p2x * (p1y - p3y) + p3x * (p2y - p1y) >= 0;
	}

	static private int winding (float p1x, float p1y, float p2x, float p2y, float p3x, float p3y) {
		return p1x * (p3y - p2y) + p2x * (p1y - p3y) + p3x * (p2y - p1y) >= 0 ? 1 : -1;
	}
}
