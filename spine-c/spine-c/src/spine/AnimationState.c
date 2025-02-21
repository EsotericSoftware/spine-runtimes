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

#include <limits.h>
#include <spine/AnimationState.h>
#include <spine/extension.h>

#define SUBSEQUENT 0
#define FIRST 1
#define HOLD_SUBSEQUENT 2
#define HOLD_FIRST 3
#define HOLD_MIX 4

#define SETUP 1
#define CURRENT 2

_SP_ARRAY_IMPLEMENT_TYPE(spTrackEntryArray, spTrackEntry *)

static spAnimation *SP_EMPTY_ANIMATION = 0;

void spAnimationState_disposeStatics(void) {
	if (SP_EMPTY_ANIMATION) spAnimation_dispose(SP_EMPTY_ANIMATION);
	SP_EMPTY_ANIMATION = 0;
}

/* Forward declaration of some "private" functions so we can keep
 the same function order in C as we have method order in Java. */
void _spAnimationState_disposeTrackEntry(spTrackEntry *entry);

void _spAnimationState_disposeTrackEntries(spAnimationState *state, spTrackEntry *entry);

int /*boolean*/ _spAnimationState_updateMixingFrom(spAnimationState *self, spTrackEntry *entry, float delta);

float _spAnimationState_applyMixingFrom(spAnimationState *self, spTrackEntry *entry, spSkeleton *skeleton,
										spMixBlend currentBlend);

void _spAnimationState_applyRotateTimeline(spAnimationState *self, spTimeline *timeline, spSkeleton *skeleton, float time,
										   float alpha, spMixBlend blend, float *timelinesRotation, int i,
										   int /*boolean*/ firstFrame);

void _spAnimationState_applyAttachmentTimeline(spAnimationState *self, spTimeline *timeline, spSkeleton *skeleton,
											   float animationTime, spMixBlend blend, int /*bool*/ firstFrame);

void _spAnimationState_queueEvents(spAnimationState *self, spTrackEntry *entry, float animationTime);

void _spAnimationState_setCurrent(spAnimationState *self, int index, spTrackEntry *current, int /*boolean*/ interrupt);

spTrackEntry *_spAnimationState_expandToIndex(spAnimationState *self, int index);

spTrackEntry *
_spAnimationState_trackEntry(spAnimationState *self, int trackIndex, spAnimation *animation, int /*boolean*/ loop,
							 spTrackEntry *last);

void _spAnimationState_animationsChanged(spAnimationState *self);

float *_spAnimationState_resizeTimelinesRotation(spTrackEntry *entry, int newSize);

void _spAnimationState_ensureCapacityPropertyIDs(spAnimationState *self, int capacity);

int _spAnimationState_addPropertyID(spAnimationState *self, spPropertyId id);

void _spTrackEntry_computeHold(spTrackEntry *self, spAnimationState *state);

_spEventQueue *_spEventQueue_create(_spAnimationState *state) {
	_spEventQueue *self = CALLOC(_spEventQueue, 1);
	self->state = state;
	self->objectsCount = 0;
	self->objectsCapacity = 16;
	self->objects = CALLOC(_spEventQueueItem, self->objectsCapacity);
	self->drainDisabled = 0;
	return self;
}

void _spEventQueue_free(_spEventQueue *self) {
	FREE(self->objects);
	FREE(self);
}

void _spEventQueue_ensureCapacity(_spEventQueue *self, int newElements) {
	if (self->objectsCount + newElements > self->objectsCapacity) {
		_spEventQueueItem *newObjects;
		self->objectsCapacity <<= 1;
		newObjects = CALLOC(_spEventQueueItem, self->objectsCapacity);
		memcpy(newObjects, self->objects, sizeof(_spEventQueueItem) * self->objectsCount);
		FREE(self->objects);
		self->objects = newObjects;
	}
}

void _spEventQueue_addType(_spEventQueue *self, spEventType type) {
	_spEventQueue_ensureCapacity(self, 1);
	self->objects[self->objectsCount++].type = type;
}

void _spEventQueue_addEntry(_spEventQueue *self, spTrackEntry *entry) {
	_spEventQueue_ensureCapacity(self, 1);
	self->objects[self->objectsCount++].entry = entry;
}

void _spEventQueue_addEvent(_spEventQueue *self, spEvent *event) {
	_spEventQueue_ensureCapacity(self, 1);
	self->objects[self->objectsCount++].event = event;
}

void _spEventQueue_start(_spEventQueue *self, spTrackEntry *entry) {
	_spEventQueue_addType(self, SP_ANIMATION_START);
	_spEventQueue_addEntry(self, entry);
	self->state->animationsChanged = 1;
}

void _spEventQueue_interrupt(_spEventQueue *self, spTrackEntry *entry) {
	_spEventQueue_addType(self, SP_ANIMATION_INTERRUPT);
	_spEventQueue_addEntry(self, entry);
}

void _spEventQueue_end(_spEventQueue *self, spTrackEntry *entry) {
	_spEventQueue_addType(self, SP_ANIMATION_END);
	_spEventQueue_addEntry(self, entry);
	self->state->animationsChanged = 1;
}

void _spEventQueue_dispose(_spEventQueue *self, spTrackEntry *entry) {
	_spEventQueue_addType(self, SP_ANIMATION_DISPOSE);
	_spEventQueue_addEntry(self, entry);
}

void _spEventQueue_complete(_spEventQueue *self, spTrackEntry *entry) {
	_spEventQueue_addType(self, SP_ANIMATION_COMPLETE);
	_spEventQueue_addEntry(self, entry);
}

