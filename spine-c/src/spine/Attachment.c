#include <spine/Attachment.h>
#include <spine/util.h>

void _Attachment_init (Attachment* self, const char* name, int type) {
	MALLOC_STR(self->name, name);
	self->type = type;
}

void _Attachment_deinit (Attachment* self) {
	FREE(self->name)
	FREE(self)
}

void Attachment_dispose (Attachment* self) {
	self->_dispose(self);
}
