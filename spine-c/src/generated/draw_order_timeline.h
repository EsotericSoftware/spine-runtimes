#ifndef SPINE_SPINE_DRAW_ORDER_TIMELINE_H
#define SPINE_SPINE_DRAW_ORDER_TIMELINE_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_draw_order_timeline spine_draw_order_timeline_create(size_t frameCount);

SPINE_C_API void spine_draw_order_timeline_dispose(spine_draw_order_timeline self);

SPINE_C_API spine_rtti spine_draw_order_timeline_get_rtti(spine_draw_order_timeline self);
SPINE_C_API void spine_draw_order_timeline_apply(spine_draw_order_timeline self, spine_skeleton skeleton, float lastTime, float time,
												 /*@null*/ spine_array_event events, float alpha, bool fromSetup, bool add, bool out,
												 bool appliedPose);
SPINE_C_API size_t spine_draw_order_timeline_get_frame_count(spine_draw_order_timeline self);
/**
 * Sets the time and draw order for the specified frame.
 *
 * @param frame Between 0 and frameCount, inclusive.
 * @param time The frame time in seconds.
 * @param drawOrder For each slot in Skeleton::getSlots(), the index of the slot in the new draw order. May be null to use setup pose draw order.
 */
SPINE_C_API void spine_draw_order_timeline_set_frame(spine_draw_order_timeline self, size_t frame, float time, /*@null*/ spine_array_int drawOrder);
/**
 * True if this timeline supports additive blending.
 */
SPINE_C_API bool spine_draw_order_timeline_get_additive(spine_draw_order_timeline self);
/**
 * True if this timeline sets values instantaneously and does not support
 * interpolation between frames.
 */
SPINE_C_API bool spine_draw_order_timeline_get_instant(spine_draw_order_timeline self);
SPINE_C_API size_t spine_draw_order_timeline_get_frame_entries(spine_draw_order_timeline self);
SPINE_C_API spine_array_float spine_draw_order_timeline_get_frames(spine_draw_order_timeline self);
SPINE_C_API float spine_draw_order_timeline_get_duration(spine_draw_order_timeline self);
SPINE_C_API spine_array_property_id spine_draw_order_timeline_get_property_ids(spine_draw_order_timeline self);
SPINE_C_API spine_rtti spine_draw_order_timeline_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_DRAW_ORDER_TIMELINE_H */
