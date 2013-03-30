#include <spine/Skeleton.h>
#include <spine/util.h>

void _Skeleton_init (Skeleton* self, SkeletonData* data) {
	CAST(SkeletonData*, self->data) = data;

	self->boneCount = self->data->boneCount;
	self->bones = MALLOC(Bone*, self->boneCount)
	int i, ii;
	for (i = 0; i < self->boneCount; ++i) {
		BoneData* boneData = self->data->bones[i];
		Bone* parent = 0;
		if (boneData->parent) {
			/* Find parent bone. */
			for (ii = 0; ii < self->boneCount; ++ii) {
				if (data->bones[ii] == boneData->parent) {
					parent = self->bones[ii];
					break;
				}
			}
		}
		self->bones[i] = Bone_create(boneData, parent);
	}

	self->slotCount = data->slotCount;
	self->slots = MALLOC(Slot*, self->slotCount)
	for (i = 0; i < self->slotCount; ++i) {
		SlotData *slotData = data->slots[i];

		/* Find bone for the slotData's boneData. */
		Bone *bone;
		for (ii = 0; ii < self->boneCount; ++ii) {
			if (data->bones[ii] == slotData->boneData) {
				bone = self->bones[ii];
				break;
			}
		}

		self->slots[i] = Slot_create(slotData, self, bone);
	}

	self->drawOrder = MALLOC(Slot*, self->slotCount)
	memcpy(self->drawOrder, self->slots, sizeof(Slot*) * self->slotCount);
}

void _Skeleton_deinit (Skeleton* self) {
	int i;
	for (i = 0; i < self->boneCount; ++i)
		Bone_dispose(self->bones[i]);
	FREE(self->bones)

	for (i = 0; i < self->slotCount; ++i)
		Slot_dispose(self->slots[i]);
	FREE(self->slots)

	FREE(self->drawOrder)
}

void Skeleton_dispose (Skeleton* self) {
	self->_dispose(self);
}

void Skeleton_updateWorldTransform (const Skeleton* self) {
	int i;
	for (i = 0; i < self->boneCount; ++i)
		Bone_updateWorldTransform(self->bones[i], self->flipX, self->flipY);
}

void Skeleton_setToBindPose (const Skeleton* self) {
	Skeleton_setBonesToBindPose(self);
	Skeleton_setSlotsToBindPose(self);
}

void Skeleton_setBonesToBindPose (const Skeleton* self) {
	int i;
	for (i = 0; i < self->boneCount; ++i)
		Bone_setToBindPose(self->bones[i]);
}

void Skeleton_setSlotsToBindPose (const Skeleton* self) {
	int i;
	for (i = 0; i < self->slotCount; ++i)
		Slot_setToBindPose(self->slots[i]);
}

Bone* Skeleton_getRootBone (const Skeleton* self) {
	if (self->boneCount == 0) return 0;
	return self->bones[0];
}

Bone* Skeleton_findBone (const Skeleton* self, const char* boneName) {
	int i;
	for (i = 0; i < self->boneCount; ++i)
		if (self->data->bones[i]->name == boneName) return self->bones[i];
	return 0;
}

int Skeleton_findBoneIndex (const Skeleton* self, const char* boneName) {
	int i;
	for (i = 0; i < self->boneCount; ++i)
		if (self->data->bones[i]->name == boneName) return i;
	return -1;
}

Slot* Skeleton_findSlot (const Skeleton* self, const char* slotName) {
	int i;
	for (i = 0; i < self->slotCount; ++i)
		if (self->data->slots[i]->name == slotName) return self->slots[i];
	return 0;
}

int Skeleton_findSlotIndex (const Skeleton* self, const char* slotName) {
	int i;
	for (i = 0; i < self->slotCount; ++i)
		if (self->data->slots[i]->name == slotName) return i;
	return -1;
}

int Skeleton_setSkinByName (Skeleton* self, const char* skinName) {
	Skin *skin = SkeletonData_findSkin(self->data, skinName);
	if (!skin) return 0;
	Skeleton_setSkin(self, skin);
	return 1;
}

void Skeleton_setSkin (Skeleton* self, Skin* newSkin) {
	if (self->skin && newSkin) {
		/* Attach each attachment in the new skin if the corresponding attachment in the old skin is currently attached. */
		const SkinEntry *entry = self->skin->entries;
		while (entry) {
			Slot *slot = self->slots[entry->slotIndex];
			if (slot->attachment == entry->attachment) {
				Attachment *attachment = Skin_getAttachment(newSkin, entry->slotIndex, entry->name);
				if (attachment) Slot_setAttachment(slot, attachment);
			}
			entry = entry->next;
		}
	}
	CAST(Skin*, self->skin) = newSkin;
}

Attachment* Skeleton_getAttachmentForSlotName (const Skeleton* self, const char* slotName, const char* attachmentName) {
	int slotIndex = SkeletonData_findSlotIndex(self->data, slotName);
	return Skeleton_getAttachmentForSlotIndex(self, slotIndex, attachmentName);
}

Attachment* Skeleton_getAttachmentForSlotIndex (const Skeleton* self, int slotIndex, const char* attachmentName) {
	if (slotIndex == -1) return 0;
	if (self->skin) {
		Attachment *attachment = Skin_getAttachment(self->skin, slotIndex, attachmentName);
		if (attachment) return attachment;
	}
	if (self->data->defaultSkin) {
		Attachment *attachment = Skin_getAttachment(self->data->defaultSkin, slotIndex, attachmentName);
		if (attachment) return attachment;
	}
	return 0;
}

int Skeleton_setAttachment (Skeleton* self, const char* slotName, const char* attachmentName) {
	int i;
	for (i = 0; i < self->slotCount; ++i) {
		Slot *slot = self->slots[i];
		if (slot->data->name == slotName) {
			Attachment* attachment = Skeleton_getAttachmentForSlotIndex(self, i, attachmentName);
			if (!attachment) return 0;
			Slot_setAttachment(slot, attachment);
			return 1;
		}
	}
	return 0;
}

void Skeleton_update (Skeleton* self, float deltaTime) {
	self->time += deltaTime;
}
