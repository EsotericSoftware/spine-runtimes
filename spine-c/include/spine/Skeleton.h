/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
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
 ******************************************************************************/

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

	const void* const vtable;
};

Skeleton* Skeleton_new (SkeletonData* data);
void Skeleton_free (Skeleton* skeleton);

void Skeleton_updateWorldTransform (const Skeleton* skeleton);

void Skeleton_setToBindPose (const Skeleton* skeleton);
void Skeleton_setBonesToBindPose (const Skeleton* skeleton);
void Skeleton_setSlotsToBindPose (const Skeleton* skeleton);

Bone* Skeleton_getRootBone (const Skeleton* skeleton);
/* Returns 0 if the bone was not found. */
Bone* Skeleton_findBone (const Skeleton* skeleton, const char* boneName);
/* Returns -1 if the bone was not found. */
int Skeleton_findBoneIndex (const Skeleton* skeleton, const char* boneName);

/* Returns 0 if the slot was not found. */
Slot* Skeleton_findSlot (const Skeleton* skeleton, const char* slotName);
/* Returns -1 if the slot was not found. */
int Skeleton_findSlotIndex (const Skeleton* skeleton, const char* slotName);

/* Returns 0 if the skin was not found. */
int Skeleton_setSkinByName (Skeleton* skeleton, const char* skinName);
/* @param skin May be 0.*/
void Skeleton_setSkin (Skeleton* skeleton, Skin* skin);

/* Returns 0 if the slot or attachment was not found. */
Attachment* Skeleton_getAttachmentForSlotName (const Skeleton* skeleton, const char* slotName, const char* attachmentName);
/* Returns 0 if the slot or attachment was not found. */
Attachment* Skeleton_getAttachmentForSlotIndex (const Skeleton* skeleton, int slotIndex, const char* attachmentName);
/* Returns 0 if the slot or attachment was not found. */
int Skeleton_setAttachment (Skeleton* skeleton, const char* slotName, const char* attachmentName);

void Skeleton_update (Skeleton* skeleton, float deltaTime);

#ifdef __cplusplus
}
}
#endif

#endif /* SPINE_SKELETON_H_*/
