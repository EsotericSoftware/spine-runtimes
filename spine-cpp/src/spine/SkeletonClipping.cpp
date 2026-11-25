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

#include <spine/SkeletonClipping.h>

#include <spine/ClippingAttachment.h>
#include <spine/Slot.h>

#include <cstring>

using namespace spine;

SkeletonClipping::SkeletonClipping() : _clipAttachment(NULL), _inverse(false) {
	_clippingPolygons.ensureCapacity(1);
}

size_t SkeletonClipping::clipStart(Skeleton &skeleton, Slot &slot, ClippingAttachment *clip) {
	if (_clipAttachment != NULL) return 0;

	int n = (int) clip->getWorldVerticesLength();
	if (n < 6) return 0;
	_clipAttachment = clip;
	_inverse = clip->getInverse();
	_clippingPolygons.clear();

	clip->computeWorldVertices(skeleton, slot, 0, n, _clippingPolygon.setSize(n, 0).buffer(), 0, 2);
	bool convex = makeClockwise(_clippingPolygon);
	if (convex || _inverse || clip->getConvex()) {
		if (!convex) makeConvex(_clippingPolygon);
		_clippingPolygon.add(_clippingPolygon[0]);
		_clippingPolygon.add(_clippingPolygon[1]);
		_clippingPolygons.add(&_clippingPolygon);
	} else {
		Array<Array<float> *> &polygons = _triangulator.decompose(_clippingPolygon, _triangulator.triangulate(_clippingPolygon));
		for (size_t i = 0, n2 = polygons.size(); i < n2; ++i) _clippingPolygons.add(polygons[i]);
	}

	return _clippingPolygons.size();
}

void SkeletonClipping::clipEnd(Slot &slot) {
	if (_clipAttachment != NULL && _clipAttachment->_endSlot == &slot._data) {
		clipEnd();
	}
}

void SkeletonClipping::clipEnd() {
	if (_clipAttachment == NULL) return;
	_clipAttachment = NULL;
	_clippingPolygons.clear();
}

bool SkeletonClipping::clipTriangles(float *vertices, unsigned short *triangles, size_t trianglesLength) {
	Array<float> &clippedVertices = _clippedVertices;
	clippedVertices.clear();
	Array<unsigned short> &clippedTriangles = _clippedTriangles;
	clippedTriangles.clear();
	unsigned short index = 0;

	if (_inverse) {
		Array<float> *polygon = _clippingPolygons[0];
		for (size_t i = 0; i < trianglesLength; i += 3) {
			int t = triangles[i] << 1;
			float x1 = vertices[t], y1 = vertices[t + 1];
			t = triangles[i + 1] << 1;
			float x2 = vertices[t], y2 = vertices[t + 1];
			t = triangles[i + 2] << 1;
			float x3 = vertices[t], y3 = vertices[t + 1];
			clipInverse(x1, y1, x2, y2, x3, y3, polygon);

			float *iv = _inverseVertices.buffer();
			for (size_t offset = 0, nn = _inverseVertices.size(); offset < nn;) {
				int polygonSize = (int) iv[offset++];
				int vertexCount = polygonSize >> 1;
				size_t s = clippedVertices.size();

				float *cv = clippedVertices.setSize(s + polygonSize, 0).buffer();
				memcpy(cv + s, iv + offset, sizeof(float) * polygonSize);

				s = clippedTriangles.size();
				unsigned short *ct = clippedTriangles.setSize(s + 3 * (vertexCount - 2), 0).buffer();
				for (int ii = 1; ii < vertexCount - 1; ii++, s += 3) {
					ct[s] = index;
					ct[s + 1] = (unsigned short) (index + ii);
					ct[s + 2] = (unsigned short) (index + ii + 1);
				}
				index += (unsigned short) vertexCount;
				offset += polygonSize;
			}
		}
		return true;
	}

	Array<float> &clipOutput = _clipOutput;
	Array<float> **polygons = _clippingPolygons.buffer();
	int polygonsCount = (int) _clippingPolygons.size();
	bool clipped = false;
	for (size_t i = 0; i < trianglesLength; i += 3) {
		int t = triangles[i] << 1;
		float x1 = vertices[t], y1 = vertices[t + 1];
		t = triangles[i + 1] << 1;
		float x2 = vertices[t], y2 = vertices[t + 1];
		t = triangles[i + 2] << 1;
		float x3 = vertices[t], y3 = vertices[t + 1];
		for (int p = 0; p < polygonsCount; p++) {
			size_t s = clippedVertices.size();
			if (clip(x1, y1, x2, y2, x3, y3, polygons[p])) {
				int clipOutputLength = (int) clipOutput.size();
				if (clipOutputLength == 0) continue;
				clipped = true;
				int clipOutputCount = clipOutputLength >> 1;

				float *cv = clippedVertices.setSize(s + clipOutputLength, 0).buffer();
				memcpy(cv + s, clipOutput.buffer(), sizeof(float) * clipOutputLength);

				s = clippedTriangles.size();
				unsigned short *ct = clippedTriangles.setSize(s + 3 * (clipOutputCount - 2), 0).buffer();
				for (int ii = 1, nn = clipOutputCount - 1; ii < nn; ii++, s += 3) {
					ct[s] = index;
					ct[s + 1] = (unsigned short) (index + ii);
					ct[s + 2] = (unsigned short) (index + ii + 1);
				}
				index += (unsigned short) clipOutputCount;
			} else {
				float *cv = clippedVertices.setSize(s + 6, 0).buffer();
				cv[s] = x1;
				cv[s + 1] = y1;
				cv[s + 2] = x2;
				cv[s + 3] = y2;
				cv[s + 4] = x3;
				cv[s + 5] = y3;

				s = clippedTriangles.size();
				unsigned short *ct = clippedTriangles.setSize(s + 3, 0).buffer();
				ct[s] = index;
				ct[s + 1] = (unsigned short) (index + 1);
				ct[s + 2] = (unsigned short) (index + 2);
				index += 3;
				break;
			}
		}
	}
	return clipped;
}

