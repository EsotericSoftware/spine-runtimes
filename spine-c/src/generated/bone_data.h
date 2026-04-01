#ifndef SPINE_SPINE_BONE_DATA_H
#define SPINE_SPINE_BONE_DATA_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_bone_data spine_bone_data_create(int index, const char *name, /*@null*/ spine_bone_data parent);

SPINE_C_API void spine_bone_data_dispose(spine_bone_data self);

/**
 * The Skeleton::getBones() index for this bone.
 */
SPINE_C_API int spine_bone_data_get_index(spine_bone_data self);
/**
 * The parent bone, or NULL if this bone is the root.
 */
SPINE_C_API /*@null*/ spine_bone_data spine_bone_data_get_parent(spine_bone_data self);
SPINE_C_API float spine_bone_data_get_length(spine_bone_data self);
SPINE_C_API void spine_bone_data_set_length(spine_bone_data self, float inValue);
SPINE_C_API spine_color spine_bone_data_get_color(spine_bone_data self);
/**
 * The bone icon name as it was in Spine, or empty if nonessential data was not
 * exported.
 */
SPINE_C_API const char *spine_bone_data_get_icon(spine_bone_data self);
SPINE_C_API void spine_bone_data_set_icon(spine_bone_data self, const char *icon);
SPINE_C_API bool spine_bone_data_get_visible(spine_bone_data self);
SPINE_C_API void spine_bone_data_set_visible(spine_bone_data self, bool inValue);
/**
 * The setup pose that most animations are relative to.
 */
SPINE_C_API spine_bone_pose spine_bone_data_get_setup_pose(spine_bone_data self);
SPINE_C_API const char *spine_bone_data_get_name(spine_bone_data self);
/**
 * When true, Skeleton::updateWorldTransform(Physics) only updates this
 * constraint if the Skeleton::getSkin() contains this constraint.
 *
 * See Skin::getConstraints().
 */
SPINE_C_API bool spine_bone_data_get_skin_required(spine_bone_data self);
SPINE_C_API void spine_bone_data_set_skin_required(spine_bone_data self, bool skinRequired);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_BONE_DATA_H */
