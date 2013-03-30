#include <spine/AtlasAttachmentLoader.h>
#include <spine/util.h>
#include <spine/extension.h>
#include <stdio.h>

void _AtlasAttachmentLoader_dispose (AttachmentLoader* self) {
	_AttachmentLoader_deinit(self);
}

Attachment* _AtlasAttachmentLoader_newAttachment (AttachmentLoader* loader, AttachmentType type, const char* name) {
	AtlasAttachmentLoader* self = (AtlasAttachmentLoader*)loader;
	switch (type) {
	case ATTACHMENT_REGION: {
		AtlasRegion* region = Atlas_findRegion(self->atlas, name);
		if (!region) {
			_AttachmentLoader_setError(loader, "Region not found: ", name);
			return 0;
		}
		return (Attachment*)RegionAttachment_create(name, region);
	}
	default: {
		char buffer[16];
		sprintf((char*)loader->error2, "%d", type);
		_AttachmentLoader_setError(loader, "Unknown attachment type: ", buffer);
		return 0;
	}
	}
}

AtlasAttachmentLoader* AtlasAttachmentLoader_create (Atlas* atlas) {
	AtlasAttachmentLoader* self = CALLOC(AtlasAttachmentLoader, 1)
	self->atlas = atlas;
	self->super._newAttachment = _AtlasAttachmentLoader_newAttachment;
	self->super._dispose = _AtlasAttachmentLoader_dispose;
	return self;
}
