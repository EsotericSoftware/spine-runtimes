#include "constraint_timeline.h"
#include <spine/spine.h>

using namespace spine;

void spine_constraint_timeline_dispose(spine_constraint_timeline self) {
	delete (ConstraintTimeline *) self;
}

spine_rtti spine_constraint_timeline_get_rtti(spine_constraint_timeline self) {
	ConstraintTimeline *_self = (ConstraintTimeline *) self;
	return (spine_rtti) &_self->getRTTI();
}

int spine_constraint_timeline_get_constraint_index(spine_constraint_timeline self) {
	ConstraintTimeline *_self = (ConstraintTimeline *) self;
	return _self->getConstraintIndex();
}

void spine_constraint_timeline_set_constraint_index(spine_constraint_timeline self, int inValue) {
	ConstraintTimeline *_self = (ConstraintTimeline *) self;
	_self->setConstraintIndex(inValue);
}

spine_rtti spine_constraint_timeline_rtti(void) {
	return (spine_rtti) &ConstraintTimeline::rtti;
}
