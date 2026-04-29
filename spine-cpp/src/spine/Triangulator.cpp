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

#include <spine/Triangulator.h>

#include <spine/MathUtil.h>

using namespace spine;

Triangulator::~Triangulator() {
	ArrayUtils::deleteElements(_convexPolygons);
	ArrayUtils::deleteElements(_convexPolygonsIndices);
}

Array<int> &Triangulator::triangulate(Array<float> &verticesArray) {
	float *vertices = verticesArray.buffer();
	int vertexCount = (int) (verticesArray.size() >> 1);

	Array<int> &indicesArray = _indices;
	indicesArray.clear();
	int *indices = indicesArray.setSize(vertexCount, 0).buffer();
	for (int i = 0; i < vertexCount; i++) indices[i] = i;

	Array<bool> &isConcaveArray = _isConcaveArray;
	bool *isConcaveItems = isConcaveArray.setSize(vertexCount, false).buffer();
	for (int i = 0; i < vertexCount; i++) isConcaveItems[i] = isConcave(i, vertexCount, vertices, indices);

	Array<int> &triangles = _triangles;
	triangles.clear();
	triangles.ensureCapacity(MathUtil::max(0, vertexCount - 2) * 3);

	while (vertexCount > 3) {
		int previous = vertexCount - 1, i = 0, next = 1;
		while (true) {
			if (!isConcaveItems[i]) {
				int p1 = indices[previous] << 1, p2 = indices[i] << 1, p3 = indices[next] << 1;
				float p1x = vertices[p1], p1y = vertices[p1 + 1];
				float p2x = vertices[p2], p2y = vertices[p2 + 1];
				float p3x = vertices[p3], p3y = vertices[p3 + 1];
				bool ear = true;
				for (int ii = next + 1 < vertexCount ? next + 1 : 0; ii != previous;) {
					if (isConcaveItems[ii]) {
						int v = indices[ii] << 1;
						float vx = vertices[v], vy = vertices[v + 1];
						if (positiveArea(p3x, p3y, p1x, p1y, vx, vy) && positiveArea(p1x, p1y, p2x, p2y, vx, vy) &&
							positiveArea(p2x, p2y, p3x, p3y, vx, vy)) {
							ear = false;
							break;
						}
					}
					if (++ii == vertexCount) ii = 0;
				}
				if (ear) break;
			}

			if (next == 0) {
				do {
					if (!isConcaveItems[i]) break;
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

		triangles.add(indices[previous]);
		triangles.add(indices[i]);
		triangles.add(indices[next]);
		indicesArray.removeAt(i);
		isConcaveArray.removeAt(i);
		vertexCount--;
		indices = indicesArray.buffer();
		isConcaveItems = isConcaveArray.buffer();

		int previousIndex = i > 0 ? i - 1 : vertexCount - 1;
		int nextIndex = i < vertexCount ? i : 0;
		isConcaveItems[previousIndex] = isConcave(previousIndex, vertexCount, vertices, indices);
		isConcaveItems[nextIndex] = isConcave(nextIndex, vertexCount, vertices, indices);
	}
	if (vertexCount == 3) {
		triangles.add(indices[2]);
		triangles.add(indices[0]);
		triangles.add(indices[1]);
	}
	return triangles;
}

Array<Array<float> *> &Triangulator::decompose(Array<float> &verticesArray, Array<int> &triangles) {
	float *vertices = verticesArray.buffer();

	Array<Array<float> *> &convexPolygons = _convexPolygons;
	for (size_t i = 0, n = convexPolygons.size(); i < n; ++i) _polygonPool.free(convexPolygons[i]);
	convexPolygons.clear();

	Array<Array<int> *> &convexPolygonsIndices = _convexPolygonsIndices;
	for (size_t i = 0, n = convexPolygonsIndices.size(); i < n; ++i) _polygonIndicesPool.free(convexPolygonsIndices[i]);
	convexPolygonsIndices.clear();

	Array<int> *polygonIndices = _polygonIndicesPool.obtain();
	polygonIndices->clear();

	Array<float> *polygon = _polygonPool.obtain();
	polygon->clear();

	int fanBaseIndex = -1, lastWinding = 0;
	int *trianglesItems = triangles.buffer();
	for (int i = 0, n = (int) triangles.size(); i < n; i += 3) {
		int t1 = trianglesItems[i] << 1, t2 = trianglesItems[i + 1] << 1, t3 = trianglesItems[i + 2] << 1;
		float x1 = vertices[t1], y1 = vertices[t1 + 1];
		float x2 = vertices[t2], y2 = vertices[t2 + 1];
		float x3 = vertices[t3], y3 = vertices[t3 + 1];

		if (fanBaseIndex == t1) {
			int o = (int) polygon->size() - 4;
			float *p = polygon->buffer();
			if (winding(p[o], p[o + 1], p[o + 2], p[o + 3], x3, y3) == lastWinding && winding(x3, y3, p[0], p[1], p[2], p[3]) == lastWinding) {
				polygon->add(x3);
				polygon->add(y3);
				polygonIndices->add(t3);
				continue;
			}
		}

		if (polygon->size() > 0) {
			convexPolygons.add(polygon);
			convexPolygonsIndices.add(polygonIndices);
			polygon = _polygonPool.obtain();
			polygonIndices = _polygonIndicesPool.obtain();
		}
		polygon->clear();
		polygon->add(x1);
		polygon->add(y1);
		polygon->add(x2);
		polygon->add(y2);
		polygon->add(x3);
		polygon->add(y3);
		polygonIndices->clear();
		polygonIndices->add(t1);
		polygonIndices->add(t2);
		polygonIndices->add(t3);
		lastWinding = winding(x1, y1, x2, y2, x3, y3);
		fanBaseIndex = t1;
	}

	if (polygon->size() > 0) {
		convexPolygons.add(polygon);
		convexPolygonsIndices.add(polygonIndices);
	}

	Array<int> **convexPolygonsIndicesItems = convexPolygonsIndices.buffer();
	Array<float> **convexPolygonsItems = convexPolygons.buffer();
	for (int i = 0, n = (int) convexPolygons.size(); i < n; i++) {
		polygonIndices = convexPolygonsIndicesItems[i];
		if (polygonIndices->size() == 0) continue;
		int firstIndex = (*polygonIndices)[0];
		int lastIndex = (*polygonIndices)[polygonIndices->size() - 1];

		polygon = convexPolygonsItems[i];
		int o = (int) polygon->size() - 4;
		float *p = polygon->buffer();
		float prevPrevX = p[o], prevPrevY = p[o + 1];
		float prevX = p[o + 2], prevY = p[o + 3];
		float firstX = p[0], firstY = p[1];
		float secondX = p[2], secondY = p[3];
		int polygonWinding = winding(prevPrevX, prevPrevY, prevX, prevY, firstX, firstY);

		for (int ii = 0; ii < n; ii++) {
			if (ii == i) continue;
			Array<int> *otherIndices = convexPolygonsIndicesItems[ii];
			if (otherIndices->size() != 3) continue;
			int otherFirstIndex = (*otherIndices)[0];
			int otherSecondIndex = (*otherIndices)[1];
			int otherLastIndex = (*otherIndices)[2];

			Array<float> *otherPoly = convexPolygonsItems[ii];
			float x3 = (*otherPoly)[otherPoly->size() - 2], y3 = (*otherPoly)[otherPoly->size() - 1];

			if (otherFirstIndex != firstIndex || otherSecondIndex != lastIndex) continue;
			if (winding(prevPrevX, prevPrevY, prevX, prevY, x3, y3) == polygonWinding &&
				winding(x3, y3, firstX, firstY, secondX, secondY) == polygonWinding) {
				otherPoly->clear();
				otherIndices->clear();
				polygon->add(x3);
				polygon->add(y3);
				polygonIndices->add(otherLastIndex);
				lastIndex = otherLastIndex;
				prevPrevX = prevX;
				prevPrevY = prevY;
				prevX = x3;
				prevY = y3;
				ii = -1;
			}
		}
	}

	for (int i = (int) convexPolygons.size() - 1; i >= 0; --i) {
		polygon = convexPolygonsItems[i];
		if (polygon->size() == 0) {
			convexPolygons.removeAt(i);
			_polygonPool.free(polygon);
			polygonIndices = convexPolygonsIndices[i];
			convexPolygonsIndices.removeAt(i);
			_polygonIndicesPool.free(polygonIndices);
		} else {
			polygon->add((*polygon)[0]);
			polygon->add((*polygon)[1]);
		}
	}

	return convexPolygons;
}

bool Triangulator::isConcave(int index, int vertexCount, const float *vertices, const int *indices) {
	int previous = indices[index > 0 ? index - 1 : vertexCount - 1] << 1;
	int current = indices[index] << 1;
	int next = indices[index + 1 < vertexCount ? index + 1 : 0] << 1;

	return !positiveArea(vertices[previous], vertices[previous + 1], vertices[current], vertices[current + 1], vertices[next], vertices[next + 1]);
}

bool Triangulator::positiveArea(float p1x, float p1y, float p2x, float p2y, float p3x, float p3y) {
	return p1x * (p3y - p2y) + p2x * (p1y - p3y) + p3x * (p2y - p1y) >= 0;
}

int Triangulator::winding(float p1x, float p1y, float p2x, float p2y, float p3x, float p3y) {
	return p1x * (p3y - p2y) + p2x * (p1y - p3y) + p3x * (p2y - p1y) >= 0 ? 1 : -1;
}
