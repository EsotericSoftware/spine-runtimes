#include "ik_constraint_timeline.h"
#include <spine/spine.h>

using namespace spine;

spine_ik_constraint_timeline spine_ik_constraint_timeline_create(size_t frameCount, size_t bezierCount, int constraintIndex) {
	return (spine_ik_constraint_timeline) new (__FILE__, __LINE__) IkConstraintTimeline(frameCount, bezierCount, constraintIndex);
}

void spine_ik_constraint_timeline_dispose(spine_ik_constraint_timeline self) {
	delete (IkConstraintTimeline *) self;
}

spine_rtti spine_ik_constraint_timeline_get_rtti(spine_ik_constraint_timeline self) {
	IkConstraintTimeline *_self = (IkConstraintTimeline *) self;
	return (spine_rtti) &_self->getRTTI();
}

void spine_ik_constraint_timeline_apply(spine_ik_constraint_timeline self, spine_skeleton skeleton, float lastTime, float time,
										/*@null*/ spine_array_event events, float alpha, bool fromSetup, bool add, bool out, bool appliedPose) {
	IkConstraintTimeline *_self = (IkConstraintTimeline *) self;
	_self->apply(*((Skeleton *) skeleton), lastTime, time, (Array<Event *> *) events, alpha, fromSetup, add, out, appliedPose);
}

void spine_ik_constraint_timeline_set_frame(spine_ik_constraint_timeline self, int frame, float time, float mix, float softness, int bendDirection,
											bool compress, bool stretch) {
	IkConstraintTimeline *_self = (IkConstraintTimeline *) self;
	_self->setFrame(frame, time, mix, softness, bendDirection, compress, stretch);
}

int spine_ik_constraint_timeline_get_constraint_index(spine_ik_constraint_timeline self) {
	IkConstraintTimeline *_self = (IkConstraintTimeline *) self;
	return _self->getConstraintIndex();
}

void spine_ik_constraint_timeline_set_constraint_index(spine_ik_constraint_timeline self, int inValue) {
	IkConstraintTimeline *_self = (IkConstraintTimeline *) self;
	_self->setConstraintIndex(inValue);
}

void spine_ik_constraint_timeline_set_linear(spine_ik_constraint_timeline self, size_t frame) {
	IkConstraintTimeline *_self = (IkConstraintTimeline *) self;
	_self->setLinear(frame);
}

void spine_ik_constraint_timeline_set_stepped(spine_ik_constraint_timeline self, size_t frame) {
	IkConstraintTimeline *_self = (IkConstraintTimeline *) self;
	_self->setStepped(frame);
}

void spine_ik_constraint_timeline_set_bezier(spine_ik_constraint_timeline self, size_t bezier, size_t frame, float value, float time1, float value1,
											 float cx1, float cy1, float cx2, float cy2, float time2, float value2) {
	IkConstraintTimeline *_self = (IkConstraintTimeline *) self;
	_self->setBezier(bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
}

float spine_ik_constraint_timeline_get_bezier_value(spine_ik_constraint_timeline self, float time, size_t frame, size_t valueOffset, size_t i) {
	IkConstraintTimeline *_self = (IkConstraintTimeline *) self;
	return _self->getBezierValue(time, frame, valueOffset, i);
}

spine_array_float spine_ik_constraint_timeline_get_curves(spine_ik_constraint_timeline self) {
	IkConstraintTimeline *_self = (IkConstraintTimeline *) self;
	return (spine_array_float) &_self->getCurves();
}

bool spine_ik_constraint_timeline_get_additive(spine_ik_constraint_timeline self) {
	IkConstraintTimeline *_self = (IkConstraintTimeline *) self;
	return _self->getAdditive();
}

bool spine_ik_constraint_timeline_get_instant(spine_ik_constraint_timeline self) {
	IkConstraintTimeline *_self = (IkConstraintTimeline *) self;
	return _self->getInstant();
}

size_t spine_ik_constraint_timeline_get_frame_entries(spine_ik_constraint_timeline self) {
	IkConstraintTimeline *_self = (IkConstraintTimeline *) self;
	return _self->getFrameEntries();
}

size_t spine_ik_constraint_timeline_get_frame_count(spine_ik_constraint_timeline self) {
	IkConstraintTimeline *_self = (IkConstraintTimeline *) self;
	return _self->getFrameCount();
}

spine_array_float spine_ik_constraint_timeline_get_frames(spine_ik_constraint_timeline self) {
	IkConstraintTimeline *_self = (IkConstraintTimeline *) self;
	return (spine_array_float) &_self->getFrames();
}

float spine_ik_constraint_timeline_get_duration(spine_ik_constraint_timeline self) {
	IkConstraintTimeline *_self = (IkConstraintTimeline *) self;
	return _self->getDuration();
}

spine_array_property_id spine_ik_constraint_timeline_get_property_ids(spine_ik_constraint_timeline self) {
	IkConstraintTimeline *_self = (IkConstraintTimeline *) self;
	return (spine_array_property_id) &_self->getPropertyIds();
}

spine_rtti spine_ik_constraint_timeline_rtti(void) {
	return (spine_rtti) &IkConstraintTimeline::rtti;
}
