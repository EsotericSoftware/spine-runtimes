#ifndef SPINE_SPINE_TO_ROTATE_H
#define SPINE_SPINE_TO_ROTATE_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_to_rotate spine_to_rotate_create(void);

SPINE_C_API void spine_to_rotate_dispose(spine_to_rotate self);

SPINE_C_API spine_rtti spine_to_rotate_get_rtti(spine_to_rotate self);
SPINE_C_API float spine_to_rotate_mix(spine_to_rotate self, spine_transform_constraint_pose pose);
SPINE_C_API void spine_to_rotate_apply(spine_to_rotate self, spine_skeleton skeleton, spine_transform_constraint_pose pose, spine_bone_pose bone,
									   float value, bool local, bool additive);
SPINE_C_API spine_rtti spine_to_rotate_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_TO_ROTATE_H */
