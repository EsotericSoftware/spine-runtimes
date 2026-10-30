#include "from_property.h"
#include <spine/spine.h>

using namespace spine;

void spine_from_property_dispose(spine_from_property self) {
	delete (FromProperty *) self;
}

spine_rtti spine_from_property_get_rtti(spine_from_property self) {
	FromProperty *_self = (FromProperty *) self;
	return (spine_rtti) &_self->getRTTI();
}

float spine_from_property_value(spine_from_property self, spine_skeleton skeleton, spine_bone_pose source, bool local, /*@null*/ float *offsets) {
	FromProperty *_self = (FromProperty *) self;
	return _self->value(*((Skeleton *) skeleton), *((BonePose *) source), local, offsets);
}

spine_rtti spine_from_property_rtti(void) {
	return (spine_rtti) &FromProperty::rtti;
}

float spine_from_property_get__offset(spine_from_property self) {
	FromProperty *_self = (FromProperty *) self;
	return _self->_offset;
}

void spine_from_property_set__offset(spine_from_property self, float value) {
	FromProperty *_self = (FromProperty *) self;
	_self->_offset = value;
}

/*@null*/ spine_array_to_property spine_from_property_get__to(spine_from_property self) {
	FromProperty *_self = (FromProperty *) self;
	return (spine_array_to_property) &_self->_to;
}

void spine_from_property_set__to(spine_from_property self, /*@null*/ spine_array_to_property value) {
	FromProperty *_self = (FromProperty *) self;
	_self->_to = *((Array<ToProperty *> *) value);
}
