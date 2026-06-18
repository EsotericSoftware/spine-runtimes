#ifndef SPINE_SPINE_SHEAR_TIMELINE_H
#define SPINE_SPINE_SHEAR_TIMELINE_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_shear_timeline spine_shear_timeline_create(size_t frameCount, size_t bezierCount, int boneIndex);

SPINE_C_API void spine_shear_timeline_dispose(spine_shear_timeline self);

SPINE_C_API spine_rtti spine_shear_timeline_get_rtti(spine_shear_timeline self);
SPINE_C_API void spine_shear_timeline_apply(spine_shear_timeline self, spine_skeleton skeleton, float lastTime, float time,
											/*@null*/ spine_array_event events, float alpha, spine_mix_from from, bool add, bool out,
											bool appliedPose);
SPINE_C_API int spine_shear_timeline_get_bone_index(spine_shear_timeline self);
SPINE_C_API void spine_shear_timeline_set_bone_index(spine_shear_timeline self, int inValue);
SPINE_C_API void spine_shear_timeline_set_frame(spine_shear_timeline self, size_t frame, float time, float value1, float value2);
SPINE_C_API void spine_shear_timeline_set_linear(spine_shear_timeline self, size_t frame);
SPINE_C_API void spine_shear_timeline_set_stepped(spine_shear_timeline self, size_t frame);
SPINE_C_API void spine_shear_timeline_set_bezier(spine_shear_timeline self, size_t bezier, size_t frame, float value, float time1, float value1,
												 float cx1, float cy1, float cx2, float cy2, float time2, float value2);
SPINE_C_API float spine_shear_timeline_get_bezier_value(spine_shear_timeline self, float time, size_t frame, size_t valueOffset, size_t i);
SPINE_C_API spine_array_float spine_shear_timeline_get_curves(spine_shear_timeline self);
/**
 * True if this timeline supports additive blending.
 */
SPINE_C_API bool spine_shear_timeline_get_additive(spine_shear_timeline self);
/**
 * True if this timeline sets values instantaneously and does not support
 * interpolation between frames.
 */
SPINE_C_API bool spine_shear_timeline_get_instant(spine_shear_timeline self);
SPINE_C_API size_t spine_shear_timeline_get_frame_entries(spine_shear_timeline self);
SPINE_C_API size_t spine_shear_timeline_get_frame_count(spine_shear_timeline self);
SPINE_C_API spine_array_float spine_shear_timeline_get_frames(spine_shear_timeline self);
SPINE_C_API float spine_shear_timeline_get_duration(spine_shear_timeline self);
SPINE_C_API spine_array_property_id spine_shear_timeline_get_property_ids(spine_shear_timeline self);
SPINE_C_API spine_rtti spine_shear_timeline_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_SHEAR_TIMELINE_H */
