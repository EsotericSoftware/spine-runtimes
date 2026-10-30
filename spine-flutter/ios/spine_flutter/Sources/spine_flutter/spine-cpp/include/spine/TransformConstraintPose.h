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

#ifndef Spine_TransformConstraintPose_h
#define Spine_TransformConstraintPose_h

#include <spine/Pose.h>
#include <spine/RTTI.h>

namespace spine {
	/// Stores a pose for a transform constraint.
	class SP_API TransformConstraintPose : public Pose<TransformConstraintPose> {
		friend class FromProperty;
		friend class ToProperty;
		friend class FromRotate;
		friend class ToRotate;
		friend class FromX;
		friend class ToX;
		friend class FromY;
		friend class ToY;
		friend class FromScaleX;
		friend class ToScaleX;
		friend class FromScaleY;
		friend class ToScaleY;
		friend class FromShearY;
		friend class ToShearY;
		friend class TransformConstraint;
		friend class TransformConstraintTimeline;
		friend class SkeletonJson;
		friend class SkeletonBinary;

	private:
		float _mixRotate, _mixX, _mixY, _mixScaleX, _mixScaleY, _mixShearY;

	public:
		TransformConstraintPose();
		virtual ~TransformConstraintPose();

		virtual void set(TransformConstraintPose &pose) override;

		/// A percentage (unbounded) that controls the mix between the constrained and unconstrained rotation.
		float getMixRotate();
		void setMixRotate(float mixRotate);

		/// A percentage (unbounded) that controls the mix between the constrained and unconstrained translation X.
		float getMixX();
		void setMixX(float mixX);

		/// A percentage (unbounded) that controls the mix between the constrained and unconstrained translation Y.
		float getMixY();
		void setMixY(float mixY);

		/// A percentage (unbounded) that controls the mix between the constrained and unconstrained scale X.
		float getMixScaleX();
		void setMixScaleX(float mixScaleX);

		/// A percentage (unbounded) that controls the mix between the constrained and unconstrained scale Y.
		float getMixScaleY();
		void setMixScaleY(float mixScaleY);

		/// A percentage (unbounded) that controls the mix between the constrained and unconstrained shear Y.
		float getMixShearY();
		void setMixShearY(float mixShearY);
	};
}

#endif