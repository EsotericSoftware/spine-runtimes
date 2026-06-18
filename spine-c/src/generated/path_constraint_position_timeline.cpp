#include "path_constraint_position_timeline.h"
#include <spine/spine.h>

using namespace spine;

spine_path_constraint_position_timeline spine_path_constraint_position_timeline_create(size_t frameCount, size_t bezierCount, int constraintIndex) {
	return (spine_path_constraint_position_timeline) new (__FILE__, __LINE__)
		PathConstraintPositionTimeline(frameCount, bezierCount, constraintIndex);
}

void spine_path_constraint_position_timeline_dispose(spine_path_constraint_position_timeline self) {
	delete (PathConstraintPositionTimeline *) self;
}

spine_rtti spine_path_constraint_position_timeline_get_rtti(spine_path_constraint_position_timeline self) {
	PathConstraintPositionTimeline *_self = (PathConstraintPositionTimeline *) self;
	return (spine_rtti) &_self->getRTTI();
}

void spine_path_constraint_position_timeline_apply(spine_path_constraint_position_timeline self, spine_skeleton skeleton, float lastTime, float time,
												   /*@null*/ spine_array_event events, float alpha, spine_mix_from from, bool add, bool out,
												   bool appliedPose) {
	PathConstraintPositionTimeline *_self = (PathConstraintPositionTimeline *) self;
	_self->apply(*((Skeleton *) skeleton), lastTime, time, (Array<Event *> *) events, alpha, (MixFrom) from, add, out, appliedPose);
}

int spine_path_constraint_position_timeline_get_constraint_index(spine_path_constraint_position_timeline self) {
	PathConstraintPositionTimeline *_self = (PathConstraintPositionTimeline *) self;
	return _self->getConstraintIndex();
}

void spine_path_constraint_position_timeline_set_constraint_index(spine_path_constraint_position_timeline self, int inValue) {
	PathConstraintPositionTimeline *_self = (PathConstraintPositionTimeline *) self;
	_self->setConstraintIndex(inValue);
}

void spine_path_constraint_position_timeline_set_frame(spine_path_constraint_position_timeline self, size_t frame, float time, float value) {
	PathConstraintPositionTimeline *_self = (PathConstraintPositionTimeline *) self;
	_self->setFrame(frame, time, value);
}

float spine_path_constraint_position_timeline_get_curve_value(spine_path_constraint_position_timeline self, float time) {
	PathConstraintPositionTimeline *_self = (PathConstraintPositionTimeline *) self;
	return _self->getCurveValue(time);
}

float spine_path_constraint_position_timeline_get_relative_value(spine_path_constraint_position_timeline self, float time, float alpha,
																 spine_mix_from from, bool add, float current, float setup) {
	PathConstraintPositionTimeline *_self = (PathConstraintPositionTimeline *) self;
	return _self->getRelativeValue(time, alpha, (MixFrom) from, add, current, setup);
}

float spine_path_constraint_position_timeline_get_absolute_value_1(spine_path_constraint_position_timeline self, float time, float alpha,
																   spine_mix_from from, bool add, float current, float setup) {
	PathConstraintPositionTimeline *_self = (PathConstraintPositionTimeline *) self;
	return _self->getAbsoluteValue(time, alpha, (MixFrom) from, add, current, setup);
}

float spine_path_constraint_position_timeline_get_absolute_value_2(spine_path_constraint_position_timeline self, float time, float alpha,
																   spine_mix_from from, bool add, float current, float setup, float value) {
	PathConstraintPositionTimeline *_self = (PathConstraintPositionTimeline *) self;
	return _self->getAbsoluteValue(time, alpha, (MixFrom) from, add, current, setup, value);
}

float spine_path_constraint_position_timeline_get_scale_value(spine_path_constraint_position_timeline self, float time, float alpha,
															  spine_mix_from from, bool add, bool out, float current, float setup) {
	PathConstraintPositionTimeline *_self = (PathConstraintPositionTimeline *) self;
	return _self->getScaleValue(time, alpha, (MixFrom) from, add, out, current, setup);
}

float spine_path_constraint_position_timeline_before_first_key(spine_mix_from from, float alpha, float current, float setup) {
	return PathConstraintPositionTimeline::beforeFirstKey((MixFrom) from, alpha, current, setup);
}

void spine_path_constraint_position_timeline_set_linear(spine_path_constraint_position_timeline self, size_t frame) {
	PathConstraintPositionTimeline *_self = (PathConstraintPositionTimeline *) self;
	_self->setLinear(frame);
}

void spine_path_constraint_position_timeline_set_stepped(spine_path_constraint_position_timeline self, size_t frame) {
	PathConstraintPositionTimeline *_self = (PathConstraintPositionTimeline *) self;
	_self->setStepped(frame);
}

void spine_path_constraint_position_timeline_set_bezier(spine_path_constraint_position_timeline self, size_t bezier, size_t frame, float value,
														float time1, float value1, float cx1, float cy1, float cx2, float cy2, float time2,
														float value2) {
	PathConstraintPositionTimeline *_self = (PathConstraintPositionTimeline *) self;
	_self->setBezier(bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
}

float spine_path_constraint_position_timeline_get_bezier_value(spine_path_constraint_position_timeline self, float time, size_t frame,
															   size_t valueOffset, size_t i) {
	PathConstraintPositionTimeline *_self = (PathConstraintPositionTimeline *) self;
	return _self->getBezierValue(time, frame, valueOffset, i);
}

spine_array_float spine_path_constraint_position_timeline_get_curves(spine_path_constraint_position_timeline self) {
	PathConstraintPositionTimeline *_self = (PathConstraintPositionTimeline *) self;
	return (spine_array_float) &_self->getCurves();
}

bool spine_path_constraint_position_timeline_get_additive(spine_path_constraint_position_timeline self) {
	PathConstraintPositionTimeline *_self = (PathConstraintPositionTimeline *) self;
	return _self->getAdditive();
}

bool spine_path_constraint_position_timeline_get_instant(spine_path_constraint_position_timeline self) {
	PathConstraintPositionTimeline *_self = (PathConstraintPositionTimeline *) self;
	return _self->getInstant();
}

size_t spine_path_constraint_position_timeline_get_frame_entries(spine_path_constraint_position_timeline self) {
	PathConstraintPositionTimeline *_self = (PathConstraintPositionTimeline *) self;
	return _self->getFrameEntries();
}

size_t spine_path_constraint_position_timeline_get_frame_count(spine_path_constraint_position_timeline self) {
	PathConstraintPositionTimeline *_self = (PathConstraintPositionTimeline *) self;
	return _self->getFrameCount();
}

spine_array_float spine_path_constraint_position_timeline_get_frames(spine_path_constraint_position_timeline self) {
	PathConstraintPositionTimeline *_self = (PathConstraintPositionTimeline *) self;
	return (spine_array_float) &_self->getFrames();
}

float spine_path_constraint_position_timeline_get_duration(spine_path_constraint_position_timeline self) {
	PathConstraintPositionTimeline *_self = (PathConstraintPositionTimeline *) self;
	return _self->getDuration();
}

spine_array_property_id spine_path_constraint_position_timeline_get_property_ids(spine_path_constraint_position_timeline self) {
	PathConstraintPositionTimeline *_self = (PathConstraintPositionTimeline *) self;
	return (spine_array_property_id) &_self->getPropertyIds();
}

spine_rtti spine_path_constraint_position_timeline_rtti(void) {
	return (spine_rtti) &PathConstraintPositionTimeline::rtti;
}
