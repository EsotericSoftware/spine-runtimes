#include "track_entry.h"
#include <spine/spine.h>

using namespace spine;

spine_track_entry spine_track_entry_create(void) {
	return (spine_track_entry) new (__FILE__, __LINE__) TrackEntry();
}

void spine_track_entry_dispose(spine_track_entry self) {
	delete (TrackEntry *) self;
}

int spine_track_entry_get_track_index(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return _self->getTrackIndex();
}

spine_animation spine_track_entry_get_animation(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return (spine_animation) &_self->getAnimation();
}

void spine_track_entry_set_animation(spine_track_entry self, spine_animation animation) {
	TrackEntry *_self = (TrackEntry *) self;
	_self->setAnimation(*((Animation *) animation));
}

/*@null*/ spine_track_entry spine_track_entry_get_previous(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return (spine_track_entry) _self->getPrevious();
}

bool spine_track_entry_get_loop(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return _self->getLoop();
}

void spine_track_entry_set_loop(spine_track_entry self, bool inValue) {
	TrackEntry *_self = (TrackEntry *) self;
	_self->setLoop(inValue);
}

bool spine_track_entry_get_additive(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return _self->getAdditive();
}

void spine_track_entry_set_additive(spine_track_entry self, bool inValue) {
	TrackEntry *_self = (TrackEntry *) self;
	_self->setAdditive(inValue);
}

bool spine_track_entry_get_reverse(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return _self->getReverse();
}

void spine_track_entry_set_reverse(spine_track_entry self, bool inValue) {
	TrackEntry *_self = (TrackEntry *) self;
	_self->setReverse(inValue);
}

bool spine_track_entry_get_shortest_rotation(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return _self->getShortestRotation();
}

void spine_track_entry_set_shortest_rotation(spine_track_entry self, bool inValue) {
	TrackEntry *_self = (TrackEntry *) self;
	_self->setShortestRotation(inValue);
}

float spine_track_entry_get_delay(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return _self->getDelay();
}

void spine_track_entry_set_delay(spine_track_entry self, float inValue) {
	TrackEntry *_self = (TrackEntry *) self;
	_self->setDelay(inValue);
}

float spine_track_entry_get_track_time(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return _self->getTrackTime();
}

void spine_track_entry_set_track_time(spine_track_entry self, float inValue) {
	TrackEntry *_self = (TrackEntry *) self;
	_self->setTrackTime(inValue);
}

float spine_track_entry_get_track_end(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return _self->getTrackEnd();
}

void spine_track_entry_set_track_end(spine_track_entry self, float inValue) {
	TrackEntry *_self = (TrackEntry *) self;
	_self->setTrackEnd(inValue);
}

float spine_track_entry_get_animation_start(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return _self->getAnimationStart();
}

void spine_track_entry_set_animation_start(spine_track_entry self, float inValue) {
	TrackEntry *_self = (TrackEntry *) self;
	_self->setAnimationStart(inValue);
}

float spine_track_entry_get_animation_end(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return _self->getAnimationEnd();
}

void spine_track_entry_set_animation_end(spine_track_entry self, float inValue) {
	TrackEntry *_self = (TrackEntry *) self;
	_self->setAnimationEnd(inValue);
}

float spine_track_entry_get_animation_last(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return _self->getAnimationLast();
}

void spine_track_entry_set_animation_last(spine_track_entry self, float inValue) {
	TrackEntry *_self = (TrackEntry *) self;
	_self->setAnimationLast(inValue);
}

float spine_track_entry_get_animation_time(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return _self->getAnimationTime();
}

float spine_track_entry_get_time_scale(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return _self->getTimeScale();
}

void spine_track_entry_set_time_scale(spine_track_entry self, float inValue) {
	TrackEntry *_self = (TrackEntry *) self;
	_self->setTimeScale(inValue);
}

float spine_track_entry_get_alpha(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return _self->getAlpha();
}

