#include "alpha_timeline.h"
#include <spine/spine.h>

using namespace spine;

spine_alpha_timeline spine_alpha_timeline_create(size_t frameCount, size_t bezierCount, int slotIndex) {
	return (spine_alpha_timeline) new (__FILE__, __LINE__) AlphaTimeline(frameCount, bezierCount, slotIndex);
}

void spine_alpha_timeline_dispose(spine_alpha_timeline self) {
	delete (AlphaTimeline *) self;
}

spine_rtti spine_alpha_timeline_get_rtti(spine_alpha_timeline self) {
	AlphaTimeline *_self = (AlphaTimeline *) self;
	return (spine_rtti) &_self->getRTTI();
}

void spine_alpha_timeline_apply(spine_alpha_timeline self, spine_skeleton skeleton, float lastTime, float time, /*@null*/ spine_array_event events,
								float alpha, bool fromSetup, bool add, bool out, bool appliedPose) {
	AlphaTimeline *_self = (AlphaTimeline *) self;
	_self->apply(*((Skeleton *) skeleton), lastTime, time, (Array<Event *> *) events, alpha, fromSetup, add, out, appliedPose);
}

int spine_alpha_timeline_get_slot_index(spine_alpha_timeline self) {
	AlphaTimeline *_self = (AlphaTimeline *) self;
	return _self->getSlotIndex();
}

void spine_alpha_timeline_set_slot_index(spine_alpha_timeline self, int inValue) {
	AlphaTimeline *_self = (AlphaTimeline *) self;
	_self->setSlotIndex(inValue);
}

void spine_alpha_timeline_set_frame(spine_alpha_timeline self, size_t frame, float time, float value) {
	AlphaTimeline *_self = (AlphaTimeline *) self;
	_self->setFrame(frame, time, value);
}

float spine_alpha_timeline_get_curve_value(spine_alpha_timeline self, float time) {
	AlphaTimeline *_self = (AlphaTimeline *) self;
	return _self->getCurveValue(time);
}

float spine_alpha_timeline_get_relative_value(spine_alpha_timeline self, float time, float alpha, bool fromSetup, bool add, float current,
											  float setup) {
	AlphaTimeline *_self = (AlphaTimeline *) self;
	return _self->getRelativeValue(time, alpha, fromSetup, add, current, setup);
}

float spine_alpha_timeline_get_absolute_value_1(spine_alpha_timeline self, float time, float alpha, bool fromSetup, bool add, float current,
												float setup) {
	AlphaTimeline *_self = (AlphaTimeline *) self;
	return _self->getAbsoluteValue(time, alpha, fromSetup, add, current, setup);
}

float spine_alpha_timeline_get_absolute_value_2(spine_alpha_timeline self, float time, float alpha, bool fromSetup, bool add, float current,
												float setup, float value) {
	AlphaTimeline *_self = (AlphaTimeline *) self;
	return _self->getAbsoluteValue(time, alpha, fromSetup, add, current, setup, value);
}

float spine_alpha_timeline_get_scale_value(spine_alpha_timeline self, float time, float alpha, bool fromSetup, bool add, bool out, float current,
										   float setup) {
	AlphaTimeline *_self = (AlphaTimeline *) self;
	return _self->getScaleValue(time, alpha, fromSetup, add, out, current, setup);
}

void spine_alpha_timeline_set_linear(spine_alpha_timeline self, size_t frame) {
	AlphaTimeline *_self = (AlphaTimeline *) self;
	_self->setLinear(frame);
}

void spine_alpha_timeline_set_stepped(spine_alpha_timeline self, size_t frame) {
	AlphaTimeline *_self = (AlphaTimeline *) self;
	_self->setStepped(frame);
}

void spine_alpha_timeline_set_bezier(spine_alpha_timeline self, size_t bezier, size_t frame, float value, float time1, float value1, float cx1,
									 float cy1, float cx2, float cy2, float time2, float value2) {
	AlphaTimeline *_self = (AlphaTimeline *) self;
	_self->setBezier(bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
}

float spine_alpha_timeline_get_bezier_value(spine_alpha_timeline self, float time, size_t frame, size_t valueOffset, size_t i) {
	AlphaTimeline *_self = (AlphaTimeline *) self;
	return _self->getBezierValue(time, frame, valueOffset, i);
}

spine_array_float spine_alpha_timeline_get_curves(spine_alpha_timeline self) {
	AlphaTimeline *_self = (AlphaTimeline *) self;
	return (spine_array_float) &_self->getCurves();
}

bool spine_alpha_timeline_get_additive(spine_alpha_timeline self) {
	AlphaTimeline *_self = (AlphaTimeline *) self;
	return _self->getAdditive();
}

bool spine_alpha_timeline_get_instant(spine_alpha_timeline self) {
	AlphaTimeline *_self = (AlphaTimeline *) self;
	return _self->getInstant();
}

size_t spine_alpha_timeline_get_frame_entries(spine_alpha_timeline self) {
	AlphaTimeline *_self = (AlphaTimeline *) self;
	return _self->getFrameEntries();
}

size_t spine_alpha_timeline_get_frame_count(spine_alpha_timeline self) {
	AlphaTimeline *_self = (AlphaTimeline *) self;
	return _self->getFrameCount();
}

spine_array_float spine_alpha_timeline_get_frames(spine_alpha_timeline self) {
	AlphaTimeline *_self = (AlphaTimeline *) self;
	return (spine_array_float) &_self->getFrames();
}

float spine_alpha_timeline_get_duration(spine_alpha_timeline self) {
	AlphaTimeline *_self = (AlphaTimeline *) self;
	return _self->getDuration();
}

spine_array_property_id spine_alpha_timeline_get_property_ids(spine_alpha_timeline self) {
	AlphaTimeline *_self = (AlphaTimeline *) self;
	return (spine_array_property_id) &_self->getPropertyIds();
}

spine_rtti spine_alpha_timeline_rtti(void) {
	return (spine_rtti) &AlphaTimeline::rtti;
}
