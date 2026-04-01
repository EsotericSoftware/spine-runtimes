#ifndef SPINE_SPINE_IK_CONSTRAINT_POSE_H
#define SPINE_SPINE_IK_CONSTRAINT_POSE_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_ik_constraint_pose spine_ik_constraint_pose_create(void);

SPINE_C_API void spine_ik_constraint_pose_dispose(spine_ik_constraint_pose self);

SPINE_C_API void spine_ik_constraint_pose_set(spine_ik_constraint_pose self, spine_ik_constraint_pose pose);
/**
 * A percentage (0-1) that controls the mix between the constrained and
 * unconstrained rotation.
 *
 * For two bone IK: if the parent bone has local nonuniform scale, the child
 * bone's local Y translation is set to 0.
 */
SPINE_C_API float spine_ik_constraint_pose_get_mix(spine_ik_constraint_pose self);
SPINE_C_API void spine_ik_constraint_pose_set_mix(spine_ik_constraint_pose self, float mix);
/**
 * For two bone IK, the target bone's distance from the maximum reach of the
 * bones where rotation begins to slow. The bones will not straighten completely
 * until the target is this far out of range.
 */
SPINE_C_API float spine_ik_constraint_pose_get_softness(spine_ik_constraint_pose self);
SPINE_C_API void spine_ik_constraint_pose_set_softness(spine_ik_constraint_pose self, float softness);
/**
 * For two bone IK, controls the bend direction of the IK bones, either 1 or -1.
 */
SPINE_C_API int spine_ik_constraint_pose_get_bend_direction(spine_ik_constraint_pose self);
SPINE_C_API void spine_ik_constraint_pose_set_bend_direction(spine_ik_constraint_pose self, int bendDirection);
/**
 * For one bone IK, when true and the target is too close, the bone is scaled to
 * reach it.
 */
SPINE_C_API bool spine_ik_constraint_pose_get_compress(spine_ik_constraint_pose self);
SPINE_C_API void spine_ik_constraint_pose_set_compress(spine_ik_constraint_pose self, bool compress);
/**
 * When true and the target is out of range, the parent bone is scaled to reach
 * it.
 *
 * For two bone IK: 1) the child bone's local Y translation is set to 0, 2)
 * stretch is not applied if softness is > 0, and 3) if the parent bone has
 * local nonuniform scale, stretch is not applied.
 */
SPINE_C_API bool spine_ik_constraint_pose_get_stretch(spine_ik_constraint_pose self);
SPINE_C_API void spine_ik_constraint_pose_set_stretch(spine_ik_constraint_pose self, bool stretch);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_IK_CONSTRAINT_POSE_H */
