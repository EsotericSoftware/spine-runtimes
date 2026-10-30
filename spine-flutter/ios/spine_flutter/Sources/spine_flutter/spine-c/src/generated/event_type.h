#ifndef SPINE_SPINE_EVENT_TYPE_H
#define SPINE_SPINE_EVENT_TYPE_H

#ifdef __cplusplus
extern "C" {
#endif

typedef enum spine_event_type {
	SPINE_EVENT_TYPE_START = 0,
	SPINE_EVENT_TYPE_INTERRUPT,
	SPINE_EVENT_TYPE_END,
	SPINE_EVENT_TYPE_DISPOSE,
	SPINE_EVENT_TYPE_COMPLETE,
	SPINE_EVENT_TYPE_EVENT
} spine_event_type;

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_EVENT_TYPE_H */
