/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include "spine-sdl-cpp.h"
#include <SDL.h>

#define STB_IMAGE_IMPLEMENTATION

#include <stb_image.h>

using namespace spine;

SkeletonDrawable::SkeletonDrawable(SkeletonData *skeletonData, AnimationStateData *animationStateData) {
	Bone::setYDown(true);
	skeleton = new (__FILE__, __LINE__) Skeleton(skeletonData);

	ownsAnimationStateData = animationStateData == 0;
	if (ownsAnimationStateData) animationStateData = new (__FILE__, __LINE__) AnimationStateData(skeletonData);
	animationState = new (__FILE__, __LINE__) AnimationState(animationStateData);
}

SkeletonDrawable::~SkeletonDrawable() {
	if (ownsAnimationStateData) delete animationState->getData();
	delete animationState;
	delete skeleton;
}

void SkeletonDrawable::update(float delta) {
	animationState->update(delta);
	animationState->apply(*skeleton);
    skeleton->update(delta);
	skeleton->updateWorldTransform(Physics_Update);
}

void SkeletonDrawable::draw(SDL_Renderer *renderer) {
	Vector<unsigned short> quadIndices;
	quadIndices.add(0);
	quadIndices.add(1);
	quadIndices.add(2);
	quadIndices.add(2);
	quadIndices.add(3);
	quadIndices.add(0);
	SDL_Texture *texture;
	SDL_Vertex sdlVertex;
	for (unsigned i = 0; i < skeleton->getSlots().size(); ++i) {
		Slot &slot = *skeleton->getDrawOrder()[i];
		Attachment *attachment = slot.getAttachment();
		if (!attachment) {
			clipper.clipEnd(slot);
			continue;
		}

		// Early out if the slot color is 0 or the bone is not active
		if (slot.getColor().a == 0 || !slot.getBone().isActive()) {
			clipper.clipEnd(slot);
			continue;
		}

		Vector<float> *vertices = &worldVertices;
		int verticesCount = 0;
		Vector<float> *uvs = NULL;
		Vector<unsigned short> *indices;
		int indicesCount = 0;
		Color *attachmentColor;

		if (attachment->getRTTI().isExactly(RegionAttachment::rtti)) {
			RegionAttachment *regionAttachment = (RegionAttachment *) attachment;
			attachmentColor = &regionAttachment->getColor();

			// Early out if the slot color is 0
			if (attachmentColor->a == 0) {
				clipper.clipEnd(slot);
				continue;
			}

			worldVertices.setSize(8, 0);
			regionAttachment->computeWorldVertices(slot, worldVertices, 0, 2);
			verticesCount = 4;
			uvs = &regionAttachment->getUVs();
			indices = &quadIndices;
			indicesCount = 6;
			texture = (SDL_Texture *) regionAttachment->getRegion()->rendererObject;

		} else if (attachment->getRTTI().isExactly(MeshAttachment::rtti)) {
			MeshAttachment *mesh = (MeshAttachment *) attachment;
			attachmentColor = &mesh->getColor();

			// Early out if the slot color is 0
			if (attachmentColor->a == 0) {
				clipper.clipEnd(slot);
				continue;
			}

			worldVertices.setSize(mesh->getWorldVerticesLength(), 0);
			mesh->computeWorldVertices(slot, 0, mesh->getWorldVerticesLength(), worldVertices.buffer(), 0, 2);
			texture = (SDL_Texture *) mesh->getRegion()->rendererObject;
			verticesCount = mesh->getWorldVerticesLength() >> 1;
			uvs = &mesh->getUVs();
			indices = &mesh->getTriangles();
			indicesCount = indices->size();

		} else if (attachment->getRTTI().isExactly(ClippingAttachment::rtti)) {
			ClippingAttachment *clip = (ClippingAttachment *) slot.getAttachment();
			clipper.clipStart(slot, clip);
			continue;
		} else
			continue;

		Uint8 r = static_cast<Uint8>(skeleton->getColor().r * slot.getColor().r * attachmentColor->r * 255);
		Uint8 g = static_cast<Uint8>(skeleton->getColor().g * slot.getColor().g * attachmentColor->g * 255);
		Uint8 b = static_cast<Uint8>(skeleton->getColor().b * slot.getColor().b * attachmentColor->b * 255);
		Uint8 a = static_cast<Uint8>(skeleton->getColor().a * slot.getColor().a * attachmentColor->a * 255);
		sdlVertex.color.r = r;
		sdlVertex.color.g = g;
		sdlVertex.color.b = b;
		sdlVertex.color.a = a;

		if (clipper.isClipping()) {
			clipper.clipTriangles(worldVertices, *indices, *uvs, 2);
			vertices = &clipper.getClippedVertices();
			verticesCount = clipper.getClippedVertices().size() >> 1;
			uvs = &clipper.getClippedUVs();
			indices = &clipper.getClippedTriangles();
			indicesCount = clipper.getClippedTriangles().size();
		}

		sdlVertices.clear();
		for (int ii = 0; ii < verticesCount << 1; ii += 2) {
			sdlVertex.position.x = (*vertices)[ii];
			sdlVertex.position.y = (*vertices)[ii + 1];
			sdlVertex.tex_coord.x = (*uvs)[ii];
			sdlVertex.tex_coord.y = (*uvs)[ii + 1];
			sdlVertices.add(sdlVertex);
		}
		sdlIndices.clear();
		for (int ii = 0; ii < (int) indices->size(); ii++)
			sdlIndices.add((*indices)[ii]);

		switch (slot.getData().getBlendMode()) {
			case BlendMode_Normal:
				SDL_SetTextureBlendMode(texture, SDL_BLENDMODE_BLEND);
				break;
			case BlendMode_Multiply:
				SDL_SetTextureBlendMode(texture, SDL_BLENDMODE_MOD);
				break;
			case BlendMode_Additive:
				SDL_SetTextureBlendMode(texture, SDL_BLENDMODE_ADD);
				break;
			case BlendMode_Screen:
				SDL_SetTextureBlendMode(texture, SDL_BLENDMODE_BLEND);
				break;
		}

		SDL_RenderGeometry(renderer, texture, sdlVertices.buffer(), sdlVertices.size(), sdlIndices.buffer(),
						   indicesCount);
		clipper.clipEnd(slot);
	}
	clipper.clipEnd();
}

