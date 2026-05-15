#include "bone_data.h"
#include <spine/spine.h>

using namespace spine;

spine_bone_data spine_bone_data_create(int index, const char *name, /*@null*/ spine_bone_data parent) {
	return (spine_bone_data) new (__FILE__, __LINE__) BoneData(index, String(name), (BoneData *) parent);
}

void spine_bone_data_dispose(spine_bone_data self) {
	delete (BoneData *) self;
}

int spine_bone_data_get_index(spine_bone_data self) {
	BoneData *_self = (BoneData *) self;
	return _self->getIndex();
}

/*@null*/ spine_bone_data spine_bone_data_get_parent(spine_bone_data self) {
	BoneData *_self = (BoneData *) self;
	return (spine_bone_data) _self->getParent();
}

float spine_bone_data_get_length(spine_bone_data self) {
	BoneData *_self = (BoneData *) self;
	return _self->getLength();
}

void spine_bone_data_set_length(spine_bone_data self, float inValue) {
	BoneData *_self = (BoneData *) self;
	_self->setLength(inValue);
}

spine_color spine_bone_data_get_color(spine_bone_data self) {
	BoneData *_self = (BoneData *) self;
	return (spine_color) &_self->getColor();
}

const char *spine_bone_data_get_icon(spine_bone_data self) {
	BoneData *_self = (BoneData *) self;
	return _self->getIcon().buffer();
}

void spine_bone_data_set_icon(spine_bone_data self, const char *icon) {
	BoneData *_self = (BoneData *) self;
	_self->setIcon(String(icon));
}

float spine_bone_data_get_icon_size(spine_bone_data self) {
	BoneData *_self = (BoneData *) self;
	return _self->getIconSize();
}

void spine_bone_data_set_icon_size(spine_bone_data self, float iconSize) {
	BoneData *_self = (BoneData *) self;
	_self->setIconSize(iconSize);
}

float spine_bone_data_get_icon_rotation(spine_bone_data self) {
	BoneData *_self = (BoneData *) self;
	return _self->getIconRotation();
}

void spine_bone_data_set_icon_rotation(spine_bone_data self, float iconRotation) {
	BoneData *_self = (BoneData *) self;
	_self->setIconRotation(iconRotation);
}

bool spine_bone_data_get_visible(spine_bone_data self) {
	BoneData *_self = (BoneData *) self;
	return _self->getVisible();
}

void spine_bone_data_set_visible(spine_bone_data self, bool inValue) {
	BoneData *_self = (BoneData *) self;
	_self->setVisible(inValue);
}

spine_bone_pose spine_bone_data_get_setup_pose(spine_bone_data self) {
	BoneData *_self = (BoneData *) self;
	return (spine_bone_pose) &_self->getSetupPose();
}

const char *spine_bone_data_get_name(spine_bone_data self) {
	BoneData *_self = (BoneData *) self;
	return _self->getName().buffer();
}

bool spine_bone_data_get_skin_required(spine_bone_data self) {
	BoneData *_self = (BoneData *) self;
	return _self->getSkinRequired();
}

void spine_bone_data_set_skin_required(spine_bone_data self, bool skinRequired) {
	BoneData *_self = (BoneData *) self;
	_self->setSkinRequired(skinRequired);
}
