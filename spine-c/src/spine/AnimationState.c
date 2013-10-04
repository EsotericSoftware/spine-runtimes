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

#include <spine/Animation.h>
#include <spine/AnimationState.h>
#include <spine/AnimationStateData.h>
#include <spine/Event.h>
#include <spine/extension.h>
#include <spine/Skeleton.h>
#include <spine/SkeletonData.h>
#include <string.h>

TrackEntry* _TrackEntry_create () {
	TrackEntry* entry = NEW(TrackEntry);
	entry->timeScale = 1;
	return entry;
}

void _TrackEntry_dispose (TrackEntry* entry) {
	FREE(entry);
}

void _TrackEntry_disposeAll (TrackEntry* entry) {
	while (entry) {
		TrackEntry* next = entry->next;
		_TrackEntry_dispose(entry);
		entry = next;
	}
}

/**/

typedef struct {
	AnimationState super;
	Event** events;
} _AnimationState;

void _AnimationState_setCurrent (AnimationState* self, int index, TrackEntry* entry);

AnimationState* AnimationState_create (AnimationStateData* data) {
	_AnimationState* internal = NEW(_AnimationState);
	AnimationState* self = SUPER(internal);
	internal->events = MALLOC(Event*, 64);
	self->timeScale = 1;
	CONST_CAST(AnimationStateData*, self->data) = data;
	return self;
}

void AnimationState_dispose (AnimationState* self) {
	int i;
	for (i = 0; i < self->trackCount; i++)
		_TrackEntry_disposeAll(self->tracks[i]);
	FREE(self);
}

void AnimationState_update (AnimationState* self, float delta) {
	int i;
	float time, endTime, trackDelta;
	delta *= self->timeScale;
	for (i = 0; i < self->trackCount; i++) {
		TrackEntry* current = self->tracks[i];
		if (!current) continue;

		trackDelta = delta * current->timeScale;
		time = current->time + trackDelta;
		endTime = current->endTime;

		current->time = time;
		if (current->previous) {
			current->previous->time += trackDelta;
			current->mixTime += trackDelta;
		}

		/* Check if completed the animation or a loop iteration. */
		if (current->loop ?
				(FMOD(current->lastTime, endTime) > FMOD(time, endTime)) : (current->lastTime < endTime && time >= endTime)) {
			int count = (int)(time / endTime);
			if (current->listener) current->listener(self, i, ANIMATION_COMPLETE, 0, count);
			if (self->listener) self->listener(self, i, ANIMATION_COMPLETE, 0, count);
		}

		if (current->next) {
			if (time - trackDelta >= current->next->delay) _AnimationState_setCurrent(self, i, current->next);
		} else {
			/* End non-looping animation when it reaches its end time and there is no next entry. */
			if (!current->loop && current->lastTime >= current->endTime) AnimationState_clearTrack(self, i);
		}
	}
}

void AnimationState_apply (AnimationState* self, Skeleton* skeleton) {
	_AnimationState* internal = SUB_CAST(_AnimationState, self);

	int i, ii;
	int eventCount;
	float time;
	TrackEntry* previous;
	for (i = 0; i < self->trackCount; i++) {
		TrackEntry* current = self->tracks[i];
		if (!current) continue;

		eventCount = 0;

		time = current->time;
		if (!current->loop && time > current->endTime) time = current->endTime;

		previous = current->previous;
		if (!previous) {
			Animation_apply(current->animation, skeleton, current->lastTime, time, current->loop, internal->events,
					&eventCount);
		} else {
			float alpha = current->mixTime / current->mixDuration;

			float previousTime = previous->time;
			if (!previous->loop && previousTime > previous->endTime) previousTime = previous->endTime;
			Animation_apply(previous->animation, skeleton, previousTime, previousTime, previous->loop, 0, 0);

			if (alpha >= 1) {
				alpha = 1;
				_TrackEntry_dispose(current->previous);
				current->previous = 0;
			}
			Animation_mix(current->animation, skeleton, current->lastTime, time, current->loop, internal->events,
					&eventCount, alpha);
		}

		for (ii = 0; ii < eventCount; ii++) {
			Event* event = internal->events[ii];
			if (current->listener) current->listener(self, i, ANIMATION_EVENT, event, 0);
			if (self->listener) self->listener(self, i, ANIMATION_EVENT, event, 0);
		}

		current->lastTime = current->time;
	}
}

