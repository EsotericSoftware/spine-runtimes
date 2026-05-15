#ifndef SPINE_SPINE_ATTACHMENT_H
#define SPINE_SPINE_ATTACHMENT_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API void spine_attachment_dispose(spine_attachment self);

SPINE_C_API spine_rtti spine_attachment_get_rtti(spine_attachment self);
SPINE_C_API const char *spine_attachment_get_name(spine_attachment self);
SPINE_C_API spine_attachment spine_attachment_copy(spine_attachment self);
SPINE_C_API /*@null*/ spine_attachment spine_attachment_get_timeline_attachment(spine_attachment self);
SPINE_C_API void spine_attachment_set_timeline_attachment(spine_attachment self, /*@null*/ spine_attachment attachment);
SPINE_C_API spine_array_int spine_attachment_get_timeline_slots(spine_attachment self);
SPINE_C_API void spine_attachment_set_timeline_slots(spine_attachment self, spine_array_int timelineSlots);
/**
 * Returns true if the slotIndex or any getTimelineSlots() have an attachment
 * whose getTimelineAttachment() is this attachment.
 *
 * @param slots The Skeleton::getSlots().
 * @param slotIndex The timeline's primary slot index.
 */
SPINE_C_API bool spine_attachment_is_timeline_active(spine_attachment self, spine_array_slot slots, int slotIndex, bool appliedPose);
SPINE_C_API int spine_attachment_get_ref_count(spine_attachment self);
SPINE_C_API void spine_attachment_reference(spine_attachment self);
SPINE_C_API void spine_attachment_dereference(spine_attachment self);
SPINE_C_API spine_rtti spine_attachment_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_ATTACHMENT_H */
