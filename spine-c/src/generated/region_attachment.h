#ifndef SPINE_SPINE_REGION_ATTACHMENT_H
#define SPINE_SPINE_REGION_ATTACHMENT_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_region_attachment spine_region_attachment_create(const char *name, /*@null*/ spine_sequence sequence);

SPINE_C_API void spine_region_attachment_dispose(spine_region_attachment self);

SPINE_C_API spine_rtti spine_region_attachment_get_rtti(spine_region_attachment self);
/**
 * Transforms the attachment's four vertices to world coordinates.
 *
 * @param slot The parent slot.
 * @param vertexOffsets The vertex offsets.
 * @param worldVertices The output world vertices. Must have a length greater than or equal to offset + 8.
 * @param offset The worldVertices index to begin writing values.
 * @param stride The number of worldVertices entries between the value pairs written.
 */
SPINE_C_API void spine_region_attachment_compute_world_vertices_1(spine_region_attachment self, spine_slot slot, /*@null*/ float *vertexOffsets,
																  /*@null*/ float *worldVertices, size_t offset, size_t stride);
SPINE_C_API void spine_region_attachment_compute_world_vertices_2(spine_region_attachment self, spine_slot slot, spine_array_float vertexOffsets,
																  spine_array_float worldVertices, size_t offset, size_t stride);
/**
 * Returns the vertex offsets for the specified slot pose.
 */
SPINE_C_API spine_array_float spine_region_attachment_get_offsets(spine_region_attachment self, spine_slot_pose pose);
SPINE_C_API float spine_region_attachment_get_x(spine_region_attachment self);
SPINE_C_API void spine_region_attachment_set_x(spine_region_attachment self, float inValue);
SPINE_C_API float spine_region_attachment_get_y(spine_region_attachment self);
SPINE_C_API void spine_region_attachment_set_y(spine_region_attachment self, float inValue);
SPINE_C_API float spine_region_attachment_get_scale_x(spine_region_attachment self);
SPINE_C_API void spine_region_attachment_set_scale_x(spine_region_attachment self, float inValue);
SPINE_C_API float spine_region_attachment_get_scale_y(spine_region_attachment self);
SPINE_C_API void spine_region_attachment_set_scale_y(spine_region_attachment self, float inValue);
/**
 * The local rotation in degrees, counter clockwise.
 */
SPINE_C_API float spine_region_attachment_get_rotation(spine_region_attachment self);
SPINE_C_API void spine_region_attachment_set_rotation(spine_region_attachment self, float inValue);
SPINE_C_API float spine_region_attachment_get_width(spine_region_attachment self);
SPINE_C_API void spine_region_attachment_set_width(spine_region_attachment self, float inValue);
SPINE_C_API float spine_region_attachment_get_height(spine_region_attachment self);
SPINE_C_API void spine_region_attachment_set_height(spine_region_attachment self, float inValue);
SPINE_C_API spine_sequence spine_region_attachment_get_sequence(spine_region_attachment self);
SPINE_C_API void spine_region_attachment_update_sequence(spine_region_attachment self);
SPINE_C_API const char *spine_region_attachment_get_path(spine_region_attachment self);
SPINE_C_API void spine_region_attachment_set_path(spine_region_attachment self, const char *inValue);
SPINE_C_API spine_color spine_region_attachment_get_color(spine_region_attachment self);
SPINE_C_API spine_attachment spine_region_attachment_copy(spine_region_attachment self);
/**
 * Computes UVs and offsets for a region attachment.
 *
 * @param uvs Output array for the computed UVs, length of 8.
 * @param offset Output array for the computed vertex offsets, length of 8.
 */
SPINE_C_API void spine_region_attachment_compute_u_vs(/*@null*/ spine_texture_region region, float x, float y, float scaleX, float scaleY,
													  float rotation, float width, float height, spine_array_float offset, spine_array_float uvs);
SPINE_C_API const char *spine_region_attachment_get_name(spine_region_attachment self);
SPINE_C_API /*@null*/ spine_attachment spine_region_attachment_get_timeline_attachment(spine_region_attachment self);
SPINE_C_API void spine_region_attachment_set_timeline_attachment(spine_region_attachment self, /*@null*/ spine_attachment attachment);
SPINE_C_API spine_array_int spine_region_attachment_get_timeline_slots(spine_region_attachment self);
SPINE_C_API void spine_region_attachment_set_timeline_slots(spine_region_attachment self, spine_array_int timelineSlots);
SPINE_C_API bool spine_region_attachment_is_timeline_active(spine_region_attachment self, spine_array_slot slots, int slotIndex, bool appliedPose);
SPINE_C_API int spine_region_attachment_get_ref_count(spine_region_attachment self);
SPINE_C_API void spine_region_attachment_reference(spine_region_attachment self);
SPINE_C_API void spine_region_attachment_dereference(spine_region_attachment self);
SPINE_C_API spine_rtti spine_region_attachment_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_REGION_ATTACHMENT_H */