void _spEventQueue_event(_spEventQueue *self, spTrackEntry *entry, spEvent *event) {
	_spEventQueue_addType(self, SP_ANIMATION_EVENT);
	_spEventQueue_addEntry(self, entry);
	_spEventQueue_addEvent(self, event);
}

void _spEventQueue_clear(_spEventQueue *self) {
	self->objectsCount = 0;
}

void _spEventQueue_drain(_spEventQueue *self) {
	int i;
	if (self->drainDisabled) return;
	self->drainDisabled = 1;
	for (i = 0; i < self->objectsCount; i += 2) {
		spEventType type = (spEventType) self->objects[i].type;
		spTrackEntry *entry = self->objects[i + 1].entry;
		spEvent *event;
		switch (type) {
			case SP_ANIMATION_START:
			case SP_ANIMATION_INTERRUPT:
			case SP_ANIMATION_COMPLETE:
				if (entry->listener) entry->listener(SUPER(self->state), type, entry, 0);
				if (self->state->super.listener) self->state->super.listener(SUPER(self->state), type, entry, 0);
				break;
			case SP_ANIMATION_END:
				if (entry->listener) entry->listener(SUPER(self->state), type, entry, 0);
				if (self->state->super.listener) self->state->super.listener(SUPER(self->state), type, entry, 0);
				/* Fall through. */
			case SP_ANIMATION_DISPOSE:
				if (entry->listener) entry->listener(SUPER(self->state), SP_ANIMATION_DISPOSE, entry, 0);
				if (self->state->super.listener)
					self->state->super.listener(SUPER(self->state), SP_ANIMATION_DISPOSE, entry, 0);
				_spAnimationState_disposeTrackEntry(entry);
				break;
			case SP_ANIMATION_EVENT:
				event = self->objects[i + 2].event;
				if (entry->listener) entry->listener(SUPER(self->state), type, entry, event);
				if (self->state->super.listener) self->state->super.listener(SUPER(self->state), type, entry, event);
				i++;
				break;
		}
	}
	_spEventQueue_clear(self);

	self->drainDisabled = 0;
}

/* These two functions are needed in the UE4 runtime, see #1037 */
void _spAnimationState_enableQueue(spAnimationState *self) {
	_spAnimationState *internal = SUB_CAST(_spAnimationState, self);
	internal->queue->drainDisabled = 0;
}

void _spAnimationState_disableQueue(spAnimationState *self) {
	_spAnimationState *internal = SUB_CAST(_spAnimationState, self);
	internal->queue->drainDisabled = 1;
}

void _spAnimationState_disposeTrackEntry(spTrackEntry *entry) {
	spIntArray_dispose(entry->timelineMode);
	spTrackEntryArray_dispose(entry->timelineHoldMix);
	FREE(entry->timelinesRotation);
	FREE(entry);
}

void _spAnimationState_disposeTrackEntries(spAnimationState *state, spTrackEntry *entry) {
	while (entry) {
		spTrackEntry *next = entry->next;
		spTrackEntry *from = entry->mixingFrom;
		while (from) {
			spTrackEntry *nextFrom = from->mixingFrom;
			if (entry->listener) entry->listener(state, SP_ANIMATION_DISPOSE, from, 0);
			if (state->listener) state->listener(state, SP_ANIMATION_DISPOSE, from, 0);
			_spAnimationState_disposeTrackEntry(from);
			from = nextFrom;
		}
		if (entry->listener) entry->listener(state, SP_ANIMATION_DISPOSE, entry, 0);
		if (state->listener) state->listener(state, SP_ANIMATION_DISPOSE, entry, 0);
		_spAnimationState_disposeTrackEntry(entry);
		entry = next;
	}
}

spAnimationState *spAnimationState_create(spAnimationStateData *data) {
	_spAnimationState *internal;
	spAnimationState *self;

	if (!SP_EMPTY_ANIMATION) {
		SP_EMPTY_ANIMATION = (spAnimation *) 1; /* dirty trick so we can recursively call spAnimation_create */
		SP_EMPTY_ANIMATION = spAnimation_create("<empty>", NULL, 0);
	}

	internal = NEW(_spAnimationState);
	self = SUPER(internal);

	self->data = data;
	self->timeScale = 1;

	internal->queue = _spEventQueue_create(internal);
	internal->events = CALLOC(spEvent *, 128);

	internal->propertyIDs = CALLOC(spPropertyId, 128);
	internal->propertyIDsCapacity = 128;

	return self;
}

void spAnimationState_dispose(spAnimationState *self) {
	int i;
	_spAnimationState *internal = SUB_CAST(_spAnimationState, self);
	for (i = 0; i < self->tracksCount; i++)
		_spAnimationState_disposeTrackEntries(self, self->tracks[i]);
	FREE(self->tracks);
	_spEventQueue_free(internal->queue);
	FREE(internal->events);
	FREE(internal->propertyIDs);
	FREE(internal);
}

