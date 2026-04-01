#ifndef SPINE_SPINE_ANIMATION_STATE_DATA_H
#define SPINE_SPINE_ANIMATION_STATE_DATA_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_animation_state_data spine_animation_state_data_create(spine_skeleton_data skeletonData);

SPINE_C_API void spine_animation_state_data_dispose(spine_animation_state_data self);

/**
 * The SkeletonData to look up animations when they are specified by name.
 */
SPINE_C_API spine_skeleton_data spine_animation_state_data_get_skeleton_data(spine_animation_state_data self);
/**
 * The mix duration to use when no mix duration has been specifically defined
 * between two animations.
 */
SPINE_C_API float spine_animation_state_data_get_default_mix(spine_animation_state_data self);
SPINE_C_API void spine_animation_state_data_set_default_mix(spine_animation_state_data self, float inValue);
/**
 * Sets a mix duration by animation names.
 */
SPINE_C_API void spine_animation_state_data_set_mix_1(spine_animation_state_data self, const char *fromName, const char *toName, float duration);
/**
 * Sets a mix duration when changing from the specified animation to the other.
 * See TrackEntry.MixDuration.
 */
SPINE_C_API void spine_animation_state_data_set_mix_2(spine_animation_state_data self, spine_animation from, spine_animation to, float duration);
/**
 * Returns the mix duration to use when changing from the specified animation to
 * the other on the same track, or the default mix if no mix duration has been
 * set.
 */
SPINE_C_API float spine_animation_state_data_get_mix(spine_animation_state_data self, spine_animation from, spine_animation to);
/**
 * Removes all mixes and sets the default mix to 0.
 */
SPINE_C_API void spine_animation_state_data_clear(spine_animation_state_data self);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_ANIMATION_STATE_DATA_H */
