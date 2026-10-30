#include "bone_timeline2.h"
#include <spine/spine.h>

using namespace spine;

void spine_bone_timeline2_dispose(spine_bone_timeline2 self) {
	delete (BoneTimeline2 *) self;
}

spine_rtti spine_bone_timeline2_get_rtti(spine_bone_timeline2 self) {
	BoneTimeline2 *_self = (BoneTimeline2 *) self;
	return (spine_rtti) &_self->getRTTI();
}

void spine_bone_timeline2_apply(spine_bone_timeline2 self, spine_skeleton skeleton, float lastTime, float time, /*@null*/ spine_array_event events,
								float alpha, spine_mix_from from, bool add, bool out, bool appliedPose) {
	BoneTimeline2 *_self = (BoneTimeline2 *) self;
	_self->apply(*((Skeleton *) skeleton), lastTime, time, (Array<Event *> *) events, alpha, (MixFrom) from, add, out, appliedPose);
}

int spine_bone_timeline2_get_bone_index(spine_bone_timeline2 self) {
	BoneTimeline2 *_self = (BoneTimeline2 *) self;
	return _self->getBoneIndex();
}

void spine_bone_timeline2_set_bone_index(spine_bone_timeline2 self, int inValue) {
	BoneTimeline2 *_self = (BoneTimeline2 *) self;
	_self->setBoneIndex(inValue);
}

void spine_bone_timeline2_set_frame(spine_bone_timeline2 self, size_t frame, float time, float value1, float value2) {
	BoneTimeline2 *_self = (BoneTimeline2 *) self;
	_self->setFrame(frame, time, value1, value2);
}

void spine_bone_timeline2_set_linear(spine_bone_timeline2 self, size_t frame) {
	BoneTimeline2 *_self = (BoneTimeline2 *) self;
	_self->setLinear(frame);
}

void spine_bone_timeline2_set_stepped(spine_bone_timeline2 self, size_t frame) {
	BoneTimeline2 *_self = (BoneTimeline2 *) self;
	_self->setStepped(frame);
}

void spine_bone_timeline2_set_bezier(spine_bone_timeline2 self, size_t bezier, size_t frame, float value, float time1, float value1, float cx1,
									 float cy1, float cx2, float cy2, float time2, float value2) {
	BoneTimeline2 *_self = (BoneTimeline2 *) self;
	_self->setBezier(bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
}

float spine_bone_timeline2_get_bezier_value(spine_bone_timeline2 self, float time, size_t frame, size_t valueOffset, size_t i) {
	BoneTimeline2 *_self = (BoneTimeline2 *) self;
	return _self->getBezierValue(time, frame, valueOffset, i);
}

spine_array_float spine_bone_timeline2_get_curves(spine_bone_timeline2 self) {
	BoneTimeline2 *_self = (BoneTimeline2 *) self;
	return (spine_array_float) &_self->getCurves();
}

bool spine_bone_timeline2_get_additive(spine_bone_timeline2 self) {
	BoneTimeline2 *_self = (BoneTimeline2 *) self;
	return _self->getAdditive();
}

bool spine_bone_timeline2_get_instant(spine_bone_timeline2 self) {
	BoneTimeline2 *_self = (BoneTimeline2 *) self;
	return _self->getInstant();
}

size_t spine_bone_timeline2_get_frame_entries(spine_bone_timeline2 self) {
	BoneTimeline2 *_self = (BoneTimeline2 *) self;
	return _self->getFrameEntries();
}

size_t spine_bone_timeline2_get_frame_count(spine_bone_timeline2 self) {
	BoneTimeline2 *_self = (BoneTimeline2 *) self;
	return _self->getFrameCount();
}

spine_array_float spine_bone_timeline2_get_frames(spine_bone_timeline2 self) {
	BoneTimeline2 *_self = (BoneTimeline2 *) self;
	return (spine_array_float) &_self->getFrames();
}

float spine_bone_timeline2_get_duration(spine_bone_timeline2 self) {
	BoneTimeline2 *_self = (BoneTimeline2 *) self;
	return _self->getDuration();
}

spine_array_property_id spine_bone_timeline2_get_property_ids(spine_bone_timeline2 self) {
	BoneTimeline2 *_self = (BoneTimeline2 *) self;
	return (spine_array_property_id) &_self->getPropertyIds();
}

spine_rtti spine_bone_timeline2_rtti(void) {
	return (spine_rtti) &BoneTimeline2::rtti;
}
