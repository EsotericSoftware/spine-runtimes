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

#include <iostream>
#include <spine-cocos2dx/RegionAttachment.h>
#include <spine-cocos2dx/Atlas.h>
#include <spine-cocos2dx/Skeleton.h>
#include <spine/Bone.h>
#include <spine/Slot.h>

USING_NS_CC;

namespace spine {

RegionAttachment::RegionAttachment (AtlasRegion *region) {
	atlas = region->page->atlas;
	const CCSize &size = region->page->texture->getContentSizeInPixels();
	float u = region->x / size.width;
	float u2 = (region->x + region->width) / size.width;
	float v = region->y / size.height;
	float v2 = (region->y + region->height) / size.height;
	if (region->rotate) {
		quad.tl.texCoords.u = u;
		quad.tl.texCoords.v = v2;
		quad.tr.texCoords.u = u;
		quad.tr.texCoords.v = v;
		quad.br.texCoords.u = u2;
		quad.br.texCoords.v = v;
		quad.bl.texCoords.u = u2;
		quad.bl.texCoords.v = v2;
	} else {
		quad.bl.texCoords.u = u;
		quad.bl.texCoords.v = v2;
		quad.tl.texCoords.u = u;
		quad.tl.texCoords.v = v;
		quad.tr.texCoords.u = u2;
		quad.tr.texCoords.v = v;
		quad.br.texCoords.u = u2;
		quad.br.texCoords.v = v2;
	}

	quad.bl.vertices.z = 0;
	quad.tl.vertices.z = 0;
	quad.tr.vertices.z = 0;
	quad.br.vertices.z = 0;
}

void RegionAttachment::draw (Slot *slot) {
	Skeleton* skeleton = (Skeleton*)slot->skeleton;

	GLubyte r = skeleton->r * slot->r * 255;
	GLubyte g = skeleton->g * slot->g * 255;
	GLubyte b = skeleton->b * slot->b * 255;
	GLubyte a = skeleton->a * slot->a * 255;
	quad.bl.colors.r = r;
	quad.bl.colors.g = g;
	quad.bl.colors.b = b;
	quad.bl.colors.a = a;
	quad.tl.colors.r = r;
	quad.tl.colors.g = g;
	quad.tl.colors.b = b;
	quad.tl.colors.a = a;
	quad.tr.colors.r = r;
	quad.tr.colors.g = g;
	quad.tr.colors.b = b;
	quad.tr.colors.a = a;
	quad.br.colors.r = r;
	quad.br.colors.g = g;
	quad.br.colors.b = b;
	quad.br.colors.a = a;

	updateWorldVertices(slot->bone);

	// cocos2dx doesn't handle batching for us, so we'll just force a single texture per skeleton.
	skeleton->addQuad(atlas, quad);
}

void RegionAttachment::updateWorldVertices (spine::Bone *bone) {
	quad.bl.vertices.x = offset[0] * bone->m00 + offset[1] * bone->m01 + bone->worldX;
	quad.bl.vertices.y = offset[0] * bone->m10 + offset[1] * bone->m11 + bone->worldY;
	quad.tl.vertices.x = offset[2] * bone->m00 + offset[3] * bone->m01 + bone->worldX;
	quad.tl.vertices.y = offset[2] * bone->m10 + offset[3] * bone->m11 + bone->worldY;
	quad.tr.vertices.x = offset[4] * bone->m00 + offset[5] * bone->m01 + bone->worldX;
	quad.tr.vertices.y = offset[4] * bone->m10 + offset[5] * bone->m11 + bone->worldY;
	quad.br.vertices.x = offset[6] * bone->m00 + offset[7] * bone->m01 + bone->worldX;
	quad.br.vertices.y = offset[6] * bone->m10 + offset[7] * bone->m11 + bone->worldY;
}

} /* namespace spine */
