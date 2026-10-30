#ifndef SPINE_SPINE_POLYGON_H
#define SPINE_SPINE_POLYGON_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_polygon spine_polygon_create(void);

SPINE_C_API void spine_polygon_dispose(spine_polygon self);

SPINE_C_API spine_array_float spine_polygon_get__vertices(spine_polygon self);
SPINE_C_API void spine_polygon_set__vertices(spine_polygon self, spine_array_float value);
SPINE_C_API int spine_polygon_get__count(spine_polygon self);
SPINE_C_API void spine_polygon_set__count(spine_polygon self, int value);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_POLYGON_H */
