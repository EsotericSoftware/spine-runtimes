#ifndef SPINE_SPINE_INTERPOLATION_H
#define SPINE_SPINE_INTERPOLATION_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

/**
 *
 * @param a Alpha value between 0 and 1.
 */
SPINE_C_API float spine_interpolation_apply_1(spine_interpolation self, float a);
SPINE_C_API float spine_interpolation_apply_2(spine_interpolation self, float start, float end, float a);
SPINE_C_API spine_interpolation spine_interpolation_linear(void);
/**
 * Aka "smoothstep".
 */
SPINE_C_API spine_interpolation spine_interpolation_smooth(void);
/**
 * Slow, then fast.
 */
SPINE_C_API spine_interpolation spine_interpolation_slow_fast(void);
/**
 * Fast, then slow.
 */
SPINE_C_API spine_interpolation spine_interpolation_fast_slow(void);
SPINE_C_API spine_interpolation spine_interpolation_circle(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_INTERPOLATION_H */
