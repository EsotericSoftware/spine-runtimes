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

#include <spine/spine-cocos2d-iphone.h>
#include <spine/extension.h>

#ifdef __cplusplus
namespace spine {
#endif

typedef struct {
	AtlasPage super;
	CCTexture2D* texture;
	CCTextureAtlas* textureAtlas;
} Cocos2dAtlasPage;

void _Cocos2dAtlasPage_dispose (AtlasPage* page) {
	Cocos2dAtlasPage* self = SUB_CAST(Cocos2dAtlasPage, page);
	_AtlasPage_deinit(SUPER(self));

	[self->texture release];
	[self->textureAtlas release];

	FREE(page);
}

AtlasPage* AtlasPage_create (const char* name, const char* path) {
	Cocos2dAtlasPage* self = NEW(Cocos2dAtlasPage);
	_AtlasPage_init(SUPER(self), name, _Cocos2dAtlasPage_dispose);

	self->texture = [[CCTextureCache sharedTextureCache] addImage:@(path)];
	[self->texture retain];
	self->textureAtlas = [[CCTextureAtlas alloc] initWithTexture:self->texture capacity:4];
	[self->textureAtlas retain];

	return SUPER(self);
}

/**/

typedef struct {
	Skeleton super;
	CCSkeleton* node;
} Cocos2dSkeleton;

void _Cocos2dSkeleton_dispose (Skeleton* self) {
	_Skeleton_deinit(self);
	FREE(self);
}

Skeleton* _Cocos2dSkeleton_create (SkeletonData* data, CCSkeleton* node) {
	Cocos2dSkeleton* self = NEW(Cocos2dSkeleton);
	_Skeleton_init(SUPER(self), data, _Cocos2dSkeleton_dispose);

	self->node = node;

	return SUPER(self);
}

/**/

typedef struct {
	RegionAttachment super;
	ccV3F_C4B_T2F_Quad quad;
	CCTextureAtlas* textureAtlas;
} Cocos2dRegionAttachment;

void _Cocos2dRegionAttachment_dispose (Attachment* self) {
	_RegionAttachment_deinit(SUB_CAST(RegionAttachment, self) );
	FREE(self);
}

ccV3F_C4B_T2F_Quad* RegionAttachment_updateQuad (Attachment* attachment, Slot* slot) {
	Cocos2dRegionAttachment* self = SUB_CAST(Cocos2dRegionAttachment, attachment);
	Cocos2dSkeleton* skeleton = SUB_CAST(Cocos2dSkeleton, slot->skeleton);

	GLubyte r = SUPER(skeleton)->r * slot->r * 255;
	GLubyte g = SUPER(skeleton)->g * slot->g * 255;
	GLubyte b = SUPER(skeleton)->b * slot->b * 255;
	GLubyte a = SUPER(skeleton)->a * slot->a * 255;
	ccV3F_C4B_T2F_Quad* quad = &self->quad;
	quad->bl.colors.r = r;
	quad->bl.colors.g = g;
	quad->bl.colors.b = b;
	quad->bl.colors.a = a;
	quad->tl.colors.r = r;
	quad->tl.colors.g = g;
	quad->tl.colors.b = b;
	quad->tl.colors.a = a;
	quad->tr.colors.r = r;
	quad->tr.colors.g = g;
	quad->tr.colors.b = b;
	quad->tr.colors.a = a;
	quad->br.colors.r = r;
	quad->br.colors.g = g;
	quad->br.colors.b = b;
	quad->br.colors.a = a;

	float* offset = SUPER(self)->offset;
	quad->bl.vertices.x = offset[0] * slot->bone->m00 + offset[1] * slot->bone->m01 + slot->bone->worldX;
	quad->bl.vertices.y = offset[0] * slot->bone->m10 + offset[1] * slot->bone->m11 + slot->bone->worldY;
	quad->tl.vertices.x = offset[2] * slot->bone->m00 + offset[3] * slot->bone->m01 + slot->bone->worldX;
	quad->tl.vertices.y = offset[2] * slot->bone->m10 + offset[3] * slot->bone->m11 + slot->bone->worldY;
	quad->tr.vertices.x = offset[4] * slot->bone->m00 + offset[5] * slot->bone->m01 + slot->bone->worldX;
	quad->tr.vertices.y = offset[4] * slot->bone->m10 + offset[5] * slot->bone->m11 + slot->bone->worldY;
	quad->br.vertices.x = offset[6] * slot->bone->m00 + offset[7] * slot->bone->m01 + slot->bone->worldX;
	quad->br.vertices.y = offset[6] * slot->bone->m10 + offset[7] * slot->bone->m11 + slot->bone->worldY;

	return quad;
}

void _Cocos2dRegionAttachment_draw (Attachment* attachment, Slot* slot) {
	RegionAttachment_updateQuad(attachment, slot);

	Cocos2dRegionAttachment* self = SUB_CAST(Cocos2dRegionAttachment, attachment);
	Cocos2dSkeleton* skeleton = SUB_CAST(Cocos2dSkeleton, slot->skeleton);

	// Cocos2d doesn't handle batching for us, so we'll just force a single texture per skeleton.
	skeleton->node->textureAtlas = self->textureAtlas;
	while (self->textureAtlas.capacity <= skeleton->node->quadCount) {
		if (![self->textureAtlas resizeCapacity:self->textureAtlas.capacity * 2]) return;
	}
	[self->textureAtlas updateQuad:&self->quad atIndex:skeleton->node->quadCount++];
}

RegionAttachment* RegionAttachment_create (const char* name, AtlasRegion* region) {
	Cocos2dRegionAttachment* self = NEW(Cocos2dRegionAttachment);
	_RegionAttachment_init(SUPER(self), name, _Cocos2dRegionAttachment_dispose, _Cocos2dRegionAttachment_draw);

	Cocos2dAtlasPage* page = SUB_CAST(Cocos2dAtlasPage, region->page);
	self->textureAtlas = page->textureAtlas;
	CGSize size = page->texture.contentSizeInPixels;
	float u = region->x / size.width;
	float u2 = (region->x + region->width) / size.width;
	float v = region->y / size.height;
	float v2 = (region->y + region->height) / size.height;
	ccV3F_C4B_T2F_Quad* quad = &self->quad;
	if (region->rotate) {
		quad->tl.texCoords.u = u;
		quad->tl.texCoords.v = v2;
		quad->tr.texCoords.u = u;
		quad->tr.texCoords.v = v;
		quad->br.texCoords.u = u2;
		quad->br.texCoords.v = v;
		quad->bl.texCoords.u = u2;
		quad->bl.texCoords.v = v2;
	} else {
		quad->bl.texCoords.u = u;
		quad->bl.texCoords.v = v2;
		quad->tl.texCoords.u = u;
		quad->tl.texCoords.v = v;
		quad->tr.texCoords.u = u2;
		quad->tr.texCoords.v = v;
		quad->br.texCoords.u = u2;
		quad->br.texCoords.v = v2;
	}

	quad->bl.vertices.z = 0;
	quad->tl.vertices.z = 0;
	quad->tr.vertices.z = 0;
	quad->br.vertices.z = 0;

	return SUPER(self);
}

ccV3F_C4B_T2F_Quad* RegionAttachment_getQuad (RegionAttachment* attachment) {
	Cocos2dRegionAttachment* self = SUB_CAST(Cocos2dRegionAttachment, attachment);
	return &self->quad;
}

/**/

char* _Util_readFile (const char* path, int* length) {
	return _readFile([[[CCFileUtils sharedFileUtils] fullPathForFilename:@(path)] UTF8String], length);
}

#ifdef __cplusplus
}
#endif

