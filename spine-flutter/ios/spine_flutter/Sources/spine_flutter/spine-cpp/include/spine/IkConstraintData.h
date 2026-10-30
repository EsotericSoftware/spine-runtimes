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

#ifndef Spine_IkConstraintData_h
#define Spine_IkConstraintData_h

#include <spine/Array.h>
#include <spine/SpineObject.h>
#include <spine/SpineString.h>
#include <spine/ConstraintData.h>
#include <spine/PosedData.h>
#include <spine/IkConstraintPose.h>

namespace spine {
	class BoneData;
	class IkConstraint;

	class SP_API IkConstraintData : public ConstraintDataGeneric<IkConstraint, IkConstraintPose> {
		friend class SkeletonBinary;

		friend class SkeletonJson;

		friend class IkConstraint;

		friend class Skeleton;

		friend class IkConstraintTimeline;

		RTTI_DECL

	public:
		explicit IkConstraintData(const String &name);

		virtual Constraint &create(Skeleton &skeleton) override;

		/// The bones that are constrained by this IK Constraint.
		Array<BoneData *> &getBones();

		/// The bone that is the IK target.
		BoneData &getTarget();

		void setTarget(BoneData &inValue);

		/// Determines how BonePose::getScaleY() changes when IkConstraintPose::getCompress() or IkConstraintPose::getStretch()
		/// sets BonePose::getScaleX().
		ScaleYMode getScaleYMode();

		void setScaleYMode(ScaleYMode scaleYMode);

	private:
		Array<BoneData *> _bones;
		BoneData *_target;
		ScaleYMode _scaleYMode;
	};
}

#endif /* Spine_IkConstraintData_h */
