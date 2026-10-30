#ifndef SPINE_SPINE_UPDATE_H
#define SPINE_SPINE_UPDATE_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API void spine_update_dispose(spine_update self);

SPINE_C_API spine_rtti spine_update_get_rtti(spine_update self);
/**
 *
 * @param physics Determines how physics and other non-deterministic updates are applied.
 */
SPINE_C_API void spine_update_update(spine_update self, spine_skeleton skeleton, spine_physics physics);
SPINE_C_API spine_rtti spine_update_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_UPDATE_H */
