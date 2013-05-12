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

#import <spine/CCSkeleton.h>
#import <spine/spine-cocos2d-iphone.h>

@interface CCSkeleton (Private)
- (void) initialize:(SkeletonData*)skeletonData ownsSkeletonData:(bool)ownsSkeletonData;
@end

@implementation CCSkeleton

@synthesize skeleton = _skeleton;
@synthesize rootBone = _rootBone;
@synthesize timeScale = _timeScale;
@synthesize debugSlots = _debugSlots;
@synthesize debugBones = _debugBones;

+ (id) skeletonWithData:(SkeletonData*)skeletonData ownsSkeletonData:(bool)ownsSkeletonData {
	return [[[CCSkeleton alloc] initWithData:skeletonData ownsSkeletonData:ownsSkeletonData] autorelease];
}

+ (id) skeletonWithFile:(NSString*)skeletonDataFile atlas:(Atlas*)atlas scale:(float)scale {
	return [[[CCSkeleton alloc] initWithFile:skeletonDataFile atlas:atlas scale:scale] autorelease];
}

+ (id) skeletonWithFile:(NSString*)skeletonDataFile atlasFile:(NSString*)atlasFile scale:(float)scale {
	return [[[CCSkeleton alloc] initWithFile:skeletonDataFile atlasFile:atlasFile scale:scale] autorelease];
}

- (void) initialize:(SkeletonData*)skeletonData ownsSkeletonData:(bool)ownsSkeletonData {
	_ownsSkeletonData = ownsSkeletonData;

	_skeleton = Skeleton_create(skeletonData);
	_rootBone = _skeleton->bones[0];

	_blendFunc.src = GL_ONE;
	_blendFunc.dst = GL_ONE_MINUS_SRC_ALPHA;
	[self setOpacityModifyRGB:YES];

	_timeScale = 1;

	[self setShaderProgram:[[CCShaderCache sharedShaderCache] programForKey:kCCShader_PositionTextureColor]];
	[self scheduleUpdate];
}

- (id) initWithData:(SkeletonData*)skeletonData ownsSkeletonData:(bool)ownsSkeletonData {
	NSAssert(skeletonData, @"skeletonData cannot be null.");

	self = [super init];
	if (!self) return nil;

	[self initialize:skeletonData ownsSkeletonData:ownsSkeletonData];

	return self;
}

- (id) initWithFile:(NSString*)skeletonDataFile atlas:(Atlas*)atlas scale:(float)scale {
	self = [super init];
	if (!self) return nil;

	SkeletonJson* json = SkeletonJson_create(atlas);
	json->scale = scale;
	SkeletonData* skeletonData = SkeletonJson_readSkeletonDataFile(json, [skeletonDataFile UTF8String]);
	NSAssert(skeletonData, ([NSString stringWithFormat:@"Error reading skeleton data file: %@\nError: %s", skeletonDataFile, json->error]));
	SkeletonJson_dispose(json);
	if (!skeletonData) return 0;

	[self initialize:skeletonData ownsSkeletonData:YES];

	return self;
}

- (id) initWithFile:(NSString*)skeletonDataFile atlasFile:(NSString*)atlasFile scale:(float)scale {
	self = [super init];
	if (!self) return nil;

	_atlas = Atlas_readAtlasFile([atlasFile UTF8String]);
	NSAssert(_atlas, ([NSString stringWithFormat:@"Error reading atlas file: %@", atlasFile]));
	if (!_atlas) return 0;

	SkeletonJson* json = SkeletonJson_create(_atlas);
	json->scale = scale;
	SkeletonData* skeletonData = SkeletonJson_readSkeletonDataFile(json, [skeletonDataFile UTF8String]);
	NSAssert(skeletonData, ([NSString stringWithFormat:@"Error reading skeleton data file: %@\nError: %s", skeletonDataFile, json->error]));
	SkeletonJson_dispose(json);
	if (!skeletonData) return 0;

	[self initialize:skeletonData ownsSkeletonData:YES];

	return self;
}

- (void) dealloc {
	if (_ownsSkeletonData) SkeletonData_dispose(_skeleton->data);
	if (_atlas) Atlas_dispose(_atlas);
	Skeleton_dispose(_skeleton);
	[super dealloc];
}

