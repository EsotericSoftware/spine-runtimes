#include "bone_local.h"
#include <spine/spine.h>

using namespace spine;

spine_bone_local spine_bone_local_create(void) {
	return (spine_bone_local) new (__FILE__, __LINE__) BoneLocal();
}

void spine_bone_local_dispose(spine_bone_local self) {
	delete (BoneLocal *) self;
}

void spine_bone_local_set(spine_bone_local self, spine_bone_local pose) {
	BoneLocal *_self = (BoneLocal *) self;
	_self->set(*((BoneLocal *) pose));
}

float spine_bone_local_get_x(spine_bone_local self) {
	BoneLocal *_self = (BoneLocal *) self;
	return _self->getX();
}

void spine_bone_local_set_x(spine_bone_local self, float x) {
	BoneLocal *_self = (BoneLocal *) self;
	_self->setX(x);
}

float spine_bone_local_get_y(spine_bone_local self) {
	BoneLocal *_self = (BoneLocal *) self;
	return _self->getY();
}

void spine_bone_local_set_y(spine_bone_local self, float y) {
	BoneLocal *_self = (BoneLocal *) self;
	_self->setY(y);
}

void spine_bone_local_set_position(spine_bone_local self, float x, float y) {
	BoneLocal *_self = (BoneLocal *) self;
	_self->setPosition(x, y);
}

float spine_bone_local_get_rotation(spine_bone_local self) {
	BoneLocal *_self = (BoneLocal *) self;
	return _self->getRotation();
}

void spine_bone_local_set_rotation(spine_bone_local self, float rotation) {
	BoneLocal *_self = (BoneLocal *) self;
	_self->setRotation(rotation);
}

float spine_bone_local_get_scale_x(spine_bone_local self) {
	BoneLocal *_self = (BoneLocal *) self;
	return _self->getScaleX();
}

void spine_bone_local_set_scale_x(spine_bone_local self, float scaleX) {
	BoneLocal *_self = (BoneLocal *) self;
	_self->setScaleX(scaleX);
}

float spine_bone_local_get_scale_y(spine_bone_local self) {
	BoneLocal *_self = (BoneLocal *) self;
	return _self->getScaleY();
}

void spine_bone_local_set_scale_y(spine_bone_local self, float scaleY) {
	BoneLocal *_self = (BoneLocal *) self;
	_self->setScaleY(scaleY);
}

void spine_bone_local_set_scale_1(spine_bone_local self, float scaleX, float scaleY) {
	BoneLocal *_self = (BoneLocal *) self;
	_self->setScale(scaleX, scaleY);
}

void spine_bone_local_set_scale_2(spine_bone_local self, float scale) {
	BoneLocal *_self = (BoneLocal *) self;
	_self->setScale(scale);
}

float spine_bone_local_get_shear_x(spine_bone_local self) {
	BoneLocal *_self = (BoneLocal *) self;
	return _self->getShearX();
}

void spine_bone_local_set_shear_x(spine_bone_local self, float shearX) {
	BoneLocal *_self = (BoneLocal *) self;
	_self->setShearX(shearX);
}

float spine_bone_local_get_shear_y(spine_bone_local self) {
	BoneLocal *_self = (BoneLocal *) self;
	return _self->getShearY();
}

void spine_bone_local_set_shear_y(spine_bone_local self, float shearY) {
	BoneLocal *_self = (BoneLocal *) self;
	_self->setShearY(shearY);
}

spine_inherit spine_bone_local_get_inherit(spine_bone_local self) {
	BoneLocal *_self = (BoneLocal *) self;
	return (spine_inherit) _self->getInherit();
}

void spine_bone_local_set_inherit(spine_bone_local self, spine_inherit inherit) {
	BoneLocal *_self = (BoneLocal *) self;
	_self->setInherit((Inherit) inherit);
}
