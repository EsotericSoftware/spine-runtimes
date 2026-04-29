#include "vertex_attachment.h"
#include <spine/spine.h>

using namespace spine;

void spine_vertex_attachment_dispose(spine_vertex_attachment self) {
	delete (VertexAttachment *) self;
}

spine_rtti spine_vertex_attachment_get_rtti(spine_vertex_attachment self) {
	VertexAttachment *_self = (VertexAttachment *) self;
	return (spine_rtti) &_self->getRTTI();
}

void spine_vertex_attachment_compute_world_vertices_1(spine_vertex_attachment self, spine_skeleton skeleton, spine_slot slot, size_t start,
													  size_t count, /*@null*/ float *worldVertices, size_t offset, size_t stride) {
	VertexAttachment *_self = (VertexAttachment *) self;
	_self->computeWorldVertices(*((Skeleton *) skeleton), *((Slot *) slot), start, count, worldVertices, offset, stride);
}

void spine_vertex_attachment_compute_world_vertices_2(spine_vertex_attachment self, spine_skeleton skeleton, spine_slot slot, size_t start,
													  size_t count, spine_array_float worldVertices, size_t offset, size_t stride) {
	VertexAttachment *_self = (VertexAttachment *) self;
	_self->computeWorldVertices(*((Skeleton *) skeleton), *((Slot *) slot), start, count, *((Array<float> *) worldVertices), offset, stride);
}

int spine_vertex_attachment_get_id(spine_vertex_attachment self) {
	VertexAttachment *_self = (VertexAttachment *) self;
	return _self->getId();
}

spine_array_int spine_vertex_attachment_get_bones(spine_vertex_attachment self) {
	VertexAttachment *_self = (VertexAttachment *) self;
	return (spine_array_int) &_self->getBones();
}

void spine_vertex_attachment_set_bones(spine_vertex_attachment self, spine_array_int bones) {
	VertexAttachment *_self = (VertexAttachment *) self;
	_self->setBones(*((Array<int> *) bones));
}

spine_array_float spine_vertex_attachment_get_vertices(spine_vertex_attachment self) {
	VertexAttachment *_self = (VertexAttachment *) self;
	return (spine_array_float) &_self->getVertices();
}

void spine_vertex_attachment_set_vertices(spine_vertex_attachment self, spine_array_float vertices) {
	VertexAttachment *_self = (VertexAttachment *) self;
	_self->setVertices(*((Array<float> *) vertices));
}

size_t spine_vertex_attachment_get_world_vertices_length(spine_vertex_attachment self) {
	VertexAttachment *_self = (VertexAttachment *) self;
	return _self->getWorldVerticesLength();
}

void spine_vertex_attachment_set_world_vertices_length(spine_vertex_attachment self, size_t inValue) {
	VertexAttachment *_self = (VertexAttachment *) self;
	_self->setWorldVerticesLength(inValue);
}

/*@null*/ spine_attachment spine_vertex_attachment_get_timeline_attachment(spine_vertex_attachment self) {
	VertexAttachment *_self = (VertexAttachment *) self;
	return (spine_attachment) _self->getTimelineAttachment();
}

void spine_vertex_attachment_set_timeline_attachment(spine_vertex_attachment self, /*@null*/ spine_attachment attachment) {
	VertexAttachment *_self = (VertexAttachment *) self;
	_self->setTimelineAttachment((Attachment *) attachment);
}

void spine_vertex_attachment_copy_to(spine_vertex_attachment self, spine_vertex_attachment other) {
	VertexAttachment *_self = (VertexAttachment *) self;
	_self->copyTo(*((VertexAttachment *) other));
}

const char *spine_vertex_attachment_get_name(spine_vertex_attachment self) {
	VertexAttachment *_self = (VertexAttachment *) self;
	return _self->getName().buffer();
}

spine_attachment spine_vertex_attachment_copy(spine_vertex_attachment self) {
	VertexAttachment *_self = (VertexAttachment *) self;
	return (spine_attachment) &_self->copy();
}

spine_array_int spine_vertex_attachment_get_timeline_slots(spine_vertex_attachment self) {
	VertexAttachment *_self = (VertexAttachment *) self;
	return (spine_array_int) &_self->getTimelineSlots();
}

void spine_vertex_attachment_set_timeline_slots(spine_vertex_attachment self, spine_array_int timelineSlots) {
	VertexAttachment *_self = (VertexAttachment *) self;
	_self->setTimelineSlots(*((Array<int> *) timelineSlots));
}

bool spine_vertex_attachment_is_timeline_active(spine_vertex_attachment self, spine_array_slot slots, int slotIndex, bool appliedPose) {
	VertexAttachment *_self = (VertexAttachment *) self;
	return _self->isTimelineActive(*((Array<Slot *> *) slots), slotIndex, appliedPose);
}

int spine_vertex_attachment_get_ref_count(spine_vertex_attachment self) {
	VertexAttachment *_self = (VertexAttachment *) self;
	return _self->getRefCount();
}

void spine_vertex_attachment_reference(spine_vertex_attachment self) {
	VertexAttachment *_self = (VertexAttachment *) self;
	_self->reference();
}

void spine_vertex_attachment_dereference(spine_vertex_attachment self) {
	VertexAttachment *_self = (VertexAttachment *) self;
	_self->dereference();
}

spine_rtti spine_vertex_attachment_rtti(void) {
	return (spine_rtti) &VertexAttachment::rtti;
}
