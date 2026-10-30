#include "slider_timeline.h"
#include <spine/spine.h>

using namespace spine;

spine_slider_timeline spine_slider_timeline_create(size_t frameCount, size_t bezierCount, int sliderIndex) {
	return (spine_slider_timeline) new (__FILE__, __LINE__) SliderTimeline(frameCount, bezierCount, sliderIndex);
}

void spine_slider_timeline_dispose(spine_slider_timeline self) {
	delete (SliderTimeline *) self;
}

spine_rtti spine_slider_timeline_get_rtti(spine_slider_timeline self) {
	SliderTimeline *_self = (SliderTimeline *) self;
	return (spine_rtti) &_self->getRTTI();
}

void spine_slider_timeline_apply(spine_slider_timeline self, spine_skeleton skeleton, float lastTime, float time, /*@null*/ spine_array_event events,
								 float alpha, spine_mix_from from, bool add, bool out, bool appliedPose) {
	SliderTimeline *_self = (SliderTimeline *) self;
	_self->apply(*((Skeleton *) skeleton), lastTime, time, (Array<Event *> *) events, alpha, (MixFrom) from, add, out, appliedPose);
}

int spine_slider_timeline_get_constraint_index(spine_slider_timeline self) {
	SliderTimeline *_self = (SliderTimeline *) self;
	return _self->getConstraintIndex();
}

void spine_slider_timeline_set_constraint_index(spine_slider_timeline self, int inValue) {
	SliderTimeline *_self = (SliderTimeline *) self;
	_self->setConstraintIndex(inValue);
}

void spine_slider_timeline_set_frame(spine_slider_timeline self, size_t frame, float time, float value) {
	SliderTimeline *_self = (SliderTimeline *) self;
	_self->setFrame(frame, time, value);
}

float spine_slider_timeline_get_curve_value(spine_slider_timeline self, float time) {
	SliderTimeline *_self = (SliderTimeline *) self;
	return _self->getCurveValue(time);
}

float spine_slider_timeline_get_relative_value(spine_slider_timeline self, float time, float alpha, spine_mix_from from, bool add, float current,
											   float setup) {
	SliderTimeline *_self = (SliderTimeline *) self;
	return _self->getRelativeValue(time, alpha, (MixFrom) from, add, current, setup);
}

float spine_slider_timeline_get_absolute_value_1(spine_slider_timeline self, float time, float alpha, spine_mix_from from, bool add, float current,
												 float setup) {
	SliderTimeline *_self = (SliderTimeline *) self;
	return _self->getAbsoluteValue(time, alpha, (MixFrom) from, add, current, setup);
}

float spine_slider_timeline_get_absolute_value_2(spine_slider_timeline self, float time, float alpha, spine_mix_from from, bool add, float current,
												 float setup, float value) {
	SliderTimeline *_self = (SliderTimeline *) self;
	return _self->getAbsoluteValue(time, alpha, (MixFrom) from, add, current, setup, value);
}

float spine_slider_timeline_get_scale_value(spine_slider_timeline self, float time, float alpha, spine_mix_from from, bool add, bool out,
											float current, float setup) {
	SliderTimeline *_self = (SliderTimeline *) self;
	return _self->getScaleValue(time, alpha, (MixFrom) from, add, out, current, setup);
}

float spine_slider_timeline_before_first_key(spine_mix_from from, float alpha, float current, float setup) {
	return SliderTimeline::beforeFirstKey((MixFrom) from, alpha, current, setup);
}

void spine_slider_timeline_set_linear(spine_slider_timeline self, size_t frame) {
	SliderTimeline *_self = (SliderTimeline *) self;
	_self->setLinear(frame);
}

void spine_slider_timeline_set_stepped(spine_slider_timeline self, size_t frame) {
	SliderTimeline *_self = (SliderTimeline *) self;
	_self->setStepped(frame);
}

void spine_slider_timeline_set_bezier(spine_slider_timeline self, size_t bezier, size_t frame, float value, float time1, float value1, float cx1,
									  float cy1, float cx2, float cy2, float time2, float value2) {
	SliderTimeline *_self = (SliderTimeline *) self;
	_self->setBezier(bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
}

float spine_slider_timeline_get_bezier_value(spine_slider_timeline self, float time, size_t frame, size_t valueOffset, size_t i) {
	SliderTimeline *_self = (SliderTimeline *) self;
	return _self->getBezierValue(time, frame, valueOffset, i);
}

spine_array_float spine_slider_timeline_get_curves(spine_slider_timeline self) {
	SliderTimeline *_self = (SliderTimeline *) self;
	return (spine_array_float) &_self->getCurves();
}

bool spine_slider_timeline_get_additive(spine_slider_timeline self) {
	SliderTimeline *_self = (SliderTimeline *) self;
	return _self->getAdditive();
}

bool spine_slider_timeline_get_instant(spine_slider_timeline self) {
	SliderTimeline *_self = (SliderTimeline *) self;
	return _self->getInstant();
}

size_t spine_slider_timeline_get_frame_entries(spine_slider_timeline self) {
	SliderTimeline *_self = (SliderTimeline *) self;
	return _self->getFrameEntries();
}

size_t spine_slider_timeline_get_frame_count(spine_slider_timeline self) {
	SliderTimeline *_self = (SliderTimeline *) self;
	return _self->getFrameCount();
}

spine_array_float spine_slider_timeline_get_frames(spine_slider_timeline self) {
	SliderTimeline *_self = (SliderTimeline *) self;
	return (spine_array_float) &_self->getFrames();
}

float spine_slider_timeline_get_duration(spine_slider_timeline self) {
	SliderTimeline *_self = (SliderTimeline *) self;
	return _self->getDuration();
}

spine_array_property_id spine_slider_timeline_get_property_ids(spine_slider_timeline self) {
	SliderTimeline *_self = (SliderTimeline *) self;
	return (spine_array_property_id) &_self->getPropertyIds();
}

spine_rtti spine_slider_timeline_rtti(void) {
	return (spine_rtti) &SliderTimeline::rtti;
}
