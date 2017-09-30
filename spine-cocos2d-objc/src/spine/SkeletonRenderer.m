/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#import <spine/SkeletonRenderer.h>
#import <spine/spine-cocos2d-objc.h>
#import <spine/extension.h>
#import <spine/GLUtils.h>
#import "CCDrawNode.h"

static unsigned short quadTriangles[6] = {0, 1, 2, 2, 3, 0};
static spTwoColorBatcher* batcher = 0;
static spMesh* mesh = 0;
static bool handlerQueued = false;

@interface SkeletonRenderer (Private)
- (void) initialize:(spSkeletonData*)skeletonData ownsSkeletonData:(bool)ownsSkeletonData;
@end

@implementation SkeletonRenderer

@synthesize skeleton = _skeleton;
@synthesize rootBone = _rootBone;
@synthesize twoColorTint = _twoColorTint;
@synthesize debugSlots = _debugSlots;
@synthesize debugBones = _debugBones;
@synthesize effect = _effect;

+ (id) skeletonWithData:(spSkeletonData*)skeletonData ownsSkeletonData:(bool)ownsSkeletonData {
	return [[[self alloc] initWithData:skeletonData ownsSkeletonData:ownsSkeletonData] autorelease];
}

+ (id) skeletonWithFile:(NSString*)skeletonDataFile atlas:(spAtlas*)atlas scale:(float)scale {
	return [[[self alloc] initWithFile:skeletonDataFile atlas:atlas scale:scale] autorelease];
}

+ (id) skeletonWithFile:(NSString*)skeletonDataFile atlasFile:(NSString*)atlasFile scale:(float)scale {
	return [[[self alloc] initWithFile:skeletonDataFile atlasFile:atlasFile scale:scale] autorelease];
}

- (void) initialize:(spSkeletonData*)skeletonData ownsSkeletonData:(bool)ownsSkeletonData {
	if (!batcher) {
		batcher = spTwoColorBatcher_create();
		mesh = spMesh_create(64000, 32000);
	}
	
	_ownsSkeletonData = ownsSkeletonData;

	_worldVertices = MALLOC(float, 1000); // Max number of vertices per mesh.

	_skeleton = spSkeleton_create(skeletonData);
	_rootBone = _skeleton->bones[0];

	_blendFunc.src = GL_ONE;
	_blendFunc.dst = GL_ONE_MINUS_SRC_ALPHA;
	_drawNode = [[CCDrawNode alloc] init];
	[_drawNode setBlendMode: [CCBlendMode premultipliedAlphaMode]];
	[self addChild:_drawNode];
	
	[self setShader:[CCShader positionTextureColorShader]];

	_premultipliedAlpha = true;
	screenMode = [CCBlendMode blendModeWithOptions:@{
		CCBlendFuncSrcColor: @(GL_ONE),
		CCBlendFuncDstColor: @(GL_ONE_MINUS_SRC_COLOR)}
	];
	
	_clipper = spSkeletonClipping_create();
	_effect = 0;
}

- (id) initWithData:(spSkeletonData*)skeletonData ownsSkeletonData:(bool)ownsSkeletonData {
	NSAssert(skeletonData, @"skeletonData cannot be null.");

	self = [super init];
	if (!self) return nil;

	[self initialize:skeletonData ownsSkeletonData:ownsSkeletonData];

	return self;
}

- (id) initWithFile:(NSString*)skeletonDataFile atlas:(spAtlas*)atlas scale:(float)scale {
	self = [super init];
	if (!self) return nil;

	spSkeletonJson* json = spSkeletonJson_create(atlas);
	json->scale = scale;
    spSkeletonData* skeletonData = nil;
    
    @synchronized(self.class) {
        spSkeletonJson_readSkeletonDataFile(json, [skeletonDataFile UTF8String]);
    }
	NSAssert(skeletonData, ([NSString stringWithFormat:@"Error reading skeleton data file: %@\nError: %s", skeletonDataFile, json->error]));
	spSkeletonJson_dispose(json);
	if (!skeletonData) return 0;

	[self initialize:skeletonData ownsSkeletonData:YES];

	return self;
}

