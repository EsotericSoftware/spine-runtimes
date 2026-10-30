#include "to_property.h"
#include <spine/spine.h>

using namespace spine;

void spine_to_property_dispose(spine_to_property self) {
	delete (ToProperty *) self;
}

spine_rtti spine_to_property_get_rtti(spine_to_property self) {
	ToProperty *_self = (ToProperty *) self;
	return (spine_rtti) &_self->getRTTI();
}

float spine_to_property_mix(spine_to_property self, spine_transform_constraint_pose pose) {
	ToProperty *_self = (ToProperty *) self;
	return _self->mix(*((TransformConstraintPose *) pose));
}

void spine_to_property_apply(spine_to_property self, spine_skeleton skeleton, spine_transform_constraint_pose pose, spine_bone_pose bone, float value,
							 bool local, bool additive) {
	ToProperty *_self = (ToProperty *) self;
	_self->apply(*((Skeleton *) skeleton), *((TransformConstraintPose *) pose), *((BonePose *) bone), value, local, additive);
}

spine_rtti spine_to_property_rtti(void) {
	return (spine_rtti) &ToProperty::rtti;
}

float spine_to_property_get__offset(spine_to_property self) {
	ToProperty *_self = (ToProperty *) self;
	return _self->_offset;
}

void spine_to_property_set__offset(spine_to_property self, float value) {
	ToProperty *_self = (ToProperty *) self;
	_self->_offset = value;
}

float spine_to_property_get__max(spine_to_property self) {
	ToProperty *_self = (ToProperty *) self;
	return _self->_max;
}

void spine_to_property_set__max(spine_to_property self, float value) {
	ToProperty *_self = (ToProperty *) self;
	_self->_max = value;
}

float spine_to_property_get__scale(spine_to_property self) {
	ToProperty *_self = (ToProperty *) self;
	return _self->_scale;
}

void spine_to_property_set__scale(spine_to_property self, float value) {
	ToProperty *_self = (ToProperty *) self;
	_self->_scale = value;
}
