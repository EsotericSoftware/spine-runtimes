#ifndef SPINE_SPINE_DEFORM_TIMELINE_H
#define SPINE_SPINE_DEFORM_TIMELINE_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_deform_timeline spine_deform_timeline_create(size_t frameCount, size_t bezierCount, int slotIndex,
															   spine_vertex_attachment attachment);

SPINE_C_API void spine_deform_timeline_dispose(spine_deform_timeline self);

SPINE_C_API spine_rtti spine_deform_timeline_get_rtti(spine_deform_timeline self);
SPINE_C_API void spine_deform_timeline_apply(spine_deform_timeline self, spine_skeleton skeleton, float lastTime, float time,
											 /*@null*/ spine_array_event events, float alpha, spine_mix_from from, bool add, bool out,
											 bool appliedPose);
/**
 * Sets the time and vertices for the specified frame.
 */
SPINE_C_API void spine_deform_timeline_set_frame(spine_deform_timeline self, int frameIndex, float time, spine_array_float vertices);
/**
 * The attachment whose vertices will be deformed.
 */
SPINE_C_API spine_vertex_attachment spine_deform_timeline_get_attachment(spine_deform_timeline self);
SPINE_C_API void spine_deform_timeline_set_attachment(spine_deform_timeline self, spine_vertex_attachment inValue);
SPINE_C_API void spine_deform_timeline_set_bezier(spine_deform_timeline self, size_t bezier, size_t frame, float value, float time1, float value1,
												  float cx1, float cy1, float cx2, float cy2, float time2, float value2);
SPINE_C_API float spine_deform_timeline_get_curve_percent(spine_deform_timeline self, float time, int frame);
SPINE_C_API size_t spine_deform_timeline_get_frame_count(spine_deform_timeline self);
SPINE_C_API int spine_deform_timeline_get_slot_index(spine_deform_timeline self);
SPINE_C_API void spine_deform_timeline_set_slot_index(spine_deform_timeline self, int inValue);
SPINE_C_API void spine_deform_timeline_set_linear(spine_deform_timeline self, size_t frame);
SPINE_C_API void spine_deform_timeline_set_stepped(spine_deform_timeline self, size_t frame);
SPINE_C_API float spine_deform_timeline_get_bezier_value(spine_deform_timeline self, float time, size_t frame, size_t valueOffset, size_t i);
SPINE_C_API spine_array_float spine_deform_timeline_get_curves(spine_deform_timeline self);
/**
 * True if this timeline supports additive blending.
 */
SPINE_C_API bool spine_deform_timeline_get_additive(spine_deform_timeline self);
/**
 * True if this timeline sets values instantaneously and does not support
 * interpolation between frames.
 */
SPINE_C_API bool spine_deform_timeline_get_instant(spine_deform_timeline self);
SPINE_C_API size_t spine_deform_timeline_get_frame_entries(spine_deform_timeline self);
SPINE_C_API spine_array_float spine_deform_timeline_get_frames(spine_deform_timeline self);
SPINE_C_API float spine_deform_timeline_get_duration(spine_deform_timeline self);
SPINE_C_API spine_array_property_id spine_deform_timeline_get_property_ids(spine_deform_timeline self);
SPINE_C_API spine_rtti spine_deform_timeline_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_DEFORM_TIMELINE_H */