void AnimationState_clearTracks (AnimationState* self) {
	int i;
	for (i = 0; i < self->trackCount; i++)
		AnimationState_clearTrack(self, i);
	self->trackCount = 0;
}

void AnimationState_clearTrack (AnimationState* self, int trackIndex) {
	TrackEntry* current;
	if (trackIndex >= self->trackCount) return;
	current = self->tracks[trackIndex];
	if (!current) return;

	if (current->listener) current->listener(self, trackIndex, ANIMATION_END, 0, 0);
	if (self->listener) self->listener(self, trackIndex, ANIMATION_END, 0, 0);

	self->tracks[trackIndex] = 0;
	if (current->previous) _TrackEntry_dispose(current->previous);
	_TrackEntry_disposeAll(current);
}

TrackEntry* _AnimationState_expandToIndex (AnimationState* self, int index) {
	TrackEntry** newTracks;
	if (index < self->trackCount) return self->tracks[index];
	newTracks = CALLOC(TrackEntry*, index + 1);
	memcpy(newTracks, self->tracks, self->trackCount * sizeof(TrackEntry*));
	self->tracks = newTracks;
	self->trackCount = index + 1;
	return 0;
}

void _AnimationState_setCurrent (AnimationState* self, int index, TrackEntry* entry) {
	TrackEntry* current = _AnimationState_expandToIndex(self, index);
	if (current) {
		if (current->previous) {
			_TrackEntry_dispose(current->previous);
			current->previous = 0;
		}

		if (current->listener) current->listener(self, index, ANIMATION_END, 0, 0);
		if (self->listener) self->listener(self, index, ANIMATION_END, 0, 0);

		entry->mixDuration = AnimationStateData_getMix(self->data, current->animation, entry->animation);
		if (entry->mixDuration > 0) {
			entry->mixTime = 0;
			entry->previous = current;
		} else
			_TrackEntry_dispose(current);
	}

	self->tracks[index] = entry;

	if (entry->listener) current->listener(self, index, ANIMATION_START, 0, 0);
	if (self->listener) self->listener(self, index, ANIMATION_START, 0, 0);
}

TrackEntry* AnimationState_setAnimationByName (AnimationState* self, int trackIndex, const char* animationName, int/*bool*/loop) {
	Animation* animation = animationName ? SkeletonData_findAnimation(self->data->skeletonData, animationName) : 0;
	return AnimationState_setAnimation(self, trackIndex, animation, loop);
}

TrackEntry* AnimationState_setAnimation (AnimationState* self, int trackIndex, Animation* animation, int/*bool*/loop) {
	TrackEntry* entry;
	TrackEntry* current = _AnimationState_expandToIndex(self, trackIndex);
	if (current) _TrackEntry_disposeAll(current->next);

	entry = _TrackEntry_create();
	entry->animation = animation;
	entry->loop = loop;
	entry->time = 0;
	entry->endTime = animation ? animation->duration : 0;
	_AnimationState_setCurrent(self, trackIndex, entry);
	return entry;
}

TrackEntry* AnimationState_addAnimationByName (AnimationState* self, int trackIndex, const char* animationName, int/*bool*/loop,
		float delay) {
	Animation* animation = animationName ? SkeletonData_findAnimation(self->data->skeletonData, animationName) : 0;
	return AnimationState_addAnimation(self, trackIndex, animation, loop, delay);
}

TrackEntry* AnimationState_addAnimation (AnimationState* self, int trackIndex, Animation* animation, int/*bool*/loop, float delay) {
	TrackEntry* last;

	TrackEntry* entry = _TrackEntry_create();
	entry->animation = animation;
	entry->loop = loop;
	entry->time = 0;
	entry->endTime = animation ? animation->duration : 0;

	last = _AnimationState_expandToIndex(self, trackIndex);
	if (last) {
		while (last->next)
			last = last->next;
		last->next = entry;
	} else
		self->tracks[trackIndex] = entry;

	if (delay <= 0) {
		if (last) {
			delay += last->endTime;
			if (animation) delay -= AnimationStateData_getMix(self->data, last->animation, animation);
		} else
			delay = 0;
	}
	entry->delay = delay;

	return entry;
}

TrackEntry* AnimationState_getCurrent (AnimationState* self, int trackIndex) {
	if (trackIndex >= self->trackCount) return 0;
	return self->tracks[trackIndex];
}