void spine_track_entry_set_alpha(spine_track_entry self, float inValue) {
	TrackEntry *_self = (TrackEntry *) self;
	_self->setAlpha(inValue);
}

float spine_track_entry_get_event_threshold(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return _self->getEventThreshold();
}

void spine_track_entry_set_event_threshold(spine_track_entry self, float inValue) {
	TrackEntry *_self = (TrackEntry *) self;
	_self->setEventThreshold(inValue);
}

float spine_track_entry_get_mix_attachment_threshold(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return _self->getMixAttachmentThreshold();
}

void spine_track_entry_set_mix_attachment_threshold(spine_track_entry self, float inValue) {
	TrackEntry *_self = (TrackEntry *) self;
	_self->setMixAttachmentThreshold(inValue);
}

float spine_track_entry_get_alpha_attachment_threshold(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return _self->getAlphaAttachmentThreshold();
}

void spine_track_entry_set_alpha_attachment_threshold(spine_track_entry self, float inValue) {
	TrackEntry *_self = (TrackEntry *) self;
	_self->setAlphaAttachmentThreshold(inValue);
}

float spine_track_entry_get_mix_draw_order_threshold(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return _self->getMixDrawOrderThreshold();
}

void spine_track_entry_set_mix_draw_order_threshold(spine_track_entry self, float inValue) {
	TrackEntry *_self = (TrackEntry *) self;
	_self->setMixDrawOrderThreshold(inValue);
}

/*@null*/ spine_track_entry spine_track_entry_get_next(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return (spine_track_entry) _self->getNext();
}

bool spine_track_entry_is_complete(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return _self->isComplete();
}

float spine_track_entry_get_mix_time(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return _self->getMixTime();
}

void spine_track_entry_set_mix_time(spine_track_entry self, float inValue) {
	TrackEntry *_self = (TrackEntry *) self;
	_self->setMixTime(inValue);
}

float spine_track_entry_get_mix_duration(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return _self->getMixDuration();
}

void spine_track_entry_set_mix_duration_1(spine_track_entry self, float inValue) {
	TrackEntry *_self = (TrackEntry *) self;
	_self->setMixDuration(inValue);
}

void spine_track_entry_set_mix_duration_2(spine_track_entry self, float mixDuration, float delay) {
	TrackEntry *_self = (TrackEntry *) self;
	_self->setMixDuration(mixDuration, delay);
}

spine_interpolation spine_track_entry_get_mix_interpolation(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return (spine_interpolation) &_self->getMixInterpolation();
}

void spine_track_entry_set_mix_interpolation(spine_track_entry self, spine_interpolation mixInterpolation) {
	TrackEntry *_self = (TrackEntry *) self;
	_self->setMixInterpolation(*((Interpolation *) mixInterpolation));
}

/*@null*/ spine_track_entry spine_track_entry_get_mixing_from(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return (spine_track_entry) _self->getMixingFrom();
}

/*@null*/ spine_track_entry spine_track_entry_get_mixing_to(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return (spine_track_entry) _self->getMixingTo();
}

void spine_track_entry_reset_rotation_directions(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	_self->resetRotationDirections();
}

float spine_track_entry_get_track_complete(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return _self->getTrackComplete();
}

bool spine_track_entry_is_empty_animation(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return _self->isEmptyAnimation();
}

bool spine_track_entry_was_applied(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return _self->wasApplied();
}

bool spine_track_entry_is_next_ready(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return _self->isNextReady();
}

/*@null*/ spine_animation_state spine_track_entry_get_animation_state(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return (spine_animation_state) _self->getAnimationState();
}

void spine_track_entry_set_animation_state(spine_track_entry self, /*@null*/ spine_animation_state state) {
	TrackEntry *_self = (TrackEntry *) self;
	_self->setAnimationState((AnimationState *) state);
}

/*@null*/ void *spine_track_entry_get_renderer_object(spine_track_entry self) {
	TrackEntry *_self = (TrackEntry *) self;
	return _self->getRendererObject();
}
