#include "to_shear_y.h"
#include <spine/spine.h>

using namespace spine;

spine_to_shear_y spine_to_shear_y_create(void) {
	return (spine_to_shear_y) new (__FILE__, __LINE__) ToShearY();
}

void spine_to_shear_y_dispose(spine_to_shear_y self) {
	delete (ToShearY *) self;
}

spine_rtti spine_to_shear_y_get_rtti(spine_to_shear_y self) {
	ToShearY *_self = (ToShearY *) self;
	return (spine_rtti) &_self->getRTTI();
}

float spine_to_shear_y_mix(spine_to_shear_y self, spine_transform_constraint_pose pose) {
	ToShearY *_self = (ToShearY *) self;
	return _self->mix(*((TransformConstraintPose *) pose));
}

void spine_to_shear_y_apply(spine_to_shear_y self, spine_skeleton skeleton, spine_transform_constraint_pose pose, spine_bone_pose bone, float value,
							bool local, bool additive) {
	ToShearY *_self = (ToShearY *) self;
	_self->apply(*((Skeleton *) skeleton), *((TransformConstraintPose *) pose), *((BonePose *) bone), value, local, additive);
}

spine_rtti spine_to_shear_y_rtti(void) {
	return (spine_rtti) &ToShearY::rtti;
}
