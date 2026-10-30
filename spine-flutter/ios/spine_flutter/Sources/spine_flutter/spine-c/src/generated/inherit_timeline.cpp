#include "inherit_timeline.h"
#include <spine/spine.h>

using namespace spine;

spine_inherit_timeline spine_inherit_timeline_create(size_t frameCount, int boneIndex) {
	return (spine_inherit_timeline) new (__FILE__, __LINE__) InheritTimeline(frameCount, boneIndex);
}

void spine_inherit_timeline_dispose(spine_inherit_timeline self) {
	delete (InheritTimeline *) self;
}

spine_rtti spine_inherit_timeline_get_rtti(spine_inherit_timeline self) {
	InheritTimeline *_self = (InheritTimeline *) self;
	return (spine_rtti) &_self->getRTTI();
}

void spine_inherit_timeline_set_frame(spine_inherit_timeline self, int frame, float time, spine_inherit inherit) {
	InheritTimeline *_self = (InheritTimeline *) self;
	_self->setFrame(frame, time, (Inherit) inherit);
}

void spine_inherit_timeline_apply(spine_inherit_timeline self, spine_skeleton skeleton, float lastTime, float time,
								  /*@null*/ spine_array_event events, float alpha, spine_mix_from from, bool add, bool out, bool appliedPose) {
	InheritTimeline *_self = (InheritTimeline *) self;
	_self->apply(*((Skeleton *) skeleton), lastTime, time, (Array<Event *> *) events, alpha, (MixFrom) from, add, out, appliedPose);
}

int spine_inherit_timeline_get_bone_index(spine_inherit_timeline self) {
	InheritTimeline *_self = (InheritTimeline *) self;
	return _self->getBoneIndex();
}

void spine_inherit_timeline_set_bone_index(spine_inherit_timeline self, int inValue) {
	InheritTimeline *_self = (InheritTimeline *) self;
	_self->setBoneIndex(inValue);
}

bool spine_inherit_timeline_get_additive(spine_inherit_timeline self) {
	InheritTimeline *_self = (InheritTimeline *) self;
	return _self->getAdditive();
}

bool spine_inherit_timeline_get_instant(spine_inherit_timeline self) {
	InheritTimeline *_self = (InheritTimeline *) self;
	return _self->getInstant();
}

size_t spine_inherit_timeline_get_frame_entries(spine_inherit_timeline self) {
	InheritTimeline *_self = (InheritTimeline *) self;
	return _self->getFrameEntries();
}

size_t spine_inherit_timeline_get_frame_count(spine_inherit_timeline self) {
	InheritTimeline *_self = (InheritTimeline *) self;
	return _self->getFrameCount();
}

spine_array_float spine_inherit_timeline_get_frames(spine_inherit_timeline self) {
	InheritTimeline *_self = (InheritTimeline *) self;
	return (spine_array_float) &_self->getFrames();
}

float spine_inherit_timeline_get_duration(spine_inherit_timeline self) {
	InheritTimeline *_self = (InheritTimeline *) self;
	return _self->getDuration();
}

spine_array_property_id spine_inherit_timeline_get_property_ids(spine_inherit_timeline self) {
	InheritTimeline *_self = (InheritTimeline *) self;
	return (spine_array_property_id) &_self->getPropertyIds();
}

spine_rtti spine_inherit_timeline_rtti(void) {
	return (spine_rtti) &InheritTimeline::rtti;
}
