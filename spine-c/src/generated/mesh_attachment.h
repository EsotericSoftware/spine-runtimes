#ifndef SPINE_SPINE_MESH_ATTACHMENT_H
#define SPINE_SPINE_MESH_ATTACHMENT_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_mesh_attachment spine_mesh_attachment_create(const char *name, /*@null*/ spine_sequence sequence);

SPINE_C_API void spine_mesh_attachment_dispose(spine_mesh_attachment self);

SPINE_C_API spine_rtti spine_mesh_attachment_get_rtti(spine_mesh_attachment self);
SPINE_C_API void spine_mesh_attachment_compute_world_vertices_1(spine_mesh_attachment self, spine_skeleton skeleton, spine_slot slot, size_t start,
																size_t count, /*@null*/ float *worldVertices, size_t offset, size_t stride);
SPINE_C_API void spine_mesh_attachment_compute_world_vertices_2(spine_mesh_attachment self, spine_skeleton skeleton, spine_slot slot, size_t start,
																size_t count, spine_array_float worldVertices, size_t offset, size_t stride);
SPINE_C_API spine_array_float spine_mesh_attachment_get_region_u_vs(spine_mesh_attachment self);
SPINE_C_API void spine_mesh_attachment_set_region_u_vs(spine_mesh_attachment self, spine_array_float inValue);
SPINE_C_API spine_array_unsigned_short spine_mesh_attachment_get_triangles(spine_mesh_attachment self);
SPINE_C_API void spine_mesh_attachment_set_triangles(spine_mesh_attachment self, spine_array_unsigned_short inValue);
SPINE_C_API int spine_mesh_attachment_get_hull_length(spine_mesh_attachment self);
SPINE_C_API void spine_mesh_attachment_set_hull_length(spine_mesh_attachment self, int inValue);
SPINE_C_API spine_sequence spine_mesh_attachment_get_sequence(spine_mesh_attachment self);
SPINE_C_API void spine_mesh_attachment_update_sequence(spine_mesh_attachment self);
SPINE_C_API const char *spine_mesh_attachment_get_path(spine_mesh_attachment self);
SPINE_C_API void spine_mesh_attachment_set_path(spine_mesh_attachment self, const char *inValue);
SPINE_C_API spine_color spine_mesh_attachment_get_color(spine_mesh_attachment self);
SPINE_C_API /*@null*/ spine_mesh_attachment spine_mesh_attachment_get_parent_mesh(spine_mesh_attachment self);
SPINE_C_API void spine_mesh_attachment_set_parent_mesh(spine_mesh_attachment self, /*@null*/ spine_mesh_attachment inValue);
SPINE_C_API spine_array_unsigned_short spine_mesh_attachment_get_edges(spine_mesh_attachment self);
SPINE_C_API void spine_mesh_attachment_set_edges(spine_mesh_attachment self, spine_array_unsigned_short inValue);
SPINE_C_API float spine_mesh_attachment_get_width(spine_mesh_attachment self);
SPINE_C_API void spine_mesh_attachment_set_width(spine_mesh_attachment self, float inValue);
SPINE_C_API float spine_mesh_attachment_get_height(spine_mesh_attachment self);
SPINE_C_API void spine_mesh_attachment_set_height(spine_mesh_attachment self, float inValue);
SPINE_C_API spine_attachment spine_mesh_attachment_copy(spine_mesh_attachment self);
SPINE_C_API spine_mesh_attachment spine_mesh_attachment_new_linked_mesh(spine_mesh_attachment self);
/**
 * Computes UVs for a mesh attachment.
 *
 * @param uvs Output array for the computed UVs, same length as regionUVs.
 */
SPINE_C_API void spine_mesh_attachment_compute_u_vs(/*@null*/ spine_texture_region region, spine_array_float regionUVs, spine_array_float uvs);
/**
 * Gets a unique ID for this attachment.
 */
SPINE_C_API int spine_mesh_attachment_get_id(spine_mesh_attachment self);
SPINE_C_API spine_array_int spine_mesh_attachment_get_bones(spine_mesh_attachment self);
SPINE_C_API void spine_mesh_attachment_set_bones(spine_mesh_attachment self, spine_array_int bones);
SPINE_C_API spine_array_float spine_mesh_attachment_get_vertices(spine_mesh_attachment self);
SPINE_C_API void spine_mesh_attachment_set_vertices(spine_mesh_attachment self, spine_array_float vertices);
SPINE_C_API size_t spine_mesh_attachment_get_world_vertices_length(spine_mesh_attachment self);
SPINE_C_API void spine_mesh_attachment_set_world_vertices_length(spine_mesh_attachment self, size_t inValue);
SPINE_C_API /*@null*/ spine_attachment spine_mesh_attachment_get_timeline_attachment(spine_mesh_attachment self);
SPINE_C_API void spine_mesh_attachment_set_timeline_attachment(spine_mesh_attachment self, /*@null*/ spine_attachment attachment);
SPINE_C_API void spine_mesh_attachment_copy_to(spine_mesh_attachment self, spine_vertex_attachment other);
SPINE_C_API const char *spine_mesh_attachment_get_name(spine_mesh_attachment self);
SPINE_C_API int spine_mesh_attachment_get_ref_count(spine_mesh_attachment self);
SPINE_C_API void spine_mesh_attachment_reference(spine_mesh_attachment self);
SPINE_C_API void spine_mesh_attachment_dereference(spine_mesh_attachment self);
SPINE_C_API spine_rtti spine_mesh_attachment_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_MESH_ATTACHMENT_H */
