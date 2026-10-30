#include "constraint_data.h"
#include <spine/spine.h>

using namespace spine;

void spine_constraint_data_dispose(spine_constraint_data self) {
	delete (ConstraintData *) self;
}

spine_rtti spine_constraint_data_get_rtti(spine_constraint_data self) {
	ConstraintData *_self = (ConstraintData *) self;
	return (spine_rtti) &_self->getRTTI();
}

spine_constraint spine_constraint_data_create_method(spine_constraint_data self, spine_skeleton skeleton) {
	ConstraintData *_self = (ConstraintData *) self;
	return (spine_constraint) &_self->create(*((Skeleton *) skeleton));
}

const char *spine_constraint_data_get_name(spine_constraint_data self) {
	ConstraintData *_self = (ConstraintData *) self;
	return _self->getName().buffer();
}

bool spine_constraint_data_get_skin_required(spine_constraint_data self) {
	ConstraintData *_self = (ConstraintData *) self;
	return _self->getSkinRequired();
}

spine_rtti spine_constraint_data_rtti(void) {
	return (spine_rtti) &ConstraintData::rtti;
}
