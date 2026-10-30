#include "path_constraint_data.h"
#include <spine/spine.h>

using namespace spine;

spine_path_constraint_data spine_path_constraint_data_create(const char *name) {
	return (spine_path_constraint_data) new (__FILE__, __LINE__) PathConstraintData(String(name));
}

void spine_path_constraint_data_dispose(spine_path_constraint_data self) {
	delete (PathConstraintData *) self;
}

spine_rtti spine_path_constraint_data_get_rtti(spine_path_constraint_data self) {
	PathConstraintData *_self = (PathConstraintData *) self;
	return (spine_rtti) &_self->getRTTI();
}

spine_constraint spine_path_constraint_data_create_method(spine_path_constraint_data self, spine_skeleton skeleton) {
	PathConstraintData *_self = (PathConstraintData *) self;
	return (spine_constraint) &_self->create(*((Skeleton *) skeleton));
}

spine_array_bone_data spine_path_constraint_data_get_bones(spine_path_constraint_data self) {
	PathConstraintData *_self = (PathConstraintData *) self;
	return (spine_array_bone_data) &_self->getBones();
}

spine_slot_data spine_path_constraint_data_get_slot(spine_path_constraint_data self) {
	PathConstraintData *_self = (PathConstraintData *) self;
	return (spine_slot_data) &_self->getSlot();
}

void spine_path_constraint_data_set_slot(spine_path_constraint_data self, spine_slot_data slot) {
	PathConstraintData *_self = (PathConstraintData *) self;
	_self->setSlot(*((SlotData *) slot));
}

spine_position_mode spine_path_constraint_data_get_position_mode(spine_path_constraint_data self) {
	PathConstraintData *_self = (PathConstraintData *) self;
	return (spine_position_mode) _self->getPositionMode();
}

void spine_path_constraint_data_set_position_mode(spine_path_constraint_data self, spine_position_mode positionMode) {
	PathConstraintData *_self = (PathConstraintData *) self;
	_self->setPositionMode((PositionMode) positionMode);
}

spine_spacing_mode spine_path_constraint_data_get_spacing_mode(spine_path_constraint_data self) {
	PathConstraintData *_self = (PathConstraintData *) self;
	return (spine_spacing_mode) _self->getSpacingMode();
}

void spine_path_constraint_data_set_spacing_mode(spine_path_constraint_data self, spine_spacing_mode spacingMode) {
	PathConstraintData *_self = (PathConstraintData *) self;
	_self->setSpacingMode((SpacingMode) spacingMode);
}

spine_rotate_mode spine_path_constraint_data_get_rotate_mode(spine_path_constraint_data self) {
	PathConstraintData *_self = (PathConstraintData *) self;
	return (spine_rotate_mode) _self->getRotateMode();
}

void spine_path_constraint_data_set_rotate_mode(spine_path_constraint_data self, spine_rotate_mode rotateMode) {
	PathConstraintData *_self = (PathConstraintData *) self;
	_self->setRotateMode((RotateMode) rotateMode);
}

float spine_path_constraint_data_get_offset_rotation(spine_path_constraint_data self) {
	PathConstraintData *_self = (PathConstraintData *) self;
	return _self->getOffsetRotation();
}

void spine_path_constraint_data_set_offset_rotation(spine_path_constraint_data self, float offsetRotation) {
	PathConstraintData *_self = (PathConstraintData *) self;
	_self->setOffsetRotation(offsetRotation);
}

const char *spine_path_constraint_data_get_name(spine_path_constraint_data self) {
	PathConstraintData *_self = (PathConstraintData *) self;
	return _self->getName().buffer();
}

bool spine_path_constraint_data_get_skin_required(spine_path_constraint_data self) {
	PathConstraintData *_self = (PathConstraintData *) self;
	return _self->getSkinRequired();
}

spine_path_constraint_pose spine_path_constraint_data_get_setup_pose(spine_path_constraint_data self) {
	PathConstraintData *_self = (PathConstraintData *) self;
	return (spine_path_constraint_pose) &_self->getSetupPose();
}

void spine_path_constraint_data_set_skin_required(spine_path_constraint_data self, bool skinRequired) {
	PathConstraintData *_self = (PathConstraintData *) self;
	_self->setSkinRequired(skinRequired);
}

spine_rtti spine_path_constraint_data_rtti(void) {
	return (spine_rtti) &PathConstraintData::rtti;
}
