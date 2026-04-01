#include "rgb2_timeline.h"
#include <spine/spine.h>

using namespace spine;

spine_rgb2_timeline spine_rgb2_timeline_create(size_t frameCount, size_t bezierCount, int slotIndex) {
	return (spine_rgb2_timeline) new (__FILE__, __LINE__) RGB2Timeline(frameCount, bezierCount, slotIndex);
}

void spine_rgb2_timeline_dispose(spine_rgb2_timeline self) {
	delete (RGB2Timeline *) self;
}

spine_rtti spine_rgb2_timeline_get_rtti(spine_rgb2_timeline self) {
	RGB2Timeline *_self = (RGB2Timeline *) self;
	return (spine_rtti) &_self->getRTTI();
}

void spine_rgb2_timeline_set_frame(spine_rgb2_timeline self, int frame, float time, float r, float g, float b, float r2, float g2, float b2) {
	RGB2Timeline *_self = (RGB2Timeline *) self;
	_self->setFrame(frame, time, r, g, b, r2, g2, b2);
}

void spine_rgb2_timeline_apply(spine_rgb2_timeline self, spine_skeleton skeleton, float lastTime, float time, /*@null*/ spine_array_event events,
							   float alpha, bool fromSetup, bool add, bool out, bool appliedPose) {
	RGB2Timeline *_self = (RGB2Timeline *) self;
	_self->apply(*((Skeleton *) skeleton), lastTime, time, (Array<Event *> *) events, alpha, fromSetup, add, out, appliedPose);
}

int spine_rgb2_timeline_get_slot_index(spine_rgb2_timeline self) {
	RGB2Timeline *_self = (RGB2Timeline *) self;
	return _self->getSlotIndex();
}

void spine_rgb2_timeline_set_slot_index(spine_rgb2_timeline self, int inValue) {
	RGB2Timeline *_self = (RGB2Timeline *) self;
	_self->setSlotIndex(inValue);
}

void spine_rgb2_timeline_set_linear(spine_rgb2_timeline self, size_t frame) {
	RGB2Timeline *_self = (RGB2Timeline *) self;
	_self->setLinear(frame);
}

void spine_rgb2_timeline_set_stepped(spine_rgb2_timeline self, size_t frame) {
	RGB2Timeline *_self = (RGB2Timeline *) self;
	_self->setStepped(frame);
}

void spine_rgb2_timeline_set_bezier(spine_rgb2_timeline self, size_t bezier, size_t frame, float value, float time1, float value1, float cx1,
									float cy1, float cx2, float cy2, float time2, float value2) {
	RGB2Timeline *_self = (RGB2Timeline *) self;
	_self->setBezier(bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
}

float spine_rgb2_timeline_get_bezier_value(spine_rgb2_timeline self, float time, size_t frame, size_t valueOffset, size_t i) {
	RGB2Timeline *_self = (RGB2Timeline *) self;
	return _self->getBezierValue(time, frame, valueOffset, i);
}

spine_array_float spine_rgb2_timeline_get_curves(spine_rgb2_timeline self) {
	RGB2Timeline *_self = (RGB2Timeline *) self;
	return (spine_array_float) &_self->getCurves();
}

bool spine_rgb2_timeline_get_additive(spine_rgb2_timeline self) {
	RGB2Timeline *_self = (RGB2Timeline *) self;
	return _self->getAdditive();
}

bool spine_rgb2_timeline_get_instant(spine_rgb2_timeline self) {
	RGB2Timeline *_self = (RGB2Timeline *) self;
	return _self->getInstant();
}

size_t spine_rgb2_timeline_get_frame_entries(spine_rgb2_timeline self) {
	RGB2Timeline *_self = (RGB2Timeline *) self;
	return _self->getFrameEntries();
}

size_t spine_rgb2_timeline_get_frame_count(spine_rgb2_timeline self) {
	RGB2Timeline *_self = (RGB2Timeline *) self;
	return _self->getFrameCount();
}

spine_array_float spine_rgb2_timeline_get_frames(spine_rgb2_timeline self) {
	RGB2Timeline *_self = (RGB2Timeline *) self;
	return (spine_array_float) &_self->getFrames();
}

float spine_rgb2_timeline_get_duration(spine_rgb2_timeline self) {
	RGB2Timeline *_self = (RGB2Timeline *) self;
	return _self->getDuration();
}

spine_array_property_id spine_rgb2_timeline_get_property_ids(spine_rgb2_timeline self) {
	RGB2Timeline *_self = (RGB2Timeline *) self;
	return (spine_array_property_id) &_self->getPropertyIds();
}

spine_rtti spine_rgb2_timeline_rtti(void) {
	return (spine_rtti) &RGB2Timeline::rtti;
}
