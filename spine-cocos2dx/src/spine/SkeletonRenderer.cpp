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

#include <spine/SkeletonRenderer.h>
#include <spine/extension.h>
#include <spine/SkeletonBatch.h>
#include <spine/SkeletonTwoColorBatch.h>
#include <spine/AttachmentVertices.h>
#include <spine/Cocos2dAttachmentLoader.h>
#include <algorithm>

USING_NS_CC;


namespace spine {

    
int computeTotalCoordCount(const spSkeleton& skeleton, int startSlotIndex, int endSlotIndex);
cocos2d::Rect computeBoundingRect(const float* coords, int vertexCount);
void interleaveCoordinates(float* dst, const float* src, int vertexCount, int dstStride);
BlendFunc makeBlendFunc(int blendMode, bool premultipliedAlpha);
void transformWorldVertices(float* dstCoord, int coordCount, const spSkeleton& skeleton, int startSlotIndex, int endSlotIndex);
bool cullRectangle(const Mat4 &transform, const cocos2d::Rect& rect, const Camera& camera);
Color4B spColorToColor4B(const spColor& color);
bool slotIsOutRange(const spSlot& slot, int startSlotIndex, int endSlotIndex);
   
 
// C Variable length array
#ifdef _MSC_VER
    // VLA not supported, use _alloca
    #define VLA(type, arr, count) \
        type* arr = static_cast<type*>( _alloca(sizeof(type) * count) )
#else
    #define VLA(type, arr, count) \
        type arr[count]
#endif
    

SkeletonRenderer* SkeletonRenderer::createWithSkeleton(spSkeleton* skeleton, bool ownsSkeleton, bool ownsSkeletonData) {
	SkeletonRenderer* node = new SkeletonRenderer(skeleton, ownsSkeleton, ownsSkeletonData);
	node->autorelease();
	return node;
}
	
SkeletonRenderer* SkeletonRenderer::createWithData (spSkeletonData* skeletonData, bool ownsSkeletonData) {
	SkeletonRenderer* node = new SkeletonRenderer(skeletonData, ownsSkeletonData);
	node->autorelease();
	return node;
}

SkeletonRenderer* SkeletonRenderer::createWithFile (const std::string& skeletonDataFile, spAtlas* atlas, float scale) {
	SkeletonRenderer* node = new SkeletonRenderer(skeletonDataFile, atlas, scale);
	node->autorelease();
	return node;
}

SkeletonRenderer* SkeletonRenderer::createWithFile (const std::string& skeletonDataFile, const std::string& atlasFile, float scale) {
	SkeletonRenderer* node = new SkeletonRenderer(skeletonDataFile, atlasFile, scale);
	node->autorelease();
	return node;
}

void SkeletonRenderer::initialize () {
	_clipper = spSkeletonClipping_create();

	_blendFunc = BlendFunc::ALPHA_PREMULTIPLIED;
	setOpacityModifyRGB(true);

	setupGLProgramState(false);
}
	
void SkeletonRenderer::setupGLProgramState (bool twoColorTintEnabled) {
	if (twoColorTintEnabled) {
		setGLProgramState(SkeletonTwoColorBatch::getInstance()->getTwoColorTintProgramState());
		return;
	}
	
	Texture2D *texture = nullptr;
	for (int i = 0, n = _skeleton->slotsCount; i < n; i++) {
		spSlot* slot = _skeleton->drawOrder[i];
		if (!slot->attachment) continue;
		switch (slot->attachment->type) {
			case SP_ATTACHMENT_REGION: {
				spRegionAttachment* attachment = (spRegionAttachment*)slot->attachment;
				texture = static_cast<AttachmentVertices*>(attachment->rendererObject)->_texture;
				break;
			}
			case SP_ATTACHMENT_MESH: {
				spMeshAttachment* attachment = (spMeshAttachment*)slot->attachment;
				texture = static_cast<AttachmentVertices*>(attachment->rendererObject)->_texture;
				break;
			}
			default:
				continue;
		}
		
		if (texture != nullptr) {
			break;
		}
	}
	setGLProgramState(GLProgramState::getOrCreateWithGLProgramName(GLProgram::SHADER_NAME_POSITION_TEXTURE_COLOR_NO_MVP, texture));
}

void SkeletonRenderer::setSkeletonData (spSkeletonData *skeletonData, bool ownsSkeletonData) {
	_skeleton = spSkeleton_create(skeletonData);
	_ownsSkeletonData = ownsSkeletonData;
}

SkeletonRenderer::SkeletonRenderer ()
	: _atlas(nullptr), _attachmentLoader(nullptr), _debugSlots(false), _debugBones(false), _debugMeshes(false), _debugBoundingRect(false), _timeScale(1), _effect(nullptr), _startSlotIndex(0), _endSlotIndex(std::numeric_limits<int>::max()) {
}
	
SkeletonRenderer::SkeletonRenderer(spSkeleton* skeleton, bool ownsSkeleton, bool ownsSkeletonData)
	: _atlas(nullptr), _attachmentLoader(nullptr), _debugSlots(false), _debugBones(false), _debugMeshes(false), _debugBoundingRect(false), _timeScale(1), _effect(nullptr), _startSlotIndex(0), _endSlotIndex(std::numeric_limits<int>::max()) {
	initWithSkeleton(skeleton, ownsSkeleton, ownsSkeletonData);
}

SkeletonRenderer::SkeletonRenderer (spSkeletonData *skeletonData, bool ownsSkeletonData)
	: _atlas(nullptr), _attachmentLoader(nullptr), _debugSlots(false), _debugBones(false), _debugMeshes(false), _debugBoundingRect(false), _timeScale(1), _effect(nullptr), _startSlotIndex(0), _endSlotIndex(std::numeric_limits<int>::max()) {
	initWithData(skeletonData, ownsSkeletonData);
}

SkeletonRenderer::SkeletonRenderer (const std::string& skeletonDataFile, spAtlas* atlas, float scale)
	: _atlas(nullptr), _attachmentLoader(nullptr), _debugSlots(false), _debugBones(false), _debugMeshes(false), _debugBoundingRect(false), _timeScale(1), _effect(nullptr), _startSlotIndex(0), _endSlotIndex(std::numeric_limits<int>::max()) {
	initWithJsonFile(skeletonDataFile, atlas, scale);
}

SkeletonRenderer::SkeletonRenderer (const std::string& skeletonDataFile, const std::string& atlasFile, float scale)
	: _atlas(nullptr), _attachmentLoader(nullptr), _debugSlots(false), _debugBones(false), _debugMeshes(false), _debugBoundingRect(false), _timeScale(1), _effect(nullptr), _startSlotIndex(0), _endSlotIndex(std::numeric_limits<int>::max()) {
	initWithJsonFile(skeletonDataFile, atlasFile, scale);
}

SkeletonRenderer::~SkeletonRenderer () {
	if (_ownsSkeletonData) spSkeletonData_dispose(_skeleton->data);
	if (_ownsSkeleton) spSkeleton_dispose(_skeleton);
	if (_atlas) spAtlas_dispose(_atlas);
	if (_attachmentLoader) spAttachmentLoader_dispose(_attachmentLoader);	
	spSkeletonClipping_dispose(_clipper);
}

void SkeletonRenderer::initWithSkeleton(spSkeleton* skeleton, bool ownsSkeleton, bool ownsSkeletonData) {
	_skeleton = skeleton;
	_ownsSkeleton = ownsSkeleton;
	_ownsSkeletonData = ownsSkeletonData;
	
	initialize();
}
	
void SkeletonRenderer::initWithData (spSkeletonData* skeletonData, bool ownsSkeletonData) {
	_ownsSkeleton = true;
	setSkeletonData(skeletonData, ownsSkeletonData);
	initialize();
}

void SkeletonRenderer::initWithJsonFile (const std::string& skeletonDataFile, spAtlas* atlas, float scale) {
    _atlas = atlas;
	_attachmentLoader = SUPER(Cocos2dAttachmentLoader_create(_atlas));

	spSkeletonJson* json = spSkeletonJson_createWithLoader(_attachmentLoader);
	json->scale = scale;
	spSkeletonData* skeletonData = spSkeletonJson_readSkeletonDataFile(json, skeletonDataFile.c_str());
	CCASSERT(skeletonData, json->error ? json->error : "Error reading skeleton data.");
	spSkeletonJson_dispose(json);

	_ownsSkeleton = true;
	setSkeletonData(skeletonData, true);

	initialize();
}

void SkeletonRenderer::initWithJsonFile (const std::string& skeletonDataFile, const std::string& atlasFile, float scale) {
	_atlas = spAtlas_createFromFile(atlasFile.c_str(), 0);
	CCASSERT(_atlas, "Error reading atlas file.");

	_attachmentLoader = SUPER(Cocos2dAttachmentLoader_create(_atlas));

	spSkeletonJson* json = spSkeletonJson_createWithLoader(_attachmentLoader);
	json->scale = scale;
	spSkeletonData* skeletonData = spSkeletonJson_readSkeletonDataFile(json, skeletonDataFile.c_str());
	CCASSERT(skeletonData, json->error ? json->error : "Error reading skeleton data file.");
	spSkeletonJson_dispose(json);

	_ownsSkeleton = true;
	setSkeletonData(skeletonData, true);

	initialize();
}
    
void SkeletonRenderer::initWithBinaryFile (const std::string& skeletonDataFile, spAtlas* atlas, float scale) {
    _atlas = atlas;
    _attachmentLoader = SUPER(Cocos2dAttachmentLoader_create(_atlas));
    
    spSkeletonBinary* binary = spSkeletonBinary_createWithLoader(_attachmentLoader);
    binary->scale = scale;
    spSkeletonData* skeletonData = spSkeletonBinary_readSkeletonDataFile(binary, skeletonDataFile.c_str());
    CCASSERT(skeletonData, binary->error ? binary->error : "Error reading skeleton data file.");
    spSkeletonBinary_dispose(binary);
    _ownsSkeleton = true;
    setSkeletonData(skeletonData, true);
    
    initialize();
}

void SkeletonRenderer::initWithBinaryFile (const std::string& skeletonDataFile, const std::string& atlasFile, float scale) {
    _atlas = spAtlas_createFromFile(atlasFile.c_str(), 0);
    CCASSERT(_atlas, "Error reading atlas file.");
    
    _attachmentLoader = SUPER(Cocos2dAttachmentLoader_create(_atlas));
    
    spSkeletonBinary* binary = spSkeletonBinary_createWithLoader(_attachmentLoader);
    binary->scale = scale;
    spSkeletonData* skeletonData = spSkeletonBinary_readSkeletonDataFile(binary, skeletonDataFile.c_str());
    CCASSERT(skeletonData, binary->error ? binary->error : "Error reading skeleton data file.");
    spSkeletonBinary_dispose(binary);
    _ownsSkeleton = true;
    setSkeletonData(skeletonData, true);
    
    initialize();
}


void SkeletonRenderer::update (float deltaTime) {
	Node::update(deltaTime);
	if (_ownsSkeleton) spSkeleton_update(_skeleton, deltaTime * _timeScale);
}

void SkeletonRenderer::draw (Renderer* renderer, const Mat4& transform, uint32_t transformFlags) {
	assert(_skeleton);
	const int coordCount = computeTotalCoordCount(*_skeleton, _startSlotIndex, _endSlotIndex);
	if (coordCount == 0)
	{
		return;
	}
	assert(coordCount % 2 == 0);

	VLA(float, worldCoords, coordCount);
	transformWorldVertices(worldCoords, coordCount, *_skeleton, _startSlotIndex, _endSlotIndex);

	#if CC_USE_CULLING
	const Camera* camera = Camera::getVisitingCamera();
	const cocos2d::Rect brect = computeBoundingRect(worldCoords, coordCount / 2);
	_boundingRect = brect;
	if (camera && cullRectangle(transform, brect, *camera))
	{
		return;
	}
	#endif

	const float* worldCoordPtr = worldCoords;
	SkeletonBatch* batch = SkeletonBatch::getInstance();
	SkeletonTwoColorBatch* twoColorBatch = SkeletonTwoColorBatch::getInstance();
	const bool hasSingleTint = (isTwoColorTint() == false);

	if (_effect) {
		_effect->begin(_effect, _skeleton);
	}

	const Color3B displayedColor = getDisplayedColor();
	spColor nodeColor;
	nodeColor.r = displayedColor.r / 255.f;
	nodeColor.g = displayedColor.g / 255.f;
	nodeColor.b = displayedColor.b / 255.f;
	nodeColor.a = getDisplayedOpacity() / 255.f;
	
	spColor color;
	spColor darkColor;
	const float darkPremultipliedAlpha = _premultipliedAlpha ? 1.f : 0;
	AttachmentVertices* attachmentVertices = nullptr;
	TwoColorTrianglesCommand* lastTwoColorTrianglesCommand = nullptr;
	for (int i = 0, n = _skeleton->slotsCount; i < n; ++i) {
		spSlot* slot = _skeleton->drawOrder[i];
		
		if (!slot->attachment) {
			spSkeletonClipping_clipEnd(_clipper, slot);
			continue;
		}

		if (slotIsOutRange(*slot, _startSlotIndex, _endSlotIndex)) {
			spSkeletonClipping_clipEnd(_clipper, slot);
			continue;
		}

		cocos2d::TrianglesCommand::Triangles triangles;
		TwoColorTriangles trianglesTwoColor;
		
		switch (slot->attachment->type) {
			case SP_ATTACHMENT_REGION: {
				spRegionAttachment* attachment = (spRegionAttachment*)slot->attachment;
				attachmentVertices = getAttachmentVertices(attachment);
				
				float* dstTriangleVertices = nullptr;
				int dstStride = 0; // in floats
				if (hasSingleTint) {
					triangles.indices = attachmentVertices->_triangles->indices;
					triangles.indexCount = attachmentVertices->_triangles->indexCount;
					triangles.verts = batch->allocateVertices(attachmentVertices->_triangles->vertCount);
					triangles.vertCount = attachmentVertices->_triangles->vertCount;
					assert(triangles.vertCount == 4);
					std::memcpy(triangles.verts, attachmentVertices->_triangles->verts, sizeof(cocos2d::V3F_C4B_T2F) * attachmentVertices->_triangles->vertCount);
					dstStride = sizeof(V3F_C4B_T2F) / sizeof(float);
					dstTriangleVertices = reinterpret_cast<float*>(triangles.verts);
				} else {
					trianglesTwoColor.indices = attachmentVertices->_triangles->indices;
					trianglesTwoColor.indexCount = attachmentVertices->_triangles->indexCount;
					trianglesTwoColor.verts = twoColorBatch->allocateVertices(attachmentVertices->_triangles->vertCount);
					trianglesTwoColor.vertCount = attachmentVertices->_triangles->vertCount;
					assert(trianglesTwoColor.vertCount == 4);
					for (int i = 0; i < trianglesTwoColor.vertCount; i++) {
						trianglesTwoColor.verts[i].texCoords = attachmentVertices->_triangles->verts[i].texCoords;
					}
					dstTriangleVertices = reinterpret_cast<float*>(trianglesTwoColor.verts);
					dstStride = sizeof(V3F_C4B_C4B_T2F) / sizeof(float);
				}
				// Copy world vertices to triangle vertices
				interleaveCoordinates(dstTriangleVertices, worldCoordPtr, 4, dstStride);
				worldCoordPtr += 8;
				
				color = attachment->color;
				
				break;
			}
			case SP_ATTACHMENT_MESH: {
				spMeshAttachment* attachment = (spMeshAttachment*)slot->attachment;
				attachmentVertices = getAttachmentVertices(attachment);
				
				float* dstTriangleVertices = nullptr;
				int dstStride = 0; // in floats
				int dstVertexCount = 0;
				if (hasSingleTint) {
					triangles.indices = attachmentVertices->_triangles->indices;
					triangles.indexCount = attachmentVertices->_triangles->indexCount;
					triangles.verts = batch->allocateVertices(attachmentVertices->_triangles->vertCount);
					triangles.vertCount = attachmentVertices->_triangles->vertCount;
					std::memcpy(triangles.verts, attachmentVertices->_triangles->verts, sizeof(cocos2d::V3F_C4B_T2F) * attachmentVertices->_triangles->vertCount);
					dstTriangleVertices = (float*)triangles.verts;
					dstStride = sizeof(V3F_C4B_T2F) / sizeof(float);
					dstVertexCount = triangles.vertCount;
				} else {
					trianglesTwoColor.indices = attachmentVertices->_triangles->indices;
					trianglesTwoColor.indexCount = attachmentVertices->_triangles->indexCount;
					trianglesTwoColor.verts = twoColorBatch->allocateVertices(attachmentVertices->_triangles->vertCount);
					trianglesTwoColor.vertCount = attachmentVertices->_triangles->vertCount;
					for (int i = 0; i < trianglesTwoColor.vertCount; i++) {
						trianglesTwoColor.verts[i].texCoords = attachmentVertices->_triangles->verts[i].texCoords;
					}
					dstTriangleVertices = (float*)trianglesTwoColor.verts;
					dstStride = sizeof(V3F_C4B_C4B_T2F) / sizeof(float);
					dstVertexCount = trianglesTwoColor.vertCount;
				}
				
				// Copy world vertices to triangle vertices
				assert(dstVertexCount * 2 == attachment->super.worldVerticesLength);
				interleaveCoordinates(dstTriangleVertices, worldCoordPtr, dstVertexCount, dstStride);
				worldCoordPtr += dstVertexCount * 2;
				
				color = attachment->color;
				break;
		}
		case SP_ATTACHMENT_CLIPPING: {
			spClippingAttachment* clip = (spClippingAttachment*)slot->attachment;
			spSkeletonClipping_clipStart(_clipper, slot, clip);
			continue;
		}
		default:
			spSkeletonClipping_clipEnd(_clipper, slot);
			continue;
		}
		
		if (slot->darkColor) {
			darkColor = *slot->darkColor;
		} else {
			darkColor.r = 0;
			darkColor.g = 0;
			darkColor.b = 0;
		}
		darkColor.a = darkPremultipliedAlpha;
		
		color.a *= nodeColor.a * _skeleton->color.a * slot->color.a;
		// skip rendering if the color of this attachment is 0
		if (color.a == 0){
			spSkeletonClipping_clipEnd(_clipper, slot);
			continue;
		}
		color.r *= nodeColor.r * _skeleton->color.r * slot->color.r;
		color.g *= nodeColor.g * _skeleton->color.g * slot->color.g;
		color.b *= nodeColor.b * _skeleton->color.b * slot->color.b;
		if (_premultipliedAlpha)
		{
			color.r *= color.a;
			color.g *= color.a;
			color.b *= color.a;
		}
		
		const cocos2d::Color4B color4B = spColorToColor4B(color);
		const cocos2d::Color4B darkColor4B = spColorToColor4B(darkColor);
		const BlendFunc blendFunc = makeBlendFunc(slot->data->blendMode, _premultipliedAlpha);

		if (hasSingleTint) {
			if (spSkeletonClipping_isClipping(_clipper)) {
				spSkeletonClipping_clipTriangles(_clipper, (float*)&triangles.verts[0].vertices, triangles.vertCount * sizeof(cocos2d::V3F_C4B_T2F) / 4, triangles.indices, triangles.indexCount, (float*)&triangles.verts[0].texCoords, 6);
				batch->deallocateVertices(triangles.vertCount);
				
				if (_clipper->clippedTriangles->size == 0){
					spSkeletonClipping_clipEnd(_clipper, slot);
					continue;
				}
				
				triangles.vertCount = _clipper->clippedVertices->size >> 1;
				triangles.verts = batch->allocateVertices(triangles.vertCount);
				triangles.indexCount = _clipper->clippedTriangles->size;
				triangles.indices =
				batch->allocateIndices(triangles.indexCount);
				std::memcpy(triangles.indices, _clipper->clippedTriangles->items, sizeof(unsigned short) * _clipper->clippedTriangles->size);
				
				cocos2d::TrianglesCommand* batchedTriangles = batch->addCommand(renderer, _globalZOrder, attachmentVertices->_texture, _glProgramState, blendFunc, triangles, transform, transformFlags);
				
				const float* verts = _clipper->clippedVertices->items;
				const float* uvs = _clipper->clippedUVs->items;
				if (_effect) {
					V3F_C4B_T2F* vertex = batchedTriangles->getTriangles().verts;
					spColor darkTmp;
					for (int v = 0, vn = batchedTriangles->getTriangles().vertCount, vv = 0; v < vn; ++v, vv+=2, ++vertex) {
						spColor lightCopy = color;
						vertex->vertices.x = verts[vv];
						vertex->vertices.y = verts[vv + 1];
						vertex->texCoords.u = uvs[vv];
						vertex->texCoords.v = uvs[vv + 1];
						_effect->transform(_effect, &vertex->vertices.x, &vertex->vertices.y, &vertex->texCoords.u, &vertex->texCoords.v, &lightCopy, &darkTmp);
						vertex->colors = spColorToColor4B(lightCopy);
					}
				} else {
					const cocos2d::Color4B color4B = spColorToColor4B(color);
					V3F_C4B_T2F* vertex = batchedTriangles->getTriangles().verts;
					for (int v = 0, vn = batchedTriangles->getTriangles().vertCount, vv = 0; v < vn; ++v, vv+=2, ++vertex) {
						vertex->vertices.x = verts[vv];
						vertex->vertices.y = verts[vv + 1];
						vertex->texCoords.u = uvs[vv];
						vertex->texCoords.v = uvs[vv + 1];
						vertex->colors = color4B;
					}
				}
			} else {
				// Not clipping
				
				cocos2d::TrianglesCommand* batchedTriangles = batch->addCommand(renderer, _globalZOrder, attachmentVertices->_texture, _glProgramState, blendFunc, triangles, transform, transformFlags);
				
				if (_effect) {
					V3F_C4B_T2F* vertex = batchedTriangles->getTriangles().verts;
					spColor darkTmp;
					for (int v = 0, vn = batchedTriangles->getTriangles().vertCount; v < vn; ++v, ++vertex) {
						spColor lightCopy = color;
						_effect->transform(_effect, &vertex->vertices.x, &vertex->vertices.y, &vertex->texCoords.u, &vertex->texCoords.v, &lightCopy, &darkTmp);
						vertex->colors = spColorToColor4B(lightCopy);
					}
				} else {
					V3F_C4B_T2F* vertex = batchedTriangles->getTriangles().verts;
					for (int v = 0, vn = batchedTriangles->getTriangles().vertCount; v < vn; ++v, ++vertex) {
						vertex->colors = color4B;
					}
				}
			}
		} else {
			// Two tints
			
			if (spSkeletonClipping_isClipping(_clipper)) {
				spSkeletonClipping_clipTriangles(_clipper, (float*)&trianglesTwoColor.verts[0].position, trianglesTwoColor.vertCount * sizeof(V3F_C4B_C4B_T2F) / 4, trianglesTwoColor.indices, trianglesTwoColor.indexCount, (float*)&trianglesTwoColor.verts[0].texCoords, 7);
				twoColorBatch->deallocateVertices(trianglesTwoColor.vertCount);
				
				if (_clipper->clippedTriangles->size == 0){
					spSkeletonClipping_clipEnd(_clipper, slot);
					continue;
				}
				
				trianglesTwoColor.vertCount = _clipper->clippedVertices->size >> 1;
				trianglesTwoColor.verts = twoColorBatch->allocateVertices(trianglesTwoColor.vertCount);
				trianglesTwoColor.indexCount = _clipper->clippedTriangles->size;
				trianglesTwoColor.indices = twoColorBatch->allocateIndices(trianglesTwoColor.indexCount);
				std::memcpy(trianglesTwoColor.indices, _clipper->clippedTriangles->items, sizeof(unsigned short) * _clipper->clippedTriangles->size);
				
				TwoColorTrianglesCommand* batchedTriangles = lastTwoColorTrianglesCommand = twoColorBatch->addCommand(renderer, _globalZOrder, attachmentVertices->_texture->getName(), _glProgramState, blendFunc, trianglesTwoColor, transform, transformFlags);
				
				const float* verts = _clipper->clippedVertices->items;
				const float* uvs = _clipper->clippedUVs->items;
				
				if (_effect) {
					V3F_C4B_C4B_T2F* vertex = batchedTriangles->getTriangles().verts;
					for (int v = 0, vn = batchedTriangles->getTriangles().vertCount, vv = 0; v < vn; ++v, vv += 2, ++vertex) {
						spColor lightCopy = color;
						spColor darkCopy = darkColor;
						vertex->position.x = verts[vv];
						vertex->position.y = verts[vv + 1];
						vertex->texCoords.u = uvs[vv];
						vertex->texCoords.v = uvs[vv + 1];
						_effect->transform(_effect, &vertex->position.x, &vertex->position.y, &vertex->texCoords.u, &vertex->texCoords.v, &lightCopy, &darkCopy);
						vertex->color = spColorToColor4B(lightCopy);
						vertex->color2 = spColorToColor4B(darkCopy);
					}
				} else {
					V3F_C4B_C4B_T2F* vertex = batchedTriangles->getTriangles().verts;
					for (int v = 0, vn = batchedTriangles->getTriangles().vertCount, vv = 0; v < vn; ++v, vv += 2, ++vertex) {
						vertex->position.x = verts[vv];
						vertex->position.y = verts[vv + 1];
						vertex->texCoords.u = uvs[vv];
						vertex->texCoords.v = uvs[vv + 1];
						vertex->color = color4B;
						vertex->color2 = darkColor4B;
					}
				}
			} else {
				TwoColorTrianglesCommand* batchedTriangles = lastTwoColorTrianglesCommand = twoColorBatch->addCommand(renderer, _globalZOrder, attachmentVertices->_texture->getName(), _glProgramState, blendFunc, trianglesTwoColor, transform, transformFlags);
				
				if (_effect) {
					V3F_C4B_C4B_T2F* vertex = batchedTriangles->getTriangles().verts;
					for (int v = 0, vn = batchedTriangles->getTriangles().vertCount; v < vn; ++v, ++vertex) {
						spColor lightCopy = color;
						spColor darkCopy = darkColor;
						_effect->transform(_effect, &vertex->position.x, &vertex->position.y, &vertex->texCoords.u, &vertex->texCoords.v, &lightCopy, &darkCopy);
						vertex->color = spColorToColor4B(lightCopy);
						vertex->color2 = spColorToColor4B(darkCopy);
					}
				} else {
					V3F_C4B_C4B_T2F* vertex = batchedTriangles->getTriangles().verts;
					for (int v = 0, vn = batchedTriangles->getTriangles().vertCount; v < vn; ++v, ++vertex) {
						vertex->color = color4B;
						vertex->color2 = darkColor4B;
					}
				}
			}
		}
		spSkeletonClipping_clipEnd(_clipper, slot);
	}
	spSkeletonClipping_clipEnd2(_clipper);
	
	if (lastTwoColorTrianglesCommand) {
		Node* parent = this->getParent();
		
		// We need to decide if we can postpone flushing the current
		// batch. We can postpone if the next sibling node is a
		// two color tinted skeleton with the same global-z.
		// The parent->getChildrenCount() > 100 check is a hack
		// as checking for a sibling is an O(n) operation, and if
		// all children of this nodes parent are skeletons, we
		// are in O(n2) territory.
		if (!parent || parent->getChildrenCount() > 100 || getChildrenCount() != 0) {
			lastTwoColorTrianglesCommand->setForceFlush(true);
		} else {
			Vector<Node*>& children = parent->getChildren();
			Node* sibling = nullptr;
			for (ssize_t i = 0; i < children.size(); i++) {
				if (children.at(i) == this) {
					if (i < children.size() - 1) {
						sibling = children.at(i+1);
						break;
					}
				}
			}
			if (!sibling) {
				lastTwoColorTrianglesCommand->setForceFlush(true);
			} else {
				SkeletonRenderer* siblingSkeleton = dynamic_cast<SkeletonRenderer*>(sibling);
				if (!siblingSkeleton || // flush is next sibling isn't a SkeletonRenderer
					!siblingSkeleton->isTwoColorTint() || // flush if next sibling isn't two color tinted
					!siblingSkeleton->isVisible() || // flush if next sibling is two color tinted but not visible
					(siblingSkeleton->getGlobalZOrder() != this->getGlobalZOrder())) { // flush if next sibling is two color tinted but z-order differs
					lastTwoColorTrianglesCommand->setForceFlush(true);
				}
			}
		}
	}
	
	if (_effect) _effect->end(_effect);

	if (_debugBoundingRect || _debugSlots || _debugBones || _debugMeshes) {
        drawDebug(renderer, transform, transformFlags);
	}
}

void SkeletonRenderer::drawDebug (Renderer* renderer, const Mat4 &transform, uint32_t transformFlags) {

	Director* director = Director::getInstance();
	director->pushMatrix(MATRIX_STACK_TYPE::MATRIX_STACK_MODELVIEW);
	director->loadMatrix(MATRIX_STACK_TYPE::MATRIX_STACK_MODELVIEW, transform);
    
    DrawNode* drawNode = DrawNode::create();
    
    // Draw bounding rectangle
    if (_debugBoundingRect) {
        glLineWidth(2);
        const cocos2d::Rect brect = getBoundingBox();
        const Vec2 points[4] =
        {
            brect.origin,
            { brect.origin.x + brect.size.width, brect.origin.y },
            { brect.origin.x + brect.size.width, brect.origin.y + brect.size.height },
            { brect.origin.x, brect.origin.y + brect.size.height }
        };
        drawNode->drawPoly(points, 4, true, Color4F::GREEN);
    }

    if (_debugSlots) {
        // Slots.
        // DrawPrimitives::setDrawColor4B(0, 0, 255, 255);
        glLineWidth(1);
        V3F_C4B_T2F_Quad quad;
        for (int i = 0, n = _skeleton->slotsCount; i < n; i++) {
            spSlot* slot = _skeleton->drawOrder[i];
            if (!slot->attachment || slot->attachment->type != SP_ATTACHMENT_REGION) {
                continue;
            }
            if (slotIsOutRange(*slot, _startSlotIndex, _endSlotIndex)) {
                continue;
            }
            spRegionAttachment* attachment = (spRegionAttachment*)slot->attachment;
            float worldVertices[8];
            spRegionAttachment_computeWorldVertices(attachment, slot->bone, worldVertices, 0, 2);
            const Vec2 points[4] =
            {
                { worldVertices[0], worldVertices[1] },
                { worldVertices[2], worldVertices[3] },
                { worldVertices[4], worldVertices[5] },
                { worldVertices[6], worldVertices[7] }
            };
            drawNode->drawPoly(points, 4, true, Color4F::BLUE);
        }
    }
    
    if (_debugBones) {
        // Bone lengths.
        glLineWidth(2);
        for (int i = 0, n = _skeleton->bonesCount; i < n; i++) {
            const spBone *bone = _skeleton->bones[i];
            float x = bone->data->length * bone->a + bone->worldX;
            float y = bone->data->length * bone->c + bone->worldY;
            drawNode->drawLine(Vec2(bone->worldX, bone->worldY), Vec2(x, y), Color4F::RED);
        }
        // Bone origins.
        auto color = Color4F::BLUE; // Root bone is blue.
        for (int i = 0, n = _skeleton->bonesCount; i < n; i++) {
            const spBone *bone = _skeleton->bones[i];
            drawNode->drawPoint(Vec2(bone->worldX, bone->worldY), 4, color);
            if (i == 0) color = Color4F::GREEN;
        }
    }
	
	if (_debugMeshes) {
		// Meshes.
		glLineWidth(1);
		for (int i = 0, n = _skeleton->slotsCount; i < n; ++i) {
			spSlot* slot = _skeleton->drawOrder[i];
			if (!slot->attachment || slot->attachment->type != SP_ATTACHMENT_MESH) continue;
			spMeshAttachment* attachment = (spMeshAttachment*)slot->attachment;
			VLA(float, worldCoord, attachment->super.worldVerticesLength);
			spVertexAttachment_computeWorldVertices(SUPER(attachment), slot, 0, attachment->super.worldVerticesLength, worldCoord, 0, 2);
			for (int t = 0; t < attachment->trianglesCount; t += 3) {
				// Fetch triangle indices
				const int idx0 = attachment->triangles[t + 0];
				const int idx1 = attachment->triangles[t + 1];
				const int idx2 = attachment->triangles[t + 2];
				const Vec2 v[3] =
				{
					worldCoord + (idx0 * 2),
					worldCoord + (idx1 * 2),
					worldCoord + (idx2 * 2)
				};
				drawNode->drawPoly(v, 3, true, Color4F::YELLOW);
			}
		}
		
	}
	
	drawNode->draw(renderer, transform, transformFlags);
	director->popMatrix(MATRIX_STACK_TYPE::MATRIX_STACK_MODELVIEW);
}

AttachmentVertices* SkeletonRenderer::getAttachmentVertices (spRegionAttachment* attachment) const {
	return (AttachmentVertices*)attachment->rendererObject;
}

AttachmentVertices* SkeletonRenderer::getAttachmentVertices (spMeshAttachment* attachment) const {
	return (AttachmentVertices*)attachment->rendererObject;
}

cocos2d::Rect SkeletonRenderer::getBoundingBox () const {
	return _boundingRect;
}

// --- Convenience methods for Skeleton_* functions.

void SkeletonRenderer::updateWorldTransform () {
	spSkeleton_updateWorldTransform(_skeleton);
}

void SkeletonRenderer::setToSetupPose () {
	spSkeleton_setToSetupPose(_skeleton);
}
void SkeletonRenderer::setBonesToSetupPose () {
	spSkeleton_setBonesToSetupPose(_skeleton);
}
void SkeletonRenderer::setSlotsToSetupPose () {
	spSkeleton_setSlotsToSetupPose(_skeleton);
}

spBone* SkeletonRenderer::findBone (const std::string& boneName) const {
	return spSkeleton_findBone(_skeleton, boneName.c_str());
}

spSlot* SkeletonRenderer::findSlot (const std::string& slotName) const {
	return spSkeleton_findSlot(_skeleton, slotName.c_str());
}

bool SkeletonRenderer::setSkin (const std::string& skinName) {
	return spSkeleton_setSkinByName(_skeleton, skinName.empty() ? 0 : skinName.c_str()) ? true : false;
}
bool SkeletonRenderer::setSkin (const char* skinName) {
	return spSkeleton_setSkinByName(_skeleton, skinName) ? true : false;
}

spAttachment* SkeletonRenderer::getAttachment (const std::string& slotName, const std::string& attachmentName) const {
	return spSkeleton_getAttachmentForSlotName(_skeleton, slotName.c_str(), attachmentName.c_str());
}
bool SkeletonRenderer::setAttachment (const std::string& slotName, const std::string& attachmentName) {
	return spSkeleton_setAttachment(_skeleton, slotName.c_str(), attachmentName.empty() ? 0 : attachmentName.c_str()) ? true : false;
}
bool SkeletonRenderer::setAttachment (const std::string& slotName, const char* attachmentName) {
	return spSkeleton_setAttachment(_skeleton, slotName.c_str(), attachmentName) ? true : false;
}
	
void SkeletonRenderer::setTwoColorTint(bool enabled) {
	setupGLProgramState(enabled);
}

bool SkeletonRenderer::isTwoColorTint() {
	return getGLProgramState() == SkeletonTwoColorBatch::getInstance()->getTwoColorTintProgramState();
}
	
void SkeletonRenderer::setVertexEffect(spVertexEffect *effect) {
	this->_effect = effect;
}
	
void SkeletonRenderer::setSlotsRange(int startSlotIndex, int endSlotIndex) {
	_startSlotIndex = startSlotIndex == -1 ? 0 : startSlotIndex;
	_endSlotIndex = endSlotIndex == -1 ? std::numeric_limits<int>::max() : endSlotIndex;
}

spSkeleton* SkeletonRenderer::getSkeleton () const {
	return _skeleton;
}

void SkeletonRenderer::setTimeScale (float scale) {
	_timeScale = scale;
}
float SkeletonRenderer::getTimeScale () const {
	return _timeScale;
}

void SkeletonRenderer::setDebugSlotsEnabled (bool enabled) {
	_debugSlots = enabled;
}
bool SkeletonRenderer::getDebugSlotsEnabled () const {
	return _debugSlots;
}

void SkeletonRenderer::setDebugBonesEnabled (bool enabled) {
	_debugBones = enabled;
}
bool SkeletonRenderer::getDebugBonesEnabled () const {
	return _debugBones;
}
	
void SkeletonRenderer::setDebugMeshesEnabled (bool enabled) {
	_debugMeshes = enabled;
}
bool SkeletonRenderer::getDebugMeshesEnabled () const {
	return _debugMeshes;
}
    
void SkeletonRenderer::setDebugBoundingRectEnabled(bool enabled) {
    _debugBoundingRect = enabled;
}

bool SkeletonRenderer::getDebugBoundingRectEnabled() const {
    return _debugBoundingRect;
}
    
void SkeletonRenderer::onEnter () {
#if CC_ENABLE_SCRIPT_BINDING
	if (_scriptType == kScriptTypeJavascript && ScriptEngineManager::sendNodeEventToJSExtended(this, kNodeOnEnter)) return;
#endif
	Node::onEnter();
	scheduleUpdate();
}

void SkeletonRenderer::onExit () {
#if CC_ENABLE_SCRIPT_BINDING
	if (_scriptType == kScriptTypeJavascript && ScriptEngineManager::sendNodeEventToJSExtended(this, kNodeOnExit)) return;
#endif
	Node::onExit();
	unscheduleUpdate();
}

// --- CCBlendProtocol

const BlendFunc& SkeletonRenderer::getBlendFunc () const {
	return _blendFunc;
}

void SkeletonRenderer::setBlendFunc (const BlendFunc &blendFunc) {
	_blendFunc = blendFunc;
}

void SkeletonRenderer::setOpacityModifyRGB (bool value) {
	_premultipliedAlpha = value;
}

bool SkeletonRenderer::isOpacityModifyRGB () const {
	return _premultipliedAlpha;
}

    
    cocos2d::Rect computeBoundingRect(const float* coords, int vertexCount)
    {
        assert(coords);
        assert(vertexCount > 0);

        const float* v = coords;
        float minX = v[0];
        float minY = v[1];
        float maxX = minX;
        float maxY = minY;
        for (int i = 1; i < vertexCount; ++i)
        {
            v += 2;
            float x = v[0];
            float y = v[1];
            minX = std::min(minX, x);
            minY = std::min(minY, y);
            maxX = std::max(maxX, x);
            maxY = std::max(maxY, y);
        }
        return { minX, minY, maxX - minX, maxY - minY };
    }
    
