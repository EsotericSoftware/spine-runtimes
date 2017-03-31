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

import java.util.Iterator;

import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.BooleanArray;
import com.badlogic.gdx.utils.FloatArray;
import com.badlogic.gdx.utils.Pool;
import com.badlogic.gdx.utils.ShortArray;

public class ConvexDecomposer {
	private final Array<FloatArray> convexPolygons = new Array();
	private final Array<ShortArray> convexPolygonsIndices = new Array();

	private final ShortArray indicesArray = new ShortArray();
	private short[] indices;
	private float[] vertices;
	private int vertexCount;
	private final BooleanArray isConcaveArray = new BooleanArray();
	private final ShortArray triangles = new ShortArray();

	private final Pool<FloatArray> polygonPool = new Pool<FloatArray>() {
		protected FloatArray newObject () {
			return new FloatArray(16);
		}
	};

	private final Pool<ShortArray> polygonIndicesPool = new Pool<ShortArray>() {
		protected ShortArray newObject () {
			return new ShortArray(16);
		}
	};

	public Array<FloatArray> decompose (FloatArray input) {
		vertices = input.items;
		int vertexCount = this.vertexCount = input.size / 2;

		ShortArray indicesArray = this.indicesArray;
		indicesArray.clear();
		short[] indices = this.indices = indicesArray.setSize(vertexCount);
		for (short i = 0; i < vertexCount; i++)
			indices[i] = i;

		boolean[] isConcave = isConcaveArray.setSize(vertexCount);
		for (int i = 0, n = vertexCount; i < n; ++i)
			isConcave[i] = isConcave(i);

		ShortArray triangles = this.triangles;
		triangles.clear();
		triangles.ensureCapacity(Math.max(0, vertexCount - 2) * 4);

		// Triangulate.
		while (this.vertexCount > 3) {
			int earTipIndex = findEarTip();
			cutEarTip(earTipIndex);

			int previousIndex = previousIndex(earTipIndex);
			int nextIndex = earTipIndex == vertexCount ? 0 : earTipIndex;
			isConcave[previousIndex] = isConcave(previousIndex);
			isConcave[nextIndex] = isConcave(nextIndex);
		}

		if (this.vertexCount == 3) {
			triangles.add(indicesArray.get(2));
			triangles.add(indicesArray.get(0));
			triangles.add(indicesArray.get(1));
		}

		polygonPool.freeAll(convexPolygons);
		convexPolygons.clear();
		polygonIndicesPool.freeAll(convexPolygonsIndices);
		convexPolygonsIndices.clear();

		ShortArray polygonIndices = polygonIndicesPool.obtain();
		polygonIndices.clear();
		FloatArray polygon = polygonPool.obtain();
		polygon.clear();
		int fanBaseIndex = -1, lastWinding = 0;

		// Merge subsequent triangles if they form a triangle fan.
		for (int i = 0, n = triangles.size; i < n; i += 3) {
			int t1 = triangles.get(i) << 1, t2 = triangles.get(i + 1) << 1, t3 = triangles.get(i + 2) << 1;
			float x1 = input.get(t1), y1 = input.get(t1 + 1);
			float x2 = input.get(t2), y2 = input.get(t2 + 1);
			float x3 = input.get(t3), y3 = input.get(t3 + 1);

			// If the base of the last triangle is the same as this triangle's base, check if they form a convex polygon (triangle
			// fan).
			boolean merged = false;
			if (fanBaseIndex == t1) {
				int o = polygon.size - 4;
				int winding1 = winding(polygon.get(o), polygon.get(o + 1), polygon.get(o + 2), polygon.get(o + 3), x3, y3);
				int winding2 = winding(x3, y3, polygon.get(0), polygon.get(1), polygon.get(2), polygon.get(3));
				if (winding1 == lastWinding && winding2 == lastWinding) {
					polygon.add(x3);
					polygon.add(y3);
					polygonIndices.add(t3);
					merged = true;
				}
			}

			// Otherwise make this triangle the new base.
			if (!merged) {
				if (polygon.size > 0) {
					convexPolygons.add(polygon);
					convexPolygonsIndices.add(polygonIndices);
				}
				polygon = polygonPool.obtain();
				polygon.clear();
				polygon.add(x1);
				polygon.add(y1);
				polygon.add(x2);
				polygon.add(y2);
				polygon.add(x3);
				polygon.add(y3);
				polygonIndices = polygonIndicesPool.obtain();
				polygonIndices.clear();
				polygonIndices.add(t1);
				polygonIndices.add(t2);
				polygonIndices.add(t3);
				lastWinding = winding(x1, y1, x2, y2, x3, y3);
				fanBaseIndex = t1;
			}
		}

		if (polygon.size > 0) {
			convexPolygons.add(polygon);
			convexPolygonsIndices.add(polygonIndices);
		}

		// Go through the list of polygons and try to merge the remaining triangles with the found triangle fans.
		for (int i = 0, n = convexPolygons.size; i < n; i++) {
			polygonIndices = convexPolygonsIndices.get(i);
			if (polygonIndices.size == 0) continue;
			int firstIndex = polygonIndices.get(0);
			int lastIndex = polygonIndices.get(polygonIndices.size - 1);

			polygon = convexPolygons.get(i);
			int o = polygon.size - 4;
			float prevPrevX = polygon.get(o);
			float prevPrevY = polygon.get(o + 1);
			float prevX = polygon.get(o + 2);
			float prevY = polygon.get(o + 3);
			float firstX = polygon.get(0);
			float firstY = polygon.get(1);
			float secondX = polygon.get(2);
			float secondY = polygon.get(3);
			int winding = winding(prevPrevX, prevPrevY, prevX, prevY, firstX, firstY);

			for (int ii = 0; ii < n; ii++) {
				if (ii == i) continue;
				ShortArray otherIndices = convexPolygonsIndices.get(ii);
				if (otherIndices.size != 3) continue;
				int otherFirstIndex = otherIndices.get(0);
				int otherSecondIndex = otherIndices.get(1);
				int otherLastIndex = otherIndices.get(2);

				FloatArray otherPoly = convexPolygons.get(ii);
				float x3 = otherPoly.get(otherPoly.size - 2);
				float y3 = otherPoly.get(otherPoly.size - 1);

				if (otherFirstIndex != firstIndex || otherSecondIndex != lastIndex) continue;
				int winding1 = winding(prevPrevX, prevPrevY, prevX, prevY, x3, y3);
				int winding2 = winding(x3, y3, firstX, firstY, secondX, secondY);
				if (winding1 == winding && winding2 == winding) {
					otherPoly.clear();
					otherIndices.clear();
					polygon.add(x3);
					polygon.add(y3);
					polygonIndices.add(otherLastIndex);
					prevPrevX = prevX;
					prevPrevY = prevY;
					prevX = x3;
					prevY = y3;
					ii = 0;
				}
			}
		}

		// Remove empty polygons that resulted from the merge step above.
		for (Iterator<FloatArray> iter = convexPolygons.iterator(); iter.hasNext();) {
			polygon = iter.next();
			if (polygon.size == 0) {
				iter.remove();
				polygonPool.free(polygon);
			}
		}

		return convexPolygons;
	}

