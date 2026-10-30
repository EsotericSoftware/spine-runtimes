#ifndef SPINE_SPINE_CONSTRAINT_DATA_H
#define SPINE_SPINE_CONSTRAINT_DATA_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API void spine_constraint_data_dispose(spine_constraint_data self);

SPINE_C_API spine_rtti spine_constraint_data_get_rtti(spine_constraint_data self);
SPINE_C_API spine_constraint spine_constraint_data_create_method(spine_constraint_data self, spine_skeleton skeleton);
SPINE_C_API const char *spine_constraint_data_get_name(spine_constraint_data self);
SPINE_C_API bool spine_constraint_data_get_skin_required(spine_constraint_data self);
SPINE_C_API spine_rtti spine_constraint_data_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_CONSTRAINT_DATA_H */