    bool slotIsOutRange(const spSlot& slot, int startSlotIndex, int endSlotIndex)
    {
        return startSlotIndex > slot.data->index || endSlotIndex < slot.data->index;
    }
    
    int computeTotalCoordCount(const spSkeleton& skeleton, int startSlotIndex, int endSlotIndex)
    {
        int coordCount = 0;
        for (int i = 0; i < skeleton.slotsCount; ++i)
        {
            const spSlot* slot = skeleton.slots[i];
            if (!slot->attachment)
            {
                continue;
            }
            if (slotIsOutRange(*slot, startSlotIndex, endSlotIndex))
            {
                continue;
            }
            if (slot->attachment->type == SP_ATTACHMENT_REGION)
            {
                coordCount += 8;
            }
            else if (slot->attachment->type == SP_ATTACHMENT_MESH)
            {
                const spMeshAttachment* mesh = reinterpret_cast<const spMeshAttachment*>(slot->attachment);
                coordCount += mesh->super.worldVerticesLength;
            }
        }
        return coordCount;
    }
    
    
    void transformWorldVertices(float* dstCoord, int coordCount, const spSkeleton& skeleton, int startSlotIndex, int endSlotIndex)
    {
        float* dstPtr = dstCoord;
#ifndef NDEBUG
        float* const dstEnd = dstCoord + coordCount;
#endif
        for (int i = 0; i < skeleton.slotsCount; ++i)
        {
            /*const*/ spSlot& slot = *skeleton.drawOrder[i]; // match the draw order of SkeletonRenderer::Draw
            if (!slot.attachment)
            {
                continue;
            }
            if (slotIsOutRange(slot, startSlotIndex, endSlotIndex))
            {
                continue;
            }
            if (slot.attachment->type == SP_ATTACHMENT_REGION)
            {
                spRegionAttachment* attachment = (spRegionAttachment*)slot.attachment;
                assert(dstPtr + 8 <= dstEnd);
                spRegionAttachment_computeWorldVertices(attachment, slot.bone, dstPtr, 0, 2);
                dstPtr += 8;
            }
            else if (slot.attachment->type == SP_ATTACHMENT_MESH)
            {
                spMeshAttachment* mesh = (spMeshAttachment*)slot.attachment;
                assert(dstPtr + mesh->super.worldVerticesLength <= dstEnd);
                spVertexAttachment_computeWorldVertices(SUPER(mesh), &slot, 0, mesh->super.worldVerticesLength, dstPtr, 0, 2);
                dstPtr += mesh->super.worldVerticesLength;
            }
        }
        assert(dstPtr == dstEnd);
    }
    
