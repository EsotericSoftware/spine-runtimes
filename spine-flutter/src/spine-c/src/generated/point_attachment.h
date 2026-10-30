#ifndef SPINE_SPINE_POINT_ATTACHMENT_H
#define SPINE_SPINE_POINT_ATTACHMENT_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_point_attachment spine_point_attachment_create(const char *name);

SPINE_C_API void spine_point_attachment_dispose(spine_point_attachment self);

SPINE_C_API spine_rtti spine_point_attachment_get_rtti(spine_point_attachment self);
/**
 * The local x position.
 */
SPINE_C_API float spine_point_attachment_get_x(spine_point_attachment self);
SPINE_C_API void spine_point_attachment_set_x(spine_point_attachment self, float inValue);
/**
 * The local y position.
 */
SPINE_C_API float spine_point_attachment_get_y(spine_point_attachment self);
SPINE_C_API void spine_point_attachment_set_y(spine_point_attachment self, float inValue);
/**
 * The local rotation in degrees, counter clockwise.
 */
SPINE_C_API float spine_point_attachment_get_rotation(spine_point_attachment self);
SPINE_C_API void spine_point_attachment_set_rotation(spine_point_attachment self, float inValue);
SPINE_C_API spine_color spine_point_attachment_get_color(spine_point_attachment self);
/**
 * Computes the world position from the local position.
 */
SPINE_C_API void spine_point_attachment_compute_world_position(spine_point_attachment self, spine_bone_pose bone, float *ox, float *oy);
/**
 * Computes the world rotation from the local rotation.
 */
SPINE_C_API float spine_point_attachment_compute_world_rotation(spine_point_attachment self, spine_bone_pose bone);
SPINE_C_API spine_attachment spine_point_attachment_copy(spine_point_attachment self);
SPINE_C_API const char *spine_point_attachment_get_name(spine_point_attachment self);
SPINE_C_API /*@null*/ spine_attachment spine_point_attachment_get_timeline_attachment(spine_point_attachment self);
SPINE_C_API void spine_point_attachment_set_timeline_attachment(spine_point_attachment self, /*@null*/ spine_attachment attachment);
SPINE_C_API spine_array_int spine_point_attachment_get_timeline_slots(spine_point_attachment self);
SPINE_C_API void spine_point_attachment_set_timeline_slots(spine_point_attachment self, spine_array_int timelineSlots);
/**
 * Returns true if the slotIndex or any getTimelineSlots() have an attachment
 * whose getTimelineAttachment() is this attachment.
 *
 * @param slots The Skeleton::getSlots().
 * @param slotIndex The timeline's primary slot index.
 */
SPINE_C_API bool spine_point_attachment_is_timeline_active(spine_point_attachment self, spine_array_slot slots, int slotIndex, bool appliedPose);
SPINE_C_API int spine_point_attachment_get_ref_count(spine_point_attachment self);
SPINE_C_API void spine_point_attachment_reference(spine_point_attachment self);
SPINE_C_API void spine_point_attachment_dereference(spine_point_attachment self);
SPINE_C_API spine_rtti spine_point_attachment_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_POINT_ATTACHMENT_H */
