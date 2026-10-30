#include "atlas.h"
#include <spine/spine.h>

using namespace spine;

void spine_atlas_dispose(spine_atlas self) {
	delete (Atlas *) self;
}

void spine_atlas_flip_v(spine_atlas self) {
	Atlas *_self = (Atlas *) self;
	_self->flipV();
}

/*@null*/ spine_atlas_region spine_atlas_find_region(spine_atlas self, const char *name) {
	Atlas *_self = (Atlas *) self;
	return (spine_atlas_region) _self->findRegion(String(name));
}

spine_array_atlas_page spine_atlas_get_pages(spine_atlas self) {
	Atlas *_self = (Atlas *) self;
	return (spine_array_atlas_page) &_self->getPages();
}

spine_array_atlas_region spine_atlas_get_regions(spine_atlas self) {
	Atlas *_self = (Atlas *) self;
	return (spine_array_atlas_region) &_self->getRegions();
}
