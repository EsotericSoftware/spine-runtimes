#ifndef SPINE_SPINE_DRAW_ORDER_FOLDER_TIMELINE_H
#define SPINE_SPINE_DRAW_ORDER_FOLDER_TIMELINE_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_draw_order_folder_timeline spine_draw_order_folder_timeline_create(size_t frameCount, spine_array_int slots, size_t slotCount);

SPINE_C_API void spine_draw_order_folder_timeline_dispose(spine_draw_order_folder_timeline self);

SPINE_C_API spine_rtti spine_draw_order_folder_timeline_get_rtti(spine_draw_order_folder_timeline self);
SPINE_C_API void spine_draw_order_folder_timeline_apply(spine_draw_order_folder_timeline self, spine_skeleton skeleton, float lastTime, float time,
														/*@null*/ spine_array_event events, float alpha, spine_mix_blend blend,
														spine_mix_direction direction, bool appliedPose);
SPINE_C_API size_t spine_draw_order_folder_timeline_get_frame_count(spine_draw_order_folder_timeline self);
/**
 * The Skeleton::getSlots() indices that this timeline affects, in setup order.
 */
SPINE_C_API spine_array_int spine_draw_order_folder_timeline_get_slots(spine_draw_order_folder_timeline self);
/**
 * Sets the time and draw order for the specified frame.
 *
 * @param frame Between 0 and frameCount, inclusive.
 * @param time The frame time in seconds.
 * @param drawOrder Ordered getSlots() indices, or null to use setup pose order.
 */
SPINE_C_API void spine_draw_order_folder_timeline_set_frame(spine_draw_order_folder_timeline self, size_t frame, float time,
															/*@null*/ spine_array_int drawOrder);
SPINE_C_API size_t spine_draw_order_folder_timeline_get_frame_entries(spine_draw_order_folder_timeline self);
SPINE_C_API spine_array_float spine_draw_order_folder_timeline_get_frames(spine_draw_order_folder_timeline self);
SPINE_C_API float spine_draw_order_folder_timeline_get_duration(spine_draw_order_folder_timeline self);
SPINE_C_API spine_array_property_id spine_draw_order_folder_timeline_get_property_ids(spine_draw_order_folder_timeline self);
SPINE_C_API spine_rtti spine_draw_order_folder_timeline_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_DRAW_ORDER_FOLDER_TIMELINE_H */
