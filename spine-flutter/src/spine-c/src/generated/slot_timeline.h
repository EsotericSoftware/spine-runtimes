#ifndef SPINE_SPINE_SLOT_TIMELINE_H
#define SPINE_SPINE_SLOT_TIMELINE_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API void spine_slot_timeline_dispose(spine_slot_timeline self);

SPINE_C_API spine_rtti spine_slot_timeline_get_rtti(spine_slot_timeline self);
/**
 * The Skeleton::getSlots() index of the slot that will be changed when this
 * timeline is applied.
 */
SPINE_C_API int spine_slot_timeline_get_slot_index(spine_slot_timeline self);
SPINE_C_API void spine_slot_timeline_set_slot_index(spine_slot_timeline self, int inValue);
SPINE_C_API spine_rtti spine_slot_timeline_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_SLOT_TIMELINE_H */
