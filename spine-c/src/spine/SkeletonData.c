#include <spine/SkeletonData.h>
#include <spine/util.h>

SkeletonData* SkeletonData_create () {
	SkeletonData* self = CALLOC(SkeletonData, 1)
	return self;
}

void SkeletonData_dispose (SkeletonData* self) {
	FREE(self)
}

BoneData* SkeletonData_findBone (const SkeletonData* self, const char* boneName) {
	int i;
	for (i = 0; i < self->boneCount; ++i)
		if (strcmp(self->bones[i]->name, boneName) == 0) return self->bones[i];
	return 0;
}

int SkeletonData_findBoneIndex (const SkeletonData* self, const char* boneName) {
	int i;
	for (i = 0; i < self->boneCount; ++i)
		if (strcmp(self->bones[i]->name, boneName) == 0) return i;
	return 0;
}

SlotData* SkeletonData_findSlot (const SkeletonData* self, const char* slotName) {
	int i;
	for (i = 0; i < self->slotCount; ++i)
		if (strcmp(self->slots[i]->name, slotName) == 0) return self->slots[i];
	return 0;
}

int SkeletonData_findSlotIndex (const SkeletonData* self, const char* slotName) {
	int i;
	for (i = 0; i < self->slotCount; ++i)
		if (strcmp(self->slots[i]->name, slotName) == 0) return i;
	return 0;
}

Skin* SkeletonData_findSkin (const SkeletonData* self, const char* skinName) {
	int i;
	for (i = 0; i < self->skinCount; ++i)
		if (strcmp(self->skins[i]->name, skinName) == 0) return self->skins[i];
	return 0;
}
