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

#ifndef Spine_SkeletonClipping_h
#define Spine_SkeletonClipping_h

#include <spine/Array.h>
#include <spine/Triangulator.h>

namespace spine {
	class Slot;
	class Skeleton;
	class ClippingAttachment;

	class SP_API SkeletonClipping : public SpineObject {
	public:
		SkeletonClipping();

		size_t clipStart(Skeleton &skeleton, Slot &slot, ClippingAttachment *clip);

		void clipEnd(Slot &slot);

		void clipEnd();

		bool clipTriangles(float *vertices, unsigned short *triangles, size_t trianglesLength);

		bool clipTriangles(float *vertices, unsigned short *triangles, size_t trianglesLength, float *uvs, size_t stride);

		bool clipTriangles(Array<float> &vertices, Array<unsigned short> &triangles, Array<float> &uvs, size_t stride);

		bool isClipping();

		Array<float> &getClippedVertices();

		Array<unsigned short> &getClippedTriangles();

		Array<float> &getClippedUVs();

	private:
		Triangulator _triangulator;
		Array<float> _clippingPolygon;
		Array<Array<float> *> _clippingPolygons;
		Array<float> _clipOutput;
		Array<float> _clippedVertices;
		Array<unsigned short> _clippedTriangles;
		Array<float> _clippedUVs;
		Array<float> _inverseVertices;
		Array<float> _scratch;
		ClippingAttachment *_clipAttachment;
		bool _inverse;

		/** Clips the input triangle against the convex, clockwise clipping area. If the triangle lies entirely within the clipping
		  * area, false is returned. The clipping area must duplicate the first vertex at the end of the vertices list. */
		bool clip(float x1, float y1, float x2, float y2, float x3, float y3, Array<float> *polygon);

		void clipInverse(float x1, float y1, float x2, float y2, float x3, float y3, Array<float> *polygon);

		static bool makeClockwise(Array<float> &polygon);

		void makeConvex(Array<float> &polygon);
	};
}

#endif /* Spine_SkeletonClipping_h */
