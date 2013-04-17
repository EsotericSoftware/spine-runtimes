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

void _AtlasPage_createTexture (AtlasPage* self, const char* path) {
	CCTexture2D* texture = [[CCTextureCache sharedTextureCache] addImage:@(path)];
	CCTextureAtlas* textureAtlas = [[CCTextureAtlas alloc] initWithTexture:texture capacity:4];
	[textureAtlas retain];
	self->texture = textureAtlas;
	CGSize size = texture.contentSizeInPixels;
	self->width = size.width;
	self->height = size.height;
}

void _AtlasPage_disposeTexture (AtlasPage* self) {
	[(CCTextureAtlas*)self->texture release];
}

char* _Util_readFile (const char* path, int* length) {
	return _readFile([[[CCFileUtils sharedFileUtils] fullPathForFilename:@(path)] UTF8String], length);
}

/**/

void RegionAttachment_updateQuad (RegionAttachment* self, Slot* slot, ccV3F_C4B_T2F_Quad* quad) {
	RegionAttachment_updateVertices(self, slot);

	GLubyte r = slot->skeleton->r * slot->r * 255;
	GLubyte g = slot->skeleton->g * slot->g * 255;
	GLubyte b = slot->skeleton->b * slot->b * 255;
	GLubyte a = slot->skeleton->a * slot->a * 255;
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

	float* offset = self->offset;
	quad->bl.vertices.x = offset[VERTEX_X1] * slot->bone->m00 + offset[VERTEX_Y1] * slot->bone->m01 + slot->bone->worldX;
	quad->bl.vertices.y = offset[VERTEX_X1] * slot->bone->m10 + offset[VERTEX_Y1] * slot->bone->m11 + slot->bone->worldY;
	quad->tl.vertices.x = offset[VERTEX_X2] * slot->bone->m00 + offset[VERTEX_Y2] * slot->bone->m01 + slot->bone->worldX;
	quad->tl.vertices.y = offset[VERTEX_X2] * slot->bone->m10 + offset[VERTEX_Y2] * slot->bone->m11 + slot->bone->worldY;
	quad->tr.vertices.x = offset[VERTEX_X3] * slot->bone->m00 + offset[VERTEX_Y3] * slot->bone->m01 + slot->bone->worldX;
	quad->tr.vertices.y = offset[VERTEX_X3] * slot->bone->m10 + offset[VERTEX_Y3] * slot->bone->m11 + slot->bone->worldY;
	quad->br.vertices.x = offset[VERTEX_X4] * slot->bone->m00 + offset[VERTEX_Y4] * slot->bone->m01 + slot->bone->worldX;
	quad->br.vertices.y = offset[VERTEX_X4] * slot->bone->m10 + offset[VERTEX_Y4] * slot->bone->m11 + slot->bone->worldY;

	if (self->region->rotate) {
		quad->tl.texCoords.u = self->region->u;
		quad->tl.texCoords.v = self->region->v2;
		quad->tr.texCoords.u = self->region->u;
		quad->tr.texCoords.v = self->region->v;
		quad->br.texCoords.u = self->region->u2;
		quad->br.texCoords.v = self->region->v;
		quad->bl.texCoords.u = self->region->u2;
		quad->bl.texCoords.v = self->region->v2;
	} else {
		quad->bl.texCoords.u = self->region->u;
		quad->bl.texCoords.v = self->region->v2;
		quad->tl.texCoords.u = self->region->u;
		quad->tl.texCoords.v = self->region->v;
		quad->tr.texCoords.u = self->region->u2;
		quad->tr.texCoords.v = self->region->v;
		quad->br.texCoords.u = self->region->u2;
		quad->br.texCoords.v = self->region->v2;
	}
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

	CONST_CAST(Skeleton*, skeleton) = Skeleton_create(skeletonData);

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

	CCTextureAtlas* textureAtlas = 0;
	int quadCount = 0;
	for (int i = 0, n = skeleton->slotCount; i < n; i++) {
		Slot* slot = skeleton->slots[i];
		if (!slot->attachment || slot->attachment->type != ATTACHMENT_REGION) continue;
		RegionAttachment* attachment = (RegionAttachment*)slot->attachment;
		CCTextureAtlas* regionTextureAtlas = (CCTextureAtlas*)attachment->region->page->texture;
		if (regionTextureAtlas != textureAtlas) {
			if (textureAtlas) {
				[textureAtlas drawNumberOfQuads:quadCount];
				quadCount = 0;
			}
		}
		textureAtlas = regionTextureAtlas;
		if (textureAtlas.capacity == quadCount && ![textureAtlas resizeCapacity:quadCount * 2]) return;
		RegionAttachment_updateQuad(attachment, slot, &textureAtlas.quads[quadCount++]);
	}
	if (textureAtlas) {
		[textureAtlas drawNumberOfQuads:quadCount];
		quadCount = 0;
	}

	if (debugSlots) {
		// Slots.
		ccDrawColor4B(0, 0, 255, 255);
		glLineWidth(1);
		CGPoint points[4];
		ccV3F_C4B_T2F_Quad quad;
		for (int i = 0, n = skeleton->slotCount; i < n; i++) {
			Slot* slot = skeleton->slots[i];
			if (!slot->attachment || slot->attachment->type != ATTACHMENT_REGION) continue;
			RegionAttachment* attachment = (RegionAttachment*)slot->attachment;
			RegionAttachment_updateQuad(attachment, slot, &quad);
			points[0] = ccp(quad.bl.vertices.x, quad.bl.vertices.y);
			points[1] = ccp(quad.br.vertices.x, quad.br.vertices.y);
			points[2] = ccp(quad.tr.vertices.x, quad.tr.vertices.y);
			points[3] = ccp(quad.tl.vertices.x, quad.tl.vertices.y);
			ccDrawPoly(points, 4, true);
		}
	}
	if (debugBones) {
		// Bone lengths.
		glLineWidth(2);
		ccDrawColor4B(255, 0, 0, 255);
		for (int i = 0, n = skeleton->boneCount; i < n; i++) {
			Bone *bone = skeleton->bones[i];
			float x = bone->data->length * bone->m00 + bone->worldX;
			float y = bone->data->length * bone->m10 + bone->worldY;
			ccDrawLine(ccp(bone->worldX, bone->worldY), ccp(x, y));
		}
		// Bone origins.
		ccPointSize(4);
		ccDrawColor4B(0, 0, 255, 255); // Root bone is blue.
		for (int i = 0, n = skeleton->boneCount; i < n; i++) {
			Bone *bone = skeleton->bones[i];
			ccDrawPoint(ccp(bone->worldX, bone->worldY));
			if (i == 0) ccDrawColor4B(0, 255, 0, 255);
		}
	}
}

