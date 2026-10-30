#ifndef SPINE_SPINE_FROM_X_H
#define SPINE_SPINE_FROM_X_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_from_x spine_from_x_create(void);

SPINE_C_API void spine_from_x_dispose(spine_from_x self);

SPINE_C_API spine_rtti spine_from_x_get_rtti(spine_from_x self);
SPINE_C_API float spine_from_x_value(spine_from_x self, spine_skeleton skeleton, spine_bone_pose source, bool local, /*@null*/ float *offsets);
SPINE_C_API spine_rtti spine_from_x_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_FROM_X_H */
