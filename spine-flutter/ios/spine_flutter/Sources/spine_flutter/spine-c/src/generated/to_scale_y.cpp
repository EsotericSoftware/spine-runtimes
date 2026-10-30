#include "to_scale_y.h"
#include <spine/spine.h>

using namespace spine;

spine_to_scale_y spine_to_scale_y_create(void) {
	return (spine_to_scale_y) new (__FILE__, __LINE__) ToScaleY();
}

void spine_to_scale_y_dispose(spine_to_scale_y self) {
	delete (ToScaleY *) self;
}

spine_rtti spine_to_scale_y_get_rtti(spine_to_scale_y self) {
	ToScaleY *_self = (ToScaleY *) self;
	return (spine_rtti) &_self->getRTTI();
}

float spine_to_scale_y_mix(spine_to_scale_y self, spine_transform_constraint_pose pose) {
	ToScaleY *_self = (ToScaleY *) self;
	return _self->mix(*((TransformConstraintPose *) pose));
}

void spine_to_scale_y_apply(spine_to_scale_y self, spine_skeleton skeleton, spine_transform_constraint_pose pose, spine_bone_pose bone, float value,
							bool local, bool additive) {
	ToScaleY *_self = (ToScaleY *) self;
	_self->apply(*((Skeleton *) skeleton), *((TransformConstraintPose *) pose), *((BonePose *) bone), value, local, additive);
}

spine_rtti spine_to_scale_y_rtti(void) {
	return (spine_rtti) &ToScaleY::rtti;
}
