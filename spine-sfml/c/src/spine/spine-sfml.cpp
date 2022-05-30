/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include <spine/spine-sfml.h>

#ifndef SPINE_MESH_VERTEX_COUNT_MAX
#define SPINE_MESH_VERTEX_COUNT_MAX 1000
#endif

using namespace sf;

sf::BlendMode normal = sf::BlendMode(sf::BlendMode::SrcAlpha, sf::BlendMode::OneMinusSrcAlpha);
sf::BlendMode additive = sf::BlendMode(sf::BlendMode::SrcAlpha, sf::BlendMode::One);
sf::BlendMode multiply = sf::BlendMode(sf::BlendMode::DstColor, sf::BlendMode::OneMinusSrcAlpha);
sf::BlendMode screen = sf::BlendMode(sf::BlendMode::One, sf::BlendMode::OneMinusSrcColor);

sf::BlendMode normalPma = sf::BlendMode(sf::BlendMode::One, sf::BlendMode::OneMinusSrcAlpha);
sf::BlendMode additivePma = sf::BlendMode(sf::BlendMode::One, sf::BlendMode::One);
sf::BlendMode multiplyPma = sf::BlendMode(sf::BlendMode::DstColor, sf::BlendMode::OneMinusSrcAlpha);
sf::BlendMode screenPma = sf::BlendMode(sf::BlendMode::One, sf::BlendMode::OneMinusSrcColor);

spColorArray *spColorArray_create(int initialCapacity) {
	spColorArray *array = ((spColorArray *) _spCalloc(1, sizeof(spColorArray), "_file_name_", 48));
	array->size = 0;
	array->capacity = initialCapacity;
	array->items = ((spColor *) _spCalloc(initialCapacity, sizeof(spColor), "_file_name_", 48));
	return array;
}
void spColorArray_dispose(spColorArray *self) {
	_spFree((void *) self->items);
	_spFree((void *) self);
}
void spColorArray_clear(spColorArray *self) { self->size = 0; }
spColorArray *spColorArray_setSize(spColorArray *self, int newSize) {
	self->size = newSize;
	if (self->capacity < newSize) {
		self->capacity = ((8) > ((int) (self->size * 1.75f)) ? (8) : ((int) (self->size * 1.75f)));
		self->items = ((spColor *) _spRealloc(self->items, sizeof(spColor) * (self->capacity)));
	}
	return self;
}
void spColorArray_ensureCapacity(spColorArray *self, int newCapacity) {
	if (self->capacity >= newCapacity) return;
	self->capacity = newCapacity;
	self->items = ((spColor *) _spRealloc(self->items, sizeof(spColor) * (self->capacity)));
}
void spColorArray_add(spColorArray *self, spColor value) {
	if (self->size == self->capacity) {
		self->capacity = ((8) > ((int) (self->size * 1.75f)) ? (8) : ((int) (self->size * 1.75f)));
		self->items = ((spColor *) _spRealloc(self->items, sizeof(spColor) * (self->capacity)));
	}
	self->items[self->size++] = value;
}
void spColorArray_addAll(spColorArray *self, spColorArray *other) {
	int i = 0;
	for (; i < other->size; i++) { spColorArray_add(self, other->items[i]); }
}
void spColorArray_addAllValues(spColorArray *self, spColor *values, int offset, int count) {
	int i = offset, n = offset + count;
	for (; i < n; i++) { spColorArray_add(self, values[i]); }
}
void spColorArray_removeAt(spColorArray *self, int index) {
	self->size--;
	memmove(self->items + index, self->items + index + 1, sizeof(spColor) * (self->size - index));
}

spColor spColorArray_pop(spColorArray *self) {
	spColor item = self->items[--self->size];
	return item;
}
spColor spColorArray_peek(spColorArray *self) { return self->items[self->size - 1]; }

