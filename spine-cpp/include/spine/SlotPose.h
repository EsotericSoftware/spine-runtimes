/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

#ifndef SPINE_SLOTPOSE_H_
#define SPINE_SLOTPOSE_H_

#include <spine/SpineObject.h>
#include <spine/RTTI.h>
#include <spine/Pose.h>
#include <spine/Color.h>
#include <spine/Array.h>

namespace spine {
	class Attachment;
	class VertexAttachment;

	/// Stores a slot's pose.
	class SP_API SlotPose : public Pose<SlotPose> {
		friend class Slot;
		friend class SlotCurveTimeline;
		friend class DeformTimeline;
		friend class RGBATimeline;
		friend class RGBTimeline;
		friend class AlphaTimeline;
		friend class RGBA2Timeline;
		friend class RGB2Timeline;
		friend class PathConstraint;
		friend class SkeletonJson;
		friend class SkeletonBinary;
		friend class AnimationState;

	protected:
		Color _color;
		Color _darkColor;
		bool _hasDarkColor;
		Attachment *_attachment;
		int _sequenceIndex;
		Array<float> _deform;

	public:
		SlotPose();
		virtual ~SlotPose();

		virtual void set(SlotPose &pose) override;

		/// The color used to tint the slot's attachment. If a dark color is set, this is used as the light color for two color
		/// tinting.
		Color &getColor();

		/// The dark color used to tint the slot's attachment for two color tinting. The dark
		/// color's alpha is not used.
		Color &getDarkColor();

		/// Returns true if this slot has a dark color.
		bool hasDarkColor();

		void setHasDarkColor(bool hasDarkColor);

		/// The current attachment for the slot, or null if the slot has no attachment.
		Attachment *getAttachment();

		/// Sets the slot's attachment and, if the attachment changed, resets sequenceIndex and clears the deform.
		/// The deform is not cleared if the old attachment has the same VertexAttachment::getTimelineAttachment() as the
		/// specified attachment.
		void setAttachment(Attachment *attachment);

		/// The index of the texture region to display when the slot's attachment has a Sequence. -1 represents the
		/// Sequence::getSetupIndex().
		int getSequenceIndex();
		void setSequenceIndex(int sequenceIndex);

		/// Values to deform the slot's attachment. For an unweighted mesh, the entries are local positions for each vertex. For a
		/// weighted mesh, the entries are an offset for each vertex which will be added to the mesh's local vertex positions.
		///
		/// See VertexAttachment::computeWorldVertices() and DeformTimeline.
		Array<float> &getDeform();
	};
}

#endif /* SPINE_SLOTPOSE_H_ */