#include <spine/Skeleton.h>
#include <spine/util.h>

void _Skeleton_init (Skeleton* this, SkeletonData* data) {
	CAST(SkeletonData*, this->data) = data;

	this->boneCount = this->data->boneCount;
	this->bones = malloc(sizeof(Bone*) * this->boneCount);
	int i, ii;
	for (i = 0; i < this->boneCount; ++i) {
		BoneData* boneData = this->data->bones[i];
		Bone* parent = 0;
		if (boneData->parent) {
			/* Find parent bone. */
			for (ii = 0; ii < this->boneCount; ++ii) {
				if (data->bones[ii] == boneData->parent) {
					parent = this->bones[ii];
					break;
				}
			}
		}
		this->bones[i] = Bone_create(boneData, parent);
	}

	this->slotCount = data->slotCount;
	this->slots = malloc(sizeof(Slot*) * this->slotCount);
	for (i = 0; i < this->slotCount; ++i) {
		SlotData *slotData = data->slots[i];

		/* Find bone for the slotData's boneData. */
		Bone *bone;
		for (ii = 0; ii < this->boneCount; ++ii) {
			if (data->bones[ii] == slotData->boneData) {
				bone = this->bones[ii];
				break;
			}
		}

		this->slots[i] = Slot_create(slotData, this, bone);
	}

	this->drawOrder = malloc(sizeof(Slot*) * this->slotCount);
	memcpy(this->drawOrder, this->slots, sizeof(Slot*) * this->slotCount);
}

void Skeleton_dispose (Skeleton* this) {
	if (this->_dispose) this->_dispose(this);

	int i;
	for (i = 0; i < this->boneCount; ++i)
		Bone_dispose(this->bones[i]);
	FREE(this->bones)

	for (i = 0; i < this->slotCount; ++i)
		Slot_dispose(this->slots[i]);
	FREE(this->slots)

	FREE(this->drawOrder)

	FREE(this)
}

void Skeleton_updateWorldTransform (const Skeleton* this) {
	int i;
	for (i = 0; i < this->boneCount; ++i)
		Bone_updateWorldTransform(this->bones[i], this->flipX, this->flipY);
}

void Skeleton_setToBindPose (const Skeleton* this) {
	Skeleton_setBonesToBindPose(this);
	Skeleton_setSlotsToBindPose(this);
}

void Skeleton_setBonesToBindPose (const Skeleton* this) {
	int i;
	for (i = 0; i < this->boneCount; ++i)
		Bone_setToBindPose(this->bones[i]);
}

void Skeleton_setSlotsToBindPose (const Skeleton* this) {
	int i;
	for (i = 0; i < this->slotCount; ++i)
		Slot_setToBindPose(this->slots[i]);
}

Bone* Skeleton_getRootBone (const Skeleton* this) {
	if (this->boneCount == 0) return 0;
	return this->bones[0];
}

Bone* Skeleton_findBone (const Skeleton* this, const char* boneName) {
	int i;
	for (i = 0; i < this->boneCount; ++i)
		if (this->data->bones[i]->name == boneName) return this->bones[i];
	return 0;
}

int Skeleton_findBoneIndex (const Skeleton* this, const char* boneName) {
	int i;
	for (i = 0; i < this->boneCount; ++i)
		if (this->data->bones[i]->name == boneName) return i;
	return -1;
}

Slot* Skeleton_findSlot (const Skeleton* this, const char* slotName) {
	int i;
	for (i = 0; i < this->slotCount; ++i)
		if (this->data->slots[i]->name == slotName) return this->slots[i];
	return 0;
}

int Skeleton_findSlotIndex (const Skeleton* this, const char* slotName) {
	int i;
	for (i = 0; i < this->slotCount; ++i)
		if (this->data->slots[i]->name == slotName) return i;
	return -1;
}

int Skeleton_setSkinByName (Skeleton* this, const char* skinName) {
	Skin *skin = SkeletonData_findSkin(this->data, skinName);
	if (!skin) return 0;
	Skeleton_setSkin(this, skin);
	return 1;
}

void Skeleton_setSkin (Skeleton* this, Skin* newSkin) {
	if (this->skin && newSkin) {
		/* Attach each attachment in the new skin if the corresponding attachment in the old skin is currently attached. */
		const SkinEntry *entry = this->skin->entries;
		while (entry) {
			Slot *slot = this->slots[entry->slotIndex];
			if (slot->attachment == entry->attachment) {
				Attachment *attachment = Skin_getAttachment(newSkin, entry->slotIndex, entry->name);
				if (attachment) Slot_setAttachment(slot, attachment);
			}
			entry = entry->next;
		}
	}
	CAST(Skin*, this->skin) = newSkin;
}

Attachment* Skeleton_getAttachmentForSlotName (const Skeleton* this, const char* slotName, const char* attachmentName) {
	int slotIndex = SkeletonData_findSlotIndex(this->data, slotName);
	return Skeleton_getAttachmentForSlotIndex(this, slotIndex, attachmentName);
}

Attachment* Skeleton_getAttachmentForSlotIndex (const Skeleton* this, int slotIndex, const char* attachmentName) {
	if (slotIndex == -1) return 0;
	if (this->skin) {
		Attachment *attachment = Skin_getAttachment(this->skin, slotIndex, attachmentName);
		if (attachment) return attachment;
	}
	if (this->data->defaultSkin) {
		Attachment *attachment = Skin_getAttachment(this->data->defaultSkin, slotIndex, attachmentName);
		if (attachment) return attachment;
	}
	return 0;
}

int Skeleton_setAttachment (Skeleton* this, const char* slotName, const char* attachmentName) {
	int i;
	for (i = 0; i < this->slotCount; ++i) {
		Slot *slot = this->slots[i];
		if (slot->data->name == slotName) {
			Attachment* attachment = Skeleton_getAttachmentForSlotIndex(this, i, attachmentName);
			if (!attachment) return 0;
			Slot_setAttachment(slot, attachment);
			return 1;
		}
	}
	return 0;
}

void Skeleton_update (Skeleton* this, float deltaTime) {
	this->time += deltaTime;
}
