#ifndef SPINE_SPINE_IK_CONSTRAINT_H
#define SPINE_SPINE_IK_CONSTRAINT_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_ik_constraint spine_ik_constraint_create(spine_ik_constraint_data data, spine_skeleton skeleton);

SPINE_C_API void spine_ik_constraint_dispose(spine_ik_constraint self);

SPINE_C_API spine_rtti spine_ik_constraint_get_rtti(spine_ik_constraint self);
SPINE_C_API spine_ik_constraint spine_ik_constraint_copy(spine_ik_constraint self, spine_skeleton skeleton);
SPINE_C_API void spine_ik_constraint_update(spine_ik_constraint self, spine_skeleton skeleton, spine_physics physics);
SPINE_C_API void spine_ik_constraint_sort(spine_ik_constraint self, spine_skeleton skeleton);
SPINE_C_API bool spine_ik_constraint_is_source_active(spine_ik_constraint self);
SPINE_C_API spine_array_bone_pose spine_ik_constraint_get_bones(spine_ik_constraint self);
SPINE_C_API spine_bone spine_ik_constraint_get_target(spine_ik_constraint self);
SPINE_C_API void spine_ik_constraint_set_target(spine_ik_constraint self, spine_bone inValue);
/**
 * Adjusts the local rotation of the bone so the world position of the tip is as
 * close to the target position as possible. The target is specified in the
 * world coordinate system.
 */
SPINE_C_API void spine_ik_constraint_apply_1(spine_skeleton skeleton, spine_bone_pose bone, float targetX, float targetY, bool compress, bool stretch,
											 spine_scale_y scaleY, float mix);
/**
 * Adjusts the parent and child bone rotations so the tip of the child is as
 * close to the target position as possible. The target is specified in the
 * world coordinate system.
 *
 * @param child A direct descendant of the parent bone.
 */
SPINE_C_API void spine_ik_constraint_apply_2(spine_skeleton skeleton, spine_bone_pose parent, spine_bone_pose child, float targetX, float targetY,
											 int bendDirection, bool stretch, spine_scale_y scaleY, float softness, float mix);
SPINE_C_API spine_ik_constraint_data spine_ik_constraint_get_data(spine_ik_constraint self);
/**
 * The unconstrained pose for this object, set by animations and application
 * code.
 */
SPINE_C_API spine_ik_constraint_pose spine_ik_constraint_get_pose(spine_ik_constraint self);
/**
 * The pose to use for rendering. If no constraints modify this pose, this is
 * the same as getPose(). Otherwise it is a copy of getPose() modified by
 * constraints.
 */
SPINE_C_API spine_ik_constraint_pose spine_ik_constraint_get_applied_pose(spine_ik_constraint self);
/**
 * Sets the constrained pose to the unconstrained pose, as a starting point for
 * constraints to be applied.
 */
SPINE_C_API void spine_ik_constraint_reset_constrained(spine_ik_constraint self);
/**
 * Sets the applied pose to the constrained pose, in anticipation of the applied
 * pose being modified by constraints.
 */
SPINE_C_API void spine_ik_constraint_constrained(spine_ik_constraint self);
SPINE_C_API bool spine_ik_constraint_is_pose_equal_to_applied(spine_ik_constraint self);
/**
 * Returns false when this won't be updated by
 * Skeleton::updateWorldTransform(Physics) because a skin is required and the
 * active skin does not contain this item. See Skin::getBones(),
 * Skin::getConstraints(), PosedData::getSkinRequired(), and
 * Skeleton::updateCache().
 */
SPINE_C_API bool spine_ik_constraint_is_active(spine_ik_constraint self);
SPINE_C_API void spine_ik_constraint_set_active(spine_ik_constraint self, bool active);
SPINE_C_API spine_rtti spine_ik_constraint_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_IK_CONSTRAINT_H */
