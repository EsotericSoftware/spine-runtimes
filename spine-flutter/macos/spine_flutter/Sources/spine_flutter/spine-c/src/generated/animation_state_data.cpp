#include "animation_state_data.h"
#include <spine/spine.h>

using namespace spine;

spine_animation_state_data spine_animation_state_data_create(spine_skeleton_data skeletonData) {
	return (spine_animation_state_data) new (__FILE__, __LINE__) AnimationStateData(*((SkeletonData *) skeletonData));
}

void spine_animation_state_data_dispose(spine_animation_state_data self) {
	delete (AnimationStateData *) self;
}

spine_skeleton_data spine_animation_state_data_get_skeleton_data(spine_animation_state_data self) {
	AnimationStateData *_self = (AnimationStateData *) self;
	return (spine_skeleton_data) &_self->getSkeletonData();
}

float spine_animation_state_data_get_default_mix(spine_animation_state_data self) {
	AnimationStateData *_self = (AnimationStateData *) self;
	return _self->getDefaultMix();
}

void spine_animation_state_data_set_default_mix(spine_animation_state_data self, float inValue) {
	AnimationStateData *_self = (AnimationStateData *) self;
	_self->setDefaultMix(inValue);
}

void spine_animation_state_data_set_mix_1(spine_animation_state_data self, const char *fromName, const char *toName, float duration) {
	AnimationStateData *_self = (AnimationStateData *) self;
	_self->setMix(String(fromName), String(toName), duration);
}

void spine_animation_state_data_set_mix_2(spine_animation_state_data self, spine_animation from, spine_animation to, float duration) {
	AnimationStateData *_self = (AnimationStateData *) self;
	_self->setMix(*((Animation *) from), *((Animation *) to), duration);
}

float spine_animation_state_data_get_mix(spine_animation_state_data self, spine_animation from, spine_animation to) {
	AnimationStateData *_self = (AnimationStateData *) self;
	return _self->getMix(*((Animation *) from), *((Animation *) to));
}

void spine_animation_state_data_clear(spine_animation_state_data self) {
	AnimationStateData *_self = (AnimationStateData *) self;
	_self->clear();
}
