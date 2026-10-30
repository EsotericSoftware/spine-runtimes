#ifndef SPINE_SPINE_PATH_CONSTRAINT_POSE_H
#define SPINE_SPINE_PATH_CONSTRAINT_POSE_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_path_constraint_pose spine_path_constraint_pose_create(void);

SPINE_C_API void spine_path_constraint_pose_dispose(spine_path_constraint_pose self);

SPINE_C_API void spine_path_constraint_pose_set(spine_path_constraint_pose self, spine_path_constraint_pose pose);
/**
 * The position along the path.
 */
SPINE_C_API float spine_path_constraint_pose_get_position(spine_path_constraint_pose self);
SPINE_C_API void spine_path_constraint_pose_set_position(spine_path_constraint_pose self, float position);
/**
 * The spacing between bones.
 */
SPINE_C_API float spine_path_constraint_pose_get_spacing(spine_path_constraint_pose self);
SPINE_C_API void spine_path_constraint_pose_set_spacing(spine_path_constraint_pose self, float spacing);
/**
 * A percentage (0-1) that controls the mix between the constrained and
 * unconstrained rotation.
 */
SPINE_C_API float spine_path_constraint_pose_get_mix_rotate(spine_path_constraint_pose self);
SPINE_C_API void spine_path_constraint_pose_set_mix_rotate(spine_path_constraint_pose self, float mixRotate);
/**
 * A percentage (0-1) that controls the mix between the constrained and
 * unconstrained translation X.
 */
SPINE_C_API float spine_path_constraint_pose_get_mix_x(spine_path_constraint_pose self);
SPINE_C_API void spine_path_constraint_pose_set_mix_x(spine_path_constraint_pose self, float mixX);
/**
 * A percentage (0-1) that controls the mix between the constrained and
 * unconstrained translation Y.
 */
SPINE_C_API float spine_path_constraint_pose_get_mix_y(spine_path_constraint_pose self);
SPINE_C_API void spine_path_constraint_pose_set_mix_y(spine_path_constraint_pose self, float mixY);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_PATH_CONSTRAINT_POSE_H */
