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

#ifndef Spine_TransformConstraint_h
#define Spine_TransformConstraint_h

#include <spine/Constraint.h>
#include <spine/TransformConstraintData.h>
#include <spine/TransformConstraintPose.h>
#include <spine/Array.h>

namespace spine {
	class Skeleton;
	class Bone;
	class BonePose;

	// Non-exported base class that inherits from the template
	class TransformConstraintBase : public ConstraintGeneric<TransformConstraint, TransformConstraintData, TransformConstraintPose> {
	public:
		TransformConstraintBase(TransformConstraintData &data)
			: ConstraintGeneric<TransformConstraint, TransformConstraintData, TransformConstraintPose>(data) {
		}
	};

	/// Adjusts the world transform of the constrained bones to match that of the source bone.
	///
	/// See https://esotericsoftware.com/spine-transform-constraints Transform constraints in the Spine User Guide.
	class SP_API TransformConstraint : public TransformConstraintBase {
		friend class Skeleton;
		friend class TransformConstraintTimeline;

	public:
		RTTI_DECL

		TransformConstraint(TransformConstraintData &data, Skeleton &skeleton);

		virtual TransformConstraint &copy(Skeleton &skeleton);

		/// Applies the constraint to the constrained bones.
		void update(Skeleton &skeleton, Physics physics) override;

		void sort(Skeleton &skeleton) override;

		bool isSourceActive() override;

		/// The bones that will be modified by this transform constraint.
		Array<BonePose *> &getBones();

		/// The bone whose world transform will be matched by the constrained bones.
		Bone &getSource();

		void setSource(Bone &source);

	private:
		Array<BonePose *> _bones;
		Bone *_source;
	};
}

#endif /* Spine_TransformConstraint_h */
