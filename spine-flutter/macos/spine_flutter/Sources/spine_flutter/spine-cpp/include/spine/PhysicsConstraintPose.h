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

#ifndef Spine_PhysicsConstraintPose_h
#define Spine_PhysicsConstraintPose_h

#include <spine/Pose.h>
#include <spine/RTTI.h>

namespace spine {
	/// Stores a pose for a physics constraint.
	class SP_API PhysicsConstraintPose : public Pose<PhysicsConstraintPose> {
		friend class PhysicsConstraint;
		friend class PhysicsConstraintTimeline;
		friend class SkeletonJson;
		friend class SkeletonBinary;

	private:
		float _inertia;
		float _strength;
		float _damping;
		float _massInverse;
		float _wind;
		float _gravity;
		float _mix;

	public:
		PhysicsConstraintPose();
		virtual ~PhysicsConstraintPose();

		virtual void set(PhysicsConstraintPose &pose) override;

		/// Controls how much bone movement is converted into physics movement.
		float getInertia();
		void setInertia(float inertia);

		/// The amount of force used to return properties to the unconstrained value.
		float getStrength();
		void setStrength(float strength);

		/// Reduces the speed of physics movements, with more of a reduction at higher speeds.
		float getDamping();
		void setDamping(float damping);

		/// Determines susceptibility to acceleration.
		float getMassInverse();
		void setMassInverse(float massInverse);

		/// Applies a constant force along the Skeleton::getWindX(), Skeleton::getWindY() vector.
		float getWind();
		void setWind(float wind);

		/// Applies a constant force along the Skeleton::getGravityX(), Skeleton::getGravityY() vector.
		float getGravity();
		void setGravity(float gravity);

		/// A percentage (0+) that controls the mix between the constrained and unconstrained poses.
		float getMix();
		void setMix(float mix);
	};
}

#endif