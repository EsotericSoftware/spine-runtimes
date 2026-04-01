#ifndef SPINE_SPINE_CONSTRAINT_TIMELINE_H
#define SPINE_SPINE_CONSTRAINT_TIMELINE_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API void spine_constraint_timeline_dispose(spine_constraint_timeline self);

SPINE_C_API spine_rtti spine_constraint_timeline_get_rtti(spine_constraint_timeline self);
/**
 * The Skeleton::getConstraints() index of the constraint that will be changed
 * when this timeline is applied.
 */
SPINE_C_API int spine_constraint_timeline_get_constraint_index(spine_constraint_timeline self);
SPINE_C_API void spine_constraint_timeline_set_constraint_index(spine_constraint_timeline self, int inValue);
SPINE_C_API spine_rtti spine_constraint_timeline_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_CONSTRAINT_TIMELINE_H */
