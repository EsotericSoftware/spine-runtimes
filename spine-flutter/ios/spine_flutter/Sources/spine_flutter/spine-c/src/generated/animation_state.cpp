#include "animation_state.h"
#include <spine/spine.h>

using namespace spine;

spine_animation_state spine_animation_state_create(spine_animation_state_data data) {
	return (spine_animation_state) new (__FILE__, __LINE__) AnimationState(*((AnimationStateData *) data));
}

void spine_animation_state_dispose(spine_animation_state self) {
	delete (AnimationState *) self;
}

void spine_animation_state_update(spine_animation_state self, float delta) {
	AnimationState *_self = (AnimationState *) self;
	_self->update(delta);
}

bool spine_animation_state_apply(spine_animation_state self, spine_skeleton skeleton) {
	AnimationState *_self = (AnimationState *) self;
	return _self->apply(*((Skeleton *) skeleton));
}

void spine_animation_state_clear_tracks(spine_animation_state self) {
	AnimationState *_self = (AnimationState *) self;
	_self->clearTracks();
}

void spine_animation_state_clear_track(spine_animation_state self, size_t trackIndex) {
	AnimationState *_self = (AnimationState *) self;
	_self->clearTrack(trackIndex);
}

spine_track_entry spine_animation_state_set_animation_1(spine_animation_state self, size_t trackIndex, const char *animationName, bool loop) {
	AnimationState *_self = (AnimationState *) self;
	return (spine_track_entry) &_self->setAnimation(trackIndex, String(animationName), loop);
}

spine_track_entry spine_animation_state_set_animation_2(spine_animation_state self, size_t trackIndex, spine_animation animation, bool loop) {
	AnimationState *_self = (AnimationState *) self;
	return (spine_track_entry) &_self->setAnimation(trackIndex, *((Animation *) animation), loop);
}

spine_track_entry spine_animation_state_add_animation_1(spine_animation_state self, size_t trackIndex, const char *animationName, bool loop,
														float delay) {
	AnimationState *_self = (AnimationState *) self;
	return (spine_track_entry) &_self->addAnimation(trackIndex, String(animationName), loop, delay);
}

spine_track_entry spine_animation_state_add_animation_2(spine_animation_state self, size_t trackIndex, spine_animation animation, bool loop,
														float delay) {
	AnimationState *_self = (AnimationState *) self;
	return (spine_track_entry) &_self->addAnimation(trackIndex, *((Animation *) animation), loop, delay);
}

spine_track_entry spine_animation_state_set_empty_animation(spine_animation_state self, size_t trackIndex, float mixDuration) {
	AnimationState *_self = (AnimationState *) self;
	return (spine_track_entry) &_self->setEmptyAnimation(trackIndex, mixDuration);
}

spine_track_entry spine_animation_state_add_empty_animation(spine_animation_state self, size_t trackIndex, float mixDuration, float delay) {
	AnimationState *_self = (AnimationState *) self;
	return (spine_track_entry) &_self->addEmptyAnimation(trackIndex, mixDuration, delay);
}

void spine_animation_state_set_empty_animations(spine_animation_state self, float mixDuration) {
	AnimationState *_self = (AnimationState *) self;
	_self->setEmptyAnimations(mixDuration);
}

/*@null*/ spine_track_entry spine_animation_state_get_track(spine_animation_state self, size_t trackIndex) {
	AnimationState *_self = (AnimationState *) self;
	return (spine_track_entry) _self->getTrack(trackIndex);
}

spine_animation_state_data spine_animation_state_get_data(spine_animation_state self) {
	AnimationState *_self = (AnimationState *) self;
	return (spine_animation_state_data) &_self->getData();
}

spine_array_track_entry spine_animation_state_get_tracks(spine_animation_state self) {
	AnimationState *_self = (AnimationState *) self;
	return (spine_array_track_entry) &_self->getTracks();
}

float spine_animation_state_get_time_scale(spine_animation_state self) {
	AnimationState *_self = (AnimationState *) self;
	return _self->getTimeScale();
}

void spine_animation_state_set_time_scale(spine_animation_state self, float inValue) {
	AnimationState *_self = (AnimationState *) self;
	_self->setTimeScale(inValue);
}

void spine_animation_state_disable_queue(spine_animation_state self) {
	AnimationState *_self = (AnimationState *) self;
	_self->disableQueue();
}

void spine_animation_state_enable_queue(spine_animation_state self) {
	AnimationState *_self = (AnimationState *) self;
	_self->enableQueue();
}

void spine_animation_state_set_manual_track_entry_disposal(spine_animation_state self, bool inValue) {
	AnimationState *_self = (AnimationState *) self;
	_self->setManualTrackEntryDisposal(inValue);
}

bool spine_animation_state_get_manual_track_entry_disposal(spine_animation_state self) {
	AnimationState *_self = (AnimationState *) self;
	return _self->getManualTrackEntryDisposal();
}

void spine_animation_state_dispose_track_entry(spine_animation_state self, /*@null*/ spine_track_entry entry) {
	AnimationState *_self = (AnimationState *) self;
	_self->disposeTrackEntry((TrackEntry *) entry);
}

/*@null*/ void *spine_animation_state_get_renderer_object(spine_animation_state self) {
	AnimationState *_self = (AnimationState *) self;
	return _self->getRendererObject();
}
