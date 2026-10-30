#ifndef SPINE_SPINE_POSED_DATA_H
#define SPINE_SPINE_POSED_DATA_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_posed_data spine_posed_data_create(const char *name);

SPINE_C_API void spine_posed_data_dispose(spine_posed_data self);

SPINE_C_API const char *spine_posed_data_get_name(spine_posed_data self);
/**
 * When true, Skeleton::updateWorldTransform(Physics) only updates this
 * constraint if the Skeleton::getSkin() contains this constraint.
 *
 * See Skin::getConstraints().
 */
SPINE_C_API bool spine_posed_data_get_skin_required(spine_posed_data self);
SPINE_C_API void spine_posed_data_set_skin_required(spine_posed_data self, bool skinRequired);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_POSED_DATA_H */
