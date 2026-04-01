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

#ifndef Spine_SlotData_h
#define Spine_SlotData_h

#include <spine/BlendMode.h>
#include <spine/PosedData.h>
#include <spine/SpineString.h>
#include <spine/RTTI.h>
#include <spine/SlotPose.h>

namespace spine {
	class BoneData;

	/// Stores the setup pose for a Slot.
	class SP_API SlotData : public PosedDataGeneric<SlotPose> {
		friend class SkeletonBinary;
		friend class SkeletonJson;
		friend class PathConstraint;
		friend class AttachmentTimeline;
		friend class RGBATimeline;
		friend class RGBTimeline;
		friend class AlphaTimeline;
		friend class RGBA2Timeline;
		friend class RGB2Timeline;
		friend class DeformTimeline;
		friend class DrawOrderTimeline;
		friend class EventTimeline;
		friend class IkConstraintTimeline;
		friend class PathConstraintMixTimeline;
		friend class PathConstraintPositionTimeline;
		friend class PathConstraintSpacingTimeline;
		friend class ScaleTimeline;
		friend class ShearTimeline;
		friend class TransformConstraintTimeline;
		friend class TranslateTimeline;
		friend class TwoColorTimeline;
		friend class Slot;

	public:
		SlotData(int index, const String &name, BoneData &boneData);

		/// The Skeleton::getSlots() index for this slot.
		int getIndex();

		/// The bone this slot belongs to.
		BoneData &getBoneData();

		void setAttachmentName(const String &attachmentName);

		/// The name of the attachment that is visible for this slot in the setup pose, or empty if no attachment is visible.
		const String &getAttachmentName();

		/// The blend mode for drawing the slot's attachment.
		BlendMode getBlendMode();
		void setBlendMode(BlendMode blendMode);

		/// False if the slot was hidden in Spine and nonessential data was exported. Does not affect runtime rendering.
		bool getVisible();
		void setVisible(bool visible);

	private:
		const int _index;
		BoneData &_boneData;
		String _attachmentName;
		BlendMode _blendMode;
		bool _visible;
	};
}

#endif /* Spine_SlotData_h */