SDL_Texture *loadTexture(SDL_Renderer *renderer, const String &path) {
	int width, height, components;
	stbi_uc *imageData = stbi_load(path.buffer(), &width, &height, &components, 4);
	if (!imageData) return nullptr;
	SDL_Texture *texture = SDL_CreateTexture(renderer, SDL_PIXELFORMAT_ABGR8888, SDL_TEXTUREACCESS_STATIC, width,
											 height);
	if (!texture) {
		stbi_image_free(imageData);
		return nullptr;
	}
	if (SDL_UpdateTexture(texture, nullptr, imageData, width * 4)) {
		stbi_image_free(imageData);
		return nullptr;
	}
	stbi_image_free(imageData);
	return texture;
}

void SDLTextureLoader::load(AtlasPage &page, const String &path) {
	SDL_Texture *texture = loadTexture(renderer, path);
	if (!texture) return;
	page.texture = texture;
	SDL_QueryTexture(texture, nullptr, nullptr, &page.width, &page.height);
	switch (page.magFilter) {
		case TextureFilter_Nearest:
			SDL_SetTextureScaleMode(texture, SDL_ScaleModeNearest);
			break;
		case TextureFilter_Linear:
			SDL_SetTextureScaleMode(texture, SDL_ScaleModeLinear);
			break;
		default:
			SDL_SetTextureScaleMode(texture, SDL_ScaleModeBest);
	}
}

void SDLTextureLoader::unload(void *texture) {
	SDL_DestroyTexture((SDL_Texture *) texture);
}

SpineExtension *spine::getDefaultExtension() {
	return new DefaultSpineExtension();
}