void spAnimationState_update(spAnimationState *self, float delta) {
	int i, n;
	_spAnimationState *internal = SUB_CAST(_spAnimationState, self);
	delta *= self->timeScale;
	for (i = 0, n = self->tracksCount; i < n; i++) {
		float currentDelta;
		spTrackEntry *current = self->tracks[i];
		spTrackEntry *next;
		if (!current) continue;

		current->animationLast = current->nextAnimationLast;
		current->trackLast = current->nextTrackLast;

		currentDelta = delta * current->timeScale;

		if (current->delay > 0) {
			current->delay -= currentDelta;
			if (current->delay > 0) continue;
			currentDelta = -current->delay;
			current->delay = 0;
		}

		next = current->next;
		if (next) {
			/* When the next entry's delay is passed, change to the next entry, preserving leftover time. */
			float nextTime = current->trackLast - next->delay;
			if (nextTime >= 0) {
				next->delay = 0;
				next->trackTime +=
						current->timeScale == 0 ? 0 : (nextTime / current->timeScale + delta) * next->timeScale;
				current->trackTime += currentDelta;
				_spAnimationState_setCurrent(self, i, next, 1);
				while (next->mixingFrom) {
					next->mixTime += delta;
					next = next->mixingFrom;
				}
				continue;
			}
		} else {
			/* Clear the track when there is no next entry, the track end time is reached, and there is no mixingFrom. */
			if (current->trackLast >= current->trackEnd && current->mixingFrom == 0) {
				self->tracks[i] = 0;
				_spEventQueue_end(internal->queue, current);
				spAnimationState_clearNext(self, current);
				continue;
			}
		}
		if (current->mixingFrom != 0 && _spAnimationState_updateMixingFrom(self, current, delta)) {
			/* End mixing from entries once all have completed. */
			spTrackEntry *from = current->mixingFrom;
			current->mixingFrom = 0;
			if (from != 0) from->mixingTo = 0;
			while (from != 0) {
				_spEventQueue_end(internal->queue, from);
				from = from->mixingFrom;
			}
		}

		current->trackTime += currentDelta;
	}

	_spEventQueue_drain(internal->queue);
}

int /*boolean*/ _spAnimationState_updateMixingFrom(spAnimationState *self, spTrackEntry *to, float delta) {
	spTrackEntry *from = to->mixingFrom;
	int finished;
	_spAnimationState *internal = SUB_CAST(_spAnimationState, self);
	if (!from) return -1;

	finished = _spAnimationState_updateMixingFrom(self, from, delta);

	from->animationLast = from->nextAnimationLast;
	from->trackLast = from->nextTrackLast;

	if (to->nextTrackLast != -1) {                           // The from entry was applied at least once.
		int discard = to->mixTime == 0 && from->mixTime == 0;// Discard the from entry when neither have advanced yet.
		if (to->mixTime >= to->mixDuration || discard) {
			// Require totalAlpha == 0 to ensure mixing is complete or the transition is a single frame or discarded.
			if (from->totalAlpha == 0 || to->mixDuration == 0 || discard) {
				to->mixingFrom = from->mixingFrom;
				if (from->mixingFrom) from->mixingFrom->mixingTo = to;
				to->interruptAlpha = from->interruptAlpha;
				_spEventQueue_end(internal->queue, from);
			}
			return finished;
		}
	}

	from->trackTime += delta * from->timeScale;
	to->mixTime += delta;
	return 0;
}

int spAnimationState_apply(spAnimationState *self, spSkeleton *skeleton) {
	_spAnimationState *internal = SUB_CAST(_spAnimationState, self);
	spTrackEntry *current;
	int i, ii, n;
	float animationLast, animationTime;
	int timelineCount;
	spTimeline **timelines;
	int /*boolean*/ firstFrame, shortestRotation;
	float *timelinesRotation;
	spTimeline *timeline;
	int applied = 0;
	spMixBlend blend;
	spMixBlend timelineBlend;
	int setupState = 0;
	spSlot **slots = NULL;
	spSlot *slot = NULL;
	const char *attachmentName = NULL;
	spEvent **applyEvents = NULL;
	float applyTime;

	if (internal->animationsChanged) _spAnimationState_animationsChanged(self);

	for (i = 0, n = self->tracksCount; i < n; i++) {
		float alpha;
		current = self->tracks[i];
		if (!current || current->delay > 0) continue;
		applied = -1;
		blend = i == 0 ? SP_MIX_BLEND_FIRST : current->mixBlend;

		/* Apply mixing from entries first. */
		alpha = current->alpha;
		if (current->mixingFrom)
			alpha *= _spAnimationState_applyMixingFrom(self, current, skeleton, blend);
		else if (current->trackTime >= current->trackEnd && current->next == 0)
			alpha = 0;
		int /*bool*/ attachments = alpha >= current->alphaAttachmentThreshold;

		/* Apply current entry. */
		animationLast = current->animationLast;
		animationTime = spTrackEntry_getAnimationTime(current);
		timelineCount = current->animation->timelines->size;
		applyEvents = internal->events;
		applyTime = animationTime;
		if (current->reverse) {
			applyTime = current->animation->duration - applyTime;
			applyEvents = NULL;
		}
		timelines = current->animation->timelines->items;
		if ((i == 0 && alpha == 1) || blend == SP_MIX_BLEND_ADD) {
			for (ii = 0; ii < timelineCount; ii++) {
				timeline = timelines[ii];
				if (timeline->type == SP_TIMELINE_ATTACHMENT) {
					_spAnimationState_applyAttachmentTimeline(self, timeline, skeleton, applyTime, blend, attachments);
				} else {
					spTimeline_apply(timelines[ii], skeleton, animationLast, applyTime, applyEvents,
									 &internal->eventsCount, alpha, blend, SP_MIX_DIRECTION_IN);
				}
			}
		} else {
			spIntArray *timelineMode = current->timelineMode;

			shortestRotation = current->shortestRotation;
			firstFrame = !shortestRotation && current->timelinesRotationCount != timelineCount << 1;
			if (firstFrame) _spAnimationState_resizeTimelinesRotation(current, timelineCount << 1);
			timelinesRotation = current->timelinesRotation;

			for (ii = 0; ii < timelineCount; ii++) {
				timeline = timelines[ii];
				timelineBlend = timelineMode->items[ii] == SUBSEQUENT ? blend : SP_MIX_BLEND_SETUP;
				if (!shortestRotation && timeline->type == SP_TIMELINE_ROTATE)
					_spAnimationState_applyRotateTimeline(self, timeline, skeleton, applyTime, alpha, timelineBlend,
														  timelinesRotation, ii << 1, firstFrame);
				else if (timeline->type == SP_TIMELINE_ATTACHMENT)
					_spAnimationState_applyAttachmentTimeline(self, timeline, skeleton, applyTime, timelineBlend, attachments);
				else
					spTimeline_apply(timeline, skeleton, animationLast, applyTime, applyEvents, &internal->eventsCount,
									 alpha, timelineBlend, SP_MIX_DIRECTION_IN);
			}
		}
		_spAnimationState_queueEvents(self, current, animationTime);
		internal->eventsCount = 0;
		current->nextAnimationLast = animationTime;
		current->nextTrackLast = current->trackTime;
	}

	setupState = self->unkeyedState + SETUP;
	slots = skeleton->slots;
	for (i = 0, n = skeleton->slotsCount; i < n; i++) {
		slot = slots[i];
		if (slot->attachmentState == setupState) {
			attachmentName = slot->data->attachmentName;
			spSlot_setAttachment(slot, attachmentName == NULL ? NULL : spSkeleton_getAttachmentForSlotIndex(skeleton, slot->data->index, attachmentName));
		}
	}
	self->unkeyedState += 2;

	_spEventQueue_drain(internal->queue);
	return applied;
}

