#include "to_y.h"
#include <spine/spine.h>

using namespace spine;

spine_to_y spine_to_y_create(void) {
	return (spine_to_y) new (__FILE__, __LINE__) ToY();
}

void spine_to_y_dispose(spine_to_y self) {
	delete (ToY *) self;
}

spine_rtti spine_to_y_get_rtti(spine_to_y self) {
	ToY *_self = (ToY *) self;
	return (spine_rtti) &_self->getRTTI();
}

float spine_to_y_mix(spine_to_y self, spine_transform_constraint_pose pose) {
	ToY *_self = (ToY *) self;
	return _self->mix(*((TransformConstraintPose *) pose));
}

void spine_to_y_apply(spine_to_y self, spine_skeleton skeleton, spine_transform_constraint_pose pose, spine_bone_pose bone, float value, bool local,
					  bool additive) {
	ToY *_self = (ToY *) self;
	_self->apply(*((Skeleton *) skeleton), *((TransformConstraintPose *) pose), *((BonePose *) bone), value, local, additive);
}

spine_rtti spine_to_y_rtti(void) {
	return (spine_rtti) &ToY::rtti;
}
