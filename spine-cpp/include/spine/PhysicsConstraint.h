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

#ifndef Spine_PhysicsConstraint_h
#define Spine_PhysicsConstraint_h

#include <spine/Constraint.h>
#include <spine/PhysicsConstraintData.h>
#include <spine/PhysicsConstraintPose.h>
#include <spine/BonePose.h>
#include <spine/Array.h>

namespace spine {
	class Skeleton;
	class BonePose;
	class PhysicsConstraintPose;

	/// Applies physics to a bone.
	///
	/// See https://esotericsoftware.com/spine-physics-constraints Physics constraints in the Spine User Guide.
	// Non-exported base class that inherits from the template
	class PhysicsConstraintBase : public ConstraintGeneric<PhysicsConstraint, PhysicsConstraintData, PhysicsConstraintPose> {
	public:
		PhysicsConstraintBase(PhysicsConstraintData &data)
			: ConstraintGeneric<PhysicsConstraint, PhysicsConstraintData, PhysicsConstraintPose>(data) {
		}
	};

	class SP_API PhysicsConstraint : public PhysicsConstraintBase {
		friend class Skeleton;
		friend class PhysicsConstraintTimeline;
		friend class PhysicsConstraintInertiaTimeline;
		friend class PhysicsConstraintStrengthTimeline;
		friend class PhysicsConstraintDampingTimeline;
		friend class PhysicsConstraintMassTimeline;
		friend class PhysicsConstraintWindTimeline;
		friend class PhysicsConstraintGravityTimeline;
		friend class PhysicsConstraintMixTimeline;
		friend class PhysicsConstraintResetTimeline;

	public:
		RTTI_DECL

		PhysicsConstraint(PhysicsConstraintData &data, Skeleton &skeleton);

		void update(Skeleton &skeleton, Physics physics) override;
		void sort(Skeleton &skeleton) override;
		bool isSourceActive() override;
		PhysicsConstraint &copy(Skeleton &skeleton);

		/// Resets all physics state that was the result of previous movement. Use this after moving a bone to prevent physics
		/// from reacting to the movement.
		void reset(Skeleton &skeleton);

		/// Translates the physics constraint so the next update() forces are applied as if the bone moved an additional amount in world space.
		void translate(float x, float y);

		/// Rotates the physics constraint so the next update() forces are applied as if the bone rotated around the specified point in world space.
		void rotate(float x, float y, float degrees);

		/// The bone constrained by this physics constraint.
		BonePose &getBone();
		void setBone(BonePose &bone);

	private:
		BonePose *_bone;

		bool _reset;
		float _ux, _uy, _cx, _cy, _tx, _ty;
		float _xOffset, _xLag, _xVelocity;
		float _yOffset, _yLag, _yVelocity;
		float _rotateOffset, _rotateLag, _rotateVelocity;
		float _scaleOffset, _scaleLag, _scaleVelocity;
		float _remaining, _lastTime;
	};
}

#endif /* Spine_PhysicsConstraint_h */