float _spAnimationState_applyMixingFrom(spAnimationState *self, spTrackEntry *to, spSkeleton *skeleton, spMixBlend blend) {
	_spAnimationState *internal = SUB_CAST(_spAnimationState, self);
	float mix;
	spEvent **events;
	int /*boolean*/ attachments;
	int /*boolean*/ drawOrder;
	float animationLast;
	float animationTime;
	int timelineCount;
	spTimeline **timelines;
	spIntArray *timelineMode;
	spTrackEntryArray *timelineHoldMix;
	spMixBlend timelineBlend;
	float alphaHold;
	float alphaMix;
	float alpha;
	int /*boolean*/ firstFrame, shortestRotation;
	float *timelinesRotation;
	int i;
	spTrackEntry *holdMix;
	float applyTime;

	spTrackEntry *from = to->mixingFrom;
	if (from->mixingFrom) _spAnimationState_applyMixingFrom(self, from, skeleton, blend);

	if (to->mixDuration == 0) { /* Single frame mix to undo mixingFrom changes. */
		mix = 1;
		if (blend == SP_MIX_BLEND_FIRST) blend = SP_MIX_BLEND_SETUP;
	} else {
		mix = to->mixTime / to->mixDuration;
		if (mix > 1) mix = 1;
		if (blend != SP_MIX_BLEND_FIRST) blend = from->mixBlend;
	}

	attachments = mix < from->mixAttachmentThreshold;
	drawOrder = mix < from->mixDrawOrderThreshold;
	timelineCount = from->animation->timelines->size;
	timelines = from->animation->timelines->items;
	alphaHold = from->alpha * to->interruptAlpha;
	alphaMix = alphaHold * (1 - mix);
	animationLast = from->animationLast;
	animationTime = spTrackEntry_getAnimationTime(from);
	applyTime = animationTime;
	events = NULL;
	if (from->reverse) {
		applyTime = from->animation->duration - applyTime;
	} else {
		if (mix < from->eventThreshold) events = internal->events;
	}

	if (blend == SP_MIX_BLEND_ADD) {
		for (i = 0; i < timelineCount; i++) {
			spTimeline *timeline = timelines[i];
			spTimeline_apply(timeline, skeleton, animationLast, applyTime, events, &internal->eventsCount, alphaMix,
							 blend, SP_MIX_DIRECTION_OUT);
		}
	} else {
		timelineMode = from->timelineMode;
		timelineHoldMix = from->timelineHoldMix;

		shortestRotation = from->shortestRotation;
		firstFrame = !shortestRotation && from->timelinesRotationCount != timelineCount << 1;
		if (firstFrame) _spAnimationState_resizeTimelinesRotation(from, timelineCount << 1);
		timelinesRotation = from->timelinesRotation;

		from->totalAlpha = 0;
		for (i = 0; i < timelineCount; i++) {
			spMixDirection direction = SP_MIX_DIRECTION_OUT;
			spTimeline *timeline = timelines[i];

			switch (timelineMode->items[i]) {
				case SUBSEQUENT:
					if (!drawOrder && timeline->type == SP_TIMELINE_DRAWORDER) continue;
					timelineBlend = blend;
					alpha = alphaMix;
					break;
				case FIRST:
					timelineBlend = SP_MIX_BLEND_SETUP;
					alpha = alphaMix;
					break;
				case HOLD_SUBSEQUENT:
					timelineBlend = blend;
					alpha = alphaHold;
					break;
				case HOLD_FIRST:
					timelineBlend = SP_MIX_BLEND_SETUP;
					alpha = alphaHold;
					break;
				default:
					timelineBlend = SP_MIX_BLEND_SETUP;
					holdMix = timelineHoldMix->items[i];
					alpha = alphaHold * MAX(0, 1 - holdMix->mixTime / holdMix->mixDuration);
					break;
			}
			from->totalAlpha += alpha;
			if (!shortestRotation && timeline->type == SP_TIMELINE_ROTATE)
				_spAnimationState_applyRotateTimeline(self, timeline, skeleton, applyTime, alpha, timelineBlend,
													  timelinesRotation, i << 1, firstFrame);
			else if (timeline->type == SP_TIMELINE_ATTACHMENT)
				_spAnimationState_applyAttachmentTimeline(self, timeline, skeleton, applyTime, timelineBlend,
														  attachments && alpha >= from->alphaAttachmentThreshold);
			else {
				if (drawOrder && timeline->type == SP_TIMELINE_DRAWORDER &&
					timelineBlend == SP_MIX_BLEND_SETUP)
					direction = SP_MIX_DIRECTION_IN;
				spTimeline_apply(timeline, skeleton, animationLast, applyTime, events, &internal->eventsCount,
								 alpha, timelineBlend, direction);
			}
		}
	}


	if (to->mixDuration > 0) _spAnimationState_queueEvents(self, from, animationTime);
	internal->eventsCount = 0;
	from->nextAnimationLast = animationTime;
	from->nextTrackLast = from->trackTime;

	return mix;
}

