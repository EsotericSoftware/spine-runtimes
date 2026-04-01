#ifndef SPINE_SPINE_SLOT_DATA_H
#define SPINE_SPINE_SLOT_DATA_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_slot_data spine_slot_data_create(int index, const char *name, spine_bone_data boneData);

SPINE_C_API void spine_slot_data_dispose(spine_slot_data self);

/**
 * The Skeleton::getSlots() index for this slot.
 */
SPINE_C_API int spine_slot_data_get_index(spine_slot_data self);
/**
 * The bone this slot belongs to.
 */
SPINE_C_API spine_bone_data spine_slot_data_get_bone_data(spine_slot_data self);
SPINE_C_API void spine_slot_data_set_attachment_name(spine_slot_data self, const char *attachmentName);
/**
 * The name of the attachment that is visible for this slot in the setup pose,
 * or empty if no attachment is visible.
 */
SPINE_C_API const char *spine_slot_data_get_attachment_name(spine_slot_data self);
/**
 * The blend mode for drawing the slot's attachment.
 */
SPINE_C_API spine_blend_mode spine_slot_data_get_blend_mode(spine_slot_data self);
SPINE_C_API void spine_slot_data_set_blend_mode(spine_slot_data self, spine_blend_mode blendMode);
/**
 * False if the slot was hidden in Spine and nonessential data was exported.
 * Does not affect runtime rendering.
 */
SPINE_C_API bool spine_slot_data_get_visible(spine_slot_data self);
SPINE_C_API void spine_slot_data_set_visible(spine_slot_data self, bool visible);
/**
 * The setup pose that most animations are relative to.
 */
SPINE_C_API spine_slot_pose spine_slot_data_get_setup_pose(spine_slot_data self);
SPINE_C_API const char *spine_slot_data_get_name(spine_slot_data self);
/**
 * When true, Skeleton::updateWorldTransform(Physics) only updates this
 * constraint if the Skeleton::getSkin() contains this constraint.
 *
 * See Skin::getConstraints().
 */
SPINE_C_API bool spine_slot_data_get_skin_required(spine_slot_data self);
SPINE_C_API void spine_slot_data_set_skin_required(spine_slot_data self, bool skinRequired);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_SLOT_DATA_H */
