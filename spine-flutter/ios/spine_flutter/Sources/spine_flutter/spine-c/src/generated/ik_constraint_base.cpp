#include "ik_constraint_base.h"
#include <spine/spine.h>

using namespace spine;

void spine_ik_constraint_base_dispose(spine_ik_constraint_base self) {
	delete (IkConstraintBase *) self;
}

spine_ik_constraint_data spine_ik_constraint_base_get_data(spine_ik_constraint_base self) {
	IkConstraintBase *_self = (IkConstraintBase *) self;
	return (spine_ik_constraint_data) &_self->getData();
}

spine_ik_constraint_pose spine_ik_constraint_base_get_pose(spine_ik_constraint_base self) {
	IkConstraintBase *_self = (IkConstraintBase *) self;
	return (spine_ik_constraint_pose) &_self->getPose();
}

spine_ik_constraint_pose spine_ik_constraint_base_get_applied_pose(spine_ik_constraint_base self) {
	IkConstraintBase *_self = (IkConstraintBase *) self;
	return (spine_ik_constraint_pose) &_self->getAppliedPose();
}

void spine_ik_constraint_base_reset_constrained(spine_ik_constraint_base self) {
	IkConstraintBase *_self = (IkConstraintBase *) self;
	_self->resetConstrained();
}

void spine_ik_constraint_base_constrained(spine_ik_constraint_base self) {
	IkConstraintBase *_self = (IkConstraintBase *) self;
	_self->constrained();
}

bool spine_ik_constraint_base_is_pose_equal_to_applied(spine_ik_constraint_base self) {
	IkConstraintBase *_self = (IkConstraintBase *) self;
	return _self->isPoseEqualToApplied();
}

bool spine_ik_constraint_base_is_active(spine_ik_constraint_base self) {
	IkConstraintBase *_self = (IkConstraintBase *) self;
	return _self->isActive();
}

void spine_ik_constraint_base_set_active(spine_ik_constraint_base self, bool active) {
	IkConstraintBase *_self = (IkConstraintBase *) self;
	_self->setActive(active);
}

spine_rtti spine_ik_constraint_base_get_rtti(spine_ik_constraint_base self) {
	IkConstraintBase *_self = (IkConstraintBase *) self;
	return (spine_rtti) &_self->getRTTI();
}

void spine_ik_constraint_base_sort(spine_ik_constraint_base self, spine_skeleton skeleton) {
	IkConstraintBase *_self = (IkConstraintBase *) self;
	_self->sort(*((Skeleton *) skeleton));
}

bool spine_ik_constraint_base_is_source_active(spine_ik_constraint_base self) {
	IkConstraintBase *_self = (IkConstraintBase *) self;
	return _self->isSourceActive();
}

void spine_ik_constraint_base_update(spine_ik_constraint_base self, spine_skeleton skeleton, spine_physics physics) {
	IkConstraintBase *_self = (IkConstraintBase *) self;
	_self->update(*((Skeleton *) skeleton), (Physics) physics);
}

spine_rtti spine_ik_constraint_base_rtti(void) {
	return (spine_rtti) &IkConstraintBase::rtti;
}