static void
_spAnimationState_setAttachment(spAnimationState *self, spSkeleton *skeleton, spSlot *slot, const char *attachmentName,
								int /*bool*/ attachments) {
	spSlot_setAttachment(slot, attachmentName == NULL ? NULL : spSkeleton_getAttachmentForSlotIndex(skeleton, slot->data->index, attachmentName));
	if (attachments) slot->attachmentState = self->unkeyedState + CURRENT;
}

/* @param target After the first and before the last entry. */
static int binarySearch1(float *values, int valuesLength, float target) {
	int i;
	for (i = 1; i < valuesLength; i++) {
		if (values[i] > target) return (int) (i - 1);
	}
	return (int) valuesLength - 1;
}

void _spAnimationState_applyAttachmentTimeline(spAnimationState *self, spTimeline *timeline, spSkeleton *skeleton,
											   float time, spMixBlend blend, int /*bool*/ attachments) {
	spAttachmentTimeline *attachmentTimeline;
	spSlot *slot;
	float *frames;

	attachmentTimeline = SUB_CAST(spAttachmentTimeline, timeline);
	slot = skeleton->slots[attachmentTimeline->slotIndex];
	if (!slot->bone->active) return;

	frames = attachmentTimeline->super.frames->items;
	if (time < frames[0]) {
		if (blend == SP_MIX_BLEND_SETUP || blend == SP_MIX_BLEND_FIRST)
			_spAnimationState_setAttachment(self, skeleton, slot, slot->data->attachmentName, attachments);
	} else {
		_spAnimationState_setAttachment(self, skeleton, slot, attachmentTimeline->attachmentNames[binarySearch1(frames, attachmentTimeline->super.frames->size, time)],
										attachments);
	}

	/* If an attachment wasn't set (ie before the first frame or attachments is false), set the setup attachment later.*/
	if (slot->attachmentState <= self->unkeyedState) slot->attachmentState = self->unkeyedState + SETUP;
}

void _spAnimationState_applyRotateTimeline(spAnimationState *self, spTimeline *timeline, spSkeleton *skeleton, float time,
										   float alpha, spMixBlend blend, float *timelinesRotation, int i,
										   int /*boolean*/ firstFrame) {
	spRotateTimeline *rotateTimeline;
	float *frames;
	spBone *bone;
	float r1, r2;
	float total, diff;
	int /*boolean*/ current, dir;
	UNUSED(self);

	if (firstFrame) timelinesRotation[i] = 0;

	if (alpha == 1) {
		spTimeline_apply(timeline, skeleton, 0, time, 0, 0, 1, blend, SP_MIX_DIRECTION_IN);
		return;
	}

	rotateTimeline = SUB_CAST(spRotateTimeline, timeline);
	frames = rotateTimeline->super.super.frames->items;
	bone = skeleton->bones[rotateTimeline->boneIndex];
	if (!bone->active) return;
	if (time < frames[0]) {
		switch (blend) {
			case SP_MIX_BLEND_SETUP:
				bone->rotation = bone->data->rotation;
			default:
				return;
			case SP_MIX_BLEND_FIRST:
				r1 = bone->rotation;
				r2 = bone->data->rotation;
		}
	} else {
		r1 = blend == SP_MIX_BLEND_SETUP ? bone->data->rotation : bone->rotation;
		r2 = bone->data->rotation + spCurveTimeline1_getCurveValue(&rotateTimeline->super, time);
	}

	/* Mix between rotations using the direction of the shortest route on the first frame while detecting crosses. */
	diff = r2 - r1;
	diff -= CEIL(diff / 360 - 0.5) * 360;
	if (diff == 0) {
		total = timelinesRotation[i];
	} else {
		float lastTotal, lastDiff, loops;
		if (firstFrame) {
			lastTotal = 0;
			lastDiff = diff;
		} else {
			lastTotal = timelinesRotation[i];
			lastDiff = timelinesRotation[i + 1];
		}
		loops = lastTotal - FMOD(lastTotal, 360);
		total = diff + loops;
		current = diff >= 0, dir = lastTotal >= 0;
		if (ABS(lastDiff) <= 90 && SIGNUM(lastDiff) != SIGNUM(diff)) {
			if (ABS(lastTotal - loops) > 180) {
				total += 360 * SIGNUM(lastTotal);
				dir = current;
			} else if (loops != 0)
				total -= 360 * SIGNUM(lastTotal);
			else
				dir = current;
		}
		if (dir != current) total += 360 * SIGNUM(lastTotal);
		timelinesRotation[i] = total;
	}
	timelinesRotation[i + 1] = diff;
	bone->rotation = r1 + total * alpha;
}