bool SkeletonClipping::clipTriangles(Array<float> &vertices, Array<unsigned short> &triangles, Array<float> &uvs, size_t stride) {
	return clipTriangles(vertices.buffer(), triangles.buffer(), triangles.size(), uvs.buffer(), stride);
}

bool SkeletonClipping::clipTriangles(float *vertices, unsigned short *triangles, size_t trianglesLength, float *uvs, size_t stride) {
	Array<float> &clippedVertices = _clippedVertices;
	clippedVertices.clear();
	Array<unsigned short> &clippedTriangles = _clippedTriangles;
	clippedTriangles.clear();
	_clippedUVs.clear();
	unsigned short index = 0;

	if (_inverse) {
		Array<float> *polygon = _clippingPolygons[0];
		for (size_t i = 0; i < trianglesLength; i += 3) {
			int t0 = triangles[i], t1 = triangles[i + 1], t2 = triangles[i + 2];
			float x1 = vertices[t0 * stride], y1 = vertices[t0 * stride + 1];
			float x2 = vertices[t1 * stride], y2 = vertices[t1 * stride + 1];
			float x3 = vertices[t2 * stride], y3 = vertices[t2 * stride + 1];
			clipInverse(x1, y1, x2, y2, x3, y3, polygon);
			size_t nn = _inverseVertices.size();
			if (nn == 0) continue;

			float u1 = uvs[t0 << 1], v1 = uvs[(t0 << 1) + 1];
			float u2 = uvs[t1 << 1], v2 = uvs[(t1 << 1) + 1];
			float u3 = uvs[t2 << 1], v3 = uvs[(t2 << 1) + 1];
			float d0 = y2 - y3, d1 = x3 - x2, d2 = x1 - x3, d4 = y3 - y1, d = 1 / (d0 * d2 + d1 * (y1 - y3));
			float *iv = _inverseVertices.buffer();
			for (size_t offset = 0; offset < nn;) {
				int polygonSize = (int) iv[offset++];
				int vertexCount = polygonSize >> 1;

				size_t s = clippedVertices.size();
				float *cv = clippedVertices.setSize(s + polygonSize, 0).buffer();
				float *cu = _clippedUVs.setSize(s + polygonSize, 0).buffer();
				for (int ii = 0; ii < polygonSize; ii += 2, s += 2) {
					float x = iv[offset + ii], y = iv[offset + ii + 1];
					cv[s] = x;
					cv[s + 1] = y;
					float c0 = x - x3, c1 = y - y3, a = (d0 * c0 + d1 * c1) * d, b = (d4 * c0 + d2 * c1) * d, c = 1 - a - b;
					cu[s] = u1 * a + u2 * b + u3 * c;
					cu[s + 1] = v1 * a + v2 * b + v3 * c;
				}

				s = clippedTriangles.size();
				unsigned short *ct = clippedTriangles.setSize(s + 3 * (vertexCount - 2), 0).buffer();
				for (int ii = 1; ii < vertexCount - 1; ii++, s += 3) {
					ct[s] = index;
					ct[s + 1] = (unsigned short) (index + ii);
					ct[s + 2] = (unsigned short) (index + ii + 1);
				}
				index += (unsigned short) vertexCount;
				offset += polygonSize;
			}
		}
		return true;
	}

	Array<float> &clipOutput = _clipOutput;
	Array<float> **polygons = _clippingPolygons.buffer();
	int polygonsCount = (int) _clippingPolygons.size();
	bool clipped = false;
	for (size_t i = 0; i < trianglesLength; i += 3) {
		int t = triangles[i];
		float x1 = vertices[t * stride], y1 = vertices[t * stride + 1];
		float u1 = uvs[t << 1], v1 = uvs[(t << 1) + 1];
		t = triangles[i + 1];
		float x2 = vertices[t * stride], y2 = vertices[t * stride + 1];
		float u2 = uvs[t << 1], v2 = uvs[(t << 1) + 1];
		t = triangles[i + 2];
		float x3 = vertices[t * stride], y3 = vertices[t * stride + 1];
		float u3 = uvs[t << 1], v3 = uvs[(t << 1) + 1];
		float d0 = 0, d1 = 0, d2 = 0, d4 = 0, d = 0;
		for (int p = 0; p < polygonsCount; p++) {
			size_t s = clippedVertices.size();
			if (clip(x1, y1, x2, y2, x3, y3, polygons[p])) {
				int clipOutputLength = (int) clipOutput.size();
				if (clipOutputLength == 0) continue;
				clipped = true;
				int clipOutputCount = clipOutputLength >> 1;
				if (d == 0) {
					d0 = y2 - y3;
					d1 = x3 - x2;
					d2 = x1 - x3;
					d4 = y3 - y1;
					d = 1 / (d0 * d2 - d1 * d4);
				}

				float *cv = clippedVertices.setSize(s + clipOutputCount * 2, 0).buffer();
				float *cu = _clippedUVs.setSize(s + clipOutputCount * 2, 0).buffer();
				for (int ii = 0; ii < clipOutputLength; ii += 2, s += 2) {
					float x = clipOutput[ii], y = clipOutput[ii + 1];
					cv[s] = x;
					cv[s + 1] = y;
					float c0 = x - x3, c1 = y - y3, a = (d0 * c0 + d1 * c1) * d, b = (d4 * c0 + d2 * c1) * d, c = 1 - a - b;
					cu[s] = u1 * a + u2 * b + u3 * c;
					cu[s + 1] = v1 * a + v2 * b + v3 * c;
				}

				s = clippedTriangles.size();
				unsigned short *ct = clippedTriangles.setSize(s + 3 * (clipOutputCount - 2), 0).buffer();
				clipOutputCount--;
				for (int ii = 1; ii < clipOutputCount; ii++, s += 3) {
					ct[s] = index;
					ct[s + 1] = (unsigned short) (index + ii);
					ct[s + 2] = (unsigned short) (index + ii + 1);
				}
				index += (unsigned short) (clipOutputCount + 1);
			} else {
				float *cv = clippedVertices.setSize(s + 6, 0).buffer();
				cv[s] = x1;
				cv[s + 1] = y1;
				cv[s + 2] = x2;
				cv[s + 3] = y2;
				cv[s + 4] = x3;
				cv[s + 5] = y3;

				float *cu = _clippedUVs.setSize(s + 6, 0).buffer();
				cu[s] = u1;
				cu[s + 1] = v1;
				cu[s + 2] = u2;
				cu[s + 3] = v2;
				cu[s + 4] = u3;
				cu[s + 5] = v3;

				s = clippedTriangles.size();
				unsigned short *ct = clippedTriangles.setSize(s + 3, 0).buffer();
				ct[s] = index;
				ct[s + 1] = (unsigned short) (index + 1);
				ct[s + 2] = (unsigned short) (index + 2);
				index += 3;
				break;
			}
		}
	}
	return clipped;
}