/**/

@implementation CCSkeleton

+ (CCSkeleton*) create:(NSString*)skeletonDataFile atlas:(Atlas*)atlas {
	return [CCSkeleton create:skeletonDataFile atlas:atlas scale:1];
}

+ (CCSkeleton*) create:(NSString*)skeletonDataFile atlas:(Atlas*)atlas scale:(float)scale {
	NSAssert(skeletonDataFile, @"skeletonDataFile cannot be nil.");
	NSAssert(atlas, @"atlas cannot be nil.");
	SkeletonJson* json = SkeletonJson_create(atlas);
	json->scale = scale;
	SkeletonData* skeletonData = SkeletonJson_readSkeletonDataFile(json, [skeletonDataFile UTF8String]);
	NSAssert(skeletonData, ([NSString stringWithFormat:@"Error reading skeleton data file: %@\nError: %s", skeletonDataFile, json->error]));
	SkeletonJson_dispose(json);
	CCSkeleton* node = skeletonData ? [CCSkeleton create:skeletonData] : 0;
	node->ownsSkeleton = true;
	return node;
}

+ (CCSkeleton*) create:(NSString*)skeletonDataFile atlasFile:(NSString*)atlasFile {
	return [CCSkeleton create:skeletonDataFile atlasFile:atlasFile scale:1];
}

