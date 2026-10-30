#ifndef SPINE_SPINE_SLOT_H
#define SPINE_SPINE_SLOT_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_slot spine_slot_create(spine_slot_data data, spine_skeleton skeleton);

SPINE_C_API void spine_slot_dispose(spine_slot self);

/**
 * The bone this slot belongs to.
 */
SPINE_C_API spine_bone spine_slot_get_bone(spine_slot self);
SPINE_C_API void spine_slot_setup_pose(spine_slot self);
/**
 * The setup pose data. May be shared with multiple instances.
 */
SPINE_C_API spine_slot_data spine_slot_get_data(spine_slot self);
/**
 * The unconstrained pose for this object, set by animations and application
 * code.
 */
SPINE_C_API spine_slot_pose spine_slot_get_pose(spine_slot self);
/**
 * The pose to use for rendering. If no constraints modify this pose, this is
 * the same as getPose(). Otherwise it is a copy of getPose() modified by
 * constraints.
 */
SPINE_C_API spine_slot_pose spine_slot_get_applied_pose(spine_slot self);
/**
 * Sets the constrained pose to the unconstrained pose, as a starting point for
 * constraints to be applied.
 */
SPINE_C_API void spine_slot_reset_constrained(spine_slot self);
/**
 * Sets the applied pose to the constrained pose, in anticipation of the applied
 * pose being modified by constraints.
 */
SPINE_C_API void spine_slot_constrained(spine_slot self);
SPINE_C_API bool spine_slot_is_pose_equal_to_applied(spine_slot self);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_SLOT_H */
