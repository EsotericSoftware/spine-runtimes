#include "bone_pose.h"
#include <spine/spine.h>

using namespace spine;

spine_bone_pose spine_bone_pose_create(void) {
	return (spine_bone_pose) new (__FILE__, __LINE__) BonePose();
}

void spine_bone_pose_dispose(spine_bone_pose self) {
	delete (BonePose *) self;
}

spine_rtti spine_bone_pose_get_rtti(spine_bone_pose self) {
	BonePose *_self = (BonePose *) self;
	return (spine_rtti) &_self->getRTTI();
}

void spine_bone_pose_update(spine_bone_pose self, spine_skeleton skeleton, spine_physics physics) {
	BonePose *_self = (BonePose *) self;
	_self->update(*((Skeleton *) skeleton), (Physics) physics);
}

void spine_bone_pose_update_world_transform(spine_bone_pose self, spine_skeleton skeleton) {
	BonePose *_self = (BonePose *) self;
	_self->updateWorldTransform(*((Skeleton *) skeleton));
}

void spine_bone_pose_update_local_transform(spine_bone_pose self, spine_skeleton skeleton) {
	BonePose *_self = (BonePose *) self;
	_self->updateLocalTransform(*((Skeleton *) skeleton));
}

void spine_bone_pose_validate_local_transform(spine_bone_pose self, spine_skeleton skeleton) {
	BonePose *_self = (BonePose *) self;
	_self->validateLocalTransform(*((Skeleton *) skeleton));
}

void spine_bone_pose_modify_local(spine_bone_pose self, spine_skeleton skeleton) {
	BonePose *_self = (BonePose *) self;
	_self->modifyLocal(*((Skeleton *) skeleton));
}

void spine_bone_pose_modify_world(spine_bone_pose self, spine_skeleton skeleton) {
	BonePose *_self = (BonePose *) self;
	_self->modifyWorld(*((Skeleton *) skeleton));
}

float spine_bone_pose_get_a(spine_bone_pose self) {
	BonePose *_self = (BonePose *) self;
	return _self->getA();
}

void spine_bone_pose_set_a(spine_bone_pose self, float a) {
	BonePose *_self = (BonePose *) self;
	_self->setA(a);
}

float spine_bone_pose_get_b(spine_bone_pose self) {
	BonePose *_self = (BonePose *) self;
	return _self->getB();
}

void spine_bone_pose_set_b(spine_bone_pose self, float b) {
	BonePose *_self = (BonePose *) self;
	_self->setB(b);
}

float spine_bone_pose_get_c(spine_bone_pose self) {
	BonePose *_self = (BonePose *) self;
	return _self->getC();
}

void spine_bone_pose_set_c(spine_bone_pose self, float c) {
	BonePose *_self = (BonePose *) self;
	_self->setC(c);
}

float spine_bone_pose_get_d(spine_bone_pose self) {
	BonePose *_self = (BonePose *) self;
	return _self->getD();
}

void spine_bone_pose_set_d(spine_bone_pose self, float d) {
	BonePose *_self = (BonePose *) self;
	_self->setD(d);
}

float spine_bone_pose_get_world_x(spine_bone_pose self) {
	BonePose *_self = (BonePose *) self;
	return _self->getWorldX();
}

void spine_bone_pose_set_world_x(spine_bone_pose self, float worldX) {
	BonePose *_self = (BonePose *) self;
	_self->setWorldX(worldX);
}

float spine_bone_pose_get_world_y(spine_bone_pose self) {
	BonePose *_self = (BonePose *) self;
	return _self->getWorldY();
}

void spine_bone_pose_set_world_y(spine_bone_pose self, float worldY) {
	BonePose *_self = (BonePose *) self;
	_self->setWorldY(worldY);
}

float spine_bone_pose_get_world_rotation_x(spine_bone_pose self) {
	BonePose *_self = (BonePose *) self;
	return _self->getWorldRotationX();
}

float spine_bone_pose_get_world_rotation_y(spine_bone_pose self) {
	BonePose *_self = (BonePose *) self;
	return _self->getWorldRotationY();
}

float spine_bone_pose_get_world_scale_x(spine_bone_pose self) {
	BonePose *_self = (BonePose *) self;
	return _self->getWorldScaleX();
}

float spine_bone_pose_get_world_scale_y(spine_bone_pose self) {
	BonePose *_self = (BonePose *) self;
	return _self->getWorldScaleY();
}

void spine_bone_pose_world_to_local(spine_bone_pose self, float worldX, float worldY, float *outLocalX, float *outLocalY) {
	BonePose *_self = (BonePose *) self;
	_self->worldToLocal(worldX, worldY, *outLocalX, *outLocalY);
}

