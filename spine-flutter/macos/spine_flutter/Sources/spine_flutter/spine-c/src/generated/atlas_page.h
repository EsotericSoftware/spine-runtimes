#ifndef SPINE_SPINE_ATLAS_PAGE_H
#define SPINE_SPINE_ATLAS_PAGE_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_atlas_page spine_atlas_page_create(const char *inName);

SPINE_C_API void spine_atlas_page_dispose(spine_atlas_page self);

SPINE_C_API const char *spine_atlas_page_get_name(spine_atlas_page self);
SPINE_C_API void spine_atlas_page_set_name(spine_atlas_page self, const char *value);
SPINE_C_API const char *spine_atlas_page_get_texture_path(spine_atlas_page self);
SPINE_C_API void spine_atlas_page_set_texture_path(spine_atlas_page self, const char *value);
SPINE_C_API spine_format spine_atlas_page_get_format(spine_atlas_page self);
SPINE_C_API void spine_atlas_page_set_format(spine_atlas_page self, spine_format value);
SPINE_C_API spine_texture_filter spine_atlas_page_get_min_filter(spine_atlas_page self);
SPINE_C_API void spine_atlas_page_set_min_filter(spine_atlas_page self, spine_texture_filter value);
SPINE_C_API spine_texture_filter spine_atlas_page_get_mag_filter(spine_atlas_page self);
SPINE_C_API void spine_atlas_page_set_mag_filter(spine_atlas_page self, spine_texture_filter value);
SPINE_C_API spine_texture_wrap spine_atlas_page_get_u_wrap(spine_atlas_page self);
SPINE_C_API void spine_atlas_page_set_u_wrap(spine_atlas_page self, spine_texture_wrap value);
SPINE_C_API spine_texture_wrap spine_atlas_page_get_v_wrap(spine_atlas_page self);
SPINE_C_API void spine_atlas_page_set_v_wrap(spine_atlas_page self, spine_texture_wrap value);
SPINE_C_API int spine_atlas_page_get_width(spine_atlas_page self);
SPINE_C_API void spine_atlas_page_set_width(spine_atlas_page self, int value);
SPINE_C_API int spine_atlas_page_get_height(spine_atlas_page self);
SPINE_C_API void spine_atlas_page_set_height(spine_atlas_page self, int value);
SPINE_C_API bool spine_atlas_page_get_pma(spine_atlas_page self);
SPINE_C_API void spine_atlas_page_set_pma(spine_atlas_page self, bool value);
SPINE_C_API int spine_atlas_page_get_index(spine_atlas_page self);
SPINE_C_API void spine_atlas_page_set_index(spine_atlas_page self, int value);
SPINE_C_API /*@null*/ void *spine_atlas_page_get_texture(spine_atlas_page self);
SPINE_C_API void spine_atlas_page_set_texture(spine_atlas_page self, /*@null*/ void *value);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_ATLAS_PAGE_H */
