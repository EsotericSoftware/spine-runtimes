#ifndef SPINE_SPINE_TO_PROPERTY_H
#define SPINE_SPINE_TO_PROPERTY_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API void spine_to_property_dispose(spine_to_property self);

SPINE_C_API spine_rtti spine_to_property_get_rtti(spine_to_property self);
/**
 * Reads the mix for this property from the specified pose.
 */
SPINE_C_API float spine_to_property_mix(spine_to_property self, spine_transform_constraint_pose pose);
/**
 * Applies the value to this property.
 */
SPINE_C_API void spine_to_property_apply(spine_to_property self, spine_skeleton skeleton, spine_transform_constraint_pose pose, spine_bone_pose bone,
										 float value, bool local, bool additive);
SPINE_C_API spine_rtti spine_to_property_rtti(void);
SPINE_C_API float spine_to_property_get__offset(spine_to_property self);
SPINE_C_API void spine_to_property_set__offset(spine_to_property self, float value);
SPINE_C_API float spine_to_property_get__max(spine_to_property self);
SPINE_C_API void spine_to_property_set__max(spine_to_property self, float value);
SPINE_C_API float spine_to_property_get__scale(spine_to_property self);
SPINE_C_API void spine_to_property_set__scale(spine_to_property self, float value);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_TO_PROPERTY_H */
