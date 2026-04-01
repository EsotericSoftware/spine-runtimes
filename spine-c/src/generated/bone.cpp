#include "bone.h"
#include <spine/spine.h>

using namespace spine;

spine_bone spine_bone_create(spine_bone_data data, /*@null*/ spine_bone parent) {
	return (spine_bone) new (__FILE__, __LINE__) Bone(*((BoneData *) data), (Bone *) parent);
}

spine_bone spine_bone_create2(spine_bone bone, /*@null*/ spine_bone parent) {
	return (spine_bone) new (__FILE__, __LINE__) Bone(*((Bone *) bone), (Bone *) parent);
}

void spine_bone_dispose(spine_bone self) {
	delete (Bone *) self;
}

spine_rtti spine_bone_get_rtti(spine_bone self) {
	Bone *_self = (Bone *) self;
	return (spine_rtti) &_self->getRTTI();
}

/*@null*/ spine_bone spine_bone_get_parent(spine_bone self) {
	Bone *_self = (Bone *) self;
	return (spine_bone) _self->getParent();
}

spine_array_bone spine_bone_get_children(spine_bone self) {
	Bone *_self = (Bone *) self;
	return (spine_array_bone) &_self->getChildren();
}

bool spine_bone_is_y_down(void) {
	return Bone::isYDown();
}

void spine_bone_set_y_down(bool value) {
	Bone::setYDown(value);
}

void spine_bone_update(spine_bone self, spine_skeleton skeleton, spine_physics physics) {
	Bone *_self = (Bone *) self;
	_self->update(*((Skeleton *) skeleton), (Physics) physics);
}

spine_bone_data spine_bone_get_data(spine_bone self) {
	Bone *_self = (Bone *) self;
	return (spine_bone_data) &_self->getData();
}

spine_bone_pose spine_bone_get_pose(spine_bone self) {
	Bone *_self = (Bone *) self;
	return (spine_bone_pose) &_self->getPose();
}

spine_bone_pose spine_bone_get_applied_pose(spine_bone self) {
	Bone *_self = (Bone *) self;
	return (spine_bone_pose) &_self->getAppliedPose();
}

void spine_bone_reset_constrained(spine_bone self) {
	Bone *_self = (Bone *) self;
	_self->resetConstrained();
}

void spine_bone_constrained(spine_bone self) {
	Bone *_self = (Bone *) self;
	_self->constrained();
}

bool spine_bone_is_pose_equal_to_applied(spine_bone self) {
	Bone *_self = (Bone *) self;
	return _self->isPoseEqualToApplied();
}

bool spine_bone_is_active(spine_bone self) {
	Bone *_self = (Bone *) self;
	return _self->isActive();
}

void spine_bone_set_active(spine_bone self, bool active) {
	Bone *_self = (Bone *) self;
	_self->setActive(active);
}

spine_rtti spine_bone_rtti(void) {
	return (spine_rtti) &Bone::rtti;
}
