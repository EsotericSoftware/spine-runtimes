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

#ifndef SPINE_BONELOCAL_H_
#define SPINE_BONELOCAL_H_

#include <spine/SpineObject.h>
#include <spine/RTTI.h>
#include <spine/Pose.h>
#include <spine/Inherit.h>

namespace spine {
	/// Stores a bone's local pose.
	class SP_API BoneLocal : public Pose<BoneLocal> {
		friend class IkConstraint;
		friend class BoneTimeline1;
		friend class BoneTimeline2;
		friend class RotateTimeline;
		friend class InheritTimeline;
		friend class ScaleTimeline;
		friend class ScaleXTimeline;
		friend class ScaleYTimeline;
		friend class ShearTimeline;
		friend class ShearXTimeline;
		friend class ShearYTimeline;
		friend class TranslateTimeline;
		friend class TranslateXTimeline;
		friend class TranslateYTimeline;
		friend class SkeletonJson;
		friend class SkeletonBinary;
		friend class Skeleton;
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
		friend class AnimationState;

	protected:
		float _x, _y, _rotation, _scaleX, _scaleY, _shearX, _shearY;
		Inherit _inherit;

	public:
		BoneLocal();
		virtual ~BoneLocal();

		virtual void set(BoneLocal &pose) override;

		/// The local x translation.
		float getX();
		void setX(float x);

		/// The local y translation.
		float getY();
		void setY(float y);

		/// Sets local x and y translation.
		void setPosition(float x, float y);

		/// The local rotation in degrees, counter clockwise.
		float getRotation();
		void setRotation(float rotation);

		/// The local scaleX.
		float getScaleX();
		void setScaleX(float scaleX);

		/// The local scaleY.
		float getScaleY();
		void setScaleY(float scaleY);

		/// Sets local scaleX and scaleY.
		void setScale(float scaleX, float scaleY);

		/// Sets local scaleX and scaleY to the same value.
		void setScale(float scale);

		/// The local shearX.
		float getShearX();
		void setShearX(float shearX);

		/// The local shearY.
		float getShearY();
		void setShearY(float shearY);

		/// Determines how parent world transforms affect this bone.
		Inherit getInherit();
		void setInherit(Inherit inherit);
	};
}

#endif /* SPINE_BONELOCAL_H_ */