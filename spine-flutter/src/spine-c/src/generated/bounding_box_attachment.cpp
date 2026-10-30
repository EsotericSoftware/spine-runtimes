#include "bounding_box_attachment.h"
#include <spine/spine.h>

using namespace spine;

spine_bounding_box_attachment spine_bounding_box_attachment_create(const char *name) {
	return (spine_bounding_box_attachment) new (__FILE__, __LINE__) BoundingBoxAttachment(String(name));
}

void spine_bounding_box_attachment_dispose(spine_bounding_box_attachment self) {
	delete (BoundingBoxAttachment *) self;
}

spine_rtti spine_bounding_box_attachment_get_rtti(spine_bounding_box_attachment self) {
	BoundingBoxAttachment *_self = (BoundingBoxAttachment *) self;
	return (spine_rtti) &_self->getRTTI();
}

spine_color spine_bounding_box_attachment_get_color(spine_bounding_box_attachment self) {
	BoundingBoxAttachment *_self = (BoundingBoxAttachment *) self;
	return (spine_color) &_self->getColor();
}

spine_attachment spine_bounding_box_attachment_copy(spine_bounding_box_attachment self) {
	BoundingBoxAttachment *_self = (BoundingBoxAttachment *) self;
	return (spine_attachment) &_self->copy();
}

void spine_bounding_box_attachment_compute_world_vertices_1(spine_bounding_box_attachment self, spine_skeleton skeleton, spine_slot slot,
															size_t start, size_t count, /*@null*/ float *worldVertices, size_t offset,
															size_t stride) {
	BoundingBoxAttachment *_self = (BoundingBoxAttachment *) self;
	_self->computeWorldVertices(*((Skeleton *) skeleton), *((Slot *) slot), start, count, worldVertices, offset, stride);
}

void spine_bounding_box_attachment_compute_world_vertices_2(spine_bounding_box_attachment self, spine_skeleton skeleton, spine_slot slot,
															size_t start, size_t count, spine_array_float worldVertices, size_t offset,
															size_t stride) {
	BoundingBoxAttachment *_self = (BoundingBoxAttachment *) self;
	_self->computeWorldVertices(*((Skeleton *) skeleton), *((Slot *) slot), start, count, *((Array<float> *) worldVertices), offset, stride);
}

int spine_bounding_box_attachment_get_id(spine_bounding_box_attachment self) {
	BoundingBoxAttachment *_self = (BoundingBoxAttachment *) self;
	return _self->getId();
}

spine_array_int spine_bounding_box_attachment_get_bones(spine_bounding_box_attachment self) {
	BoundingBoxAttachment *_self = (BoundingBoxAttachment *) self;
	return (spine_array_int) &_self->getBones();
}

void spine_bounding_box_attachment_set_bones(spine_bounding_box_attachment self, spine_array_int bones) {
	BoundingBoxAttachment *_self = (BoundingBoxAttachment *) self;
	_self->setBones(*((Array<int> *) bones));
}

spine_array_float spine_bounding_box_attachment_get_vertices(spine_bounding_box_attachment self) {
	BoundingBoxAttachment *_self = (BoundingBoxAttachment *) self;
	return (spine_array_float) &_self->getVertices();
}

void spine_bounding_box_attachment_set_vertices(spine_bounding_box_attachment self, spine_array_float vertices) {
	BoundingBoxAttachment *_self = (BoundingBoxAttachment *) self;
	_self->setVertices(*((Array<float> *) vertices));
}

size_t spine_bounding_box_attachment_get_world_vertices_length(spine_bounding_box_attachment self) {
	BoundingBoxAttachment *_self = (BoundingBoxAttachment *) self;
	return _self->getWorldVerticesLength();
}

void spine_bounding_box_attachment_set_world_vertices_length(spine_bounding_box_attachment self, size_t inValue) {
	BoundingBoxAttachment *_self = (BoundingBoxAttachment *) self;
	_self->setWorldVerticesLength(inValue);
}

/*@null*/ spine_attachment spine_bounding_box_attachment_get_timeline_attachment(spine_bounding_box_attachment self) {
	BoundingBoxAttachment *_self = (BoundingBoxAttachment *) self;
	return (spine_attachment) _self->getTimelineAttachment();
}

void spine_bounding_box_attachment_set_timeline_attachment(spine_bounding_box_attachment self, /*@null*/ spine_attachment attachment) {
	BoundingBoxAttachment *_self = (BoundingBoxAttachment *) self;
	_self->setTimelineAttachment((Attachment *) attachment);
}

void spine_bounding_box_attachment_copy_to(spine_bounding_box_attachment self, spine_vertex_attachment other) {
	BoundingBoxAttachment *_self = (BoundingBoxAttachment *) self;
	_self->copyTo(*((VertexAttachment *) other));
}

const char *spine_bounding_box_attachment_get_name(spine_bounding_box_attachment self) {
	BoundingBoxAttachment *_self = (BoundingBoxAttachment *) self;
	return _self->getName().buffer();
}

spine_array_int spine_bounding_box_attachment_get_timeline_slots(spine_bounding_box_attachment self) {
	BoundingBoxAttachment *_self = (BoundingBoxAttachment *) self;
	return (spine_array_int) &_self->getTimelineSlots();
}

void spine_bounding_box_attachment_set_timeline_slots(spine_bounding_box_attachment self, spine_array_int timelineSlots) {
	BoundingBoxAttachment *_self = (BoundingBoxAttachment *) self;
	_self->setTimelineSlots(*((Array<int> *) timelineSlots));
}

bool spine_bounding_box_attachment_is_timeline_active(spine_bounding_box_attachment self, spine_array_slot slots, int slotIndex, bool appliedPose) {
	BoundingBoxAttachment *_self = (BoundingBoxAttachment *) self;
	return _self->isTimelineActive(*((Array<Slot *> *) slots), slotIndex, appliedPose);
}

int spine_bounding_box_attachment_get_ref_count(spine_bounding_box_attachment self) {
	BoundingBoxAttachment *_self = (BoundingBoxAttachment *) self;
	return _self->getRefCount();
}

void spine_bounding_box_attachment_reference(spine_bounding_box_attachment self) {
	BoundingBoxAttachment *_self = (BoundingBoxAttachment *) self;
	_self->reference();
}

void spine_bounding_box_attachment_dereference(spine_bounding_box_attachment self) {
	BoundingBoxAttachment *_self = (BoundingBoxAttachment *) self;
	_self->dereference();
}

spine_rtti spine_bounding_box_attachment_rtti(void) {
	return (spine_rtti) &BoundingBoxAttachment::rtti;
}
