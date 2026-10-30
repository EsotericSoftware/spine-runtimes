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

#include <spine/TransformConstraintData.h>
#include <spine/TransformConstraint.h>
#include <spine/BoneData.h>
#include <spine/BonePose.h>
#include <spine/TransformConstraintPose.h>
#include <spine/MathUtil.h>
#include <spine/ArrayUtils.h>
#include <spine/Skeleton.h>

using namespace spine;

// Define static constants for TransformConstraintData
const int TransformConstraintData::ROTATION = 0;
const int TransformConstraintData::X = 1;
const int TransformConstraintData::Y = 2;
const int TransformConstraintData::SCALEX = 3;
const int TransformConstraintData::SCALEY = 4;
const int TransformConstraintData::SHEARY = 5;

RTTI_IMPL(TransformConstraintData, ConstraintData)
RTTI_IMPL_NOPARENT(FromProperty)
RTTI_IMPL_NOPARENT(ToProperty)
RTTI_IMPL(FromRotate, FromProperty)
RTTI_IMPL(ToRotate, ToProperty)
RTTI_IMPL(FromX, FromProperty)
RTTI_IMPL(ToX, ToProperty)
RTTI_IMPL(FromY, FromProperty)
RTTI_IMPL(ToY, ToProperty)
RTTI_IMPL(FromScaleX, FromProperty)
RTTI_IMPL(ToScaleX, ToProperty)
RTTI_IMPL(FromScaleY, FromProperty)
RTTI_IMPL(ToScaleY, ToProperty)
RTTI_IMPL(FromShearY, FromProperty)
RTTI_IMPL(ToShearY, ToProperty)

TransformConstraintData::TransformConstraintData(const String &name)
	: ConstraintDataGeneric<TransformConstraint, TransformConstraintPose>(name), _source(NULL), _localSource(false), _localTarget(false),
	  _additive(false), _clamp(false) {
	for (int i = 0; i < 6; i++) {
		_offsets[i] = 0;
	}
}

Array<BoneData *> &TransformConstraintData::getBones() {
	return _bones;
}

BoneData &TransformConstraintData::getSource() {
	return *_source;
}

void TransformConstraintData::setSource(BoneData &source) {
	_source = &source;
}

float TransformConstraintData::getOffsetRotation() {
	return _offsets[ROTATION];
}

void TransformConstraintData::setOffsetRotation(float offsetRotation) {
	_offsets[ROTATION] = offsetRotation;
}

float TransformConstraintData::getOffsetX() {
	return _offsets[X];
}

void TransformConstraintData::setOffsetX(float offsetX) {
	_offsets[X] = offsetX;
}

float TransformConstraintData::getOffsetY() {
	return _offsets[Y];
}

void TransformConstraintData::setOffsetY(float offsetY) {
	_offsets[Y] = offsetY;
}

float TransformConstraintData::getOffsetScaleX() {
	return _offsets[SCALEX];
}

void TransformConstraintData::setOffsetScaleX(float offsetScaleX) {
	_offsets[SCALEX] = offsetScaleX;
}

float TransformConstraintData::getOffsetScaleY() {
	return _offsets[SCALEY];
}

void TransformConstraintData::setOffsetScaleY(float offsetScaleY) {
	_offsets[SCALEY] = offsetScaleY;
}

float TransformConstraintData::getOffsetShearY() {
	return _offsets[SHEARY];
}

void TransformConstraintData::setOffsetShearY(float offsetShearY) {
	_offsets[SHEARY] = offsetShearY;
}

bool TransformConstraintData::getLocalSource() {
	return _localSource;
}

void TransformConstraintData::setLocalSource(bool localSource) {
	_localSource = localSource;
}

bool TransformConstraintData::getLocalTarget() {
	return _localTarget;
}

void TransformConstraintData::setLocalTarget(bool localTarget) {
	_localTarget = localTarget;
}

bool TransformConstraintData::getAdditive() {
	return _additive;
}

void TransformConstraintData::setAdditive(bool additive) {
	_additive = additive;
}

bool TransformConstraintData::getClamp() {
	return _clamp;
}

void TransformConstraintData::setClamp(bool clamp) {
	_clamp = clamp;
}

Array<FromProperty *> &TransformConstraintData::getProperties() {
	return _properties;
}

FromProperty::FromProperty() : SpineObject(), _offset(0) {
}

