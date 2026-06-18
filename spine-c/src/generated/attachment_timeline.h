#ifndef SPINE_SPINE_ATTACHMENT_TIMELINE_H
#define SPINE_SPINE_ATTACHMENT_TIMELINE_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_attachment_timeline spine_attachment_timeline_create(size_t frameCount, int slotIndex);

SPINE_C_API void spine_attachment_timeline_dispose(spine_attachment_timeline self);

SPINE_C_API spine_rtti spine_attachment_timeline_get_rtti(spine_attachment_timeline self);
SPINE_C_API void spine_attachment_timeline_apply(spine_attachment_timeline self, spine_skeleton skeleton, float lastTime, float time,
												 /*@null*/ spine_array_event events, float alpha, spine_mix_from from, bool add, bool out,
												 bool appliedPose);
/**
 * Sets the time and attachment name for the specified frame.
 *
 * @param frame Between 0 and frameCount, inclusive.
 * @param time The frame time in seconds.
 */
SPINE_C_API void spine_attachment_timeline_set_frame(spine_attachment_timeline self, int frame, float time, const char *attachmentName);
SPINE_C_API int spine_attachment_timeline_get_slot_index(spine_attachment_timeline self);
SPINE_C_API void spine_attachment_timeline_set_slot_index(spine_attachment_timeline self, int inValue);
/**
 * True if this timeline supports additive blending.
 */
SPINE_C_API bool spine_attachment_timeline_get_additive(spine_attachment_timeline self);
/**
 * True if this timeline sets values instantaneously and does not support
 * interpolation between frames.
 */
SPINE_C_API bool spine_attachment_timeline_get_instant(spine_attachment_timeline self);
SPINE_C_API size_t spine_attachment_timeline_get_frame_entries(spine_attachment_timeline self);
SPINE_C_API size_t spine_attachment_timeline_get_frame_count(spine_attachment_timeline self);
SPINE_C_API spine_array_float spine_attachment_timeline_get_frames(spine_attachment_timeline self);
SPINE_C_API float spine_attachment_timeline_get_duration(spine_attachment_timeline self);
SPINE_C_API spine_array_property_id spine_attachment_timeline_get_property_ids(spine_attachment_timeline self);
SPINE_C_API spine_rtti spine_attachment_timeline_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_ATTACHMENT_TIMELINE_H */
