#include "transform_constraint.h"
#include <spine/spine.h>

using namespace spine;

spine_transform_constraint spine_transform_constraint_create(spine_transform_constraint_data data, spine_skeleton skeleton) {
	return (spine_transform_constraint) new (__FILE__, __LINE__) TransformConstraint(*((TransformConstraintData *) data), *((Skeleton *) skeleton));
}

void spine_transform_constraint_dispose(spine_transform_constraint self) {
	delete (TransformConstraint *) self;
}

spine_rtti spine_transform_constraint_get_rtti(spine_transform_constraint self) {
	TransformConstraint *_self = (TransformConstraint *) self;
	return (spine_rtti) &_self->getRTTI();
}

spine_transform_constraint spine_transform_constraint_copy(spine_transform_constraint self, spine_skeleton skeleton) {
	TransformConstraint *_self = (TransformConstraint *) self;
	return (spine_transform_constraint) &_self->copy(*((Skeleton *) skeleton));
}

void spine_transform_constraint_update(spine_transform_constraint self, spine_skeleton skeleton, spine_physics physics) {
	TransformConstraint *_self = (TransformConstraint *) self;
	_self->update(*((Skeleton *) skeleton), (Physics) physics);
}

void spine_transform_constraint_sort(spine_transform_constraint self, spine_skeleton skeleton) {
	TransformConstraint *_self = (TransformConstraint *) self;
	_self->sort(*((Skeleton *) skeleton));
}

bool spine_transform_constraint_is_source_active(spine_transform_constraint self) {
	TransformConstraint *_self = (TransformConstraint *) self;
	return _self->isSourceActive();
}

spine_array_bone_pose spine_transform_constraint_get_bones(spine_transform_constraint self) {
	TransformConstraint *_self = (TransformConstraint *) self;
	return (spine_array_bone_pose) &_self->getBones();
}

spine_bone spine_transform_constraint_get_source(spine_transform_constraint self) {
	TransformConstraint *_self = (TransformConstraint *) self;
	return (spine_bone) &_self->getSource();
}

void spine_transform_constraint_set_source(spine_transform_constraint self, spine_bone source) {
	TransformConstraint *_self = (TransformConstraint *) self;
	_self->setSource(*((Bone *) source));
}

spine_transform_constraint_data spine_transform_constraint_get_data(spine_transform_constraint self) {
	TransformConstraint *_self = (TransformConstraint *) self;
	return (spine_transform_constraint_data) &_self->getData();
}

spine_transform_constraint_pose spine_transform_constraint_get_pose(spine_transform_constraint self) {
	TransformConstraint *_self = (TransformConstraint *) self;
	return (spine_transform_constraint_pose) &_self->getPose();
}

spine_transform_constraint_pose spine_transform_constraint_get_applied_pose(spine_transform_constraint self) {
	TransformConstraint *_self = (TransformConstraint *) self;
	return (spine_transform_constraint_pose) &_self->getAppliedPose();
}

void spine_transform_constraint_reset_constrained(spine_transform_constraint self) {
	TransformConstraint *_self = (TransformConstraint *) self;
	_self->resetConstrained();
}

void spine_transform_constraint_constrained(spine_transform_constraint self) {
	TransformConstraint *_self = (TransformConstraint *) self;
	_self->constrained();
}

bool spine_transform_constraint_is_pose_equal_to_applied(spine_transform_constraint self) {
	TransformConstraint *_self = (TransformConstraint *) self;
	return _self->isPoseEqualToApplied();
}

bool spine_transform_constraint_is_active(spine_transform_constraint self) {
	TransformConstraint *_self = (TransformConstraint *) self;
	return _self->isActive();
}

void spine_transform_constraint_set_active(spine_transform_constraint self, bool active) {
	TransformConstraint *_self = (TransformConstraint *) self;
	_self->setActive(active);
}

spine_rtti spine_transform_constraint_rtti(void) {
	return (spine_rtti) &TransformConstraint::rtti;
}
