#include "transform_constraint_pose.h"
#include <spine/spine.h>

using namespace spine;

spine_transform_constraint_pose spine_transform_constraint_pose_create(void) {
	return (spine_transform_constraint_pose) new (__FILE__, __LINE__) TransformConstraintPose();
}

void spine_transform_constraint_pose_dispose(spine_transform_constraint_pose self) {
	delete (TransformConstraintPose *) self;
}

void spine_transform_constraint_pose_set(spine_transform_constraint_pose self, spine_transform_constraint_pose pose) {
	TransformConstraintPose *_self = (TransformConstraintPose *) self;
	_self->set(*((TransformConstraintPose *) pose));
}

float spine_transform_constraint_pose_get_mix_rotate(spine_transform_constraint_pose self) {
	TransformConstraintPose *_self = (TransformConstraintPose *) self;
	return _self->getMixRotate();
}

void spine_transform_constraint_pose_set_mix_rotate(spine_transform_constraint_pose self, float mixRotate) {
	TransformConstraintPose *_self = (TransformConstraintPose *) self;
	_self->setMixRotate(mixRotate);
}

float spine_transform_constraint_pose_get_mix_x(spine_transform_constraint_pose self) {
	TransformConstraintPose *_self = (TransformConstraintPose *) self;
	return _self->getMixX();
}

void spine_transform_constraint_pose_set_mix_x(spine_transform_constraint_pose self, float mixX) {
	TransformConstraintPose *_self = (TransformConstraintPose *) self;
	_self->setMixX(mixX);
}

float spine_transform_constraint_pose_get_mix_y(spine_transform_constraint_pose self) {
	TransformConstraintPose *_self = (TransformConstraintPose *) self;
	return _self->getMixY();
}

void spine_transform_constraint_pose_set_mix_y(spine_transform_constraint_pose self, float mixY) {
	TransformConstraintPose *_self = (TransformConstraintPose *) self;
	_self->setMixY(mixY);
}

float spine_transform_constraint_pose_get_mix_scale_x(spine_transform_constraint_pose self) {
	TransformConstraintPose *_self = (TransformConstraintPose *) self;
	return _self->getMixScaleX();
}

void spine_transform_constraint_pose_set_mix_scale_x(spine_transform_constraint_pose self, float mixScaleX) {
	TransformConstraintPose *_self = (TransformConstraintPose *) self;
	_self->setMixScaleX(mixScaleX);
}

float spine_transform_constraint_pose_get_mix_scale_y(spine_transform_constraint_pose self) {
	TransformConstraintPose *_self = (TransformConstraintPose *) self;
	return _self->getMixScaleY();
}

void spine_transform_constraint_pose_set_mix_scale_y(spine_transform_constraint_pose self, float mixScaleY) {
	TransformConstraintPose *_self = (TransformConstraintPose *) self;
	_self->setMixScaleY(mixScaleY);
}

float spine_transform_constraint_pose_get_mix_shear_y(spine_transform_constraint_pose self) {
	TransformConstraintPose *_self = (TransformConstraintPose *) self;
	return _self->getMixShearY();
}

void spine_transform_constraint_pose_set_mix_shear_y(spine_transform_constraint_pose self, float mixShearY) {
	TransformConstraintPose *_self = (TransformConstraintPose *) self;
	_self->setMixShearY(mixShearY);
}
