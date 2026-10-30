#include "transform_constraint_data.h"
#include <spine/spine.h>

using namespace spine;

spine_transform_constraint_data spine_transform_constraint_data_create(const char *name) {
	return (spine_transform_constraint_data) new (__FILE__, __LINE__) TransformConstraintData(String(name));
}

void spine_transform_constraint_data_dispose(spine_transform_constraint_data self) {
	delete (TransformConstraintData *) self;
}

spine_rtti spine_transform_constraint_data_get_rtti(spine_transform_constraint_data self) {
	TransformConstraintData *_self = (TransformConstraintData *) self;
	return (spine_rtti) &_self->getRTTI();
}

spine_constraint spine_transform_constraint_data_create_method(spine_transform_constraint_data self, spine_skeleton skeleton) {
	TransformConstraintData *_self = (TransformConstraintData *) self;
	return (spine_constraint) &_self->create(*((Skeleton *) skeleton));
}

spine_array_bone_data spine_transform_constraint_data_get_bones(spine_transform_constraint_data self) {
	TransformConstraintData *_self = (TransformConstraintData *) self;
	return (spine_array_bone_data) &_self->getBones();
}

spine_bone_data spine_transform_constraint_data_get_source(spine_transform_constraint_data self) {
	TransformConstraintData *_self = (TransformConstraintData *) self;
	return (spine_bone_data) &_self->getSource();
}

void spine_transform_constraint_data_set_source(spine_transform_constraint_data self, spine_bone_data source) {
	TransformConstraintData *_self = (TransformConstraintData *) self;
	_self->setSource(*((BoneData *) source));
}

float spine_transform_constraint_data_get_offset_rotation(spine_transform_constraint_data self) {
	TransformConstraintData *_self = (TransformConstraintData *) self;
	return _self->getOffsetRotation();
}

void spine_transform_constraint_data_set_offset_rotation(spine_transform_constraint_data self, float offsetRotation) {
	TransformConstraintData *_self = (TransformConstraintData *) self;
	_self->setOffsetRotation(offsetRotation);
}

float spine_transform_constraint_data_get_offset_x(spine_transform_constraint_data self) {
	TransformConstraintData *_self = (TransformConstraintData *) self;
	return _self->getOffsetX();
}

void spine_transform_constraint_data_set_offset_x(spine_transform_constraint_data self, float offsetX) {
	TransformConstraintData *_self = (TransformConstraintData *) self;
	_self->setOffsetX(offsetX);
}

float spine_transform_constraint_data_get_offset_y(spine_transform_constraint_data self) {
	TransformConstraintData *_self = (TransformConstraintData *) self;
	return _self->getOffsetY();
}

void spine_transform_constraint_data_set_offset_y(spine_transform_constraint_data self, float offsetY) {
	TransformConstraintData *_self = (TransformConstraintData *) self;
	_self->setOffsetY(offsetY);
}

float spine_transform_constraint_data_get_offset_scale_x(spine_transform_constraint_data self) {
	TransformConstraintData *_self = (TransformConstraintData *) self;
	return _self->getOffsetScaleX();
}

void spine_transform_constraint_data_set_offset_scale_x(spine_transform_constraint_data self, float offsetScaleX) {
	TransformConstraintData *_self = (TransformConstraintData *) self;
	_self->setOffsetScaleX(offsetScaleX);
}

float spine_transform_constraint_data_get_offset_scale_y(spine_transform_constraint_data self) {
	TransformConstraintData *_self = (TransformConstraintData *) self;
	return _self->getOffsetScaleY();
}

void spine_transform_constraint_data_set_offset_scale_y(spine_transform_constraint_data self, float offsetScaleY) {
	TransformConstraintData *_self = (TransformConstraintData *) self;
	_self->setOffsetScaleY(offsetScaleY);
}

float spine_transform_constraint_data_get_offset_shear_y(spine_transform_constraint_data self) {
	TransformConstraintData *_self = (TransformConstraintData *) self;
	return _self->getOffsetShearY();
}

void spine_transform_constraint_data_set_offset_shear_y(spine_transform_constraint_data self, float offsetShearY) {
	TransformConstraintData *_self = (TransformConstraintData *) self;
	_self->setOffsetShearY(offsetShearY);
}

bool spine_transform_constraint_data_get_local_source(spine_transform_constraint_data self) {
	TransformConstraintData *_self = (TransformConstraintData *) self;
	return _self->getLocalSource();
}

void spine_transform_constraint_data_set_local_source(spine_transform_constraint_data self, bool localSource) {
	TransformConstraintData *_self = (TransformConstraintData *) self;
	_self->setLocalSource(localSource);
}

bool spine_transform_constraint_data_get_local_target(spine_transform_constraint_data self) {
	TransformConstraintData *_self = (TransformConstraintData *) self;
	return _self->getLocalTarget();
}

void spine_transform_constraint_data_set_local_target(spine_transform_constraint_data self, bool localTarget) {
	TransformConstraintData *_self = (TransformConstraintData *) self;
	_self->setLocalTarget(localTarget);
}

bool spine_transform_constraint_data_get_additive(spine_transform_constraint_data self) {
	TransformConstraintData *_self = (TransformConstraintData *) self;
	return _self->getAdditive();
}

void spine_transform_constraint_data_set_additive(spine_transform_constraint_data self, bool additive) {
	TransformConstraintData *_self = (TransformConstraintData *) self;
	_self->setAdditive(additive);
}

bool spine_transform_constraint_data_get_clamp(spine_transform_constraint_data self) {
	TransformConstraintData *_self = (TransformConstraintData *) self;
	return _self->getClamp();
}

void spine_transform_constraint_data_set_clamp(spine_transform_constraint_data self, bool clamp) {
	TransformConstraintData *_self = (TransformConstraintData *) self;
	_self->setClamp(clamp);
}

spine_array_from_property spine_transform_constraint_data_get_properties(spine_transform_constraint_data self) {
	TransformConstraintData *_self = (TransformConstraintData *) self;
	return (spine_array_from_property) &_self->getProperties();
}

const char *spine_transform_constraint_data_get_name(spine_transform_constraint_data self) {
	TransformConstraintData *_self = (TransformConstraintData *) self;
	return _self->getName().buffer();
}

bool spine_transform_constraint_data_get_skin_required(spine_transform_constraint_data self) {
	TransformConstraintData *_self = (TransformConstraintData *) self;
	return _self->getSkinRequired();
}

spine_transform_constraint_pose spine_transform_constraint_data_get_setup_pose(spine_transform_constraint_data self) {
	TransformConstraintData *_self = (TransformConstraintData *) self;
	return (spine_transform_constraint_pose) &_self->getSetupPose();
}

void spine_transform_constraint_data_set_skin_required(spine_transform_constraint_data self, bool skinRequired) {
	TransformConstraintData *_self = (TransformConstraintData *) self;
	_self->setSkinRequired(skinRequired);
}

spine_rtti spine_transform_constraint_data_rtti(void) {
	return (spine_rtti) &TransformConstraintData::rtti;
}
