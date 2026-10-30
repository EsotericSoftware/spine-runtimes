#include "skin.h"
#include <spine/spine.h>

using namespace spine;

spine_skin spine_skin_create(const char *name) {
	return (spine_skin) new (__FILE__, __LINE__) Skin(String(name));
}

void spine_skin_dispose(spine_skin self) {
	delete (Skin *) self;
}

void spine_skin_set_attachment(spine_skin self, size_t slotIndex, const char *placeholder, /*@null*/ spine_attachment attachment) {
	Skin *_self = (Skin *) self;
	_self->setAttachment(slotIndex, String(placeholder), (Attachment *) attachment);
}

/*@null*/ spine_attachment spine_skin_get_attachment(spine_skin self, size_t slotIndex, const char *placeholder) {
	Skin *_self = (Skin *) self;
	return (spine_attachment) _self->getAttachment(slotIndex, String(placeholder));
}

void spine_skin_remove_attachment(spine_skin self, size_t slotIndex, const char *placeholder) {
	Skin *_self = (Skin *) self;
	_self->removeAttachment(slotIndex, String(placeholder));
}

void spine_skin_find_attachments_for_slot(spine_skin self, size_t slotIndex, spine_array_attachment attachments) {
	Skin *_self = (Skin *) self;
	_self->findAttachmentsForSlot(slotIndex, *((Array<Attachment *> *) attachments));
}

const char *spine_skin_get_name(spine_skin self) {
	Skin *_self = (Skin *) self;
	return _self->getName().buffer();
}

void spine_skin_add_skin(spine_skin self, spine_skin other) {
	Skin *_self = (Skin *) self;
	_self->addSkin(*((Skin *) other));
}

void spine_skin_copy_skin(spine_skin self, spine_skin other) {
	Skin *_self = (Skin *) self;
	_self->copySkin(*((Skin *) other));
}

spine_array_bone_data spine_skin_get_bones(spine_skin self) {
	Skin *_self = (Skin *) self;
	return (spine_array_bone_data) &_self->getBones();
}

spine_array_constraint_data spine_skin_get_constraints(spine_skin self) {
	Skin *_self = (Skin *) self;
	return (spine_array_constraint_data) &_self->getConstraints();
}

spine_color spine_skin_get_color(spine_skin self) {
	Skin *_self = (Skin *) self;
	return (spine_color) &_self->getColor();
}
