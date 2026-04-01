#ifndef SPINE_SPINE_SLIDER_DATA_H
#define SPINE_SPINE_SLIDER_DATA_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_slider_data spine_slider_data_create(const char *name);

SPINE_C_API void spine_slider_data_dispose(spine_slider_data self);

SPINE_C_API spine_rtti spine_slider_data_get_rtti(spine_slider_data self);
/**
 * Creates a slider instance.
 */
SPINE_C_API spine_constraint spine_slider_data_create_method(spine_slider_data self, spine_skeleton skeleton);
/**
 * The animation the slider will apply.
 */
SPINE_C_API spine_animation spine_slider_data_get_animation(spine_slider_data self);
SPINE_C_API void spine_slider_data_set_animation(spine_slider_data self, spine_animation animation);
/**
 * When true, the animation is applied by adding it to the current pose rather
 * than overwriting it.
 */
SPINE_C_API bool spine_slider_data_get_additive(spine_slider_data self);
SPINE_C_API void spine_slider_data_set_additive(spine_slider_data self, bool additive);
/**
 * When true, the animation repeats after its duration, otherwise the last frame
 * is used.
 */
SPINE_C_API bool spine_slider_data_get_loop(spine_slider_data self);
SPINE_C_API void spine_slider_data_set_loop(spine_slider_data self, bool loop);
/**
 * When set, the bone's transform property is used to set the slider's
 * SliderPose::getTime().
 */
SPINE_C_API /*@null*/ spine_bone_data spine_slider_data_get_bone(spine_slider_data self);
SPINE_C_API void spine_slider_data_set_bone(spine_slider_data self, /*@null*/ spine_bone_data bone);
/**
 * When a bone is set, the specified transform property is used to set the
 * slider's SliderPose::getTime().
 */
SPINE_C_API /*@null*/ spine_from_property spine_slider_data_get_property(spine_slider_data self);
SPINE_C_API void spine_slider_data_set_property(spine_slider_data self, /*@null*/ spine_from_property property);
/**
 * When a bone is set, this is the scale of the property value in relation to
 * the slider time.
 */
SPINE_C_API float spine_slider_data_get_scale(spine_slider_data self);
SPINE_C_API void spine_slider_data_set_scale(spine_slider_data self, float scale);
/**
 * When a bone is set, the offset is added to the property.
 */
SPINE_C_API float spine_slider_data_get_offset(spine_slider_data self);
SPINE_C_API void spine_slider_data_set_offset(spine_slider_data self, float offset);
/**
 * When true and a bone is set, the bone's local transform property is read
 * instead of its world transform.
 */
SPINE_C_API bool spine_slider_data_get_local(spine_slider_data self);
SPINE_C_API void spine_slider_data_set_local(spine_slider_data self, bool local);
/**
 * Resolve ambiguity by forwarding to PosedData's implementation
 */
SPINE_C_API const char *spine_slider_data_get_name(spine_slider_data self);
SPINE_C_API bool spine_slider_data_get_skin_required(spine_slider_data self);
/**
 * The setup pose that most animations are relative to.
 */
SPINE_C_API spine_slider_pose spine_slider_data_get_setup_pose(spine_slider_data self);
SPINE_C_API void spine_slider_data_set_skin_required(spine_slider_data self, bool skinRequired);
SPINE_C_API spine_rtti spine_slider_data_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_SLIDER_DATA_H */
