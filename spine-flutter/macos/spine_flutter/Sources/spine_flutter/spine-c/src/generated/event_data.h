#ifndef SPINE_SPINE_EVENT_DATA_H
#define SPINE_SPINE_EVENT_DATA_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_event_data spine_event_data_create(const char *name);

SPINE_C_API void spine_event_data_dispose(spine_event_data self);

/**
 * The name of the event, unique across all events in the skeleton.
 */
SPINE_C_API const char *spine_event_data_get_name(spine_event_data self);
/**
 * The setup values that are shared by all events with this data.
 */
SPINE_C_API spine_event spine_event_data_get_setup_pose_1(spine_event_data self);
SPINE_C_API spine_event spine_event_data_get_setup_pose_2(spine_event_data self);
/**
 * Path to an audio file relative to the audio folder as defined in Spine.
 */
SPINE_C_API const char *spine_event_data_get_audio_path(spine_event_data self);
SPINE_C_API void spine_event_data_set_audio_path(spine_event_data self, const char *inValue);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_EVENT_DATA_H */
