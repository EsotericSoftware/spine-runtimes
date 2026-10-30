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

#include <spine/SliderData.h>
#include <spine/Slider.h>
#include <spine/Skeleton.h>
#include <spine/ArrayUtils.h>
#include <spine/TransformConstraintData.h>

using namespace spine;

RTTI_IMPL(SliderData, ConstraintData)

SliderData::SliderData(const String &name)
	: ConstraintDataGeneric<Slider, SliderPose>(name), _animation(NULL), _additive(false), _loop(false), _bone(NULL), _property(NULL), _offset(0.0f),
	  _scale(0.0f), _max(0.0f), _local(false) {
}

SliderData::~SliderData() {
	if (_property) {
		ArrayUtils::deleteElements(_property->_to);
		delete _property;
		_property = NULL;
	}
}

Constraint &SliderData::create(Skeleton &skeleton) {
	return *(new (__FILE__, __LINE__) Slider(*this, skeleton));
}

Animation &SliderData::getAnimation() {
	return *_animation;
}

void SliderData::setAnimation(Animation &animation) {
	_animation = &animation;
}

bool SliderData::getAdditive() {
	return _additive;
}

void SliderData::setAdditive(bool additive) {
	_additive = additive;
}

bool SliderData::getLoop() {
	return _loop;
}

void SliderData::setLoop(bool loop) {
	_loop = loop;
}

BoneData *SliderData::getBone() {
	return _bone;
}

void SliderData::setBone(BoneData *bone) {
	_bone = bone;
}

FromProperty *SliderData::getProperty() {
	return _property;
}

void SliderData::setProperty(FromProperty *property) {
	_property = property;
}

float SliderData::getScale() {
	return _scale;
}

void SliderData::setScale(float scale) {
	_scale = scale;
}

float SliderData::getOffset() {
	return _offset;
}

void SliderData::setOffset(float offset) {
	_offset = offset;
}

float SliderData::getMax() {
	return _max;
}

void SliderData::setMax(float max) {
	_max = max;
}

bool SliderData::getLocal() {
	return _local;
}

void SliderData::setLocal(bool local) {
	_local = local;
}