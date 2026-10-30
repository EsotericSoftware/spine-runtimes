#ifndef SPINE_SPINE_SKELETON_RENDERER_H
#define SPINE_SPINE_SKELETON_RENDERER_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_skeleton_renderer spine_skeleton_renderer_create(void);

SPINE_C_API void spine_skeleton_renderer_dispose(spine_skeleton_renderer self);

SPINE_C_API /*@null*/ spine_render_command spine_skeleton_renderer_render(spine_skeleton_renderer self, spine_skeleton skeleton);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_SKELETON_RENDERER_H */
