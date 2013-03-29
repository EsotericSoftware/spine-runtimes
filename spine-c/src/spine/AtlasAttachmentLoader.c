#include <spine/AtlasAttachmentLoader.h>
#include <spine/util.h>
#include <spine/extension.h>
#include <stdio.h>

void _AtlasAttachmentLoader_dispose (AttachmentLoader* this) {
	_AttachmentLoader_deinit(this);
}

Attachment* _AtlasAttachmentLoader_newAttachment (AttachmentLoader* loader, AttachmentType type, const char* name) {
	AtlasAttachmentLoader* this = (AtlasAttachmentLoader*)loader;
	switch (type) {
	case ATTACHMENT_REGION: {
		AtlasRegion* region = Atlas_findRegion(this->atlas, name);
		if (!region) return _AttachmentLoader_setError(loader, "Region not found: ", name);
		return (Attachment*)RegionAttachment_create(name, region);
	}
	default: {
		char buffer[16];
		sprintf((char*)loader->error2, "%d", type);
		return _AttachmentLoader_setError(loader, "Unknown attachment type: ", buffer);
	}
	}
}

AtlasAttachmentLoader* AtlasAttachmentLoader_create (Atlas* atlas) {
	AtlasAttachmentLoader* this = calloc(1, sizeof(AtlasAttachmentLoader));
	this->atlas = atlas;
	this->super._newAttachment = _AtlasAttachmentLoader_newAttachment;
	this->super._dispose = _AtlasAttachmentLoader_dispose;
	return this;
}