+ (CCSkeleton*) create:(NSString*)skeletonDataFile atlasFile:(NSString*)atlasFile scale:(float)scale {
	NSAssert(skeletonDataFile, @"skeletonDataFile cannot be nil.");
	NSAssert(atlasFile, @"atlasFile cannot be nil.");
	Atlas* atlas = Atlas_readAtlasFile([atlasFile UTF8String]);
	NSAssert(atlas, ([NSString stringWithFormat:@"Error reading atlas file: %@", atlasFile]));
	if (!atlas) return 0;
	SkeletonJson* json = SkeletonJson_create(atlas);
	json->scale = scale;
	SkeletonData* skeletonData = SkeletonJson_readSkeletonDataFile(json, [skeletonDataFile UTF8String]);
	NSAssert(skeletonData, ([NSString stringWithFormat:@"Error reading skeleton data file: %@\nError: %s", skeletonDataFile, json->error]));
	SkeletonJson_dispose(json);
	if (!skeletonData) {
		Atlas_dispose(atlas);
		return 0;
	}
	CCSkeleton* node = [CCSkeleton create:skeletonData];
	node->ownsSkeleton = true;
	node->atlas = atlas;
	return node;
}

+ (CCSkeleton*) create:(SkeletonData*)skeletonData {
	return [CCSkeleton create:skeletonData stateData:0];
}

+ (CCSkeleton*) create:(SkeletonData*)skeletonData stateData:(AnimationStateData*)stateData {
	return [[[CCSkeleton alloc] init:skeletonData stateData:stateData] autorelease];
}

- (id) init:(SkeletonData*)skeletonData {
	return [self init:skeletonData stateData:0];
}

- (id) init:(SkeletonData*)skeletonData stateData:(AnimationStateData*)stateData {
	NSAssert(skeletonData, @"skeletonData cannot be nil.");

	self = [super init];
	if (!self) return nil;

	CONST_CAST(Skeleton*, skeleton) = _Cocos2dSkeleton_create(skeletonData, self);

	if (!stateData) {
		stateData = AnimationStateData_create(skeletonData);
		ownsStateData = true;
	}
	CONST_CAST(AnimationState*, state) = AnimationState_create(stateData);

	blendFunc.src = GL_ONE;
	blendFunc.dst = GL_ONE_MINUS_SRC_ALPHA;

	timeScale = 1;

	[self setShaderProgram:[[CCShaderCache sharedShaderCache] programForKey:kCCShader_PositionTextureColor]];
	[self scheduleUpdate];

	return self;
}

- (void) dealloc {
	if (ownsSkeleton) Skeleton_dispose(skeleton);
	if (ownsStateData) AnimationStateData_dispose(state->data);
	if (atlas) Atlas_dispose(atlas);
	AnimationState_dispose(state);
	[super dealloc];
}

- (void) update:(ccTime)deltaTime {
	Skeleton_update(skeleton, deltaTime);
	AnimationState_update(state, deltaTime * timeScale);
	AnimationState_apply(state, skeleton);
	Skeleton_updateWorldTransform(skeleton);
}

- (void) draw {
	CC_NODE_DRAW_SETUP();

	ccGLBlendFunc(blendFunc.src, blendFunc.dst);
	ccColor3B color = self.color;
	skeleton->r = color.r / (float)255;
	skeleton->g = color.g / (float)255;
	skeleton->b = color.b / (float)255;
	skeleton->a = self.opacity / (float)255;

	quadCount = 0;
	for (int i = 0, n = skeleton->slotCount; i < n; ++i)
		if (skeleton->slots[i]->attachment) Attachment_draw(skeleton->slots[i]->attachment, skeleton->slots[i]);
	if (textureAtlas) [textureAtlas drawNumberOfQuads:quadCount];

	if (debugSlots) {
		// Slots.
		ccDrawColor4B(0, 0, 255, 255);
		glLineWidth(1);
		CGPoint points[4];
		for (int i = 0, n = skeleton->slotCount; i < n; ++i) {
			if (!skeleton->slots[i]->attachment) continue;
			ccV3F_C4B_T2F_Quad* quad = &((Cocos2dRegionAttachment*)skeleton->slots[i]->attachment)->quad;
			points[0] = ccp(quad->bl.vertices.x, quad->bl.vertices.y);
			points[1] = ccp(quad->br.vertices.x, quad->br.vertices.y);
			points[2] = ccp(quad->tr.vertices.x, quad->tr.vertices.y);
			points[3] = ccp(quad->tl.vertices.x, quad->tl.vertices.y);
			ccDrawPoly(points, 4, true);
		}
	}
	if (debugBones) {
		// Bone lengths.
		glLineWidth(2);
		ccDrawColor4B(255, 0, 0, 255);
		for (int i = 0, n = skeleton->boneCount; i < n; ++i) {
			Bone *bone = skeleton->bones[i];
			float x = bone->data->length * bone->m00 + bone->worldX;
			float y = bone->data->length * bone->m10 + bone->worldY;
			ccDrawLine(ccp(bone->worldX, bone->worldY), ccp(x, y));
		}
		// Bone origins.
		ccPointSize(4);
		ccDrawColor4B(0, 0, 255, 255); // Root bone is blue.
		for (int i = 0, n = skeleton->boneCount; i < n; ++i) {
			Bone *bone = skeleton->bones[i];
			ccDrawPoint(ccp(bone->worldX, bone->worldY));
			if (i == 0) ccDrawColor4B(0, 255, 0, 255);
		}
	}
}

