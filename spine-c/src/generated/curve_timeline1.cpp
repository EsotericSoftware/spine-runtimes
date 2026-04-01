#include "curve_timeline1.h"
#include <spine/spine.h>

using namespace spine;

void spine_curve_timeline1_dispose(spine_curve_timeline1 self) {
	delete (CurveTimeline1 *) self;
}

spine_rtti spine_curve_timeline1_get_rtti(spine_curve_timeline1 self) {
	CurveTimeline1 *_self = (CurveTimeline1 *) self;
	return (spine_rtti) &_self->getRTTI();
}

void spine_curve_timeline1_set_frame(spine_curve_timeline1 self, size_t frame, float time, float value) {
	CurveTimeline1 *_self = (CurveTimeline1 *) self;
	_self->setFrame(frame, time, value);
}

float spine_curve_timeline1_get_curve_value(spine_curve_timeline1 self, float time) {
	CurveTimeline1 *_self = (CurveTimeline1 *) self;
	return _self->getCurveValue(time);
}

float spine_curve_timeline1_get_relative_value(spine_curve_timeline1 self, float time, float alpha, bool fromSetup, bool add, float current,
											   float setup) {
	CurveTimeline1 *_self = (CurveTimeline1 *) self;
	return _self->getRelativeValue(time, alpha, fromSetup, add, current, setup);
}

float spine_curve_timeline1_get_absolute_value_1(spine_curve_timeline1 self, float time, float alpha, bool fromSetup, bool add, float current,
												 float setup) {
	CurveTimeline1 *_self = (CurveTimeline1 *) self;
	return _self->getAbsoluteValue(time, alpha, fromSetup, add, current, setup);
}

float spine_curve_timeline1_get_absolute_value_2(spine_curve_timeline1 self, float time, float alpha, bool fromSetup, bool add, float current,
												 float setup, float value) {
	CurveTimeline1 *_self = (CurveTimeline1 *) self;
	return _self->getAbsoluteValue(time, alpha, fromSetup, add, current, setup, value);
}

float spine_curve_timeline1_get_scale_value(spine_curve_timeline1 self, float time, float alpha, bool fromSetup, bool add, bool out, float current,
											float setup) {
	CurveTimeline1 *_self = (CurveTimeline1 *) self;
	return _self->getScaleValue(time, alpha, fromSetup, add, out, current, setup);
}

void spine_curve_timeline1_set_linear(spine_curve_timeline1 self, size_t frame) {
	CurveTimeline1 *_self = (CurveTimeline1 *) self;
	_self->setLinear(frame);
}

void spine_curve_timeline1_set_stepped(spine_curve_timeline1 self, size_t frame) {
	CurveTimeline1 *_self = (CurveTimeline1 *) self;
	_self->setStepped(frame);
}

void spine_curve_timeline1_set_bezier(spine_curve_timeline1 self, size_t bezier, size_t frame, float value, float time1, float value1, float cx1,
									  float cy1, float cx2, float cy2, float time2, float value2) {
	CurveTimeline1 *_self = (CurveTimeline1 *) self;
	_self->setBezier(bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
}

float spine_curve_timeline1_get_bezier_value(spine_curve_timeline1 self, float time, size_t frame, size_t valueOffset, size_t i) {
	CurveTimeline1 *_self = (CurveTimeline1 *) self;
	return _self->getBezierValue(time, frame, valueOffset, i);
}

spine_array_float spine_curve_timeline1_get_curves(spine_curve_timeline1 self) {
	CurveTimeline1 *_self = (CurveTimeline1 *) self;
	return (spine_array_float) &_self->getCurves();
}

void spine_curve_timeline1_apply(spine_curve_timeline1 self, spine_skeleton skeleton, float lastTime, float time, /*@null*/ spine_array_event events,
								 float alpha, bool fromSetup, bool add, bool out, bool appliedPose) {
	CurveTimeline1 *_self = (CurveTimeline1 *) self;
	_self->apply(*((Skeleton *) skeleton), lastTime, time, (Array<Event *> *) events, alpha, fromSetup, add, out, appliedPose);
}

bool spine_curve_timeline1_get_additive(spine_curve_timeline1 self) {
	CurveTimeline1 *_self = (CurveTimeline1 *) self;
	return _self->getAdditive();
}

bool spine_curve_timeline1_get_instant(spine_curve_timeline1 self) {
	CurveTimeline1 *_self = (CurveTimeline1 *) self;
	return _self->getInstant();
}

size_t spine_curve_timeline1_get_frame_entries(spine_curve_timeline1 self) {
	CurveTimeline1 *_self = (CurveTimeline1 *) self;
	return _self->getFrameEntries();
}

size_t spine_curve_timeline1_get_frame_count(spine_curve_timeline1 self) {
	CurveTimeline1 *_self = (CurveTimeline1 *) self;
	return _self->getFrameCount();
}

spine_array_float spine_curve_timeline1_get_frames(spine_curve_timeline1 self) {
	CurveTimeline1 *_self = (CurveTimeline1 *) self;
	return (spine_array_float) &_self->getFrames();
}

float spine_curve_timeline1_get_duration(spine_curve_timeline1 self) {
	CurveTimeline1 *_self = (CurveTimeline1 *) self;
	return _self->getDuration();
}

spine_array_property_id spine_curve_timeline1_get_property_ids(spine_curve_timeline1 self) {
	CurveTimeline1 *_self = (CurveTimeline1 *) self;
	return (spine_array_property_id) &_self->getPropertyIds();
}

spine_rtti spine_curve_timeline1_rtti(void) {
	return (spine_rtti) &CurveTimeline1::rtti;
}
