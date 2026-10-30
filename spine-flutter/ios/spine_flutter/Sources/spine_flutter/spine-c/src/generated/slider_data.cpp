#include "slider_data.h"
#include <spine/spine.h>

using namespace spine;

spine_slider_data spine_slider_data_create(const char *name) {
	return (spine_slider_data) new (__FILE__, __LINE__) SliderData(String(name));
}

void spine_slider_data_dispose(spine_slider_data self) {
	delete (SliderData *) self;
}

spine_rtti spine_slider_data_get_rtti(spine_slider_data self) {
	SliderData *_self = (SliderData *) self;
	return (spine_rtti) &_self->getRTTI();
}

spine_constraint spine_slider_data_create_method(spine_slider_data self, spine_skeleton skeleton) {
	SliderData *_self = (SliderData *) self;
	return (spine_constraint) &_self->create(*((Skeleton *) skeleton));
}

spine_animation spine_slider_data_get_animation(spine_slider_data self) {
	SliderData *_self = (SliderData *) self;
	return (spine_animation) &_self->getAnimation();
}

void spine_slider_data_set_animation(spine_slider_data self, spine_animation animation) {
	SliderData *_self = (SliderData *) self;
	_self->setAnimation(*((Animation *) animation));
}

bool spine_slider_data_get_additive(spine_slider_data self) {
	SliderData *_self = (SliderData *) self;
	return _self->getAdditive();
}

void spine_slider_data_set_additive(spine_slider_data self, bool additive) {
	SliderData *_self = (SliderData *) self;
	_self->setAdditive(additive);
}

bool spine_slider_data_get_loop(spine_slider_data self) {
	SliderData *_self = (SliderData *) self;
	return _self->getLoop();
}

void spine_slider_data_set_loop(spine_slider_data self, bool loop) {
	SliderData *_self = (SliderData *) self;
	_self->setLoop(loop);
}

/*@null*/ spine_bone_data spine_slider_data_get_bone(spine_slider_data self) {
	SliderData *_self = (SliderData *) self;
	return (spine_bone_data) _self->getBone();
}

void spine_slider_data_set_bone(spine_slider_data self, /*@null*/ spine_bone_data bone) {
	SliderData *_self = (SliderData *) self;
	_self->setBone((BoneData *) bone);
}

/*@null*/ spine_from_property spine_slider_data_get_property(spine_slider_data self) {
	SliderData *_self = (SliderData *) self;
	return (spine_from_property) _self->getProperty();
}

void spine_slider_data_set_property(spine_slider_data self, /*@null*/ spine_from_property property) {
	SliderData *_self = (SliderData *) self;
	_self->setProperty((FromProperty *) property);
}

float spine_slider_data_get_scale(spine_slider_data self) {
	SliderData *_self = (SliderData *) self;
	return _self->getScale();
}

void spine_slider_data_set_scale(spine_slider_data self, float scale) {
	SliderData *_self = (SliderData *) self;
	_self->setScale(scale);
}

float spine_slider_data_get_offset(spine_slider_data self) {
	SliderData *_self = (SliderData *) self;
	return _self->getOffset();
}

void spine_slider_data_set_offset(spine_slider_data self, float offset) {
	SliderData *_self = (SliderData *) self;
	_self->setOffset(offset);
}

float spine_slider_data_get_max(spine_slider_data self) {
	SliderData *_self = (SliderData *) self;
	return _self->getMax();
}

void spine_slider_data_set_max(spine_slider_data self, float max) {
	SliderData *_self = (SliderData *) self;
	_self->setMax(max);
}

bool spine_slider_data_get_local(spine_slider_data self) {
	SliderData *_self = (SliderData *) self;
	return _self->getLocal();
}

void spine_slider_data_set_local(spine_slider_data self, bool local) {
	SliderData *_self = (SliderData *) self;
	_self->setLocal(local);
}

const char *spine_slider_data_get_name(spine_slider_data self) {
	SliderData *_self = (SliderData *) self;
	return _self->getName().buffer();
}

bool spine_slider_data_get_skin_required(spine_slider_data self) {
	SliderData *_self = (SliderData *) self;
	return _self->getSkinRequired();
}

spine_slider_pose spine_slider_data_get_setup_pose(spine_slider_data self) {
	SliderData *_self = (SliderData *) self;
	return (spine_slider_pose) &_self->getSetupPose();
}

void spine_slider_data_set_skin_required(spine_slider_data self, bool skinRequired) {
	SliderData *_self = (SliderData *) self;
	_self->setSkinRequired(skinRequired);
}

spine_rtti spine_slider_data_rtti(void) {
	return (spine_rtti) &SliderData::rtti;
}
