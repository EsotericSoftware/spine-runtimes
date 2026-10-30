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

#ifndef Spine_Triangulator_h
#define Spine_Triangulator_h

#include <spine/Array.h>
#include <spine/Pool.h>

namespace spine {
	class SP_API Triangulator : public SpineObject {
	public:
		~Triangulator();

		Array<int> &triangulate(Array<float> &vertices);

		Array<Array<float> *> &decompose(Array<float> &vertices, Array<int> &triangles);

	private:
		Array<Array<float> *> _convexPolygons;
		Array<Array<int> *> _convexPolygonsIndices;

		Array<int> _indices;
		Array<bool> _isConcaveArray;
		Array<int> _triangles;

		Pool<Array<float>> _polygonPool;
		Pool<Array<int>> _polygonIndicesPool;

		static bool isConcave(int index, int vertexCount, const float *vertices, const int *indices);

		static bool positiveArea(float p1x, float p1y, float p2x, float p2y, float p3x, float p3y);

		static int winding(float p1x, float p1y, float p2x, float p2y, float p3x, float p3y);
	};
}

#endif /* Spine_Triangulator_h */
