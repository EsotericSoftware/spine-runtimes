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

#include <spine/SkeletonRenderer.h>
#include <spine/spine-cocos2dx.h>
#include <spine/extension.h>
#include <spine/PolygonBatch.h>
#include <algorithm>

USING_NS_CC;
using std::min;
using std::max;

namespace spine {

static const int quadTriangles[6] = {0, 1, 2, 2, 3, 0};

SkeletonRenderer* SkeletonRenderer::createWithData (spSkeletonData* skeletonData, bool ownsSkeletonData) {
	SkeletonRenderer* node = new SkeletonRenderer(skeletonData, ownsSkeletonData);
	node->autorelease();
	return node;
}

SkeletonRenderer* SkeletonRenderer::createWithFile (const char* skeletonDataFile, spAtlas* atlas, float scale) {
	SkeletonRenderer* node = new SkeletonRenderer(skeletonDataFile, atlas, scale);
	node->autorelease();
	return node;
}

SkeletonRenderer* SkeletonRenderer::createWithFile (const char* skeletonDataFile, const char* atlasFile, float scale) {
	SkeletonRenderer* node = new SkeletonRenderer(skeletonDataFile, atlasFile, scale);
	node->autorelease();
	return node;
}

void SkeletonRenderer::initialize () {
	atlas = 0;
	debugSlots = false;
	debugBones = false;
	timeScale = 1;

	worldVertices = MALLOC(float, 1000); // Max number of vertices per mesh.

	batch = PolygonBatch::createWithCapacity(2000); // Max number of vertices and triangles per batch.
	batch->retain();

	blendFunc.src = GL_ONE;
	blendFunc.dst = GL_ONE_MINUS_SRC_ALPHA;
	setOpacityModifyRGB(true);

	setShaderProgram(CCShaderCache::sharedShaderCache()->programForKey(kCCShader_PositionTextureColor));
	scheduleUpdate();
}

void SkeletonRenderer::setSkeletonData (spSkeletonData *skeletonData, bool ownsSkeletonData) {
	skeleton = spSkeleton_create(skeletonData);
	rootBone = skeleton->bones[0];
	this->ownsSkeletonData = ownsSkeletonData;
}

SkeletonRenderer::SkeletonRenderer () {
	initialize();
}

SkeletonRenderer::SkeletonRenderer (spSkeletonData *skeletonData, bool ownsSkeletonData) {
	initialize();

	setSkeletonData(skeletonData, ownsSkeletonData);
}

SkeletonRenderer::SkeletonRenderer (const char* skeletonDataFile, spAtlas* atlas, float scale) {
	initialize();

	spSkeletonJson* json = spSkeletonJson_create(atlas);
	json->scale = scale;
	spSkeletonData* skeletonData = spSkeletonJson_readSkeletonDataFile(json, skeletonDataFile);
	CCAssert(skeletonData, json->error ? json->error : "Error reading skeleton data.");
	spSkeletonJson_dispose(json);

	setSkeletonData(skeletonData, true);
}

SkeletonRenderer::SkeletonRenderer (const char* skeletonDataFile, const char* atlasFile, float scale) {
	initialize();

	atlas = spAtlas_createFromFile(atlasFile, 0);
	CCAssert(atlas, "Error reading atlas file.");

	spSkeletonJson* json = spSkeletonJson_create(atlas);
	json->scale = scale;
	spSkeletonData* skeletonData = spSkeletonJson_readSkeletonDataFile(json, skeletonDataFile);
	CCAssert(skeletonData, json->error ? json->error : "Error reading skeleton data file.");
	spSkeletonJson_dispose(json);

	setSkeletonData(skeletonData, true);
}

SkeletonRenderer::~SkeletonRenderer () {
	if (ownsSkeletonData) spSkeletonData_dispose(skeleton->data);
	if (atlas) spAtlas_dispose(atlas);
	spSkeleton_dispose(skeleton);
	FREE(worldVertices);
	batch->release();
}

void SkeletonRenderer::update (float deltaTime) {
	spSkeleton_update(skeleton, deltaTime * timeScale);
}

void SkeletonRenderer::draw () {
	CC_NODE_DRAW_SETUP();
	ccGLBindVAO(0);

	ccColor3B nodeColor = getColor();
	skeleton->r = nodeColor.r / (float)255;
	skeleton->g = nodeColor.g / (float)255;
	skeleton->b = nodeColor.b / (float)255;
	skeleton->a = getDisplayedOpacity() / (float)255;

	int blendMode = -1;
	ccColor4B color;
	const float* uvs = nullptr;
	int verticesCount = 0;
	const int* triangles = nullptr;
	int trianglesCount = 0;
	float r = 0, g = 0, b = 0, a = 0;
	for (int i = 0, n = skeleton->slotsCount; i < n; i++) {
		spSlot* slot = skeleton->drawOrder[i];
		if (!slot->attachment) continue;
		CCTexture2D *texture = nullptr;
		switch (slot->attachment->type) {
		case SP_ATTACHMENT_REGION: {
			spRegionAttachment* attachment = (spRegionAttachment*)slot->attachment;
			spRegionAttachment_computeWorldVertices(attachment, slot->bone, worldVertices);
			texture = getTexture(attachment);
			uvs = attachment->uvs;
			verticesCount = 8;
			triangles = quadTriangles;
			trianglesCount = 6;
			r = attachment->r;
			g = attachment->g;
			b = attachment->b;
			a = attachment->a;
			break;
		}
		case SP_ATTACHMENT_MESH: {
			spMeshAttachment* attachment = (spMeshAttachment*)slot->attachment;
			spMeshAttachment_computeWorldVertices(attachment, slot, worldVertices);
			texture = getTexture(attachment);
			uvs = attachment->uvs;
			verticesCount = attachment->verticesCount;
			triangles = attachment->triangles;
			trianglesCount = attachment->trianglesCount;
			r = attachment->r;
			g = attachment->g;
			b = attachment->b;
			a = attachment->a;
			break;
		}
		case SP_ATTACHMENT_SKINNED_MESH: {
			spSkinnedMeshAttachment* attachment = (spSkinnedMeshAttachment*)slot->attachment;
			spSkinnedMeshAttachment_computeWorldVertices(attachment, slot, worldVertices);
			texture = getTexture(attachment);
			uvs = attachment->uvs;
			verticesCount = attachment->uvsCount;
			triangles = attachment->triangles;
			trianglesCount = attachment->trianglesCount;
			r = attachment->r;
			g = attachment->g;
			b = attachment->b;
			a = attachment->a;
			break;
		}
		}
		if (texture) {
			if (slot->data->blendMode != blendMode) {
				batch->flush();
				blendMode = slot->data->blendMode;
				switch (slot->data->blendMode) {
				case SP_BLEND_MODE_ADDITIVE:
					ccGLBlendFunc(premultipliedAlpha ? GL_ONE : GL_SRC_ALPHA, GL_ONE);
					break;
				case SP_BLEND_MODE_MULTIPLY:
					ccGLBlendFunc(GL_DST_COLOR, GL_ONE_MINUS_SRC_ALPHA);
					break;
				case SP_BLEND_MODE_SCREEN:
					ccGLBlendFunc(GL_ONE, GL_ONE_MINUS_SRC_COLOR);
					break;
				default:
					ccGLBlendFunc(blendFunc.src, blendFunc.dst);
				}
			}
			color.a = skeleton->a * slot->a * a * 255;
			float multiplier = premultipliedAlpha ? color.a : 255;
			color.r = skeleton->r * slot->r * r * multiplier;
			color.g = skeleton->g * slot->g * g * multiplier;
			color.b = skeleton->b * slot->b * b * multiplier;
			batch->add(texture, worldVertices, uvs, verticesCount, triangles, trianglesCount, &color);
		}
	}
	batch->flush();

	if (debugSlots) {
		// Slots.
		ccDrawColor4B(0, 0, 255, 255);
		glLineWidth(1);
		CCPoint points[4];
		for (int i = 0, n = skeleton->slotsCount; i < n; i++) {
			spSlot* slot = skeleton->drawOrder[i];
			if (!slot->attachment || slot->attachment->type != SP_ATTACHMENT_REGION) continue;
			spRegionAttachment* attachment = (spRegionAttachment*)slot->attachment;
			spRegionAttachment_computeWorldVertices(attachment, slot->bone, worldVertices);
			points[0] = ccp(worldVertices[0], worldVertices[1]);
			points[1] = ccp(worldVertices[2], worldVertices[3]);
			points[2] = ccp(worldVertices[4], worldVertices[5]);
			points[3] = ccp(worldVertices[6], worldVertices[7]);
			ccDrawPoly(points, 4, true);
		}
	}
	if (debugBones) {
		// Bone lengths.
		glLineWidth(2);
		ccDrawColor4B(255, 0, 0, 255);
		for (int i = 0, n = skeleton->bonesCount; i < n; i++) {
			spBone *bone = skeleton->bones[i];
			float x = bone->data->length * bone->m00 + bone->worldX;
			float y = bone->data->length * bone->m10 + bone->worldY;
			ccDrawLine(ccp(bone->worldX, bone->worldY), ccp(x, y));
		}
		// Bone origins.
		ccPointSize(4);
		ccDrawColor4B(0, 0, 255, 255); // Root bone is blue.
		for (int i = 0, n = skeleton->bonesCount; i < n; i++) {
			spBone *bone = skeleton->bones[i];
			ccDrawPoint(ccp(bone->worldX, bone->worldY));
			if (i == 0) ccDrawColor4B(0, 255, 0, 255);
		}
	}
}

CCTexture2D* SkeletonRenderer::getTexture (spRegionAttachment* attachment) const {
	return (CCTexture2D*)((spAtlasRegion*)attachment->rendererObject)->page->rendererObject;
}

CCTexture2D* SkeletonRenderer::getTexture (spMeshAttachment* attachment) const {
	return (CCTexture2D*)((spAtlasRegion*)attachment->rendererObject)->page->rendererObject;
}

CCTexture2D* SkeletonRenderer::getTexture (spSkinnedMeshAttachment* attachment) const {
	return (CCTexture2D*)((spAtlasRegion*)attachment->rendererObject)->page->rendererObject;
}

CCRect SkeletonRenderer::boundingBox () {
	float minX = FLT_MAX, minY = FLT_MAX, maxX = FLT_MIN, maxY = FLT_MIN;
	float scaleX = getScaleX(), scaleY = getScaleY();
	for (int i = 0; i < skeleton->slotsCount; ++i) {
		spSlot* slot = skeleton->slots[i];
		if (!slot->attachment) continue;
		int verticesCount;
		if (slot->attachment->type == SP_ATTACHMENT_REGION) {
			spRegionAttachment* attachment = (spRegionAttachment*)slot->attachment;
			spRegionAttachment_computeWorldVertices(attachment, slot->bone, worldVertices);
			verticesCount = 8;
		} else if (slot->attachment->type == SP_ATTACHMENT_MESH) {
			spMeshAttachment* mesh = (spMeshAttachment*)slot->attachment;
			spMeshAttachment_computeWorldVertices(mesh, slot, worldVertices);
			verticesCount = mesh->verticesCount;
		} else if (slot->attachment->type == SP_ATTACHMENT_SKINNED_MESH) {
			spSkinnedMeshAttachment* mesh = (spSkinnedMeshAttachment*)slot->attachment;
			spSkinnedMeshAttachment_computeWorldVertices(mesh, slot, worldVertices);
			verticesCount = mesh->uvsCount;
		} else
			continue;
		for (int ii = 0; ii < verticesCount; ii += 2) {
			float x = worldVertices[ii] * scaleX, y = worldVertices[ii + 1] * scaleY;
			minX = min(minX, x);
			minY = min(minY, y);
			maxX = max(maxX, x);
			maxY = max(maxY, y);
		}
	}
	CCPoint position = getPosition();
	return CCRect(position.x + minX, position.y + minY, maxX - minX, maxY - minY);
}

// --- Convenience methods for Skeleton_* functions.

void SkeletonRenderer::updateWorldTransform () {
	spSkeleton_updateWorldTransform(skeleton);
}

void SkeletonRenderer::setToSetupPose () {
	spSkeleton_setToSetupPose(skeleton);
}
void SkeletonRenderer::setBonesToSetupPose () {
	spSkeleton_setBonesToSetupPose(skeleton);
}
void SkeletonRenderer::setSlotsToSetupPose () {
	spSkeleton_setSlotsToSetupPose(skeleton);
}

spBone* SkeletonRenderer::findBone (const char* boneName) const {
	return spSkeleton_findBone(skeleton, boneName);
}

spSlot* SkeletonRenderer::findSlot (const char* slotName) const {
	return spSkeleton_findSlot(skeleton, slotName);
}

bool SkeletonRenderer::setSkin (const char* skinName) {
	return spSkeleton_setSkinByName(skeleton, skinName) ? true : false;
}

spAttachment* SkeletonRenderer::getAttachment (const char* slotName, const char* attachmentName) const {
	return spSkeleton_getAttachmentForSlotName(skeleton, slotName, attachmentName);
}
bool SkeletonRenderer::setAttachment (const char* slotName, const char* attachmentName) {
	return spSkeleton_setAttachment(skeleton, slotName, attachmentName) ? true : false;
}

// --- CCBlendProtocol

ccBlendFunc SkeletonRenderer::getBlendFunc () {
    return blendFunc;
}

void SkeletonRenderer::setBlendFunc (ccBlendFunc blendFunc) {
    this->blendFunc = blendFunc;
}

void SkeletonRenderer::setOpacityModifyRGB (bool value) {
	premultipliedAlpha = value;
}

bool SkeletonRenderer::isOpacityModifyRGB () {
	return premultipliedAlpha;
}

}
