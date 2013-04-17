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

#ifndef SPINE_COCOS2D_H_
#define SPINE_COCOS2D_H_

#include <spine/spine.h>
#include "cocos2d.h"

@interface CCSkeleton : CCNodeRGBA<CCBlendProtocol> {
@private
	bool ownsSkeleton;
	bool ownsStateData;
	Atlas* atlas;

@public
	Skeleton* const skeleton;
	AnimationState* const state;
	float timeScale;
	bool debugSlots;
	bool debugBones;

    ccBlendFunc blendFunc;
}

+ (CCSkeleton*) create:(NSString*)skeletonDataFile atlas:(Atlas*)atlas;
+ (CCSkeleton*) create:(NSString*)skeletonDataFile atlas:(Atlas*)atlas scale:(float)scale;

+ (CCSkeleton*) create:(NSString*)skeletonDataFile atlasFile:(NSString*)atlasFile;
+ (CCSkeleton*) create:(NSString*)skeletonDataFile atlasFile:(NSString*)atlasFile scale:(float)scale;

+ (CCSkeleton*) create:(SkeletonData*)skeletonData;
+ (CCSkeleton*) create:(SkeletonData*)skeletonData stateData:(AnimationStateData*)stateData;

- init:(SkeletonData*)skeletonData;
- init:(SkeletonData*)skeletonData stateData:(AnimationStateData*)stateData;

- (void) setMix:(NSString*)fromName to:(NSString*)toName duration:(float)duration;
- (void) setAnimation:(NSString*)animationName loop:(bool)loop;

- (void) updateWorldTransform;

- (void) setToBindPose;
- (void) setBonesToBindPose;
- (void) setSlotsToBindPose;

/* Returns 0 if the bone was not found. */
- (Bone*) findBone:(NSString*)boneName;
/* Returns -1 if the bone was not found. */
- (int) findBoneIndex:(NSString*)boneName;

/* Returns 0 if the slot was not found. */
- (Slot*) findSlot:(NSString*)slotName;
/* Returns -1 if the slot was not found. */
- (int) findSlotIndex:(NSString*)slotName;

/* Sets the skin used to look up attachments not found in the SkeletonData defaultSkin. Attachments from the new skin are
 * attached if the corresponding attachment from the old skin was attached. Returns false if the skin was not found.
 * @param skin May be 0.*/
- (bool) setSkin:(NSString*)skinName;

/* Returns 0 if the slot or attachment was not found. */
- (Attachment*) getAttachmentForSlotName:(NSString*)slotName attachmentName:(NSString*)attachmentName;
/* Returns 0 if the slot or attachment was not found. */
- (Attachment*) getAttachmentForSlotIndex:(int)slotIndex attachmentName:(NSString*)attachmentName;
/* Returns false if the slot or attachment was not found. */
- (bool) setAttachment:(NSString*)slotName attachmentName:(NSString*)attachmentName;

@end

/**/

#ifdef __cplusplus
namespace spine {
extern "C" {
#endif

void RegionAttachment_updateQuad (RegionAttachment* self, Slot* slot, ccV3F_C4B_T2F_Quad* quad);

#ifdef __cplusplus
}
}
using namespace spine;
#endif

#endif /* SPINE_COCOS2D_H_ */
