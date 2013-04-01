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

#include <spine/RegionAttachment.h>
#include <math.h>
#include <spine/extension.h>

#ifdef __cplusplus
namespace spine {
#endif

void _RegionAttachment_init (RegionAttachment* self, const char* name) {
	self->scaleX = 1;
	self->scaleY = 1;
	_Attachment_init(SUPER(self), name, ATTACHMENT_REGION);
}

void _RegionAttachment_deinit (RegionAttachment* self) {
	_Attachment_deinit(SUPER(self));
}

void RegionAttachment_updateOffset (RegionAttachment* self) {
	float localX2 = self->width / 2;
	float localY2 = self->height / 2;
	float localX = -localX2;
	float localY = -localY2;
	localX *= self->scaleX;
	localY *= self->scaleY;
	localX2 *= self->scaleX;
	localY2 *= self->scaleY;
	float radians = (float)(self->rotation * 3.1415926535897932385 / 180);
	float cosine = cosf(radians);
	float sine = sinf(radians);
	float localXCos = localX * cosine + self->x;
	float localXSin = localX * sine;
	float localYCos = localY * cosine + self->y;
	float localYSin = localY * sine;
	float localX2Cos = localX2 * cosine + self->x;
	float localX2Sin = localX2 * sine;
	float localY2Cos = localY2 * cosine + self->y;
	float localY2Sin = localY2 * sine;
	self->offset[0] = localXCos - localYSin;
	self->offset[1] = localYCos + localXSin;
	self->offset[2] = localXCos - localY2Sin;
	self->offset[3] = localY2Cos + localXSin;
	self->offset[4] = localX2Cos - localY2Sin;
	self->offset[5] = localY2Cos + localX2Sin;
	self->offset[6] = localX2Cos - localYSin;
	self->offset[7] = localYCos + localX2Sin;
}

#ifdef __cplusplus
}
#endif
