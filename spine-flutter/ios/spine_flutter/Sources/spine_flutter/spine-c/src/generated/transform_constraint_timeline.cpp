#include "transform_constraint_timeline.h"
#include <spine/spine.h>

using namespace spine;

spine_transform_constraint_timeline spine_transform_constraint_timeline_create(size_t frameCount, size_t bezierCount, int transformConstraintIndex) {
	return (spine_transform_constraint_timeline) new (__FILE__, __LINE__)
		TransformConstraintTimeline(frameCount, bezierCount, transformConstraintIndex);
}

void spine_transform_constraint_timeline_dispose(spine_transform_constraint_timeline self) {
	delete (TransformConstraintTimeline *) self;
}

spine_rtti spine_transform_constraint_timeline_get_rtti(spine_transform_constraint_timeline self) {
	TransformConstraintTimeline *_self = (TransformConstraintTimeline *) self;
	return (spine_rtti) &_self->getRTTI();
}

void spine_transform_constraint_timeline_apply(spine_transform_constraint_timeline self, spine_skeleton skeleton, float lastTime, float time,
											   /*@null*/ spine_array_event events, float alpha, spine_mix_from from, bool add, bool out,
											   bool appliedPose) {
	TransformConstraintTimeline *_self = (TransformConstraintTimeline *) self;
	_self->apply(*((Skeleton *) skeleton), lastTime, time, (Array<Event *> *) events, alpha, (MixFrom) from, add, out, appliedPose);
}

void spine_transform_constraint_timeline_set_frame(spine_transform_constraint_timeline self, int frame, float time, float mixRotate, float mixX,
												   float mixY, float mixScaleX, float mixScaleY, float mixShearY) {
	TransformConstraintTimeline *_self = (TransformConstraintTimeline *) self;
	_self->setFrame(frame, time, mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY);
}

int spine_transform_constraint_timeline_get_constraint_index(spine_transform_constraint_timeline self) {
	TransformConstraintTimeline *_self = (TransformConstraintTimeline *) self;
	return _self->getConstraintIndex();
}

void spine_transform_constraint_timeline_set_constraint_index(spine_transform_constraint_timeline self, int inValue) {
	TransformConstraintTimeline *_self = (TransformConstraintTimeline *) self;
	_self->setConstraintIndex(inValue);
}

void spine_transform_constraint_timeline_set_linear(spine_transform_constraint_timeline self, size_t frame) {
	TransformConstraintTimeline *_self = (TransformConstraintTimeline *) self;
	_self->setLinear(frame);
}

void spine_transform_constraint_timeline_set_stepped(spine_transform_constraint_timeline self, size_t frame) {
	TransformConstraintTimeline *_self = (TransformConstraintTimeline *) self;
	_self->setStepped(frame);
}

void spine_transform_constraint_timeline_set_bezier(spine_transform_constraint_timeline self, size_t bezier, size_t frame, float value, float time1,
													float value1, float cx1, float cy1, float cx2, float cy2, float time2, float value2) {
	TransformConstraintTimeline *_self = (TransformConstraintTimeline *) self;
	_self->setBezier(bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
}

float spine_transform_constraint_timeline_get_bezier_value(spine_transform_constraint_timeline self, float time, size_t frame, size_t valueOffset,
														   size_t i) {
	TransformConstraintTimeline *_self = (TransformConstraintTimeline *) self;
	return _self->getBezierValue(time, frame, valueOffset, i);
}

spine_array_float spine_transform_constraint_timeline_get_curves(spine_transform_constraint_timeline self) {
	TransformConstraintTimeline *_self = (TransformConstraintTimeline *) self;
	return (spine_array_float) &_self->getCurves();
}

bool spine_transform_constraint_timeline_get_additive(spine_transform_constraint_timeline self) {
	TransformConstraintTimeline *_self = (TransformConstraintTimeline *) self;
	return _self->getAdditive();
}

bool spine_transform_constraint_timeline_get_instant(spine_transform_constraint_timeline self) {
	TransformConstraintTimeline *_self = (TransformConstraintTimeline *) self;
	return _self->getInstant();
}

size_t spine_transform_constraint_timeline_get_frame_entries(spine_transform_constraint_timeline self) {
	TransformConstraintTimeline *_self = (TransformConstraintTimeline *) self;
	return _self->getFrameEntries();
}

size_t spine_transform_constraint_timeline_get_frame_count(spine_transform_constraint_timeline self) {
	TransformConstraintTimeline *_self = (TransformConstraintTimeline *) self;
	return _self->getFrameCount();
}

spine_array_float spine_transform_constraint_timeline_get_frames(spine_transform_constraint_timeline self) {
	TransformConstraintTimeline *_self = (TransformConstraintTimeline *) self;
	return (spine_array_float) &_self->getFrames();
}

float spine_transform_constraint_timeline_get_duration(spine_transform_constraint_timeline self) {
	TransformConstraintTimeline *_self = (TransformConstraintTimeline *) self;
	return _self->getDuration();
}

spine_array_property_id spine_transform_constraint_timeline_get_property_ids(spine_transform_constraint_timeline self) {
	TransformConstraintTimeline *_self = (TransformConstraintTimeline *) self;
	return (spine_array_property_id) &_self->getPropertyIds();
}

spine_rtti spine_transform_constraint_timeline_rtti(void) {
	return (spine_rtti) &TransformConstraintTimeline::rtti;
}
