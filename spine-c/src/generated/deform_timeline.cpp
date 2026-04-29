#include "deform_timeline.h"
#include <spine/spine.h>

using namespace spine;

spine_deform_timeline spine_deform_timeline_create(size_t frameCount, size_t bezierCount, int slotIndex, spine_vertex_attachment attachment) {
	return (spine_deform_timeline) new (__FILE__, __LINE__) DeformTimeline(frameCount, bezierCount, slotIndex, *((VertexAttachment *) attachment));
}

void spine_deform_timeline_dispose(spine_deform_timeline self) {
	delete (DeformTimeline *) self;
}

spine_rtti spine_deform_timeline_get_rtti(spine_deform_timeline self) {
	DeformTimeline *_self = (DeformTimeline *) self;
	return (spine_rtti) &_self->getRTTI();
}

void spine_deform_timeline_apply(spine_deform_timeline self, spine_skeleton skeleton, float lastTime, float time, /*@null*/ spine_array_event events,
								 float alpha, bool fromSetup, bool add, bool out, bool appliedPose) {
	DeformTimeline *_self = (DeformTimeline *) self;
	_self->apply(*((Skeleton *) skeleton), lastTime, time, (Array<Event *> *) events, alpha, fromSetup, add, out, appliedPose);
}

void spine_deform_timeline_set_frame(spine_deform_timeline self, int frameIndex, float time, spine_array_float vertices) {
	DeformTimeline *_self = (DeformTimeline *) self;
	_self->setFrame(frameIndex, time, *((Array<float> *) vertices));
}

spine_vertex_attachment spine_deform_timeline_get_attachment(spine_deform_timeline self) {
	DeformTimeline *_self = (DeformTimeline *) self;
	return (spine_vertex_attachment) &_self->getAttachment();
}

void spine_deform_timeline_set_attachment(spine_deform_timeline self, spine_vertex_attachment inValue) {
	DeformTimeline *_self = (DeformTimeline *) self;
	_self->setAttachment(*((VertexAttachment *) inValue));
}

void spine_deform_timeline_set_bezier(spine_deform_timeline self, size_t bezier, size_t frame, float value, float time1, float value1, float cx1,
									  float cy1, float cx2, float cy2, float time2, float value2) {
	DeformTimeline *_self = (DeformTimeline *) self;
	_self->setBezier(bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
}

float spine_deform_timeline_get_curve_percent(spine_deform_timeline self, float time, int frame) {
	DeformTimeline *_self = (DeformTimeline *) self;
	return _self->getCurvePercent(time, frame);
}

size_t spine_deform_timeline_get_frame_count(spine_deform_timeline self) {
	DeformTimeline *_self = (DeformTimeline *) self;
	return _self->getFrameCount();
}

int spine_deform_timeline_get_slot_index(spine_deform_timeline self) {
	DeformTimeline *_self = (DeformTimeline *) self;
	return _self->getSlotIndex();
}

void spine_deform_timeline_set_slot_index(spine_deform_timeline self, int inValue) {
	DeformTimeline *_self = (DeformTimeline *) self;
	_self->setSlotIndex(inValue);
}

void spine_deform_timeline_set_linear(spine_deform_timeline self, size_t frame) {
	DeformTimeline *_self = (DeformTimeline *) self;
	_self->setLinear(frame);
}

void spine_deform_timeline_set_stepped(spine_deform_timeline self, size_t frame) {
	DeformTimeline *_self = (DeformTimeline *) self;
	_self->setStepped(frame);
}

float spine_deform_timeline_get_bezier_value(spine_deform_timeline self, float time, size_t frame, size_t valueOffset, size_t i) {
	DeformTimeline *_self = (DeformTimeline *) self;
	return _self->getBezierValue(time, frame, valueOffset, i);
}

spine_array_float spine_deform_timeline_get_curves(spine_deform_timeline self) {
	DeformTimeline *_self = (DeformTimeline *) self;
	return (spine_array_float) &_self->getCurves();
}

bool spine_deform_timeline_get_additive(spine_deform_timeline self) {
	DeformTimeline *_self = (DeformTimeline *) self;
	return _self->getAdditive();
}

bool spine_deform_timeline_get_instant(spine_deform_timeline self) {
	DeformTimeline *_self = (DeformTimeline *) self;
	return _self->getInstant();
}

size_t spine_deform_timeline_get_frame_entries(spine_deform_timeline self) {
	DeformTimeline *_self = (DeformTimeline *) self;
	return _self->getFrameEntries();
}

spine_array_float spine_deform_timeline_get_frames(spine_deform_timeline self) {
	DeformTimeline *_self = (DeformTimeline *) self;
	return (spine_array_float) &_self->getFrames();
}

float spine_deform_timeline_get_duration(spine_deform_timeline self) {
	DeformTimeline *_self = (DeformTimeline *) self;
	return _self->getDuration();
}

spine_array_property_id spine_deform_timeline_get_property_ids(spine_deform_timeline self) {
	DeformTimeline *_self = (DeformTimeline *) self;
	return (spine_array_property_id) &_self->getPropertyIds();
}

spine_rtti spine_deform_timeline_rtti(void) {
	return (spine_rtti) &DeformTimeline::rtti;
}
