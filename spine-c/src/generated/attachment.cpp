#include "attachment.h"
#include <spine/spine.h>

using namespace spine;

void spine_attachment_dispose(spine_attachment self) {
	delete (Attachment *) self;
}

spine_rtti spine_attachment_get_rtti(spine_attachment self) {
	Attachment *_self = (Attachment *) self;
	return (spine_rtti) &_self->getRTTI();
}

const char *spine_attachment_get_name(spine_attachment self) {
	Attachment *_self = (Attachment *) self;
	return _self->getName().buffer();
}

spine_attachment spine_attachment_copy(spine_attachment self) {
	Attachment *_self = (Attachment *) self;
	return (spine_attachment) &_self->copy();
}

/*@null*/ spine_attachment spine_attachment_get_timeline_attachment(spine_attachment self) {
	Attachment *_self = (Attachment *) self;
	return (spine_attachment) _self->getTimelineAttachment();
}

void spine_attachment_set_timeline_attachment(spine_attachment self, /*@null*/ spine_attachment attachment) {
	Attachment *_self = (Attachment *) self;
	_self->setTimelineAttachment((Attachment *) attachment);
}

spine_array_int spine_attachment_get_timeline_slots(spine_attachment self) {
	Attachment *_self = (Attachment *) self;
	return (spine_array_int) &_self->getTimelineSlots();
}

void spine_attachment_set_timeline_slots(spine_attachment self, spine_array_int timelineSlots) {
	Attachment *_self = (Attachment *) self;
	_self->setTimelineSlots(*((Array<int> *) timelineSlots));
}

bool spine_attachment_is_timeline_active(spine_attachment self, spine_array_slot slots, int slotIndex, bool appliedPose) {
	Attachment *_self = (Attachment *) self;
	return _self->isTimelineActive(*((Array<Slot *> *) slots), slotIndex, appliedPose);
}

int spine_attachment_get_ref_count(spine_attachment self) {
	Attachment *_self = (Attachment *) self;
	return _self->getRefCount();
}

void spine_attachment_reference(spine_attachment self) {
	Attachment *_self = (Attachment *) self;
	_self->reference();
}

void spine_attachment_dereference(spine_attachment self) {
	Attachment *_self = (Attachment *) self;
	_self->dereference();
}

spine_rtti spine_attachment_rtti(void) {
	return (spine_rtti) &Attachment::rtti;
}
