#ifndef SPINE_SPINE_SLOT_POSE_H
#define SPINE_SPINE_SLOT_POSE_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_slot_pose spine_slot_pose_create(void);

SPINE_C_API void spine_slot_pose_dispose(spine_slot_pose self);

SPINE_C_API void spine_slot_pose_set(spine_slot_pose self, spine_slot_pose pose);
/**
 * The color used to tint the slot's attachment. If a dark color is set, this is
 * used as the light color for two color tinting.
 */
SPINE_C_API spine_color spine_slot_pose_get_color(spine_slot_pose self);
/**
 * The dark color used to tint the slot's attachment for two color tinting. The
 * dark color's alpha is not used.
 */
SPINE_C_API spine_color spine_slot_pose_get_dark_color(spine_slot_pose self);
/**
 * Returns true if this slot has a dark color.
 */
SPINE_C_API bool spine_slot_pose_has_dark_color(spine_slot_pose self);
SPINE_C_API void spine_slot_pose_set_has_dark_color(spine_slot_pose self, bool hasDarkColor);
/**
 * The current attachment for the slot, or null if the slot has no attachment.
 */
SPINE_C_API /*@null*/ spine_attachment spine_slot_pose_get_attachment(spine_slot_pose self);
/**
 * Sets the slot's attachment and, if the attachment changed, resets
 * sequenceIndex and clears the deform. The deform is not cleared if the old
 * attachment has the same VertexAttachment::getTimelineAttachment() as the
 * specified attachment.
 */
SPINE_C_API void spine_slot_pose_set_attachment(spine_slot_pose self, /*@null*/ spine_attachment attachment);
/**
 * The index of the texture region to display when the slot's attachment has a
 * Sequence. -1 represents the Sequence::getSetupIndex().
 */
SPINE_C_API int spine_slot_pose_get_sequence_index(spine_slot_pose self);
SPINE_C_API void spine_slot_pose_set_sequence_index(spine_slot_pose self, int sequenceIndex);
/**
 * Values to deform the slot's attachment. For an unweighted mesh, the entries
 * are local positions for each vertex. For a weighted mesh, the entries are an
 * offset for each vertex which will be added to the mesh's local vertex
 * positions.
 *
 * See VertexAttachment::computeWorldVertices() and DeformTimeline.
 */
SPINE_C_API spine_array_float spine_slot_pose_get_deform(spine_slot_pose self);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_SLOT_POSE_H */
