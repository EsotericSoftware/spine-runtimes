/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#import <spine/spine.h>
#import "cocos2d.h"

/** Draws a skeleton. */
@interface SkeletonRenderer : CCNode<CCBlendProtocol> {
	spSkeleton* _skeleton;
	spBone* _rootBone;
	bool _debugSlots;
	bool _debugBones;
	bool _premultipliedAlpha;
	bool _twoColorTint;
    bool _skipVisibilityCheck;
	ccBlendFunc _blendFunc;
	CCDrawNode* _drawNode;
	bool _ownsSkeletonData;
	spAtlas* _atlas;
	float* _worldVertices;
	CCBlendMode* screenMode;
	spSkeletonClipping* _clipper;
	spVertexEffect* _effect;
}

+ (id) skeletonWithData:(spSkeletonData*)skeletonData ownsSkeletonData:(bool)ownsSkeletonData;
+ (id) skeletonWithFile:(NSString*)skeletonDataFile atlas:(spAtlas*)atlas scale:(float)scale;
+ (id) skeletonWithFile:(NSString*)skeletonDataFile atlasFile:(NSString*)atlasFile scale:(float)scale;

- (id) initWithData:(spSkeletonData*)skeletonData ownsSkeletonData:(bool)ownsSkeletonData;
- (id) initWithFile:(NSString*)skeletonDataFile atlas:(spAtlas*)atlas scale:(float)scale;
- (id) initWithFile:(NSString*)skeletonDataFile atlasFile:(NSString*)atlasFile scale:(float)scale;

- (CCTexture*) getTextureForRegion:(spRegionAttachment*)attachment;
- (CCTexture*) getTextureForMesh:(spMeshAttachment*)attachment;

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
@property (nonatomic) bool twoColorTint;
@property (nonatomic) bool debugSlots;
@property (nonatomic) bool debugBones;
@property (nonatomic) bool skipVisibilityCheck;
@property (nonatomic) spBone* rootBone;
@property (nonatomic) spVertexEffect* effect;

@end
