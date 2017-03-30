
package com.esotericsoftware.spine.utils;

import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;
import com.badlogic.gdx.utils.IntArray;
import com.badlogic.gdx.utils.ShortArray;

public class ConvexDecomposer {
	static private final int CONCAVE = -1;
	static private final int TANGENTIAL = 0;
	static private final int CONVEX = 1;

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

		// A polygon with n vertices has a triangulation of n-2 triangles.
		ShortArray triangles = this.triangles;
		triangles.clear();
		triangles.ensureCapacity(Math.max(0, vertexCount - 2) * 4);

		while (this.vertexCount > 3) {
			int earTipIndex = findEarTip();
			System.out.println("tip index: " + earTipIndex);
			cutEarTip(earTipIndex);

			// The type of the two vertices adjacent to the clipped vertex may have changed.
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

		Array<FloatArray> polyResult = new Array<FloatArray>();
		Array<ShortArray> polyIndicesResult = new Array<ShortArray>();

		ShortArray polyIndices = new ShortArray();
		FloatArray poly = new FloatArray();
		int idx1 = triangles.get(0);
		polyIndices.add(idx1);
		idx1 <<= 1;
		int idx2 = triangles.get(1);
		polyIndices.add(idx2);
		idx2 <<= 1;
		int idx3 = triangles.get(2);
		polyIndices.add(idx3);
		idx3 <<= 1;
		System.out.println("Triangle: " + idx1 / 2 + ", " + idx2 / 2 + ", " + idx3 / 2);
		
		float x1 = polygon.get(idx1);
		float y1 = polygon.get(idx1 + 1);
		float x2 = polygon.get(idx2);
		float y2 = polygon.get(idx2 + 1);
		float x3 = polygon.get(idx3);
		float y3 = polygon.get(idx3 + 1);
		
		poly.add(x1);
		poly.add(y1);
		poly.add(x2);
		poly.add(y2);
		poly.add(x3);
		poly.add(y3);
		int lastWinding = winding(x1, y1, x2, y2, x3, y3);
		int fanBaseIndex = idx1 >> 1;		

		for (int i = 3, n = triangles.size; i < n; i += 3) {
			idx1 = triangles.get(i);
			idx2 = triangles.get(i + 1);
			idx3 = triangles.get(i + 2);
			System.out.println("Triangle: " + idx1 + ", " + idx2 + ", " + idx3);

			x1 = polygon.get(idx1 * 2);
			y1 = polygon.get(idx1 * 2 + 1);
			x2 = polygon.get(idx2 * 2);
			y2 = polygon.get(idx2 * 2 + 1);
			x3 = polygon.get(idx3 * 2);
			y3 = polygon.get(idx3 * 2 + 1);

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
				polyResult.add(poly);
				polyIndicesResult.add(polyIndices);
				poly = new FloatArray();
				poly.add(x1);
				poly.add(y1);
				poly.add(x2);
				poly.add(y2);
				poly.add(x3);
				poly.add(y3);
				polyIndices = new ShortArray();
				polyIndices.add(idx1);
				polyIndices.add(idx2);
				polyIndices.add(idx3);
				lastWinding = winding(x1, y1, x2, y2, x3, y3);
				fanBaseIndex = idx1;
			}
		}

		if (poly.size > 0) {
			polyResult.add(poly);
			polyIndicesResult.add(polyIndices);
		}

		for (ShortArray pIndices : polyIndicesResult) {
			System.out.println("Poly: " + pIndices.toString(","));
		}

		return polyResult;
	}

	public static int winding (float v1x, float v1y, float v2x, float v2y, float v3x, float v3y) {
		float vx = v2x - v1x;
		float vy = v2y - v1y;
		return v3x * vy - v3y * vx + vx * v1y - v1x * vy >= 0 ? 1 : -1;
	}

	/** @return {@link #CONCAVE}, {@link #TANGENTIAL} or {@link #CONVEX} */
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

		// Desperate mode: if no vertex is an ear tip, we are dealing with a degenerate polygon (e.g. nearly collinear).
		// Note that the input was not necessarily degenerate, but we could have made it so by clipping some valid ears.

		// Idea taken from Martin Held, "FIST: Fast industrial-strength triangulation of polygons", Algorithmica (1998),
		// http://citeseerx.ist.psu.edu/viewdoc/summary?doi=10.1.1.115.291

		// Return a convex or tangential vertex if one exists.
		int[] vertexTypes = this.vertexTypes.items;
		for (int i = 0; i < vertexCount; i++)
			if (vertexTypes[i] != CONCAVE) return i;
		return 0; // If all vertices are concave, just return the first one.
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

		// Check if any point is inside the triangle formed by previous, current and next vertices.
		// Only consider vertices that are not part of this triangle, or else we'll always find one inside.
		for (int i = nextIndex(nextIndex); i != previousIndex; i = nextIndex(i)) {
			// Concave vertices can obviously be inside the candidate ear, but so can tangential vertices
			// if they coincide with one of the triangle's vertices.
			if (vertexTypes[i] != CONVEX) {
				int v = indices[i] * 2;
				float vx = vertices[v];
				float vy = vertices[v + 1];
				// Because the polygon has clockwise winding order, the area sign will be positive if the point is strictly inside.
				// It will be 0 on the edge, which we want to include as well.
				// note: check the edge defined by p1->p3 first since this fails _far_ more then the other 2 checks.
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
}