    void interleaveCoordinates(float* __restrict dst, const float* __restrict src, int count, int dstStride)
    {
        if (dstStride == 2)
        {
            std::memcpy(dst, src, sizeof(float) * count * 2);
        }
        else
        {
            for (int i = 0; i < count; ++i)
            {
                dst[0] = src[0];
                dst[1] = src[1];
                dst += dstStride;
                src += 2;
            }
        }
        
    }
    
    BlendFunc makeBlendFunc(int blendMode, bool premultipliedAlpha)
    {
        BlendFunc blendFunc;
        switch (blendMode) {
            case SP_BLEND_MODE_ADDITIVE:
                blendFunc.src = premultipliedAlpha ? GL_ONE : GL_SRC_ALPHA;
                blendFunc.dst = GL_ONE;
                break;
            case SP_BLEND_MODE_MULTIPLY:
                blendFunc.src = GL_DST_COLOR;
                blendFunc.dst = GL_ONE_MINUS_SRC_ALPHA;
                break;
            case SP_BLEND_MODE_SCREEN:
                blendFunc.src = GL_ONE;
                blendFunc.dst = GL_ONE_MINUS_SRC_COLOR;
                break;
            default:
                blendFunc.src = premultipliedAlpha ? GL_ONE : GL_SRC_ALPHA;
                blendFunc.dst = GL_ONE_MINUS_SRC_ALPHA;
                break;
        }
        return blendFunc;
    }
    
    
    bool cullRectangle(const Mat4 &transform, const cocos2d::Rect& rect, const Camera& camera)
    {
        // Compute rectangle center and half extents in local space
        // TODO: Pass the bounding rectangle with this representation directly
        const float halfRectWidth = rect.size.width * 0.5f;
        const float halfRectHeight = rect.size.height * 0.5f;
        const float l_cx = rect.origin.x + halfRectWidth;
        const float l_cy = rect.origin.y + halfRectHeight;
        
        // Transform rectangle center to world space
        const float w_cx = (l_cx * transform.m[0] + l_cy * transform.m[4]) + transform.m[12];
        const float w_cy = (l_cx * transform.m[1] + l_cy * transform.m[5]) + transform.m[13];
        
        // Compute rectangle half extents in world space
        const float w_ex = std::abs(halfRectWidth * transform.m[0]) + std::abs(halfRectHeight * transform.m[4]);
        const float w_ey = std::abs(halfRectWidth * transform.m[1]) + std::abs(halfRectHeight * transform.m[5]);
        
        // Transform rectangle to clip space
        const Mat4& viewMatrix = camera.getViewMatrix();
        const Mat4& projectionMatrix = camera.getProjectionMatrix();
        const float c_cx = (w_cx + viewMatrix.m[12]) * projectionMatrix.m[0];
        const float c_cy = (w_cy + viewMatrix.m[13]) * projectionMatrix.m[5];
        const float c_ex = w_ex * projectionMatrix.m[0];
        const float c_ey = w_ey * projectionMatrix.m[5];
        // The rectangle has z == 0 in world space
        // cw = projectionMatrix[11] * vz = -vz = wz -viewMatrix.m[14] = -viewMatrix.m[14]
        const float c_w = -viewMatrix.m[14]; // w in clip space
        
        // For each edge, test the rectangle corner closest to it
        // If its distance to the edge is negative, the whole rectangle is outside the screen
        // Note: the test is conservative and can return false positives in some cases
        // The test is done in clip space [-1, +1]
        // e.g. left culling <==> (c_cx + c_ex) / cw < -1 <==> (c_cx + c_ex) < -cw
        
        // Left
        if (c_cx + c_ex < -c_w)
        {
            return true;
        }
        
        // Right
        if (c_cx - c_ex > c_w)
        {
            return true;
        }
        
        // Bottom
        if (c_cy + c_ey < -c_w)
        {
            return true;
        }
        
        // Top
        if (c_cy - c_ey > c_w)
        {
            return true;
        }
        
        return false;
    }

   
    Color4B spColorToColor4B(const spColor& color)
    {
        return { (GLubyte)(color.r * 255.f), (GLubyte)(color.g * 255.f), (GLubyte)(color.b * 255.f), (GLubyte)(color.a * 255.f) };
    }
    
}
