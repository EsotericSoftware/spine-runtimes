/******************************************************************************
 * Spine Runtimes Software License
 * Version 2
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software, you may not (a) modify, translate, adapt or
 * otherwise create derivative works, improvements of the Software or develop
 * new applications using the Software or (b) remove, delete, alter or obscure
 * any trademarks or any copyright, trademark, patent or other intellectual
 * property or proprietary rights notices on or in the Software, including
 * any copy thereof. Redistributions in binary or source form must include
 * this license and terms. THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
 * TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/
//
// Added support for FFD by Wojciech Trzasko CodingFingers on 24.02.2014.
//

#import <spine/CCSkeleton.h>
#import <spine/spine-cocos2d-iphone.h>
#import <spine/MeshAttachment.h>

#import "cocos2d.h"

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
	return [[[self alloc] initWithData:skeletonData ownsSkeletonData:ownsSkeletonData] autorelease];
}

+ (id) skeletonWithFile:(NSString*)skeletonDataFile atlas:(Atlas*)atlas scale:(float)scale {
	return [[[self alloc] initWithFile:skeletonDataFile atlas:atlas scale:scale] autorelease];
}

+ (id) skeletonWithFile:(NSString*)skeletonDataFile atlasFile:(NSString*)atlasFile scale:(float)scale {
	return [[[self alloc] initWithFile:skeletonDataFile atlasFile:atlasFile scale:scale] autorelease];
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
    
    _renderPool = [[[CCRenderPool alloc] init] retain];
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
	json->scale = scale == 0 ? (1 / CC_CONTENT_SCALE_FACTOR()) : scale;
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
	json->scale = scale == 0 ? (1 / CC_CONTENT_SCALE_FACTOR()) : scale;
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
    
    [_renderPool release];
    
	[super dealloc];
}

- (void) update:(ccTime)deltaTime {
	Skeleton_update(_skeleton, deltaTime * _timeScale);
}

- (void) draw {

	CC_NODE_DRAW_SETUP();
    
    // Updating loop
	ccGLBlendFunc(_blendFunc.src, _blendFunc.dst);
	ccColor3B color = self.color;
	_skeleton->r = color.r / (float)255;
	_skeleton->g = color.g / (float)255;
	_skeleton->b = color.b / (float)255;
	_skeleton->a = self.opacity / (float)255;

	int additive = 0;
	CCTriangleTextureAtlas* textureAtlas = 0;
	ccV3F_C4B_T2F_Triangle triangle;
	triangle.a.vertices.z = 0;
	triangle.b.vertices.z = 0;
	triangle.c.vertices.z = 0;
    
    int quadTriangleIds[6] = {
        0, 1, 2,
        2, 3, 0
    };
        
    int prevTrianglesCount = 0;
	for (int i = 0, n = _skeleton->slotCount; i < n; i++)
    {
		Slot* slot = _skeleton->drawOrder[i];
        
		if (!slot->attachment) continue;
               
        if (slot->attachment->type == ATTACHMENT_REGION || slot->attachment->type == ATTACHMENT_MESH)
        {
            // Getting texture atlas
            CCTriangleTextureAtlas* regionTextureAtlas = NULL;
            if(slot->attachment->type == ATTACHMENT_REGION)
            {
                RegionAttachment* attachment = (RegionAttachment*)slot->attachment;
                regionTextureAtlas = (CCTriangleTextureAtlas*)[self getTextureAtlas:attachment];
            }
            else
            {
                MeshAttachment* attachment = (MeshAttachment*)slot->attachment;
                regionTextureAtlas = (CCTriangleTextureAtlas*)[self getTextureAtlasFromMeshAttachment:attachment];
            }
            
            // Handle additive blending
            if (slot->data->additiveBlending != additive)
            {
                if (textureAtlas)
                {
                    [_renderPool addRenderAtlasToPool:textureAtlas withStart:prevTrianglesCount stop:textureAtlas.currentTriangles blending:additive atIndex:_renderPool.length];
                    
                    prevTrianglesCount = textureAtlas.currentTriangles;
                }
                additive = !additive;
            }
            // Handling changing textureAtlas
            else if (regionTextureAtlas != textureAtlas && textureAtlas)
            {
                [_renderPool addRenderAtlasToPool:textureAtlas withStart:prevTrianglesCount stop:textureAtlas.currentTriangles blending:additive atIndex:_renderPool.length];
                
                prevTrianglesCount = regionTextureAtlas.currentTriangles;
            }
            
            textureAtlas = regionTextureAtlas;
            
            if(slot->attachment->type == ATTACHMENT_REGION)
            {
                RegionAttachment* attachment = (RegionAttachment*)slot->attachment;
                
                // If no more room for triangles
                if (textureAtlas.totalTriangles + 2 > textureAtlas.capacity)
                {
                    // Resize
                    if(textureAtlas.capacity < n * 2)
                    {
                        // Assuming only region attachments
                        [textureAtlas resizeCapacity:n * 2];
                    }
                    else
                    {
                        // Assuming that were ffd
                        [textureAtlas resizeCapacity:(textureAtlas.totalTriangles + n)];
                    }
                }
                
                ccV3F_C4B_T2F vertices[4];
                float verticesPos[8];
                unsigned int startVerticle = textureAtlas.totalVertices;
                
                RegionAttachment_computeWorldVertices(attachment, slot->skeleton->x, slot->skeleton->y, slot->bone, verticesPos);
                
                RegionAttachment_updateVertices(attachment, slot, vertices, _premultipliedAlpha, verticesPos);
                [textureAtlas updateVertices:vertices atIndex:textureAtlas.totalVertices length:4];
                
                textureAtlas.currentTriangles += 2;
                [textureAtlas updateTrianglesIndices:quadTriangleIds length:6 withOffset:startVerticle];
            }
            else
            {
                MeshAttachment* attachment = (MeshAttachment*)slot->attachment;
                
                unsigned int verticesCount = attachment->verticesLength / 2;
                unsigned int trianglesCount = attachment->trianglesIndicesLength / 3;
    
                // Resizing when buffer is too small.
                if((trianglesCount + textureAtlas.totalTriangles) > textureAtlas.capacity)
                {
                    int multiplier = ceil((float)(trianglesCount + textureAtlas.totalTriangles) / (float)textureAtlas.capacity);
                    [textureAtlas resizeCapacity:(textureAtlas.capacity * multiplier)];
                }
                
                MeshAttachment_computeWorldVertices(attachment, slot->skeleton->x, slot->skeleton->y, slot->bone);
                
                unsigned int initVertexCount = textureAtlas.totalVertices;
                ccV3F_C4B_T2F vertices[verticesCount];
                
                MeshAttachment_updateVertices(attachment, slot, vertices, _premultipliedAlpha);
                [textureAtlas updateVertices:vertices atIndex:textureAtlas.totalVertices length:verticesCount];
                
                textureAtlas.currentTriangles += trianglesCount;
                [textureAtlas updateTrianglesIndices:attachment->trianglesIndices length:attachment->trianglesIndicesLength withOffset:initVertexCount];
            }
        }
	}
    
    // Adding rendering info for last vertices
    [_renderPool addRenderAtlasToPool:textureAtlas withStart:prevTrianglesCount stop:textureAtlas.currentTriangles blending:additive atIndex:_renderPool.length];
    
    // Render loop
    // Used to minimize glBufferData calls
    CCTriangleTextureAtlas* prevTextureAtlas = nil;
    BOOL blending = NO;
    
    for (int i = 0; i < _renderPool.length; i++)
    {
        ccRenderInfoStructure* poolData = [_renderPool pool];
        
        NSInteger startIndex    = poolData[i].startIndex;
        NSInteger stopIndex     = poolData[i].stopIndex;
        CCTriangleTextureAtlas* renderTextureAtlas = poolData[i].textureAtlas;
        
        if(renderTextureAtlas != prevTextureAtlas)
        {
            // Transfer data for actual textureAtlas
            [renderTextureAtlas transferBuffers];
        }
        
        // Saving actual textureAtlas
        prevTextureAtlas = renderTextureAtlas;
        
        if(blending != poolData[i].blending)
        {
            blending = !blending;
            ccGLBlendFunc(_blendFunc.src, blending ? GL_ONE : _blendFunc.dst);
        }
        
        // Rendering part of object
        [renderTextureAtlas drawTriangles:(stopIndex - startIndex) fromIndex:startIndex];
    }
    
    // Clearing loop
    for (int i = 0; i < _renderPool.length; i++)
    {
        ccRenderInfoStructure* renderInfo = [_renderPool pool];
        CCTriangleTextureAtlas* atlas = renderInfo[i].textureAtlas;
        
        [atlas removeAllVertices];
        [atlas removeAllTriangles];
        atlas.currentTriangles = 0;
    }
    [_renderPool removeAllInfo];
    
	if (_debugSlots) {
		// Slots.
		ccDrawColor4B(0, 0, 255, 255);
		glLineWidth(1);
		CGPoint points[4];
		ccV3F_C4B_T2F_Quad quad;
		for (int i = 0, n = _skeleton->slotCount; i < n; i++) {
			Slot* slot = _skeleton->drawOrder[i];
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

- (CCTextureAtlas*) getTextureAtlasFromMeshAttachment:(MeshAttachment*)meshAttachment {
	return (CCTextureAtlas*)((AtlasRegion*)meshAttachment->rendererObject)->page->rendererObject;
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
		RegionAttachment_computeWorldVertices(attachment, slot->skeleton->x, slot->skeleton->y, slot->bone, vertices);
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
