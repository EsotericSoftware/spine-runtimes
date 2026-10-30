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

#ifndef Spine_IkConstraintPose_h
#define Spine_IkConstraintPose_h

#include <spine/Pose.h>
#include <spine/RTTI.h>

namespace spine {

	/// Stores a pose for an IK constraint.
	class SP_API IkConstraintPose : public Pose<IkConstraintPose> {
		friend class IkConstraint;
		friend class IkConstraintTimeline;
		friend class SkeletonJson;
		friend class SkeletonBinary;

	public:
		IkConstraintPose();
		virtual ~IkConstraintPose();

		virtual void set(IkConstraintPose &pose) override;

		/// A percentage (0-1) that controls the mix between the constrained and unconstrained rotation.
		///
		/// For two bone IK: if the parent bone has local nonuniform scale, the child bone's local Y translation is set to 0.
		float getMix();
		void setMix(float mix);

		/// For two bone IK, the target bone's distance from the maximum reach of the bones where rotation begins to slow. The bones
		/// will not straighten completely until the target is this far out of range.
		float getSoftness();
		void setSoftness(float softness);

		/// For two bone IK, controls the bend direction of the IK bones, either 1 or -1.
		int getBendDirection();
		void setBendDirection(int bendDirection);

		/// For one bone IK, when true and the target is too close, the bone is scaled to reach it.
		bool getCompress();
		void setCompress(bool compress);

		/// When true and the target is out of range, the parent bone is scaled to reach it.
		///
		/// For two bone IK: 1) the child bone's local Y translation is set to 0, 2) stretch is not applied if softness is
		/// > 0, and 3) if the parent bone has local nonuniform scale, stretch is not applied.
		bool getStretch();
		void setStretch(bool stretch);

	private:
		int _bendDirection;
		bool _compress, _stretch;
		float _mix, _softness;
	};
}

#endif /* Spine_IkConstraintPose_h */