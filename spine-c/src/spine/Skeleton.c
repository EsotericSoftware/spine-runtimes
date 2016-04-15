/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include <spine/Skeleton.h>
#include <string.h>
#include <spine/extension.h>

typedef enum {
	SP_UPDATE_BONE, SP_UPDATE_IK_CONSTRAINT, SP_UPDATE_TRANSFORM_CONSTRAINT
} _spUpdateType;

typedef struct {
	_spUpdateType type;
	void* object;
} _spUpdate;

typedef struct {
	spSkeleton super;

	int updateCacheCount;
	_spUpdate* updateCache;
} _spSkeleton;

spSkeleton* spSkeleton_create (spSkeletonData* data) {
	int i, ii;

	_spSkeleton* internal = NEW(_spSkeleton);
	spSkeleton* self = SUPER(internal);
	CONST_CAST(spSkeletonData*, self->data) = data;

	self->bonesCount = self->data->bonesCount;
	self->bones = MALLOC(spBone*, self->bonesCount);

	for (i = 0; i < self->bonesCount; ++i) {
		spBoneData* boneData = self->data->bones[i];
		spBone* parent = 0;
		if (boneData->parent) {
			/* Find parent bone. */
			for (ii = 0; ii < self->bonesCount; ++ii) {
				if (data->bones[ii] == boneData->parent) {
					parent = self->bones[ii];
					break;
				}
			}
		}
		self->bones[i] = spBone_create(boneData, self, parent);
	}
	CONST_CAST(spBone*, self->root) = self->bones[0];

	self->slotsCount = data->slotsCount;
	self->slots = MALLOC(spSlot*, self->slotsCount);
	for (i = 0; i < self->slotsCount; ++i) {
		spSlotData *slotData = data->slots[i];

		/* Find bone for the slotData's boneData. */
		spBone* bone = 0;
		for (ii = 0; ii < self->bonesCount; ++ii) {
			if (data->bones[ii] == slotData->boneData) {
				bone = self->bones[ii];
				break;
			}
		}
		self->slots[i] = spSlot_create(slotData, bone);
	}

	self->drawOrder = MALLOC(spSlot*, self->slotsCount);
	memcpy(self->drawOrder, self->slots, sizeof(spSlot*) * self->slotsCount);

	self->r = 1;
	self->g = 1;
	self->b = 1;
	self->a = 1;

	self->ikConstraintsCount = data->ikConstraintsCount;
	self->ikConstraints = MALLOC(spIkConstraint*, self->ikConstraintsCount);
	for (i = 0; i < self->data->ikConstraintsCount; ++i)
		self->ikConstraints[i] = spIkConstraint_create(self->data->ikConstraints[i], self);

	self->transformConstraintsCount = data->transformConstraintsCount;
	self->transformConstraints = MALLOC(spTransformConstraint*, self->transformConstraintsCount);
	for (i = 0; i < self->data->transformConstraintsCount; ++i)
		self->transformConstraints[i] = spTransformConstraint_create(self->data->transformConstraints[i], self);

	spSkeleton_updateCache(self);

	return self;
}

void spSkeleton_dispose (spSkeleton* self) {
	int i;
	_spSkeleton* internal = SUB_CAST(_spSkeleton, self);

	FREE(internal->updateCache);

	for (i = 0; i < self->bonesCount; ++i)
		spBone_dispose(self->bones[i]);
	FREE(self->bones);

	for (i = 0; i < self->slotsCount; ++i)
		spSlot_dispose(self->slots[i]);
	FREE(self->slots);

	for (i = 0; i < self->ikConstraintsCount; ++i)
		spIkConstraint_dispose(self->ikConstraints[i]);
	FREE(self->ikConstraints);

	for (i = 0; i < self->transformConstraintsCount; ++i)
		spTransformConstraint_dispose(self->transformConstraints[i]);
	FREE(self->transformConstraints);

	FREE(self->drawOrder);
	FREE(self);
}

