#include "to_x.h"
#include <spine/spine.h>

using namespace spine;

spine_to_x spine_to_x_create(void) {
	return (spine_to_x) new (__FILE__, __LINE__) ToX();
}

void spine_to_x_dispose(spine_to_x self) {
	delete (ToX *) self;
}

spine_rtti spine_to_x_get_rtti(spine_to_x self) {
	ToX *_self = (ToX *) self;
	return (spine_rtti) &_self->getRTTI();
}

float spine_to_x_mix(spine_to_x self, spine_transform_constraint_pose pose) {
	ToX *_self = (ToX *) self;
	return _self->mix(*((TransformConstraintPose *) pose));
}

void spine_to_x_apply(spine_to_x self, spine_skeleton skeleton, spine_transform_constraint_pose pose, spine_bone_pose bone, float value, bool local,
					  bool additive) {
	ToX *_self = (ToX *) self;
	_self->apply(*((Skeleton *) skeleton), *((TransformConstraintPose *) pose), *((BonePose *) bone), value, local, additive);
}

spine_rtti spine_to_x_rtti(void) {
	return (spine_rtti) &ToX::rtti;
}
