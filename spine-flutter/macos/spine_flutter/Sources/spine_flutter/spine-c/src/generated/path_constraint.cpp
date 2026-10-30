#include "path_constraint.h"
#include <spine/spine.h>

using namespace spine;

spine_path_constraint spine_path_constraint_create(spine_path_constraint_data data, spine_skeleton skeleton) {
	return (spine_path_constraint) new (__FILE__, __LINE__) PathConstraint(*((PathConstraintData *) data), *((Skeleton *) skeleton));
}

void spine_path_constraint_dispose(spine_path_constraint self) {
	delete (PathConstraint *) self;
}

spine_rtti spine_path_constraint_get_rtti(spine_path_constraint self) {
	PathConstraint *_self = (PathConstraint *) self;
	return (spine_rtti) &_self->getRTTI();
}

spine_path_constraint spine_path_constraint_copy(spine_path_constraint self, spine_skeleton skeleton) {
	PathConstraint *_self = (PathConstraint *) self;
	return (spine_path_constraint) &_self->copy(*((Skeleton *) skeleton));
}

void spine_path_constraint_update(spine_path_constraint self, spine_skeleton skeleton, spine_physics physics) {
	PathConstraint *_self = (PathConstraint *) self;
	_self->update(*((Skeleton *) skeleton), (Physics) physics);
}

void spine_path_constraint_sort(spine_path_constraint self, spine_skeleton skeleton) {
	PathConstraint *_self = (PathConstraint *) self;
	_self->sort(*((Skeleton *) skeleton));
}

bool spine_path_constraint_is_source_active(spine_path_constraint self) {
	PathConstraint *_self = (PathConstraint *) self;
	return _self->isSourceActive();
}

spine_array_bone_pose spine_path_constraint_get_bones(spine_path_constraint self) {
	PathConstraint *_self = (PathConstraint *) self;
	return (spine_array_bone_pose) &_self->getBones();
}

spine_slot spine_path_constraint_get_slot(spine_path_constraint self) {
	PathConstraint *_self = (PathConstraint *) self;
	return (spine_slot) &_self->getSlot();
}

void spine_path_constraint_set_slot(spine_path_constraint self, spine_slot slot) {
	PathConstraint *_self = (PathConstraint *) self;
	_self->setSlot(*((Slot *) slot));
}

spine_path_constraint_data spine_path_constraint_get_data(spine_path_constraint self) {
	PathConstraint *_self = (PathConstraint *) self;
	return (spine_path_constraint_data) &_self->getData();
}

spine_path_constraint_pose spine_path_constraint_get_pose(spine_path_constraint self) {
	PathConstraint *_self = (PathConstraint *) self;
	return (spine_path_constraint_pose) &_self->getPose();
}

spine_path_constraint_pose spine_path_constraint_get_applied_pose(spine_path_constraint self) {
	PathConstraint *_self = (PathConstraint *) self;
	return (spine_path_constraint_pose) &_self->getAppliedPose();
}

void spine_path_constraint_reset_constrained(spine_path_constraint self) {
	PathConstraint *_self = (PathConstraint *) self;
	_self->resetConstrained();
}

void spine_path_constraint_constrained(spine_path_constraint self) {
	PathConstraint *_self = (PathConstraint *) self;
	_self->constrained();
}

bool spine_path_constraint_is_pose_equal_to_applied(spine_path_constraint self) {
	PathConstraint *_self = (PathConstraint *) self;
	return _self->isPoseEqualToApplied();
}

bool spine_path_constraint_is_active(spine_path_constraint self) {
	PathConstraint *_self = (PathConstraint *) self;
	return _self->isActive();
}

void spine_path_constraint_set_active(spine_path_constraint self, bool active) {
	PathConstraint *_self = (PathConstraint *) self;
	_self->setActive(active);
}

spine_rtti spine_path_constraint_rtti(void) {
	return (spine_rtti) &PathConstraint::rtti;
}
