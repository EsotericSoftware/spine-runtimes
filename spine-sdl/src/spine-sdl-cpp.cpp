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
#include <spine/Debug.h>

using namespace spine;

SkeletonRenderer *skeletonRenderer = nullptr;

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

void SkeletonDrawable::update(float delta, Physics physics) {
	animationState->update(delta);
	animationState->apply(*skeleton);
	skeleton->update(delta);
	skeleton->updateWorldTransform(physics);
}

inline void toSDLColor(uint32_t color, SDL_Color *sdlColor) {
	sdlColor->a = (color >> 24) & 0xFF;
	sdlColor->r = (color >> 16) & 0xFF;
	sdlColor->g = (color >> 8) & 0xFF;
	sdlColor->b = color & 0xFF;
}

void SkeletonDrawable::draw(SDL_Renderer *renderer) {
	if (!skeletonRenderer) skeletonRenderer = new (__FILE__, __LINE__) SkeletonRenderer();
	RenderCommand *command = skeletonRenderer->render(*skeleton);
	while (command) {
		float *positions = command->positions;
		float *uvs = command->uvs;
		uint32_t *colors = command->colors;
		sdlVertices.clear();
		for (int ii = 0; ii < command->numVertices << 1; ii += 2) {
			SDL_Vertex sdlVertex;
			sdlVertex.position.x = positions[ii];
			sdlVertex.position.y = positions[ii + 1];
			sdlVertex.tex_coord.x = uvs[ii];
			sdlVertex.tex_coord.y = uvs[ii + 1];
			toSDLColor(colors[ii >> 1], &sdlVertex.color);
			sdlVertices.add(sdlVertex);
		}
		sdlIndices.clear();
		uint16_t *indices = command->indices;
		for (int ii = 0; ii < command->numIndices; ii++)
			sdlIndices.add(indices[ii]);

		BlendMode blendMode = command->blendMode;
		SDL_Texture *texture = (SDL_Texture *) command->texture;
		if (!usePremultipliedAlpha) {
			switch (blendMode) {
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
		} else {
			SDL_BlendMode target;
			switch (blendMode) {
				case BlendMode_Normal:
					target = SDL_ComposeCustomBlendMode(SDL_BLENDFACTOR_ONE, SDL_BLENDFACTOR_ONE_MINUS_SRC_ALPHA, SDL_BLENDOPERATION_ADD, SDL_BLENDFACTOR_ONE, SDL_BLENDFACTOR_ONE_MINUS_SRC_ALPHA, SDL_BLENDOPERATION_ADD);
					SDL_SetTextureBlendMode(texture, target);
					break;
				case BlendMode_Multiply:
					SDL_SetTextureBlendMode(texture, SDL_BLENDMODE_MOD);
					break;
				case BlendMode_Additive:
					target = SDL_ComposeCustomBlendMode(SDL_BLENDFACTOR_ONE, SDL_BLENDFACTOR_ONE, SDL_BLENDOPERATION_ADD, SDL_BLENDFACTOR_ONE, SDL_BLENDFACTOR_ONE, SDL_BLENDOPERATION_ADD);
					SDL_SetTextureBlendMode(texture, SDL_BLENDMODE_ADD);
					break;
				case BlendMode_Screen:
					target = SDL_ComposeCustomBlendMode(SDL_BLENDFACTOR_ONE, SDL_BLENDFACTOR_ONE_MINUS_SRC_ALPHA, SDL_BLENDOPERATION_ADD, SDL_BLENDFACTOR_ONE, SDL_BLENDFACTOR_ONE_MINUS_SRC_ALPHA, SDL_BLENDOPERATION_ADD);
					SDL_SetTextureBlendMode(texture, SDL_BLENDMODE_BLEND);
					break;
			}
		}

		SDL_RenderGeometry(renderer, texture, sdlVertices.buffer(), sdlVertices.size(), sdlIndices.buffer(),
						   command->numIndices);
		command = command->next;
	}
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
