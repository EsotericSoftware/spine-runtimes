#ifndef SPINE_SPINE_PHYSICS_CONSTRAINT_POSE_H
#define SPINE_SPINE_PHYSICS_CONSTRAINT_POSE_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_physics_constraint_pose spine_physics_constraint_pose_create(void);

SPINE_C_API void spine_physics_constraint_pose_dispose(spine_physics_constraint_pose self);

SPINE_C_API void spine_physics_constraint_pose_set(spine_physics_constraint_pose self, spine_physics_constraint_pose pose);
/**
 * Controls how much bone movement is converted into physics movement.
 */
SPINE_C_API float spine_physics_constraint_pose_get_inertia(spine_physics_constraint_pose self);
SPINE_C_API void spine_physics_constraint_pose_set_inertia(spine_physics_constraint_pose self, float inertia);
/**
 * The amount of force used to return properties to the unconstrained value.
 */
SPINE_C_API float spine_physics_constraint_pose_get_strength(spine_physics_constraint_pose self);
SPINE_C_API void spine_physics_constraint_pose_set_strength(spine_physics_constraint_pose self, float strength);
/**
 * Reduces the speed of physics movements, with more of a reduction at higher
 * speeds.
 */
SPINE_C_API float spine_physics_constraint_pose_get_damping(spine_physics_constraint_pose self);
SPINE_C_API void spine_physics_constraint_pose_set_damping(spine_physics_constraint_pose self, float damping);
/**
 * Determines susceptibility to acceleration.
 */
SPINE_C_API float spine_physics_constraint_pose_get_mass_inverse(spine_physics_constraint_pose self);
SPINE_C_API void spine_physics_constraint_pose_set_mass_inverse(spine_physics_constraint_pose self, float massInverse);
/**
 * Applies a constant force along the Skeleton::getWindX(), Skeleton::getWindY()
 * vector.
 */
SPINE_C_API float spine_physics_constraint_pose_get_wind(spine_physics_constraint_pose self);
SPINE_C_API void spine_physics_constraint_pose_set_wind(spine_physics_constraint_pose self, float wind);
/**
 * Applies a constant force along the Skeleton::getGravityX(),
 * Skeleton::getGravityY() vector.
 */
SPINE_C_API float spine_physics_constraint_pose_get_gravity(spine_physics_constraint_pose self);
SPINE_C_API void spine_physics_constraint_pose_set_gravity(spine_physics_constraint_pose self, float gravity);
/**
 * A percentage (0+) that controls the mix between the constrained and
 * unconstrained poses.
 */
SPINE_C_API float spine_physics_constraint_pose_get_mix(spine_physics_constraint_pose self);
SPINE_C_API void spine_physics_constraint_pose_set_mix(spine_physics_constraint_pose self, float mix);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_PHYSICS_CONSTRAINT_POSE_H */
