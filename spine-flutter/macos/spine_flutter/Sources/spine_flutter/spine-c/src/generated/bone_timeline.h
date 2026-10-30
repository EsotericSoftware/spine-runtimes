#ifndef SPINE_SPINE_BONE_TIMELINE_H
#define SPINE_SPINE_BONE_TIMELINE_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API void spine_bone_timeline_dispose(spine_bone_timeline self);

SPINE_C_API spine_rtti spine_bone_timeline_get_rtti(spine_bone_timeline self);
/**
 * The Skeleton::getBones() index of the bone that will be changed when this
 * timeline is applied.
 */
SPINE_C_API int spine_bone_timeline_get_bone_index(spine_bone_timeline self);
SPINE_C_API void spine_bone_timeline_set_bone_index(spine_bone_timeline self, int inValue);
SPINE_C_API spine_rtti spine_bone_timeline_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_BONE_TIMELINE_H */
