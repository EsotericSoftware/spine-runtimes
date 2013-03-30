#include <spine/AttachmentLoader.h>
#include <spine/util.h>

void _AttachmentLoader_init (AttachmentLoader* self) {
}

void _AttachmentLoader_deinit (AttachmentLoader* self) {
	FREE(self->error1)
	FREE(self->error2)
}

void AttachmentLoader_dispose (AttachmentLoader* self) {
	self->_dispose(self);
}

Attachment* AttachmentLoader_newAttachment (AttachmentLoader* self, AttachmentType type, const char* name) {
	FREE(self->error1)
	FREE(self->error2)
	self->error1 = 0;
	self->error2 = 0;
	return self->_newAttachment(self, type, name);
}

void _AttachmentLoader_setError (AttachmentLoader* self, const char* error1, const char* error2) {
	FREE(self->error1)
	FREE(self->error2)
	MALLOC_STR(self->error1, error1)
	MALLOC_STR(self->error2, error2)
}
