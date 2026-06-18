#include "timeline.h"
#include <spine/spine.h>

using namespace spine;

void spine_timeline_dispose(spine_timeline self) {
	delete (Timeline *) self;
}

spine_rtti spine_timeline_get_rtti(spine_timeline self) {
	Timeline *_self = (Timeline *) self;
	return (spine_rtti) &_self->getRTTI();
}

void spine_timeline_apply(spine_timeline self, spine_skeleton skeleton, float lastTime, float time, /*@null*/ spine_array_event events, float alpha,
						  spine_mix_from from, bool add, bool out, bool appliedPose) {
	Timeline *_self = (Timeline *) self;
	_self->apply(*((Skeleton *) skeleton), lastTime, time, (Array<Event *> *) events, alpha, (MixFrom) from, add, out, appliedPose);
}

bool spine_timeline_get_additive(spine_timeline self) {
	Timeline *_self = (Timeline *) self;
	return _self->getAdditive();
}

bool spine_timeline_get_instant(spine_timeline self) {
	Timeline *_self = (Timeline *) self;
	return _self->getInstant();
}

size_t spine_timeline_get_frame_entries(spine_timeline self) {
	Timeline *_self = (Timeline *) self;
	return _self->getFrameEntries();
}

size_t spine_timeline_get_frame_count(spine_timeline self) {
	Timeline *_self = (Timeline *) self;
	return _self->getFrameCount();
}

spine_array_float spine_timeline_get_frames(spine_timeline self) {
	Timeline *_self = (Timeline *) self;
	return (spine_array_float) &_self->getFrames();
}

float spine_timeline_get_duration(spine_timeline self) {
	Timeline *_self = (Timeline *) self;
	return _self->getDuration();
}

spine_array_property_id spine_timeline_get_property_ids(spine_timeline self) {
	Timeline *_self = (Timeline *) self;
	return (spine_array_property_id) &_self->getPropertyIds();
}

spine_rtti spine_timeline_rtti(void) {
	return (spine_rtti) &Timeline::rtti;
}
