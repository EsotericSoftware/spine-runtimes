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

#include "spine-sdl-c.h"
#include <spine/spine.h>
#include <spine/extension.h>

#define STB_IMAGE_IMPLEMENTATION

#include <stb_image.h>

_SP_ARRAY_IMPLEMENT_TYPE_NO_CONTAINS(spSdlVertexArray, SDL_Vertex)

spSkeletonDrawable *spSkeletonDrawable_create(spSkeletonData *skeletonData, spAnimationStateData *animationStateData) {
	spBone_setYDown(-1);
	spSkeletonDrawable *self = NEW(spSkeletonDrawable);
	self->skeleton = spSkeleton_create(skeletonData);
	self->animationState = spAnimationState_create(animationStateData);
    self->usePremultipliedAlpha = 0;
	self->sdlIndices = spIntArray_create(12);
	self->sdlVertices = spSdlVertexArray_create(12);
	self->worldVertices = spFloatArray_create(12);
	self->clipper = spSkeletonClipping_create();
	return self;
}

void spSkeletonDrawable_dispose(spSkeletonDrawable *self) {
	spSkeleton_dispose(self->skeleton);
	spAnimationState_dispose(self->animationState);
	spIntArray_dispose(self->sdlIndices);
	spSdlVertexArray_dispose(self->sdlVertices);
	spFloatArray_dispose(self->worldVertices);
	spSkeletonClipping_dispose(self->clipper);
	FREE(self);
}

void spSkeletonDrawable_update(spSkeletonDrawable *self, float delta, spPhysics physics) {
	spAnimationState_update(self->animationState, delta);
	spAnimationState_apply(self->animationState, self->skeleton);
	spSkeleton_updateWorldTransform(self->skeleton, physics);
}