FromProperty::~FromProperty() {
}

ToProperty::ToProperty() : SpineObject(), _offset(0), _max(0), _scale(1) {
}

ToProperty::~ToProperty() {
}

float FromRotate::value(Skeleton &skeleton, BonePose &source, bool local, float *offsets) {
	if (local) return source._rotation + offsets[TransformConstraintData::ROTATION];
	float sx = skeleton.getScaleX(), sy = skeleton.getScaleY();
	float value = MathUtil::atan2(source._c / sy, source._a / sx) * MathUtil::Rad_Deg +
		((source._a * source._d - source._b * source._c) * sx * sy > 0 ? offsets[TransformConstraintData::ROTATION]
																	   : -offsets[TransformConstraintData::ROTATION]);
	if (value < 0) value += 360;
	return value;
}

float ToRotate::mix(TransformConstraintPose &pose) {
	return pose._mixRotate;
}

void ToRotate::apply(Skeleton &skeleton, TransformConstraintPose &pose, BonePose &bone, float value, bool local, bool additive) {
	if (local) {
		bone._rotation += (additive ? value : value - bone._rotation) * pose._mixRotate;
	} else {
		float sx = skeleton.getScaleX(), sy = skeleton.getScaleY(), ix = 1 / sx, iy = 1 / sy;
		float a = bone._a * ix, b = bone._b * ix, c = bone._c * iy, d = bone._d * iy;
		value *= MathUtil::Deg_Rad;
		if (!additive) value -= MathUtil::atan2(c, a);
		if (value > MathUtil::Pi)
			value -= MathUtil::Pi_2;
		else if (value < -MathUtil::Pi)//
			value += MathUtil::Pi_2;
		value *= pose._mixRotate;
		float cosVal = MathUtil::cos(value), sinVal = MathUtil::sin(value);
		bone._a = (cosVal * a - sinVal * c) * sx;
		bone._b = (cosVal * b - sinVal * d) * sx;
		bone._c = (sinVal * a + cosVal * c) * sy;
		bone._d = (sinVal * b + cosVal * d) * sy;
	}
}

float FromX::value(Skeleton &skeleton, BonePose &source, bool local, float *offsets) {
	return local
		? source._x + offsets[TransformConstraintData::X]
		: (offsets[TransformConstraintData::X] * source._a + offsets[TransformConstraintData::Y] * source._b + source._worldX) / skeleton.getScaleX();
}

float ToX::mix(TransformConstraintPose &pose) {
	return pose._mixX;
}

void ToX::apply(Skeleton &skeleton, TransformConstraintPose &pose, BonePose &bone, float value, bool local, bool additive) {
	if (local) {
		bone._x += (additive ? value : value - bone._x) * pose._mixX;
	} else {
		if (!additive) value -= bone._worldX / skeleton.getScaleX();
		bone._worldX += value * pose._mixX * skeleton.getScaleX();
	}
}

float FromY::value(Skeleton &skeleton, BonePose &source, bool local, float *offsets) {
	return local
		? source._y + offsets[TransformConstraintData::Y]
		: (offsets[TransformConstraintData::X] * source._c + offsets[TransformConstraintData::Y] * source._d + source._worldY) / skeleton.getScaleY();
}

float ToY::mix(TransformConstraintPose &pose) {
	return pose._mixY;
}

void ToY::apply(Skeleton &skeleton, TransformConstraintPose &pose, BonePose &bone, float value, bool local, bool additive) {
	if (local) {
		bone._y += (additive ? value : value - bone._y) * pose._mixY;
	} else {
		if (!additive) value -= bone._worldY / skeleton.getScaleY();
		bone._worldY += value * pose._mixY * skeleton.getScaleY();
	}
}

float FromScaleX::value(Skeleton &skeleton, BonePose &source, bool local, float *offsets) {
	if (local) return source._scaleX + offsets[TransformConstraintData::SCALEX];
	float a = source._a / skeleton.getScaleX(), c = source._c / skeleton.getScaleY();
	return MathUtil::sqrt(a * a + c * c) + offsets[TransformConstraintData::SCALEX];
}

float ToScaleX::mix(TransformConstraintPose &pose) {
	return pose._mixScaleX;
}

