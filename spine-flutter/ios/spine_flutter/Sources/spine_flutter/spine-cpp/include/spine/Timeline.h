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

#ifndef Spine_Timeline_h
#define Spine_Timeline_h

#include <spine/RTTI.h>
#include <spine/Array.h>
#include <spine/SpineObject.h>
#include <spine/Property.h>

namespace spine {
	class Skeleton;

	class Event;

	enum MixFrom {
		MixFrom_Current = 0,
		MixFrom_Setup = 1,
		MixFrom_First = 2
	};

	/// The base class for all timelines.
	///
	/// See <a href='https://esotericsoftware.com/spine-applying-animations#Timeline-API'>Applying Animations</a> in the Spine
	/// Runtimes Guide.
	class SP_API Timeline : public SpineObject {
		RTTI_DECL_NOPARENT

	public:
		Timeline(size_t frameCount, size_t frameEntries);

		virtual ~Timeline();

		/// Applies this timeline to the skeleton.
		///
		/// See <a href='https://esotericsoftware.com/spine-applying-animations#Timeline-API'>Applying Animations</a> in the Spine
		/// Runtimes Guide.
		/// @param skeleton The skeleton the timeline is applied to. This provides access to the bones, slots, and other skeleton
		///           components the timelines may change.
		/// @param lastTime The last time in seconds this timeline was applied. Some timelines trigger only at discrete times, in
		///           which case all keys are triggered between lastTime (exclusive) and time (inclusive). Pass -1 the first time a
		///           timeline is applied to ensure frame 0 is triggered.
		/// @param time The time in seconds the skeleton is being posed for. Timelines find the frame before and after this time and
		///           interpolate between the frame values.
		/// @param events If any events are fired, they are added to this list. Can be NULL to ignore fired events or if no timelines
		///           fire events.
		/// @param alpha 0 applies setup or current values (depending on from), 1 uses timeline values, and intermediate values
		///           interpolate between them. Adjusting alpha over time can mix a timeline in or out.
		/// @param from If true, alpha transitions between setup and timeline values, setup values are used before the first
		///           frame (current values are not used). If false, alpha transitions between current and timeline values, no change
		///           is made before the first frame.
		/// @param add If true, for timelines that support it, their values are added to the setup or current values (depending on
		///           from).
		/// @param out True when the animation is mixing out, else it is mixing in. Used by timelines that perform instant
		///           transitions.
		/// @param appliedPose True to modify getAppliedPose(), else getPose() is modified.
		virtual void apply(Skeleton &skeleton, float lastTime, float time, Array<Event *> *events, float alpha, MixFrom from, bool add, bool out,
						   bool appliedPose) = 0;

		/// True if this timeline supports additive blending.
		bool getAdditive() {
			return _additive;
		}

		/// True if this timeline sets values instantaneously and does not support interpolation between frames.
		bool getInstant() {
			return _instant;
		}

		size_t getFrameEntries();

		size_t getFrameCount();

		Array<float> &getFrames();

		float getDuration();

		virtual Array<PropertyId> &getPropertyIds();

	protected:
		void setPropertyIds(PropertyId propertyIds[], size_t propertyIdsCount);

		Array<PropertyId> _propertyIds;
		Array<float> _frames;
		size_t _frameEntries;
		bool _additive;
		bool _instant;
	};
}

#endif /* Spine_Timeline_h */