bool SkeletonClipping::isClipping() {
	return _clipAttachment != NULL;
}

Array<float> &SkeletonClipping::getClippedVertices() {
	return _clippedVertices;
}

Array<unsigned short> &SkeletonClipping::getClippedTriangles() {
	return _clippedTriangles;
}

Array<float> &SkeletonClipping::getClippedUVs() {
	return _clippedUVs;
}

bool SkeletonClipping::clip(float x1, float y1, float x2, float y2, float x3, float y3, Array<float> *polygon) {
	Array<float> &originalOutput = _clipOutput;
	bool clipped = false;

	Array<float> *input;
	Array<float> *output;
	if (polygon->size() % 4 >= 2) {
		input = &_clipOutput;
		output = &_scratch;
	} else {
		input = &_scratch;
		output = &_clipOutput;
	}

	float *v = polygon->buffer();
	float *iv = input->setSize(8, 0).buffer();
	iv[0] = x1;
	iv[1] = y1;
	iv[2] = x2;
	iv[3] = y2;
	iv[4] = x3;
	iv[5] = y3;
	iv[6] = x1;
	iv[7] = y1;
	output->clear();

	int last = (int) polygon->size() - 4;
	for (int i = 0;; i += 2) {
		float edgeX = v[i], edgeY = v[i + 1], ex = edgeX - v[i + 2], ey = edgeY - v[i + 3];
		size_t outputStart = output->size();
		iv = input->buffer();
		x1 = iv[0];
		y1 = iv[1];
		float s1 = ey * (edgeX - x1) - ex * (edgeY - y1);
		for (size_t ii = 2, nn = input->size() - 2; ii <= nn; ii += 2) {
			x2 = iv[ii];
			y2 = iv[ii + 1];
			float s2 = ey * (edgeX - x2) - ex * (edgeY - y2);
			if (s1 > 0) {
				if (s2 > 0) {
					output->add(x2);
					output->add(y2);
				} else {
					float ix = x2 - x1, iy = y2 - y1, t = s1 / (ix * ey - iy * ex);
					if (t >= 0 && t <= 1) {
						output->add(x1 + ix * t);
						output->add(y1 + iy * t);
						clipped = true;
					} else {
						output->add(x2);
						output->add(y2);
					}
				}
			} else if (s2 > 0) {
				float ix = x2 - x1, iy = y2 - y1, t = s1 / (ix * ey - iy * ex);
				if (t >= 0 && t <= 1) {
					output->add(x1 + ix * t);
					output->add(y1 + iy * t);
					output->add(x2);
					output->add(y2);
					clipped = true;
				} else {
					output->add(x2);
					output->add(y2);
				}
			} else {
				clipped = true;
			}
			x1 = x2;
			y1 = y2;
			s1 = s2;
		}
		if (outputStart == output->size()) {
			originalOutput.clear();
			return true;
		}

		output->add((*output)[0]);
		output->add((*output)[1]);

		if (i == last) break;
		Array<float> *temp = output;
		output = input;
		output->clear();
		input = temp;
	}

	if (&originalOutput != output) {
		originalOutput.clear();
		for (size_t i = 0, n = output->size() - 2; i < n; ++i) originalOutput.add((*output)[i]);
	} else {
		originalOutput.setSize(originalOutput.size() - 2, 0);
	}

	return clipped;
}

