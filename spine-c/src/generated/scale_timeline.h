#ifndef SPINE_SPINE_SCALE_TIMELINE_H
#define SPINE_SPINE_SCALE_TIMELINE_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_scale_timeline spine_scale_timeline_create(size_t frameCount, size_t bezierCount, int boneIndex);

SPINE_C_API void spine_scale_timeline_dispose(spine_scale_timeline self);

SPINE_C_API spine_rtti spine_scale_timeline_get_rtti(spine_scale_timeline self);
SPINE_C_API void spine_scale_timeline_apply(spine_scale_timeline self, spine_skeleton skeleton, float lastTime, float time,
											/*@null*/ spine_array_event events, float alpha, bool fromSetup, bool add, bool out, bool appliedPose);
SPINE_C_API int spine_scale_timeline_get_bone_index(spine_scale_timeline self);
SPINE_C_API void spine_scale_timeline_set_bone_index(spine_scale_timeline self, int inValue);
SPINE_C_API void spine_scale_timeline_set_frame(spine_scale_timeline self, size_t frame, float time, float value1, float value2);
SPINE_C_API void spine_scale_timeline_set_linear(spine_scale_timeline self, size_t frame);
SPINE_C_API void spine_scale_timeline_set_stepped(spine_scale_timeline self, size_t frame);
SPINE_C_API void spine_scale_timeline_set_bezier(spine_scale_timeline self, size_t bezier, size_t frame, float value, float time1, float value1,
												 float cx1, float cy1, float cx2, float cy2, float time2, float value2);
SPINE_C_API float spine_scale_timeline_get_bezier_value(spine_scale_timeline self, float time, size_t frame, size_t valueOffset, size_t i);
SPINE_C_API spine_array_float spine_scale_timeline_get_curves(spine_scale_timeline self);
/**
 * True if this timeline supports additive blending.
 */
SPINE_C_API bool spine_scale_timeline_get_additive(spine_scale_timeline self);
/**
 * True if this timeline sets values instantaneously and does not support
 * interpolation between frames.
 */
SPINE_C_API bool spine_scale_timeline_get_instant(spine_scale_timeline self);
SPINE_C_API size_t spine_scale_timeline_get_frame_entries(spine_scale_timeline self);
SPINE_C_API size_t spine_scale_timeline_get_frame_count(spine_scale_timeline self);
SPINE_C_API spine_array_float spine_scale_timeline_get_frames(spine_scale_timeline self);
SPINE_C_API float spine_scale_timeline_get_duration(spine_scale_timeline self);
SPINE_C_API spine_array_property_id spine_scale_timeline_get_property_ids(spine_scale_timeline self);
SPINE_C_API spine_rtti spine_scale_timeline_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_SCALE_TIMELINE_H */
