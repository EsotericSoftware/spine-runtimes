#include "draw_order_timeline.h"
#include <spine/spine.h>

using namespace spine;

spine_draw_order_timeline spine_draw_order_timeline_create(size_t frameCount) {
	return (spine_draw_order_timeline) new (__FILE__, __LINE__) DrawOrderTimeline(frameCount);
}

void spine_draw_order_timeline_dispose(spine_draw_order_timeline self) {
	delete (DrawOrderTimeline *) self;
}

spine_rtti spine_draw_order_timeline_get_rtti(spine_draw_order_timeline self) {
	DrawOrderTimeline *_self = (DrawOrderTimeline *) self;
	return (spine_rtti) &_self->getRTTI();
}

void spine_draw_order_timeline_apply(spine_draw_order_timeline self, spine_skeleton skeleton, float lastTime, float time,
									 /*@null*/ spine_array_event events, float alpha, spine_mix_from from, bool add, bool out, bool appliedPose) {
	DrawOrderTimeline *_self = (DrawOrderTimeline *) self;
	_self->apply(*((Skeleton *) skeleton), lastTime, time, (Array<Event *> *) events, alpha, (MixFrom) from, add, out, appliedPose);
}

size_t spine_draw_order_timeline_get_frame_count(spine_draw_order_timeline self) {
	DrawOrderTimeline *_self = (DrawOrderTimeline *) self;
	return _self->getFrameCount();
}

void spine_draw_order_timeline_set_frame(spine_draw_order_timeline self, size_t frame, float time, /*@null*/ spine_array_int drawOrder) {
	DrawOrderTimeline *_self = (DrawOrderTimeline *) self;
	_self->setFrame(frame, time, (Array<int> *) drawOrder);
}

bool spine_draw_order_timeline_get_additive(spine_draw_order_timeline self) {
	DrawOrderTimeline *_self = (DrawOrderTimeline *) self;
	return _self->getAdditive();
}

bool spine_draw_order_timeline_get_instant(spine_draw_order_timeline self) {
	DrawOrderTimeline *_self = (DrawOrderTimeline *) self;
	return _self->getInstant();
}

size_t spine_draw_order_timeline_get_frame_entries(spine_draw_order_timeline self) {
	DrawOrderTimeline *_self = (DrawOrderTimeline *) self;
	return _self->getFrameEntries();
}

spine_array_float spine_draw_order_timeline_get_frames(spine_draw_order_timeline self) {
	DrawOrderTimeline *_self = (DrawOrderTimeline *) self;
	return (spine_array_float) &_self->getFrames();
}

float spine_draw_order_timeline_get_duration(spine_draw_order_timeline self) {
	DrawOrderTimeline *_self = (DrawOrderTimeline *) self;
	return _self->getDuration();
}

spine_array_property_id spine_draw_order_timeline_get_property_ids(spine_draw_order_timeline self) {
	DrawOrderTimeline *_self = (DrawOrderTimeline *) self;
	return (spine_array_property_id) &_self->getPropertyIds();
}

spine_rtti spine_draw_order_timeline_rtti(void) {
	return (spine_rtti) &DrawOrderTimeline::rtti;
}
