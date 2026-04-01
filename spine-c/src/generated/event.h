#ifndef SPINE_SPINE_EVENT_H
#define SPINE_SPINE_EVENT_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_event spine_event_create(float time, spine_event_data data);

SPINE_C_API void spine_event_dispose(spine_event self);

/**
 * The event's setup pose data.
 */
SPINE_C_API spine_event_data spine_event_get_data(spine_event self);
/**
 * The animation time this event was keyed, or -1 for the setup pose.
 */
SPINE_C_API float spine_event_get_time(spine_event self);
/**
 * The integer payload for this event.
 */
SPINE_C_API int spine_event_get_int(spine_event self);
SPINE_C_API void spine_event_set_int(spine_event self, int inValue);
/**
 * The float payload for this event.
 */
SPINE_C_API float spine_event_get_float(spine_event self);
SPINE_C_API void spine_event_set_float(spine_event self, float inValue);
/**
 * The string payload for this event.
 */
SPINE_C_API const char *spine_event_get_string(spine_event self);
SPINE_C_API void spine_event_set_string(spine_event self, const char *inValue);
/**
 * If an audio path is set, the volume for the audio.
 */
SPINE_C_API float spine_event_get_volume(spine_event self);
SPINE_C_API void spine_event_set_volume(spine_event self, float inValue);
/**
 * If an audio path is set, the left/right balance for the audio.
 */
SPINE_C_API float spine_event_get_balance(spine_event self);
SPINE_C_API void spine_event_set_balance(spine_event self, float inValue);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_EVENT_H */
