#ifndef SPINE_SPINE_PHYSICS_CONSTRAINT_DATA_H
#define SPINE_SPINE_PHYSICS_CONSTRAINT_DATA_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_physics_constraint_data spine_physics_constraint_data_create(const char *name);

SPINE_C_API void spine_physics_constraint_data_dispose(spine_physics_constraint_data self);

SPINE_C_API spine_rtti spine_physics_constraint_data_get_rtti(spine_physics_constraint_data self);
SPINE_C_API spine_constraint spine_physics_constraint_data_create_method(spine_physics_constraint_data self, spine_skeleton skeleton);
/**
 * The bone constrained by this physics constraint.
 */
SPINE_C_API spine_bone_data spine_physics_constraint_data_get_bone(spine_physics_constraint_data self);
SPINE_C_API void spine_physics_constraint_data_set_bone(spine_physics_constraint_data self, spine_bone_data bone);
/**
 * The time in milliseconds required to advanced the physics simulation one
 * step.
 */
SPINE_C_API float spine_physics_constraint_data_get_step(spine_physics_constraint_data self);
SPINE_C_API void spine_physics_constraint_data_set_step(spine_physics_constraint_data self, float step);
/**
 * Physics influence on x translation, 0-1.
 */
SPINE_C_API float spine_physics_constraint_data_get_x(spine_physics_constraint_data self);
SPINE_C_API void spine_physics_constraint_data_set_x(spine_physics_constraint_data self, float x);
/**
 * Physics influence on y translation, 0-1.
 */
SPINE_C_API float spine_physics_constraint_data_get_y(spine_physics_constraint_data self);
SPINE_C_API void spine_physics_constraint_data_set_y(spine_physics_constraint_data self, float y);
/**
 * Physics influence on rotation, 0-1.
 */
SPINE_C_API float spine_physics_constraint_data_get_rotate(spine_physics_constraint_data self);
SPINE_C_API void spine_physics_constraint_data_set_rotate(spine_physics_constraint_data self, float rotate);
/**
 * Physics influence on scaleX, 0-1.
 */
SPINE_C_API float spine_physics_constraint_data_get_scale_x(spine_physics_constraint_data self);
SPINE_C_API void spine_physics_constraint_data_set_scale_x(spine_physics_constraint_data self, float scaleX);
/**
 * Physics influence on shearX, 0-1.
 */
SPINE_C_API float spine_physics_constraint_data_get_shear_x(spine_physics_constraint_data self);
SPINE_C_API void spine_physics_constraint_data_set_shear_x(spine_physics_constraint_data self, float shearX);
/**
 * Movement greater than the limit will not have a greater affect on physics.
 */
SPINE_C_API float spine_physics_constraint_data_get_limit(spine_physics_constraint_data self);
SPINE_C_API void spine_physics_constraint_data_set_limit(spine_physics_constraint_data self, float limit);
/**
 * Determines how BonePose::getScaleY() changes when getScaleX() sets
 * BonePose::getScaleX().
 */
SPINE_C_API spine_scale_y_mode spine_physics_constraint_data_get_scale_y_mode(spine_physics_constraint_data self);
SPINE_C_API void spine_physics_constraint_data_set_scale_y_mode(spine_physics_constraint_data self, spine_scale_y_mode scaleYMode);
/**
 * True when this constraint's inertia is controlled by global slider timelines.
 */
SPINE_C_API bool spine_physics_constraint_data_get_inertia_global(spine_physics_constraint_data self);
SPINE_C_API void spine_physics_constraint_data_set_inertia_global(spine_physics_constraint_data self, bool inertiaGlobal);
/**
 * True when this constraint's strength is controlled by global slider
 * timelines.
 */
SPINE_C_API bool spine_physics_constraint_data_get_strength_global(spine_physics_constraint_data self);
SPINE_C_API void spine_physics_constraint_data_set_strength_global(spine_physics_constraint_data self, bool strengthGlobal);
/**
 * True when this constraint's damping is controlled by global slider timelines.
 */
SPINE_C_API bool spine_physics_constraint_data_get_damping_global(spine_physics_constraint_data self);
SPINE_C_API void spine_physics_constraint_data_set_damping_global(spine_physics_constraint_data self, bool dampingGlobal);
/**
 * True when this constraint's mass is controlled by global slider timelines.
 */
SPINE_C_API bool spine_physics_constraint_data_get_mass_global(spine_physics_constraint_data self);
SPINE_C_API void spine_physics_constraint_data_set_mass_global(spine_physics_constraint_data self, bool massGlobal);
/**
 * True when this constraint's wind is controlled by global slider timelines.
 */
SPINE_C_API bool spine_physics_constraint_data_get_wind_global(spine_physics_constraint_data self);
SPINE_C_API void spine_physics_constraint_data_set_wind_global(spine_physics_constraint_data self, bool windGlobal);
/**
 * True when this constraint's gravity is controlled by global slider timelines.
 */
SPINE_C_API bool spine_physics_constraint_data_get_gravity_global(spine_physics_constraint_data self);
SPINE_C_API void spine_physics_constraint_data_set_gravity_global(spine_physics_constraint_data self, bool gravityGlobal);
/**
 * True when this constraint's mix is controlled by global slider timelines.
 */
SPINE_C_API bool spine_physics_constraint_data_get_mix_global(spine_physics_constraint_data self);
SPINE_C_API void spine_physics_constraint_data_set_mix_global(spine_physics_constraint_data self, bool mixGlobal);
/**
 * Resolve ambiguity by forwarding to PosedData's implementation
 */
SPINE_C_API const char *spine_physics_constraint_data_get_name(spine_physics_constraint_data self);
SPINE_C_API bool spine_physics_constraint_data_get_skin_required(spine_physics_constraint_data self);
/**
 * The setup pose that most animations are relative to.
 */
SPINE_C_API spine_physics_constraint_pose spine_physics_constraint_data_get_setup_pose(spine_physics_constraint_data self);
SPINE_C_API void spine_physics_constraint_data_set_skin_required(spine_physics_constraint_data self, bool skinRequired);
SPINE_C_API spine_rtti spine_physics_constraint_data_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_PHYSICS_CONSTRAINT_DATA_H */