void _spAtlasPage_createTexture(spAtlasPage *self, const char *path) {
	Texture *texture = new Texture();
	if (!texture->loadFromFile(path)) return;

	if (self->magFilter == SP_ATLAS_LINEAR) texture->setSmooth(true);
	if (self->uWrap == SP_ATLAS_REPEAT && self->vWrap == SP_ATLAS_REPEAT) texture->setRepeated(true);

	self->rendererObject = texture;
	Vector2u size = texture->getSize();
	self->width = size.x;
	self->height = size.y;
}

void _spAtlasPage_disposeTexture(spAtlasPage *self) {
	delete (Texture *) self->rendererObject;
}

char *_spUtil_readFile(const char *path, int *length) {
	return _spReadFile(path, length);
}

/**/

namespace spine {

	SkeletonDrawable::SkeletonDrawable(spSkeletonData *skeletonData, spAnimationStateData *stateData) : timeScale(1),
																										vertexArray(new VertexArray(Triangles, skeletonData->bonesCount * 4)),
																										worldVertices(0), clipper(0) {
		spBone_setYDown(true);
		worldVertices = MALLOC(float, SPINE_MESH_VERTEX_COUNT_MAX);
		skeleton = spSkeleton_create(skeletonData);
		tempUvs = spFloatArray_create(16);
		tempColors = spColorArray_create(16);

		ownsAnimationStateData = stateData == 0;
		if (ownsAnimationStateData) stateData = spAnimationStateData_create(skeletonData);

		state = spAnimationState_create(stateData);

		clipper = spSkeletonClipping_create();
	}

	SkeletonDrawable::~SkeletonDrawable() {
		delete vertexArray;
		FREE(worldVertices);
		if (ownsAnimationStateData) spAnimationStateData_dispose(state->data);
		spAnimationState_dispose(state);
		spSkeleton_dispose(skeleton);
		spSkeletonClipping_dispose(clipper);
		spFloatArray_dispose(tempUvs);
		spColorArray_dispose(tempColors);
	}

	void SkeletonDrawable::update(float deltaTime) {
		spAnimationState_update(state, deltaTime * timeScale);
		spAnimationState_apply(state, skeleton);
		spSkeleton_updateWorldTransform(skeleton);
	}

