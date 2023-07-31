/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#ifndef SPINE_ANIMATIONSTATE_H_
#define SPINE_ANIMATIONSTATE_H_

#include <spine/dll.h>
#include <spine/Animation.h>
#include <spine/AnimationStateData.h>
#include <spine/Event.h>
#include <spine/Array.h>

#ifdef __cplusplus
extern "C" {
#endif

typedef enum {
	SP_ANIMATION_START,
	SP_ANIMATION_INTERRUPT,
	SP_ANIMATION_END,
	SP_ANIMATION_COMPLETE,
	SP_ANIMATION_DISPOSE,
	SP_ANIMATION_EVENT
} spEventType;

typedef struct spAnimationState spAnimationState;
typedef struct spTrackEntry spTrackEntry;

typedef void (*spAnimationStateListener)(spAnimationState *state, spEventType type, spTrackEntry *entry,
										 spEvent *event);

_SP_ARRAY_DECLARE_TYPE(spTrackEntryArray, spTrackEntry*)

struct spTrackEntry {
	spAnimation *animation;
	spTrackEntry *previous;
	spTrackEntry *next;
	spTrackEntry *mixingFrom;
	spTrackEntry *mixingTo;
	spAnimationStateListener listener;
	int trackIndex;
	int /*boolean*/ loop;
	int /*boolean*/ holdPrevious;
	int /*boolean*/ reverse;
	int /*boolean*/ shortestRotation;
	float eventThreshold, attachmentThreshold, drawOrderThreshold;
	float animationStart, animationEnd, animationLast, nextAnimationLast;
	float delay, trackTime, trackLast, nextTrackLast, trackEnd, timeScale;
	float alpha, mixTime, mixDuration, interruptAlpha, totalAlpha;
	spMixBlend mixBlend;
	spIntArray *timelineMode;
	spTrackEntryArray *timelineHoldMix;
	float *timelinesRotation;
	int timelinesRotationCount;
	void *rendererObject;
	void *userData;
};

struct spAnimationState {
	spAnimationStateData *const data;

	int tracksCount;
	spTrackEntry **tracks;

	spAnimationStateListener listener;

	float timeScale;

	void *rendererObject;
	void *userData;

	int unkeyedState;
};

/* @param data May be 0 for no mixing. */
SP_API spAnimationState *spAnimationState_create(spAnimationStateData *data);

SP_API void spAnimationState_dispose(spAnimationState *self);

SP_API void spAnimationState_update(spAnimationState *self, float delta);

SP_API int /**bool**/ spAnimationState_apply(spAnimationState *self, struct spSkeleton *skeleton);

SP_API void spAnimationState_clearTracks(spAnimationState *self);

SP_API void spAnimationState_clearTrack(spAnimationState *self, int trackIndex);

/** Set the current animation. Any queued animations are cleared. */
SP_API spTrackEntry *
spAnimationState_setAnimationByName(spAnimationState *self, int trackIndex, const char *animationName,
									int/*bool*/loop);

SP_API spTrackEntry *
spAnimationState_setAnimation(spAnimationState *self, int trackIndex, spAnimation *animation, int/*bool*/loop);

/** Adds an animation to be played delay seconds after the current or last queued animation, taking into account any mix
 * duration. */
SP_API spTrackEntry *
spAnimationState_addAnimationByName(spAnimationState *self, int trackIndex, const char *animationName,
									int/*bool*/loop, float delay);

SP_API spTrackEntry *
spAnimationState_addAnimation(spAnimationState *self, int trackIndex, spAnimation *animation, int/*bool*/loop,
							  float delay);

SP_API spTrackEntry *spAnimationState_setEmptyAnimation(spAnimationState *self, int trackIndex, float mixDuration);

SP_API spTrackEntry *
spAnimationState_addEmptyAnimation(spAnimationState *self, int trackIndex, float mixDuration, float delay);

SP_API void spAnimationState_setEmptyAnimations(spAnimationState *self, float mixDuration);

SP_API spTrackEntry *spAnimationState_getCurrent(spAnimationState *self, int trackIndex);

SP_API void spAnimationState_clearListenerNotifications(spAnimationState *self);

SP_API float spTrackEntry_getAnimationTime(spTrackEntry *entry);

SP_API float spTrackEntry_getTrackComplete(spTrackEntry *entry);

SP_API void spAnimationState_clearNext(spAnimationState *self, spTrackEntry *entry);

/** Use this to dispose static memory before your app exits to appease your memory leak detector*/
SP_API void spAnimationState_disposeStatics();

#ifdef __cplusplus
}
#endif

#endif /* SPINE_ANIMATIONSTATE_H_ */
