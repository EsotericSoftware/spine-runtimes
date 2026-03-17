#ifndef SPINE_SPINE_SEQUENCE_H
#define SPINE_SPINE_SEQUENCE_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_sequence spine_sequence_create(int count, bool pathSuffix);
/**
 * Copy constructor.
 */
SPINE_C_API spine_sequence spine_sequence_create2(spine_sequence other);

SPINE_C_API void spine_sequence_dispose(spine_sequence self);

/**
 * Computes UVs and offsets for the specified attachment. Must be called if the
 * regions or attachment properties are changed.
 */
SPINE_C_API void spine_sequence_update_1(spine_sequence self, spine_region_attachment attachment);
SPINE_C_API void spine_sequence_update_2(spine_sequence self, spine_mesh_attachment attachment);
SPINE_C_API spine_array_texture_region spine_sequence_get_regions(spine_sequence self);
SPINE_C_API int spine_sequence_resolve_index(spine_sequence self, spine_slot_pose pose);
SPINE_C_API /*@null*/ spine_texture_region spine_sequence_get_region(spine_sequence self, int index);
SPINE_C_API spine_array_float spine_sequence_get_u_vs(spine_sequence self, int index);
/**
 * Returns vertex offsets from the center of a RegionAttachment. Invalid to call
 * for a MeshAttachment.
 */
SPINE_C_API spine_array_float spine_sequence_get_offsets(spine_sequence self, int index);
SPINE_C_API int spine_sequence_get_start(spine_sequence self);
SPINE_C_API void spine_sequence_set_start(spine_sequence self, int start);
SPINE_C_API int spine_sequence_get_digits(spine_sequence self);
SPINE_C_API void spine_sequence_set_digits(spine_sequence self, int digits);
/**
 * The index of the region to show for the setup pose.
 */
SPINE_C_API int spine_sequence_get_setup_index(spine_sequence self);
SPINE_C_API void spine_sequence_set_setup_index(spine_sequence self, int setupIndex);
SPINE_C_API bool spine_sequence_has_path_suffix(spine_sequence self);
SPINE_C_API const char *spine_sequence_get_path(spine_sequence self, const char *basePath, int index);
/**
 * Returns a unique ID for this attachment.
 */
SPINE_C_API int spine_sequence_get_id(spine_sequence self);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_SEQUENCE_H */
