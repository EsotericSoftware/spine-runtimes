#include "physics_constraint_mix_timeline.h"
#include <spine/spine.h>

using namespace spine;

spine_physics_constraint_mix_timeline spine_physics_constraint_mix_timeline_create(size_t frameCount, size_t bezierCount,
																				   int physicsConstraintIndex) {
	return (spine_physics_constraint_mix_timeline) new (__FILE__, __LINE__)
		PhysicsConstraintMixTimeline(frameCount, bezierCount, physicsConstraintIndex);
}

void spine_physics_constraint_mix_timeline_dispose(spine_physics_constraint_mix_timeline self) {
	delete (PhysicsConstraintMixTimeline *) self;
}

spine_rtti spine_physics_constraint_mix_timeline_get_rtti(spine_physics_constraint_mix_timeline self) {
	PhysicsConstraintMixTimeline *_self = (PhysicsConstraintMixTimeline *) self;
	return (spine_rtti) &_self->getRTTI();
}

void spine_physics_constraint_mix_timeline_apply(spine_physics_constraint_mix_timeline self, spine_skeleton skeleton, float lastTime, float time,
												 /*@null*/ spine_array_event events, float alpha, bool fromSetup, bool add, bool out,
												 bool appliedPose) {
	PhysicsConstraintMixTimeline *_self = (PhysicsConstraintMixTimeline *) self;
	_self->apply(*((Skeleton *) skeleton), lastTime, time, (Array<Event *> *) events, alpha, fromSetup, add, out, appliedPose);
}

int spine_physics_constraint_mix_timeline_get_constraint_index(spine_physics_constraint_mix_timeline self) {
	PhysicsConstraintMixTimeline *_self = (PhysicsConstraintMixTimeline *) self;
	return _self->getConstraintIndex();
}

void spine_physics_constraint_mix_timeline_set_constraint_index(spine_physics_constraint_mix_timeline self, int inValue) {
	PhysicsConstraintMixTimeline *_self = (PhysicsConstraintMixTimeline *) self;
	_self->setConstraintIndex(inValue);
}

void spine_physics_constraint_mix_timeline_set_frame(spine_physics_constraint_mix_timeline self, size_t frame, float time, float value) {
	PhysicsConstraintMixTimeline *_self = (PhysicsConstraintMixTimeline *) self;
	_self->setFrame(frame, time, value);
}

float spine_physics_constraint_mix_timeline_get_curve_value(spine_physics_constraint_mix_timeline self, float time) {
	PhysicsConstraintMixTimeline *_self = (PhysicsConstraintMixTimeline *) self;
	return _self->getCurveValue(time);
}

float spine_physics_constraint_mix_timeline_get_relative_value(spine_physics_constraint_mix_timeline self, float time, float alpha, bool fromSetup,
															   bool add, float current, float setup) {
	PhysicsConstraintMixTimeline *_self = (PhysicsConstraintMixTimeline *) self;
	return _self->getRelativeValue(time, alpha, fromSetup, add, current, setup);
}

float spine_physics_constraint_mix_timeline_get_absolute_value_1(spine_physics_constraint_mix_timeline self, float time, float alpha, bool fromSetup,
																 bool add, float current, float setup) {
	PhysicsConstraintMixTimeline *_self = (PhysicsConstraintMixTimeline *) self;
	return _self->getAbsoluteValue(time, alpha, fromSetup, add, current, setup);
}

float spine_physics_constraint_mix_timeline_get_absolute_value_2(spine_physics_constraint_mix_timeline self, float time, float alpha, bool fromSetup,
																 bool add, float current, float setup, float value) {
	PhysicsConstraintMixTimeline *_self = (PhysicsConstraintMixTimeline *) self;
	return _self->getAbsoluteValue(time, alpha, fromSetup, add, current, setup, value);
}

