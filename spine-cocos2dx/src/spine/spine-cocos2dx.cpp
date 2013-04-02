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

#include <spine/spine-cocos2dx.h>
#include <spine/extension.h>

USING_NS_CC;
namespace spine {

void _Cocos2dxAtlasPage_dispose (AtlasPage* page) {
	Cocos2dxAtlasPage* self = SUB_CAST(Cocos2dxAtlasPage, page);
	_AtlasPage_deinit(SUPER(self));

	CC_SAFE_RELEASE_NULL(self->texture);
	CC_SAFE_RELEASE_NULL(self->atlas);

	FREE(page);
}

AtlasPage* AtlasPage_create (const char* name) {
	Cocos2dxAtlasPage* self = NEW(Cocos2dxAtlasPage);
	_AtlasPage_init(SUPER(self), name);
	VTABLE(AtlasPage, self) ->dispose = _Cocos2dxAtlasPage_dispose;

	self->texture = CCTextureCache::sharedTextureCache()->addImage(name);
	self->texture->retain();
	self->atlas = CCTextureAtlas::createWithTexture(self->texture, 4);
	self->atlas->retain();

	return SUPER(self);
}

/**/

void _Cocos2dxSkeleton_dispose (Skeleton* self) {
	_Skeleton_deinit(self);
	FREE(self);
}

Skeleton* _Cocos2dxSkeleton_create (SkeletonData* data, CCSkeleton* node) {
	Cocos2dxSkeleton* self = NEW(Cocos2dxSkeleton);
	_Skeleton_init(SUPER(self), data);
	VTABLE(Skeleton, self) ->dispose = _Cocos2dxSkeleton_dispose;

	self->node = node;

	return SUPER(self);
}

CCSkeleton* CCSkeleton::create (SkeletonData* skeletonData, AnimationStateData* stateData) {
	CCSkeleton* node = new CCSkeleton(skeletonData, stateData);
	node->autorelease();
	return node;
}

CCSkeleton::CCSkeleton (SkeletonData *skeletonData, AnimationStateData *stateData) :
				debugSlots(false), debugBones(false) {
	skeleton = _Cocos2dxSkeleton_create(skeletonData, this);
	state = AnimationState_create(stateData);

	blendFunc.src = GL_ONE;
	blendFunc.dst = GL_ONE_MINUS_SRC_ALPHA;

	setShaderProgram(CCShaderCache::sharedShaderCache()->programForKey(kCCShader_PositionTextureColor));
	scheduleUpdate();
}

CCSkeleton::~CCSkeleton () {
	Skeleton_dispose(skeleton);
	AnimationState_dispose(state);
}

void CCSkeleton::update (float deltaTime) {
	Skeleton_update(skeleton, deltaTime);
	AnimationState_update(state, deltaTime * timeScale);
	AnimationState_apply(state, skeleton);
	Skeleton_updateWorldTransform(skeleton);
}

void CCSkeleton::draw () {
	CC_NODE_DRAW_SETUP();

	ccGLBlendFunc(blendFunc.src, blendFunc.dst);
	ccColor3B color = getColor();
	skeleton->r = color.r / (float)255;
	skeleton->g = color.g / (float)255;
	skeleton->b = color.b / (float)255;
	skeleton->a = getOpacity() / (float)255;

	quadCount = 0;
	for (int i = 0, n = skeleton->slotCount; i < n; i++)
		if (skeleton->slots[i]->attachment) Attachment_draw(skeleton->slots[i]->attachment, skeleton->slots[i]);
	if (atlas) atlas->drawNumberOfQuads(quadCount);

	if (debugSlots) {
		// Slots.
		ccDrawColor4B(0, 0, 255, 255);
		glLineWidth(1);
		CCPoint points[4];
		for (int i = 0, n = skeleton->slotCount; i < n; i++) {
			if (!skeleton->slots[i]->attachment) continue;
			ccV3F_C4B_T2F_Quad* quad = &((Cocos2dxRegionAttachment*)skeleton->slots[i]->attachment)->quad;
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

// CCBlendProtocol

ccBlendFunc CCSkeleton::getBlendFunc () {
    return blendFunc;
}

void CCSkeleton::setBlendFunc (ccBlendFunc blendFunc) {
    this->blendFunc = blendFunc;
}

/**/

void _Cocos2dxRegionAttachment_dispose (Attachment* self) {
	_RegionAttachment_deinit(SUB_CAST(RegionAttachment, self) );
	FREE(self);
}

void _Cocos2dxRegionAttachment_draw (Attachment* attachment, Slot* slot) {
	Cocos2dxRegionAttachment* self = SUB_CAST(Cocos2dxRegionAttachment, attachment);
	Cocos2dxSkeleton* skeleton = SUB_CAST(Cocos2dxSkeleton, slot->skeleton);

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

	// cocos2dx doesn't handle batching for us, so we'll just force a single texture per skeleton.
	skeleton->node->atlas = self->atlas;
	if (self->atlas->getCapacity() <= skeleton->node->quadCount) {
		if (!self->atlas->resizeCapacity(self->atlas->getCapacity() * 2)) return;
	}
	self->atlas->updateQuad(quad, skeleton->node->quadCount++);
}

RegionAttachment* RegionAttachment_create (const char* name, AtlasRegion* region) {
	Cocos2dxRegionAttachment* self = NEW(Cocos2dxRegionAttachment);
	_RegionAttachment_init(SUPER(self), name);
	VTABLE(Attachment, self) ->dispose = _Cocos2dxRegionAttachment_dispose;
	VTABLE(Attachment, self) ->draw = _Cocos2dxRegionAttachment_draw;

	Cocos2dxAtlasPage* page = SUB_CAST(Cocos2dxAtlasPage, region->page);
	self->atlas = page->atlas;
	const CCSize& size = page->texture->getContentSizeInPixels();
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

/**/

char* _Util_readFile (const char* path, int* length) {
	unsigned long size;
    char* data = reinterpret_cast<char*>(CCFileUtils::sharedFileUtils()->getFileData(
		CCFileUtils::sharedFileUtils()->fullPathForFilename(path).c_str(), "r", &size));
	*length = size;
	return data;
}

}
