#include "rtti.h"
#include <spine/spine.h>

using namespace spine;

void spine_rtti_dispose(spine_rtti self) {
	delete (RTTI *) self;
}

/*@null*/ const char *spine_rtti_get_class_name(spine_rtti self) {
	RTTI *_self = (RTTI *) self;
	return _self->getClassName();
}

bool spine_rtti_is_exactly(spine_rtti self, spine_rtti rtti) {
	RTTI *_self = (RTTI *) self;
	return _self->isExactly(*((const RTTI *) rtti));
}

bool spine_rtti_instance_of(spine_rtti self, spine_rtti rtti) {
	RTTI *_self = (RTTI *) self;
	return _self->instanceOf(*((const RTTI *) rtti));
}