void _spAnimationState_queueEvents(spAnimationState *self, spTrackEntry *entry, float animationTime) {
	spEvent **events;
	spEvent *event;
	_spAnimationState *internal = SUB_CAST(_spAnimationState, self);
	int i, n, complete;
	float animationStart = entry->animationStart, animationEnd = entry->animationEnd;
	float duration = animationEnd - animationStart;
	float trackLastWrapped = FMOD(entry->trackLast, duration);

	/* Queue events before complete. */
	events = internal->events;
	for (i = 0, n = internal->eventsCount; i < n; i++) {
		event = events[i];
		if (event->time < trackLastWrapped) break;
		if (event->time > animationEnd) continue; /* Discard events outside animation start/end. */
		_spEventQueue_event(internal->queue, entry, event);
	}

	/* Queue complete if completed a loop iteration or the animation. */
	if (entry->loop) {
		if (duration == 0)
			complete = -1;
		else {
			int cycles = (int) (entry->trackTime / duration);
			complete = cycles > 0 && cycles > (int) (entry->trackLast / duration);
		}
	} else {
		complete = (animationTime >= animationEnd && entry->animationLast < animationEnd);
	}
	if (complete) _spEventQueue_complete(internal->queue, entry);

	/* Queue events after complete. */
	for (; i < n; i++) {
		event = events[i];
		if (event->time < animationStart) continue; /* Discard events outside animation start/end. */
		_spEventQueue_event(internal->queue, entry, event);
	}
}

void spAnimationState_clearTracks(spAnimationState *self) {
	_spAnimationState *internal = SUB_CAST(_spAnimationState, self);
	int i, n, oldDrainDisabled;
	oldDrainDisabled = internal->queue->drainDisabled;
	internal->queue->drainDisabled = 1;
	for (i = 0, n = self->tracksCount; i < n; i++)
		spAnimationState_clearTrack(self, i);
	self->tracksCount = 0;
	internal->queue->drainDisabled = oldDrainDisabled;
	_spEventQueue_drain(internal->queue);
}

void spAnimationState_clearTrack(spAnimationState *self, int trackIndex) {
	spTrackEntry *current;
	spTrackEntry *entry;
	spTrackEntry *from;
	_spAnimationState *internal = SUB_CAST(_spAnimationState, self);

	if (trackIndex >= self->tracksCount) return;
	current = self->tracks[trackIndex];
	if (!current) return;

	_spEventQueue_end(internal->queue, current);

	spAnimationState_clearNext(self, current);

	entry = current;
	while (1) {
		from = entry->mixingFrom;
		if (!from) break;
		_spEventQueue_end(internal->queue, from);
		entry->mixingFrom = 0;
		entry->mixingTo = 0;
		entry = from;
	}

	self->tracks[current->trackIndex] = 0;
	_spEventQueue_drain(internal->queue);
}

void _spAnimationState_setCurrent(spAnimationState *self, int index, spTrackEntry *current, int /*boolean*/ interrupt) {
	_spAnimationState *internal = SUB_CAST(_spAnimationState, self);
	spTrackEntry *from = _spAnimationState_expandToIndex(self, index);
	self->tracks[index] = current;
	current->previous = NULL;

	if (from) {
		if (interrupt) _spEventQueue_interrupt(internal->queue, from);
		current->mixingFrom = from;
		from->mixingTo = current;
		current->mixTime = 0;

		/* Store the interrupted mix percentage. */
		if (from->mixingFrom != 0 && from->mixDuration > 0)
			current->interruptAlpha *= MIN(1, from->mixTime / from->mixDuration);

		from->timelinesRotationCount = 0;
	}

	_spEventQueue_start(internal->queue, current);
}

/** Set the current animation. Any queued animations are cleared. */
spTrackEntry *spAnimationState_setAnimationByName(spAnimationState *self, int trackIndex, const char *animationName,
												  int /*bool*/ loop) {
	spAnimation *animation = spSkeletonData_findAnimation(self->data->skeletonData, animationName);
	return spAnimationState_setAnimation(self, trackIndex, animation, loop);
}

spTrackEntry *
spAnimationState_setAnimation(spAnimationState *self, int trackIndex, spAnimation *animation, int /*bool*/ loop) {
	spTrackEntry *entry;
	_spAnimationState *internal = SUB_CAST(_spAnimationState, self);
	int interrupt = 1;
	spTrackEntry *current = _spAnimationState_expandToIndex(self, trackIndex);
	if (current) {
		if (current->nextTrackLast == -1) {
			/* Don't mix from an entry that was never applied. */
			self->tracks[trackIndex] = current->mixingFrom;
			_spEventQueue_interrupt(internal->queue, current);
			_spEventQueue_end(internal->queue, current);
			spAnimationState_clearNext(self, current);
			current = current->mixingFrom;
			interrupt = 0;
		} else
			spAnimationState_clearNext(self, current);
	}
	entry = _spAnimationState_trackEntry(self, trackIndex, animation, loop, current);
	_spAnimationState_setCurrent(self, trackIndex, entry, interrupt);
	_spEventQueue_drain(internal->queue);
	return entry;
}

/** Adds an animation to be played delay seconds after the current or last queued animation, taking into account any mix
 * duration. */
spTrackEntry *spAnimationState_addAnimationByName(spAnimationState *self, int trackIndex, const char *animationName,
												  int /*bool*/ loop, float delay) {
	spAnimation *animation = spSkeletonData_findAnimation(self->data->skeletonData, animationName);
	return spAnimationState_addAnimation(self, trackIndex, animation, loop, delay);
}

