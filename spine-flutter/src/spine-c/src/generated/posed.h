#ifndef SPINE_SPINE_POSED_H
#define SPINE_SPINE_POSED_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API void spine_posed_dispose(spine_posed self);

SPINE_C_API void spine_posed_constrained(spine_posed self);
SPINE_C_API void spine_posed_reset_constrained(spine_posed self);
SPINE_C_API bool spine_posed_is_pose_equal_to_applied(spine_posed self);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_POSED_H */