void spSkeletonDrawable_draw(spSkeletonDrawable *self, struct SDL_Renderer *renderer) {
	static unsigned short quadIndices[] = {0, 1, 2, 2, 3, 0};
	spSkeleton *skeleton = self->skeleton;
	spSkeletonClipping *clipper = self->clipper;
	SDL_Texture *texture;
	SDL_Vertex sdlVertex;
	for (int i = 0; i < skeleton->slotsCount; ++i) {
		spSlot *slot = skeleton->drawOrder[i];
		spAttachment *attachment = slot->attachment;
		if (!attachment) {
			spSkeletonClipping_clipEnd(clipper, slot);
			continue;
		}

		// Early out if the slot color is 0 or the bone is not active
		if (slot->color.a == 0 || !slot->bone->active) {
			spSkeletonClipping_clipEnd(clipper, slot);
			continue;
		}

		spFloatArray *vertices = self->worldVertices;
		int verticesCount = 0;
		float *uvs = NULL;
		unsigned short *indices;
		int indicesCount = 0;
		spColor *attachmentColor = NULL;

		if (attachment->type == SP_ATTACHMENT_REGION) {
			spRegionAttachment *region = (spRegionAttachment *) attachment;
			attachmentColor = &region->color;

			// Early out if the slot color is 0
			if (attachmentColor->a == 0) {
				spSkeletonClipping_clipEnd(clipper, slot);
				continue;
			}

			spFloatArray_setSize(vertices, 8);
			spRegionAttachment_computeWorldVertices(region, slot, vertices->items, 0, 2);
			verticesCount = 4;
			uvs = region->uvs;
			indices = quadIndices;
			indicesCount = 6;
			texture = (SDL_Texture *) ((spAtlasRegion *) region->rendererObject)->page->rendererObject;
		} else if (attachment->type == SP_ATTACHMENT_MESH) {
			spMeshAttachment *mesh = (spMeshAttachment *) attachment;
			attachmentColor = &mesh->color;

			// Early out if the slot color is 0
			if (attachmentColor->a == 0) {
				spSkeletonClipping_clipEnd(clipper, slot);
				continue;
			}

			spFloatArray_setSize(vertices, mesh->super.worldVerticesLength);
			spVertexAttachment_computeWorldVertices(SUPER(mesh), slot, 0, mesh->super.worldVerticesLength, vertices->items, 0, 2);
			verticesCount = mesh->super.worldVerticesLength >> 1;
			uvs = mesh->uvs;
			indices = mesh->triangles;
			indicesCount = mesh->trianglesCount;
			texture = (SDL_Texture *) ((spAtlasRegion *) mesh->rendererObject)->page->rendererObject;
		} else if (attachment->type == SP_ATTACHMENT_CLIPPING) {
			spClippingAttachment *clip = (spClippingAttachment *) slot->attachment;
			spSkeletonClipping_clipStart(clipper, slot, clip);
			continue;
		} else
			continue;

		Uint8 r = (Uint8) (skeleton->color.r * slot->color.r * attachmentColor->r * 255);
		Uint8 g = (Uint8) (skeleton->color.g * slot->color.g * attachmentColor->g * 255);
		Uint8 b = (Uint8) (skeleton->color.b * slot->color.b * attachmentColor->b * 255);
		Uint8 a = (Uint8) (skeleton->color.a * slot->color.a * attachmentColor->a * 255);
		sdlVertex.color.r = r;
		sdlVertex.color.g = g;
		sdlVertex.color.b = b;
		sdlVertex.color.a = a;

		if (spSkeletonClipping_isClipping(clipper)) {
			spSkeletonClipping_clipTriangles(clipper, vertices->items, verticesCount << 1, indices, indicesCount, uvs, 2);
			vertices = clipper->clippedVertices;
			verticesCount = clipper->clippedVertices->size >> 1;
			uvs = clipper->clippedUVs->items;
			indices = clipper->clippedTriangles->items;
			indicesCount = clipper->clippedTriangles->size;
		}

		spSdlVertexArray_clear(self->sdlVertices);
		for (int ii = 0; ii < verticesCount << 1; ii += 2) {
			sdlVertex.position.x = vertices->items[ii];
			sdlVertex.position.y = vertices->items[ii + 1];
			sdlVertex.tex_coord.x = uvs[ii];
			sdlVertex.tex_coord.y = uvs[ii + 1];
			spSdlVertexArray_add(self->sdlVertices, sdlVertex);
		}
		spIntArray_clear(self->sdlIndices);
		for (int ii = 0; ii < (int) indicesCount; ii++)
			spIntArray_add(self->sdlIndices, indices[ii]);

        if (!self->usePremultipliedAlpha) {
            switch (slot->data->blendMode) {
                case SP_BLEND_MODE_NORMAL:
                    SDL_SetTextureBlendMode(texture, SDL_BLENDMODE_BLEND);
                    break;
                case SP_BLEND_MODE_MULTIPLY:
                    SDL_SetTextureBlendMode(texture, SDL_BLENDMODE_MOD);
                    break;
                case SP_BLEND_MODE_ADDITIVE:
                    SDL_SetTextureBlendMode(texture, SDL_BLENDMODE_ADD);
                    break;
                case SP_BLEND_MODE_SCREEN:
                    SDL_SetTextureBlendMode(texture, SDL_BLENDMODE_BLEND);
                    break;
            }
        } else {
            SDL_BlendMode target;
            switch (slot->data->blendMode) {
                case SP_BLEND_MODE_NORMAL:
                    target = SDL_ComposeCustomBlendMode(SDL_BLENDFACTOR_ONE, SDL_BLENDFACTOR_ONE_MINUS_SRC_ALPHA, SDL_BLENDOPERATION_ADD, SDL_BLENDFACTOR_ONE, SDL_BLENDFACTOR_ONE_MINUS_SRC_ALPHA, SDL_BLENDOPERATION_ADD);
                    SDL_SetTextureBlendMode(texture, target);
                    break;
                case SP_BLEND_MODE_MULTIPLY:
                    SDL_SetTextureBlendMode(texture, SDL_BLENDMODE_MOD);
                    break;
                case SP_BLEND_MODE_ADDITIVE:
                    target = SDL_ComposeCustomBlendMode(SDL_BLENDFACTOR_ONE, SDL_BLENDFACTOR_ONE, SDL_BLENDOPERATION_ADD, SDL_BLENDFACTOR_ONE, SDL_BLENDFACTOR_ONE, SDL_BLENDOPERATION_ADD);
                    SDL_SetTextureBlendMode(texture, SDL_BLENDMODE_ADD);
                    break;
                case SP_BLEND_MODE_SCREEN:
                    target = SDL_ComposeCustomBlendMode(SDL_BLENDFACTOR_ONE, SDL_BLENDFACTOR_ONE_MINUS_SRC_ALPHA, SDL_BLENDOPERATION_ADD, SDL_BLENDFACTOR_ONE, SDL_BLENDFACTOR_ONE_MINUS_SRC_ALPHA, SDL_BLENDOPERATION_ADD);
                    SDL_SetTextureBlendMode(texture, SDL_BLENDMODE_BLEND);
                    break;
            }
        }

		SDL_RenderGeometry(renderer, texture, self->sdlVertices->items, self->sdlVertices->size, self->sdlIndices->items,
						   indicesCount);
		spSkeletonClipping_clipEnd(clipper, slot);
	}
	spSkeletonClipping_clipEnd2(clipper);
}

void _spAtlasPage_createTexture(spAtlasPage *self, const char *path) {
	int width, height, components;
	stbi_uc *imageData = stbi_load(path, &width, &height, &components, 4);
	if (!imageData) return;
	SDL_Texture *texture = SDL_CreateTexture((SDL_Renderer *) self->atlas->rendererObject, SDL_PIXELFORMAT_ABGR8888, SDL_TEXTUREACCESS_STATIC, width,
											 height);
	if (!texture) {
		stbi_image_free(imageData);
		return;
	}
	if (SDL_UpdateTexture(texture, NULL, imageData, width * 4)) {
		stbi_image_free(imageData);
		return;
	}
	stbi_image_free(imageData);
	self->rendererObject = texture;
	return;
}

void _spAtlasPage_disposeTexture(spAtlasPage *self) {
	SDL_DestroyTexture((SDL_Texture *) self->rendererObject);
}

char *_spUtil_readFile(const char *path, int *length) {
	return _spReadFile(path, length);
}
