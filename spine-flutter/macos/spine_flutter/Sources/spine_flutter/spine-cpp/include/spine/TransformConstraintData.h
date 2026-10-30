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

#ifndef Spine_TransformConstraintData_h
#define Spine_TransformConstraintData_h

#include <spine/ConstraintData.h>
#include <spine/PosedData.h>
#include <spine/Array.h>
#include <spine/TransformConstraintPose.h>

namespace spine {
	class BoneData;
	class TransformConstraint;
	class BonePose;
	class TransformConstraintPose;

	class FromProperty;
	class ToProperty;

	/// Source property for a TransformConstraint.
	class SP_API FromProperty : public SpineObject {
		friend class SkeletonBinary;

	public:
		RTTI_DECL_NOPARENT

		/// The value of this property that corresponds to ToProperty offset.
		float _offset;

		/// Constrained properties.
		Array<ToProperty *> _to;

		FromProperty();
		virtual ~FromProperty();

		/// Reads this property from the specified bone.
		virtual float value(Skeleton &skeleton, BonePose &source, bool local, float *offsets) = 0;
	};

	/// Constrained property for a TransformConstraint.
	class SP_API ToProperty : public SpineObject {
		friend class SkeletonBinary;

	public:
		RTTI_DECL_NOPARENT

		/// The value of this property that corresponds to FromProperty offset.
		float _offset;

		/// The maximum value of this property when clamped.
		float _max;

		/// The scale of the FromProperty value in relation to this property.
		float _scale;

		ToProperty();
		virtual ~ToProperty();

		/// Reads the mix for this property from the specified pose.
		virtual float mix(TransformConstraintPose &pose) = 0;

		/// Applies the value to this property.
		virtual void apply(Skeleton &skeleton, TransformConstraintPose &pose, BonePose &bone, float value, bool local, bool additive) = 0;
	};

	class SP_API FromRotate : public FromProperty {
	public:
		RTTI_DECL

		FromRotate() : FromProperty() {
		}
		~FromRotate() {
		}

		float value(Skeleton &skeleton, BonePose &source, bool local, float *offsets) override;
	};

	class SP_API ToRotate : public ToProperty {
	public:
		RTTI_DECL

		ToRotate() : ToProperty() {
		}
		~ToRotate() {
		}

		float mix(TransformConstraintPose &pose) override;
		void apply(Skeleton &skeleton, TransformConstraintPose &pose, BonePose &bone, float value, bool local, bool additive) override;
	};

	class SP_API FromX : public FromProperty {
	public:
		RTTI_DECL

		FromX() : FromProperty() {
		}
		~FromX() {
		}

		float value(Skeleton &skeleton, BonePose &source, bool local, float *offsets) override;
	};

	class SP_API ToX : public ToProperty {
	public:
		RTTI_DECL

		ToX() : ToProperty() {
		}
		~ToX() {
		}

		float mix(TransformConstraintPose &pose) override;
		void apply(Skeleton &skeleton, TransformConstraintPose &pose, BonePose &bone, float value, bool local, bool additive) override;
	};

	class SP_API FromY : public FromProperty {
	public:
		RTTI_DECL

		FromY() : FromProperty() {
		}
		~FromY() {
		}

		float value(Skeleton &skeleton, BonePose &source, bool local, float *offsets) override;
	};

	class SP_API ToY : public ToProperty {
	public:
		RTTI_DECL

		ToY() : ToProperty() {
		}
		~ToY() {
		}

		float mix(TransformConstraintPose &pose) override;
		void apply(Skeleton &skeleton, TransformConstraintPose &pose, BonePose &bone, float value, bool local, bool additive) override;
	};

	class SP_API FromScaleX : public FromProperty {
	public:
		RTTI_DECL

		FromScaleX() : FromProperty() {
		}
		~FromScaleX() {
		}

		float value(Skeleton &skeleton, BonePose &source, bool local, float *offsets) override;
	};

	class SP_API ToScaleX : public ToProperty {
	public:
		RTTI_DECL

		ToScaleX() : ToProperty() {
		}
		~ToScaleX() {
		}

