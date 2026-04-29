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

#ifndef Spine_ClippingAttachment_h
#define Spine_ClippingAttachment_h

#include <spine/VertexAttachment.h>
#include <spine/Color.h>

namespace spine {
	class SlotData;

	class SP_API ClippingAttachment : public VertexAttachment {
		friend class SkeletonBinary;

		friend class SkeletonJson;

		friend class SkeletonClipping;

		RTTI_DECL

	public:
		explicit ClippingAttachment(const String &name);

		/// Clipping is performed between the clipping attachment's slot and the end slot. If NULL, clipping is done until
		/// the end of the skeleton's rendering.
		SlotData *getEndSlot();

		void setEndSlot(SlotData *inValue);

		/// When true the clipping polygon is treated as convex for more efficient clipping. If the polygon deforms to concave then the
		/// convex hull is used. When false the clipping polygon can be concave and if so has an additional CPU cost. Inverse clipping
		/// always uses convex.
		bool getConvex();

		void setConvex(bool convex);

		/// When false, everything inside the clipping polygon is visible. When true, everything outside the clipping polygon is
		/// visible and clipping is convex.
		bool getInverse();

		void setInverse(bool inverse);

		Color &getColor();

		virtual Attachment &copy() override;

	private:
		SlotData *_endSlot;
		bool _convex;
		bool _inverse;
		Color _color;
	};
}

#endif /* Spine_ClippingAttachment_h */
