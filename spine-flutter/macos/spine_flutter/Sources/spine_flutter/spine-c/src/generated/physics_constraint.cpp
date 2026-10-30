#include "physics_constraint.h"
#include <spine/spine.h>

using namespace spine;

spine_physics_constraint spine_physics_constraint_create(spine_physics_constraint_data data, spine_skeleton skeleton) {
	return (spine_physics_constraint) new (__FILE__, __LINE__) PhysicsConstraint(*((PhysicsConstraintData *) data), *((Skeleton *) skeleton));
}

void spine_physics_constraint_dispose(spine_physics_constraint self) {
	delete (PhysicsConstraint *) self;
}

spine_rtti spine_physics_constraint_get_rtti(spine_physics_constraint self) {
	PhysicsConstraint *_self = (PhysicsConstraint *) self;
	return (spine_rtti) &_self->getRTTI();
}

void spine_physics_constraint_update(spine_physics_constraint self, spine_skeleton skeleton, spine_physics physics) {
	PhysicsConstraint *_self = (PhysicsConstraint *) self;
	_self->update(*((Skeleton *) skeleton), (Physics) physics);
}

void spine_physics_constraint_sort(spine_physics_constraint self, spine_skeleton skeleton) {
	PhysicsConstraint *_self = (PhysicsConstraint *) self;
	_self->sort(*((Skeleton *) skeleton));
}

bool spine_physics_constraint_is_source_active(spine_physics_constraint self) {
	PhysicsConstraint *_self = (PhysicsConstraint *) self;
	return _self->isSourceActive();
}

spine_physics_constraint spine_physics_constraint_copy(spine_physics_constraint self, spine_skeleton skeleton) {
	PhysicsConstraint *_self = (PhysicsConstraint *) self;
	return (spine_physics_constraint) &_self->copy(*((Skeleton *) skeleton));
}

void spine_physics_constraint_reset(spine_physics_constraint self, spine_skeleton skeleton) {
	PhysicsConstraint *_self = (PhysicsConstraint *) self;
	_self->reset(*((Skeleton *) skeleton));
}

void spine_physics_constraint_translate(spine_physics_constraint self, float x, float y) {
	PhysicsConstraint *_self = (PhysicsConstraint *) self;
	_self->translate(x, y);
}

void spine_physics_constraint_rotate(spine_physics_constraint self, float x, float y, float degrees) {
	PhysicsConstraint *_self = (PhysicsConstraint *) self;
	_self->rotate(x, y, degrees);
}

spine_bone_pose spine_physics_constraint_get_bone(spine_physics_constraint self) {
	PhysicsConstraint *_self = (PhysicsConstraint *) self;
	return (spine_bone_pose) &_self->getBone();
}

void spine_physics_constraint_set_bone(spine_physics_constraint self, spine_bone_pose bone) {
	PhysicsConstraint *_self = (PhysicsConstraint *) self;
	_self->setBone(*((BonePose *) bone));
}

spine_physics_constraint_data spine_physics_constraint_get_data(spine_physics_constraint self) {
	PhysicsConstraint *_self = (PhysicsConstraint *) self;
	return (spine_physics_constraint_data) &_self->getData();
}

spine_physics_constraint_pose spine_physics_constraint_get_pose(spine_physics_constraint self) {
	PhysicsConstraint *_self = (PhysicsConstraint *) self;
	return (spine_physics_constraint_pose) &_self->getPose();
}

spine_physics_constraint_pose spine_physics_constraint_get_applied_pose(spine_physics_constraint self) {
	PhysicsConstraint *_self = (PhysicsConstraint *) self;
	return (spine_physics_constraint_pose) &_self->getAppliedPose();
}

void spine_physics_constraint_reset_constrained(spine_physics_constraint self) {
	PhysicsConstraint *_self = (PhysicsConstraint *) self;
	_self->resetConstrained();
}

void spine_physics_constraint_constrained(spine_physics_constraint self) {
	PhysicsConstraint *_self = (PhysicsConstraint *) self;
	_self->constrained();
}

bool spine_physics_constraint_is_pose_equal_to_applied(spine_physics_constraint self) {
	PhysicsConstraint *_self = (PhysicsConstraint *) self;
	return _self->isPoseEqualToApplied();
}

bool spine_physics_constraint_is_active(spine_physics_constraint self) {
	PhysicsConstraint *_self = (PhysicsConstraint *) self;
	return _self->isActive();
}

void spine_physics_constraint_set_active(spine_physics_constraint self, bool active) {
	PhysicsConstraint *_self = (PhysicsConstraint *) self;
	_self->setActive(active);
}

spine_rtti spine_physics_constraint_rtti(void) {
	return (spine_rtti) &PhysicsConstraint::rtti;
}
