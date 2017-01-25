/******************************************************************************
 * Spine Runtime Software License - Version 1.0
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Single User License or Spine Professional License must be
 *    purchased from Esoteric Software and the license must remain valid:
 *    http://esotericsoftware.com/
 * 2. Redistributions of source code must retain this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer.
 * 3. Redistributions in binary form must reproduce this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer, in the documentation and/or other materials provided with the
 *    distribution.
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
 *****************************************************************************/

#include <spine/CCSkeleton.h>
#include <spine/spine-cocos2dx.h>

USING_NS_CC;
using std::min;
using std::max;

namespace spine {

CCSkeleton* CCSkeleton::createWithData (spSkeletonData* skeletonData, bool ownsSkeletonData) {
	CCSkeleton* node = new CCSkeleton(skeletonData, ownsSkeletonData);
	node->autorelease();
	return node;
}

CCSkeleton* CCSkeleton::createWithFile (const char* skeletonDataFile, spAtlas* atlas, float scale) {
	CCSkeleton* node = new CCSkeleton(skeletonDataFile, atlas, scale);
	node->autorelease();
	return node;
}

CCSkeleton* CCSkeleton::createWithFile (const char* skeletonDataFile, const char* atlasFile, float scale) {
	CCSkeleton* node = new CCSkeleton(skeletonDataFile, atlasFile, scale);
	node->autorelease();
	return node;
}

void CCSkeleton::initialize () {
	atlas = 0;
	debugSlots = false;
	debugBones = false;
	timeScale = 1;

	blendFunc.src = GL_ONE;
	blendFunc.dst = GL_ONE_MINUS_SRC_ALPHA;
	setOpacityModifyRGB(true);

	setShaderProgram(CCShaderCache::getInstance()->getProgram(GLProgram::SHADER_NAME_POSITION_TEXTURE_COLOR));
	scheduleUpdate();
}

void CCSkeleton::setSkeletonData (spSkeletonData *skeletonData, bool ownsSkeletonData) {
	skeleton = spSkeleton_create(skeletonData);
	rootBone = skeleton->bones[0];
	this->ownsSkeletonData = ownsSkeletonData;	
}

CCSkeleton::CCSkeleton () {
	initialize();
}

CCSkeleton::CCSkeleton (spSkeletonData *skeletonData, bool ownsSkeletonData) {
	initialize();

	setSkeletonData(skeletonData, ownsSkeletonData);
}

CCSkeleton::CCSkeleton (const char* skeletonDataFile, spAtlas* atlas, float scale) {
	initialize();

	spSkeletonJson* json = spSkeletonJson_create(atlas);
	json->scale = scale;
	spSkeletonData* skeletonData = spSkeletonJson_readSkeletonDataFile(json, skeletonDataFile);
	CCAssert(skeletonData, json->error ? json->error : "Error reading skeleton data.");
	spSkeletonJson_dispose(json);

	setSkeletonData(skeletonData, true);
}

CCSkeleton::CCSkeleton (const char* skeletonDataFile, const char* atlasFile, float scale) {
	initialize();

	atlas = spAtlas_readAtlasFile(atlasFile);
	CCAssert(atlas, "Error reading atlas file.");

	spSkeletonJson* json = spSkeletonJson_create(atlas);
	json->scale = scale;
	spSkeletonData* skeletonData = spSkeletonJson_readSkeletonDataFile(json, skeletonDataFile);
	CCAssert(skeletonData, json->error ? json->error : "Error reading skeleton data file.");
	spSkeletonJson_dispose(json);

	setSkeletonData(skeletonData, true);
}

CCSkeleton::~CCSkeleton () {
	if (ownsSkeletonData) spSkeletonData_dispose(skeleton->data);
	if (atlas) spAtlas_dispose(atlas);
	spSkeleton_dispose(skeleton);
}

void CCSkeleton::update (float deltaTime) {
	spSkeleton_update(skeleton, deltaTime * timeScale);
}

void CCSkeleton::draw () {
	CC_NODE_DRAW_SETUP();

	GL::blendFunc(blendFunc.src, blendFunc.dst);
	Color3B color = getColor();
	skeleton->r = color.r / (float)255;
	skeleton->g = color.g / (float)255;
	skeleton->b = color.b / (float)255;
	skeleton->a = getOpacity() / (float)255;
	if (premultipliedAlpha) {
		skeleton->r *= skeleton->a;
		skeleton->g *= skeleton->a;
		skeleton->b *= skeleton->a;
	}

    int additive = 0;
	TextureAtlas* textureAtlas = 0;
	V3F_C4B_T2F_Quad quad;
	quad.tl.vertices.z = 0;
	quad.tr.vertices.z = 0;
	quad.bl.vertices.z = 0;
	quad.br.vertices.z = 0;
	for (int i = 0, n = skeleton->slotCount; i < n; i++) {
		spSlot* slot = skeleton->drawOrder[i];
		if (!slot->attachment || slot->attachment->type != ATTACHMENT_REGION) continue;
		spRegionAttachment* attachment = (spRegionAttachment*)slot->attachment;
        TextureAtlas* regionTextureAtlas = getTextureAtlas(attachment);

        if (slot->data->additiveBlending != additive) {
            if (textureAtlas) {
                textureAtlas->drawQuads();
                textureAtlas->removeAllQuads();
            }
            additive = !additive;
            GL::blendFunc(blendFunc.src, additive ? GL_ONE : blendFunc.dst);
        } else if (regionTextureAtlas != textureAtlas && textureAtlas) {
            textureAtlas->drawQuads();
            textureAtlas->removeAllQuads();
        }
        textureAtlas = regionTextureAtlas;

		int quadCount = textureAtlas->getTotalQuads();
		if (textureAtlas->getCapacity() == quadCount) {
			textureAtlas->drawQuads();
			textureAtlas->removeAllQuads();
			if (!textureAtlas->resizeCapacity(textureAtlas->getCapacity() * 2)) return;
		}

		spRegionAttachment_updateQuad(attachment, slot, &quad, premultipliedAlpha);
		textureAtlas->updateQuad(&quad, quadCount);
	}
	if (textureAtlas) {
		textureAtlas->drawQuads();
		textureAtlas->removeAllQuads();
	}

	if (debugSlots) {
		// Slots.
		DrawPrimitives::setDrawColor4B(0, 0, 255, 255);
		glLineWidth(1);
		Point points[4];
		V3F_C4B_T2F_Quad quad;
		for (int i = 0, n = skeleton->slotCount; i < n; i++) {
			spSlot* slot = skeleton->drawOrder[i];
			if (!slot->attachment || slot->attachment->type != ATTACHMENT_REGION) continue;
			spRegionAttachment* attachment = (spRegionAttachment*)slot->attachment;
			spRegionAttachment_updateQuad(attachment, slot, &quad);
			points[0] = Point(quad.bl.vertices.x, quad.bl.vertices.y);
			points[1] = Point(quad.br.vertices.x, quad.br.vertices.y);
			points[2] = Point(quad.tr.vertices.x, quad.tr.vertices.y);
			points[3] = Point(quad.tl.vertices.x, quad.tl.vertices.y);
			DrawPrimitives::drawPoly(points, 4, true);
		}
	}
	if (debugBones) {
		// Bone lengths.
		glLineWidth(2);
		DrawPrimitives::setDrawColor4B(255, 0, 0, 255);
		for (int i = 0, n = skeleton->boneCount; i < n; i++) {
			spBone *bone = skeleton->bones[i];
			float x = bone->data->length * bone->m00 + bone->worldX;
			float y = bone->data->length * bone->m10 + bone->worldY;
            DrawPrimitives::drawLine(Point(bone->worldX, bone->worldY), Point(x, y));
		}
		// Bone origins.
		DrawPrimitives::setPointSize(4);
		DrawPrimitives::setDrawColor4B(0, 0, 255, 255); // Root bone is blue.
		for (int i = 0, n = skeleton->boneCount; i < n; i++) {
			spBone *bone = skeleton->bones[i];
            DrawPrimitives::drawPoint(Point(bone->worldX, bone->worldY));
			if (i == 0) DrawPrimitives::setDrawColor4B(0, 255, 0, 255);
		}
	}
}

TextureAtlas* CCSkeleton::getTextureAtlas (spRegionAttachment* regionAttachment) const {
	return (TextureAtlas*)((spAtlasRegion*)regionAttachment->rendererObject)->page->rendererObject;
}

Rect CCSkeleton::getBoundingBox() const {
	float minX = FLT_MAX, minY = FLT_MAX, maxX = FLT_MIN, maxY = FLT_MIN;
	float scaleX = getScaleX();
	float scaleY = getScaleY();
	float vertices[8];
	for (int i = 0; i < skeleton->slotCount; ++i) {
		spSlot* slot = skeleton->slots[i];
		if (!slot->attachment || slot->attachment->type != ATTACHMENT_REGION) continue;
		spRegionAttachment* attachment = (spRegionAttachment*)slot->attachment;
		spRegionAttachment_computeWorldVertices(attachment, slot->skeleton->x, slot->skeleton->y, slot->bone, vertices);
		minX = min(minX, vertices[VERTEX_X1] * scaleX);
		minY = min(minY, vertices[VERTEX_Y1] * scaleY);
		maxX = max(maxX, vertices[VERTEX_X1] * scaleX);
		maxY = max(maxY, vertices[VERTEX_Y1] * scaleY);
		minX = min(minX, vertices[VERTEX_X4] * scaleX);
		minY = min(minY, vertices[VERTEX_Y4] * scaleY);
		maxX = max(maxX, vertices[VERTEX_X4] * scaleX);
		maxY = max(maxY, vertices[VERTEX_Y4] * scaleY);
		minX = min(minX, vertices[VERTEX_X2] * scaleX);
		minY = min(minY, vertices[VERTEX_Y2] * scaleY);
		maxX = max(maxX, vertices[VERTEX_X2] * scaleX);
		maxY = max(maxY, vertices[VERTEX_Y2] * scaleY);
		minX = min(minX, vertices[VERTEX_X3] * scaleX);
		minY = min(minY, vertices[VERTEX_Y3] * scaleY);
		maxX = max(maxX, vertices[VERTEX_X3] * scaleX);
		maxY = max(maxY, vertices[VERTEX_Y3] * scaleY);
	}
	Point position = getPosition();
	return Rect(position.x + minX, position.y + minY, maxX - minX, maxY - minY);
}

// --- Convenience methods for Skeleton_* functions.

void CCSkeleton::updateWorldTransform () {
	spSkeleton_updateWorldTransform(skeleton);
}

void CCSkeleton::setToSetupPose () {
	spSkeleton_setToSetupPose(skeleton);
}
void CCSkeleton::setBonesToSetupPose () {
	spSkeleton_setBonesToSetupPose(skeleton);
}
void CCSkeleton::setSlotsToSetupPose () {
	spSkeleton_setSlotsToSetupPose(skeleton);
}

spBone* CCSkeleton::findBone (const char* boneName) const {
	return spSkeleton_findBone(skeleton, boneName);
}

spSlot* CCSkeleton::findSlot (const char* slotName) const {
	return spSkeleton_findSlot(skeleton, slotName);
}

bool CCSkeleton::setSkin (const char* skinName) {
	return spSkeleton_setSkinByName(skeleton, skinName) ? true : false;
}

spAttachment* CCSkeleton::getAttachment (const char* slotName, const char* attachmentName) const {
	return spSkeleton_getAttachmentForSlotName(skeleton, slotName, attachmentName);
}
bool CCSkeleton::setAttachment (const char* slotName, const char* attachmentName) {
	return spSkeleton_setAttachment(skeleton, slotName, attachmentName) ? true : false;
}

// --- BlendProtocol
    
const BlendFunc& CCSkeleton::getBlendFunc() const {
    return blendFunc;
}
    
void CCSkeleton::setBlendFunc( const BlendFunc &blendFunc) {
    this->blendFunc = blendFunc;
}
    
void CCSkeleton::setOpacityModifyRGB (bool value) {
    premultipliedAlpha = value;
}
    
bool CCSkeleton::isOpacityModifyRGB () const {
    return premultipliedAlpha;
}

}
