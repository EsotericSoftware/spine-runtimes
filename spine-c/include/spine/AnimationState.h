/******************************************************************************
 * Spine Runtime Software License - Version 1.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Essential, Professional, Enterprise, or Education License must
 *    be purchased from Esoteric Software and the license must remain valid:
 *    http://esotericsoftware.com/
 * 2. Redistributions of source code must retain this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer.
 * 3. Redistributions in binary form must reproduce this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer, in the documentation and/or other materials provided with the
 *    distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#ifndef SPINE_ANIMATIONSTATE_H_
#define SPINE_ANIMATIONSTATE_H_

#include <spine/AnimationStateData.h>
#include <spine/Event.h>

#ifdef __cplusplus
extern "C" {
#endif

typedef enum {
	ANIMATION_START, ANIMATION_END, ANIMATION_COMPLETE, ANIMATION_EVENT
} EventType;

typedef struct AnimationState AnimationState;

typedef void (*AnimationStateListener) (AnimationState* state, int trackIndex, EventType type, Event* event, int loopCount);

typedef struct TrackEntry TrackEntry;
struct TrackEntry {
	TrackEntry* next;
	TrackEntry* previous;
	Animation* animation;
	int/*bool*/loop;
	float delay, time, lastTime, endTime, timeScale;
	AnimationStateListener listener;
	float mixTime, mixDuration;
};

struct AnimationState {
	AnimationStateData* const data;
	float timeScale;
	AnimationStateListener listener;
	void* context;

	int trackCount;
	TrackEntry** tracks;
};

/* @param data May be 0 for no mixing. */
AnimationState* AnimationState_create (AnimationStateData* data);
void AnimationState_dispose (AnimationState* self);

void AnimationState_update (AnimationState* self, float delta);
void AnimationState_apply (AnimationState* self, struct Skeleton* skeleton);

void AnimationState_clearTracks (AnimationState* self);
void AnimationState_clearTrack (AnimationState* self, int trackIndex);

/** Set the current animation. Any queued animations are cleared. */
TrackEntry* AnimationState_setAnimationByName (AnimationState* self, int trackIndex, const char* animationName, int/*bool*/loop);
TrackEntry* AnimationState_setAnimation (AnimationState* self, int trackIndex, Animation* animation, int/*bool*/loop);

/** Adds an animation to be played delay seconds after the current or last queued animation, taking into account any mix
 * duration. */
TrackEntry* AnimationState_addAnimationByName (AnimationState* self, int trackIndex, const char* animationName, int/*bool*/loop,
		float delay);
TrackEntry* AnimationState_addAnimation (AnimationState* self, int trackIndex, Animation* animation, int/*bool*/loop,
		float delay);

TrackEntry* AnimationState_getCurrent (AnimationState* self, int trackIndex);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_ANIMATIONSTATE_H_ */
