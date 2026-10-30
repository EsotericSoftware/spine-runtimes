#ifndef SPINE_SPINE_BONE_H
#define SPINE_SPINE_BONE_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

/**
 *
 * @param parent May be NULL.
 */
SPINE_C_API spine_bone spine_bone_create(spine_bone_data data, /*@null*/ spine_bone parent);
/**
 * Copy constructor. Does not copy the child bones.
 */
SPINE_C_API spine_bone spine_bone_create2(spine_bone bone, /*@null*/ spine_bone parent);

SPINE_C_API void spine_bone_dispose(spine_bone self);

SPINE_C_API spine_rtti spine_bone_get_rtti(spine_bone self);
/**
 * The parent bone, or null if this is the root bone.
 */
SPINE_C_API /*@null*/ spine_bone spine_bone_get_parent(spine_bone self);
/**
 * The immediate children of this bone.
 */
SPINE_C_API spine_array_bone spine_bone_get_children(spine_bone self);
SPINE_C_API bool spine_bone_is_y_down(void);
SPINE_C_API void spine_bone_set_y_down(bool value);
SPINE_C_API void spine_bone_update(spine_bone self, spine_skeleton skeleton, spine_physics physics);
/**
 * The setup pose data. May be shared with multiple instances.
 */
SPINE_C_API spine_bone_data spine_bone_get_data(spine_bone self);
/**
 * The unconstrained pose for this object, set by animations and application
 * code.
 */
SPINE_C_API spine_bone_pose spine_bone_get_pose(spine_bone self);
/**
 * The pose to use for rendering. If no constraints modify this pose, this is
 * the same as getPose(). Otherwise it is a copy of getPose() modified by
 * constraints.
 */
SPINE_C_API spine_bone_pose spine_bone_get_applied_pose(spine_bone self);
/**
 * Sets the constrained pose to the unconstrained pose, as a starting point for
 * constraints to be applied.
 */
SPINE_C_API void spine_bone_reset_constrained(spine_bone self);
/**
 * Sets the applied pose to the constrained pose, in anticipation of the applied
 * pose being modified by constraints.
 */
SPINE_C_API void spine_bone_constrained(spine_bone self);
SPINE_C_API bool spine_bone_is_pose_equal_to_applied(spine_bone self);
/**
 * Returns false when this won't be updated by
 * Skeleton::updateWorldTransform(Physics) because a skin is required and the
 * active skin does not contain this item. See Skin::getBones(),
 * Skin::getConstraints(), PosedData::getSkinRequired(), and
 * Skeleton::updateCache().
 */
SPINE_C_API bool spine_bone_is_active(spine_bone self);
SPINE_C_API void spine_bone_set_active(spine_bone self, bool active);
SPINE_C_API spine_rtti spine_bone_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_BONE_H */