spTrackEntry *
spAnimationState_addAnimation(spAnimationState *self, int trackIndex, spAnimation *animation, int /*bool*/ loop,
							  float delay) {
	spTrackEntry *entry;
	_spAnimationState *internal = SUB_CAST(_spAnimationState, self);
	spTrackEntry *last = _spAnimationState_expandToIndex(self, trackIndex);
	if (last) {
		while (last->next)
			last = last->next;
	}

	entry = _spAnimationState_trackEntry(self, trackIndex, animation, loop, last);

	if (!last) {
		_spAnimationState_setCurrent(self, trackIndex, entry, 1);
		_spEventQueue_drain(internal->queue);
	} else {
		last->next = entry;
		entry->previous = last;
		if (delay <= 0) delay += spTrackEntry_getTrackComplete(last) - entry->mixDuration;
	}

	entry->delay = delay;
	return entry;
}

spTrackEntry *spAnimationState_setEmptyAnimation(spAnimationState *self, int trackIndex, float mixDuration) {
	spTrackEntry *entry = spAnimationState_setAnimation(self, trackIndex, SP_EMPTY_ANIMATION, 0);
	entry->mixDuration = mixDuration;
	entry->trackEnd = mixDuration;
	return entry;
}

spTrackEntry *
spAnimationState_addEmptyAnimation(spAnimationState *self, int trackIndex, float mixDuration, float delay) {
	spTrackEntry *entry = spAnimationState_addAnimation(self, trackIndex, SP_EMPTY_ANIMATION, 0, delay);
	if (delay <= 0) entry->delay += entry->mixDuration - mixDuration;
	entry->mixDuration = mixDuration;
	entry->trackEnd = mixDuration;
	return entry;
}

void spAnimationState_setEmptyAnimations(spAnimationState *self, float mixDuration) {
	int i, n, oldDrainDisabled;
	spTrackEntry *current;
	_spAnimationState *internal = SUB_CAST(_spAnimationState, self);
	oldDrainDisabled = internal->queue->drainDisabled;
	internal->queue->drainDisabled = 1;
	for (i = 0, n = self->tracksCount; i < n; i++) {
		current = self->tracks[i];
		if (current) spAnimationState_setEmptyAnimation(self, current->trackIndex, mixDuration);
	}
	internal->queue->drainDisabled = oldDrainDisabled;
	_spEventQueue_drain(internal->queue);
}

spTrackEntry *_spAnimationState_expandToIndex(spAnimationState *self, int index) {
	spTrackEntry **newTracks;
	if (index < self->tracksCount) return self->tracks[index];
	newTracks = CALLOC(spTrackEntry *, index + 1);
	memcpy(newTracks, self->tracks, self->tracksCount * sizeof(spTrackEntry *));
	FREE(self->tracks);
	self->tracks = newTracks;
	self->tracksCount = index + 1;
	return 0;
}

spTrackEntry *
_spAnimationState_trackEntry(spAnimationState *self, int trackIndex, spAnimation *animation, int /*boolean*/ loop,
							 spTrackEntry *last) {
	spTrackEntry *entry = NEW(spTrackEntry);
	entry->trackIndex = trackIndex;
	entry->animation = animation;
	entry->loop = loop;
	entry->holdPrevious = 0;
	entry->reverse = 0;
	entry->shortestRotation = 0;
	entry->previous = 0;
	entry->next = 0;

	entry->eventThreshold = 0;
	entry->mixAttachmentThreshold = 0;
	entry->alphaAttachmentThreshold = 0;
	entry->mixDrawOrderThreshold = 0;

	entry->animationStart = 0;
	entry->animationEnd = animation->duration;
	entry->animationLast = -1;
	entry->nextAnimationLast = -1;

	entry->delay = 0;
	entry->trackTime = 0;
	entry->trackLast = -1;
	entry->nextTrackLast = -1;
	entry->trackEnd = (float) INT_MAX;
	entry->timeScale = 1;

	entry->alpha = 1;
	entry->mixTime = 0;
	entry->mixDuration = !last ? 0 : spAnimationStateData_getMix(self->data, last->animation, animation);
	entry->interruptAlpha = 1;
	entry->totalAlpha = 0;
	entry->mixBlend = SP_MIX_BLEND_REPLACE;

	entry->timelineMode = spIntArray_create(16);
	entry->timelineHoldMix = spTrackEntryArray_create(16);

	return entry;
}

void spAnimationState_clearNext(spAnimationState *self, spTrackEntry *entry) {
	_spAnimationState *internal = SUB_CAST(_spAnimationState, self);
	spTrackEntry *next = entry->next;
	while (next) {
		_spEventQueue_dispose(internal->queue, next);
		next = next->next;
	}
	entry->next = 0;
}

void _spAnimationState_animationsChanged(spAnimationState *self) {
	_spAnimationState *internal = SUB_CAST(_spAnimationState, self);
	int i, n;
	spTrackEntry *entry;
	internal->animationsChanged = 0;

	internal->propertyIDsCount = 0;
	i = 0;
	n = self->tracksCount;

	for (; i < n; i++) {
		entry = self->tracks[i];
		if (!entry) continue;
		while (entry->mixingFrom != 0)
			entry = entry->mixingFrom;
		do {
			if (entry->mixingTo == 0 || entry->mixBlend != SP_MIX_BLEND_ADD) _spTrackEntry_computeHold(entry, self);
			entry = entry->mixingTo;
		} while (entry != 0);
	}
}

float *_spAnimationState_resizeTimelinesRotation(spTrackEntry *entry, int newSize) {
	if (entry->timelinesRotationCount != newSize) {
		float *newTimelinesRotation = CALLOC(float, newSize);
		FREE(entry->timelinesRotation);
		entry->timelinesRotation = newTimelinesRotation;
		entry->timelinesRotationCount = newSize;
	}
	return entry->timelinesRotation;
}

