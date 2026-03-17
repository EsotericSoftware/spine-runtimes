#ifndef SPINE_SPINE_SKELETON_DATA_H
#define SPINE_SPINE_SKELETON_DATA_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_skeleton_data spine_skeleton_data_create(void);

SPINE_C_API void spine_skeleton_data_dispose(spine_skeleton_data self);

/**
 * Finds a bone by comparing each bone's name. It is more efficient to cache the
 * results of this method than to call it multiple times.
 *
 * @return May be NULL.
 */
SPINE_C_API /*@null*/ spine_bone_data spine_skeleton_data_find_bone(spine_skeleton_data self, const char *boneName);
/**
 *
 * @return May be NULL.
 */
SPINE_C_API /*@null*/ spine_slot_data spine_skeleton_data_find_slot(spine_skeleton_data self, const char *slotName);
/**
 *
 * @return May be NULL.
 */
SPINE_C_API /*@null*/ spine_skin spine_skeleton_data_find_skin(spine_skeleton_data self, const char *skinName);
/**
 *
 * @return May be NULL.
 */
SPINE_C_API /*@null*/ spine_event_data spine_skeleton_data_find_event(spine_skeleton_data self, const char *eventDataName);
/**
 *
 * @return May be NULL.
 */
SPINE_C_API /*@null*/ spine_animation spine_skeleton_data_find_animation(spine_skeleton_data self, const char *animationName);
/**
 * Collects animations used by slider constraints.
 */
SPINE_C_API spine_array_animation spine_skeleton_data_find_slider_animations(spine_skeleton_data self, spine_array_animation animations);
/**
 * The skeleton's name, which by default is the name of the skeleton data file
 * when possible, or null when a name hasn't been set.
 */
SPINE_C_API const char *spine_skeleton_data_get_name(spine_skeleton_data self);
SPINE_C_API void spine_skeleton_data_set_name(spine_skeleton_data self, const char *inValue);
/**
 * The skeleton's bones, sorted parent first. The root bone is always the first
 * bone.
 */
SPINE_C_API spine_array_bone_data spine_skeleton_data_get_bones(spine_skeleton_data self);
/**
 * The skeleton's slots in the setup pose draw order.
 */
SPINE_C_API spine_array_slot_data spine_skeleton_data_get_slots(spine_skeleton_data self);
/**
 * All skins, including the default skin.
 */
SPINE_C_API spine_array_skin spine_skeleton_data_get_skins(spine_skeleton_data self);
/**
 * The skeleton's default skin. By default this skin contains all attachments
 * that were not in a skin in Spine.
 *
 * @return May be NULL.
 */
SPINE_C_API /*@null*/ spine_skin spine_skeleton_data_get_default_skin(spine_skeleton_data self);
SPINE_C_API void spine_skeleton_data_set_default_skin(spine_skeleton_data self, /*@null*/ spine_skin inValue);
/**
 * The skeleton's events.
 */
SPINE_C_API spine_array_event_data spine_skeleton_data_get_events(spine_skeleton_data self);
/**
 * The skeleton's animations.
 */
SPINE_C_API spine_array_animation spine_skeleton_data_get_animations(spine_skeleton_data self);
/**
 * The skeleton's constraints.
 */
SPINE_C_API spine_array_constraint_data spine_skeleton_data_get_constraints(spine_skeleton_data self);
/**
 * The X coordinate of the skeleton's axis aligned bounding box in the setup
 * pose.
 */
SPINE_C_API float spine_skeleton_data_get_x(spine_skeleton_data self);
SPINE_C_API void spine_skeleton_data_set_x(spine_skeleton_data self, float inValue);
/**
 * The Y coordinate of the skeleton's axis aligned bounding box in the setup
 * pose.
 */
SPINE_C_API float spine_skeleton_data_get_y(spine_skeleton_data self);
SPINE_C_API void spine_skeleton_data_set_y(spine_skeleton_data self, float inValue);
/**
 * The width of the skeleton's axis aligned bounding box in the setup pose.
 */
SPINE_C_API float spine_skeleton_data_get_width(spine_skeleton_data self);
SPINE_C_API void spine_skeleton_data_set_width(spine_skeleton_data self, float inValue);
/**
 * The height of the skeleton's axis aligned bounding box in the setup pose.
 */
SPINE_C_API float spine_skeleton_data_get_height(spine_skeleton_data self);
SPINE_C_API void spine_skeleton_data_set_height(spine_skeleton_data self, float inValue);
/**
 * Baseline scale factor for applying physics and other effects based on
 * distance to non-scalable properties, such as angle or scale. Default is 100.
 */
SPINE_C_API float spine_skeleton_data_get_reference_scale(spine_skeleton_data self);
SPINE_C_API void spine_skeleton_data_set_reference_scale(spine_skeleton_data self, float inValue);
/**
 * The Spine version used to export this data, or NULL.
 */
SPINE_C_API const char *spine_skeleton_data_get_version(spine_skeleton_data self);
SPINE_C_API void spine_skeleton_data_set_version(spine_skeleton_data self, const char *inValue);
/**
 * The skeleton data hash. This value will change if any of the skeleton data
 * has changed.
 */
SPINE_C_API const char *spine_skeleton_data_get_hash(spine_skeleton_data self);
SPINE_C_API void spine_skeleton_data_set_hash(spine_skeleton_data self, const char *inValue);
/**
 * The path to the images directory as defined in Spine, or null if nonessential
 * data was not exported.
 */
SPINE_C_API const char *spine_skeleton_data_get_images_path(spine_skeleton_data self);
SPINE_C_API void spine_skeleton_data_set_images_path(spine_skeleton_data self, const char *inValue);
/**
 * The path to the audio directory as defined in Spine, or null if nonessential
 * data was not exported.
 */
SPINE_C_API const char *spine_skeleton_data_get_audio_path(spine_skeleton_data self);
SPINE_C_API void spine_skeleton_data_set_audio_path(spine_skeleton_data self, const char *inValue);
/**
 * The dopesheet FPS in Spine. Available only when nonessential data was
 * exported.
 */
SPINE_C_API float spine_skeleton_data_get_fps(spine_skeleton_data self);
SPINE_C_API void spine_skeleton_data_set_fps(spine_skeleton_data self, float inValue);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_SKELETON_DATA_H */
