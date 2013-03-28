#include <spine/Attachment.h>
#include <spine/util.h>

static AttachmentLoader loader;

void Attachment_setAttachmentLoader (AttachmentLoader value) {
	loader = value;
}

AttachmentLoader Attachment_getAttachmentLoader () {
	return loader;
}

void Attachment_init (Attachment* this, const char* name) {
	MALLOC_STR(this->name, name);
}

void Attachment_dispose (Attachment* this) {
	this->_dispose(this);
	FREE(this->name)
	FREE(this)
}
