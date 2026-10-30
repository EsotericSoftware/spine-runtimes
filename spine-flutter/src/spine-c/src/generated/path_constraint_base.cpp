#include "path_constraint_base.h"
#include <spine/spine.h>

using namespace spine;

void spine_path_constraint_base_dispose(spine_path_constraint_base self) {
	delete (PathConstraintBase *) self;
}

spine_path_constraint_data spine_path_constraint_base_get_data(spine_path_constraint_base self) {
	PathConstraintBase *_self = (PathConstraintBase *) self;
	return (spine_path_constraint_data) &_self->getData();
}

spine_path_constraint_pose spine_path_constraint_base_get_pose(spine_path_constraint_base self) {
	PathConstraintBase *_self = (PathConstraintBase *) self;
	return (spine_path_constraint_pose) &_self->getPose();
}

spine_path_constraint_pose spine_path_constraint_base_get_applied_pose(spine_path_constraint_base self) {
	PathConstraintBase *_self = (PathConstraintBase *) self;
	return (spine_path_constraint_pose) &_self->getAppliedPose();
}

void spine_path_constraint_base_reset_constrained(spine_path_constraint_base self) {
	PathConstraintBase *_self = (PathConstraintBase *) self;
	_self->resetConstrained();
}

void spine_path_constraint_base_constrained(spine_path_constraint_base self) {
	PathConstraintBase *_self = (PathConstraintBase *) self;
	_self->constrained();
}

bool spine_path_constraint_base_is_pose_equal_to_applied(spine_path_constraint_base self) {
	PathConstraintBase *_self = (PathConstraintBase *) self;
	return _self->isPoseEqualToApplied();
}

bool spine_path_constraint_base_is_active(spine_path_constraint_base self) {
	PathConstraintBase *_self = (PathConstraintBase *) self;
	return _self->isActive();
}

void spine_path_constraint_base_set_active(spine_path_constraint_base self, bool active) {
	PathConstraintBase *_self = (PathConstraintBase *) self;
	_self->setActive(active);
}

spine_rtti spine_path_constraint_base_get_rtti(spine_path_constraint_base self) {
	PathConstraintBase *_self = (PathConstraintBase *) self;
	return (spine_rtti) &_self->getRTTI();
}

void spine_path_constraint_base_sort(spine_path_constraint_base self, spine_skeleton skeleton) {
	PathConstraintBase *_self = (PathConstraintBase *) self;
	_self->sort(*((Skeleton *) skeleton));
}

bool spine_path_constraint_base_is_source_active(spine_path_constraint_base self) {
	PathConstraintBase *_self = (PathConstraintBase *) self;
	return _self->isSourceActive();
}

void spine_path_constraint_base_update(spine_path_constraint_base self, spine_skeleton skeleton, spine_physics physics) {
	PathConstraintBase *_self = (PathConstraintBase *) self;
	_self->update(*((Skeleton *) skeleton), (Physics) physics);
}

spine_rtti spine_path_constraint_base_rtti(void) {
	return (spine_rtti) &PathConstraintBase::rtti;
}
