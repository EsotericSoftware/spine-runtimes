#ifndef SPINE_SPINE_COLOR_H
#define SPINE_SPINE_COLOR_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_color spine_color_create(void);
SPINE_C_API spine_color spine_color_create2(float r, float g, float b, float a);

SPINE_C_API void spine_color_dispose(spine_color self);

SPINE_C_API spine_color spine_color_set_1(spine_color self, float _r, float _g, float _b, float _a);
SPINE_C_API spine_color spine_color_set_2(spine_color self, float _r, float _g, float _b);
SPINE_C_API spine_color spine_color_set_3(spine_color self, spine_color other);
SPINE_C_API spine_color spine_color_add_1(spine_color self, float _r, float _g, float _b, float _a);
SPINE_C_API spine_color spine_color_add_2(spine_color self, float _r, float _g, float _b);
SPINE_C_API spine_color spine_color_add_3(spine_color self, spine_color other);
SPINE_C_API spine_color spine_color_clamp(spine_color self);
SPINE_C_API float spine_color_parse_hex(/*@null*/ const char *value, size_t index);
/**
 * Convert packed RGBA8888 integer to Color
 */
SPINE_C_API void spine_color_rgba8888_to_color(spine_color color, int value);
/**
 * Convert packed RGB888 integer to Color (no alpha)
 */
SPINE_C_API void spine_color_rgb888_to_color(spine_color color, int value);
SPINE_C_API float spine_color_get_r(spine_color self);
SPINE_C_API void spine_color_set_r(spine_color self, float value);
SPINE_C_API float spine_color_get_g(spine_color self);
SPINE_C_API void spine_color_set_g(spine_color self, float value);
SPINE_C_API float spine_color_get_b(spine_color self);
SPINE_C_API void spine_color_set_b(spine_color self, float value);
SPINE_C_API float spine_color_get_a(spine_color self);
SPINE_C_API void spine_color_set_a(spine_color self, float value);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_COLOR_H */