- (id) initWithFile:(NSString*)skeletonDataFile atlasFile:(NSString*)atlasFile scale:(float)scale {
	self = [super init];
	if (!self) return nil;

    @synchronized(self.class) {
        _atlas = spAtlas_createFromFile([atlasFile UTF8String], 0);
    }
	NSAssert(_atlas, ([NSString stringWithFormat:@"Error reading atlas file: %@", atlasFile]));
	if (!_atlas) return 0;

	spSkeletonJson* json = spSkeletonJson_create(_atlas);
	json->scale = scale;
    spSkeletonData* skeletonData;
    
    @synchronized(self.class) {
        skeletonData = spSkeletonJson_readSkeletonDataFile(json, [skeletonDataFile UTF8String]);
    }
	NSAssert(skeletonData, ([NSString stringWithFormat:@"Error reading skeleton data file: %@\nError: %s", skeletonDataFile, json->error]));
	spSkeletonJson_dispose(json);
	if (!skeletonData) return 0;

	[self initialize:skeletonData ownsSkeletonData:YES];

	return self;
}

- (void) dealloc {
	if (_ownsSkeletonData) spSkeletonData_dispose(_skeleton->data);
	if (_atlas) spAtlas_dispose(_atlas);
	spSkeleton_dispose(_skeleton);
	FREE(_worldVertices);
	spSkeletonClipping_dispose(_clipper);
	[super dealloc];
}

