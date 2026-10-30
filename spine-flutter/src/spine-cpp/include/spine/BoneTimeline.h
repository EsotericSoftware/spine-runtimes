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

#ifndef Spine_BoneTimeline_h
#define Spine_BoneTimeline_h

#include <cstddef>
#include <spine/dll.h>
#include <spine/CurveTimeline.h>

namespace spine {
	class Skeleton;
	class Event;
	class BonePose;

	/// An interface for timelines which change the property of a bone.
	class SP_API BoneTimeline {
		RTTI_DECL_NOPARENT

	public:
		BoneTimeline(int boneIndex) {
		}
		virtual ~BoneTimeline() {
		}

		/// The Skeleton::getBones() index of the bone that will be changed when this timeline is applied.
		virtual int getBoneIndex() const = 0;

		virtual void setBoneIndex(int inValue) = 0;
	};

	/// Base class for timelines that animate a single bone property.
	class SP_API BoneTimeline1 : public CurveTimeline1, public BoneTimeline {
		friend class SkeletonBinary;
		friend class SkeletonJson;
		friend class AnimationState;

		RTTI_DECL

	public:
		BoneTimeline1(size_t frameCount, size_t bezierCount, int boneIndex, Property property);

		virtual void apply(Skeleton &skeleton, float lastTime, float time, Array<Event *> *events, float alpha, MixFrom from, bool add, bool out,
						   bool appliedPose) override;

		virtual int getBoneIndex() const override {
			return _boneIndex;
		}

		virtual void setBoneIndex(int inValue) override {
			_boneIndex = inValue;
		}

	protected:
		/// Applies changes to the pose based on the timeline values.
		virtual void _apply(BonePose &pose, BonePose &setup, float time, float alpha, MixFrom from, bool add, bool out) = 0;

		int _boneIndex;
	};

	/// Base class for timelines that animate two bone properties.
	class SP_API BoneTimeline2 : public CurveTimeline, public BoneTimeline {
		friend class SkeletonBinary;
		friend class SkeletonJson;
		friend class AnimationState;

		RTTI_DECL

	public:
		BoneTimeline2(size_t frameCount, size_t bezierCount, int boneIndex, Property property1, Property property2);

		virtual void apply(Skeleton &skeleton, float lastTime, float time, Array<Event *> *events, float alpha, MixFrom from, bool add, bool out,
						   bool appliedPose) override;

		virtual int getBoneIndex() const override {
			return _boneIndex;
		}

		virtual void setBoneIndex(int inValue) override {
			_boneIndex = inValue;
		}

		virtual void setFrame(size_t frame, float time, float value1, float value2);

	protected:
		/// Applies changes to the pose based on the timeline values.
		virtual void _apply(BonePose &pose, BonePose &setup, float time, float alpha, MixFrom from, bool add, bool out) = 0;

		int _boneIndex;

		static const int ENTRIES;
		static const int VALUE1;
		static const int VALUE2;
	};
}

#endif