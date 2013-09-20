/******************************************************************************
 * Spine Runtime Software License - Version 1.0
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Single User License or Spine Professional License must be
 *    purchased from Esoteric Software and the license must remain valid:
 *    http://esotericsoftware.com/
 * 2. Redistributions of source code must retain this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer.
 * 3. Redistributions in binary form must reproduce this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer, in the documentation and/or other materials provided with the
 *    distribution.
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
 *****************************************************************************/

#include <spine/spine-cocos2d-iphone.h>
#include <spine/extension.h>

void _AtlasPage_createTexture (AtlasPage* self, const char* path) {
	CCTexture2D* texture = [[CCTextureCache sharedTextureCache] addImage:@(path)];
	CCTextureAtlas* textureAtlas = [[CCTextureAtlas alloc] initWithTexture:texture capacity:4];
	self->rendererObject = textureAtlas;
	CGSize size = texture.contentSizeInPixels;
	self->width = size.width;
	self->height = size.height;
}

void _AtlasPage_disposeTexture (AtlasPage* self) {
	[(CCTextureAtlas*)self->rendererObject release];
}

char* _Util_readFile (const char* path, int* length) {
	return _readFile([[[CCFileUtils sharedFileUtils] fullPathForFilename:@(path)] UTF8String], length);
}

/**/

void RegionAttachment_updateQuad (RegionAttachment* self, Slot* slot, ccV3F_C4B_T2F_Quad* quad, bool premultipliedAlpha) {
	float vertices[8];
	RegionAttachment_computeVertices(self, slot->skeleton->x, slot->skeleton->y, slot->bone, vertices);

	GLubyte r = slot->skeleton->r * slot->r * 255;
	GLubyte g = slot->skeleton->g * slot->g * 255;
	GLubyte b = slot->skeleton->b * slot->b * 255;
	float normalizedAlpha = slot->skeleton->a * slot->a;
	if (premultipliedAlpha) {
		r *= normalizedAlpha;
		g *= normalizedAlpha;
		b *= normalizedAlpha;
	}
	GLubyte a = normalizedAlpha * 255;
	quad->bl.colors.r = r;
	quad->bl.colors.g = g;
	quad->bl.colors.b = b;
	quad->bl.colors.a = a;
	quad->tl.colors.r = r;
	quad->tl.colors.g = g;
	quad->tl.colors.b = b;
	quad->tl.colors.a = a;
	quad->tr.colors.r = r;
	quad->tr.colors.g = g;
	quad->tr.colors.b = b;
	quad->tr.colors.a = a;
	quad->br.colors.r = r;
	quad->br.colors.g = g;
	quad->br.colors.b = b;
	quad->br.colors.a = a;

	quad->bl.vertices.x = vertices[VERTEX_X1];
	quad->bl.vertices.y = vertices[VERTEX_Y1];
	quad->tl.vertices.x = vertices[VERTEX_X2];
	quad->tl.vertices.y = vertices[VERTEX_Y2];
	quad->tr.vertices.x = vertices[VERTEX_X3];
	quad->tr.vertices.y = vertices[VERTEX_Y3];
	quad->br.vertices.x = vertices[VERTEX_X4];
	quad->br.vertices.y = vertices[VERTEX_Y4];

	quad->bl.texCoords.u = self->uvs[VERTEX_X1];
	quad->bl.texCoords.v = self->uvs[VERTEX_Y1];
	quad->tl.texCoords.u = self->uvs[VERTEX_X2];
	quad->tl.texCoords.v = self->uvs[VERTEX_Y2];
	quad->tr.texCoords.u = self->uvs[VERTEX_X3];
	quad->tr.texCoords.v = self->uvs[VERTEX_Y3];
	quad->br.texCoords.u = self->uvs[VERTEX_X4];
	quad->br.texCoords.v = self->uvs[VERTEX_Y4];
}

#ifdef __cplusplus
}
#endif
