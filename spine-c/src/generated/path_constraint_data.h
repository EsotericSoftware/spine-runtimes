#ifndef SPINE_SPINE_PATH_CONSTRAINT_DATA_H
#define SPINE_SPINE_PATH_CONSTRAINT_DATA_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_path_constraint_data spine_path_constraint_data_create(const char *name);

SPINE_C_API void spine_path_constraint_data_dispose(spine_path_constraint_data self);

SPINE_C_API spine_rtti spine_path_constraint_data_get_rtti(spine_path_constraint_data self);
SPINE_C_API spine_constraint spine_path_constraint_data_create_method(spine_path_constraint_data self, spine_skeleton skeleton);
/**
 * The bones that will be modified by this path constraint.
 */
SPINE_C_API spine_array_bone_data spine_path_constraint_data_get_bones(spine_path_constraint_data self);
/**
 * The slot whose path attachment will be used to constrained the bones.
 */
SPINE_C_API spine_slot_data spine_path_constraint_data_get_slot(spine_path_constraint_data self);
SPINE_C_API void spine_path_constraint_data_set_slot(spine_path_constraint_data self, spine_slot_data slot);
/**
 * The mode for positioning the first bone on the path.
 */
SPINE_C_API spine_position_mode spine_path_constraint_data_get_position_mode(spine_path_constraint_data self);
SPINE_C_API void spine_path_constraint_data_set_position_mode(spine_path_constraint_data self, spine_position_mode positionMode);
/**
 * The mode for positioning the bones after the first bone on the path.
 */
SPINE_C_API spine_spacing_mode spine_path_constraint_data_get_spacing_mode(spine_path_constraint_data self);
SPINE_C_API void spine_path_constraint_data_set_spacing_mode(spine_path_constraint_data self, spine_spacing_mode spacingMode);
/**
 * The mode for adjusting the rotation of the bones.
 */
SPINE_C_API spine_rotate_mode spine_path_constraint_data_get_rotate_mode(spine_path_constraint_data self);
SPINE_C_API void spine_path_constraint_data_set_rotate_mode(spine_path_constraint_data self, spine_rotate_mode rotateMode);
/**
 * An offset added to the constrained bone rotation.
 */
SPINE_C_API float spine_path_constraint_data_get_offset_rotation(spine_path_constraint_data self);
SPINE_C_API void spine_path_constraint_data_set_offset_rotation(spine_path_constraint_data self, float offsetRotation);
/**
 * Resolve ambiguity by forwarding to PosedData's implementation
 */
SPINE_C_API const char *spine_path_constraint_data_get_name(spine_path_constraint_data self);
SPINE_C_API bool spine_path_constraint_data_get_skin_required(spine_path_constraint_data self);
/**
 * The setup pose that most animations are relative to.
 */
SPINE_C_API spine_path_constraint_pose spine_path_constraint_data_get_setup_pose(spine_path_constraint_data self);
SPINE_C_API void spine_path_constraint_data_set_skin_required(spine_path_constraint_data self, bool skinRequired);
SPINE_C_API spine_rtti spine_path_constraint_data_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_PATH_CONSTRAINT_DATA_H */
