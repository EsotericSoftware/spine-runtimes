#include "SfmlRegionAttachment.h"
#include <stdlib.h>

void SfmlRegionAttachment_dispose (Attachment* attachment) {
	SfmlRegionAttachment* this = (SfmlRegionAttachment*)attachment;
	RegionAttachment_dispose(&this->super);
}

SfmlRegionAttachment* SfmlRegionAttachment_create (const char* name) {
	SfmlRegionAttachment* this = calloc(1, sizeof(SfmlRegionAttachment));
	RegionAttachment_init(&this->super, name);
	((Attachment*)this)->_dispose = SfmlRegionAttachment_dispose;
	return this;
}
