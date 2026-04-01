#include "scale_timeline.h"
#include <spine/spine.h>

using namespace spine;

spine_scale_timeline spine_scale_timeline_create(size_t frameCount, size_t bezierCount, int boneIndex) {
	return (spine_scale_timeline) new (__FILE__, __LINE__) ScaleTimeline(frameCount, bezierCount, boneIndex);
}

void spine_scale_timeline_dispose(spine_scale_timeline self) {
	delete (ScaleTimeline *) self;
}

spine_rtti spine_scale_timeline_get_rtti(spine_scale_timeline self) {
	ScaleTimeline *_self = (ScaleTimeline *) self;
	return (spine_rtti) &_self->getRTTI();
}

void spine_scale_timeline_apply(spine_scale_timeline self, spine_skeleton skeleton, float lastTime, float time, /*@null*/ spine_array_event events,
								float alpha, bool fromSetup, bool add, bool out, bool appliedPose) {
	ScaleTimeline *_self = (ScaleTimeline *) self;
	_self->apply(*((Skeleton *) skeleton), lastTime, time, (Array<Event *> *) events, alpha, fromSetup, add, out, appliedPose);
}

int spine_scale_timeline_get_bone_index(spine_scale_timeline self) {
	ScaleTimeline *_self = (ScaleTimeline *) self;
	return _self->getBoneIndex();
}

void spine_scale_timeline_set_bone_index(spine_scale_timeline self, int inValue) {
	ScaleTimeline *_self = (ScaleTimeline *) self;
	_self->setBoneIndex(inValue);
}

void spine_scale_timeline_set_frame(spine_scale_timeline self, size_t frame, float time, float value1, float value2) {
	ScaleTimeline *_self = (ScaleTimeline *) self;
	_self->setFrame(frame, time, value1, value2);
}

void spine_scale_timeline_set_linear(spine_scale_timeline self, size_t frame) {
	ScaleTimeline *_self = (ScaleTimeline *) self;
	_self->setLinear(frame);
}

void spine_scale_timeline_set_stepped(spine_scale_timeline self, size_t frame) {
	ScaleTimeline *_self = (ScaleTimeline *) self;
	_self->setStepped(frame);
}

void spine_scale_timeline_set_bezier(spine_scale_timeline self, size_t bezier, size_t frame, float value, float time1, float value1, float cx1,
									 float cy1, float cx2, float cy2, float time2, float value2) {
	ScaleTimeline *_self = (ScaleTimeline *) self;
	_self->setBezier(bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
}

float spine_scale_timeline_get_bezier_value(spine_scale_timeline self, float time, size_t frame, size_t valueOffset, size_t i) {
	ScaleTimeline *_self = (ScaleTimeline *) self;
	return _self->getBezierValue(time, frame, valueOffset, i);
}

spine_array_float spine_scale_timeline_get_curves(spine_scale_timeline self) {
	ScaleTimeline *_self = (ScaleTimeline *) self;
	return (spine_array_float) &_self->getCurves();
}

bool spine_scale_timeline_get_additive(spine_scale_timeline self) {
	ScaleTimeline *_self = (ScaleTimeline *) self;
	return _self->getAdditive();
}

bool spine_scale_timeline_get_instant(spine_scale_timeline self) {
	ScaleTimeline *_self = (ScaleTimeline *) self;
	return _self->getInstant();
}

size_t spine_scale_timeline_get_frame_entries(spine_scale_timeline self) {
	ScaleTimeline *_self = (ScaleTimeline *) self;
	return _self->getFrameEntries();
}

size_t spine_scale_timeline_get_frame_count(spine_scale_timeline self) {
	ScaleTimeline *_self = (ScaleTimeline *) self;
	return _self->getFrameCount();
}

spine_array_float spine_scale_timeline_get_frames(spine_scale_timeline self) {
	ScaleTimeline *_self = (ScaleTimeline *) self;
	return (spine_array_float) &_self->getFrames();
}

float spine_scale_timeline_get_duration(spine_scale_timeline self) {
	ScaleTimeline *_self = (ScaleTimeline *) self;
	return _self->getDuration();
}

spine_array_property_id spine_scale_timeline_get_property_ids(spine_scale_timeline self) {
	ScaleTimeline *_self = (ScaleTimeline *) self;
	return (spine_array_property_id) &_self->getPropertyIds();
}

spine_rtti spine_scale_timeline_rtti(void) {
	return (spine_rtti) &ScaleTimeline::rtti;
}
