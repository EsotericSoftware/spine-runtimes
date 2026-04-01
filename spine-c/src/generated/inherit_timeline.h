#ifndef SPINE_SPINE_INHERIT_TIMELINE_H
#define SPINE_SPINE_INHERIT_TIMELINE_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_inherit_timeline spine_inherit_timeline_create(size_t frameCount, int boneIndex);

SPINE_C_API void spine_inherit_timeline_dispose(spine_inherit_timeline self);

SPINE_C_API spine_rtti spine_inherit_timeline_get_rtti(spine_inherit_timeline self);
/**
 * Sets the inherit transform mode for the specified frame.
 *
 * @param frame Between 0 and frameCount, inclusive.
 * @param time The frame time in seconds.
 */
SPINE_C_API void spine_inherit_timeline_set_frame(spine_inherit_timeline self, int frame, float time, spine_inherit inherit);
SPINE_C_API void spine_inherit_timeline_apply(spine_inherit_timeline self, spine_skeleton skeleton, float lastTime, float time,
											  /*@null*/ spine_array_event events, float alpha, bool fromSetup, bool add, bool out, bool appliedPose);
SPINE_C_API int spine_inherit_timeline_get_bone_index(spine_inherit_timeline self);
SPINE_C_API void spine_inherit_timeline_set_bone_index(spine_inherit_timeline self, int inValue);
/**
 * True if this timeline supports additive blending.
 */
SPINE_C_API bool spine_inherit_timeline_get_additive(spine_inherit_timeline self);
/**
 * True if this timeline sets values instantaneously and does not support
 * interpolation between frames.
 */
SPINE_C_API bool spine_inherit_timeline_get_instant(spine_inherit_timeline self);
SPINE_C_API size_t spine_inherit_timeline_get_frame_entries(spine_inherit_timeline self);
SPINE_C_API size_t spine_inherit_timeline_get_frame_count(spine_inherit_timeline self);
SPINE_C_API spine_array_float spine_inherit_timeline_get_frames(spine_inherit_timeline self);
SPINE_C_API float spine_inherit_timeline_get_duration(spine_inherit_timeline self);
SPINE_C_API spine_array_property_id spine_inherit_timeline_get_property_ids(spine_inherit_timeline self);
SPINE_C_API spine_rtti spine_inherit_timeline_rtti(void);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_INHERIT_TIMELINE_H */