- (CGRect) boundingBox {
	float minX = FLT_MAX, minY = FLT_MAX, maxX = FLT_MIN, maxY = FLT_MIN;
	float scaleX = self.scaleX;
	float scaleY = self.scaleY;
	ccV3F_C4B_T2F_Quad quad;
	for (int i = 0; i < skeleton->slotCount; ++i) {
		Slot* slot = skeleton->slots[i];
		if (!slot->attachment || slot->attachment->type != ATTACHMENT_REGION) continue;
		RegionAttachment* attachment = (RegionAttachment*)slot->attachment;
		RegionAttachment_updateQuad(attachment, slot, &quad);
		minX = fmin(minX, quad.bl.vertices.x * scaleX);
		minY = fmin(minY, quad.bl.vertices.y * scaleY);
		maxX = fmax(maxX, quad.bl.vertices.x * scaleX);
		maxY = fmax(maxY, quad.bl.vertices.y * scaleY);
		minX = fmin(minX, quad.br.vertices.x * scaleX);
		minY = fmin(minY, quad.br.vertices.y * scaleY);
		maxX = fmax(maxX, quad.br.vertices.x * scaleX);
		maxY = fmax(maxY, quad.br.vertices.y * scaleY);
		minX = fmin(minX, quad.tl.vertices.x * scaleX);
		minY = fmin(minY, quad.tl.vertices.y * scaleY);
		maxX = fmax(maxX, quad.tl.vertices.x * scaleX);
		maxY = fmax(maxY, quad.tl.vertices.y * scaleY);
		minX = fmin(minX, quad.tr.vertices.x * scaleX);
		minY = fmin(minY, quad.tr.vertices.y * scaleY);
		maxX = fmax(maxX, quad.tr.vertices.x * scaleX);
		maxY = fmax(maxY, quad.tr.vertices.y * scaleY);
	}
	minX = self.position.x + minX;
	minY = self.position.y + minY;
	maxX = self.position.x + maxX;
	maxY = self.position.y + maxY;
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
