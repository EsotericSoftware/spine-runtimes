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

#ifndef Spine_PathConstraint_h
#define Spine_PathConstraint_h

#include <spine/Constraint.h>
#include <spine/ConstraintData.h>
#include <spine/PathConstraintData.h>
#include <spine/PathConstraintPose.h>
#include <spine/Array.h>

namespace spine {
	class Skeleton;
	class PathAttachment;
	class BonePose;
	class Slot;
	class Bone;
	class Skin;
	class Attachment;

	/// Adjusts the rotation, translation, and scale of the constrained bones so they follow a PathAttachment.
	///
	/// See https://esotericsoftware.com/spine-path-constraints Path constraints in the Spine User Guide.
	// Non-exported base class that inherits from the template
	class PathConstraintBase : public ConstraintGeneric<PathConstraint, PathConstraintData, PathConstraintPose> {
	public:
		PathConstraintBase(PathConstraintData &data) : ConstraintGeneric<PathConstraint, PathConstraintData, PathConstraintPose>(data) {
		}
	};

	class SP_API PathConstraint : public PathConstraintBase {
		friend class Skeleton;
		friend class PathConstraintMixTimeline;
		friend class PathConstraintPositionTimeline;
		friend class PathConstraintPositionTimeline;
		friend class PathConstraintSpacingTimeline;

		RTTI_DECL

	public:
		static const float epsilon;
		static const int NONE;
		static const int BEFORE;
		static const int AFTER;

		PathConstraint(PathConstraintData &data, Skeleton &skeleton);

		PathConstraint &copy(Skeleton &skeleton);

		/// Applies the constraint to the constrained bones.
		virtual void update(Skeleton &skeleton, Physics physics) override;

		virtual void sort(Skeleton &skeleton) override;

		virtual bool isSourceActive() override;

		/// The bones that will be modified by this path constraint.
		Array<BonePose *> &getBones();

		/// The slot whose path attachment will be used to constrained the bones.
		Slot &getSlot();

		void setSlot(Slot &slot);

	private:
		Array<BonePose *> _bones;
		Slot *_slot;

		Array<float> _spaces;
		Array<float> _positions;
		Array<float> _world;
		Array<float> _curves;
		Array<float> _lengths;
		Array<float> _segments;

		Array<float> &computeWorldPositions(Skeleton &skeleton, PathAttachment &path, int spacesCount, bool tangents);

		void addBeforePosition(float p, Array<float> &temp, int i, Array<float> &output, int o);

		void addAfterPosition(float p, Array<float> &temp, int i, Array<float> &output, int o);

		void addCurvePosition(float p, float x1, float y1, float cx1, float cy1, float cx2, float cy2, float x2, float y2, Array<float> &output,
							  int o, bool tangents);

		void sortPathSlot(Skeleton &skeleton, Skin &skin, int slotIndex, Bone &slotBone);

		void sortPath(Skeleton &skeleton, Attachment *attachment, Bone &slotBone);
	};
}

#endif /* Spine_PathConstraint_h */
