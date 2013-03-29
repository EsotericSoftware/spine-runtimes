#include "SfmlRegionAttachment.h"
#include <stdlib.h>

void _RegionAttachment_init (RegionAttachment* attachment, const char* name);

void SfmlRegionAttachment_dispose (Attachment* attachment) {
	/* SfmlRegionAttachment* this = (SfmlRegionAttachment*)attachment; */
	/* dispose something */
}

SfmlRegionAttachment* SfmlRegionAttachment_create (const char* name) {
	SfmlRegionAttachment* this = calloc(1, sizeof(SfmlRegionAttachment));
	_RegionAttachment_init(&this->super, name);
	((Attachment*)this)->_dispose = SfmlRegionAttachment_dispose;
	return this;
}
