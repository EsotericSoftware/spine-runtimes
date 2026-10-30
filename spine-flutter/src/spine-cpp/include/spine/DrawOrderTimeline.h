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

#ifndef Spine_DrawOrderTimeline_h
#define Spine_DrawOrderTimeline_h

#include <spine/Timeline.h>

namespace spine {
	/// Changes the Skeleton::getDrawOrder().
	class SP_API DrawOrderTimeline : public Timeline {
		friend class SkeletonBinary;

		friend class SkeletonJson;

		RTTI_DECL

	public:
		static PropertyId getPropertyId();

		explicit DrawOrderTimeline(size_t frameCount);

		virtual void apply(Skeleton &skeleton, float lastTime, float time, Array<Event *> *events, float alpha, MixFrom from, bool add, bool out,
						   bool appliedPose) override;

		size_t getFrameCount();

		/// The draw order for each frame. See setFrame().
		Array<Array<int>> &getDrawOrders();

		/// Sets the time and draw order for the specified frame.
		/// @param frame Between 0 and frameCount, inclusive.
		/// @param time The frame time in seconds.
		/// @param drawOrder For each slot in Skeleton::getSlots(), the index of the slot in the new draw order. May be null to use
		///           setup pose draw order.
		void setFrame(size_t frame, float time, Array<int> *drawOrder);

	private:
		Array<Array<int>> _drawOrders;
	};
}

#endif /* Spine_DrawOrderTimeline_h */
