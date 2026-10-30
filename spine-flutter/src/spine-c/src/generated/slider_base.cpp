#include "slider_base.h"
#include <spine/spine.h>

using namespace spine;

void spine_slider_base_dispose(spine_slider_base self) {
	delete (SliderBase *) self;
}

spine_slider_data spine_slider_base_get_data(spine_slider_base self) {
	SliderBase *_self = (SliderBase *) self;
	return (spine_slider_data) &_self->getData();
}

spine_slider_pose spine_slider_base_get_pose(spine_slider_base self) {
	SliderBase *_self = (SliderBase *) self;
	return (spine_slider_pose) &_self->getPose();
}

spine_slider_pose spine_slider_base_get_applied_pose(spine_slider_base self) {
	SliderBase *_self = (SliderBase *) self;
	return (spine_slider_pose) &_self->getAppliedPose();
}

void spine_slider_base_reset_constrained(spine_slider_base self) {
	SliderBase *_self = (SliderBase *) self;
	_self->resetConstrained();
}

void spine_slider_base_constrained(spine_slider_base self) {
	SliderBase *_self = (SliderBase *) self;
	_self->constrained();
}

bool spine_slider_base_is_pose_equal_to_applied(spine_slider_base self) {
	SliderBase *_self = (SliderBase *) self;
	return _self->isPoseEqualToApplied();
}

bool spine_slider_base_is_active(spine_slider_base self) {
	SliderBase *_self = (SliderBase *) self;
	return _self->isActive();
}

void spine_slider_base_set_active(spine_slider_base self, bool active) {
	SliderBase *_self = (SliderBase *) self;
	_self->setActive(active);
}

spine_rtti spine_slider_base_get_rtti(spine_slider_base self) {
	SliderBase *_self = (SliderBase *) self;
	return (spine_rtti) &_self->getRTTI();
}

void spine_slider_base_sort(spine_slider_base self, spine_skeleton skeleton) {
	SliderBase *_self = (SliderBase *) self;
	_self->sort(*((Skeleton *) skeleton));
}

bool spine_slider_base_is_source_active(spine_slider_base self) {
	SliderBase *_self = (SliderBase *) self;
	return _self->isSourceActive();
}

void spine_slider_base_update(spine_slider_base self, spine_skeleton skeleton, spine_physics physics) {
	SliderBase *_self = (SliderBase *) self;
	_self->update(*((Skeleton *) skeleton), (Physics) physics);
}

spine_rtti spine_slider_base_rtti(void) {
	return (spine_rtti) &SliderBase::rtti;
}
