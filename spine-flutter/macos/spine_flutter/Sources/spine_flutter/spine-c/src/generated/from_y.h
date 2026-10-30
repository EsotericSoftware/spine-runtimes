#ifndef SPINE_SPINE_FROM_Y_H
#define SPINE_SPINE_FROM_Y_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_from_y spine_from_y_create(void);

SPINE_C_API void spine_from_y_dispose(spine_from_y self);

SPINE_C_API spine_rtti spine_from_y_get_rtti(spine_from_y self);
SPINE_C_API float spine_from_y_value(spine_from_y self, spine_skeleton skeleton, spine_bone_pose source, bool local, /*@null*/ float *offsets);
SPINE_C_API spine_rtti spine_from_y_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_FROM_Y_H */
