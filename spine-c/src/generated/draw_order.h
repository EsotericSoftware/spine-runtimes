#ifndef SPINE_SPINE_DRAW_ORDER_H
#define SPINE_SPINE_DRAW_ORDER_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_draw_order spine_draw_order_create(spine_array_slot setupPose);

SPINE_C_API void spine_draw_order_dispose(spine_draw_order self);

/**
 * Sets the unconstrained draw order to the setup pose order.
 */
SPINE_C_API void spine_draw_order_setup_pose(spine_draw_order self);
/**
 * The unconstrained draw order, set by animations and application code.
 */
SPINE_C_API spine_array_slot spine_draw_order_get_pose(spine_draw_order self);
/**
 * The constrained draw order for rendering. If no constraints modify the draw
 * order, this is the same as getPose(). Otherwise it is a copy of getPose()
 * modified by constraints.
 */
SPINE_C_API spine_array_slot spine_draw_order_get_applied_pose(spine_draw_order self);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_DRAW_ORDER_H */
