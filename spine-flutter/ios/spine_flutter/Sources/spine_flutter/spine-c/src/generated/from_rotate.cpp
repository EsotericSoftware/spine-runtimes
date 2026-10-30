#include "from_rotate.h"
#include <spine/spine.h>

using namespace spine;

spine_from_rotate spine_from_rotate_create(void) {
	return (spine_from_rotate) new (__FILE__, __LINE__) FromRotate();
}

void spine_from_rotate_dispose(spine_from_rotate self) {
	delete (FromRotate *) self;
}

spine_rtti spine_from_rotate_get_rtti(spine_from_rotate self) {
	FromRotate *_self = (FromRotate *) self;
	return (spine_rtti) &_self->getRTTI();
}

float spine_from_rotate_value(spine_from_rotate self, spine_skeleton skeleton, spine_bone_pose source, bool local, /*@null*/ float *offsets) {
	FromRotate *_self = (FromRotate *) self;
	return _self->value(*((Skeleton *) skeleton), *((BonePose *) source), local, offsets);
}

spine_rtti spine_from_rotate_rtti(void) {
	return (spine_rtti) &FromRotate::rtti;
}
