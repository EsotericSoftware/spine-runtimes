#ifndef SPINE_SPINE_CONSTRAINT_H
#define SPINE_SPINE_CONSTRAINT_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API void spine_constraint_dispose(spine_constraint self);

SPINE_C_API spine_rtti spine_constraint_get_rtti(spine_constraint self);
SPINE_C_API spine_constraint_data spine_constraint_get_data(spine_constraint self);
SPINE_C_API void spine_constraint_sort(spine_constraint self, spine_skeleton skeleton);
SPINE_C_API bool spine_constraint_is_source_active(spine_constraint self);
/**
 * Inherited from Update
 */
SPINE_C_API void spine_constraint_update(spine_constraint self, spine_skeleton skeleton, spine_physics physics);
SPINE_C_API spine_rtti spine_constraint_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_CONSTRAINT_H */
