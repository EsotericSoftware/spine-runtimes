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

#ifndef Spine_MeshAttachment_h
#define Spine_MeshAttachment_h

#include <spine/Array.h>
#include <spine/Color.h>
#include <spine/HasRendererObject.h>
#include <spine/Sequence.h>
#include <spine/TextureRegion.h>
#include <spine/VertexAttachment.h>

namespace spine {
	/// Attachment that displays a texture region using a mesh.
	class SP_API MeshAttachment : public VertexAttachment {
		friend class SkeletonBinary;
		friend class SkeletonJson;
		friend class AtlasAttachmentLoader;

		RTTI_DECL

	public:
		explicit MeshAttachment(const String &name, Sequence *sequence);

		virtual ~MeshAttachment();

		using VertexAttachment::computeWorldVertices;

		virtual void computeWorldVertices(Skeleton &skeleton, Slot &slot, size_t start, size_t count, float *worldVertices, size_t offset,
										  size_t stride = 2) override;

		Array<float> &getRegionUVs();
		void setRegionUVs(Array<float> &inValue);

		Array<unsigned short> &getTriangles();
		void setTriangles(Array<unsigned short> &inValue);

		int getHullLength();
		void setHullLength(int inValue);

		Sequence &getSequence();

		void updateSequence();

		const String &getPath();
		void setPath(const String &inValue);

		Color &getColor();

		/// The source mesh if this is a linked mesh, else NULL. A linked mesh shares the bones, vertices, regionUVs,
		/// triangles, hullLength, edges, width, and height with the source mesh, but may have a different name or path,
		/// and therefore a different texture region.
		MeshAttachment *getSourceMesh();
		void setSourceMesh(MeshAttachment *inValue);

		/// Vertex index pairs describing edges for controlling triangulation, or empty if nonessential data was not
		/// exported. Mesh triangles do not cross edges. Triangulation is not performed at runtime.
		Array<unsigned short> &getEdges();
		void setEdges(Array<unsigned short> &inValue);

		float getWidth();
		void setWidth(float inValue);

		float getHeight();
		void setHeight(float inValue);

		virtual Attachment &copy() override;

		MeshAttachment &newLinkedMesh();

		/// Computes UVs for a mesh attachment.
		/// @param uvs Output array for the computed UVs, same length as regionUVs.
		static void computeUVs(TextureRegion *region, Array<float> &regionUVs, Array<float> &uvs);

	private:
		Sequence *_sequence;
		Array<float> _regionUVs;
		Array<unsigned short> _triangles;
		int _hullLength;
		String _path;
		Color _color;
		MeshAttachment *_sourceMesh;

		Array<unsigned short> _edges;
		float _width, _height;
	};
}

#endif /* Spine_MeshAttachment_h */
