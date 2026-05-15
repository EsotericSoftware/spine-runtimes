#include "ik_constraint_data.h"
#include <spine/spine.h>

using namespace spine;

spine_ik_constraint_data spine_ik_constraint_data_create(const char *name) {
	return (spine_ik_constraint_data) new (__FILE__, __LINE__) IkConstraintData(String(name));
}

void spine_ik_constraint_data_dispose(spine_ik_constraint_data self) {
	delete (IkConstraintData *) self;
}

spine_rtti spine_ik_constraint_data_get_rtti(spine_ik_constraint_data self) {
	IkConstraintData *_self = (IkConstraintData *) self;
	return (spine_rtti) &_self->getRTTI();
}

spine_constraint spine_ik_constraint_data_create_method(spine_ik_constraint_data self, spine_skeleton skeleton) {
	IkConstraintData *_self = (IkConstraintData *) self;
	return (spine_constraint) &_self->create(*((Skeleton *) skeleton));
}

spine_array_bone_data spine_ik_constraint_data_get_bones(spine_ik_constraint_data self) {
	IkConstraintData *_self = (IkConstraintData *) self;
	return (spine_array_bone_data) &_self->getBones();
}

spine_bone_data spine_ik_constraint_data_get_target(spine_ik_constraint_data self) {
	IkConstraintData *_self = (IkConstraintData *) self;
	return (spine_bone_data) &_self->getTarget();
}

void spine_ik_constraint_data_set_target(spine_ik_constraint_data self, spine_bone_data inValue) {
	IkConstraintData *_self = (IkConstraintData *) self;
	_self->setTarget(*((BoneData *) inValue));
}

spine_scale_y_mode spine_ik_constraint_data_get_scale_y_mode(spine_ik_constraint_data self) {
	IkConstraintData *_self = (IkConstraintData *) self;
	return (spine_scale_y_mode) _self->getScaleYMode();
}

void spine_ik_constraint_data_set_scale_y_mode(spine_ik_constraint_data self, spine_scale_y_mode scaleYMode) {
	IkConstraintData *_self = (IkConstraintData *) self;
	_self->setScaleYMode((ScaleYMode) scaleYMode);
}

const char *spine_ik_constraint_data_get_name(spine_ik_constraint_data self) {
	IkConstraintData *_self = (IkConstraintData *) self;
	return _self->getName().buffer();
}

bool spine_ik_constraint_data_get_skin_required(spine_ik_constraint_data self) {
	IkConstraintData *_self = (IkConstraintData *) self;
	return _self->getSkinRequired();
}

spine_ik_constraint_pose spine_ik_constraint_data_get_setup_pose(spine_ik_constraint_data self) {
	IkConstraintData *_self = (IkConstraintData *) self;
	return (spine_ik_constraint_pose) &_self->getSetupPose();
}

void spine_ik_constraint_data_set_skin_required(spine_ik_constraint_data self, bool skinRequired) {
	IkConstraintData *_self = (IkConstraintData *) self;
	_self->setSkinRequired(skinRequired);
}

spine_rtti spine_ik_constraint_data_rtti(void) {
	return (spine_rtti) &IkConstraintData::rtti;
}
