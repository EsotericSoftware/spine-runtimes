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

#ifndef Spine_PhysicsConstraintData_h
#define Spine_PhysicsConstraintData_h

#include <spine/ConstraintData.h>
#include <spine/PosedData.h>
#include <spine/PhysicsConstraintPose.h>

namespace spine {
	class BoneData;
	class PhysicsConstraint;

	/// Stores the setup pose for a PhysicsConstraint.
	///
	/// See https://esotericsoftware.com/spine-physics-constraints Physics constraints in the Spine User Guide.
	class SP_API PhysicsConstraintData : public ConstraintDataGeneric<PhysicsConstraint, PhysicsConstraintPose> {
		friend class SkeletonBinary;
		friend class SkeletonJson;
		friend class PhysicsConstraint;
		friend class Skeleton;

		RTTI_DECL
	public:
		explicit PhysicsConstraintData(const String &name);

		virtual Constraint &create(Skeleton &skeleton) override;

		/// The bone constrained by this physics constraint.
		BoneData &getBone();
		void setBone(BoneData &bone);

		/// The time in milliseconds required to advanced the physics simulation one step.
		float getStep();
		void setStep(float step);

		/// Physics influence on x translation, 0-1.
		float getX();
		void setX(float x);

		/// Physics influence on y translation, 0-1.
		float getY();
		void setY(float y);

		/// Physics influence on rotation, 0-1.
		float getRotate();
		void setRotate(float rotate);

		/// Physics influence on scaleX, 0-1.
		float getScaleX();
		void setScaleX(float scaleX);

		/// Physics influence on shearX, 0-1.
		float getShearX();
		void setShearX(float shearX);

		/// Movement greater than the limit will not have a greater affect on physics.
		float getLimit();
		void setLimit(float limit);

		/// Determines how BonePose::getScaleY() changes when getScaleX() sets BonePose::getScaleX().
		ScaleYMode getScaleYMode();
		void setScaleYMode(ScaleYMode scaleYMode);

		/// True when this constraint's inertia is controlled by global slider timelines.
		bool getInertiaGlobal();
		void setInertiaGlobal(bool inertiaGlobal);

		/// True when this constraint's strength is controlled by global slider timelines.
		bool getStrengthGlobal();
		void setStrengthGlobal(bool strengthGlobal);

		/// True when this constraint's damping is controlled by global slider timelines.
		bool getDampingGlobal();
		void setDampingGlobal(bool dampingGlobal);

		/// True when this constraint's mass is controlled by global slider timelines.
		bool getMassGlobal();
		void setMassGlobal(bool massGlobal);

		/// True when this constraint's wind is controlled by global slider timelines.
		bool getWindGlobal();
		void setWindGlobal(bool windGlobal);

		/// True when this constraint's gravity is controlled by global slider timelines.
		bool getGravityGlobal();
		void setGravityGlobal(bool gravityGlobal);

		/// True when this constraint's mix is controlled by global slider timelines.
		bool getMixGlobal();
		void setMixGlobal(bool mixGlobal);

	private:
		BoneData *_bone;
		float _x, _y, _rotate, _scaleX, _shearX, _limit, _step;
		ScaleYMode _scaleYMode;
		bool _inertiaGlobal, _strengthGlobal, _dampingGlobal, _massGlobal, _windGlobal, _gravityGlobal, _mixGlobal;
	};
}

#endif /* Spine_PhysicsConstraintData_h */