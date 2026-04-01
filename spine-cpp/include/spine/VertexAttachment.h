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

#ifndef Spine_VertexAttachment_h
#define Spine_VertexAttachment_h

#include <spine/Attachment.h>

#include <spine/Array.h>

namespace spine {
	class Slot;
	class Skeleton;

	/// An attachment with vertices that are transformed by one or more bones and can be deformed by
	/// SlotPose::getDeform().
	class SP_API VertexAttachment : public Attachment {
		friend class SkeletonBinary;

		friend class SkeletonJson;

		friend class DeformTimeline;

		RTTI_DECL

	public:
		explicit VertexAttachment(const String &name);

		virtual ~VertexAttachment();


		/// Transforms the attachment's local vertices to world coordinates. If SlotPose::getDeform() is not empty,
		/// it is used to deform the vertices.
		///
		/// See https://esotericsoftware.com/spine-runtime-skeletons#World-transforms World transforms in the Spine
		/// Runtimes Guide.
		/// @param start The index of the first vertices value to transform. Each vertex has 2 values, x and y.
		/// @param count The number of world vertex values to output. Must be <= WorldVerticesLength - start.
		/// @param worldVertices The output world vertices. Must have a length >= offset + count * stride / 2.
		/// @param offset The worldVertices index to begin writing values.
		/// @param stride The number of worldVertices entries between the value pairs written.
		virtual void computeWorldVertices(Skeleton &skeleton, Slot &slot, size_t start, size_t count, float *worldVertices, size_t offset,
										  size_t stride = 2);

		virtual void computeWorldVertices(Skeleton &skeleton, Slot &slot, size_t start, size_t count, Array<float> &worldVertices, size_t offset,
										  size_t stride = 2);

		/// Gets a unique ID for this attachment.
		int getId();

		/// The bones that affect the vertices. The entries are, for each vertex, the number of bones affecting the
		/// vertex followed by that many bone indices, which is Skeleton::getBones() index. Empty if this attachment
		/// has no weights.
		Array<int> &getBones();

		void setBones(Array<int> &bones);

		/// The vertex positions in the bone's coordinate system. For a non-weighted attachment, the values are x,y pairs
		/// for each vertex. For a weighted attachment, the values are x,y,weight triplets for each bone affecting each
		/// vertex.
		Array<float> &getVertices();

		void setVertices(Array<float> &vertices);

		size_t getWorldVerticesLength();

		void setWorldVerticesLength(size_t inValue);

		Attachment *getTimelineAttachment();

		void setTimelineAttachment(Attachment *attachment);

		void copyTo(VertexAttachment &other);

	protected:
		Array<int> _bones;
		Array<float> _vertices;
		size_t _worldVerticesLength;

	private:
		const int _id;

		static int getNextID();
	};
}

#endif /* Spine_VertexAttachment_h */
