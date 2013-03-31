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
#include <spine/spine.h>
#include <spine/extension.h>
#include <SFML/Graphics/Vertex.hpp>
#include <SFML/Graphics/VertexArray.hpp>
#include <SFML/Graphics/Texture.hpp>
#include <SFML/Graphics/RenderTarget.hpp>
#include <SFML/Graphics/RenderStates.hpp>

using sf::Quads;
using sf::RenderTarget;
using sf::RenderStates;
using sf::Texture;
using sf::Uint8;
using sf::Vertex;
using sf::VertexArray;

namespace spine {

void _SfmlAtlasPage_free (AtlasPage* page) {
	SfmlAtlasPage* self = SUB_CAST(SfmlAtlasPage, page);
	_AtlasPage_deinit(SUPER(self));

	delete self->texture;

	FREE(page);
}

AtlasPage* AtlasPage_new (const char* name) {
	SfmlAtlasPage* self = NEW(SfmlAtlasPage);
	_AtlasPage_init(SUPER(self), name);
	VTABLE(AtlasPage, self) ->free = _SfmlAtlasPage_free;

	self->texture = new Texture();
	self->texture->loadFromFile(name);

	return SUPER(self);
}

/**/

void _SfmlSkeleton_free (Skeleton* skeleton) {
	SfmlSkeleton* self = SUB_CAST(SfmlSkeleton, skeleton);
	_Skeleton_deinit(SUPER(self));

	delete self->vertexArray;
	delete self->drawable;

	FREE(self);
}

Skeleton* Skeleton_new (SkeletonData* data) {
	Bone_setYDown(1);

	SfmlSkeleton* self = NEW(SfmlSkeleton);
	_Skeleton_init(SUPER(self), data);
	VTABLE(Skeleton, self) ->free = _SfmlSkeleton_free;

	self->drawable = new SkeletonDrawable(SUPER(self));
	self->vertexArray = new VertexArray(Quads, data->boneCount * 4);

	return SUPER(self);
}

SkeletonDrawable& Skeleton_getDrawable (const Skeleton* self) {
	return *SUB_CAST(SfmlSkeleton, self) ->drawable;
}

SkeletonDrawable::SkeletonDrawable (Skeleton* self) :
				skeleton(SUB_CAST(SfmlSkeleton, self) ) {
}

void SkeletonDrawable::draw (RenderTarget& target, RenderStates states) const {
	skeleton->vertexArray->clear();
	for (int i = 0; i < SUPER(skeleton)->slotCount; ++i)
		if (SUPER(skeleton)->slots[i]->attachment)
			Attachment_draw(SUPER(skeleton)->slots[i]->attachment, SUPER(skeleton)->slots[i]);
	states.texture = skeleton->texture;
	target.draw(*skeleton->vertexArray, states);
}

/**/

void _SfmlRegionAttachment_free (Attachment* self) {
	_RegionAttachment_deinit(SUB_CAST(RegionAttachment, self) );
	FREE(self);
}

void _SfmlRegionAttachment_draw (Attachment* attachment, Slot* slot) {
	SfmlRegionAttachment* self = SUB_CAST(SfmlRegionAttachment, attachment);
	SfmlSkeleton* skeleton = (SfmlSkeleton*)slot->skeleton;
	Uint8 r = SUPER(skeleton)->r * slot->r * 255;
	Uint8 g = SUPER(skeleton)->g * slot->g * 255;
	Uint8 b = SUPER(skeleton)->b * slot->b * 255;
	Uint8 a = SUPER(skeleton)->a * slot->a * 255;
	sf::Vertex* vertices = self->vertices;
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

	float* offset = SUPER(self)->offset;
	Bone* bone = slot->bone;
	vertices[0].position.x = offset[0] * bone->m00 + offset[1] * bone->m01 + bone->worldX;
	vertices[0].position.y = offset[0] * bone->m10 + offset[1] * bone->m11 + bone->worldY;
	vertices[1].position.x = offset[2] * bone->m00 + offset[3] * bone->m01 + bone->worldX;
	vertices[1].position.y = offset[2] * bone->m10 + offset[3] * bone->m11 + bone->worldY;
	vertices[2].position.x = offset[4] * bone->m00 + offset[5] * bone->m01 + bone->worldX;
	vertices[2].position.y = offset[4] * bone->m10 + offset[5] * bone->m11 + bone->worldY;
	vertices[3].position.x = offset[6] * bone->m00 + offset[7] * bone->m01 + bone->worldX;
	vertices[3].position.y = offset[6] * bone->m10 + offset[7] * bone->m11 + bone->worldY;

	// SMFL doesn't handle batching for us, so we'll just force a single texture per skeleton.
	skeleton->texture = self->texture;
	skeleton->vertexArray->append(vertices[0]);
	skeleton->vertexArray->append(vertices[1]);
	skeleton->vertexArray->append(vertices[2]);
	skeleton->vertexArray->append(vertices[3]);
}

RegionAttachment* RegionAttachment_new (const char* name, AtlasRegion* region) {
	SfmlRegionAttachment* self = NEW(SfmlRegionAttachment);
	_RegionAttachment_init(SUPER(self), name);
	VTABLE(Attachment, self) ->free = _SfmlRegionAttachment_free;
	VTABLE(Attachment, self) ->draw = _SfmlRegionAttachment_draw;

	self->texture = ((SfmlAtlasPage*)region->page)->texture;
	int u = region->x;
	int u2 = u + region->width;
	int v = region->y;
	int v2 = v + region->height;
	if (region->rotate) {
		self->vertices[1].texCoords.x = u;
		self->vertices[1].texCoords.y = v2;
		self->vertices[2].texCoords.x = u;
		self->vertices[2].texCoords.y = v;
		self->vertices[3].texCoords.x = u2;
		self->vertices[3].texCoords.y = v;
		self->vertices[0].texCoords.x = u2;
		self->vertices[0].texCoords.y = v2;
	} else {
		self->vertices[0].texCoords.x = u;
		self->vertices[0].texCoords.y = v2;
		self->vertices[1].texCoords.x = u;
		self->vertices[1].texCoords.y = v;
		self->vertices[2].texCoords.x = u2;
		self->vertices[2].texCoords.y = v;
		self->vertices[3].texCoords.x = u2;
		self->vertices[3].texCoords.y = v2;
	}

	return SUPER(self);
}

}
