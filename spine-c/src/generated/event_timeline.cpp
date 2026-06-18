#include "event_timeline.h"
#include <spine/spine.h>

using namespace spine;

spine_event_timeline spine_event_timeline_create(size_t frameCount) {
	return (spine_event_timeline) new (__FILE__, __LINE__) EventTimeline(frameCount);
}

void spine_event_timeline_dispose(spine_event_timeline self) {
	delete (EventTimeline *) self;
}

spine_rtti spine_event_timeline_get_rtti(spine_event_timeline self) {
	EventTimeline *_self = (EventTimeline *) self;
	return (spine_rtti) &_self->getRTTI();
}

void spine_event_timeline_apply(spine_event_timeline self, spine_skeleton skeleton, float lastTime, float time, /*@null*/ spine_array_event events,
								float alpha, spine_mix_from from, bool add, bool out, bool appliedPose) {
	EventTimeline *_self = (EventTimeline *) self;
	_self->apply(*((Skeleton *) skeleton), lastTime, time, (Array<Event *> *) events, alpha, (MixFrom) from, add, out, appliedPose);
}

size_t spine_event_timeline_get_frame_count(spine_event_timeline self) {
	EventTimeline *_self = (EventTimeline *) self;
	return _self->getFrameCount();
}

spine_array_event spine_event_timeline_get_events(spine_event_timeline self) {
	EventTimeline *_self = (EventTimeline *) self;
	return (spine_array_event) &_self->getEvents();
}

void spine_event_timeline_set_frame(spine_event_timeline self, size_t frame, spine_event event) {
	EventTimeline *_self = (EventTimeline *) self;
	_self->setFrame(frame, *((Event *) event));
}

bool spine_event_timeline_get_additive(spine_event_timeline self) {
	EventTimeline *_self = (EventTimeline *) self;
	return _self->getAdditive();
}

bool spine_event_timeline_get_instant(spine_event_timeline self) {
	EventTimeline *_self = (EventTimeline *) self;
	return _self->getInstant();
}

size_t spine_event_timeline_get_frame_entries(spine_event_timeline self) {
	EventTimeline *_self = (EventTimeline *) self;
	return _self->getFrameEntries();
}

spine_array_float spine_event_timeline_get_frames(spine_event_timeline self) {
	EventTimeline *_self = (EventTimeline *) self;
	return (spine_array_float) &_self->getFrames();
}

float spine_event_timeline_get_duration(spine_event_timeline self) {
	EventTimeline *_self = (EventTimeline *) self;
	return _self->getDuration();
}

spine_array_property_id spine_event_timeline_get_property_ids(spine_event_timeline self) {
	EventTimeline *_self = (EventTimeline *) self;
	return (spine_array_property_id) &_self->getPropertyIds();
}

spine_rtti spine_event_timeline_rtti(void) {
	return (spine_rtti) &EventTimeline::rtti;
}
