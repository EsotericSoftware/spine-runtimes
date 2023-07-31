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

#include <spine/RegionAttachment.h>
#include <spine/extension.h>

typedef enum {
	BLX = 0,
	BLY,
	ULX,
	ULY,
	URX,
	URY,
	BRX,
	BRY
} spVertexIndex;

void _spRegionAttachment_dispose(spAttachment *attachment) {
	spRegionAttachment *self = SUB_CAST(spRegionAttachment, attachment);
	if (self->sequence) spSequence_dispose(self->sequence);
	_spAttachment_deinit(attachment);
	FREE(self->path);
	FREE(self);
}

spAttachment *_spRegionAttachment_copy(spAttachment *attachment) {
	spRegionAttachment *self = SUB_CAST(spRegionAttachment, attachment);
	spRegionAttachment *copy = spRegionAttachment_create(attachment->name);
	copy->region = self->region;
	copy->rendererObject = self->rendererObject;
	MALLOC_STR(copy->path, self->path);
	copy->x = self->x;
	copy->y = self->y;
	copy->scaleX = self->scaleX;
	copy->scaleY = self->scaleY;
	copy->rotation = self->rotation;
	copy->width = self->width;
	copy->height = self->height;
	memcpy(copy->uvs, self->uvs, sizeof(float) * 8);
	memcpy(copy->offset, self->offset, sizeof(float) * 8);
	spColor_setFromColor(&copy->color, &self->color);
	copy->sequence = self->sequence ? spSequence_copy(self->sequence) : NULL;
	return SUPER(copy);
}

spRegionAttachment *spRegionAttachment_create(const char *name) {
	spRegionAttachment *self = NEW(spRegionAttachment);
	self->scaleX = 1;
	self->scaleY = 1;
	spColor_setFromFloats(&self->color, 1, 1, 1, 1);
	_spAttachment_init(SUPER(self), name, SP_ATTACHMENT_REGION, _spRegionAttachment_dispose, _spRegionAttachment_copy);
	return self;
}

void spRegionAttachment_updateRegion(spRegionAttachment *self) {
	float regionScaleX, regionScaleY, localX, localY, localX2, localY2;
	float radians, cosine, sine;
	float localXCos, localXSin, localYCos, localYSin, localX2Cos, localX2Sin, localY2Cos, localY2Sin;

	if (self->region == NULL) {
		self->uvs[0] = 0;
		self->uvs[1] = 0;
		self->uvs[2] = 1;
		self->uvs[3] = 1;
		self->uvs[4] = 1;
		self->uvs[5] = 0;
		self->uvs[6] = 0;
		self->uvs[7] = 0;
		return;
	}

	regionScaleX = self->width / self->region->originalWidth * self->scaleX;
	regionScaleY = self->height / self->region->originalHeight * self->scaleY;
	localX = -self->width / 2 * self->scaleX + self->region->offsetX * regionScaleX;
	localY = -self->height / 2 * self->scaleY + self->region->offsetY * regionScaleY;
	localX2 = localX + self->region->width * regionScaleX;
	localY2 = localY + self->region->height * regionScaleY;
	radians = self->rotation * DEG_RAD;
	cosine = COS(radians), sine = SIN(radians);
	localXCos = localX * cosine + self->x;
	localXSin = localX * sine;
	localYCos = localY * cosine + self->y;
	localYSin = localY * sine;
	localX2Cos = localX2 * cosine + self->x;
	localX2Sin = localX2 * sine;
	localY2Cos = localY2 * cosine + self->y;
	localY2Sin = localY2 * sine;

	self->offset[BLX] = localXCos - localYSin;
	self->offset[BLY] = localYCos + localXSin;
	self->offset[ULX] = localXCos - localY2Sin;
	self->offset[ULY] = localY2Cos + localXSin;
	self->offset[URX] = localX2Cos - localY2Sin;
	self->offset[URY] = localY2Cos + localX2Sin;
	self->offset[BRX] = localX2Cos - localYSin;
	self->offset[BRY] = localYCos + localX2Sin;

	if (self->region->degrees == 90) {
		self->uvs[URX] = self->region->u;
		self->uvs[URY] = self->region->v2;
		self->uvs[BRX] = self->region->u;
		self->uvs[BRY] = self->region->v;
		self->uvs[BLX] = self->region->u2;
		self->uvs[BLY] = self->region->v;
		self->uvs[ULX] = self->region->u2;
		self->uvs[ULY] = self->region->v2;
	} else {
		self->uvs[ULX] = self->region->u;
		self->uvs[ULY] = self->region->v2;
		self->uvs[URX] = self->region->u;
		self->uvs[URY] = self->region->v;
		self->uvs[BRX] = self->region->u2;
		self->uvs[BRY] = self->region->v;
		self->uvs[BLX] = self->region->u2;
		self->uvs[BLY] = self->region->v2;
	}
}

void spRegionAttachment_computeWorldVertices(spRegionAttachment *self, spSlot *slot, float *vertices, int offset,
											 int stride) {
	const float *offsets = self->offset;
	spBone *bone = slot->bone;
	float x = bone->worldX, y = bone->worldY;
	float offsetX, offsetY;

	if (self->sequence) spSequence_apply(self->sequence, slot, SUPER(self));

	offsetX = offsets[BRX];
	offsetY = offsets[BRY];
	vertices[offset] = offsetX * bone->a + offsetY * bone->b + x; /* br */
	vertices[offset + 1] = offsetX * bone->c + offsetY * bone->d + y;
	offset += stride;

	offsetX = offsets[BLX];
	offsetY = offsets[BLY];
	vertices[offset] = offsetX * bone->a + offsetY * bone->b + x; /* bl */
	vertices[offset + 1] = offsetX * bone->c + offsetY * bone->d + y;
	offset += stride;

	offsetX = offsets[ULX];
	offsetY = offsets[ULY];
	vertices[offset] = offsetX * bone->a + offsetY * bone->b + x; /* ul */
	vertices[offset + 1] = offsetX * bone->c + offsetY * bone->d + y;
	offset += stride;

	offsetX = offsets[URX];
	offsetY = offsets[URY];
	vertices[offset] = offsetX * bone->a + offsetY * bone->b + x; /* ur */
	vertices[offset + 1] = offsetX * bone->c + offsetY * bone->d + y;
}