		float mix(TransformConstraintPose &pose) override;
		void apply(Skeleton &skeleton, TransformConstraintPose &pose, BonePose &bone, float value, bool local, bool additive) override;
	};

	class SP_API FromScaleY : public FromProperty {
	public:
		RTTI_DECL

		FromScaleY() : FromProperty() {
		}
		~FromScaleY() {
		}

		float value(Skeleton &skeleton, BonePose &source, bool local, float *offsets) override;
	};

	class SP_API ToScaleY : public ToProperty {
	public:
		RTTI_DECL

		ToScaleY() : ToProperty() {
		}
		~ToScaleY() {
		}

		float mix(TransformConstraintPose &pose) override;
		void apply(Skeleton &skeleton, TransformConstraintPose &pose, BonePose &bone, float value, bool local, bool additive) override;
	};

	class SP_API FromShearY : public FromProperty {
	public:
		RTTI_DECL

		FromShearY() : FromProperty() {
		}
		~FromShearY() {
		}

		float value(Skeleton &skeleton, BonePose &source, bool local, float *offsets) override;
	};

	class SP_API ToShearY : public ToProperty {
	public:
		RTTI_DECL

		ToShearY() : ToProperty() {
		}
		~ToShearY() {
		}

		float mix(TransformConstraintPose &pose) override;
		void apply(Skeleton &skeleton, TransformConstraintPose &pose, BonePose &bone, float value, bool local, bool additive) override;
	};

	/// Stores the setup pose for a TransformConstraint.
	///
	/// See https://esotericsoftware.com/spine-transform-constraints Transform constraints in the Spine User Guide.
	class SP_API TransformConstraintData : public ConstraintDataGeneric<TransformConstraint, TransformConstraintPose> {
	public:
		RTTI_DECL
		static const int ROTATION;
		static const int X;
		static const int Y;
		static const int SCALEX;
		static const int SCALEY;
		static const int SHEARY;
		friend class SkeletonBinary;
		friend class SkeletonJson;
		friend class TransformConstraint;
		friend class Skeleton;
		friend class TransformConstraintTimeline;

	public:
		explicit TransformConstraintData(const String &name);
		~TransformConstraintData();

		virtual Constraint &create(Skeleton &skeleton) override;

		/// The bones that will be modified by this transform constraint.
		Array<BoneData *> &getBones();

		/// The bone whose world transform will be copied to the constrained bones.
		BoneData &getSource();
		void setSource(BoneData &source);

		/// An offset added to the constrained bone rotation.
		float getOffsetRotation();
		void setOffsetRotation(float offsetRotation);

		/// An offset added to the constrained bone X translation.
		float getOffsetX();
		void setOffsetX(float offsetX);

		/// An offset added to the constrained bone Y translation.
		float getOffsetY();
		void setOffsetY(float offsetY);

		/// An offset added to the constrained bone scaleX.
		float getOffsetScaleX();
		void setOffsetScaleX(float offsetScaleX);

		/// An offset added to the constrained bone scaleY.
		float getOffsetScaleY();
		void setOffsetScaleY(float offsetScaleY);

		/// An offset added to the constrained bone shearY.
		float getOffsetShearY();
		void setOffsetShearY(float offsetShearY);

		/// Reads the source bone's local transform instead of its world transform.
		bool getLocalSource();
		void setLocalSource(bool localSource);

		/// Sets the constrained bones' local transforms instead of their world transforms.
		bool getLocalTarget();
		void setLocalTarget(bool localTarget);

		/// Adds the source bone transform to the constrained bones instead of setting it absolutely.
		bool getAdditive();
		void setAdditive(bool additive);

		/// Prevents constrained bones from exceeding the ranged defined by offset and max.
		bool getClamp();
		void setClamp(bool clamp);

		/// The mapping of transform properties to other transform properties.
		Array<FromProperty *> &getProperties();

	private:
		Array<BoneData *> _bones;
		BoneData *_source;
		float _offsets[6];// [rotation, x, y, scaleX, scaleY, shearY]
		bool _localSource, _localTarget, _additive, _clamp;
		Array<FromProperty *> _properties;
	};
}

#endif /* Spine_TransformConstraintData_h */