void ToScaleX::apply(Skeleton &skeleton, TransformConstraintPose &pose, BonePose &bone, float value, bool local, bool additive) {
	if (local) {
		if (additive)
			bone._scaleX *= 1 + (value - 1) * pose._mixScaleX;
		else if (bone._scaleX != 0)//
			bone._scaleX += (value - bone._scaleX) * pose._mixScaleX;
	} else if (additive) {
		float s = 1 + (value - 1) * pose._mixScaleX;
		bone._a *= s;
		bone._c *= s;
	} else {
		float a = bone._a / skeleton.getScaleX(), c = bone._c / skeleton.getScaleY(), s = MathUtil::sqrt(a * a + c * c);
		if (s != 0) {
			s = 1 + (value - s) * pose._mixScaleX / s;
			bone._a *= s;
			bone._c *= s;
		}
	}
}

float FromScaleY::value(Skeleton &skeleton, BonePose &source, bool local, float *offsets) {
	if (local) return source._scaleY + offsets[TransformConstraintData::SCALEY];
	float b = source._b / skeleton.getScaleX(), d = source._d / skeleton.getScaleY();
	return MathUtil::sqrt(b * b + d * d) + offsets[TransformConstraintData::SCALEY];
}

float ToScaleY::mix(TransformConstraintPose &pose) {
	return pose._mixScaleY;
}

void ToScaleY::apply(Skeleton &skeleton, TransformConstraintPose &pose, BonePose &bone, float value, bool local, bool additive) {
	if (local) {
		if (additive)
			bone._scaleY *= 1 + (value - 1) * pose._mixScaleY;
		else if (bone._scaleY != 0)//
			bone._scaleY += (value - bone._scaleY) * pose._mixScaleY;
	} else if (additive) {
		float s = 1 + (value - 1) * pose._mixScaleY;
		bone._b *= s;
		bone._d *= s;
	} else {
		float b = bone._b / skeleton.getScaleX(), d = bone._d / skeleton.getScaleY(), s = MathUtil::sqrt(b * b + d * d);
		if (s != 0) {
			s = 1 + (value - s) * pose._mixScaleY / s;
			bone._b *= s;
			bone._d *= s;
		}
	}
}

float FromShearY::value(Skeleton &skeleton, BonePose &source, bool local, float *offsets) {
	if (local) return source._shearY + offsets[TransformConstraintData::SHEARY];
	float ix = 1 / skeleton.getScaleX(), iy = 1 / skeleton.getScaleY();
	return (MathUtil::atan2(source._d * iy, source._b * ix) - MathUtil::atan2(source._c * iy, source._a * ix)) * MathUtil::Rad_Deg - 90 +
		offsets[TransformConstraintData::SHEARY];
}

float ToShearY::mix(TransformConstraintPose &pose) {
	return pose._mixShearY;
}

void ToShearY::apply(Skeleton &skeleton, TransformConstraintPose &pose, BonePose &bone, float value, bool local, bool additive) {
	if (local) {
		if (!additive) value -= bone._shearY;
		bone._shearY += value * pose._mixShearY;
	} else {
		float sx = skeleton.getScaleX(), sy = skeleton.getScaleY(), b = bone._b / sx, d = bone._d / sy, by = MathUtil::atan2(d, b);
		value = (value + 90) * MathUtil::Deg_Rad;
		if (additive)
			value -= MathUtil::Pi / 2;
		else {
			value -= by - MathUtil::atan2(bone._c / sy, bone._a / sx);
			if (value > MathUtil::Pi)
				value -= MathUtil::Pi_2;
			else if (value < -MathUtil::Pi)//
				value += MathUtil::Pi_2;
		}
		value = by + value * pose._mixShearY;
		float s = MathUtil::sqrt(b * b + d * d);
		bone._b = MathUtil::cos(value) * s * sx;
		bone._d = MathUtil::sin(value) * s * sy;
	}
}

TransformConstraintData::~TransformConstraintData() {
	// Clean up all FromProperty objects, which will in turn clean up their ToProperty objects
	for (size_t i = 0; i < _properties.size(); i++) {
		FromProperty *fromProp = _properties[i];
		if (fromProp) {
			// Clean up the ToProperty objects in the FromProperty
			ArrayUtils::deleteElements(fromProp->_to);
			delete fromProp;
		}
	}
	_properties.clear();
}

Constraint &TransformConstraintData::create(Skeleton &skeleton) {
	return *(new (__FILE__, __LINE__) TransformConstraint(*this, skeleton));
}