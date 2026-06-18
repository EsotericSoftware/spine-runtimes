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

#ifndef Spine_Animation_h
#define Spine_Animation_h

#include <spine/Array.h>
#include <spine/Color.h>
#include <spine/Map.h>
#include <spine/SpineObject.h>
#include <spine/SpineString.h>
#include <spine/Property.h>
#include <spine/Timeline.h>

namespace spine {
	class Timeline;
	class BoneTimeline;

	class Skeleton;

	class Event;

	class AnimationState;

	/// Stores a list of timelines to animate a skeleton's pose over time.
	///
	/// See <a href='https://esotericsoftware.com/spine-applying-animations#Timeline-API'>Applying Animations</a> in the Spine
	/// Runtimes Guide.
	class SP_API Animation : public SpineObject {
		friend class AnimationState;

		friend class TrackEntry;

		friend class AnimationStateData;

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

		friend class RotateTimeline;

		friend class ScaleTimeline;

		friend class ShearTimeline;

		friend class TransformConstraintTimeline;

		friend class TranslateTimeline;

		friend class TranslateXTimeline;

		friend class TranslateYTimeline;

		friend class TwoColorTimeline;

		friend class Slider;

	public:
		/// Creates a new animation. The timelines must be set before use.
		Animation(const String &name);

		~Animation();

		/// If this list or the timelines it contains are modified, the timelines and bones must be set again to recompute the
		/// animation's bone indices and timeline property IDs.
		///
		/// See setTimelines().
		Array<Timeline *> &getTimelines();

		/// Sets the timelines and bone indices.
		void setTimelines(Array<Timeline *> &timelines, Array<int> &bones);

		/// Returns true if this animation contains a timeline with any of the specified property IDs.
		bool hasTimeline(Array<PropertyId> &ids);

		/// The duration of the animation in seconds, which is usually the highest time of all frames in the timeline. The duration is
		/// used to know when it has completed and when it should loop back to the start.
		float getDuration();

		void setDuration(float inValue);

		/// Applies the animation's timelines to the specified skeleton.
		///
		/// See Timeline::apply() and
		/// <a href='https://esotericsoftware.com/spine-applying-animations#Timeline-API'>Applying Animations</a> in the Spine
		/// Runtimes Guide.
		/// @param skeleton The skeleton the animation is applied to. This provides access to the bones, slots, and other skeleton
		///           components the timelines may change.
		/// @param lastTime The last time in seconds this animation was applied. Some timelines trigger only at discrete times, in
		///           which case all keys are triggered between lastTime (exclusive) and time (inclusive). Pass -1 the first time an
		///           animation is applied to ensure frame 0 is triggered.
		/// @param time The time in seconds the skeleton is being posed for. Timelines find the frame before and after this time and
		///           interpolate between the frame values.
		/// @param loop True if time beyond the animation duration repeats the animation, else the last frame is used.
		/// @param events If any events are fired, they are added to this list. Can be NULL to ignore fired events or if no timelines
		///           fire events.
		/// @param alpha 0 applies setup or current values (depending on from), 1 uses timeline values, and intermediate values
		///           interpolate between them. Adjusting alpha over time can mix an animation in or out.
		/// @param from If true, alpha transitions between setup and timeline values, setup values are used before the first
		///           frame (current values are not used). If false, alpha transitions between current and timeline values, no change
		///           is made before the first frame.
		/// @param add If true, for timelines that support it, their values are added to the setup or current values (depending on
		///           from).
		/// @param out True when the animation is mixing out, else it is mixing in. Used by timelines that perform instant
		///           transitions.
		/// @param appliedPose True to modify getAppliedPose(), else the unconstrained pose is modified.
		void apply(Skeleton &skeleton, float lastTime, float time, bool loop, Array<Event *> *events, float alpha, MixFrom from, bool add, bool out,
				   bool appliedPose);

		/// The animation's name, which is unique across all animations in the skeleton.
		const String &getName();

		/// The Skeleton::getBones() indices affected by this animation.
		///
		/// See setTimelines() and BoneTimeline::getBoneIndex().
		const Array<int> &getBones();

		/// The color of the animation as it was in Spine, or a default color if nonessential data was not exported.
		Color &getColor();

		/// @param target After the first and before the last entry.
		static int search(Array<float> &values, float target);

		static int search(Array<float> &values, float target, int step);

	protected:
		Array<Timeline *> _timelines;
		Map<PropertyId, bool> _timelineIds;
		Array<int> _bones;
		float _duration;
		String _name;
		Color _color;
	};
}

#endif /* Spine_Animation_h */
