#include "slot_timeline.h"
#include <spine/spine.h>

using namespace spine;

void spine_slot_timeline_dispose(spine_slot_timeline self) {
	delete (SlotTimeline *) self;
}

spine_rtti spine_slot_timeline_get_rtti(spine_slot_timeline self) {
	SlotTimeline *_self = (SlotTimeline *) self;
	return (spine_rtti) &_self->getRTTI();
}

int spine_slot_timeline_get_slot_index(spine_slot_timeline self) {
	SlotTimeline *_self = (SlotTimeline *) self;
	return _self->getSlotIndex();
}

void spine_slot_timeline_set_slot_index(spine_slot_timeline self, int inValue) {
	SlotTimeline *_self = (SlotTimeline *) self;
	_self->setSlotIndex(inValue);
}

spine_rtti spine_slot_timeline_rtti(void) {
	return (spine_rtti) &SlotTimeline::rtti;
}
