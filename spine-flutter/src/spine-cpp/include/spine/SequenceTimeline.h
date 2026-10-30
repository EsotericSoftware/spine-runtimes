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

#ifndef Spine_SequenceTimeline_h
#define Spine_SequenceTimeline_h

#include <spine/Timeline.h>
#include <spine/SlotTimeline.h>
#include <spine/Sequence.h>

namespace spine {
	class Attachment;
	class HasTextureRegion;
	class Slot;

	/// Changes the sequence index for an attachment's Sequence.
	class SP_API SequenceTimeline : public Timeline, public SlotTimeline {
		friend class SkeletonBinary;

		friend class SkeletonJson;

		RTTI_DECL

	public:
		explicit SequenceTimeline(size_t frameCount, int slotIndex, Attachment &attachment);

		virtual ~SequenceTimeline();

		virtual void apply(Skeleton &skeleton, float lastTime, float time, Array<Event *> *events, float alpha, MixFrom from, bool add, bool out,
						   bool appliedPose) override;

		/// Sets the time, mode, index, and frame time for the specified frame.
		/// @param frame Between 0 and frameCount, inclusive.
		/// @param delay Seconds between frames.
		void setFrame(int frame, float time, SequenceMode mode, int index, float delay);

		/// The attachment for which the sequence index will be set.
		///
		/// See Attachment::getTimelineAttachment().
		Attachment &getAttachment() {
			return *(Attachment *) _attachment;
		}

		virtual int getSlotIndex() override;

		virtual void setSlotIndex(int inValue) override;

	protected:
		int _slotIndex;
		Attachment *_attachment;

		void setupPose(Slot &slot, bool appliedPose);
		void applyToSlot(Slot &slot, bool appliedPose, Sequence &sequence, float time, float before, int modeAndIndex, float delay);

		static const int ENTRIES = 3;
		static const int MODE = 1;
		static const int DELAY = 2;
	};
}

#endif /* Spine_SequenceTimeline_h */