	private boolean isConcave (int index) {
		short[] indices = this.indices;
		int previous = indices[previousIndex(index)] * 2;
		int current = indices[index] * 2;
		int next = indices[nextIndex(index)] * 2;
		float[] vertices = this.vertices;
		return !positiveArea(vertices[previous], vertices[previous + 1], vertices[current], vertices[current + 1], vertices[next],
			vertices[next + 1]);
	}

	private int findEarTip () {
		int vertexCount = this.vertexCount;
		for (int i = 0; i < vertexCount; i++)
			if (isEarTip(i)) return i;

		boolean[] isConcave = this.isConcaveArray.items;
		for (int i = 0; i < vertexCount; i++)
			if (!isConcave[i]) return i;
		return 0;
	}

	private boolean isEarTip (int earTipIndex) {
		boolean[] isConcave = this.isConcaveArray.items;
		if (isConcave[earTipIndex]) return false;

		int previousIndex = previousIndex(earTipIndex);
		int nextIndex = nextIndex(earTipIndex);
		short[] indices = this.indices;
		int p1 = indices[previousIndex] * 2;
		int p2 = indices[earTipIndex] * 2;
		int p3 = indices[nextIndex] * 2;
		float[] vertices = this.vertices;
		float p1x = vertices[p1], p1y = vertices[p1 + 1];
		float p2x = vertices[p2], p2y = vertices[p2 + 1];
		float p3x = vertices[p3], p3y = vertices[p3 + 1];

		for (int i = nextIndex(nextIndex); i != previousIndex; i = nextIndex(i)) {
			if (isConcave[i]) {
				int v = indices[i] * 2;
				float vx = vertices[v], vy = vertices[v + 1];
				if (positiveArea(p3x, p3y, p1x, p1y, vx, vy)) {
					if (positiveArea(p1x, p1y, p2x, p2y, vx, vy)) {
						if (positiveArea(p2x, p2y, p3x, p3y, vx, vy)) return false;
					}
				}
			}
		}
		return true;
	}

	private void cutEarTip (int earTipIndex) {
		short[] indices = this.indices;
		ShortArray triangles = this.triangles;

		triangles.add(indices[previousIndex(earTipIndex)]);
		triangles.add(indices[earTipIndex]);
		triangles.add(indices[nextIndex(earTipIndex)]);

		indicesArray.removeIndex(earTipIndex);
		isConcaveArray.removeIndex(earTipIndex);
		vertexCount--;
	}

	private int previousIndex (int index) {
		return (index == 0 ? vertexCount : index) - 1;
	}

	private int nextIndex (int index) {
		return (index + 1) % vertexCount;
	}

	static private boolean positiveArea (float p1x, float p1y, float p2x, float p2y, float p3x, float p3y) {
		return p1x * (p3y - p2y) + p2x * (p1y - p3y) + p3x * (p2y - p1y) >= 0;
	}

	static private int winding (float p1x, float p1y, float p2x, float p2y, float p3x, float p3y) {
		float px = p2x - p1x, py = p2y - p1y;
		return p3x * py - p3y * px + px * p1y - p1x * py >= 0 ? 1 : -1;
	}
}
