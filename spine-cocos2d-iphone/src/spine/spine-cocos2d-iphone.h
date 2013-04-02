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

typedef struct {
	AtlasPage super;
	CCTexture2D* texture;
	CCTextureAtlas* atlas;
} Cocos2dAtlasPage;

/**/

@class CCSkeleton;

typedef struct {
	Skeleton super;
	CCSkeleton* node;
} Cocos2dSkeleton;

@interface CCSkeleton : CCNodeRGBA<CCBlendProtocol> {
@public
	Skeleton* skeleton;
	AnimationState* state;
	bool debugSlots;
	bool debugBones;
	CCTextureAtlas* atlas; // All region attachments for a skeleton must use the same texture.
	unsigned int quadCount;
    ccBlendFunc blendFunc;
}

+ (CCSkeleton*) create:(SkeletonData*)skeletonData;

- init:(SkeletonData*)skeletonData;
- init:(SkeletonData*)skeletonData stateData:(AnimationStateData*)stateData;

@end

/**/

typedef struct {
	RegionAttachment super;
	ccV3F_C4B_T2F_Quad quad;
	CCTextureAtlas* atlas;
} Cocos2dRegionAttachment;

#endif /* SPINE_COCOS2D_H_ */
