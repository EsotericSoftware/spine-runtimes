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

#ifndef Spine_PathConstraintPose_h
#define Spine_PathConstraintPose_h

#include <spine/Pose.h>
#include <spine/RTTI.h>

namespace spine {
	/// Stores a pose for a path constraint.
	class SP_API PathConstraintPose : public Pose<PathConstraintPose> {
		friend class PathConstraint;
		friend class PathConstraintPositionTimeline;
		friend class PathConstraintSpacingTimeline;
		friend class PathConstraintMixTimeline;
		friend class SkeletonJson;
		friend class SkeletonBinary;

	private:
		float _position;
		float _spacing;
		float _mixRotate;
		float _mixX;
		float _mixY;

	public:
		PathConstraintPose();
		virtual ~PathConstraintPose();

		virtual void set(PathConstraintPose &pose) override;

		/// The position along the path.
		float getPosition();
		void setPosition(float position);

		/// The spacing between bones.
		float getSpacing();
		void setSpacing(float spacing);

		/// A percentage (0-1) that controls the mix between the constrained and unconstrained rotation.
		float getMixRotate();
		void setMixRotate(float mixRotate);

		/// A percentage (0-1) that controls the mix between the constrained and unconstrained translation X.
		float getMixX();
		void setMixX(float mixX);

		/// A percentage (0-1) that controls the mix between the constrained and unconstrained translation Y.
		float getMixY();
		void setMixY(float mixY);
	};
}

#endif