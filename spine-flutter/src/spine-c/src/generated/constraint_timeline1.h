#ifndef SPINE_SPINE_CONSTRAINT_TIMELINE1_H
#define SPINE_SPINE_CONSTRAINT_TIMELINE1_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API void spine_constraint_timeline1_dispose(spine_constraint_timeline1 self);

SPINE_C_API spine_rtti spine_constraint_timeline1_get_rtti(spine_constraint_timeline1 self);
SPINE_C_API int spine_constraint_timeline1_get_constraint_index(spine_constraint_timeline1 self);
SPINE_C_API void spine_constraint_timeline1_set_constraint_index(spine_constraint_timeline1 self, int inValue);
/**
 * Sets the time and value for the specified frame.
 *
 * @param frame Between 0 and frameCount, inclusive.
 * @param time The frame time in seconds.
 */
SPINE_C_API void spine_constraint_timeline1_set_frame(spine_constraint_timeline1 self, size_t frame, float time, float value);
/**
 * Returns the interpolated value for the specified time.
 */
SPINE_C_API float spine_constraint_timeline1_get_curve_value(spine_constraint_timeline1 self, float time);
SPINE_C_API float spine_constraint_timeline1_get_relative_value(spine_constraint_timeline1 self, float time, float alpha, spine_mix_from from,
																bool add, float current, float setup);
SPINE_C_API float spine_constraint_timeline1_get_absolute_value_1(spine_constraint_timeline1 self, float time, float alpha, spine_mix_from from,
																  bool add, float current, float setup);
SPINE_C_API float spine_constraint_timeline1_get_absolute_value_2(spine_constraint_timeline1 self, float time, float alpha, spine_mix_from from,
																  bool add, float current, float setup, float value);
SPINE_C_API float spine_constraint_timeline1_get_scale_value(spine_constraint_timeline1 self, float time, float alpha, spine_mix_from from, bool add,
															 bool out, float current, float setup);
SPINE_C_API float spine_constraint_timeline1_before_first_key(spine_mix_from from, float alpha, float current, float setup);
SPINE_C_API void spine_constraint_timeline1_set_linear(spine_constraint_timeline1 self, size_t frame);
SPINE_C_API void spine_constraint_timeline1_set_stepped(spine_constraint_timeline1 self, size_t frame);
SPINE_C_API void spine_constraint_timeline1_set_bezier(spine_constraint_timeline1 self, size_t bezier, size_t frame, float value, float time1,
													   float value1, float cx1, float cy1, float cx2, float cy2, float time2, float value2);
SPINE_C_API float spine_constraint_timeline1_get_bezier_value(spine_constraint_timeline1 self, float time, size_t frame, size_t valueOffset,
															  size_t i);
SPINE_C_API spine_array_float spine_constraint_timeline1_get_curves(spine_constraint_timeline1 self);
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
SPINE_C_API void spine_constraint_timeline1_apply(spine_constraint_timeline1 self, spine_skeleton skeleton, float lastTime, float time,
												  /*@null*/ spine_array_event events, float alpha, spine_mix_from from, bool add, bool out,
												  bool appliedPose);
/**
 * True if this timeline supports additive blending.
 */
SPINE_C_API bool spine_constraint_timeline1_get_additive(spine_constraint_timeline1 self);
/**
 * True if this timeline sets values instantaneously and does not support
 * interpolation between frames.
 */
SPINE_C_API bool spine_constraint_timeline1_get_instant(spine_constraint_timeline1 self);
SPINE_C_API size_t spine_constraint_timeline1_get_frame_entries(spine_constraint_timeline1 self);
SPINE_C_API size_t spine_constraint_timeline1_get_frame_count(spine_constraint_timeline1 self);
SPINE_C_API spine_array_float spine_constraint_timeline1_get_frames(spine_constraint_timeline1 self);
SPINE_C_API float spine_constraint_timeline1_get_duration(spine_constraint_timeline1 self);
SPINE_C_API spine_array_property_id spine_constraint_timeline1_get_property_ids(spine_constraint_timeline1 self);
SPINE_C_API spine_rtti spine_constraint_timeline1_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_CONSTRAINT_TIMELINE1_H */
