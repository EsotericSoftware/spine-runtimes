#include "ik_constraint.h"
#include <spine/spine.h>

using namespace spine;

spine_ik_constraint spine_ik_constraint_create(spine_ik_constraint_data data, spine_skeleton skeleton) {
	return (spine_ik_constraint) new (__FILE__, __LINE__) IkConstraint(*((IkConstraintData *) data), *((Skeleton *) skeleton));
}

void spine_ik_constraint_dispose(spine_ik_constraint self) {
	delete (IkConstraint *) self;
}

spine_rtti spine_ik_constraint_get_rtti(spine_ik_constraint self) {
	IkConstraint *_self = (IkConstraint *) self;
	return (spine_rtti) &_self->getRTTI();
}

spine_ik_constraint spine_ik_constraint_copy(spine_ik_constraint self, spine_skeleton skeleton) {
	IkConstraint *_self = (IkConstraint *) self;
	return (spine_ik_constraint) &_self->copy(*((Skeleton *) skeleton));
}

void spine_ik_constraint_update(spine_ik_constraint self, spine_skeleton skeleton, spine_physics physics) {
	IkConstraint *_self = (IkConstraint *) self;
	_self->update(*((Skeleton *) skeleton), (Physics) physics);
}

void spine_ik_constraint_sort(spine_ik_constraint self, spine_skeleton skeleton) {
	IkConstraint *_self = (IkConstraint *) self;
	_self->sort(*((Skeleton *) skeleton));
}

bool spine_ik_constraint_is_source_active(spine_ik_constraint self) {
	IkConstraint *_self = (IkConstraint *) self;
	return _self->isSourceActive();
}

spine_array_bone_pose spine_ik_constraint_get_bones(spine_ik_constraint self) {
	IkConstraint *_self = (IkConstraint *) self;
	return (spine_array_bone_pose) &_self->getBones();
}

spine_bone spine_ik_constraint_get_target(spine_ik_constraint self) {
	IkConstraint *_self = (IkConstraint *) self;
	return (spine_bone) &_self->getTarget();
}

void spine_ik_constraint_set_target(spine_ik_constraint self, spine_bone inValue) {
	IkConstraint *_self = (IkConstraint *) self;
	_self->setTarget(*((Bone *) inValue));
}

void spine_ik_constraint_apply_1(spine_skeleton skeleton, spine_bone_pose bone, float targetX, float targetY, bool compress, bool stretch,
								 spine_scale_y scaleY, float mix) {
	IkConstraint::apply(*((Skeleton *) skeleton), *((BonePose *) bone), targetX, targetY, compress, stretch, (ScaleY) scaleY, mix);
}

void spine_ik_constraint_apply_2(spine_skeleton skeleton, spine_bone_pose parent, spine_bone_pose child, float targetX, float targetY,
								 int bendDirection, bool stretch, spine_scale_y scaleY, float softness, float mix) {
	IkConstraint::apply(*((Skeleton *) skeleton), *((BonePose *) parent), *((BonePose *) child), targetX, targetY, bendDirection, stretch,
						(ScaleY) scaleY, softness, mix);
}

spine_ik_constraint_data spine_ik_constraint_get_data(spine_ik_constraint self) {
	IkConstraint *_self = (IkConstraint *) self;
	return (spine_ik_constraint_data) &_self->getData();
}

spine_ik_constraint_pose spine_ik_constraint_get_pose(spine_ik_constraint self) {
	IkConstraint *_self = (IkConstraint *) self;
	return (spine_ik_constraint_pose) &_self->getPose();
}

spine_ik_constraint_pose spine_ik_constraint_get_applied_pose(spine_ik_constraint self) {
	IkConstraint *_self = (IkConstraint *) self;
	return (spine_ik_constraint_pose) &_self->getAppliedPose();
}

void spine_ik_constraint_reset_constrained(spine_ik_constraint self) {
	IkConstraint *_self = (IkConstraint *) self;
	_self->resetConstrained();
}

void spine_ik_constraint_constrained(spine_ik_constraint self) {
	IkConstraint *_self = (IkConstraint *) self;
	_self->constrained();
}

bool spine_ik_constraint_is_pose_equal_to_applied(spine_ik_constraint self) {
	IkConstraint *_self = (IkConstraint *) self;
	return _self->isPoseEqualToApplied();
}

bool spine_ik_constraint_is_active(spine_ik_constraint self) {
	IkConstraint *_self = (IkConstraint *) self;
	return _self->isActive();
}

void spine_ik_constraint_set_active(spine_ik_constraint self, bool active) {
	IkConstraint *_self = (IkConstraint *) self;
	_self->setActive(active);
}

spine_rtti spine_ik_constraint_rtti(void) {
	return (spine_rtti) &IkConstraint::rtti;
}
