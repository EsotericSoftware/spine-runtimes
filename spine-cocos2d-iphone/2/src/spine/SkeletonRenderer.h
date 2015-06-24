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

#import <spine/spine.h>
#import "cocos2d.h"

@class spPolygonBatch;

/** Draws a skeleton. */
@interface SkeletonRenderer : CCNodeRGBA<CCBlendProtocol> {
	spSkeleton* _skeleton;
	spBone* _rootBone;
	bool _debugSlots;
	bool _debugBones;
	bool _premultipliedAlpha;
	ccBlendFunc _blendFunc;

	bool _ownsSkeletonData;
	spAtlas* _atlas;
	spPolygonBatch* batch;
	float* worldVertices;
}

+ (id) skeletonWithData:(spSkeletonData*)skeletonData ownsSkeletonData:(bool)ownsSkeletonData;
+ (id) skeletonWithFile:(NSString*)skeletonDataFile atlas:(spAtlas*)atlas scale:(float)scale;
+ (id) skeletonWithFile:(NSString*)skeletonDataFile atlasFile:(NSString*)atlasFile scale:(float)scale;

- (id) initWithData:(spSkeletonData*)skeletonData ownsSkeletonData:(bool)ownsSkeletonData;
- (id) initWithFile:(NSString*)skeletonDataFile atlas:(spAtlas*)atlas scale:(float)scale;
- (id) initWithFile:(NSString*)skeletonDataFile atlasFile:(NSString*)atlasFile scale:(float)scale;

- (CCTexture2D*) getTextureForRegion:(spRegionAttachment*)attachment;
- (CCTexture2D*) getTextureForMesh:(spMeshAttachment*)attachment;
- (CCTexture2D*) getTextureForSkinnedMesh:(spSkinnedMeshAttachment*)attachment;

// --- Convenience methods for common Skeleton_* functions.
- (void) updateWorldTransform;

- (void) setToSetupPose;
- (void) setBonesToSetupPose;
- (void) setSlotsToSetupPose;

/* Returns 0 if the bone was not found. */
- (spBone*) findBone:(NSString*)boneName;

/* Returns 0 if the slot was not found. */
- (spSlot*) findSlot:(NSString*)slotName;

/* Sets the skin used to look up attachments not found in the SkeletonData defaultSkin. Attachments from the new skin are
 * attached if the corresponding attachment from the old skin was attached. If there was no old skin, each slot's setup mode
 * attachment is attached from the new skin. Returns false if the skin was not found.
 * @param skin May be 0.*/
- (bool) setSkin:(NSString*)skinName;

/* Returns 0 if the slot or attachment was not found. */
- (spAttachment*) getAttachment:(NSString*)slotName attachmentName:(NSString*)attachmentName;
/* Returns false if the slot or attachment was not found. */
- (bool) setAttachment:(NSString*)slotName attachmentName:(NSString*)attachmentName;

@property (nonatomic, readonly) spSkeleton* skeleton;
@property (nonatomic) bool debugSlots;
@property (nonatomic) bool debugBones;
@property (nonatomic) spBone* rootBone;

@end