float spine_physics_constraint_mix_timeline_get_scale_value(spine_physics_constraint_mix_timeline self, float time, float alpha, bool fromSetup,
															bool add, bool out, float current, float setup) {
	PhysicsConstraintMixTimeline *_self = (PhysicsConstraintMixTimeline *) self;
	return _self->getScaleValue(time, alpha, fromSetup, add, out, current, setup);
}

void spine_physics_constraint_mix_timeline_set_linear(spine_physics_constraint_mix_timeline self, size_t frame) {
	PhysicsConstraintMixTimeline *_self = (PhysicsConstraintMixTimeline *) self;
	_self->setLinear(frame);
}

void spine_physics_constraint_mix_timeline_set_stepped(spine_physics_constraint_mix_timeline self, size_t frame) {
	PhysicsConstraintMixTimeline *_self = (PhysicsConstraintMixTimeline *) self;
	_self->setStepped(frame);
}

void spine_physics_constraint_mix_timeline_set_bezier(spine_physics_constraint_mix_timeline self, size_t bezier, size_t frame, float value,
													  float time1, float value1, float cx1, float cy1, float cx2, float cy2, float time2,
													  float value2) {
	PhysicsConstraintMixTimeline *_self = (PhysicsConstraintMixTimeline *) self;
	_self->setBezier(bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
}

float spine_physics_constraint_mix_timeline_get_bezier_value(spine_physics_constraint_mix_timeline self, float time, size_t frame, size_t valueOffset,
															 size_t i) {
	PhysicsConstraintMixTimeline *_self = (PhysicsConstraintMixTimeline *) self;
	return _self->getBezierValue(time, frame, valueOffset, i);
}

spine_array_float spine_physics_constraint_mix_timeline_get_curves(spine_physics_constraint_mix_timeline self) {
	PhysicsConstraintMixTimeline *_self = (PhysicsConstraintMixTimeline *) self;
	return (spine_array_float) &_self->getCurves();
}

bool spine_physics_constraint_mix_timeline_get_additive(spine_physics_constraint_mix_timeline self) {
	PhysicsConstraintMixTimeline *_self = (PhysicsConstraintMixTimeline *) self;
	return _self->getAdditive();
}

bool spine_physics_constraint_mix_timeline_get_instant(spine_physics_constraint_mix_timeline self) {
	PhysicsConstraintMixTimeline *_self = (PhysicsConstraintMixTimeline *) self;
	return _self->getInstant();
}

size_t spine_physics_constraint_mix_timeline_get_frame_entries(spine_physics_constraint_mix_timeline self) {
	PhysicsConstraintMixTimeline *_self = (PhysicsConstraintMixTimeline *) self;
	return _self->getFrameEntries();
}

size_t spine_physics_constraint_mix_timeline_get_frame_count(spine_physics_constraint_mix_timeline self) {
	PhysicsConstraintMixTimeline *_self = (PhysicsConstraintMixTimeline *) self;
	return _self->getFrameCount();
}

spine_array_float spine_physics_constraint_mix_timeline_get_frames(spine_physics_constraint_mix_timeline self) {
	PhysicsConstraintMixTimeline *_self = (PhysicsConstraintMixTimeline *) self;
	return (spine_array_float) &_self->getFrames();
}

float spine_physics_constraint_mix_timeline_get_duration(spine_physics_constraint_mix_timeline self) {
	PhysicsConstraintMixTimeline *_self = (PhysicsConstraintMixTimeline *) self;
	return _self->getDuration();
}

spine_array_property_id spine_physics_constraint_mix_timeline_get_property_ids(spine_physics_constraint_mix_timeline self) {
	PhysicsConstraintMixTimeline *_self = (PhysicsConstraintMixTimeline *) self;
	return (spine_array_property_id) &_self->getPropertyIds();
}

spine_rtti spine_physics_constraint_mix_timeline_rtti(void) {
	return (spine_rtti) &PhysicsConstraintMixTimeline::rtti;
}
