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

#include <spine/SkeletonData.h>

#include <spine/Animation.h>
#include <spine/BoneData.h>
#include <spine/ConstraintData.h>
#include <spine/EventData.h>
#include <spine/IkConstraintData.h>
#include <spine/PathConstraintData.h>
#include <spine/PhysicsConstraintData.h>
#include <spine/Skin.h>
#include <spine/SlotData.h>
#include <spine/TransformConstraintData.h>
#include <spine/SliderData.h>

#include <spine/ArrayUtils.h>

using namespace spine;

SkeletonData::SkeletonData()
	: _name(), _defaultSkin(NULL), _x(0), _y(0), _width(0), _height(0), _referenceScale(100), _version(), _hash(), _fps(30), _imagesPath(),
	  _audioPath() {
}

SkeletonData::~SkeletonData() {
	ArrayUtils::deleteElements(_bones);
	ArrayUtils::deleteElements(_slots);
	ArrayUtils::deleteElements(_skins);

	_defaultSkin = NULL;

	ArrayUtils::deleteElements(_events);
	ArrayUtils::deleteElements(_animations);
	ArrayUtils::deleteElements(_constraints);
	for (size_t i = 0; i < _strings.size(); i++) {
		SpineExtension::free(_strings[i], __FILE__, __LINE__);
	}
}

BoneData *SkeletonData::findBone(const String &boneName) {
	return ArrayUtils::findWithName(_bones, boneName);
}

SlotData *SkeletonData::findSlot(const String &slotName) {
	return ArrayUtils::findWithName(_slots, slotName);
}

Skin *SkeletonData::findSkin(const String &skinName) {
	return ArrayUtils::findWithName(_skins, skinName);
}

EventData *SkeletonData::findEvent(const String &eventDataName) {
	return ArrayUtils::findWithName(_events, eventDataName);
}

Animation *SkeletonData::findAnimation(const String &animationName) {
	return ArrayUtils::findWithName(_animations, animationName);
}

Array<Animation *> &SkeletonData::findSliderAnimations(Array<Animation *> &animations) {
	for (size_t i = 0, n = _constraints.size(); i < n; i++) {
		ConstraintData *constraint = _constraints[i];
		if (constraint->getRTTI().instanceOf(SliderData::rtti)) {
			SliderData *data = static_cast<SliderData *>(constraint);
			if (data->_animation != NULL) animations.add(data->_animation);
		}
	}
	return animations;
}

const String &SkeletonData::getName() {
	return _name;
}

void SkeletonData::setName(const String &inValue) {
	_name = inValue;
}

Array<BoneData *> &SkeletonData::getBones() {
	return _bones;
}

Array<SlotData *> &SkeletonData::getSlots() {
	return _slots;
}

Array<Skin *> &SkeletonData::getSkins() {
	return _skins;
}

Skin *SkeletonData::getDefaultSkin() {
	return _defaultSkin;
}

void SkeletonData::setDefaultSkin(Skin *inValue) {
	_defaultSkin = inValue;
}

Array<EventData *> &SkeletonData::getEvents() {
	return _events;
}

Array<Animation *> &SkeletonData::getAnimations() {
	return _animations;
}

float SkeletonData::getX() {
	return _x;
}

void SkeletonData::setX(float inValue) {
	_x = inValue;
}

float SkeletonData::getY() {
	return _y;
}

void SkeletonData::setY(float inValue) {
	_y = inValue;
}

float SkeletonData::getWidth() {
	return _width;
}

void SkeletonData::setWidth(float inValue) {
	_width = inValue;
}

float SkeletonData::getHeight() {
	return _height;
}

void SkeletonData::setHeight(float inValue) {
	_height = inValue;
}

float SkeletonData::getReferenceScale() {
	return _referenceScale;
}

void SkeletonData::setReferenceScale(float inValue) {
	_referenceScale = inValue;
}

const String &SkeletonData::getVersion() {
	return _version;
}

void SkeletonData::setVersion(const String &inValue) {
	_version = inValue;
}

const String &SkeletonData::getHash() {
	return _hash;
}

void SkeletonData::setHash(const String &inValue) {
	_hash = inValue;
}

const String &SkeletonData::getImagesPath() {
	return _imagesPath;
}

void SkeletonData::setImagesPath(const String &inValue) {
	_imagesPath = inValue;
}


const String &SkeletonData::getAudioPath() {
	return _audioPath;
}

void SkeletonData::setAudioPath(const String &inValue) {
	_audioPath = inValue;
}

float SkeletonData::getFps() {
	return _fps;
}

void SkeletonData::setFps(float inValue) {
	_fps = inValue;
}

Array<ConstraintData *> &SkeletonData::getConstraints() {
	return _constraints;
}
