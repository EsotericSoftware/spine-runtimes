#include "shear_timeline.h"
#include <spine/spine.h>

using namespace spine;

spine_shear_timeline spine_shear_timeline_create(size_t frameCount, size_t bezierCount, int boneIndex) {
	return (spine_shear_timeline) new (__FILE__, __LINE__) ShearTimeline(frameCount, bezierCount, boneIndex);
}

void spine_shear_timeline_dispose(spine_shear_timeline self) {
	delete (ShearTimeline *) self;
}

spine_rtti spine_shear_timeline_get_rtti(spine_shear_timeline self) {
	ShearTimeline *_self = (ShearTimeline *) self;
	return (spine_rtti) &_self->getRTTI();
}

void spine_shear_timeline_apply(spine_shear_timeline self, spine_skeleton skeleton, float lastTime, float time, /*@null*/ spine_array_event events,
								float alpha, spine_mix_from from, bool add, bool out, bool appliedPose) {
	ShearTimeline *_self = (ShearTimeline *) self;
	_self->apply(*((Skeleton *) skeleton), lastTime, time, (Array<Event *> *) events, alpha, (MixFrom) from, add, out, appliedPose);
}

int spine_shear_timeline_get_bone_index(spine_shear_timeline self) {
	ShearTimeline *_self = (ShearTimeline *) self;
	return _self->getBoneIndex();
}

void spine_shear_timeline_set_bone_index(spine_shear_timeline self, int inValue) {
	ShearTimeline *_self = (ShearTimeline *) self;
	_self->setBoneIndex(inValue);
}

void spine_shear_timeline_set_frame(spine_shear_timeline self, size_t frame, float time, float value1, float value2) {
	ShearTimeline *_self = (ShearTimeline *) self;
	_self->setFrame(frame, time, value1, value2);
}

void spine_shear_timeline_set_linear(spine_shear_timeline self, size_t frame) {
	ShearTimeline *_self = (ShearTimeline *) self;
	_self->setLinear(frame);
}

void spine_shear_timeline_set_stepped(spine_shear_timeline self, size_t frame) {
	ShearTimeline *_self = (ShearTimeline *) self;
	_self->setStepped(frame);
}

void spine_shear_timeline_set_bezier(spine_shear_timeline self, size_t bezier, size_t frame, float value, float time1, float value1, float cx1,
									 float cy1, float cx2, float cy2, float time2, float value2) {
	ShearTimeline *_self = (ShearTimeline *) self;
	_self->setBezier(bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
}

float spine_shear_timeline_get_bezier_value(spine_shear_timeline self, float time, size_t frame, size_t valueOffset, size_t i) {
	ShearTimeline *_self = (ShearTimeline *) self;
	return _self->getBezierValue(time, frame, valueOffset, i);
}

spine_array_float spine_shear_timeline_get_curves(spine_shear_timeline self) {
	ShearTimeline *_self = (ShearTimeline *) self;
	return (spine_array_float) &_self->getCurves();
}

bool spine_shear_timeline_get_additive(spine_shear_timeline self) {
	ShearTimeline *_self = (ShearTimeline *) self;
	return _self->getAdditive();
}

bool spine_shear_timeline_get_instant(spine_shear_timeline self) {
	ShearTimeline *_self = (ShearTimeline *) self;
	return _self->getInstant();
}

size_t spine_shear_timeline_get_frame_entries(spine_shear_timeline self) {
	ShearTimeline *_self = (ShearTimeline *) self;
	return _self->getFrameEntries();
}

size_t spine_shear_timeline_get_frame_count(spine_shear_timeline self) {
	ShearTimeline *_self = (ShearTimeline *) self;
	return _self->getFrameCount();
}

spine_array_float spine_shear_timeline_get_frames(spine_shear_timeline self) {
	ShearTimeline *_self = (ShearTimeline *) self;
	return (spine_array_float) &_self->getFrames();
}

float spine_shear_timeline_get_duration(spine_shear_timeline self) {
	ShearTimeline *_self = (ShearTimeline *) self;
	return _self->getDuration();
}

spine_array_property_id spine_shear_timeline_get_property_ids(spine_shear_timeline self) {
	ShearTimeline *_self = (ShearTimeline *) self;
	return (spine_array_property_id) &_self->getPropertyIds();
}

spine_rtti spine_shear_timeline_rtti(void) {
	return (spine_rtti) &ShearTimeline::rtti;
}