void spSkeleton_updateCache (const spSkeleton* self) {
	int i, ii;
	_spUpdate* update;
	_spSkeleton* internal = SUB_CAST(_spSkeleton, self);
	int capacity = self->bonesCount + self->transformConstraintsCount + self->ikConstraintsCount;

	FREE(internal->updateCache);
	internal->updateCache = MALLOC(_spUpdate, capacity);
	internal->updateCacheCount = 0;

	for (i = 0; i < self->bonesCount; ++i) {
		spBone* bone = self->bones[i];
		update = internal->updateCache + internal->updateCacheCount++;
		update->type = SP_UPDATE_BONE;
		update->object = bone;
		for (ii = 0; ii < self->ikConstraintsCount; ++ii) {
			spIkConstraint* ikConstraint = self->ikConstraints[ii];
			if (bone == ikConstraint->bones[ikConstraint->bonesCount - 1]) {
				update = internal->updateCache + internal->updateCacheCount++;
				update->type = SP_UPDATE_IK_CONSTRAINT;
				update->object = ikConstraint;
				break;
			}
		}
	}

	for (i = 0; i < self->transformConstraintsCount; ++i) {
		spTransformConstraint* transformConstraint = self->transformConstraints[i];
		for (ii = internal->updateCacheCount - 1; ii >= 0; --ii) {
			void* object = internal->updateCache[ii].object;
			if (object == transformConstraint->bone || object == transformConstraint->target) {
				int insertIndex = ii + 1;
				update = internal->updateCache + insertIndex;
				memmove(update + 1, update, (internal->updateCacheCount - insertIndex) * sizeof(_spUpdate));
				update->type = SP_UPDATE_TRANSFORM_CONSTRAINT;
				update->object = transformConstraint;
				internal->updateCacheCount++;
				break;
			}
		}
	}
}

void spSkeleton_updateWorldTransform (const spSkeleton* self) {
	int i;
	_spSkeleton* internal = SUB_CAST(_spSkeleton, self);

	for (i = 0; i < internal->updateCacheCount; ++i) {
		_spUpdate* update = internal->updateCache + i;
		switch (update->type) {
		case SP_UPDATE_BONE:
			spBone_updateWorldTransform((spBone*)update->object);
			break;
		case SP_UPDATE_IK_CONSTRAINT:
			spIkConstraint_apply((spIkConstraint*)update->object);
			break;
		case SP_UPDATE_TRANSFORM_CONSTRAINT:
			spTransformConstraint_apply((spTransformConstraint*)update->object);
			break;
		}
	}
}

void spSkeleton_setToSetupPose (const spSkeleton* self) {
	spSkeleton_setBonesToSetupPose(self);
	spSkeleton_setSlotsToSetupPose(self);
}

void spSkeleton_setBonesToSetupPose (const spSkeleton* self) {
	int i;
	for (i = 0; i < self->bonesCount; ++i)
		spBone_setToSetupPose(self->bones[i]);

	for (i = 0; i < self->ikConstraintsCount; ++i) {
		spIkConstraint* ikConstraint = self->ikConstraints[i];
		ikConstraint->bendDirection = ikConstraint->data->bendDirection;
		ikConstraint->mix = ikConstraint->data->mix;
	}

	for (i = 0; i < self->transformConstraintsCount; ++i) {
		spTransformConstraint* transformConstraint = self->transformConstraints[i];
		transformConstraint->translateMix = transformConstraint->data->translateMix;
		transformConstraint->x = transformConstraint->data->x;
		transformConstraint->y = transformConstraint->data->y;
	}
}

void spSkeleton_setSlotsToSetupPose (const spSkeleton* self) {
	int i;
	memcpy(self->drawOrder, self->slots, self->slotsCount * sizeof(spSlot*));
	for (i = 0; i < self->slotsCount; ++i)
		spSlot_setToSetupPose(self->slots[i]);
}

spBone* spSkeleton_findBone (const spSkeleton* self, const char* boneName) {
	int i;
	for (i = 0; i < self->bonesCount; ++i)
		if (strcmp(self->data->bones[i]->name, boneName) == 0) return self->bones[i];
	return 0;
}

