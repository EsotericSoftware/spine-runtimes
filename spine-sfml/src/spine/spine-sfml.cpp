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

#include <spine/spine-sfml.h>
#include <spine/extension.h>
#include <SFML/Graphics/Vertex.hpp>
#include <SFML/Graphics/VertexArray.hpp>
#include <SFML/Graphics/Texture.hpp>
#include <SFML/Graphics/RenderTarget.hpp>
#include <SFML/Graphics/RenderStates.hpp>

using namespace sf;

namespace spine {

void _AtlasPage_createTexture (AtlasPage* self, const char* path) {
	Texture* texture = new Texture();
	if (!texture->loadFromFile(path)) return;
	self->texture = texture;
	Vector2u size = texture->getSize();
	self->width = size.x;
	self->height = size.x;
}

void _AtlasPage_disposeTexture (AtlasPage* self) {
	delete (Texture*)self->texture;
}

char* _Util_readFile (const char* path, int* length) {
	return _readFile(path, length);
}

/**/

SkeletonDrawable::SkeletonDrawable (SkeletonData* skeletonData, AnimationStateData* stateData) :
				timeScale(1),
				vertexArray(new VertexArray(Quads, skeletonData->boneCount * 4)) {
	Bone_setYDown(true);

	skeleton = Skeleton_create(skeletonData);
	state = AnimationState_create(stateData);
}

SkeletonDrawable::~SkeletonDrawable () {
	delete vertexArray;
	AnimationState_dispose(state);
	Skeleton_dispose(skeleton);
}

void SkeletonDrawable::update (float deltaTime) {
	Skeleton_update(skeleton, deltaTime);
	AnimationState_update(state, deltaTime * timeScale);
	AnimationState_apply(state, skeleton);
	Skeleton_updateWorldTransform(skeleton);
}

void SkeletonDrawable::draw (RenderTarget& target, RenderStates states) const {
	vertexArray->clear();
	for (int i = 0; i < skeleton->slotCount; ++i) {
		Slot* slot = skeleton->slots[i];
		Attachment* attachment = slot->attachment;
		if (!attachment || attachment->type != ATTACHMENT_REGION) continue;
		RegionAttachment* regionAttachment = (RegionAttachment*)attachment;
		RegionAttachment_updateVertices(regionAttachment, slot);

		Uint8 r = skeleton->r * slot->r * 255;
		Uint8 g = skeleton->g * slot->g * 255;
		Uint8 b = skeleton->b * slot->b * 255;
		Uint8 a = skeleton->a * slot->a * 255;

		sf::Vertex vertices[4];
		vertices[0].color.r = r;
		vertices[0].color.g = g;
		vertices[0].color.b = b;
		vertices[0].color.a = a;
		vertices[1].color.r = r;
		vertices[1].color.g = g;
		vertices[1].color.b = b;
		vertices[1].color.a = a;
		vertices[2].color.r = r;
		vertices[2].color.g = g;
		vertices[2].color.b = b;
		vertices[2].color.a = a;
		vertices[3].color.r = r;
		vertices[3].color.g = g;
		vertices[3].color.b = b;
		vertices[3].color.a = a;

		vertices[0].position.x = regionAttachment->vertices[VERTEX_X1];
		vertices[0].position.y = regionAttachment->vertices[VERTEX_Y1];
		vertices[1].position.x = regionAttachment->vertices[VERTEX_X2];
		vertices[1].position.y = regionAttachment->vertices[VERTEX_Y2];
		vertices[2].position.x = regionAttachment->vertices[VERTEX_X3];
		vertices[2].position.y = regionAttachment->vertices[VERTEX_Y3];
		vertices[3].position.x = regionAttachment->vertices[VERTEX_X4];
		vertices[3].position.y = regionAttachment->vertices[VERTEX_Y4];

		int u = regionAttachment->region->x;
		int u2 = u + regionAttachment->region->width;
		int v = regionAttachment->region->y;
		int v2 = v + regionAttachment->region->height;
		if (regionAttachment->region->rotate) {
			vertices[1].texCoords.x = u;
			vertices[1].texCoords.y = v2;
			vertices[2].texCoords.x = u;
			vertices[2].texCoords.y = v;
			vertices[3].texCoords.x = u2;
			vertices[3].texCoords.y = v;
			vertices[0].texCoords.x = u2;
			vertices[0].texCoords.y = v2;
		} else {
			vertices[0].texCoords.x = u;
			vertices[0].texCoords.y = v2;
			vertices[1].texCoords.x = u;
			vertices[1].texCoords.y = v;
			vertices[2].texCoords.x = u2;
			vertices[2].texCoords.y = v;
			vertices[3].texCoords.x = u2;
			vertices[3].texCoords.y = v2;
		}

		// SMFL doesn't handle batching for us, so we'll just force a single texture per skeleton.
		states.texture = (Texture*)regionAttachment->region->page->texture;
		vertexArray->append(vertices[0]);
		vertexArray->append(vertices[1]);
		vertexArray->append(vertices[2]);
		vertexArray->append(vertices[3]);
	}
	target.draw(*vertexArray, states);
}

} /* namespace spine */
