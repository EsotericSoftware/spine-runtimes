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

#include <spine/AtlasAttachmentLoader.h>
#include <stdio.h>
#include <spine/extension.h>

#ifdef __cplusplus
namespace spine {
#endif

void _AtlasAttachmentLoader_dispose (AttachmentLoader* self) {
	_AttachmentLoader_deinit(self);
}

Attachment* _AtlasAttachmentLoader_newAttachment (AttachmentLoader* loader, AttachmentType type, const char* name) {
	AtlasAttachmentLoader* self = SUB_CAST(AtlasAttachmentLoader, loader);
	switch (type) {
	case ATTACHMENT_REGION: {
		AtlasRegion* region = Atlas_findRegion(self->atlas, name);
		if (!region) {
			_AttachmentLoader_setError(loader, "Region not found: ", name);
			return 0;
		}
		return SUPER_CAST(Attachment, RegionAttachment_create(name, region)) ;
	}
	default: {
		char buffer[16];
		sprintf(buffer, "%d", type);
		_AttachmentLoader_setError(loader, "Unknown attachment type: ", buffer);
		return 0;
	}
	}
}

AtlasAttachmentLoader* AtlasAttachmentLoader_create (Atlas* atlas) {
	AtlasAttachmentLoader* self = NEW(AtlasAttachmentLoader);
	_AttachmentLoader_init(SUPER(self));
	self->atlas = atlas;
	VTABLE(AttachmentLoader, self) ->newAttachment = _AtlasAttachmentLoader_newAttachment;
	VTABLE(AttachmentLoader, self) ->dispose = _AtlasAttachmentLoader_dispose;
	return self;
}

#ifdef __cplusplus
}
#endif
