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

#ifndef Spine_Slider_h
#define Spine_Slider_h

#include <spine/Constraint.h>
#include <spine/SliderData.h>
#include <spine/SliderPose.h>

namespace spine {
	class Skeleton;
	class Bone;
	class Animation;

	/// Applies an animation based on either the slider's SliderPose::getTime() or a bone's transform property.
	///
	/// See https://esotericsoftware.com/spine-sliders Sliders in the Spine User Guide.
	// Non-exported base class that inherits from the template
	class SliderBase : public ConstraintGeneric<Slider, SliderData, SliderPose> {
	public:
		SliderBase(SliderData &data) : ConstraintGeneric<Slider, SliderData, SliderPose>(data) {
		}
	};

	class SP_API Slider : public SliderBase {
		friend class Skeleton;
		friend class SliderTimeline;
		friend class SliderMixTimeline;

		RTTI_DECL

	public:
		Slider(SliderData &data, Skeleton &skeleton);

		Slider &copy(Skeleton &skeleton);

		virtual void update(Skeleton &skeleton, Physics physics) override;

		virtual void sort(Skeleton &skeleton) override;

		virtual bool isSourceActive() override;

		/// When set, the bone's transform property is used to set the slider's SliderPose::getTime().
		Bone &getBone();

		void setBone(Bone &bone);

	private:
		Bone *_bone;
		static float _offsets[6];
	};
}

#endif /* Spine_Slider_h */