#ifndef SPINE_SPINE_RENDER_COMMAND_H
#define SPINE_SPINE_RENDER_COMMAND_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API void spine_render_command_dispose(spine_render_command self);

SPINE_C_API /*@null*/ float *spine_render_command_get_positions(spine_render_command self);
SPINE_C_API /*@null*/ float *spine_render_command_get_uvs(spine_render_command self);
SPINE_C_API /*@null*/ uint32_t *spine_render_command_get_colors(spine_render_command self);
SPINE_C_API /*@null*/ uint32_t *spine_render_command_get_dark_colors(spine_render_command self);
SPINE_C_API int32_t spine_render_command_get_num_vertices(spine_render_command self);
SPINE_C_API /*@null*/ uint16_t *spine_render_command_get_indices(spine_render_command self);
SPINE_C_API int32_t spine_render_command_get_num_indices(spine_render_command self);
SPINE_C_API spine_blend_mode spine_render_command_get_blend_mode(spine_render_command self);
SPINE_C_API /*@null*/ void *spine_render_command_get_texture(spine_render_command self);
SPINE_C_API /*@null*/ spine_render_command spine_render_command_get_next(spine_render_command self);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_RENDER_COMMAND_H */
