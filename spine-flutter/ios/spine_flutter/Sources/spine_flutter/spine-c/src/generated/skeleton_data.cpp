#include "skeleton_data.h"
#include <spine/spine.h>

using namespace spine;

spine_skeleton_data spine_skeleton_data_create(void) {
	return (spine_skeleton_data) new (__FILE__, __LINE__) SkeletonData();
}

void spine_skeleton_data_dispose(spine_skeleton_data self) {
	delete (SkeletonData *) self;
}

/*@null*/ spine_bone_data spine_skeleton_data_find_bone(spine_skeleton_data self, const char *boneName) {
	SkeletonData *_self = (SkeletonData *) self;
	return (spine_bone_data) _self->findBone(String(boneName));
}

/*@null*/ spine_slot_data spine_skeleton_data_find_slot(spine_skeleton_data self, const char *slotName) {
	SkeletonData *_self = (SkeletonData *) self;
	return (spine_slot_data) _self->findSlot(String(slotName));
}

/*@null*/ spine_skin spine_skeleton_data_find_skin(spine_skeleton_data self, const char *skinName) {
	SkeletonData *_self = (SkeletonData *) self;
	return (spine_skin) _self->findSkin(String(skinName));
}

/*@null*/ spine_event_data spine_skeleton_data_find_event(spine_skeleton_data self, const char *eventDataName) {
	SkeletonData *_self = (SkeletonData *) self;
	return (spine_event_data) _self->findEvent(String(eventDataName));
}

/*@null*/ spine_animation spine_skeleton_data_find_animation(spine_skeleton_data self, const char *animationName) {
	SkeletonData *_self = (SkeletonData *) self;
	return (spine_animation) _self->findAnimation(String(animationName));
}

spine_array_animation spine_skeleton_data_find_slider_animations(spine_skeleton_data self, spine_array_animation animations) {
	SkeletonData *_self = (SkeletonData *) self;
	return (spine_array_animation) &_self->findSliderAnimations(*((Array<Animation *> *) animations));
}

const char *spine_skeleton_data_get_name(spine_skeleton_data self) {
	SkeletonData *_self = (SkeletonData *) self;
	return _self->getName().buffer();
}

void spine_skeleton_data_set_name(spine_skeleton_data self, const char *inValue) {
	SkeletonData *_self = (SkeletonData *) self;
	_self->setName(String(inValue));
}

spine_array_bone_data spine_skeleton_data_get_bones(spine_skeleton_data self) {
	SkeletonData *_self = (SkeletonData *) self;
	return (spine_array_bone_data) &_self->getBones();
}

spine_array_slot_data spine_skeleton_data_get_slots(spine_skeleton_data self) {
	SkeletonData *_self = (SkeletonData *) self;
	return (spine_array_slot_data) &_self->getSlots();
}

spine_array_skin spine_skeleton_data_get_skins(spine_skeleton_data self) {
	SkeletonData *_self = (SkeletonData *) self;
	return (spine_array_skin) &_self->getSkins();
}

/*@null*/ spine_skin spine_skeleton_data_get_default_skin(spine_skeleton_data self) {
	SkeletonData *_self = (SkeletonData *) self;
	return (spine_skin) _self->getDefaultSkin();
}

void spine_skeleton_data_set_default_skin(spine_skeleton_data self, /*@null*/ spine_skin inValue) {
	SkeletonData *_self = (SkeletonData *) self;
	_self->setDefaultSkin((Skin *) inValue);
}

spine_array_event_data spine_skeleton_data_get_events(spine_skeleton_data self) {
	SkeletonData *_self = (SkeletonData *) self;
	return (spine_array_event_data) &_self->getEvents();
}

spine_array_animation spine_skeleton_data_get_animations(spine_skeleton_data self) {
	SkeletonData *_self = (SkeletonData *) self;
	return (spine_array_animation) &_self->getAnimations();
}

spine_array_constraint_data spine_skeleton_data_get_constraints(spine_skeleton_data self) {
	SkeletonData *_self = (SkeletonData *) self;
	return (spine_array_constraint_data) &_self->getConstraints();
}

float spine_skeleton_data_get_x(spine_skeleton_data self) {
	SkeletonData *_self = (SkeletonData *) self;
	return _self->getX();
}

void spine_skeleton_data_set_x(spine_skeleton_data self, float inValue) {
	SkeletonData *_self = (SkeletonData *) self;
	_self->setX(inValue);
}

float spine_skeleton_data_get_y(spine_skeleton_data self) {
	SkeletonData *_self = (SkeletonData *) self;
	return _self->getY();
}

void spine_skeleton_data_set_y(spine_skeleton_data self, float inValue) {
	SkeletonData *_self = (SkeletonData *) self;
	_self->setY(inValue);
}

float spine_skeleton_data_get_width(spine_skeleton_data self) {
	SkeletonData *_self = (SkeletonData *) self;
	return _self->getWidth();
}

void spine_skeleton_data_set_width(spine_skeleton_data self, float inValue) {
	SkeletonData *_self = (SkeletonData *) self;
	_self->setWidth(inValue);
}

float spine_skeleton_data_get_height(spine_skeleton_data self) {
	SkeletonData *_self = (SkeletonData *) self;
	return _self->getHeight();
}

void spine_skeleton_data_set_height(spine_skeleton_data self, float inValue) {
	SkeletonData *_self = (SkeletonData *) self;
	_self->setHeight(inValue);
}

float spine_skeleton_data_get_reference_scale(spine_skeleton_data self) {
	SkeletonData *_self = (SkeletonData *) self;
	return _self->getReferenceScale();
}

void spine_skeleton_data_set_reference_scale(spine_skeleton_data self, float inValue) {
	SkeletonData *_self = (SkeletonData *) self;
	_self->setReferenceScale(inValue);
}

const char *spine_skeleton_data_get_version(spine_skeleton_data self) {
	SkeletonData *_self = (SkeletonData *) self;
	return _self->getVersion().buffer();
}

void spine_skeleton_data_set_version(spine_skeleton_data self, const char *inValue) {
	SkeletonData *_self = (SkeletonData *) self;
	_self->setVersion(String(inValue));
}

const char *spine_skeleton_data_get_hash(spine_skeleton_data self) {
	SkeletonData *_self = (SkeletonData *) self;
	return _self->getHash().buffer();
}

void spine_skeleton_data_set_hash(spine_skeleton_data self, const char *inValue) {
	SkeletonData *_self = (SkeletonData *) self;
	_self->setHash(String(inValue));
}

const char *spine_skeleton_data_get_images_path(spine_skeleton_data self) {
	SkeletonData *_self = (SkeletonData *) self;
	return _self->getImagesPath().buffer();
}

void spine_skeleton_data_set_images_path(spine_skeleton_data self, const char *inValue) {
	SkeletonData *_self = (SkeletonData *) self;
	_self->setImagesPath(String(inValue));
}

const char *spine_skeleton_data_get_audio_path(spine_skeleton_data self) {
	SkeletonData *_self = (SkeletonData *) self;
	return _self->getAudioPath().buffer();
}

void spine_skeleton_data_set_audio_path(spine_skeleton_data self, const char *inValue) {
	SkeletonData *_self = (SkeletonData *) self;
	_self->setAudioPath(String(inValue));
}

float spine_skeleton_data_get_fps(spine_skeleton_data self) {
	SkeletonData *_self = (SkeletonData *) self;
	return _self->getFps();
}

void spine_skeleton_data_set_fps(spine_skeleton_data self, float inValue) {
	SkeletonData *_self = (SkeletonData *) self;
	_self->setFps(inValue);
}
