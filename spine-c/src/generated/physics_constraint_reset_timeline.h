#ifndef SPINE_SPINE_PHYSICS_CONSTRAINT_RESET_TIMELINE_H
#define SPINE_SPINE_PHYSICS_CONSTRAINT_RESET_TIMELINE_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

/**
 *
 * @param constraintIndex -1 for all physics constraints in the skeleton.
 */
SPINE_C_API spine_physics_constraint_reset_timeline spine_physics_constraint_reset_timeline_create(size_t frameCount, int constraintIndex);

SPINE_C_API void spine_physics_constraint_reset_timeline_dispose(spine_physics_constraint_reset_timeline self);

SPINE_C_API spine_rtti spine_physics_constraint_reset_timeline_get_rtti(spine_physics_constraint_reset_timeline self);
SPINE_C_API void spine_physics_constraint_reset_timeline_apply(spine_physics_constraint_reset_timeline self, spine_skeleton skeleton, float lastTime,
															   float time, /*@null*/ spine_array_event events, float alpha, spine_mix_from from,
															   bool add, bool out, bool appliedPose);
SPINE_C_API int spine_physics_constraint_reset_timeline_get_frame_count(spine_physics_constraint_reset_timeline self);
SPINE_C_API int spine_physics_constraint_reset_timeline_get_constraint_index(spine_physics_constraint_reset_timeline self);
SPINE_C_API void spine_physics_constraint_reset_timeline_set_constraint_index(spine_physics_constraint_reset_timeline self, int inValue);
/**
 * Sets the time for the specified frame.
 */
SPINE_C_API void spine_physics_constraint_reset_timeline_set_frame(spine_physics_constraint_reset_timeline self, int frame, float time);
/**
 * True if this timeline supports additive blending.
 */
SPINE_C_API bool spine_physics_constraint_reset_timeline_get_additive(spine_physics_constraint_reset_timeline self);
/**
 * True if this timeline sets values instantaneously and does not support
 * interpolation between frames.
 */
SPINE_C_API bool spine_physics_constraint_reset_timeline_get_instant(spine_physics_constraint_reset_timeline self);
SPINE_C_API size_t spine_physics_constraint_reset_timeline_get_frame_entries(spine_physics_constraint_reset_timeline self);
SPINE_C_API spine_array_float spine_physics_constraint_reset_timeline_get_frames(spine_physics_constraint_reset_timeline self);
SPINE_C_API float spine_physics_constraint_reset_timeline_get_duration(spine_physics_constraint_reset_timeline self);
SPINE_C_API spine_array_property_id spine_physics_constraint_reset_timeline_get_property_ids(spine_physics_constraint_reset_timeline self);
SPINE_C_API spine_rtti spine_physics_constraint_reset_timeline_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_PHYSICS_CONSTRAINT_RESET_TIMELINE_H */
