#include "slot_pose.h"
#include <spine/spine.h>

using namespace spine;

spine_slot_pose spine_slot_pose_create(void) {
	return (spine_slot_pose) new (__FILE__, __LINE__) SlotPose();
}

void spine_slot_pose_dispose(spine_slot_pose self) {
	delete (SlotPose *) self;
}

void spine_slot_pose_set(spine_slot_pose self, spine_slot_pose pose) {
	SlotPose *_self = (SlotPose *) self;
	_self->set(*((SlotPose *) pose));
}

spine_color spine_slot_pose_get_color(spine_slot_pose self) {
	SlotPose *_self = (SlotPose *) self;
	return (spine_color) &_self->getColor();
}

spine_color spine_slot_pose_get_dark_color(spine_slot_pose self) {
	SlotPose *_self = (SlotPose *) self;
	return (spine_color) &_self->getDarkColor();
}

bool spine_slot_pose_has_dark_color(spine_slot_pose self) {
	SlotPose *_self = (SlotPose *) self;
	return _self->hasDarkColor();
}

void spine_slot_pose_set_has_dark_color(spine_slot_pose self, bool hasDarkColor) {
	SlotPose *_self = (SlotPose *) self;
	_self->setHasDarkColor(hasDarkColor);
}

/*@null*/ spine_attachment spine_slot_pose_get_attachment(spine_slot_pose self) {
	SlotPose *_self = (SlotPose *) self;
	return (spine_attachment) _self->getAttachment();
}

void spine_slot_pose_set_attachment(spine_slot_pose self, /*@null*/ spine_attachment attachment) {
	SlotPose *_self = (SlotPose *) self;
	_self->setAttachment((Attachment *) attachment);
}

int spine_slot_pose_get_sequence_index(spine_slot_pose self) {
	SlotPose *_self = (SlotPose *) self;
	return _self->getSequenceIndex();
}

void spine_slot_pose_set_sequence_index(spine_slot_pose self, int sequenceIndex) {
	SlotPose *_self = (SlotPose *) self;
	_self->setSequenceIndex(sequenceIndex);
}

spine_array_float spine_slot_pose_get_deform(spine_slot_pose self) {
	SlotPose *_self = (SlotPose *) self;
	return (spine_array_float) &_self->getDeform();
}
