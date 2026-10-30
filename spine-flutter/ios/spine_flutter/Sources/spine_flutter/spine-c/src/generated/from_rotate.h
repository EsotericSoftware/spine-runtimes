#ifndef SPINE_SPINE_FROM_ROTATE_H
#define SPINE_SPINE_FROM_ROTATE_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_from_rotate spine_from_rotate_create(void);

SPINE_C_API void spine_from_rotate_dispose(spine_from_rotate self);

SPINE_C_API spine_rtti spine_from_rotate_get_rtti(spine_from_rotate self);
SPINE_C_API float spine_from_rotate_value(spine_from_rotate self, spine_skeleton skeleton, spine_bone_pose source, bool local,
										  /*@null*/ float *offsets);
SPINE_C_API spine_rtti spine_from_rotate_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_FROM_ROTATE_H */
