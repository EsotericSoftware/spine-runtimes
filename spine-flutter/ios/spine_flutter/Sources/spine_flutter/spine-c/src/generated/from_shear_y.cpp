#include "from_shear_y.h"
#include <spine/spine.h>

using namespace spine;

spine_from_shear_y spine_from_shear_y_create(void) {
	return (spine_from_shear_y) new (__FILE__, __LINE__) FromShearY();
}

void spine_from_shear_y_dispose(spine_from_shear_y self) {
	delete (FromShearY *) self;
}

spine_rtti spine_from_shear_y_get_rtti(spine_from_shear_y self) {
	FromShearY *_self = (FromShearY *) self;
	return (spine_rtti) &_self->getRTTI();
}

float spine_from_shear_y_value(spine_from_shear_y self, spine_skeleton skeleton, spine_bone_pose source, bool local, /*@null*/ float *offsets) {
	FromShearY *_self = (FromShearY *) self;
	return _self->value(*((Skeleton *) skeleton), *((BonePose *) source), local, offsets);
}

spine_rtti spine_from_shear_y_rtti(void) {
	return (spine_rtti) &FromShearY::rtti;
}