-(void)draw:(CCRenderer *)renderer transform:(const GLKMatrix4 *)transform {
	// FIXME we need to clear the mesh parts at the end of the frame
	// there's no general event mechanism to get notified on end frame
	// that doesn't need to be re-added every frame. This is a poor man
	// notification system that may break if the block is called on a
	// separate thread.
	if (!handlerQueued) {
		[[CCDirector sharedDirector] addFrameCompletionHandler: ^{			
			spMesh_clearParts(mesh);
			handlerQueued = false;
		}];
		handlerQueued = true;
	}
	
	if (_effect) _effect->begin(_effect, _skeleton);
	
	CCColor* nodeColor = self.color;
	_skeleton->color.r = nodeColor.red;
	_skeleton->color.g = nodeColor.green;
	_skeleton->color.b = nodeColor.blue;
	_skeleton->color.a = self.displayedOpacity;

	int blendMode = -1;
	uint32_t srcBlend = GL_SRC_ALPHA;
	uint32_t dstBlend = GL_ONE_MINUS_SRC_ALPHA;
	float* uvs = 0;
	float* vertices = _worldVertices;
	int verticesCount = 0;
	unsigned short* triangles = 0;
	int trianglesCount = 0;
	float r = 0, g = 0, b = 0, a = 0;
	float dr = 0, dg = 0, db = 0, da = _premultipliedAlpha ? 1 : 0;
	for (int i = 0, n = _skeleton->slotsCount; i < n; i++) {
		spSlot* slot = _skeleton->drawOrder[i];
		if (!slot->attachment) continue;
		CCTexture *texture = 0;
		switch (slot->attachment->type) {
		case SP_ATTACHMENT_REGION: {
			spRegionAttachment* attachment = (spRegionAttachment*)slot->attachment;
			spRegionAttachment_computeWorldVertices(attachment, slot->bone, vertices, 0, 2);
			texture = [self getTextureForRegion:attachment];
			uvs = attachment->uvs;
			verticesCount = 8;
			triangles = quadTriangles;
			trianglesCount = 6;
			r = attachment->color.r;
			g = attachment->color.g;
			b = attachment->color.b;
			a = attachment->color.a;
			break;
		}
		case SP_ATTACHMENT_MESH: {
			spMeshAttachment* attachment = (spMeshAttachment*)slot->attachment;
			spVertexAttachment_computeWorldVertices(SUPER(attachment), slot, 0, attachment->super.worldVerticesLength, vertices, 0, 2);
			texture = [self getTextureForMesh:attachment];
			uvs = attachment->uvs;
			verticesCount = attachment->super.worldVerticesLength;
			triangles = attachment->triangles;
			trianglesCount = attachment->trianglesCount;
			r = attachment->color.r;
			g = attachment->color.g;
			b = attachment->color.b;
			a = attachment->color.a;
			break;
		}
		case SP_ATTACHMENT_CLIPPING: {
			spClippingAttachment* clip = (spClippingAttachment*)slot->attachment;
			spSkeletonClipping_clipStart(_clipper, slot, clip);
		}
		default: ;
		}
		
		if (texture) {
			if (slot->data->blendMode != blendMode) {
				blendMode = slot->data->blendMode;
				switch (slot->data->blendMode) {
				case SP_BLEND_MODE_ADDITIVE:
					[self setBlendMode:[CCBlendMode addMode]];
					srcBlend = !_premultipliedAlpha ? GL_SRC_ALPHA : GL_ONE;
					dstBlend = GL_ONE;
					break;
				case SP_BLEND_MODE_MULTIPLY:
					[self setBlendMode:[CCBlendMode multiplyMode]];
					srcBlend = GL_DST_COLOR;
					dstBlend = GL_ONE_MINUS_SRC_ALPHA;
					break;
				case SP_BLEND_MODE_SCREEN:
					[self setBlendMode:screenMode];
					srcBlend = GL_ONE;
					dstBlend = GL_ONE_MINUS_SRC_COLOR;
					break;
				default:
					[self setBlendMode:_premultipliedAlpha ? [CCBlendMode premultipliedAlphaMode] : [CCBlendMode alphaMode]];
					srcBlend = !_premultipliedAlpha ? GL_SRC_ALPHA : GL_ONE;
					dstBlend = GL_ONE_MINUS_SRC_ALPHA;
				}
			}
			if (_premultipliedAlpha) {
				a *= _skeleton->color.a * slot->color.a;
				r *= _skeleton->color.r * slot->color.r * a;
				g *= _skeleton->color.g * slot->color.g * a;
				b *= _skeleton->color.b * slot->color.b * a;
			} else {
				a *= _skeleton->color.a * slot->color.a;
				r *= _skeleton->color.r * slot->color.r;
				g *= _skeleton->color.g * slot->color.g;
				b *= _skeleton->color.b * slot->color.b;
			}
			self.texture = texture;
			CGSize size = texture.contentSize;
			GLKVector2 center = GLKVector2Make(size.width / 2.0, size.height / 2.0);
			GLKVector2 extents = GLKVector2Make(size.width / 2.0, size.height / 2.0);
			if (_skipVisibilityCheck || CCRenderCheckVisbility(transform, center, extents)) {
				
				if (spSkeletonClipping_isClipping(_clipper)) {
					spSkeletonClipping_clipTriangles(_clipper, vertices, verticesCount, triangles, trianglesCount, uvs, 2);
					vertices = _clipper->clippedVertices->items;
					verticesCount = _clipper->clippedVertices->size;
					uvs = _clipper->clippedUVs->items;
					triangles = _clipper->clippedTriangles->items;
					trianglesCount = _clipper->clippedTriangles->size;
				}
				
				if (trianglesCount > 0) {
					if (!self.twoColorTint) {
						CCRenderBuffer buffer = [renderer enqueueTriangles:(trianglesCount / 3) andVertexes:verticesCount withState:self.renderState globalSortOrder:0];
						for (int i = 0; i * 2 < verticesCount; ++i) {
							CCVertex vertex;
							vertex.position = GLKVector4Make(vertices[i * 2], vertices[i * 2 + 1], 0.0, 1.0);
							vertex.color = GLKVector4Make(r, g, b, a);
							vertex.texCoord1 = GLKVector2Make(uvs[i * 2], 1 - uvs[i * 2 + 1]);
							if (_effect) {
								spColor light;
								spColor dark;
								light.r = r;
								light.g = g;
								light.b = b;
								light.a = a;
								dark.r = dark.g = dark.b = dark.a = 0;
								_effect->transform(_effect, &vertex.position.x, &vertex.position.y, &vertex.texCoord1.s, &vertex.texCoord1.t, &light, &dark);
								vertex.color.r = light.r;
								vertex.color.g = light.g;
								vertex.color.b = light.b;
								vertex.color.a = light.a;
							}
							CCRenderBufferSetVertex(buffer, i, CCVertexApplyTransform(vertex, transform));
						}
						for (int j = 0; j * 3 < trianglesCount; ++j) {
							CCRenderBufferSetTriangle(buffer, j, triangles[j * 3], triangles[j * 3 + 1], triangles[j * 3 + 2]);
						}
					} else {
						if (slot->darkColor) {
							dr = slot->darkColor->r;
							dg = slot->darkColor->g;
							db = slot->darkColor->b;
						} else {
							dr = dg = db = 0;
						}
						
						spMeshPart meshPart;
						spMesh_allocatePart(mesh, &meshPart, verticesCount / 2, trianglesCount, self.texture.name, srcBlend, dstBlend);
						
						spVertex* verts = &meshPart.mesh->vertices[meshPart.startVertex];
						unsigned short* indices = &meshPart.mesh->indices[meshPart.startIndex];
						
						if (_effect) {
							spColor light;
							light.r = r;
							light.g = g;
							light.b = b;
							light.a = a;
							spColor dark;
							dark.r = dr;
							dark.g = dg;
							dark.b = db;
							dark.a = da;
							for (int i = 0; i * 2 < verticesCount; i++, verts++) {
								spColor lightCopy = light;
								spColor darkCopy = dark;
								
								CCVertex vertex;
								vertex.position = GLKVector4Make(vertices[i * 2], vertices[i * 2 + 1], 0.0, 1.0);
								verts->u = uvs[i * 2];
								verts->v = 1 - uvs[i * 2 + 1];
								_effect->transform(_effect, &vertex.position.x, &vertex.position.y, &verts->u, &verts->v, &lightCopy, &darkCopy);
								
								vertex = CCVertexApplyTransform(vertex, transform);
								verts->x = vertex.position.x;
								verts->y = vertex.position.y;
								verts->z = vertex.position.z;
								verts->w = vertex.position.w;
								verts->color = ((unsigned short)(lightCopy.r * 255))| ((unsigned short)(lightCopy.g * 255)) << 8 | ((unsigned short)(lightCopy.b * 255)) <<16 | ((unsigned short)(lightCopy.a * 255)) << 24;
								verts->color2 = ((unsigned short)(darkCopy.r * 255)) | ((unsigned short)(darkCopy.g * 255)) << 8 | ((unsigned short)(darkCopy.b * 255)) << 16 | ((unsigned short)(darkCopy.a * 255)) << 24;
								
							}
						} else {
							for (int i = 0; i * 2 < verticesCount; i++, verts++) {
								CCVertex vertex;
								vertex.position = GLKVector4Make(vertices[i * 2], vertices[i * 2 + 1], 0.0, 1.0);
								vertex = CCVertexApplyTransform(vertex, transform);
								verts->x = vertex.position.x;
								verts->y = vertex.position.y;
								verts->z = vertex.position.z;
								verts->w = vertex.position.w;
								verts->color = ((unsigned short)(r * 255))| ((unsigned short)(g * 255)) << 8 | ((unsigned short)(b * 255)) <<16 | ((unsigned short)(a * 255)) << 24;
								verts->color2 = ((unsigned short)(dr * 255)) | ((unsigned short)(dg * 255)) << 8 | ((unsigned short)(db * 255)) << 16 | ((unsigned short)(da * 255)) << 24;
								verts->u = uvs[i * 2];
								verts->v = 1 - uvs[i * 2 + 1];
							}
						}
						
						for (int j = 0; j < trianglesCount; j++, indices++) {
							*indices = triangles[j];
						}
						
						[renderer enqueueBlock:^{
							spTwoColorBatcher_add(batcher, meshPart);
						} globalSortOrder:0 debugLabel: nil threadSafe: false];
					}
				}
			}
		}
		spSkeletonClipping_clipEnd(_clipper, slot);
	}
	spSkeletonClipping_clipEnd2(_clipper);
	
	if (self.twoColorTint) {
		[renderer enqueueBlock:^{
			spTwoColorBatcher_flush(batcher);
		} globalSortOrder:0 debugLabel: nil threadSafe: false];
	}
	
	[_drawNode clear];
	if (_debugSlots) {
		// Slots.
		CGPoint points[4];
		for (int i = 0, n = _skeleton->slotsCount; i < n; i++) {
			spSlot* slot = _skeleton->drawOrder[i];
			if (!slot->attachment || slot->attachment->type != SP_ATTACHMENT_REGION) continue;
			spRegionAttachment* attachment = (spRegionAttachment*)slot->attachment;
			spRegionAttachment_computeWorldVertices(attachment, slot->bone, _worldVertices, 0, 2);
			points[0] = ccp(_worldVertices[0], _worldVertices[1]);
			points[1] = ccp(_worldVertices[2], _worldVertices[3]);
			points[2] = ccp(_worldVertices[4], _worldVertices[5]);
			points[3] = ccp(_worldVertices[6], _worldVertices[7]);
			[_drawNode drawPolyWithVerts:points count:4 fillColor:[CCColor clearColor] borderWidth:1 borderColor:[CCColor blueColor]];
		}
	}
	if (_debugBones) {
		// Bone lengths.
		for (int i = 0, n = _skeleton->bonesCount; i < n; i++) {
			spBone *bone = _skeleton->bones[i];
			float x = bone->data->length * bone->a + bone->worldX;
			float y = bone->data->length * bone->c + bone->worldY;
			[_drawNode drawSegmentFrom:ccp(bone->worldX, bone->worldY) to: ccp(x, y)radius:2 color:[CCColor redColor]];
		}
		
		// Bone origins.
		for (int i = 0, n = _skeleton->bonesCount; i < n; i++) {
			spBone *bone = _skeleton->bones[i];
			[_drawNode drawDot:ccp(bone->worldX, bone->worldY) radius:4 color:[CCColor greenColor]];
			if (i == 0) [_drawNode drawDot:ccp(bone->worldX, bone->worldY) radius:4 color:[CCColor blueColor]];
		}
	}
	
	if (_effect) _effect->end(_effect);
}

