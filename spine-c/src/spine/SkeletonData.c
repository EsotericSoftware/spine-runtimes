#include <spine/SkeletonData.h>
#include <spine/util.h>

SkeletonData* SkeletonData_create () {
	SkeletonData* this = calloc(1, sizeof(SkeletonData));
	return this;
}

void SkeletonData_dispose (SkeletonData* this) {
	FREE(this)
}

BoneData* SkeletonData_findBone (const SkeletonData* this, const char* boneName) {
	int i;
	for (i = 0; i < this->boneCount; ++i)
		if (strcmp(this->bones[i]->name, boneName) == 0) return this->bones[i];
	return 0;
}

int SkeletonData_findBoneIndex (const SkeletonData* this, const char* boneName) {
	int i;
	for (i = 0; i < this->boneCount; ++i)
		if (strcmp(this->bones[i]->name, boneName) == 0) return i;
	return 0;
}

SlotData* SkeletonData_findSlot (const SkeletonData* this, const char* slotName) {
	int i;
	for (i = 0; i < this->slotCount; ++i)
		if (strcmp(this->slots[i]->name, slotName) == 0) return this->slots[i];
	return 0;
}

int SkeletonData_findSlotIndex (const SkeletonData* this, const char* slotName) {
	int i;
	for (i = 0; i < this->slotCount; ++i)
		if (strcmp(this->slots[i]->name, slotName) == 0) return i;
	return 0;
}

Skin* SkeletonData_findSkin (const SkeletonData* this, const char* skinName) {
	int i;
	for (i = 0; i < this->skinCount; ++i)
		if (strcmp(this->skins[i]->name, skinName) == 0) return this->skins[i];
	return 0;
}
