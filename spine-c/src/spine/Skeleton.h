#ifndef SPINE_SKELETON_H_
#define SPINE_SKELETON_H_

#include <spine/SkeletonData.h>
#include <spine/Slot.h>
#include <spine/Skin.h>

#ifdef __cplusplus
namespace spine {
extern "C" {
#endif

typedef struct Skeleton Skeleton;
struct Skeleton {
	SkeletonData* const data;

	int boneCount;
	Bone** bones;

	int slotCount;
	Slot** slots;
	Slot** drawOrder;

	Skin* const skin;
	float r, g, b, a;
	float time;
	int/*bool*/flipX, flipY;

	void (*_dispose) (Skeleton* skeleton);
};

void Skeleton_init (Skeleton* skeleton, SkeletonData* data);
void Skeleton_dispose (Skeleton* skeleton);

void Skeleton_updateWorldTransform (const Skeleton* skeleton);

void Skeleton_setToBindPose (const Skeleton* skeleton);
void Skeleton_setBonesToBindPose (const Skeleton* skeleton);
void Skeleton_setSlotsToBindPose (const Skeleton* skeleton);

Bone* Skeleton_getRootBone (const Skeleton* skeleton);
/** Returns 0 if the bone could not be found. */
Bone* Skeleton_findBone (const Skeleton* skeleton, const char* boneName);
/** Returns -1 if the bone could not be found. */
int Skeleton_findBoneIndex (const Skeleton* skeleton, const char* boneName);

/** Returns 0 if the slot could not be found. */
Slot* Skeleton_findSlot (const Skeleton* skeleton, const char* slotName);
/** Returns -1 if the slot could not be found. */
int Skeleton_findSlotIndex (const Skeleton* skeleton, const char* slotName);

/** Returns 0 if the skin could not be found. */
int Skeleton_setSkinByName (Skeleton* skeleton, const char* skinName);
/** @param skin May be 0.*/
void Skeleton_setSkin (Skeleton* skeleton, Skin* skin);

/** Returns 0 if the slot or attachment could not be found. */
Attachment* Skeleton_getAttachmentForSlotName (const Skeleton* skeleton, const char* slotName, const char* attachmentName);
/** Returns 0 if the slot or attachment could not be found. */
Attachment* Skeleton_getAttachmentForSlotIndex (const Skeleton* skeleton, int slotIndex, const char* attachmentName);
/** Returns 0 if the slot or attachment could not be found. */
int Skeleton_setAttachment (Skeleton* skeleton, const char* slotName, const char* attachmentName);

void Skeleton_update (Skeleton* skeleton, float deltaTime);

#ifdef __cplusplus
}
}
#endif

#endif /* SPINE_SKELETON_H_*/
