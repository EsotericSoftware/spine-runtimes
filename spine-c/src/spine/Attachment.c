#include <spine/Attachment.h>
#include <spine/util.h>
#include <spine/Slot.h>

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

void Attachment_draw (Attachment* self, Slot* slot) {
	self->_draw(self, slot);
}