- (void) update:(ccTime)deltaTime {
	Skeleton_update(_skeleton, deltaTime * _timeScale);
}

- (void) draw {
	CC_NODE_DRAW_SETUP();

	ccGLBlendFunc(_blendFunc.src, _blendFunc.dst);
	ccColor3B color = self.color;
	_skeleton->r = color.r / (float)255;
	_skeleton->g = color.g / (float)255;
	_skeleton->b = color.b / (float)255;
	_skeleton->a = self.opacity / (float)255;
	if (_premultipliedAlpha) {
		_skeleton->r *= _skeleton->a;
		_skeleton->g *= _skeleton->a;
		_skeleton->b *= _skeleton->a;
	}

	CCTextureAtlas* textureAtlas = 0;
	ccV3F_C4B_T2F_Quad quad;
	quad.tl.vertices.z = 0;
	quad.tr.vertices.z = 0;
	quad.bl.vertices.z = 0;
	quad.br.vertices.z = 0;
	for (int i = 0, n = _skeleton->slotCount; i < n; i++) {
		Slot* slot = _skeleton->slots[i];
		if (!slot->attachment || slot->attachment->type != ATTACHMENT_REGION) continue;
		RegionAttachment* attachment = (RegionAttachment*)slot->attachment;
		CCTextureAtlas* regionTextureAtlas = [self getTextureAtlas:attachment];
		if (regionTextureAtlas != textureAtlas) {
			if (textureAtlas) {
				[textureAtlas drawQuads];
				[textureAtlas removeAllQuads];
			}
		}
		textureAtlas = regionTextureAtlas;
		if (textureAtlas.capacity == textureAtlas.totalQuads &&
			![textureAtlas resizeCapacity:textureAtlas.capacity * 2]) return;
		RegionAttachment_updateQuad(attachment, slot, &quad, _premultipliedAlpha);
		[textureAtlas updateQuad:&quad atIndex:textureAtlas.totalQuads];
	}
	if (textureAtlas) {
		[textureAtlas drawQuads];
		[textureAtlas removeAllQuads];
	}

	if (_debugSlots) {
		// Slots.
		ccDrawColor4B(0, 0, 255, 255);
		glLineWidth(1);
		CGPoint points[4];
		ccV3F_C4B_T2F_Quad quad;
		for (int i = 0, n = _skeleton->slotCount; i < n; i++) {
			Slot* slot = _skeleton->slots[i];
			if (!slot->attachment || slot->attachment->type != ATTACHMENT_REGION) continue;
			RegionAttachment* attachment = (RegionAttachment*)slot->attachment;
			RegionAttachment_updateQuad(attachment, slot, &quad, _premultipliedAlpha);
			points[0] = ccp(quad.bl.vertices.x, quad.bl.vertices.y);
			points[1] = ccp(quad.br.vertices.x, quad.br.vertices.y);
			points[2] = ccp(quad.tr.vertices.x, quad.tr.vertices.y);
			points[3] = ccp(quad.tl.vertices.x, quad.tl.vertices.y);
			ccDrawPoly(points, 4, true);
		}
	}
	if (_debugBones) {
		// Bone lengths.
		glLineWidth(2);
		ccDrawColor4B(255, 0, 0, 255);
		for (int i = 0, n = _skeleton->boneCount; i < n; i++) {
			Bone *bone = _skeleton->bones[i];
			float x = bone->data->length * bone->m00 + bone->worldX;
			float y = bone->data->length * bone->m10 + bone->worldY;
			ccDrawLine(ccp(bone->worldX, bone->worldY), ccp(x, y));
		}
		// Bone origins.
		ccPointSize(4);
		ccDrawColor4B(0, 0, 255, 255); // Root bone is blue.
		for (int i = 0, n = _skeleton->boneCount; i < n; i++) {
			Bone *bone = _skeleton->bones[i];
			ccDrawPoint(ccp(bone->worldX, bone->worldY));
			if (i == 0) ccDrawColor4B(0, 255, 0, 255);
		}
	}
}

