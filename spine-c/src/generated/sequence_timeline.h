#ifndef SPINE_SPINE_SEQUENCE_TIMELINE_H
#define SPINE_SPINE_SEQUENCE_TIMELINE_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_sequence_timeline spine_sequence_timeline_create(size_t frameCount, int slotIndex, spine_attachment attachment);

SPINE_C_API void spine_sequence_timeline_dispose(spine_sequence_timeline self);

SPINE_C_API spine_rtti spine_sequence_timeline_get_rtti(spine_sequence_timeline self);
SPINE_C_API void spine_sequence_timeline_apply(spine_sequence_timeline self, spine_skeleton skeleton, float lastTime, float time,
											   /*@null*/ spine_array_event events, float alpha, bool fromSetup, bool add, bool out, bool appliedPose);
/**
 * Sets the time, mode, index, and frame time for the specified frame.
 *
 * @param frame Between 0 and frameCount, inclusive.
 * @param delay Seconds between frames.
 */
SPINE_C_API void spine_sequence_timeline_set_frame(spine_sequence_timeline self, int frame, float time, spine_sequence_mode mode, int index,
												   float delay);
/**
 * The attachment for which the sequence index will be set.
 *
 * See Attachment::getTimelineAttachment().
 */
SPINE_C_API spine_attachment spine_sequence_timeline_get_attachment(spine_sequence_timeline self);
SPINE_C_API int spine_sequence_timeline_get_slot_index(spine_sequence_timeline self);
SPINE_C_API void spine_sequence_timeline_set_slot_index(spine_sequence_timeline self, int inValue);
/**
 * True if this timeline supports additive blending.
 */
SPINE_C_API bool spine_sequence_timeline_get_additive(spine_sequence_timeline self);
/**
 * True if this timeline sets values instantaneously and does not support
 * interpolation between frames.
 */
SPINE_C_API bool spine_sequence_timeline_get_instant(spine_sequence_timeline self);
SPINE_C_API size_t spine_sequence_timeline_get_frame_entries(spine_sequence_timeline self);
SPINE_C_API size_t spine_sequence_timeline_get_frame_count(spine_sequence_timeline self);
SPINE_C_API spine_array_float spine_sequence_timeline_get_frames(spine_sequence_timeline self);
SPINE_C_API float spine_sequence_timeline_get_duration(spine_sequence_timeline self);
SPINE_C_API spine_array_property_id spine_sequence_timeline_get_property_ids(spine_sequence_timeline self);
SPINE_C_API spine_rtti spine_sequence_timeline_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_SEQUENCE_TIMELINE_H */
