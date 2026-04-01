#include "slot_curve_timeline.h"
#include <spine/spine.h>

using namespace spine;

void spine_slot_curve_timeline_dispose(spine_slot_curve_timeline self) {
	delete (SlotCurveTimeline *) self;
}

spine_rtti spine_slot_curve_timeline_get_rtti(spine_slot_curve_timeline self) {
	SlotCurveTimeline *_self = (SlotCurveTimeline *) self;
	return (spine_rtti) &_self->getRTTI();
}

void spine_slot_curve_timeline_apply(spine_slot_curve_timeline self, spine_skeleton skeleton, float lastTime, float time,
									 /*@null*/ spine_array_event events, float alpha, bool fromSetup, bool add, bool out, bool appliedPose) {
	SlotCurveTimeline *_self = (SlotCurveTimeline *) self;
	_self->apply(*((Skeleton *) skeleton), lastTime, time, (Array<Event *> *) events, alpha, fromSetup, add, out, appliedPose);
}

int spine_slot_curve_timeline_get_slot_index(spine_slot_curve_timeline self) {
	SlotCurveTimeline *_self = (SlotCurveTimeline *) self;
	return _self->getSlotIndex();
}

void spine_slot_curve_timeline_set_slot_index(spine_slot_curve_timeline self, int inValue) {
	SlotCurveTimeline *_self = (SlotCurveTimeline *) self;
	_self->setSlotIndex(inValue);
}

void spine_slot_curve_timeline_set_linear(spine_slot_curve_timeline self, size_t frame) {
	SlotCurveTimeline *_self = (SlotCurveTimeline *) self;
	_self->setLinear(frame);
}

void spine_slot_curve_timeline_set_stepped(spine_slot_curve_timeline self, size_t frame) {
	SlotCurveTimeline *_self = (SlotCurveTimeline *) self;
	_self->setStepped(frame);
}

void spine_slot_curve_timeline_set_bezier(spine_slot_curve_timeline self, size_t bezier, size_t frame, float value, float time1, float value1,
										  float cx1, float cy1, float cx2, float cy2, float time2, float value2) {
	SlotCurveTimeline *_self = (SlotCurveTimeline *) self;
	_self->setBezier(bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
}

float spine_slot_curve_timeline_get_bezier_value(spine_slot_curve_timeline self, float time, size_t frame, size_t valueOffset, size_t i) {
	SlotCurveTimeline *_self = (SlotCurveTimeline *) self;
	return _self->getBezierValue(time, frame, valueOffset, i);
}

spine_array_float spine_slot_curve_timeline_get_curves(spine_slot_curve_timeline self) {
	SlotCurveTimeline *_self = (SlotCurveTimeline *) self;
	return (spine_array_float) &_self->getCurves();
}

bool spine_slot_curve_timeline_get_additive(spine_slot_curve_timeline self) {
	SlotCurveTimeline *_self = (SlotCurveTimeline *) self;
	return _self->getAdditive();
}

bool spine_slot_curve_timeline_get_instant(spine_slot_curve_timeline self) {
	SlotCurveTimeline *_self = (SlotCurveTimeline *) self;
	return _self->getInstant();
}

size_t spine_slot_curve_timeline_get_frame_entries(spine_slot_curve_timeline self) {
	SlotCurveTimeline *_self = (SlotCurveTimeline *) self;
	return _self->getFrameEntries();
}

size_t spine_slot_curve_timeline_get_frame_count(spine_slot_curve_timeline self) {
	SlotCurveTimeline *_self = (SlotCurveTimeline *) self;
	return _self->getFrameCount();
}

spine_array_float spine_slot_curve_timeline_get_frames(spine_slot_curve_timeline self) {
	SlotCurveTimeline *_self = (SlotCurveTimeline *) self;
	return (spine_array_float) &_self->getFrames();
}

float spine_slot_curve_timeline_get_duration(spine_slot_curve_timeline self) {
	SlotCurveTimeline *_self = (SlotCurveTimeline *) self;
	return _self->getDuration();
}

spine_array_property_id spine_slot_curve_timeline_get_property_ids(spine_slot_curve_timeline self) {
	SlotCurveTimeline *_self = (SlotCurveTimeline *) self;
	return (spine_array_property_id) &_self->getPropertyIds();
}

spine_rtti spine_slot_curve_timeline_rtti(void) {
	return (spine_rtti) &SlotCurveTimeline::rtti;
}