- (CCTextureAtlas*) getTextureAtlas:(RegionAttachment*)regionAttachment {
	return (CCTextureAtlas*)((AtlasRegion*)regionAttachment->rendererObject)->page->rendererObject;
}

- (CGRect) boundingBox {
	float minX = FLT_MAX, minY = FLT_MAX, maxX = FLT_MIN, maxY = FLT_MIN;
	float scaleX = self.scaleX;
	float scaleY = self.scaleY;
	float vertices[8];
	for (int i = 0; i < _skeleton->slotCount; ++i) {
		Slot* slot = _skeleton->slots[i];
		if (!slot->attachment || slot->attachment->type != ATTACHMENT_REGION) continue;
		RegionAttachment* attachment = (RegionAttachment*)slot->attachment;
		RegionAttachment_computeVertices(attachment, slot, vertices);
		minX = fmin(minX, vertices[VERTEX_X1] * scaleX);
		minY = fmin(minY, vertices[VERTEX_Y1] * scaleY);
		maxX = fmax(maxX, vertices[VERTEX_X1] * scaleX);
		maxY = fmax(maxY, vertices[VERTEX_Y1] * scaleY);
		minX = fmin(minX, vertices[VERTEX_X4] * scaleX);
		minY = fmin(minY, vertices[VERTEX_Y4] * scaleY);
		maxX = fmax(maxX, vertices[VERTEX_X4] * scaleX);
		maxY = fmax(maxY, vertices[VERTEX_Y4] * scaleY);
		minX = fmin(minX, vertices[VERTEX_X2] * scaleX);
		minY = fmin(minY, vertices[VERTEX_Y2] * scaleY);
		maxX = fmax(maxX, vertices[VERTEX_X2] * scaleX);
		maxY = fmax(maxY, vertices[VERTEX_Y2] * scaleY);
		minX = fmin(minX, vertices[VERTEX_X3] * scaleX);
		minY = fmin(minY, vertices[VERTEX_Y3] * scaleY);
		maxX = fmax(maxX, vertices[VERTEX_X3] * scaleX);
		maxY = fmax(maxY, vertices[VERTEX_Y3] * scaleY);
	}
	minX = self.position.x + minX;
	minY = self.position.y + minY;
	maxX = self.position.x + maxX;
	maxY = self.position.y + maxY;
	return CGRectMake(minX, minY, maxX - minX, maxY - minY);
}

// --- Convenience methods for Skeleton_* functions.

- (void) updateWorldTransform {
	Skeleton_updateWorldTransform(_skeleton);
}

- (void) setToSetupPose {
	Skeleton_setToSetupPose(_skeleton);
}
- (void) setBonesToSetupPose {
	Skeleton_setBonesToSetupPose(_skeleton);
}
- (void) setSlotsToSetupPose {
	Skeleton_setSlotsToSetupPose(_skeleton);
}

- (Bone*) findBone:(NSString*)boneName {
	return Skeleton_findBone(_skeleton, [boneName UTF8String]);
}

- (Slot*) findSlot:(NSString*)slotName {
	return Skeleton_findSlot(_skeleton, [slotName UTF8String]);
}

- (bool) setSkin:(NSString*)skinName {
	return (bool)Skeleton_setSkinByName(_skeleton, skinName ? [skinName UTF8String] : 0);
}

- (Attachment*) getAttachment:(NSString*)slotName attachmentName:(NSString*)attachmentName {
	return Skeleton_getAttachmentForSlotName(_skeleton, [slotName UTF8String], [attachmentName UTF8String]);
}
- (bool) setAttachment:(NSString*)slotName attachmentName:(NSString*)attachmentName {
	return (bool)Skeleton_setAttachment(_skeleton, [slotName UTF8String], [attachmentName UTF8String]);
}

// --- CCBlendProtocol

- (void) setBlendFunc:(ccBlendFunc)func {
	self.blendFunc = func;
}

- (ccBlendFunc) blendFunc {
	return _blendFunc;
}

- (void) setOpacityModifyRGB:(BOOL)value {
	_premultipliedAlpha = value;
}

- (BOOL) doesOpacityModifyRGB {
	return _premultipliedAlpha;
}

@end
