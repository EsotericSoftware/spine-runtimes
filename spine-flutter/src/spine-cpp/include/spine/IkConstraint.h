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

#ifndef Spine_IkConstraint_h
#define Spine_IkConstraint_h

#include <spine/Constraint.h>
#include <spine/ConstraintData.h>
#include <spine/IkConstraintData.h>
#include <spine/IkConstraintPose.h>
#include <spine/Array.h>

namespace spine {
	class Skeleton;
	class Bone;
	class BonePose;

	// Non-exported base class that inherits from the template
	class IkConstraintBase : public ConstraintGeneric<IkConstraint, IkConstraintData, IkConstraintPose> {
	public:
		IkConstraintBase(IkConstraintData &data) : ConstraintGeneric<IkConstraint, IkConstraintData, IkConstraintPose>(data) {
		}
	};

	class SP_API IkConstraint : public IkConstraintBase {
		friend class Skeleton;

		friend class IkConstraintTimeline;

		RTTI_DECL

	public:
		IkConstraint(IkConstraintData &data, Skeleton &skeleton);

		virtual IkConstraint &copy(Skeleton &skeleton);

		virtual void update(Skeleton &skeleton, Physics physics) override;

		virtual void sort(Skeleton &skeleton) override;

		virtual bool isSourceActive() override;

		Array<BonePose *> &getBones();

		Bone &getTarget();

		void setTarget(Bone &inValue);

		/// Adjusts the local rotation of the bone so the world position of the tip is as close to the target position as
		/// possible. The target is specified in the world coordinate system.
		static void apply(Skeleton &skeleton, BonePose &bone, float targetX, float targetY, bool compress, bool stretch, ScaleYMode scaleYMode,
						  float mix);

		/// Adjusts the parent and child bone rotations so the tip of the child is as close to the target position as
		/// possible. The target is specified in the world coordinate system.
		/// @param child A direct descendant of the parent bone.
		static void apply(Skeleton &skeleton, BonePose &parent, BonePose &child, float targetX, float targetY, int bendDirection, bool stretch,
						  ScaleYMode scaleYMode, float softness, float mix);

	private:
		Array<BonePose *> _bones;
		Bone *_target;
	};
}

#endif /* Spine_IkConstraint_h */
