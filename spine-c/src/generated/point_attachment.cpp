#include "point_attachment.h"
#include <spine/spine.h>

using namespace spine;

spine_point_attachment spine_point_attachment_create(const char *name) {
	return (spine_point_attachment) new (__FILE__, __LINE__) PointAttachment(String(name));
}

void spine_point_attachment_dispose(spine_point_attachment self) {
	delete (PointAttachment *) self;
}

spine_rtti spine_point_attachment_get_rtti(spine_point_attachment self) {
	PointAttachment *_self = (PointAttachment *) self;
	return (spine_rtti) &_self->getRTTI();
}

float spine_point_attachment_get_x(spine_point_attachment self) {
	PointAttachment *_self = (PointAttachment *) self;
	return _self->getX();
}

void spine_point_attachment_set_x(spine_point_attachment self, float inValue) {
	PointAttachment *_self = (PointAttachment *) self;
	_self->setX(inValue);
}

float spine_point_attachment_get_y(spine_point_attachment self) {
	PointAttachment *_self = (PointAttachment *) self;
	return _self->getY();
}

void spine_point_attachment_set_y(spine_point_attachment self, float inValue) {
	PointAttachment *_self = (PointAttachment *) self;
	_self->setY(inValue);
}

float spine_point_attachment_get_rotation(spine_point_attachment self) {
	PointAttachment *_self = (PointAttachment *) self;
	return _self->getRotation();
}

void spine_point_attachment_set_rotation(spine_point_attachment self, float inValue) {
	PointAttachment *_self = (PointAttachment *) self;
	_self->setRotation(inValue);
}

spine_color spine_point_attachment_get_color(spine_point_attachment self) {
	PointAttachment *_self = (PointAttachment *) self;
	return (spine_color) &_self->getColor();
}

void spine_point_attachment_compute_world_position(spine_point_attachment self, spine_bone_pose bone, float *ox, float *oy) {
	PointAttachment *_self = (PointAttachment *) self;
	_self->computeWorldPosition(*((BonePose *) bone), *ox, *oy);
}

float spine_point_attachment_compute_world_rotation(spine_point_attachment self, spine_bone_pose bone) {
	PointAttachment *_self = (PointAttachment *) self;
	return _self->computeWorldRotation(*((BonePose *) bone));
}

spine_attachment spine_point_attachment_copy(spine_point_attachment self) {
	PointAttachment *_self = (PointAttachment *) self;
	return (spine_attachment) &_self->copy();
}

const char *spine_point_attachment_get_name(spine_point_attachment self) {
	PointAttachment *_self = (PointAttachment *) self;
	return _self->getName().buffer();
}

/*@null*/ spine_attachment spine_point_attachment_get_timeline_attachment(spine_point_attachment self) {
	PointAttachment *_self = (PointAttachment *) self;
	return (spine_attachment) _self->getTimelineAttachment();
}

void spine_point_attachment_set_timeline_attachment(spine_point_attachment self, /*@null*/ spine_attachment attachment) {
	PointAttachment *_self = (PointAttachment *) self;
	_self->setTimelineAttachment((Attachment *) attachment);
}

int spine_point_attachment_get_ref_count(spine_point_attachment self) {
	PointAttachment *_self = (PointAttachment *) self;
	return _self->getRefCount();
}

void spine_point_attachment_reference(spine_point_attachment self) {
	PointAttachment *_self = (PointAttachment *) self;
	_self->reference();
}

void spine_point_attachment_dereference(spine_point_attachment self) {
	PointAttachment *_self = (PointAttachment *) self;
	_self->dereference();
}

spine_rtti spine_point_attachment_rtti(void) {
	return (spine_rtti) &PointAttachment::rtti;
}
