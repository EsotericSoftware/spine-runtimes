#ifndef SPINE_SPINE_ANIMATION_H
#define SPINE_SPINE_ANIMATION_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

/**
 * Creates a new animation. The timelines must be set before use.
 */
SPINE_C_API spine_animation spine_animation_create(const char *name);

SPINE_C_API void spine_animation_dispose(spine_animation self);

/**
 * If this list or the timelines it contains are modified, the timelines and
 * bones must be set again to recompute the animation's bone indices and
 * timeline property IDs.
 *
 * See setTimelines().
 */
SPINE_C_API spine_array_timeline spine_animation_get_timelines(spine_animation self);
/**
 * Sets the timelines and bone indices.
 */
SPINE_C_API void spine_animation_set_timelines(spine_animation self, spine_array_timeline timelines, spine_array_int bones);
/**
 * Returns true if this animation contains a timeline with any of the specified
 * property IDs.
 */
SPINE_C_API bool spine_animation_has_timeline(spine_animation self, spine_array_property_id ids);
/**
 * The duration of the animation in seconds, which is usually the highest time
 * of all frames in the timeline. The duration is used to know when it has
 * completed and when it should loop back to the start.
 */
SPINE_C_API float spine_animation_get_duration(spine_animation self);
SPINE_C_API void spine_animation_set_duration(spine_animation self, float inValue);
/**
 * Applies the animation's timelines to the specified skeleton.
 *
 * See Timeline::apply() and Applying Animations in the Spine Runtimes Guide.
 *
 * @param skeleton The skeleton the animation is applied to. This provides access to the bones, slots, and other skeleton components the timelines may change.
 * @param lastTime The last time in seconds this animation was applied. Some timelines trigger only at discrete times, in which case all keys are triggered between lastTime (exclusive) and time (inclusive). Pass -1 the first time an animation is applied to ensure frame 0 is triggered.
 * @param time The time in seconds the skeleton is being posed for. Timelines find the frame before and after this time and interpolate between the frame values.
 * @param loop True if time beyond the animation duration repeats the animation, else the last frame is used.
 * @param events If any events are fired, they are added to this list. Can be NULL to ignore fired events or if no timelines fire events.
 * @param alpha 0 applies setup or current values (depending on fromSetup), 1 uses timeline values, and intermediate values interpolate between them. Adjusting alpha over time can mix an animation in or out.
 * @param fromSetup If true, alpha transitions between setup and timeline values, setup values are used before the first frame (current values are not used). If false, alpha transitions between current and timeline values, no change is made before the first frame.
 * @param add If true, for timelines that support it, their values are added to the setup or current values (depending on fromSetup).
 * @param out True when the animation is mixing out, else it is mixing in. Used by timelines that perform instant transitions.
 * @param appliedPose True to modify getAppliedPose(), else the unconstrained pose is modified.
 */
SPINE_C_API void spine_animation_apply(spine_animation self, spine_skeleton skeleton, float lastTime, float time, bool loop,
									   /*@null*/ spine_array_event events, float alpha, bool fromSetup, bool add, bool out, bool appliedPose);
/**
 * The animation's name, which is unique across all animations in the skeleton.
 */
SPINE_C_API const char *spine_animation_get_name(spine_animation self);
/**
 * The Skeleton::getBones() indices affected by this animation.
 *
 * See setTimelines() and BoneTimeline::getBoneIndex().
 */
SPINE_C_API spine_array_int spine_animation_get_bones(spine_animation self);
/**
 * The color of the animation as it was in Spine, or a default color if
 * nonessential data was not exported.
 */
SPINE_C_API spine_color spine_animation_get_color(spine_animation self);
/**
 *
 * @param target After the first and before the last entry.
 */
SPINE_C_API int spine_animation_search_1(spine_array_float values, float target);
SPINE_C_API int spine_animation_search_2(spine_array_float values, float target, int step);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_ANIMATION_H */
