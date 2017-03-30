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
import com.badlogic.gdx.utils.FloatArray;
import com.badlogic.gdx.utils.IntArray;
import com.badlogic.gdx.utils.Pool;
import com.badlogic.gdx.utils.ShortArray;

public class ConvexDecomposer {
	static private final int CONCAVE = -1;
	static private final int CONVEX = 1;

	private Pool<FloatArray> polygonPool = new Pool<FloatArray>() {
		@Override
		protected FloatArray newObject () {
			return new FloatArray(16);
		}
	};

	private Pool<ShortArray> polygonIndicesPool = new Pool<ShortArray>() {
		@Override
		protected ShortArray newObject () {
			return new ShortArray(16);
		}
	};

	private Array<FloatArray> convexPolygons = new Array<FloatArray>();
	private Array<ShortArray> convexPolygonsIndices = new Array<ShortArray>();

	private final ShortArray indicesArray = new ShortArray();
	private short[] indices;
	private float[] vertices;
	private int vertexCount;
	private final IntArray vertexTypes = new IntArray();
	private final ShortArray triangles = new ShortArray();

	public Array<FloatArray> decompose (FloatArray polygon) {
		this.vertices = polygon.items;
		int vertexCount = this.vertexCount = polygon.size / 2;

		ShortArray indicesArray = this.indicesArray;
		indicesArray.clear();
		indicesArray.ensureCapacity(vertexCount);
		indicesArray.size = vertexCount;
		short[] indices = this.indices = indicesArray.items;
		for (short i = 0; i < vertexCount; i++)
			indices[i] = i;

		IntArray vertexTypes = this.vertexTypes;
		vertexTypes.clear();
		vertexTypes.ensureCapacity(vertexCount);
		for (int i = 0, n = vertexCount; i < n; ++i)
			vertexTypes.add(classifyVertex(i));

		ShortArray triangles = this.triangles;
		triangles.clear();
		triangles.ensureCapacity(Math.max(0, vertexCount - 2) * 4);

		// Triangulate
		while (this.vertexCount > 3) {
			int earTipIndex = findEarTip();
			cutEarTip(earTipIndex);

			int previousIndex = previousIndex(earTipIndex);
			int nextIndex = earTipIndex == vertexCount ? 0 : earTipIndex;
			vertexTypes.set(previousIndex, classifyVertex(previousIndex));
			vertexTypes.set(nextIndex, classifyVertex(nextIndex));
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

		ShortArray polyIndices = polygonIndicesPool.obtain();
		polyIndices.clear();
		FloatArray poly = polygonPool.obtain();
		poly.clear();
		int fanBaseIndex = -1;
		int lastWinding = 0;

		// Merge subsequent triangles if they form a triangle fan
		for (int i = 0, n = triangles.size; i < n; i += 3) {
			int idx1 = triangles.get(i) << 1;
			int idx2 = triangles.get(i + 1) << 1;
			int idx3 = triangles.get(i + 2) << 1;

			float x1 = polygon.get(idx1);
			float y1 = polygon.get(idx1 + 1);
			float x2 = polygon.get(idx2);
			float y2 = polygon.get(idx2 + 1);
			float x3 = polygon.get(idx3);
			float y3 = polygon.get(idx3 + 1);

			// if the base of the last triangle
			// is the same as this triangle's base
			// check if they form a convex polygon (triangle fan)
			boolean merged = false;
			if (fanBaseIndex == idx1) {
				int o = poly.size - 4;
				int winding1 = winding(poly.get(o), poly.get(o + 1), poly.get(o + 2), poly.get(o + 3), x3, y3);
				int winding2 = winding(x3, y3, poly.get(0), poly.get(1), poly.get(2), poly.get(3));
				if (winding1 == lastWinding && winding2 == lastWinding) {
					poly.add(x3);
					poly.add(y3);
					polyIndices.add(idx3);
					merged = true;
				}
			}

			// otherwise make this triangle
			// the new base
			if (!merged) {
				if (poly.size > 0) {
					convexPolygons.add(poly);
					convexPolygonsIndices.add(polyIndices);
				}
				poly = polygonPool.obtain();
				poly.clear();
				poly.add(x1);
				poly.add(y1);
				poly.add(x2);
				poly.add(y2);
				poly.add(x3);
				poly.add(y3);
				polyIndices = polygonIndicesPool.obtain();
				polyIndices.clear();
				polyIndices.add(idx1);
				polyIndices.add(idx2);
				polyIndices.add(idx3);
				lastWinding = winding(x1, y1, x2, y2, x3, y3);
				fanBaseIndex = idx1;
			}
		}

		if (poly.size > 0) {
			convexPolygons.add(poly);
			convexPolygonsIndices.add(polyIndices);
		}

		// go through the list of polygons and try
		// to merge the remaining triangles with
		// the found triangle fans
		for (int i = 0, n = convexPolygons.size; i < n; i++) {
			polyIndices = convexPolygonsIndices.get(i);
			if (polyIndices.size == 0) continue;
			int firstIndex = polyIndices.get(0);
			int lastIndex = polyIndices.get(polyIndices.size - 1);

			poly = convexPolygons.get(i);
			int o = poly.size - 4;
			float prevPrevX = poly.get(o);
			float prevPrevY = poly.get(o + 1);
			float prevX = poly.get(o + 2);
			float prevY = poly.get(o + 3);
			float firstX = poly.get(0);
			float firstY = poly.get(1);
			float secondX = poly.get(2);
			float secondY = poly.get(3);
			int winding = winding(prevPrevX, prevPrevY, prevX, prevY, firstX, firstY);

			for (int j = 0; j < n; j++) {
				if (j == i) continue;
				ShortArray otherIndices = convexPolygonsIndices.get(j);
				if (otherIndices.size != 3) continue;
				int otherFirstIndex = otherIndices.get(0);
				int otherSecondIndex = otherIndices.get(1);
				int otherLastIndex = otherIndices.get(2);

				FloatArray otherPoly = convexPolygons.get(j);
				float x3 = otherPoly.get(otherPoly.size - 2);
				float y3 = otherPoly.get(otherPoly.size - 1);

				if (otherFirstIndex != firstIndex || otherSecondIndex != lastIndex) continue;
				int winding1 = winding(prevPrevX, prevPrevY, prevX, prevY, x3, y3);
				int winding2 = winding(x3, y3, firstX, firstY, secondX, secondY);
				if (winding1 == winding && winding2 == winding) {
					otherPoly.clear();
					otherIndices.clear();
					poly.add(x3);
					poly.add(y3);
					polyIndices.add(otherLastIndex);
					prevPrevX = prevX;
					prevPrevY = prevY;
					prevX = x3;
					prevY = y3;
					j = 0;
				}
			}
		}

		// Remove empty polygons that resulted from the
		// merge step above
		Iterator<FloatArray> polyIter = convexPolygons.iterator();
		while (polyIter.hasNext()) {
			poly = polyIter.next();
			if (poly.size == 0) {
				polyIter.remove();
				polygonPool.free(poly);
			}
		}

		return convexPolygons;
	}

	private int classifyVertex (int index) {
		short[] indices = this.indices;
		int previous = indices[previousIndex(index)] * 2;
		int current = indices[index] * 2;
		int next = indices[nextIndex(index)] * 2;
		float[] vertices = this.vertices;
		return computeSpannedAreaSign(vertices[previous], vertices[previous + 1], vertices[current], vertices[current + 1],
			vertices[next], vertices[next + 1]);
	}

	private int findEarTip () {
		int vertexCount = this.vertexCount;
		for (int i = 0; i < vertexCount; i++)
			if (isEarTip(i)) return i;

		int[] vertexTypes = this.vertexTypes.items;
		for (int i = 0; i < vertexCount; i++)
			if (vertexTypes[i] != CONCAVE) return i;
		return 0;
	}

	private boolean isEarTip (int earTipIndex) {
		int[] vertexTypes = this.vertexTypes.items;
		if (vertexTypes[earTipIndex] == CONCAVE) return false;

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
			if (vertexTypes[i] != CONVEX) {
				int v = indices[i] * 2;
				float vx = vertices[v];
				float vy = vertices[v + 1];
				if (computeSpannedAreaSign(p3x, p3y, p1x, p1y, vx, vy) >= 0) {
					if (computeSpannedAreaSign(p1x, p1y, p2x, p2y, vx, vy) >= 0) {
						if (computeSpannedAreaSign(p2x, p2y, p3x, p3y, vx, vy) >= 0) return false;
					}
				}
			}
		}
		return true;
	}

