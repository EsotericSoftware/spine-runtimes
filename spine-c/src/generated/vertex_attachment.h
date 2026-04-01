#ifndef SPINE_SPINE_VERTEX_ATTACHMENT_H
#define SPINE_SPINE_VERTEX_ATTACHMENT_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API void spine_vertex_attachment_dispose(spine_vertex_attachment self);

SPINE_C_API spine_rtti spine_vertex_attachment_get_rtti(spine_vertex_attachment self);
/**
 * Transforms the attachment's local vertices to world coordinates. If
 * SlotPose::getDeform() is not empty, it is used to deform the vertices.
 *
 * See https://esotericsoftware.com/spine-runtime-skeletons#World-transforms
 * World transforms in the Spine Runtimes Guide.
 *
 * @param start The index of the first vertices value to transform. Each vertex has 2 values, x and y.
 * @param count The number of world vertex values to output. Must be < = WorldVerticesLength - start.
 * @param worldVertices The output world vertices. Must have a length >= offset + count * stride / 2.
 * @param offset The worldVertices index to begin writing values.
 * @param stride The number of worldVertices entries between the value pairs written.
 */
SPINE_C_API void spine_vertex_attachment_compute_world_vertices_1(spine_vertex_attachment self, spine_skeleton skeleton, spine_slot slot,
																  size_t start, size_t count, /*@null*/ float *worldVertices, size_t offset,
																  size_t stride);
SPINE_C_API void spine_vertex_attachment_compute_world_vertices_2(spine_vertex_attachment self, spine_skeleton skeleton, spine_slot slot,
																  size_t start, size_t count, spine_array_float worldVertices, size_t offset,
																  size_t stride);
/**
 * Gets a unique ID for this attachment.
 */
SPINE_C_API int spine_vertex_attachment_get_id(spine_vertex_attachment self);
/**
 * The bones that affect the vertices. The entries are, for each vertex, the
 * number of bones affecting the vertex followed by that many bone indices,
 * which is Skeleton::getBones() index. Empty if this attachment has no weights.
 */
SPINE_C_API spine_array_int spine_vertex_attachment_get_bones(spine_vertex_attachment self);
SPINE_C_API void spine_vertex_attachment_set_bones(spine_vertex_attachment self, spine_array_int bones);
/**
 * The vertex positions in the bone's coordinate system. For a non-weighted
 * attachment, the values are x,y pairs for each vertex. For a weighted
 * attachment, the values are x,y,weight triplets for each bone affecting each
 * vertex.
 */
SPINE_C_API spine_array_float spine_vertex_attachment_get_vertices(spine_vertex_attachment self);
SPINE_C_API void spine_vertex_attachment_set_vertices(spine_vertex_attachment self, spine_array_float vertices);
SPINE_C_API size_t spine_vertex_attachment_get_world_vertices_length(spine_vertex_attachment self);
SPINE_C_API void spine_vertex_attachment_set_world_vertices_length(spine_vertex_attachment self, size_t inValue);
SPINE_C_API /*@null*/ spine_attachment spine_vertex_attachment_get_timeline_attachment(spine_vertex_attachment self);
SPINE_C_API void spine_vertex_attachment_set_timeline_attachment(spine_vertex_attachment self, /*@null*/ spine_attachment attachment);
SPINE_C_API void spine_vertex_attachment_copy_to(spine_vertex_attachment self, spine_vertex_attachment other);
SPINE_C_API const char *spine_vertex_attachment_get_name(spine_vertex_attachment self);
SPINE_C_API spine_attachment spine_vertex_attachment_copy(spine_vertex_attachment self);
SPINE_C_API int spine_vertex_attachment_get_ref_count(spine_vertex_attachment self);
SPINE_C_API void spine_vertex_attachment_reference(spine_vertex_attachment self);
SPINE_C_API void spine_vertex_attachment_dereference(spine_vertex_attachment self);
SPINE_C_API spine_rtti spine_vertex_attachment_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_VERTEX_ATTACHMENT_H */
