#ifndef SPINE_SPINE_BONE_TIMELINE1_H
#define SPINE_SPINE_BONE_TIMELINE1_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API void spine_bone_timeline1_dispose(spine_bone_timeline1 self);

SPINE_C_API spine_rtti spine_bone_timeline1_get_rtti(spine_bone_timeline1 self);
SPINE_C_API void spine_bone_timeline1_apply(spine_bone_timeline1 self, spine_skeleton skeleton, float lastTime, float time,
											/*@null*/ spine_array_event events, float alpha, bool fromSetup, bool add, bool out, bool appliedPose);
SPINE_C_API int spine_bone_timeline1_get_bone_index(spine_bone_timeline1 self);
SPINE_C_API void spine_bone_timeline1_set_bone_index(spine_bone_timeline1 self, int inValue);
/**
 * Sets the time and value for the specified frame.
 *
 * @param frame Between 0 and frameCount, inclusive.
 * @param time The frame time in seconds.
 */
SPINE_C_API void spine_bone_timeline1_set_frame(spine_bone_timeline1 self, size_t frame, float time, float value);
/**
 * Returns the interpolated value for the specified time.
 */
SPINE_C_API float spine_bone_timeline1_get_curve_value(spine_bone_timeline1 self, float time);
SPINE_C_API float spine_bone_timeline1_get_relative_value(spine_bone_timeline1 self, float time, float alpha, bool fromSetup, bool add, float current,
														  float setup);
SPINE_C_API float spine_bone_timeline1_get_absolute_value_1(spine_bone_timeline1 self, float time, float alpha, bool fromSetup, bool add,
															float current, float setup);
SPINE_C_API float spine_bone_timeline1_get_absolute_value_2(spine_bone_timeline1 self, float time, float alpha, bool fromSetup, bool add,
															float current, float setup, float value);
SPINE_C_API float spine_bone_timeline1_get_scale_value(spine_bone_timeline1 self, float time, float alpha, bool fromSetup, bool add, bool out,
													   float current, float setup);
SPINE_C_API void spine_bone_timeline1_set_linear(spine_bone_timeline1 self, size_t frame);
SPINE_C_API void spine_bone_timeline1_set_stepped(spine_bone_timeline1 self, size_t frame);
SPINE_C_API void spine_bone_timeline1_set_bezier(spine_bone_timeline1 self, size_t bezier, size_t frame, float value, float time1, float value1,
												 float cx1, float cy1, float cx2, float cy2, float time2, float value2);
SPINE_C_API float spine_bone_timeline1_get_bezier_value(spine_bone_timeline1 self, float time, size_t frame, size_t valueOffset, size_t i);
SPINE_C_API spine_array_float spine_bone_timeline1_get_curves(spine_bone_timeline1 self);
/**
 * True if this timeline supports additive blending.
 */
SPINE_C_API bool spine_bone_timeline1_get_additive(spine_bone_timeline1 self);
/**
 * True if this timeline sets values instantaneously and does not support
 * interpolation between frames.
 */
SPINE_C_API bool spine_bone_timeline1_get_instant(spine_bone_timeline1 self);
SPINE_C_API size_t spine_bone_timeline1_get_frame_entries(spine_bone_timeline1 self);
SPINE_C_API size_t spine_bone_timeline1_get_frame_count(spine_bone_timeline1 self);
SPINE_C_API spine_array_float spine_bone_timeline1_get_frames(spine_bone_timeline1 self);
SPINE_C_API float spine_bone_timeline1_get_duration(spine_bone_timeline1 self);
SPINE_C_API spine_array_property_id spine_bone_timeline1_get_property_ids(spine_bone_timeline1 self);
SPINE_C_API spine_rtti spine_bone_timeline1_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_BONE_TIMELINE1_H */