	private void cutEarTip (int earTipIndex) {
		short[] indices = this.indices;
		ShortArray triangles = this.triangles;

		short idx1 = indices[previousIndex(earTipIndex)];
		short idx2 = indices[earTipIndex];
		short idx3 = indices[nextIndex(earTipIndex)];
		triangles.add(idx1);
		triangles.add(idx2);
		triangles.add(idx3);

		indicesArray.removeIndex(earTipIndex);
		vertexTypes.removeIndex(earTipIndex);
		vertexCount--;
	}

	private int previousIndex (int index) {
		return (index == 0 ? vertexCount : index) - 1;
	}

	private int nextIndex (int index) {
		return (index + 1) % vertexCount;
	}

	static private int computeSpannedAreaSign (float p1x, float p1y, float p2x, float p2y, float p3x, float p3y) {
		float area = p1x * (p3y - p2y);
		area += p2x * (p1y - p3y);
		area += p3x * (p2y - p1y);
		return (int)Math.signum(area);
	}

	public static int winding (float v1x, float v1y, float v2x, float v2y, float v3x, float v3y) {
		float vx = v2x - v1x;
		float vy = v2y - v1y;
		return v3x * vy - v3y * vx + vx * v1y - v1x * vy >= 0 ? 1 : -1;
	}
}
