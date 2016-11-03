/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include <spine/AnimationState.h>
#include <spine/extension.h>
#include <string.h>

_spEventQueue* _spEventQueue_create (spAnimationState* state) {
	_spEventQueue *self = MALLOC(_spEventQueue, 1);
	self->state = state;
	self->objectsCount = 0;
	self->objectsCapacity = 16;
	self->objects = MALLOC(_spEventQueueItem, self->objectsCapacity * sizeof(_spEventQueueItem));
	self->drainDisabled = 0;
	return self;
}

void _spEventQueue_free (_spEventQueue* self) {
	if (!self) return;
	if (self->objects) FREE(self->objects);
}

void _spEventQueue_ensureCapacity (_spEventQueue* self, int newElements) {
	if (self->objectsCount + newElements > self->objectsCapacity) {
		_spEventQueueItem* newObjects;
		self->objectsCapacity <<= 1;
		newObjects = MALLOC(_spEventQueueItem, self->objectsCapacity);
		memcpy(newObjects, self->objects, self->objectsCount * sizeof(_spEventQueueItem));
		FREE(self->objects);
		self->objects = newObjects;
	}
}

void _spEventQueue_addType (_spEventQueue* self, spEventType type) {
	_spEventQueue_ensureCapacity(self, 1);
	self->objects[self->objectsCount++].type = type;
}

void _spEventQueue_addEntry (_spEventQueue* self, spTrackEntry* entry) {
	_spEventQueue_ensureCapacity(self, 1);
	self->objects[self->objectsCount++].entry = entry;
}

void _spEventQueue_addEvent (_spEventQueue* self, spEvent* event) {
	_spEventQueue_ensureCapacity(self, 1);
	self->objects[self->objectsCount++].event = event;
}

void _spEventQueue_start (_spEventQueue* self, spTrackEntry* entry) {
	_spAnimationState* internalState = (_spAnimationState*)self->state;
	_spEventQueue_addType(self, SP_ANIMATION_START);
	_spEventQueue_addEntry(self, entry);
	internalState->animationsChanged = 1;
}

void _spEventQueue_interrupt (_spEventQueue* self, spTrackEntry* entry) {
	_spEventQueue_addType(self, SP_ANIMATION_INTERRUPT);
	_spEventQueue_addEntry(self, entry);
}

void _spEventQueue_end (_spEventQueue* self, spTrackEntry* entry) {
	_spAnimationState* internalState = (_spAnimationState*)self->state;
	_spEventQueue_addType(self, SP_ANIMATION_END);
	_spEventQueue_addEntry(self, entry);
	internalState->animationsChanged = 1;
}

void _spEventQueue_dispose (_spEventQueue* self, spTrackEntry* entry) {
	_spEventQueue_addType(self, SP_ANIMATION_DISPOSE);
	_spEventQueue_addEntry(self, entry);
}

void _spEventQueue_complete (_spEventQueue* self, spTrackEntry* entry) {
	_spEventQueue_addType(self, SP_ANIMATION_COMPLETE);
	_spEventQueue_addEntry(self, entry);
}

void _spEventQueue_event (_spEventQueue* self, spTrackEntry* entry, spEvent* event) {
	_spEventQueue_addType(self, SP_ANIMATION_EVENT);
	_spEventQueue_addEntry(self, entry);
	_spEventQueue_addEvent(self, event);
}

void _spEventQueue_clear (_spEventQueue* self) {
	self->objectsCount = 0;
}

void _spEventQueue_drain (_spEventQueue* self) {
	int i;
	if (self->drainDisabled) return;
	self->drainDisabled = 1;
	for (i = 0; i < self->objectsCount; i += 2) {
		spEventType type = self->objects[i].type;
		spTrackEntry* entry = self->objects[i+1].entry;
		if (type != SP_ANIMATION_EVENT) {
			if (entry->listener) entry->listener(self->state, type, entry, 0);
			if (self->state->listener) self->state->listener(self->state, type, entry, 0);
		} else {
			spEvent* event = self->objects[i+2].event;
			if (entry->listener) entry->listener(self->state, type, entry, event);
			if (self->state->listener) self->state->listener(self->state, type, entry, event);
			i++;
		}
	}

	_spEventQueue_clear(self);
	self->drainDisabled = 0;
}
