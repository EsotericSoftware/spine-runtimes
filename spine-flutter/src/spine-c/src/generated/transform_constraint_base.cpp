#include "transform_constraint_base.h"
#include <spine/spine.h>

using namespace spine;

void spine_transform_constraint_base_dispose(spine_transform_constraint_base self) {
	delete (TransformConstraintBase *) self;
}

spine_transform_constraint_data spine_transform_constraint_base_get_data(spine_transform_constraint_base self) {
	TransformConstraintBase *_self = (TransformConstraintBase *) self;
	return (spine_transform_constraint_data) &_self->getData();
}

spine_transform_constraint_pose spine_transform_constraint_base_get_pose(spine_transform_constraint_base self) {
	TransformConstraintBase *_self = (TransformConstraintBase *) self;
	return (spine_transform_constraint_pose) &_self->getPose();
}

spine_transform_constraint_pose spine_transform_constraint_base_get_applied_pose(spine_transform_constraint_base self) {
	TransformConstraintBase *_self = (TransformConstraintBase *) self;
	return (spine_transform_constraint_pose) &_self->getAppliedPose();
}

void spine_transform_constraint_base_reset_constrained(spine_transform_constraint_base self) {
	TransformConstraintBase *_self = (TransformConstraintBase *) self;
	_self->resetConstrained();
}

void spine_transform_constraint_base_constrained(spine_transform_constraint_base self) {
	TransformConstraintBase *_self = (TransformConstraintBase *) self;
	_self->constrained();
}

bool spine_transform_constraint_base_is_pose_equal_to_applied(spine_transform_constraint_base self) {
	TransformConstraintBase *_self = (TransformConstraintBase *) self;
	return _self->isPoseEqualToApplied();
}

bool spine_transform_constraint_base_is_active(spine_transform_constraint_base self) {
	TransformConstraintBase *_self = (TransformConstraintBase *) self;
	return _self->isActive();
}

void spine_transform_constraint_base_set_active(spine_transform_constraint_base self, bool active) {
	TransformConstraintBase *_self = (TransformConstraintBase *) self;
	_self->setActive(active);
}

spine_rtti spine_transform_constraint_base_get_rtti(spine_transform_constraint_base self) {
	TransformConstraintBase *_self = (TransformConstraintBase *) self;
	return (spine_rtti) &_self->getRTTI();
}

void spine_transform_constraint_base_sort(spine_transform_constraint_base self, spine_skeleton skeleton) {
	TransformConstraintBase *_self = (TransformConstraintBase *) self;
	_self->sort(*((Skeleton *) skeleton));
}

bool spine_transform_constraint_base_is_source_active(spine_transform_constraint_base self) {
	TransformConstraintBase *_self = (TransformConstraintBase *) self;
	return _self->isSourceActive();
}

void spine_transform_constraint_base_update(spine_transform_constraint_base self, spine_skeleton skeleton, spine_physics physics) {
	TransformConstraintBase *_self = (TransformConstraintBase *) self;
	_self->update(*((Skeleton *) skeleton), (Physics) physics);
}

spine_rtti spine_transform_constraint_base_rtti(void) {
	return (spine_rtti) &TransformConstraintBase::rtti;
}
