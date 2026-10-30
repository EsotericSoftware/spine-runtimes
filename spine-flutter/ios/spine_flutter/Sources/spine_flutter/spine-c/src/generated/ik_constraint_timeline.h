#ifndef SPINE_SPINE_IK_CONSTRAINT_TIMELINE_H
#define SPINE_SPINE_IK_CONSTRAINT_TIMELINE_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_ik_constraint_timeline spine_ik_constraint_timeline_create(size_t frameCount, size_t bezierCount, int constraintIndex);

SPINE_C_API void spine_ik_constraint_timeline_dispose(spine_ik_constraint_timeline self);

SPINE_C_API spine_rtti spine_ik_constraint_timeline_get_rtti(spine_ik_constraint_timeline self);
SPINE_C_API void spine_ik_constraint_timeline_apply(spine_ik_constraint_timeline self, spine_skeleton skeleton, float lastTime, float time,
													/*@null*/ spine_array_event events, float alpha, spine_mix_from from, bool add, bool out,
													bool appliedPose);
/**
 * Sets the time, mix, softness, bend direction, compress, and stretch for the
 * specified frame.
 *
 * @param frame Between 0 and frameCount, inclusive.
 * @param time The frame time in seconds.
 * @param bendDirection 1 or -1.
 */
SPINE_C_API void spine_ik_constraint_timeline_set_frame(spine_ik_constraint_timeline self, int frame, float time, float mix, float softness,
														int bendDirection, bool compress, bool stretch);
SPINE_C_API int spine_ik_constraint_timeline_get_constraint_index(spine_ik_constraint_timeline self);
SPINE_C_API void spine_ik_constraint_timeline_set_constraint_index(spine_ik_constraint_timeline self, int inValue);
SPINE_C_API void spine_ik_constraint_timeline_set_linear(spine_ik_constraint_timeline self, size_t frame);
SPINE_C_API void spine_ik_constraint_timeline_set_stepped(spine_ik_constraint_timeline self, size_t frame);
SPINE_C_API void spine_ik_constraint_timeline_set_bezier(spine_ik_constraint_timeline self, size_t bezier, size_t frame, float value, float time1,
														 float value1, float cx1, float cy1, float cx2, float cy2, float time2, float value2);
SPINE_C_API float spine_ik_constraint_timeline_get_bezier_value(spine_ik_constraint_timeline self, float time, size_t frame, size_t valueOffset,
																size_t i);
SPINE_C_API spine_array_float spine_ik_constraint_timeline_get_curves(spine_ik_constraint_timeline self);
/**
 * True if this timeline supports additive blending.
 */
SPINE_C_API bool spine_ik_constraint_timeline_get_additive(spine_ik_constraint_timeline self);
/**
 * True if this timeline sets values instantaneously and does not support
 * interpolation between frames.
 */
SPINE_C_API bool spine_ik_constraint_timeline_get_instant(spine_ik_constraint_timeline self);
SPINE_C_API size_t spine_ik_constraint_timeline_get_frame_entries(spine_ik_constraint_timeline self);
SPINE_C_API size_t spine_ik_constraint_timeline_get_frame_count(spine_ik_constraint_timeline self);
SPINE_C_API spine_array_float spine_ik_constraint_timeline_get_frames(spine_ik_constraint_timeline self);
SPINE_C_API float spine_ik_constraint_timeline_get_duration(spine_ik_constraint_timeline self);
SPINE_C_API spine_array_property_id spine_ik_constraint_timeline_get_property_ids(spine_ik_constraint_timeline self);
SPINE_C_API spine_rtti spine_ik_constraint_timeline_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_IK_CONSTRAINT_TIMELINE_H */