- (CGRect) boundingBox {
	float minX = FLT_MAX, minY = FLT_MAX, maxX = FLT_MIN, maxY = FLT_MIN;
	for (int i = 0; i < skeleton->slotCount; ++i) {
		Slot* slot = skeleton->slots[i];
		Attachment* attachment = slot->attachment;
		if (attachment->type != ATTACHMENT_REGION) continue;
		Cocos2dRegionAttachment* regionAttachment = SUB_CAST(Cocos2dRegionAttachment, attachment);
		minX = fmin(minX, regionAttachment->quad.bl.vertices.x);
		minY = fmin(minY, regionAttachment->quad.bl.vertices.y);
		maxX = fmax(maxX, regionAttachment->quad.bl.vertices.x);
		maxY = fmax(maxY, regionAttachment->quad.bl.vertices.y);
		minX = fmin(minX, regionAttachment->quad.br.vertices.x);
		minY = fmin(minY, regionAttachment->quad.br.vertices.y);
		maxX = fmax(maxX, regionAttachment->quad.br.vertices.x);
		maxY = fmax(maxY, regionAttachment->quad.br.vertices.y);
		minX = fmin(minX, regionAttachment->quad.tl.vertices.x);
		minY = fmin(minY, regionAttachment->quad.tl.vertices.y);
		maxX = fmax(maxX, regionAttachment->quad.tl.vertices.x);
		maxY = fmax(maxY, regionAttachment->quad.tl.vertices.y);
		minX = fmin(minX, regionAttachment->quad.tr.vertices.x);
		minY = fmin(minY, regionAttachment->quad.tr.vertices.y);
		maxX = fmax(maxX, regionAttachment->quad.tr.vertices.x);
		maxY = fmax(maxY, regionAttachment->quad.tr.vertices.y);
	}
	return CGRectMake(minX, minY, maxX - minX, maxY - minY);
}

// Convenience methods:

- (void) setMix:(NSString*)fromName to:(NSString*)toName duration:(float)duration {
	AnimationStateData_setMixByName(state->data, [fromName UTF8String], [toName UTF8String], duration);
}
- (void) setAnimation:(NSString*)animationName loop:(bool)loop {
	AnimationState_setAnimationByName(state, [animationName UTF8String], loop);
}

- (void) updateWorldTransform {
	Skeleton_updateWorldTransform(skeleton);
}

- (void) setToBindPose {
	Skeleton_setToBindPose(skeleton);
}
- (void) setBonesToBindPose {
	Skeleton_setBonesToBindPose(skeleton);
}
- (void) setSlotsToBindPose {
	Skeleton_setSlotsToBindPose(skeleton);
}

- (Bone*) findBone:(NSString*)boneName {
	return Skeleton_findBone(skeleton, [boneName UTF8String]);
}
- (int) findBoneIndex:(NSString*)boneName {
	return Skeleton_findBoneIndex(skeleton, [boneName UTF8String]);
}

- (Slot*) findSlot:(NSString*)slotName {
	return Skeleton_findSlot(skeleton, [slotName UTF8String]);
}
- (int) findSlotIndex:(NSString*)slotName {
	return Skeleton_findSlotIndex(skeleton, [slotName UTF8String]);
}

- (bool) setSkin:(NSString*)skinName {
	return (bool)Skeleton_setSkinByName(skeleton, [skinName UTF8String]);
}

- (Attachment*) getAttachmentForSlotName:(NSString*)slotName attachmentName:(NSString*)attachmentName {
	return Skeleton_getAttachmentForSlotName(skeleton, [slotName UTF8String], [attachmentName UTF8String]);
}
- (Attachment*) getAttachmentForSlotIndex:(int)slotIndex attachmentName:(NSString*)attachmentName {
	return Skeleton_getAttachmentForSlotIndex(skeleton, slotIndex, [attachmentName UTF8String]);
}
- (bool) setAttachment:(NSString*)slotName attachmentName:(NSString*)attachmentName {
	return (bool)Skeleton_setAttachment(skeleton, [slotName UTF8String], [attachmentName UTF8String]);
}

// CCBlendProtocol

- (void) setBlendFunc:(ccBlendFunc)func {
	self.blendFunc = func;
}

- (ccBlendFunc) blendFunc {
	return blendFunc;
}

@end
