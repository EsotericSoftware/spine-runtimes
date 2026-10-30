#ifndef SPINE_SPINE_BONE_TIMELINE2_H
#define SPINE_SPINE_BONE_TIMELINE2_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API void spine_bone_timeline2_dispose(spine_bone_timeline2 self);

SPINE_C_API spine_rtti spine_bone_timeline2_get_rtti(spine_bone_timeline2 self);
SPINE_C_API void spine_bone_timeline2_apply(spine_bone_timeline2 self, spine_skeleton skeleton, float lastTime, float time,
											/*@null*/ spine_array_event events, float alpha, spine_mix_from from, bool add, bool out,
											bool appliedPose);
SPINE_C_API int spine_bone_timeline2_get_bone_index(spine_bone_timeline2 self);
SPINE_C_API void spine_bone_timeline2_set_bone_index(spine_bone_timeline2 self, int inValue);
SPINE_C_API void spine_bone_timeline2_set_frame(spine_bone_timeline2 self, size_t frame, float time, float value1, float value2);
SPINE_C_API void spine_bone_timeline2_set_linear(spine_bone_timeline2 self, size_t frame);
SPINE_C_API void spine_bone_timeline2_set_stepped(spine_bone_timeline2 self, size_t frame);
SPINE_C_API void spine_bone_timeline2_set_bezier(spine_bone_timeline2 self, size_t bezier, size_t frame, float value, float time1, float value1,
												 float cx1, float cy1, float cx2, float cy2, float time2, float value2);
SPINE_C_API float spine_bone_timeline2_get_bezier_value(spine_bone_timeline2 self, float time, size_t frame, size_t valueOffset, size_t i);
SPINE_C_API spine_array_float spine_bone_timeline2_get_curves(spine_bone_timeline2 self);
/**
 * True if this timeline supports additive blending.
 */
SPINE_C_API bool spine_bone_timeline2_get_additive(spine_bone_timeline2 self);
/**
 * True if this timeline sets values instantaneously and does not support
 * interpolation between frames.
 */
SPINE_C_API bool spine_bone_timeline2_get_instant(spine_bone_timeline2 self);
SPINE_C_API size_t spine_bone_timeline2_get_frame_entries(spine_bone_timeline2 self);
SPINE_C_API size_t spine_bone_timeline2_get_frame_count(spine_bone_timeline2 self);
SPINE_C_API spine_array_float spine_bone_timeline2_get_frames(spine_bone_timeline2 self);
SPINE_C_API float spine_bone_timeline2_get_duration(spine_bone_timeline2 self);
SPINE_C_API spine_array_property_id spine_bone_timeline2_get_property_ids(spine_bone_timeline2 self);
SPINE_C_API spine_rtti spine_bone_timeline2_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_BONE_TIMELINE2_H */
