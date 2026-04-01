#include "physics_constraint_reset_timeline.h"
#include <spine/spine.h>

using namespace spine;

spine_physics_constraint_reset_timeline spine_physics_constraint_reset_timeline_create(size_t frameCount, int constraintIndex) {
	return (spine_physics_constraint_reset_timeline) new (__FILE__, __LINE__) PhysicsConstraintResetTimeline(frameCount, constraintIndex);
}

void spine_physics_constraint_reset_timeline_dispose(spine_physics_constraint_reset_timeline self) {
	delete (PhysicsConstraintResetTimeline *) self;
}

spine_rtti spine_physics_constraint_reset_timeline_get_rtti(spine_physics_constraint_reset_timeline self) {
	PhysicsConstraintResetTimeline *_self = (PhysicsConstraintResetTimeline *) self;
	return (spine_rtti) &_self->getRTTI();
}

void spine_physics_constraint_reset_timeline_apply(spine_physics_constraint_reset_timeline self, spine_skeleton skeleton, float lastTime, float time,
												   /*@null*/ spine_array_event events, float alpha, bool fromSetup, bool add, bool out,
												   bool appliedPose) {
	PhysicsConstraintResetTimeline *_self = (PhysicsConstraintResetTimeline *) self;
	_self->apply(*((Skeleton *) skeleton), lastTime, time, (Array<Event *> *) events, alpha, fromSetup, add, out, appliedPose);
}

int spine_physics_constraint_reset_timeline_get_frame_count(spine_physics_constraint_reset_timeline self) {
	PhysicsConstraintResetTimeline *_self = (PhysicsConstraintResetTimeline *) self;
	return _self->getFrameCount();
}

int spine_physics_constraint_reset_timeline_get_constraint_index(spine_physics_constraint_reset_timeline self) {
	PhysicsConstraintResetTimeline *_self = (PhysicsConstraintResetTimeline *) self;
	return _self->getConstraintIndex();
}

void spine_physics_constraint_reset_timeline_set_constraint_index(spine_physics_constraint_reset_timeline self, int inValue) {
	PhysicsConstraintResetTimeline *_self = (PhysicsConstraintResetTimeline *) self;
	_self->setConstraintIndex(inValue);
}

void spine_physics_constraint_reset_timeline_set_frame(spine_physics_constraint_reset_timeline self, int frame, float time) {
	PhysicsConstraintResetTimeline *_self = (PhysicsConstraintResetTimeline *) self;
	_self->setFrame(frame, time);
}

bool spine_physics_constraint_reset_timeline_get_additive(spine_physics_constraint_reset_timeline self) {
	PhysicsConstraintResetTimeline *_self = (PhysicsConstraintResetTimeline *) self;
	return _self->getAdditive();
}

bool spine_physics_constraint_reset_timeline_get_instant(spine_physics_constraint_reset_timeline self) {
	PhysicsConstraintResetTimeline *_self = (PhysicsConstraintResetTimeline *) self;
	return _self->getInstant();
}

size_t spine_physics_constraint_reset_timeline_get_frame_entries(spine_physics_constraint_reset_timeline self) {
	PhysicsConstraintResetTimeline *_self = (PhysicsConstraintResetTimeline *) self;
	return _self->getFrameEntries();
}

spine_array_float spine_physics_constraint_reset_timeline_get_frames(spine_physics_constraint_reset_timeline self) {
	PhysicsConstraintResetTimeline *_self = (PhysicsConstraintResetTimeline *) self;
	return (spine_array_float) &_self->getFrames();
}

float spine_physics_constraint_reset_timeline_get_duration(spine_physics_constraint_reset_timeline self) {
	PhysicsConstraintResetTimeline *_self = (PhysicsConstraintResetTimeline *) self;
	return _self->getDuration();
}

spine_array_property_id spine_physics_constraint_reset_timeline_get_property_ids(spine_physics_constraint_reset_timeline self) {
	PhysicsConstraintResetTimeline *_self = (PhysicsConstraintResetTimeline *) self;
	return (spine_array_property_id) &_self->getPropertyIds();
}

spine_rtti spine_physics_constraint_reset_timeline_rtti(void) {
	return (spine_rtti) &PhysicsConstraintResetTimeline::rtti;
}
