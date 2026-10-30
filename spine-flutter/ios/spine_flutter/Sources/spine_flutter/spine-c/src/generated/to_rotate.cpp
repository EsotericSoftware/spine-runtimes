#include "to_rotate.h"
#include <spine/spine.h>

using namespace spine;

spine_to_rotate spine_to_rotate_create(void) {
	return (spine_to_rotate) new (__FILE__, __LINE__) ToRotate();
}

void spine_to_rotate_dispose(spine_to_rotate self) {
	delete (ToRotate *) self;
}

spine_rtti spine_to_rotate_get_rtti(spine_to_rotate self) {
	ToRotate *_self = (ToRotate *) self;
	return (spine_rtti) &_self->getRTTI();
}

float spine_to_rotate_mix(spine_to_rotate self, spine_transform_constraint_pose pose) {
	ToRotate *_self = (ToRotate *) self;
	return _self->mix(*((TransformConstraintPose *) pose));
}

void spine_to_rotate_apply(spine_to_rotate self, spine_skeleton skeleton, spine_transform_constraint_pose pose, spine_bone_pose bone, float value,
						   bool local, bool additive) {
	ToRotate *_self = (ToRotate *) self;
	_self->apply(*((Skeleton *) skeleton), *((TransformConstraintPose *) pose), *((BonePose *) bone), value, local, additive);
}

spine_rtti spine_to_rotate_rtti(void) {
	return (spine_rtti) &ToRotate::rtti;
}
