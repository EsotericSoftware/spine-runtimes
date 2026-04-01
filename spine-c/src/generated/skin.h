#ifndef SPINE_SPINE_SKIN_H
#define SPINE_SPINE_SKIN_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_skin spine_skin_create(const char *name);

SPINE_C_API void spine_skin_dispose(spine_skin self);

/**
 * Adds an attachment to the skin for the specified slot index and placeholder
 * name. If the placeholder name already exists for the slot, the previous value
 * is replaced.
 */
SPINE_C_API void spine_skin_set_attachment(spine_skin self, size_t slotIndex, const char *placeholderName, /*@null*/ spine_attachment attachment);
/**
 * Returns the attachment for the specified slot index and placeholder name, or
 * NULL.
 */
SPINE_C_API /*@null*/ spine_attachment spine_skin_get_attachment(spine_skin self, size_t slotIndex, const char *placeholderName);
/**
 * Removes the attachment from the skin.
 */
SPINE_C_API void spine_skin_remove_attachment(spine_skin self, size_t slotIndex, const char *placeholderName);
/**
 * Finds the attachments for a given slot. The results are added to the passed
 * array of Attachments.
 *
 * @param slotIndex The target slotIndex. To find the slot index, use SkeletonData::findSlot and SlotData::getIndex.
 * @param attachments Found Attachments will be added to this array.
 */
SPINE_C_API void spine_skin_find_attachments_for_slot(spine_skin self, size_t slotIndex, spine_array_attachment attachments);
SPINE_C_API const char *spine_skin_get_name(spine_skin self);
/**
 * Adds all attachments, bones, and constraints from the specified skin to this
 * skin.
 */
SPINE_C_API void spine_skin_add_skin(spine_skin self, spine_skin other);
/**
 * Adds all attachments, bones, and constraints from the specified skin to this
 * skin. Attachments are deep copied.
 */
SPINE_C_API void spine_skin_copy_skin(spine_skin self, spine_skin other);
SPINE_C_API spine_array_bone_data spine_skin_get_bones(spine_skin self);
SPINE_C_API spine_array_constraint_data spine_skin_get_constraints(spine_skin self);
SPINE_C_API spine_color spine_skin_get_color(spine_skin self);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_SKIN_H */
