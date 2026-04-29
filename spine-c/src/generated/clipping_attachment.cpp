#include "clipping_attachment.h"
#include <spine/spine.h>

using namespace spine;

spine_clipping_attachment spine_clipping_attachment_create(const char *name) {
	return (spine_clipping_attachment) new (__FILE__, __LINE__) ClippingAttachment(String(name));
}

void spine_clipping_attachment_dispose(spine_clipping_attachment self) {
	delete (ClippingAttachment *) self;
}

spine_rtti spine_clipping_attachment_get_rtti(spine_clipping_attachment self) {
	ClippingAttachment *_self = (ClippingAttachment *) self;
	return (spine_rtti) &_self->getRTTI();
}

/*@null*/ spine_slot_data spine_clipping_attachment_get_end_slot(spine_clipping_attachment self) {
	ClippingAttachment *_self = (ClippingAttachment *) self;
	return (spine_slot_data) _self->getEndSlot();
}

void spine_clipping_attachment_set_end_slot(spine_clipping_attachment self, /*@null*/ spine_slot_data inValue) {
	ClippingAttachment *_self = (ClippingAttachment *) self;
	_self->setEndSlot((SlotData *) inValue);
}

bool spine_clipping_attachment_get_convex(spine_clipping_attachment self) {
	ClippingAttachment *_self = (ClippingAttachment *) self;
	return _self->getConvex();
}

void spine_clipping_attachment_set_convex(spine_clipping_attachment self, bool convex) {
	ClippingAttachment *_self = (ClippingAttachment *) self;
	_self->setConvex(convex);
}

bool spine_clipping_attachment_get_inverse(spine_clipping_attachment self) {
	ClippingAttachment *_self = (ClippingAttachment *) self;
	return _self->getInverse();
}

void spine_clipping_attachment_set_inverse(spine_clipping_attachment self, bool inverse) {
	ClippingAttachment *_self = (ClippingAttachment *) self;
	_self->setInverse(inverse);
}

spine_color spine_clipping_attachment_get_color(spine_clipping_attachment self) {
	ClippingAttachment *_self = (ClippingAttachment *) self;
	return (spine_color) &_self->getColor();
}

spine_attachment spine_clipping_attachment_copy(spine_clipping_attachment self) {
	ClippingAttachment *_self = (ClippingAttachment *) self;
	return (spine_attachment) &_self->copy();
}

void spine_clipping_attachment_compute_world_vertices_1(spine_clipping_attachment self, spine_skeleton skeleton, spine_slot slot, size_t start,
														size_t count, /*@null*/ float *worldVertices, size_t offset, size_t stride) {
	ClippingAttachment *_self = (ClippingAttachment *) self;
	_self->computeWorldVertices(*((Skeleton *) skeleton), *((Slot *) slot), start, count, worldVertices, offset, stride);
}

void spine_clipping_attachment_compute_world_vertices_2(spine_clipping_attachment self, spine_skeleton skeleton, spine_slot slot, size_t start,
														size_t count, spine_array_float worldVertices, size_t offset, size_t stride) {
	ClippingAttachment *_self = (ClippingAttachment *) self;
	_self->computeWorldVertices(*((Skeleton *) skeleton), *((Slot *) slot), start, count, *((Array<float> *) worldVertices), offset, stride);
}

int spine_clipping_attachment_get_id(spine_clipping_attachment self) {
	ClippingAttachment *_self = (ClippingAttachment *) self;
	return _self->getId();
}

spine_array_int spine_clipping_attachment_get_bones(spine_clipping_attachment self) {
	ClippingAttachment *_self = (ClippingAttachment *) self;
	return (spine_array_int) &_self->getBones();
}

void spine_clipping_attachment_set_bones(spine_clipping_attachment self, spine_array_int bones) {
	ClippingAttachment *_self = (ClippingAttachment *) self;
	_self->setBones(*((Array<int> *) bones));
}

spine_array_float spine_clipping_attachment_get_vertices(spine_clipping_attachment self) {
	ClippingAttachment *_self = (ClippingAttachment *) self;
	return (spine_array_float) &_self->getVertices();
}

void spine_clipping_attachment_set_vertices(spine_clipping_attachment self, spine_array_float vertices) {
	ClippingAttachment *_self = (ClippingAttachment *) self;
	_self->setVertices(*((Array<float> *) vertices));
}

size_t spine_clipping_attachment_get_world_vertices_length(spine_clipping_attachment self) {
	ClippingAttachment *_self = (ClippingAttachment *) self;
	return _self->getWorldVerticesLength();
}

void spine_clipping_attachment_set_world_vertices_length(spine_clipping_attachment self, size_t inValue) {
	ClippingAttachment *_self = (ClippingAttachment *) self;
	_self->setWorldVerticesLength(inValue);
}

/*@null*/ spine_attachment spine_clipping_attachment_get_timeline_attachment(spine_clipping_attachment self) {
	ClippingAttachment *_self = (ClippingAttachment *) self;
	return (spine_attachment) _self->getTimelineAttachment();
}

void spine_clipping_attachment_set_timeline_attachment(spine_clipping_attachment self, /*@null*/ spine_attachment attachment) {
	ClippingAttachment *_self = (ClippingAttachment *) self;
	_self->setTimelineAttachment((Attachment *) attachment);
}

void spine_clipping_attachment_copy_to(spine_clipping_attachment self, spine_vertex_attachment other) {
	ClippingAttachment *_self = (ClippingAttachment *) self;
	_self->copyTo(*((VertexAttachment *) other));
}

const char *spine_clipping_attachment_get_name(spine_clipping_attachment self) {
	ClippingAttachment *_self = (ClippingAttachment *) self;
	return _self->getName().buffer();
}

spine_array_int spine_clipping_attachment_get_timeline_slots(spine_clipping_attachment self) {
	ClippingAttachment *_self = (ClippingAttachment *) self;
	return (spine_array_int) &_self->getTimelineSlots();
}

void spine_clipping_attachment_set_timeline_slots(spine_clipping_attachment self, spine_array_int timelineSlots) {
	ClippingAttachment *_self = (ClippingAttachment *) self;
	_self->setTimelineSlots(*((Array<int> *) timelineSlots));
}

bool spine_clipping_attachment_is_timeline_active(spine_clipping_attachment self, spine_array_slot slots, int slotIndex, bool appliedPose) {
	ClippingAttachment *_self = (ClippingAttachment *) self;
	return _self->isTimelineActive(*((Array<Slot *> *) slots), slotIndex, appliedPose);
}

int spine_clipping_attachment_get_ref_count(spine_clipping_attachment self) {
	ClippingAttachment *_self = (ClippingAttachment *) self;
	return _self->getRefCount();
}

void spine_clipping_attachment_reference(spine_clipping_attachment self) {
	ClippingAttachment *_self = (ClippingAttachment *) self;
	_self->reference();
}

void spine_clipping_attachment_dereference(spine_clipping_attachment self) {
	ClippingAttachment *_self = (ClippingAttachment *) self;
	_self->dereference();
}

spine_rtti spine_clipping_attachment_rtti(void) {
	return (spine_rtti) &ClippingAttachment::rtti;
}