- (CCTexture*) getTextureForRegion:(spRegionAttachment*)attachment {
	return (CCTexture*)((spAtlasRegion*)attachment->rendererObject)->page->rendererObject;
}

- (CCTexture*) getTextureForMesh:(spMeshAttachment*)attachment {
	return (CCTexture*)((spAtlasRegion*)attachment->rendererObject)->page->rendererObject;
}

- (CGRect) boundingBox {
	float minX = FLT_MAX, minY = FLT_MAX, maxX = FLT_MIN, maxY = FLT_MIN;
	float scaleX = self.scaleX, scaleY = self.scaleY;
	for (int i = 0; i < _skeleton->slotsCount; ++i) {
		spSlot* slot = _skeleton->slots[i];
		if (!slot->attachment) continue;
		int verticesCount;
		if (slot->attachment->type == SP_ATTACHMENT_REGION) {
			spRegionAttachment* attachment = (spRegionAttachment*)slot->attachment;
			spRegionAttachment_computeWorldVertices(attachment, slot->bone, _worldVertices, 0, 2);
			verticesCount = 8;
		} else if (slot->attachment->type == SP_ATTACHMENT_MESH) {
			spMeshAttachment* mesh = (spMeshAttachment*)slot->attachment;
			spVertexAttachment_computeWorldVertices(SUPER(mesh), slot, 0, mesh->super.worldVerticesLength, _worldVertices, 0, 2);

			verticesCount = mesh->super.worldVerticesLength;
		} else
			continue;
		for (int ii = 0; ii < verticesCount; ii += 2) {
			float x = _worldVertices[ii] * scaleX, y = _worldVertices[ii + 1] * scaleY;
			minX = fmin(minX, x);
			minY = fmin(minY, y);
			maxX = fmax(maxX, x);
			maxY = fmax(maxY, y);
		}
	}
	minX = self.position.x + minX;
	minY = self.position.y + minY;
	maxX = self.position.x + maxX;
	maxY = self.position.y + maxY;
	return CGRectMake(minX, minY, maxX - minX, maxY - minY);
}

