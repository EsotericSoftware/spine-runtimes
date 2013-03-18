#include <iostream>
#include <spine-cc/RegionAttachment.h>
#include <spine-cc/Atlas.h>
#include <spine-cc/Skeleton.h>
#include <spine/Bone.h>
#include <spine/Slot.h>

namespace spine {

RegionAttachment::RegionAttachment (AtlasRegion *region) {
	texture = region->page->texture;
    const CCSize& texSize = texture->getContentSizeInPixels();
	float u = region->x / texSize.width;
	float u2 = (region->x + region->width) / texSize.width;
	float v = region->y / texSize.height;
	float v2 = (region->y + region->height) / texSize.height;
	if (region->rotate) {
		vertices.tl.texCoords.u = u;
		vertices.tl.texCoords.v = v2;
		vertices.tr.texCoords.u = u;
		vertices.tr.texCoords.v = v;
		vertices.br.texCoords.u = u2;
		vertices.br.texCoords.v = v;
		vertices.bl.texCoords.u = u2;
		vertices.bl.texCoords.v = v2;
	} else {
		vertices.bl.texCoords.u = u;
		vertices.bl.texCoords.v = v2;
		vertices.tl.texCoords.u = u;
		vertices.tl.texCoords.v = v;
		vertices.tr.texCoords.u = u2;
		vertices.tr.texCoords.v = v;
		vertices.br.texCoords.u = u2;
		vertices.br.texCoords.v = v2;
	}
}

void RegionAttachment::draw (Slot *slot) {
	Skeleton* skeleton = (Skeleton*)slot->skeleton;

	GLubyte r = skeleton->r * slot->r * 255;
	GLubyte g = skeleton->g * slot->g * 255;
	GLubyte b = skeleton->b * slot->b * 255;
	GLubyte a = skeleton->a * slot->a * 255;
	vertices.bl.colors.r = r;
	vertices.bl.colors.g = g;
	vertices.bl.colors.b = b;
	vertices.bl.colors.a = a;
	vertices.tl.colors.r = r;
	vertices.tl.colors.g = g;
	vertices.tl.colors.b = b;
	vertices.tl.colors.a = a;
	vertices.tr.colors.r = r;
	vertices.tr.colors.g = g;
	vertices.tr.colors.b = b;
	vertices.tr.colors.a = a;
	vertices.br.colors.r = r;
	vertices.br.colors.g = g;
	vertices.br.colors.b = b;
	vertices.br.colors.a = a;

	updateWorldVertices(slot->bone);

	// SMFL doesn't handle batching for us, so we'll just force a single texture per skeleton.
	skeleton->texture = texture;
	skeleton->vertexArray.push_back(vertices);
}

void RegionAttachment::updateWorldVertices (spine::Bone *bone) {
	vertices.bl.vertices.x = offset[0] * bone->m00 + offset[1] * bone->m01 + bone->worldX;
	vertices.bl.vertices.y = offset[0] * bone->m10 + offset[1] * bone->m11 + bone->worldY;
    vertices.bl.vertices.z = 0;
	vertices.tl.vertices.x = offset[2] * bone->m00 + offset[3] * bone->m01 + bone->worldX;
	vertices.tl.vertices.y = offset[2] * bone->m10 + offset[3] * bone->m11 + bone->worldY;
    vertices.tl.vertices.z = 0;
	vertices.tr.vertices.x = offset[4] * bone->m00 + offset[5] * bone->m01 + bone->worldX;
	vertices.tr.vertices.y = offset[4] * bone->m10 + offset[5] * bone->m11 + bone->worldY;
    vertices.tr.vertices.z = 0;
	vertices.br.vertices.x = offset[6] * bone->m00 + offset[7] * bone->m01 + bone->worldX;
	vertices.br.vertices.y = offset[6] * bone->m10 + offset[7] * bone->m11 + bone->worldY;
    vertices.br.vertices.z = 0;
}

} /* namespace spine */
