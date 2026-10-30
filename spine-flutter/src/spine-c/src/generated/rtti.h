#ifndef SPINE_SPINE_RTTI_H
#define SPINE_SPINE_RTTI_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API void spine_rtti_dispose(spine_rtti self);

SPINE_C_API /*@null*/ const char *spine_rtti_get_class_name(spine_rtti self);
SPINE_C_API bool spine_rtti_is_exactly(spine_rtti self, spine_rtti rtti);
SPINE_C_API bool spine_rtti_instance_of(spine_rtti self, spine_rtti rtti);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_RTTI_H */
