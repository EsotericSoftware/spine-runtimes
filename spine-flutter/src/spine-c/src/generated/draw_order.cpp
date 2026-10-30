#include "draw_order.h"
#include <spine/spine.h>

using namespace spine;

spine_draw_order spine_draw_order_create(spine_array_slot setupPose) {
	return (spine_draw_order) new (__FILE__, __LINE__) DrawOrder(*((Array<Slot *> *) setupPose));
}

void spine_draw_order_dispose(spine_draw_order self) {
	delete (DrawOrder *) self;
}

void spine_draw_order_setup_pose(spine_draw_order self) {
	DrawOrder *_self = (DrawOrder *) self;
	_self->setupPose();
}

spine_array_slot spine_draw_order_get_pose(spine_draw_order self) {
	DrawOrder *_self = (DrawOrder *) self;
	return (spine_array_slot) &_self->getPose();
}

spine_array_slot spine_draw_order_get_applied_pose(spine_draw_order self) {
	DrawOrder *_self = (DrawOrder *) self;
	return (spine_array_slot) &_self->getAppliedPose();
}
