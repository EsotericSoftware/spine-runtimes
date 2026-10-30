#ifndef SPINE_SPINE_BONE_LOCAL_H
#define SPINE_SPINE_BONE_LOCAL_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_bone_local spine_bone_local_create(void);

SPINE_C_API void spine_bone_local_dispose(spine_bone_local self);

SPINE_C_API void spine_bone_local_set(spine_bone_local self, spine_bone_local pose);
/**
 * The local x translation.
 */
SPINE_C_API float spine_bone_local_get_x(spine_bone_local self);
SPINE_C_API void spine_bone_local_set_x(spine_bone_local self, float x);
/**
 * The local y translation.
 */
SPINE_C_API float spine_bone_local_get_y(spine_bone_local self);
SPINE_C_API void spine_bone_local_set_y(spine_bone_local self, float y);
/**
 * Sets local x and y translation.
 */
SPINE_C_API void spine_bone_local_set_position(spine_bone_local self, float x, float y);
/**
 * The local rotation in degrees, counter clockwise.
 */
SPINE_C_API float spine_bone_local_get_rotation(spine_bone_local self);
SPINE_C_API void spine_bone_local_set_rotation(spine_bone_local self, float rotation);
/**
 * The local scaleX.
 */
SPINE_C_API float spine_bone_local_get_scale_x(spine_bone_local self);
SPINE_C_API void spine_bone_local_set_scale_x(spine_bone_local self, float scaleX);
/**
 * The local scaleY.
 */
SPINE_C_API float spine_bone_local_get_scale_y(spine_bone_local self);
SPINE_C_API void spine_bone_local_set_scale_y(spine_bone_local self, float scaleY);
/**
 * Sets local scaleX and scaleY.
 */
SPINE_C_API void spine_bone_local_set_scale_1(spine_bone_local self, float scaleX, float scaleY);
/**
 * Sets local scaleX and scaleY to the same value.
 */
SPINE_C_API void spine_bone_local_set_scale_2(spine_bone_local self, float scale);
/**
 * The local shearX.
 */
SPINE_C_API float spine_bone_local_get_shear_x(spine_bone_local self);
SPINE_C_API void spine_bone_local_set_shear_x(spine_bone_local self, float shearX);
/**
 * The local shearY.
 */
SPINE_C_API float spine_bone_local_get_shear_y(spine_bone_local self);
SPINE_C_API void spine_bone_local_set_shear_y(spine_bone_local self, float shearY);
/**
 * Determines how parent world transforms affect this bone.
 */
SPINE_C_API spine_inherit spine_bone_local_get_inherit(spine_bone_local self);
SPINE_C_API void spine_bone_local_set_inherit(spine_bone_local self, spine_inherit inherit);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_BONE_LOCAL_H */