// --- Convenience methods for Skeleton_* functions.

- (void) updateWorldTransform {
	spSkeleton_updateWorldTransform(_skeleton);
}

- (void) setToSetupPose {
	spSkeleton_setToSetupPose(_skeleton);
}
- (void) setBonesToSetupPose {
	spSkeleton_setBonesToSetupPose(_skeleton);
}
- (void) setSlotsToSetupPose {
	spSkeleton_setSlotsToSetupPose(_skeleton);
}

- (spBone*) findBone:(NSString*)boneName {
	return spSkeleton_findBone(_skeleton, [boneName UTF8String]);
}

- (spSlot*) findSlot:(NSString*)slotName {
	return spSkeleton_findSlot(_skeleton, [slotName UTF8String]);
}

- (bool) setSkin:(NSString*)skinName {
	return (bool)spSkeleton_setSkinByName(_skeleton, skinName ? [skinName UTF8String] : 0);
}

- (spAttachment*) getAttachment:(NSString*)slotName attachmentName:(NSString*)attachmentName {
	return spSkeleton_getAttachmentForSlotName(_skeleton, [slotName UTF8String], [attachmentName UTF8String]);
}
- (bool) setAttachment:(NSString*)slotName attachmentName:(NSString*)attachmentName {
	return (bool)spSkeleton_setAttachment(_skeleton, [slotName UTF8String], [attachmentName UTF8String]);
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
