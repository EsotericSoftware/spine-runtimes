#ifndef SPINE_SPINE_ATLAS_REGION_H
#define SPINE_SPINE_ATLAS_REGION_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_atlas_region spine_atlas_region_create(void);

SPINE_C_API void spine_atlas_region_dispose(spine_atlas_region self);

SPINE_C_API spine_rtti spine_atlas_region_get_rtti(spine_atlas_region self);
SPINE_C_API /*@null*/ spine_atlas_page spine_atlas_region_get_page(spine_atlas_region self);
SPINE_C_API const char *spine_atlas_region_get_name(spine_atlas_region self);
SPINE_C_API int spine_atlas_region_get_index(spine_atlas_region self);
SPINE_C_API int spine_atlas_region_get_x(spine_atlas_region self);
SPINE_C_API int spine_atlas_region_get_y(spine_atlas_region self);
SPINE_C_API float spine_atlas_region_get_offset_x(spine_atlas_region self);
SPINE_C_API float spine_atlas_region_get_offset_y(spine_atlas_region self);
SPINE_C_API int spine_atlas_region_get_packed_width(spine_atlas_region self);
SPINE_C_API int spine_atlas_region_get_packed_height(spine_atlas_region self);
SPINE_C_API int spine_atlas_region_get_original_width(spine_atlas_region self);
SPINE_C_API int spine_atlas_region_get_original_height(spine_atlas_region self);
SPINE_C_API bool spine_atlas_region_get_rotate(spine_atlas_region self);
SPINE_C_API int spine_atlas_region_get_degrees(spine_atlas_region self);
SPINE_C_API spine_array_int spine_atlas_region_get_splits(spine_atlas_region self);
SPINE_C_API spine_array_int spine_atlas_region_get_pads(spine_atlas_region self);
SPINE_C_API spine_array_float spine_atlas_region_get_values(spine_atlas_region self);
SPINE_C_API void spine_atlas_region_set_page(spine_atlas_region self, /*@null*/ spine_atlas_page value);
SPINE_C_API void spine_atlas_region_set_name(spine_atlas_region self, const char *value);
SPINE_C_API void spine_atlas_region_set_index(spine_atlas_region self, int value);
SPINE_C_API void spine_atlas_region_set_x(spine_atlas_region self, int value);
SPINE_C_API void spine_atlas_region_set_y(spine_atlas_region self, int value);
SPINE_C_API void spine_atlas_region_set_offset_x(spine_atlas_region self, float value);
SPINE_C_API void spine_atlas_region_set_offset_y(spine_atlas_region self, float value);
SPINE_C_API void spine_atlas_region_set_packed_width(spine_atlas_region self, int value);
SPINE_C_API void spine_atlas_region_set_packed_height(spine_atlas_region self, int value);
SPINE_C_API void spine_atlas_region_set_original_width(spine_atlas_region self, int value);
SPINE_C_API void spine_atlas_region_set_original_height(spine_atlas_region self, int value);
SPINE_C_API void spine_atlas_region_set_rotate(spine_atlas_region self, bool value);
SPINE_C_API void spine_atlas_region_set_degrees(spine_atlas_region self, int value);
SPINE_C_API void spine_atlas_region_set_splits(spine_atlas_region self, spine_array_int value);
SPINE_C_API void spine_atlas_region_set_pads(spine_atlas_region self, spine_array_int value);
SPINE_C_API void spine_atlas_region_set_values(spine_atlas_region self, spine_array_float value);
SPINE_C_API float spine_atlas_region_get_u(spine_atlas_region self);
SPINE_C_API void spine_atlas_region_set_u(spine_atlas_region self, float value);
SPINE_C_API float spine_atlas_region_get_v(spine_atlas_region self);
SPINE_C_API void spine_atlas_region_set_v(spine_atlas_region self, float value);
SPINE_C_API float spine_atlas_region_get_u2(spine_atlas_region self);
SPINE_C_API void spine_atlas_region_set_u2(spine_atlas_region self, float value);
SPINE_C_API float spine_atlas_region_get_v2(spine_atlas_region self);
SPINE_C_API void spine_atlas_region_set_v2(spine_atlas_region self, float value);
SPINE_C_API int spine_atlas_region_get_region_width(spine_atlas_region self);
SPINE_C_API void spine_atlas_region_set_region_width(spine_atlas_region self, int value);
SPINE_C_API int spine_atlas_region_get_region_height(spine_atlas_region self);
SPINE_C_API void spine_atlas_region_set_region_height(spine_atlas_region self, int value);
SPINE_C_API /*@null*/ void *spine_atlas_region_get_renderer_object(spine_atlas_region self);
SPINE_C_API void spine_atlas_region_set_renderer_object(spine_atlas_region self, /*@null*/ void *value);
SPINE_C_API spine_rtti spine_atlas_region_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_ATLAS_REGION_H */
