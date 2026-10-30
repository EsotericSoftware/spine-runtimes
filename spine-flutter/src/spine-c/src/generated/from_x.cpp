#include "from_x.h"
#include <spine/spine.h>

using namespace spine;

spine_from_x spine_from_x_create(void) {
	return (spine_from_x) new (__FILE__, __LINE__) FromX();
}

void spine_from_x_dispose(spine_from_x self) {
	delete (FromX *) self;
}

spine_rtti spine_from_x_get_rtti(spine_from_x self) {
	FromX *_self = (FromX *) self;
	return (spine_rtti) &_self->getRTTI();
}

float spine_from_x_value(spine_from_x self, spine_skeleton skeleton, spine_bone_pose source, bool local, /*@null*/ float *offsets) {
	FromX *_self = (FromX *) self;
	return _self->value(*((Skeleton *) skeleton), *((BonePose *) source), local, offsets);
}

spine_rtti spine_from_x_rtti(void) {
	return (spine_rtti) &FromX::rtti;
}