void _spAnimationState_ensureCapacityPropertyIDs(spAnimationState *self, int capacity) {
	_spAnimationState *internal = SUB_CAST(_spAnimationState, self);
	if (internal->propertyIDsCapacity < capacity) {
		spPropertyId *newPropertyIDs = CALLOC(spPropertyId, capacity << 1);
		memcpy(newPropertyIDs, internal->propertyIDs, sizeof(spPropertyId) * internal->propertyIDsCount);
		FREE(internal->propertyIDs);
		internal->propertyIDs = newPropertyIDs;
		internal->propertyIDsCapacity = capacity << 1;
	}
}

int _spAnimationState_addPropertyID(spAnimationState *self, spPropertyId id) {
	int i, n;
	_spAnimationState *internal = SUB_CAST(_spAnimationState, self);

	for (i = 0, n = internal->propertyIDsCount; i < n; i++) {
		if (internal->propertyIDs[i] == id) return 0;
	}

	_spAnimationState_ensureCapacityPropertyIDs(self, internal->propertyIDsCount + 1);
	internal->propertyIDs[internal->propertyIDsCount] = id;
	internal->propertyIDsCount++;
	return 1;
}

int _spAnimationState_addPropertyIDs(spAnimationState *self, spPropertyId *ids, int numIds) {
	int i, n;
	_spAnimationState *internal = SUB_CAST(_spAnimationState, self);
	int oldSize = internal->propertyIDsCount;

	for (i = 0, n = numIds; i < n; i++) {
		_spAnimationState_addPropertyID(self, ids[i]);
	}

	return internal->propertyIDsCount != oldSize;
}

spTrackEntry *spAnimationState_getCurrent(spAnimationState *self, int trackIndex) {
	if (trackIndex >= self->tracksCount) return 0;
	return self->tracks[trackIndex];
}

void spAnimationState_clearListenerNotifications(spAnimationState *self) {
	_spAnimationState *internal = SUB_CAST(_spAnimationState, self);
	_spEventQueue_clear(internal->queue);
}

float spTrackEntry_getAnimationTime(spTrackEntry *entry) {
	if (entry->loop) {
		float duration = entry->animationEnd - entry->animationStart;
		if (duration == 0) return entry->animationStart;
		return FMOD(entry->trackTime, duration) + entry->animationStart;
	}
	return MIN(entry->trackTime + entry->animationStart, entry->animationEnd);
}

void spTrackEntry_resetRotationDirections(spTrackEntry *entry) {
	FREE(entry->timelinesRotation);
	entry->timelinesRotation = NULL;
	entry->timelinesRotationCount = 0;
}

float spTrackEntry_getTrackComplete(spTrackEntry *entry) {
	float duration = entry->animationEnd - entry->animationStart;
	if (duration != 0) {
		if (entry->loop) return duration * (1 + (int) (entry->trackTime / duration)); /* Completion of next loop. */
		if (entry->trackTime < duration) return duration;                             /* Before duration. */
	}
	return entry->trackTime; /* Next update. */
}

void spTrackEntry_setMixDuration(spTrackEntry *entry, float mixDuration, float delay) {
	entry->mixDuration = mixDuration;
	if (entry->previous && delay <= 0) delay += spTrackEntry_getTrackComplete(entry) - mixDuration;
	entry->delay = delay;
}

int spTrackEntry_wasApplied(spTrackEntry *entry) {
	return entry->nextTrackLast != -1;
}

int spTrackEntry_isNextReady(spTrackEntry *entry) {
	return entry->next != NULL && entry->nextTrackLast - entry->next->delay >= 0;
}

void _spTrackEntry_computeHold(spTrackEntry *entry, spAnimationState *state) {
	spTrackEntry *to;
	spTimeline **timelines;
	int timelinesCount;
	int *timelineMode;
	spTrackEntry **timelineHoldMix;
	spTrackEntry *next;
	int i;

	to = entry->mixingTo;
	timelines = entry->animation->timelines->items;
	timelinesCount = entry->animation->timelines->size;
	timelineMode = spIntArray_setSize(entry->timelineMode, timelinesCount)->items;
	spTrackEntryArray_clear(entry->timelineHoldMix);
	timelineHoldMix = spTrackEntryArray_setSize(entry->timelineHoldMix, timelinesCount)->items;

	if (to != 0 && to->holdPrevious) {
		for (i = 0; i < timelinesCount; i++) {
			spPropertyId *ids = timelines[i]->propertyIds;
			int numIds = timelines[i]->propertyIdsCount;
			timelineMode[i] = _spAnimationState_addPropertyIDs(state, ids, numIds) ? HOLD_FIRST : HOLD_SUBSEQUENT;
		}
		return;
	}

	i = 0;
continue_outer:
	for (; i < timelinesCount; i++) {
		spTimeline *timeline = timelines[i];
		spPropertyId *ids = timeline->propertyIds;
		int numIds = timeline->propertyIdsCount;
		if (!_spAnimationState_addPropertyIDs(state, ids, numIds))
			timelineMode[i] = SUBSEQUENT;
		else if (to == 0 || timeline->type == SP_TIMELINE_ATTACHMENT ||
				 timeline->type == SP_TIMELINE_DRAWORDER ||
				 timeline->type == SP_TIMELINE_EVENT ||
				 !spAnimation_hasTimeline(to->animation, ids, numIds)) {
			timelineMode[i] = FIRST;
		} else {
			for (next = to->mixingTo; next != 0; next = next->mixingTo) {
				if (spAnimation_hasTimeline(next->animation, ids, numIds)) continue;
				if (next->mixDuration > 0) {
					timelineMode[i] = HOLD_MIX;
					timelineHoldMix[i] = next;
					i++;
					goto continue_outer;
				}
				break;
			}
			timelineMode[i] = HOLD_FIRST;
		}
	}
}
