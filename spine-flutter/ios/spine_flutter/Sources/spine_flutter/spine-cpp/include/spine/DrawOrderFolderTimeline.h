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

#ifndef Spine_DrawOrderFolderTimeline_h
#define Spine_DrawOrderFolderTimeline_h

#include <spine/Timeline.h>

namespace spine {
	class Slot;

	/// Changes a subset of the Skeleton::getDrawOrder() draw order.
	class SP_API DrawOrderFolderTimeline : public Timeline {
		friend class SkeletonBinary;

		friend class SkeletonJson;

		RTTI_DECL

	public:
		DrawOrderFolderTimeline(size_t frameCount, Array<int> &slots, size_t slotCount);

		virtual void apply(Skeleton &skeleton, float lastTime, float time, Array<Event *> *events, float alpha, MixFrom from, bool add, bool out,
						   bool appliedPose) override;

		size_t getFrameCount();

		/// The Skeleton::getSlots() indices that this timeline affects, in setup order.
		Array<int> &getSlots();

		/// The draw order for each frame. See setFrame().
		Array<Array<int>> &getDrawOrders();

		/// Sets the time and draw order for the specified frame.
		/// @param frame Between 0 and frameCount, inclusive.
		/// @param time The frame time in seconds.
		/// @param drawOrder Ordered getSlots() indices, or null to use setup pose order.
		void setFrame(size_t frame, float time, Array<int> *drawOrder);

	private:
		Array<int> _slots;
		Array<bool> _inFolder;
		Array<Array<int>> _drawOrders;

		void setup(Array<Slot *> &pose, Array<Slot *> &setupPose);
		void apply(Array<Slot *> &pose, Array<Slot *> &setupPose, Array<int> &drawOrder);
	};
}

#endif /* Spine_DrawOrderFolderTimeline_h */
