#include "slot_data.h"
#include <spine/spine.h>

using namespace spine;

spine_slot_data spine_slot_data_create(int index, const char *name, spine_bone_data boneData) {
	return (spine_slot_data) new (__FILE__, __LINE__) SlotData(index, String(name), *((BoneData *) boneData));
}

void spine_slot_data_dispose(spine_slot_data self) {
	delete (SlotData *) self;
}

int spine_slot_data_get_index(spine_slot_data self) {
	SlotData *_self = (SlotData *) self;
	return _self->getIndex();
}

spine_bone_data spine_slot_data_get_bone_data(spine_slot_data self) {
	SlotData *_self = (SlotData *) self;
	return (spine_bone_data) &_self->getBoneData();
}

void spine_slot_data_set_attachment_name(spine_slot_data self, const char *attachmentName) {
	SlotData *_self = (SlotData *) self;
	_self->setAttachmentName(String(attachmentName));
}

const char *spine_slot_data_get_attachment_name(spine_slot_data self) {
	SlotData *_self = (SlotData *) self;
	return _self->getAttachmentName().buffer();
}

spine_blend_mode spine_slot_data_get_blend_mode(spine_slot_data self) {
	SlotData *_self = (SlotData *) self;
	return (spine_blend_mode) _self->getBlendMode();
}

void spine_slot_data_set_blend_mode(spine_slot_data self, spine_blend_mode blendMode) {
	SlotData *_self = (SlotData *) self;
	_self->setBlendMode((BlendMode) blendMode);
}

bool spine_slot_data_get_visible(spine_slot_data self) {
	SlotData *_self = (SlotData *) self;
	return _self->getVisible();
}

void spine_slot_data_set_visible(spine_slot_data self, bool visible) {
	SlotData *_self = (SlotData *) self;
	_self->setVisible(visible);
}

spine_slot_pose spine_slot_data_get_setup_pose(spine_slot_data self) {
	SlotData *_self = (SlotData *) self;
	return (spine_slot_pose) &_self->getSetupPose();
}

const char *spine_slot_data_get_name(spine_slot_data self) {
	SlotData *_self = (SlotData *) self;
	return _self->getName().buffer();
}

bool spine_slot_data_get_skin_required(spine_slot_data self) {
	SlotData *_self = (SlotData *) self;
	return _self->getSkinRequired();
}

void spine_slot_data_set_skin_required(spine_slot_data self, bool skinRequired) {
	SlotData *_self = (SlotData *) self;
	_self->setSkinRequired(skinRequired);
}
