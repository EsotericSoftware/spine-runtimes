#include "event_data.h"
#include <spine/spine.h>

using namespace spine;

spine_event_data spine_event_data_create(const char *name) {
	return (spine_event_data) new (__FILE__, __LINE__) EventData(String(name));
}

void spine_event_data_dispose(spine_event_data self) {
	delete (EventData *) self;
}

const char *spine_event_data_get_name(spine_event_data self) {
	EventData *_self = (EventData *) self;
	return _self->getName().buffer();
}

spine_event spine_event_data_get_setup_pose_1(spine_event_data self) {
	EventData *_self = (EventData *) self;
	return (spine_event) &_self->getSetupPose();
}

spine_event spine_event_data_get_setup_pose_2(spine_event_data self) {
	EventData *_self = (EventData *) self;
	return (spine_event) &_self->getSetupPose();
}

const char *spine_event_data_get_audio_path(spine_event_data self) {
	EventData *_self = (EventData *) self;
	return _self->getAudioPath().buffer();
}

void spine_event_data_set_audio_path(spine_event_data self, const char *inValue) {
	EventData *_self = (EventData *) self;
	_self->setAudioPath(String(inValue));
}
