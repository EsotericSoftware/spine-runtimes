#ifndef SPINE_SPINE_SKELETON_CLIPPING_H
#define SPINE_SPINE_SKELETON_CLIPPING_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_skeleton_clipping spine_skeleton_clipping_create(void);

SPINE_C_API void spine_skeleton_clipping_dispose(spine_skeleton_clipping self);

SPINE_C_API size_t spine_skeleton_clipping_clip_start(spine_skeleton_clipping self, spine_skeleton skeleton, spine_slot slot,
													  /*@null*/ spine_clipping_attachment clip);
SPINE_C_API void spine_skeleton_clipping_clip_end_1(spine_skeleton_clipping self, spine_slot slot);
SPINE_C_API void spine_skeleton_clipping_clip_end_2(spine_skeleton_clipping self);
SPINE_C_API bool spine_skeleton_clipping_clip_triangles_1(spine_skeleton_clipping self, /*@null*/ float *vertices,
														  /*@null*/ unsigned short *triangles, size_t trianglesLength);
SPINE_C_API bool spine_skeleton_clipping_clip_triangles_2(spine_skeleton_clipping self, /*@null*/ float *vertices,
														  /*@null*/ unsigned short *triangles, size_t trianglesLength, /*@null*/ float *uvs,
														  size_t stride);
SPINE_C_API bool spine_skeleton_clipping_clip_triangles_3(spine_skeleton_clipping self, spine_array_float vertices,
														  spine_array_unsigned_short triangles, spine_array_float uvs, size_t stride);
SPINE_C_API bool spine_skeleton_clipping_is_clipping(spine_skeleton_clipping self);
SPINE_C_API spine_array_float spine_skeleton_clipping_get_clipped_vertices(spine_skeleton_clipping self);
SPINE_C_API spine_array_unsigned_short spine_skeleton_clipping_get_clipped_triangles(spine_skeleton_clipping self);
SPINE_C_API spine_array_float spine_skeleton_clipping_get_clipped_u_vs(spine_skeleton_clipping self);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_SKELETON_CLIPPING_H */
