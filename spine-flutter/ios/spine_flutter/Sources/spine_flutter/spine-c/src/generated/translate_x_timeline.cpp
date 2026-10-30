#include "translate_x_timeline.h"
#include <spine/spine.h>

using namespace spine;

spine_translate_x_timeline spine_translate_x_timeline_create(size_t frameCount, size_t bezierCount, int boneIndex) {
	return (spine_translate_x_timeline) new (__FILE__, __LINE__) TranslateXTimeline(frameCount, bezierCount, boneIndex);
}

void spine_translate_x_timeline_dispose(spine_translate_x_timeline self) {
	delete (TranslateXTimeline *) self;
}

spine_rtti spine_translate_x_timeline_get_rtti(spine_translate_x_timeline self) {
	TranslateXTimeline *_self = (TranslateXTimeline *) self;
	return (spine_rtti) &_self->getRTTI();
}

void spine_translate_x_timeline_apply(spine_translate_x_timeline self, spine_skeleton skeleton, float lastTime, float time,
									  /*@null*/ spine_array_event events, float alpha, spine_mix_from from, bool add, bool out, bool appliedPose) {
	TranslateXTimeline *_self = (TranslateXTimeline *) self;
	_self->apply(*((Skeleton *) skeleton), lastTime, time, (Array<Event *> *) events, alpha, (MixFrom) from, add, out, appliedPose);
}

int spine_translate_x_timeline_get_bone_index(spine_translate_x_timeline self) {
	TranslateXTimeline *_self = (TranslateXTimeline *) self;
	return _self->getBoneIndex();
}

void spine_translate_x_timeline_set_bone_index(spine_translate_x_timeline self, int inValue) {
	TranslateXTimeline *_self = (TranslateXTimeline *) self;
	_self->setBoneIndex(inValue);
}

void spine_translate_x_timeline_set_frame(spine_translate_x_timeline self, size_t frame, float time, float value) {
	TranslateXTimeline *_self = (TranslateXTimeline *) self;
	_self->setFrame(frame, time, value);
}

float spine_translate_x_timeline_get_curve_value(spine_translate_x_timeline self, float time) {
	TranslateXTimeline *_self = (TranslateXTimeline *) self;
	return _self->getCurveValue(time);
}

float spine_translate_x_timeline_get_relative_value(spine_translate_x_timeline self, float time, float alpha, spine_mix_from from, bool add,
													float current, float setup) {
	TranslateXTimeline *_self = (TranslateXTimeline *) self;
	return _self->getRelativeValue(time, alpha, (MixFrom) from, add, current, setup);
}

float spine_translate_x_timeline_get_absolute_value_1(spine_translate_x_timeline self, float time, float alpha, spine_mix_from from, bool add,
													  float current, float setup) {
	TranslateXTimeline *_self = (TranslateXTimeline *) self;
	return _self->getAbsoluteValue(time, alpha, (MixFrom) from, add, current, setup);
}

float spine_translate_x_timeline_get_absolute_value_2(spine_translate_x_timeline self, float time, float alpha, spine_mix_from from, bool add,
													  float current, float setup, float value) {
	TranslateXTimeline *_self = (TranslateXTimeline *) self;
	return _self->getAbsoluteValue(time, alpha, (MixFrom) from, add, current, setup, value);
}

float spine_translate_x_timeline_get_scale_value(spine_translate_x_timeline self, float time, float alpha, spine_mix_from from, bool add, bool out,
												 float current, float setup) {
	TranslateXTimeline *_self = (TranslateXTimeline *) self;
	return _self->getScaleValue(time, alpha, (MixFrom) from, add, out, current, setup);
}

float spine_translate_x_timeline_before_first_key(spine_mix_from from, float alpha, float current, float setup) {
	return TranslateXTimeline::beforeFirstKey((MixFrom) from, alpha, current, setup);
}

void spine_translate_x_timeline_set_linear(spine_translate_x_timeline self, size_t frame) {
	TranslateXTimeline *_self = (TranslateXTimeline *) self;
	_self->setLinear(frame);
}

void spine_translate_x_timeline_set_stepped(spine_translate_x_timeline self, size_t frame) {
	TranslateXTimeline *_self = (TranslateXTimeline *) self;
	_self->setStepped(frame);
}

void spine_translate_x_timeline_set_bezier(spine_translate_x_timeline self, size_t bezier, size_t frame, float value, float time1, float value1,
										   float cx1, float cy1, float cx2, float cy2, float time2, float value2) {
	TranslateXTimeline *_self = (TranslateXTimeline *) self;
	_self->setBezier(bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
}

float spine_translate_x_timeline_get_bezier_value(spine_translate_x_timeline self, float time, size_t frame, size_t valueOffset, size_t i) {
	TranslateXTimeline *_self = (TranslateXTimeline *) self;
	return _self->getBezierValue(time, frame, valueOffset, i);
}

spine_array_float spine_translate_x_timeline_get_curves(spine_translate_x_timeline self) {
	TranslateXTimeline *_self = (TranslateXTimeline *) self;
	return (spine_array_float) &_self->getCurves();
}

bool spine_translate_x_timeline_get_additive(spine_translate_x_timeline self) {
	TranslateXTimeline *_self = (TranslateXTimeline *) self;
	return _self->getAdditive();
}

bool spine_translate_x_timeline_get_instant(spine_translate_x_timeline self) {
	TranslateXTimeline *_self = (TranslateXTimeline *) self;
	return _self->getInstant();
}

size_t spine_translate_x_timeline_get_frame_entries(spine_translate_x_timeline self) {
	TranslateXTimeline *_self = (TranslateXTimeline *) self;
	return _self->getFrameEntries();
}

size_t spine_translate_x_timeline_get_frame_count(spine_translate_x_timeline self) {
	TranslateXTimeline *_self = (TranslateXTimeline *) self;
	return _self->getFrameCount();
}

spine_array_float spine_translate_x_timeline_get_frames(spine_translate_x_timeline self) {
	TranslateXTimeline *_self = (TranslateXTimeline *) self;
	return (spine_array_float) &_self->getFrames();
}

float spine_translate_x_timeline_get_duration(spine_translate_x_timeline self) {
	TranslateXTimeline *_self = (TranslateXTimeline *) self;
	return _self->getDuration();
}

spine_array_property_id spine_translate_x_timeline_get_property_ids(spine_translate_x_timeline self) {
	TranslateXTimeline *_self = (TranslateXTimeline *) self;
	return (spine_array_property_id) &_self->getPropertyIds();
}

spine_rtti spine_translate_x_timeline_rtti(void) {
	return (spine_rtti) &TranslateXTimeline::rtti;
}