int spSkeleton_findBoneIndex (const spSkeleton* self, const char* boneName) {
	int i;
	for (i = 0; i < self->bonesCount; ++i)
		if (strcmp(self->data->bones[i]->name, boneName) == 0) return i;
	return -1;
}

spSlot* spSkeleton_findSlot (const spSkeleton* self, const char* slotName) {
	int i;
	for (i = 0; i < self->slotsCount; ++i)
		if (strcmp(self->data->slots[i]->name, slotName) == 0) return self->slots[i];
	return 0;
}

int spSkeleton_findSlotIndex (const spSkeleton* self, const char* slotName) {
	int i;
	for (i = 0; i < self->slotsCount; ++i)
		if (strcmp(self->data->slots[i]->name, slotName) == 0) return i;
	return -1;
}

int spSkeleton_setSkinByName (spSkeleton* self, const char* skinName) {
	spSkin *skin;
	if (!skinName) {
		spSkeleton_setSkin(self, 0);
		return 1;
	}
	skin = spSkeletonData_findSkin(self->data, skinName);
	if (!skin) return 0;
	spSkeleton_setSkin(self, skin);
	return 1;
}

void spSkeleton_setSkin (spSkeleton* self, spSkin* newSkin) {
	if (newSkin) {
		if (self->skin)
			spSkin_attachAll(newSkin, self, self->skin);
		else {
			/* No previous skin, attach setup pose attachments. */
			int i;
			for (i = 0; i < self->slotsCount; ++i) {
				spSlot* slot = self->slots[i];
				if (slot->data->attachmentName) {
					spAttachment* attachment = spSkin_getAttachment(newSkin, i, slot->data->attachmentName);
					if (attachment) spSlot_setAttachment(slot, attachment);
				}
			}
		}
	}
	CONST_CAST(spSkin*, self->skin) = newSkin;
}

spAttachment* spSkeleton_getAttachmentForSlotName (const spSkeleton* self, const char* slotName, const char* attachmentName) {
	int slotIndex = spSkeletonData_findSlotIndex(self->data, slotName);
	return spSkeleton_getAttachmentForSlotIndex(self, slotIndex, attachmentName);
}

spAttachment* spSkeleton_getAttachmentForSlotIndex (const spSkeleton* self, int slotIndex, const char* attachmentName) {
	if (slotIndex == -1) return 0;
	if (self->skin) {
		spAttachment *attachment = spSkin_getAttachment(self->skin, slotIndex, attachmentName);
		if (attachment) return attachment;
	}
	if (self->data->defaultSkin) {
		spAttachment *attachment = spSkin_getAttachment(self->data->defaultSkin, slotIndex, attachmentName);
		if (attachment) return attachment;
	}
	return 0;
}

int spSkeleton_setAttachment (spSkeleton* self, const char* slotName, const char* attachmentName) {
	int i;
	for (i = 0; i < self->slotsCount; ++i) {
		spSlot *slot = self->slots[i];
		if (strcmp(slot->data->name, slotName) == 0) {
			if (!attachmentName)
				spSlot_setAttachment(slot, 0);
			else {
				spAttachment* attachment = spSkeleton_getAttachmentForSlotIndex(self, i, attachmentName);
				if (!attachment) return 0;
				spSlot_setAttachment(slot, attachment);
			}
			return 1;
		}
	}
	return 0;
}

spIkConstraint* spSkeleton_findIkConstraint (const spSkeleton* self, const char* constraintName) {
	int i;
	for (i = 0; i < self->ikConstraintsCount; ++i)
		if (strcmp(self->ikConstraints[i]->data->name, constraintName) == 0) return self->ikConstraints[i];
	return 0;
}

spTransformConstraint* spSkeleton_findTransformConstraint (const spSkeleton* self, const char* constraintName) {
	int i;
	for (i = 0; i < self->transformConstraintsCount; ++i)
		if (strcmp(self->transformConstraints[i]->data->name, constraintName) == 0) return self->transformConstraints[i];
	return 0;
}

void spSkeleton_update (spSkeleton* self, float deltaTime) {
	self->time += deltaTime;
}
