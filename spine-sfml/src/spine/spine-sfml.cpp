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

#include <spine/spine-sfml.h>
#include <spine/extension.h>
#include <SFML/Graphics/Vertex.hpp>
#include <SFML/Graphics/VertexArray.hpp>
#include <SFML/Graphics/Texture.hpp>
#include <SFML/Graphics/RenderTarget.hpp>
#include <SFML/Graphics/RenderStates.hpp>

using namespace sf;

void _AtlasPage_createTexture (AtlasPage* self, const char* path) {
	Texture* texture = new Texture();
	if (!texture->loadFromFile(path)) return;
	texture->setSmooth(true);
	self->rendererObject = texture;
	Vector2u size = texture->getSize();
	self->width = size.x;
	self->height = size.y;
}

void _AtlasPage_disposeTexture (AtlasPage* self) {
	delete (Texture*)self->rendererObject;
}

char* _Util_readFile (const char* path, int* length) {
	return _readFile(path, length);
}

/**/

namespace spine {

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
	states.blendMode = BlendAlpha;

	float worldVertices[8];
	for (int i = 0; i < skeleton->slotCount; ++i) {
		Slot* slot = skeleton->drawOrder[i];
		Attachment* attachment = slot->attachment;
		if (!attachment || attachment->type != ATTACHMENT_REGION) continue;
		RegionAttachment* regionAttachment = (RegionAttachment*)attachment;

		BlendMode blend = slot->data->additiveBlending ? BlendAdd : BlendAlpha;
		if (states.blendMode != blend) {
			target.draw(*vertexArray, states);
			vertexArray->clear();
			states.blendMode = blend;
		}

		RegionAttachment_computeWorldVertices(regionAttachment, slot->skeleton->x, slot->skeleton->y, slot->bone, worldVertices);

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

		vertices[0].position.x = worldVertices[VERTEX_X1];
		vertices[0].position.y = worldVertices[VERTEX_Y1];
		vertices[1].position.x = worldVertices[VERTEX_X2];
		vertices[1].position.y = worldVertices[VERTEX_Y2];
		vertices[2].position.x = worldVertices[VERTEX_X3];
		vertices[2].position.y = worldVertices[VERTEX_Y3];
		vertices[3].position.x = worldVertices[VERTEX_X4];
		vertices[3].position.y = worldVertices[VERTEX_Y4];

		// SMFL doesn't handle batching for us, so we'll just force a single texture per skeleton.
		states.texture = (Texture*)((AtlasRegion*)regionAttachment->rendererObject)->page->rendererObject;

		Vector2u size = states.texture->getSize();
		vertices[0].texCoords.x = regionAttachment->uvs[VERTEX_X1] * size.x;
		vertices[0].texCoords.y = regionAttachment->uvs[VERTEX_Y1] * size.y;
		vertices[1].texCoords.x = regionAttachment->uvs[VERTEX_X2] * size.x;
		vertices[1].texCoords.y = regionAttachment->uvs[VERTEX_Y2] * size.y;
		vertices[2].texCoords.x = regionAttachment->uvs[VERTEX_X3] * size.x;
		vertices[2].texCoords.y = regionAttachment->uvs[VERTEX_Y3] * size.y;
		vertices[3].texCoords.x = regionAttachment->uvs[VERTEX_X4] * size.x;
		vertices[3].texCoords.y = regionAttachment->uvs[VERTEX_Y4] * size.y;

		vertexArray->append(vertices[0]);
		vertexArray->append(vertices[1]);
		vertexArray->append(vertices[2]);
		vertexArray->append(vertices[3]);
	}
	target.draw(*vertexArray, states);
}

} /* namespace spine */
