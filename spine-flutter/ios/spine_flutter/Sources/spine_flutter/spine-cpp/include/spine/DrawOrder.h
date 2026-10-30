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

#ifndef Spine_DrawOrder_h
#define Spine_DrawOrder_h

#include <spine/Array.h>
#include <spine/SpineObject.h>

namespace spine {
	class Slot;

	/// Stores the skeleton's draw order, which is the order that each slot's attachment is rendered.
	class SP_API DrawOrder : public SpineObject {
		friend class Skeleton;

		friend class DrawOrderTimeline;

		friend class DrawOrderFolderTimeline;

		friend class Slider;

	public:
		explicit DrawOrder(Array<Slot *> &setupPose);

		/// Sets the unconstrained draw order to the setup pose order.
		void setupPose();

		/// The unconstrained draw order, set by animations and application code.
		Array<Slot *> &getPose();

		/// The constrained draw order for rendering. If no constraints modify the draw order, this is the same as getPose().
		/// Otherwise it is a copy of getPose() modified by constraints.
		Array<Slot *> &getAppliedPose();

	private:
		/// Sets the applied pose to the unconstrained pose, for when no constraints will modify the draw order.
		void unconstrained();

		/// Sets the applied pose to the constrained pose, in anticipation of the applied pose being modified by constraints.
		void constrained();

		/// Copies the unconstrained pose to the constrained pose, as a starting point for constraints to be applied.
		void reset();

		Array<Slot *> &_setupPose;
		Array<Slot *> _pose;
		Array<Slot *> _constrainedPose;
		Array<Slot *> *_appliedPose;
	};
}

#endif /* Spine_DrawOrder_h */
