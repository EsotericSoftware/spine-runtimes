#ifndef SPINE_SPINE_TRANSFORM_CONSTRAINT_POSE_H
#define SPINE_SPINE_TRANSFORM_CONSTRAINT_POSE_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_transform_constraint_pose spine_transform_constraint_pose_create(void);

SPINE_C_API void spine_transform_constraint_pose_dispose(spine_transform_constraint_pose self);

SPINE_C_API void spine_transform_constraint_pose_set(spine_transform_constraint_pose self, spine_transform_constraint_pose pose);
/**
 * A percentage that controls the mix between the constrained and unconstrained
 * rotation.
 */
SPINE_C_API float spine_transform_constraint_pose_get_mix_rotate(spine_transform_constraint_pose self);
SPINE_C_API void spine_transform_constraint_pose_set_mix_rotate(spine_transform_constraint_pose self, float mixRotate);
/**
 * A percentage that controls the mix between the constrained and unconstrained
 * translation X.
 */
SPINE_C_API float spine_transform_constraint_pose_get_mix_x(spine_transform_constraint_pose self);
SPINE_C_API void spine_transform_constraint_pose_set_mix_x(spine_transform_constraint_pose self, float mixX);
/**
 * A percentage that controls the mix between the constrained and unconstrained
 * translation Y.
 */
SPINE_C_API float spine_transform_constraint_pose_get_mix_y(spine_transform_constraint_pose self);
SPINE_C_API void spine_transform_constraint_pose_set_mix_y(spine_transform_constraint_pose self, float mixY);
/**
 * A percentage that controls the mix between the constrained and unconstrained
 * scale X.
 */
SPINE_C_API float spine_transform_constraint_pose_get_mix_scale_x(spine_transform_constraint_pose self);
SPINE_C_API void spine_transform_constraint_pose_set_mix_scale_x(spine_transform_constraint_pose self, float mixScaleX);
/**
 * A percentage that controls the mix between the constrained and unconstrained
 * scale Y.
 */
SPINE_C_API float spine_transform_constraint_pose_get_mix_scale_y(spine_transform_constraint_pose self);
SPINE_C_API void spine_transform_constraint_pose_set_mix_scale_y(spine_transform_constraint_pose self, float mixScaleY);
/**
 * A percentage that controls the mix between the constrained and unconstrained
 * shear Y.
 */
SPINE_C_API float spine_transform_constraint_pose_get_mix_shear_y(spine_transform_constraint_pose self);
SPINE_C_API void spine_transform_constraint_pose_set_mix_shear_y(spine_transform_constraint_pose self, float mixShearY);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_TRANSFORM_CONSTRAINT_POSE_H */
