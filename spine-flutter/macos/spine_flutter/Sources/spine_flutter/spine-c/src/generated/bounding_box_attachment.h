#ifndef SPINE_SPINE_BOUNDING_BOX_ATTACHMENT_H
#define SPINE_SPINE_BOUNDING_BOX_ATTACHMENT_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_bounding_box_attachment spine_bounding_box_attachment_create(const char *name);

SPINE_C_API void spine_bounding_box_attachment_dispose(spine_bounding_box_attachment self);

SPINE_C_API spine_rtti spine_bounding_box_attachment_get_rtti(spine_bounding_box_attachment self);
SPINE_C_API spine_color spine_bounding_box_attachment_get_color(spine_bounding_box_attachment self);
SPINE_C_API spine_attachment spine_bounding_box_attachment_copy(spine_bounding_box_attachment self);
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
SPINE_C_API void spine_bounding_box_attachment_compute_world_vertices_1(spine_bounding_box_attachment self, spine_skeleton skeleton, spine_slot slot,
																		size_t start, size_t count, /*@null*/ float *worldVertices, size_t offset,
																		size_t stride);
SPINE_C_API void spine_bounding_box_attachment_compute_world_vertices_2(spine_bounding_box_attachment self, spine_skeleton skeleton, spine_slot slot,
																		size_t start, size_t count, spine_array_float worldVertices, size_t offset,
																		size_t stride);
/**
 * Gets a unique ID for this attachment.
 */
SPINE_C_API int spine_bounding_box_attachment_get_id(spine_bounding_box_attachment self);
/**
 * The bones that affect the vertices. The entries are, for each vertex, the
 * number of bones affecting the vertex followed by that many bone indices,
 * which is Skeleton::getBones() index. Empty if this attachment has no weights.
 */
SPINE_C_API spine_array_int spine_bounding_box_attachment_get_bones(spine_bounding_box_attachment self);
SPINE_C_API void spine_bounding_box_attachment_set_bones(spine_bounding_box_attachment self, spine_array_int bones);
/**
 * The vertex positions in the bone's coordinate system. For a non-weighted
 * attachment, the values are x,y pairs for each vertex. For a weighted
 * attachment, the values are x,y,weight triplets for each bone affecting each
 * vertex.
 */
SPINE_C_API spine_array_float spine_bounding_box_attachment_get_vertices(spine_bounding_box_attachment self);
SPINE_C_API void spine_bounding_box_attachment_set_vertices(spine_bounding_box_attachment self, spine_array_float vertices);
SPINE_C_API size_t spine_bounding_box_attachment_get_world_vertices_length(spine_bounding_box_attachment self);
SPINE_C_API void spine_bounding_box_attachment_set_world_vertices_length(spine_bounding_box_attachment self, size_t inValue);
SPINE_C_API /*@null*/ spine_attachment spine_bounding_box_attachment_get_timeline_attachment(spine_bounding_box_attachment self);
SPINE_C_API void spine_bounding_box_attachment_set_timeline_attachment(spine_bounding_box_attachment self, /*@null*/ spine_attachment attachment);
SPINE_C_API void spine_bounding_box_attachment_copy_to(spine_bounding_box_attachment self, spine_vertex_attachment other);
SPINE_C_API const char *spine_bounding_box_attachment_get_name(spine_bounding_box_attachment self);
SPINE_C_API spine_array_int spine_bounding_box_attachment_get_timeline_slots(spine_bounding_box_attachment self);
SPINE_C_API void spine_bounding_box_attachment_set_timeline_slots(spine_bounding_box_attachment self, spine_array_int timelineSlots);
/**
 * Returns true if the slotIndex or any getTimelineSlots() have an attachment
 * whose getTimelineAttachment() is this attachment.
 *
 * @param slots The Skeleton::getSlots().
 * @param slotIndex The timeline's primary slot index.
 */
SPINE_C_API bool spine_bounding_box_attachment_is_timeline_active(spine_bounding_box_attachment self, spine_array_slot slots, int slotIndex,
																  bool appliedPose);
SPINE_C_API int spine_bounding_box_attachment_get_ref_count(spine_bounding_box_attachment self);
SPINE_C_API void spine_bounding_box_attachment_reference(spine_bounding_box_attachment self);
SPINE_C_API void spine_bounding_box_attachment_dereference(spine_bounding_box_attachment self);
SPINE_C_API spine_rtti spine_bounding_box_attachment_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_BOUNDING_BOX_ATTACHMENT_H */
