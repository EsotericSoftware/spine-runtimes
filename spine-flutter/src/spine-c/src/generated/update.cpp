#include "update.h"
#include <spine/spine.h>

using namespace spine;

void spine_update_dispose(spine_update self) {
	delete (Update *) self;
}

spine_rtti spine_update_get_rtti(spine_update self) {
	Update *_self = (Update *) self;
	return (spine_rtti) &_self->getRTTI();
}

void spine_update_update(spine_update self, spine_skeleton skeleton, spine_physics physics) {
	Update *_self = (Update *) self;
	_self->update(*((Skeleton *) skeleton), (Physics) physics);
}

spine_rtti spine_update_rtti(void) {
	return (spine_rtti) &Update::rtti;
}
