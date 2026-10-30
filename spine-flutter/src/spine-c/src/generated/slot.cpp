#include "slot.h"
#include <spine/spine.h>

using namespace spine;

spine_slot spine_slot_create(spine_slot_data data, spine_skeleton skeleton) {
	return (spine_slot) new (__FILE__, __LINE__) Slot(*((SlotData *) data), *((Skeleton *) skeleton));
}

void spine_slot_dispose(spine_slot self) {
	delete (Slot *) self;
}

spine_bone spine_slot_get_bone(spine_slot self) {
	Slot *_self = (Slot *) self;
	return (spine_bone) &_self->getBone();
}

void spine_slot_setup_pose(spine_slot self) {
	Slot *_self = (Slot *) self;
	_self->setupPose();
}

spine_slot_data spine_slot_get_data(spine_slot self) {
	Slot *_self = (Slot *) self;
	return (spine_slot_data) &_self->getData();
}

spine_slot_pose spine_slot_get_pose(spine_slot self) {
	Slot *_self = (Slot *) self;
	return (spine_slot_pose) &_self->getPose();
}

spine_slot_pose spine_slot_get_applied_pose(spine_slot self) {
	Slot *_self = (Slot *) self;
	return (spine_slot_pose) &_self->getAppliedPose();
}

void spine_slot_reset_constrained(spine_slot self) {
	Slot *_self = (Slot *) self;
	_self->resetConstrained();
}

void spine_slot_constrained(spine_slot self) {
	Slot *_self = (Slot *) self;
	_self->constrained();
}

bool spine_slot_is_pose_equal_to_applied(spine_slot self) {
	Slot *_self = (Slot *) self;
	return _self->isPoseEqualToApplied();
}
