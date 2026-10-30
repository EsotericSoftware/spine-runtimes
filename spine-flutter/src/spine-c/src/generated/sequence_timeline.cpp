#include "sequence_timeline.h"
#include <spine/spine.h>

using namespace spine;

spine_sequence_timeline spine_sequence_timeline_create(size_t frameCount, int slotIndex, spine_attachment attachment) {
	return (spine_sequence_timeline) new (__FILE__, __LINE__) SequenceTimeline(frameCount, slotIndex, *((Attachment *) attachment));
}

void spine_sequence_timeline_dispose(spine_sequence_timeline self) {
	delete (SequenceTimeline *) self;
}

spine_rtti spine_sequence_timeline_get_rtti(spine_sequence_timeline self) {
	SequenceTimeline *_self = (SequenceTimeline *) self;
	return (spine_rtti) &_self->getRTTI();
}

void spine_sequence_timeline_apply(spine_sequence_timeline self, spine_skeleton skeleton, float lastTime, float time,
								   /*@null*/ spine_array_event events, float alpha, spine_mix_from from, bool add, bool out, bool appliedPose) {
	SequenceTimeline *_self = (SequenceTimeline *) self;
	_self->apply(*((Skeleton *) skeleton), lastTime, time, (Array<Event *> *) events, alpha, (MixFrom) from, add, out, appliedPose);
}

void spine_sequence_timeline_set_frame(spine_sequence_timeline self, int frame, float time, spine_sequence_mode mode, int index, float delay) {
	SequenceTimeline *_self = (SequenceTimeline *) self;
	_self->setFrame(frame, time, (SequenceMode) mode, index, delay);
}

spine_attachment spine_sequence_timeline_get_attachment(spine_sequence_timeline self) {
	SequenceTimeline *_self = (SequenceTimeline *) self;
	return (spine_attachment) &_self->getAttachment();
}

int spine_sequence_timeline_get_slot_index(spine_sequence_timeline self) {
	SequenceTimeline *_self = (SequenceTimeline *) self;
	return _self->getSlotIndex();
}

void spine_sequence_timeline_set_slot_index(spine_sequence_timeline self, int inValue) {
	SequenceTimeline *_self = (SequenceTimeline *) self;
	_self->setSlotIndex(inValue);
}

bool spine_sequence_timeline_get_additive(spine_sequence_timeline self) {
	SequenceTimeline *_self = (SequenceTimeline *) self;
	return _self->getAdditive();
}

bool spine_sequence_timeline_get_instant(spine_sequence_timeline self) {
	SequenceTimeline *_self = (SequenceTimeline *) self;
	return _self->getInstant();
}

size_t spine_sequence_timeline_get_frame_entries(spine_sequence_timeline self) {
	SequenceTimeline *_self = (SequenceTimeline *) self;
	return _self->getFrameEntries();
}

size_t spine_sequence_timeline_get_frame_count(spine_sequence_timeline self) {
	SequenceTimeline *_self = (SequenceTimeline *) self;
	return _self->getFrameCount();
}

spine_array_float spine_sequence_timeline_get_frames(spine_sequence_timeline self) {
	SequenceTimeline *_self = (SequenceTimeline *) self;
	return (spine_array_float) &_self->getFrames();
}

float spine_sequence_timeline_get_duration(spine_sequence_timeline self) {
	SequenceTimeline *_self = (SequenceTimeline *) self;
	return _self->getDuration();
}

spine_array_property_id spine_sequence_timeline_get_property_ids(spine_sequence_timeline self) {
	SequenceTimeline *_self = (SequenceTimeline *) self;
	return (spine_array_property_id) &_self->getPropertyIds();
}

spine_rtti spine_sequence_timeline_rtti(void) {
	return (spine_rtti) &SequenceTimeline::rtti;
}