void SkeletonClipping::clipInverse(float x1, float y1, float x2, float y2, float x3, float y3, Array<float> *polygon) {
	_inverseVertices.clear();
	_inverseVertices.ensureCapacity(polygon->size() * 3);
	int last = (int) polygon->size() - 4;

	Array<float> *input;
	Array<float> *output;
	if (polygon->size() % 4 >= 2) {
		input = &_clipOutput;
		output = &_scratch;
	} else {
		input = &_scratch;
		output = &_clipOutput;
	}

	float *v = polygon->buffer();
	float *iv = input->setSize(8, 0).buffer();
	iv[0] = x1;
	iv[1] = y1;
	iv[2] = x2;
	iv[3] = y2;
	iv[4] = x3;
	iv[5] = y3;
	iv[6] = x1;
	iv[7] = y1;
	output->clear();

	for (int i = 0;; i += 2) {
		float edgeX = v[i], edgeY = v[i + 1], ex = edgeX - v[i + 2], ey = edgeY - v[i + 3];
		size_t outputStart = output->size(), fragmentStart = _inverseVertices.size();
		_inverseVertices.add(0);
		iv = input->buffer();
		x1 = iv[0];
		y1 = iv[1];
		float s1 = ey * (edgeX - x1) - ex * (edgeY - y1);
		for (size_t ii = 2, nn = input->size() - 2; ii <= nn; ii += 2) {
			x2 = iv[ii];
			y2 = iv[ii + 1];
			float s2 = ey * (edgeX - x2) - ex * (edgeY - y2);
			if (s1 > 0) {
				if (s2 > 0) {
					output->add(x2);
					output->add(y2);
				} else {
					float ix = x2 - x1, iy = y2 - y1, t = s1 / (ix * ey - iy * ex);
					if (t >= 0 && t <= 1) {
						float cx = x1 + ix * t, cy = y1 + iy * t;
						output->add(cx);
						output->add(cy);
						_inverseVertices.add(cx);
						_inverseVertices.add(cy);
						_inverseVertices.add(x2);
						_inverseVertices.add(y2);
					} else {
						output->add(x2);
						output->add(y2);
					}
				}
			} else if (s2 > 0) {
				float ix = x2 - x1, iy = y2 - y1, t = s1 / (ix * ey - iy * ex);
				if (t >= 0 && t <= 1) {
					float cx = x1 + ix * t, cy = y1 + iy * t;
					_inverseVertices.add(cx);
					_inverseVertices.add(cy);
					output->add(cx);
					output->add(cy);
					output->add(x2);
					output->add(y2);
				} else {
					output->add(x2);
					output->add(y2);
				}
			} else {
				_inverseVertices.add(x2);
				_inverseVertices.add(y2);
			}
			x1 = x2;
			y1 = y2;
			s1 = s2;
		}

		int fragmentSize = (int) _inverseVertices.size() - (int) fragmentStart - 1;
		if (fragmentSize >= 6)
			_inverseVertices[fragmentStart] = (float) fragmentSize;
		else
			_inverseVertices.setSize(fragmentStart, 0);

		if (outputStart == output->size()) break;

		output->add((*output)[0]);
		output->add((*output)[1]);

		if (i == last) break;
		Array<float> *temp = output;
		output = input;
		output->clear();
		input = temp;
	}
}

bool SkeletonClipping::makeClockwise(Array<float> &polygon) {
	float *v = polygon.buffer();
	int n = (int) polygon.size();
	bool noCW = true, noCCW = true;
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

void SkeletonClipping::makeConvex(Array<float> &polygon) {
	int n = (int) polygon.size();
	float *v = polygon.buffer();
	float *sorted = _clipOutput.setSize(n, 0).buffer();
	for (int i = 0; i < n; i++) sorted[i] = v[i];
	for (int i = 2; i < n; i += 2) {
		float x = sorted[i], y = sorted[i + 1];
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
	polygon.setSize(s - 2, 0);
}
