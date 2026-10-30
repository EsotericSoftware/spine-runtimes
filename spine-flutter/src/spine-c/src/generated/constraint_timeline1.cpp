#include "constraint_timeline1.h"
#include <spine/spine.h>

using namespace spine;

void spine_constraint_timeline1_dispose(spine_constraint_timeline1 self) {
	delete (ConstraintTimeline1 *) self;
}

spine_rtti spine_constraint_timeline1_get_rtti(spine_constraint_timeline1 self) {
	ConstraintTimeline1 *_self = (ConstraintTimeline1 *) self;
	return (spine_rtti) &_self->getRTTI();
}

int spine_constraint_timeline1_get_constraint_index(spine_constraint_timeline1 self) {
	ConstraintTimeline1 *_self = (ConstraintTimeline1 *) self;
	return _self->getConstraintIndex();
}

void spine_constraint_timeline1_set_constraint_index(spine_constraint_timeline1 self, int inValue) {
	ConstraintTimeline1 *_self = (ConstraintTimeline1 *) self;
	_self->setConstraintIndex(inValue);
}

void spine_constraint_timeline1_set_frame(spine_constraint_timeline1 self, size_t frame, float time, float value) {
	ConstraintTimeline1 *_self = (ConstraintTimeline1 *) self;
	_self->setFrame(frame, time, value);
}

float spine_constraint_timeline1_get_curve_value(spine_constraint_timeline1 self, float time) {
	ConstraintTimeline1 *_self = (ConstraintTimeline1 *) self;
	return _self->getCurveValue(time);
}

float spine_constraint_timeline1_get_relative_value(spine_constraint_timeline1 self, float time, float alpha, spine_mix_from from, bool add,
													float current, float setup) {
	ConstraintTimeline1 *_self = (ConstraintTimeline1 *) self;
	return _self->getRelativeValue(time, alpha, (MixFrom) from, add, current, setup);
}

float spine_constraint_timeline1_get_absolute_value_1(spine_constraint_timeline1 self, float time, float alpha, spine_mix_from from, bool add,
													  float current, float setup) {
	ConstraintTimeline1 *_self = (ConstraintTimeline1 *) self;
	return _self->getAbsoluteValue(time, alpha, (MixFrom) from, add, current, setup);
}

float spine_constraint_timeline1_get_absolute_value_2(spine_constraint_timeline1 self, float time, float alpha, spine_mix_from from, bool add,
													  float current, float setup, float value) {
	ConstraintTimeline1 *_self = (ConstraintTimeline1 *) self;
	return _self->getAbsoluteValue(time, alpha, (MixFrom) from, add, current, setup, value);
}

float spine_constraint_timeline1_get_scale_value(spine_constraint_timeline1 self, float time, float alpha, spine_mix_from from, bool add, bool out,
												 float current, float setup) {
	ConstraintTimeline1 *_self = (ConstraintTimeline1 *) self;
	return _self->getScaleValue(time, alpha, (MixFrom) from, add, out, current, setup);
}

float spine_constraint_timeline1_before_first_key(spine_mix_from from, float alpha, float current, float setup) {
	return ConstraintTimeline1::beforeFirstKey((MixFrom) from, alpha, current, setup);
}

void spine_constraint_timeline1_set_linear(spine_constraint_timeline1 self, size_t frame) {
	ConstraintTimeline1 *_self = (ConstraintTimeline1 *) self;
	_self->setLinear(frame);
}

void spine_constraint_timeline1_set_stepped(spine_constraint_timeline1 self, size_t frame) {
	ConstraintTimeline1 *_self = (ConstraintTimeline1 *) self;
	_self->setStepped(frame);
}

void spine_constraint_timeline1_set_bezier(spine_constraint_timeline1 self, size_t bezier, size_t frame, float value, float time1, float value1,
										   float cx1, float cy1, float cx2, float cy2, float time2, float value2) {
	ConstraintTimeline1 *_self = (ConstraintTimeline1 *) self;
	_self->setBezier(bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
}

float spine_constraint_timeline1_get_bezier_value(spine_constraint_timeline1 self, float time, size_t frame, size_t valueOffset, size_t i) {
	ConstraintTimeline1 *_self = (ConstraintTimeline1 *) self;
	return _self->getBezierValue(time, frame, valueOffset, i);
}

spine_array_float spine_constraint_timeline1_get_curves(spine_constraint_timeline1 self) {
	ConstraintTimeline1 *_self = (ConstraintTimeline1 *) self;
	return (spine_array_float) &_self->getCurves();
}

void spine_constraint_timeline1_apply(spine_constraint_timeline1 self, spine_skeleton skeleton, float lastTime, float time,
									  /*@null*/ spine_array_event events, float alpha, spine_mix_from from, bool add, bool out, bool appliedPose) {
	ConstraintTimeline1 *_self = (ConstraintTimeline1 *) self;
	_self->apply(*((Skeleton *) skeleton), lastTime, time, (Array<Event *> *) events, alpha, (MixFrom) from, add, out, appliedPose);
}

bool spine_constraint_timeline1_get_additive(spine_constraint_timeline1 self) {
	ConstraintTimeline1 *_self = (ConstraintTimeline1 *) self;
	return _self->getAdditive();
}

bool spine_constraint_timeline1_get_instant(spine_constraint_timeline1 self) {
	ConstraintTimeline1 *_self = (ConstraintTimeline1 *) self;
	return _self->getInstant();
}

size_t spine_constraint_timeline1_get_frame_entries(spine_constraint_timeline1 self) {
	ConstraintTimeline1 *_self = (ConstraintTimeline1 *) self;
	return _self->getFrameEntries();
}

size_t spine_constraint_timeline1_get_frame_count(spine_constraint_timeline1 self) {
	ConstraintTimeline1 *_self = (ConstraintTimeline1 *) self;
	return _self->getFrameCount();
}

spine_array_float spine_constraint_timeline1_get_frames(spine_constraint_timeline1 self) {
	ConstraintTimeline1 *_self = (ConstraintTimeline1 *) self;
	return (spine_array_float) &_self->getFrames();
}

float spine_constraint_timeline1_get_duration(spine_constraint_timeline1 self) {
	ConstraintTimeline1 *_self = (ConstraintTimeline1 *) self;
	return _self->getDuration();
}

spine_array_property_id spine_constraint_timeline1_get_property_ids(spine_constraint_timeline1 self) {
	ConstraintTimeline1 *_self = (ConstraintTimeline1 *) self;
	return (spine_array_property_id) &_self->getPropertyIds();
}

spine_rtti spine_constraint_timeline1_rtti(void) {
	return (spine_rtti) &ConstraintTimeline1::rtti;
}
