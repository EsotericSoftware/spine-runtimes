#include "ik_constraint_pose.h"
#include <spine/spine.h>

using namespace spine;

spine_ik_constraint_pose spine_ik_constraint_pose_create(void) {
	return (spine_ik_constraint_pose) new (__FILE__, __LINE__) IkConstraintPose();
}

void spine_ik_constraint_pose_dispose(spine_ik_constraint_pose self) {
	delete (IkConstraintPose *) self;
}

void spine_ik_constraint_pose_set(spine_ik_constraint_pose self, spine_ik_constraint_pose pose) {
	IkConstraintPose *_self = (IkConstraintPose *) self;
	_self->set(*((IkConstraintPose *) pose));
}

float spine_ik_constraint_pose_get_mix(spine_ik_constraint_pose self) {
	IkConstraintPose *_self = (IkConstraintPose *) self;
	return _self->getMix();
}

void spine_ik_constraint_pose_set_mix(spine_ik_constraint_pose self, float mix) {
	IkConstraintPose *_self = (IkConstraintPose *) self;
	_self->setMix(mix);
}

float spine_ik_constraint_pose_get_softness(spine_ik_constraint_pose self) {
	IkConstraintPose *_self = (IkConstraintPose *) self;
	return _self->getSoftness();
}

void spine_ik_constraint_pose_set_softness(spine_ik_constraint_pose self, float softness) {
	IkConstraintPose *_self = (IkConstraintPose *) self;
	_self->setSoftness(softness);
}

int spine_ik_constraint_pose_get_bend_direction(spine_ik_constraint_pose self) {
	IkConstraintPose *_self = (IkConstraintPose *) self;
	return _self->getBendDirection();
}

void spine_ik_constraint_pose_set_bend_direction(spine_ik_constraint_pose self, int bendDirection) {
	IkConstraintPose *_self = (IkConstraintPose *) self;
	_self->setBendDirection(bendDirection);
}

bool spine_ik_constraint_pose_get_compress(spine_ik_constraint_pose self) {
	IkConstraintPose *_self = (IkConstraintPose *) self;
	return _self->getCompress();
}

void spine_ik_constraint_pose_set_compress(spine_ik_constraint_pose self, bool compress) {
	IkConstraintPose *_self = (IkConstraintPose *) self;
	_self->setCompress(compress);
}

bool spine_ik_constraint_pose_get_stretch(spine_ik_constraint_pose self) {
	IkConstraintPose *_self = (IkConstraintPose *) self;
	return _self->getStretch();
}

void spine_ik_constraint_pose_set_stretch(spine_ik_constraint_pose self, bool stretch) {
	IkConstraintPose *_self = (IkConstraintPose *) self;
	_self->setStretch(stretch);
}
