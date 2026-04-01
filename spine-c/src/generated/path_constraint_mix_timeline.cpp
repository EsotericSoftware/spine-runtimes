#include "path_constraint_mix_timeline.h"
#include <spine/spine.h>

using namespace spine;

spine_path_constraint_mix_timeline spine_path_constraint_mix_timeline_create(size_t frameCount, size_t bezierCount, int constraintIndex) {
	return (spine_path_constraint_mix_timeline) new (__FILE__, __LINE__) PathConstraintMixTimeline(frameCount, bezierCount, constraintIndex);
}

void spine_path_constraint_mix_timeline_dispose(spine_path_constraint_mix_timeline self) {
	delete (PathConstraintMixTimeline *) self;
}

spine_rtti spine_path_constraint_mix_timeline_get_rtti(spine_path_constraint_mix_timeline self) {
	PathConstraintMixTimeline *_self = (PathConstraintMixTimeline *) self;
	return (spine_rtti) &_self->getRTTI();
}

void spine_path_constraint_mix_timeline_apply(spine_path_constraint_mix_timeline self, spine_skeleton skeleton, float lastTime, float time,
											  /*@null*/ spine_array_event events, float alpha, bool fromSetup, bool add, bool out, bool appliedPose) {
	PathConstraintMixTimeline *_self = (PathConstraintMixTimeline *) self;
	_self->apply(*((Skeleton *) skeleton), lastTime, time, (Array<Event *> *) events, alpha, fromSetup, add, out, appliedPose);
}

void spine_path_constraint_mix_timeline_set_frame(spine_path_constraint_mix_timeline self, int frame, float time, float mixRotate, float mixX,
												  float mixY) {
	PathConstraintMixTimeline *_self = (PathConstraintMixTimeline *) self;
	_self->setFrame(frame, time, mixRotate, mixX, mixY);
}

int spine_path_constraint_mix_timeline_get_constraint_index(spine_path_constraint_mix_timeline self) {
	PathConstraintMixTimeline *_self = (PathConstraintMixTimeline *) self;
	return _self->getConstraintIndex();
}

void spine_path_constraint_mix_timeline_set_constraint_index(spine_path_constraint_mix_timeline self, int inValue) {
	PathConstraintMixTimeline *_self = (PathConstraintMixTimeline *) self;
	_self->setConstraintIndex(inValue);
}

void spine_path_constraint_mix_timeline_set_linear(spine_path_constraint_mix_timeline self, size_t frame) {
	PathConstraintMixTimeline *_self = (PathConstraintMixTimeline *) self;
	_self->setLinear(frame);
}

void spine_path_constraint_mix_timeline_set_stepped(spine_path_constraint_mix_timeline self, size_t frame) {
	PathConstraintMixTimeline *_self = (PathConstraintMixTimeline *) self;
	_self->setStepped(frame);
}

void spine_path_constraint_mix_timeline_set_bezier(spine_path_constraint_mix_timeline self, size_t bezier, size_t frame, float value, float time1,
												   float value1, float cx1, float cy1, float cx2, float cy2, float time2, float value2) {
	PathConstraintMixTimeline *_self = (PathConstraintMixTimeline *) self;
	_self->setBezier(bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
}

float spine_path_constraint_mix_timeline_get_bezier_value(spine_path_constraint_mix_timeline self, float time, size_t frame, size_t valueOffset,
														  size_t i) {
	PathConstraintMixTimeline *_self = (PathConstraintMixTimeline *) self;
	return _self->getBezierValue(time, frame, valueOffset, i);
}

spine_array_float spine_path_constraint_mix_timeline_get_curves(spine_path_constraint_mix_timeline self) {
	PathConstraintMixTimeline *_self = (PathConstraintMixTimeline *) self;
	return (spine_array_float) &_self->getCurves();
}

bool spine_path_constraint_mix_timeline_get_additive(spine_path_constraint_mix_timeline self) {
	PathConstraintMixTimeline *_self = (PathConstraintMixTimeline *) self;
	return _self->getAdditive();
}

bool spine_path_constraint_mix_timeline_get_instant(spine_path_constraint_mix_timeline self) {
	PathConstraintMixTimeline *_self = (PathConstraintMixTimeline *) self;
	return _self->getInstant();
}

size_t spine_path_constraint_mix_timeline_get_frame_entries(spine_path_constraint_mix_timeline self) {
	PathConstraintMixTimeline *_self = (PathConstraintMixTimeline *) self;
	return _self->getFrameEntries();
}

size_t spine_path_constraint_mix_timeline_get_frame_count(spine_path_constraint_mix_timeline self) {
	PathConstraintMixTimeline *_self = (PathConstraintMixTimeline *) self;
	return _self->getFrameCount();
}

spine_array_float spine_path_constraint_mix_timeline_get_frames(spine_path_constraint_mix_timeline self) {
	PathConstraintMixTimeline *_self = (PathConstraintMixTimeline *) self;
	return (spine_array_float) &_self->getFrames();
}

float spine_path_constraint_mix_timeline_get_duration(spine_path_constraint_mix_timeline self) {
	PathConstraintMixTimeline *_self = (PathConstraintMixTimeline *) self;
	return _self->getDuration();
}

spine_array_property_id spine_path_constraint_mix_timeline_get_property_ids(spine_path_constraint_mix_timeline self) {
	PathConstraintMixTimeline *_self = (PathConstraintMixTimeline *) self;
	return (spine_array_property_id) &_self->getPropertyIds();
}

spine_rtti spine_path_constraint_mix_timeline_rtti(void) {
	return (spine_rtti) &PathConstraintMixTimeline::rtti;
}
