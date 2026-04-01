#ifndef SPINE_SPINE_TRANSFORM_CONSTRAINT_DATA_H
#define SPINE_SPINE_TRANSFORM_CONSTRAINT_DATA_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_transform_constraint_data spine_transform_constraint_data_create(const char *name);

SPINE_C_API void spine_transform_constraint_data_dispose(spine_transform_constraint_data self);

SPINE_C_API spine_rtti spine_transform_constraint_data_get_rtti(spine_transform_constraint_data self);
SPINE_C_API spine_constraint spine_transform_constraint_data_create_method(spine_transform_constraint_data self, spine_skeleton skeleton);
/**
 * The bones that will be modified by this transform constraint.
 */
SPINE_C_API spine_array_bone_data spine_transform_constraint_data_get_bones(spine_transform_constraint_data self);
/**
 * The bone whose world transform will be copied to the constrained bones.
 */
SPINE_C_API spine_bone_data spine_transform_constraint_data_get_source(spine_transform_constraint_data self);
SPINE_C_API void spine_transform_constraint_data_set_source(spine_transform_constraint_data self, spine_bone_data source);
/**
 * An offset added to the constrained bone rotation.
 */
SPINE_C_API float spine_transform_constraint_data_get_offset_rotation(spine_transform_constraint_data self);
SPINE_C_API void spine_transform_constraint_data_set_offset_rotation(spine_transform_constraint_data self, float offsetRotation);
/**
 * An offset added to the constrained bone X translation.
 */
SPINE_C_API float spine_transform_constraint_data_get_offset_x(spine_transform_constraint_data self);
SPINE_C_API void spine_transform_constraint_data_set_offset_x(spine_transform_constraint_data self, float offsetX);
/**
 * An offset added to the constrained bone Y translation.
 */
SPINE_C_API float spine_transform_constraint_data_get_offset_y(spine_transform_constraint_data self);
SPINE_C_API void spine_transform_constraint_data_set_offset_y(spine_transform_constraint_data self, float offsetY);
/**
 * An offset added to the constrained bone scaleX.
 */
SPINE_C_API float spine_transform_constraint_data_get_offset_scale_x(spine_transform_constraint_data self);
SPINE_C_API void spine_transform_constraint_data_set_offset_scale_x(spine_transform_constraint_data self, float offsetScaleX);
/**
 * An offset added to the constrained bone scaleY.
 */
SPINE_C_API float spine_transform_constraint_data_get_offset_scale_y(spine_transform_constraint_data self);
SPINE_C_API void spine_transform_constraint_data_set_offset_scale_y(spine_transform_constraint_data self, float offsetScaleY);
/**
 * An offset added to the constrained bone shearY.
 */
SPINE_C_API float spine_transform_constraint_data_get_offset_shear_y(spine_transform_constraint_data self);
SPINE_C_API void spine_transform_constraint_data_set_offset_shear_y(spine_transform_constraint_data self, float offsetShearY);
/**
 * Reads the source bone's local transform instead of its world transform.
 */
SPINE_C_API bool spine_transform_constraint_data_get_local_source(spine_transform_constraint_data self);
SPINE_C_API void spine_transform_constraint_data_set_local_source(spine_transform_constraint_data self, bool localSource);
/**
 * Sets the constrained bones' local transforms instead of their world
 * transforms.
 */
SPINE_C_API bool spine_transform_constraint_data_get_local_target(spine_transform_constraint_data self);
SPINE_C_API void spine_transform_constraint_data_set_local_target(spine_transform_constraint_data self, bool localTarget);
/**
 * Adds the source bone transform to the constrained bones instead of setting it
 * absolutely.
 */
SPINE_C_API bool spine_transform_constraint_data_get_additive(spine_transform_constraint_data self);
SPINE_C_API void spine_transform_constraint_data_set_additive(spine_transform_constraint_data self, bool additive);
/**
 * Prevents constrained bones from exceeding the ranged defined by offset and
 * max.
 */
SPINE_C_API bool spine_transform_constraint_data_get_clamp(spine_transform_constraint_data self);
SPINE_C_API void spine_transform_constraint_data_set_clamp(spine_transform_constraint_data self, bool clamp);
/**
 * The mapping of transform properties to other transform properties.
 */
SPINE_C_API spine_array_from_property spine_transform_constraint_data_get_properties(spine_transform_constraint_data self);
/**
 * Resolve ambiguity by forwarding to PosedData's implementation
 */
SPINE_C_API const char *spine_transform_constraint_data_get_name(spine_transform_constraint_data self);
SPINE_C_API bool spine_transform_constraint_data_get_skin_required(spine_transform_constraint_data self);
/**
 * The setup pose that most animations are relative to.
 */
SPINE_C_API spine_transform_constraint_pose spine_transform_constraint_data_get_setup_pose(spine_transform_constraint_data self);
SPINE_C_API void spine_transform_constraint_data_set_skin_required(spine_transform_constraint_data self, bool skinRequired);
SPINE_C_API spine_rtti spine_transform_constraint_data_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_TRANSFORM_CONSTRAINT_DATA_H */
