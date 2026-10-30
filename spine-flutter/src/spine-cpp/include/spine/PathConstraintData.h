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

#ifndef Spine_PathConstraintData_h
#define Spine_PathConstraintData_h

#include <spine/ConstraintData.h>
#include <spine/PosedData.h>
#include <spine/Array.h>
#include <spine/PathConstraintPose.h>
#include <spine/dll.h>
#include <spine/PositionMode.h>
#include <spine/SpacingMode.h>
#include <spine/RotateMode.h>

namespace spine {
	class BoneData;
	class SlotData;
	class PathConstraint;
	class Skeleton;

	/// Stores the setup pose for a PathConstraint.
	///
	/// See https://esotericsoftware.com/spine-path-constraints Path constraints in the Spine User Guide.
	class SP_API PathConstraintData : public ConstraintDataGeneric<PathConstraint, PathConstraintPose> {
		friend class SkeletonBinary;

		friend class SkeletonJson;

		friend class PathConstraint;

		friend class Skeleton;

		friend class PathConstraintMixTimeline;

		friend class PathConstraintPositionTimeline;

		friend class PathConstraintSpacingTimeline;

		RTTI_DECL
	public:
		explicit PathConstraintData(const String &name);

		virtual Constraint &create(Skeleton &skeleton) override;


		/// The bones that will be modified by this path constraint.
		Array<BoneData *> &getBones();

		/// The slot whose path attachment will be used to constrained the bones.
		SlotData &getSlot();

		void setSlot(SlotData &slot);

		/// The mode for positioning the first bone on the path.
		PositionMode getPositionMode();

		void setPositionMode(PositionMode positionMode);

		/// The mode for positioning the bones after the first bone on the path.
		SpacingMode getSpacingMode();

		void setSpacingMode(SpacingMode spacingMode);

		/// The mode for adjusting the rotation of the bones.
		RotateMode getRotateMode();

		void setRotateMode(RotateMode rotateMode);

		/// An offset added to the constrained bone rotation.
		float getOffsetRotation();

		void setOffsetRotation(float offsetRotation);

	private:
		Array<BoneData *> _bones;
		SlotData *_slot;
		PositionMode _positionMode;
		SpacingMode _spacingMode;
		RotateMode _rotateMode;
		float _offsetRotation;
	};
}

#endif /* Spine_PathConstraintData_h */
