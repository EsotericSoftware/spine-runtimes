#ifndef SPINE_SPINE_EVENT_QUEUE_ENTRY_H
#define SPINE_SPINE_EVENT_QUEUE_ENTRY_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_event_queue_entry spine_event_queue_entry_create(spine_event_type eventType, /*@null*/ spine_track_entry trackEntry,
																   /*@null*/ spine_event event);

SPINE_C_API void spine_event_queue_entry_dispose(spine_event_queue_entry self);

SPINE_C_API spine_event_type spine_event_queue_entry_get__type(spine_event_queue_entry self);
SPINE_C_API void spine_event_queue_entry_set__type(spine_event_queue_entry self, spine_event_type value);
SPINE_C_API /*@null*/ spine_track_entry spine_event_queue_entry_get__entry(spine_event_queue_entry self);
SPINE_C_API void spine_event_queue_entry_set__entry(spine_event_queue_entry self, /*@null*/ spine_track_entry value);
SPINE_C_API /*@null*/ spine_event spine_event_queue_entry_get__event(spine_event_queue_entry self);
SPINE_C_API void spine_event_queue_entry_set__event(spine_event_queue_entry self, /*@null*/ spine_event value);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_EVENT_QUEUE_ENTRY_H */