void spine_bone_pose_local_to_world(spine_bone_pose self, float localX, float localY, float *outWorldX, float *outWorldY) {
	BonePose *_self = (BonePose *) self;
	_self->localToWorld(localX, localY, *outWorldX, *outWorldY);
}

void spine_bone_pose_world_to_parent(spine_bone_pose self, float worldX, float worldY, float *outParentX, float *outParentY) {
	BonePose *_self = (BonePose *) self;
	_self->worldToParent(worldX, worldY, *outParentX, *outParentY);
}

void spine_bone_pose_parent_to_world(spine_bone_pose self, float parentX, float parentY, float *outWorldX, float *outWorldY) {
	BonePose *_self = (BonePose *) self;
	_self->parentToWorld(parentX, parentY, *outWorldX, *outWorldY);
}

float spine_bone_pose_world_to_local_rotation(spine_bone_pose self, float worldRotation) {
	BonePose *_self = (BonePose *) self;
	return _self->worldToLocalRotation(worldRotation);
}

float spine_bone_pose_local_to_world_rotation(spine_bone_pose self, float localRotation) {
	BonePose *_self = (BonePose *) self;
	return _self->localToWorldRotation(localRotation);
}

void spine_bone_pose_rotate_world(spine_bone_pose self, float degrees) {
	BonePose *_self = (BonePose *) self;
	_self->rotateWorld(degrees);
}

void spine_bone_pose_set(spine_bone_pose self, spine_bone_local pose) {
	BonePose *_self = (BonePose *) self;
	_self->set(*((BoneLocal *) pose));
}

float spine_bone_pose_get_x(spine_bone_pose self) {
	BonePose *_self = (BonePose *) self;
	return _self->getX();
}

void spine_bone_pose_set_x(spine_bone_pose self, float x) {
	BonePose *_self = (BonePose *) self;
	_self->setX(x);
}

float spine_bone_pose_get_y(spine_bone_pose self) {
	BonePose *_self = (BonePose *) self;
	return _self->getY();
}

void spine_bone_pose_set_y(spine_bone_pose self, float y) {
	BonePose *_self = (BonePose *) self;
	_self->setY(y);
}

void spine_bone_pose_set_position(spine_bone_pose self, float x, float y) {
	BonePose *_self = (BonePose *) self;
	_self->setPosition(x, y);
}

float spine_bone_pose_get_rotation(spine_bone_pose self) {
	BonePose *_self = (BonePose *) self;
	return _self->getRotation();
}

void spine_bone_pose_set_rotation(spine_bone_pose self, float rotation) {
	BonePose *_self = (BonePose *) self;
	_self->setRotation(rotation);
}

float spine_bone_pose_get_scale_x(spine_bone_pose self) {
	BonePose *_self = (BonePose *) self;
	return _self->getScaleX();
}

void spine_bone_pose_set_scale_x(spine_bone_pose self, float scaleX) {
	BonePose *_self = (BonePose *) self;
	_self->setScaleX(scaleX);
}

float spine_bone_pose_get_scale_y(spine_bone_pose self) {
	BonePose *_self = (BonePose *) self;
	return _self->getScaleY();
}

void spine_bone_pose_set_scale_y(spine_bone_pose self, float scaleY) {
	BonePose *_self = (BonePose *) self;
	_self->setScaleY(scaleY);
}

void spine_bone_pose_set_scale_1(spine_bone_pose self, float scaleX, float scaleY) {
	BonePose *_self = (BonePose *) self;
	_self->setScale(scaleX, scaleY);
}

void spine_bone_pose_set_scale_2(spine_bone_pose self, float scale) {
	BonePose *_self = (BonePose *) self;
	_self->setScale(scale);
}

float spine_bone_pose_get_shear_x(spine_bone_pose self) {
	BonePose *_self = (BonePose *) self;
	return _self->getShearX();
}

void spine_bone_pose_set_shear_x(spine_bone_pose self, float shearX) {
	BonePose *_self = (BonePose *) self;
	_self->setShearX(shearX);
}

float spine_bone_pose_get_shear_y(spine_bone_pose self) {
	BonePose *_self = (BonePose *) self;
	return _self->getShearY();
}

void spine_bone_pose_set_shear_y(spine_bone_pose self, float shearY) {
	BonePose *_self = (BonePose *) self;
	_self->setShearY(shearY);
}

spine_inherit spine_bone_pose_get_inherit(spine_bone_pose self) {
	BonePose *_self = (BonePose *) self;
	return (spine_inherit) _self->getInherit();
}

void spine_bone_pose_set_inherit(spine_bone_pose self, spine_inherit inherit) {
	BonePose *_self = (BonePose *) self;
	_self->setInherit((Inherit) inherit);
}

spine_rtti spine_bone_pose_rtti(void) {
	return (spine_rtti) &BonePose::rtti;
}
