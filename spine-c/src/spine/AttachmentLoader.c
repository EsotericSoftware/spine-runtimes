#include <spine/AttachmentLoader.h>
#include <spine/util.h>

void _AttachmentLoader_init (AttachmentLoader* this) {
}

void _AttachmentLoader_deinit (AttachmentLoader* this) {
	FREE(this->error1)
	FREE(this->error2)
}

void AttachmentLoader_dispose (AttachmentLoader* this) {
	this->_dispose(this);
}

Attachment* AttachmentLoader_newAttachment (AttachmentLoader* this, AttachmentType type, const char* name) {
	FREE(this->error1)
	FREE(this->error2)
	this->error1 = 0;
	this->error2 = 0;
	return this->_newAttachment(this, type, name);
}

void* _AttachmentLoader_setError (AttachmentLoader* this, const char* error1, const char* error2) {
	FREE(this->error1)
	FREE(this->error2)
	MALLOC_STR(this->error1, error1)
	MALLOC_STR(this->error2, error2)
	return 0;
}
