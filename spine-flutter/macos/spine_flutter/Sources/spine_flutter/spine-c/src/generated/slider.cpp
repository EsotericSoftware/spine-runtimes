#include "slider.h"
#include <spine/spine.h>

using namespace spine;

spine_slider spine_slider_create(spine_slider_data data, spine_skeleton skeleton) {
	return (spine_slider) new (__FILE__, __LINE__) Slider(*((SliderData *) data), *((Skeleton *) skeleton));
}

void spine_slider_dispose(spine_slider self) {
	delete (Slider *) self;
}

spine_rtti spine_slider_get_rtti(spine_slider self) {
	Slider *_self = (Slider *) self;
	return (spine_rtti) &_self->getRTTI();
}

spine_slider spine_slider_copy(spine_slider self, spine_skeleton skeleton) {
	Slider *_self = (Slider *) self;
	return (spine_slider) &_self->copy(*((Skeleton *) skeleton));
}

void spine_slider_update(spine_slider self, spine_skeleton skeleton, spine_physics physics) {
	Slider *_self = (Slider *) self;
	_self->update(*((Skeleton *) skeleton), (Physics) physics);
}

void spine_slider_sort(spine_slider self, spine_skeleton skeleton) {
	Slider *_self = (Slider *) self;
	_self->sort(*((Skeleton *) skeleton));
}

bool spine_slider_is_source_active(spine_slider self) {
	Slider *_self = (Slider *) self;
	return _self->isSourceActive();
}

spine_bone spine_slider_get_bone(spine_slider self) {
	Slider *_self = (Slider *) self;
	return (spine_bone) &_self->getBone();
}

void spine_slider_set_bone(spine_slider self, spine_bone bone) {
	Slider *_self = (Slider *) self;
	_self->setBone(*((Bone *) bone));
}

spine_slider_data spine_slider_get_data(spine_slider self) {
	Slider *_self = (Slider *) self;
	return (spine_slider_data) &_self->getData();
}

spine_slider_pose spine_slider_get_pose(spine_slider self) {
	Slider *_self = (Slider *) self;
	return (spine_slider_pose) &_self->getPose();
}

spine_slider_pose spine_slider_get_applied_pose(spine_slider self) {
	Slider *_self = (Slider *) self;
	return (spine_slider_pose) &_self->getAppliedPose();
}

void spine_slider_reset_constrained(spine_slider self) {
	Slider *_self = (Slider *) self;
	_self->resetConstrained();
}

void spine_slider_constrained(spine_slider self) {
	Slider *_self = (Slider *) self;
	_self->constrained();
}

bool spine_slider_is_pose_equal_to_applied(spine_slider self) {
	Slider *_self = (Slider *) self;
	return _self->isPoseEqualToApplied();
}

bool spine_slider_is_active(spine_slider self) {
	Slider *_self = (Slider *) self;
	return _self->isActive();
}

void spine_slider_set_active(spine_slider self, bool active) {
	Slider *_self = (Slider *) self;
	_self->setActive(active);
}

spine_rtti spine_slider_rtti(void) {
	return (spine_rtti) &Slider::rtti;
}