	void SkeletonDrawable::draw(RenderTarget &target, RenderStates states) const {
		vertexArray->clear();
		states.texture = 0;
		unsigned short quadIndices[6] = {0, 1, 2, 2, 3, 0};

		// Early out if skeleton is invisible
		if (skeleton->color.a == 0) return;

		sf::Vertex vertex;
		Texture *texture = 0;
		for (int i = 0; i < skeleton->slotsCount; ++i) {
			spSlot *slot = skeleton->drawOrder[i];
			spAttachment *attachment = slot->attachment;
			if (!attachment) continue;

			// Early out if slot is invisible
			if (slot->color.a == 0 || !slot->bone->active) {
				spSkeletonClipping_clipEnd(clipper, slot);
				continue;
			}

			float *vertices = worldVertices;
			int verticesCount = 0;
			float *uvs = 0;
			unsigned short *indices = 0;
			int indicesCount = 0;
			spColor *attachmentColor;

			if (attachment->type == SP_ATTACHMENT_REGION) {
				spRegionAttachment *regionAttachment = (spRegionAttachment *) attachment;
				attachmentColor = &regionAttachment->color;

				// Early out if slot is invisible
				if (attachmentColor->a == 0) {
					spSkeletonClipping_clipEnd(clipper, slot);
					continue;
				}

				spRegionAttachment_computeWorldVertices(regionAttachment, slot, vertices, 0, 2);
				verticesCount = 4;
				uvs = regionAttachment->uvs;
				indices = quadIndices;
				indicesCount = 6;
				texture = (Texture *) ((spAtlasRegion *) regionAttachment->rendererObject)->page->rendererObject;

			} else if (attachment->type == SP_ATTACHMENT_MESH) {
				spMeshAttachment *mesh = (spMeshAttachment *) attachment;
				attachmentColor = &mesh->color;

				// Early out if slot is invisible
				if (attachmentColor->a == 0) {
					spSkeletonClipping_clipEnd(clipper, slot);
					continue;
				}

				if (mesh->super.worldVerticesLength > SPINE_MESH_VERTEX_COUNT_MAX) continue;
				texture = (Texture *) ((spAtlasRegion *) mesh->rendererObject)->page->rendererObject;
				spVertexAttachment_computeWorldVertices(SUPER(mesh), slot, 0, mesh->super.worldVerticesLength, worldVertices, 0, 2);
				verticesCount = mesh->super.worldVerticesLength >> 1;
				uvs = mesh->uvs;
				indices = mesh->triangles;
				indicesCount = mesh->trianglesCount;

			} else if (attachment->type == SP_ATTACHMENT_CLIPPING) {
				spClippingAttachment *clip = (spClippingAttachment *) slot->attachment;
				spSkeletonClipping_clipStart(clipper, slot, clip);
				continue;
			} else
				continue;

			Uint8 r = static_cast<Uint8>(skeleton->color.r * slot->color.r * attachmentColor->r * 255);
			Uint8 g = static_cast<Uint8>(skeleton->color.g * slot->color.g * attachmentColor->g * 255);
			Uint8 b = static_cast<Uint8>(skeleton->color.b * slot->color.b * attachmentColor->b * 255);
			Uint8 a = static_cast<Uint8>(skeleton->color.a * slot->color.a * attachmentColor->a * 255);
			vertex.color.r = r;
			vertex.color.g = g;
			vertex.color.b = b;
			vertex.color.a = a;

			spColor light;
			light.r = r / 255.0f;
			light.g = g / 255.0f;
			light.b = b / 255.0f;
			light.a = a / 255.0f;

			sf::BlendMode blend;
			if (!usePremultipliedAlpha) {
				switch (slot->data->blendMode) {
					case SP_BLEND_MODE_NORMAL:
						blend = normal;
						break;
					case SP_BLEND_MODE_ADDITIVE:
						blend = additive;
						break;
					case SP_BLEND_MODE_MULTIPLY:
						blend = multiply;
						break;
					case SP_BLEND_MODE_SCREEN:
						blend = screen;
						break;
					default:
						blend = normal;
				}
			} else {
				switch (slot->data->blendMode) {
					case SP_BLEND_MODE_NORMAL:
						blend = normalPma;
						break;
					case SP_BLEND_MODE_ADDITIVE:
						blend = additivePma;
						break;
					case SP_BLEND_MODE_MULTIPLY:
						blend = multiplyPma;
						break;
					case SP_BLEND_MODE_SCREEN:
						blend = screenPma;
						break;
					default:
						blend = normalPma;
				}
			}

			if (states.texture == 0) states.texture = texture;

			if (states.blendMode != blend || states.texture != texture) {
				target.draw(*vertexArray, states);
				vertexArray->clear();
				states.blendMode = blend;
				states.texture = texture;
			}

			if (spSkeletonClipping_isClipping(clipper)) {
				spSkeletonClipping_clipTriangles(clipper, vertices, verticesCount << 1, indices, indicesCount, uvs, 2);
				vertices = clipper->clippedVertices->items;
				verticesCount = clipper->clippedVertices->size >> 1;
				uvs = clipper->clippedUVs->items;
				indices = clipper->clippedTriangles->items;
				indicesCount = clipper->clippedTriangles->size;
			}

			Vector2u size = texture->getSize();

			for (int j = 0; j < indicesCount; ++j) {
				int index = indices[j] << 1;
				vertex.position.x = vertices[index];
				vertex.position.y = vertices[index + 1];
				vertex.texCoords.x = uvs[index] * size.x;
				vertex.texCoords.y = uvs[index + 1] * size.y;
				vertexArray->append(vertex);
			}

			spSkeletonClipping_clipEnd(clipper, slot);
		}
		target.draw(*vertexArray, states);
		spSkeletonClipping_clipEnd2(clipper);
	}

} /* namespace spine */
