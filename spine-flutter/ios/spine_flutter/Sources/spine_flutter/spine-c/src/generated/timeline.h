#ifndef SPINE_SPINE_TIMELINE_H
#define SPINE_SPINE_TIMELINE_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API void spine_timeline_dispose(spine_timeline self);

SPINE_C_API spine_rtti spine_timeline_get_rtti(spine_timeline self);
/**
 * Applies this timeline to the skeleton.
 *
 * See Applying Animations in the Spine Runtimes Guide.
 *
 * @param skeleton The skeleton the timeline is applied to. This provides access to the bones, slots, and other skeleton components the timelines may change.
 * @param lastTime The last time in seconds this timeline was applied. Some timelines trigger only at discrete times, in which case all keys are triggered between lastTime (exclusive) and time (inclusive). Pass -1 the first time a timeline is applied to ensure frame 0 is triggered.
 * @param time The time in seconds the skeleton is being posed for. Timelines find the frame before and after this time and interpolate between the frame values.
 * @param events If any events are fired, they are added to this list. Can be NULL to ignore fired events or if no timelines fire events.
 * @param alpha 0 applies setup or current values (depending on from), 1 uses timeline values, and intermediate values interpolate between them. Adjusting alpha over time can mix a timeline in or out.
 * @param from If true, alpha transitions between setup and timeline values, setup values are used before the first frame (current values are not used). If false, alpha transitions between current and timeline values, no change is made before the first frame.
 * @param add If true, for timelines that support it, their values are added to the setup or current values (depending on from).
 * @param out True when the animation is mixing out, else it is mixing in. Used by timelines that perform instant transitions.
 * @param appliedPose True to modify getAppliedPose(), else getPose() is modified.
 */
SPINE_C_API void spine_timeline_apply(spine_timeline self, spine_skeleton skeleton, float lastTime, float time, /*@null*/ spine_array_event events,
									  float alpha, spine_mix_from from, bool add, bool out, bool appliedPose);
/**
 * True if this timeline supports additive blending.
 */
SPINE_C_API bool spine_timeline_get_additive(spine_timeline self);
/**
 * True if this timeline sets values instantaneously and does not support
 * interpolation between frames.
 */
SPINE_C_API bool spine_timeline_get_instant(spine_timeline self);
SPINE_C_API size_t spine_timeline_get_frame_entries(spine_timeline self);
SPINE_C_API size_t spine_timeline_get_frame_count(spine_timeline self);
SPINE_C_API spine_array_float spine_timeline_get_frames(spine_timeline self);
SPINE_C_API float spine_timeline_get_duration(spine_timeline self);
SPINE_C_API spine_array_property_id spine_timeline_get_property_ids(spine_timeline self);
SPINE_C_API spine_rtti spine_timeline_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_TIMELINE_H */
