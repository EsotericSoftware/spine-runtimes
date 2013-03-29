#include <spine/Attachment.h>
#include <spine/util.h>

void _Attachment_init (Attachment* this, const char* name, int type) {
	MALLOC_STR(this->name, name);
	this->type = type;
}

void _Attachment_deinit (Attachment* this) {
	FREE(this->name)
	FREE(this)
}

void Attachment_dispose (Attachment* this) {
	this->_dispose(this);
}
