#ifndef SPINE_SKELETONDATA_H_
#define SPINE_SKELETONDATA_H_

#include <spine/BoneData.h>
#include <spine/SlotData.h>
#include <spine/Skin.h>

#ifdef __cplusplus
namespace spine {extern "C" {
#endif

typedef struct {
	int boneCount;
	BoneData** bones;

	int slotCount;
	SlotData** slots;

	int skinCount;
	Skin** skins;

	Skin* defaultSkin;
} SkeletonData;

SkeletonData* SkeletonData_create ();
void SkeletonData_dispose (SkeletonData* skeletonData);

BoneData* SkeletonData_findBone (const SkeletonData* skeletonData, const char* boneName);
int SkeletonData_findBoneIndex (const SkeletonData* skeletonData, const char* boneName);

SlotData* SkeletonData_findSlot (const SkeletonData* skeletonData, const char* slotName);
int SkeletonData_findSlotIndex (const SkeletonData* skeletonData, const char* slotName);

Skin* SkeletonData_findSkin (const SkeletonData* skeletonData, const char* skinName);

#ifdef __cplusplus
}}
#endif

#endif /* SPINE_SKELETONDATA_H_ */
