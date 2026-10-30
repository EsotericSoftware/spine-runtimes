#include "event.h"
#include <spine/spine.h>

using namespace spine;

spine_event spine_event_create(float time, spine_event_data data) {
	return (spine_event) new (__FILE__, __LINE__) Event(time, *((const EventData *) data));
}

void spine_event_dispose(spine_event self) {
	delete (Event *) self;
}

spine_event_data spine_event_get_data(spine_event self) {
	Event *_self = (Event *) self;
	return (spine_event_data) &_self->getData();
}

float spine_event_get_time(spine_event self) {
	Event *_self = (Event *) self;
	return _self->getTime();
}

int spine_event_get_int(spine_event self) {
	Event *_self = (Event *) self;
	return _self->getInt();
}

void spine_event_set_int(spine_event self, int inValue) {
	Event *_self = (Event *) self;
	_self->setInt(inValue);
}

float spine_event_get_float(spine_event self) {
	Event *_self = (Event *) self;
	return _self->getFloat();
}

void spine_event_set_float(spine_event self, float inValue) {
	Event *_self = (Event *) self;
	_self->setFloat(inValue);
}

const char *spine_event_get_string(spine_event self) {
	Event *_self = (Event *) self;
	return _self->getString().buffer();
}

void spine_event_set_string(spine_event self, const char *inValue) {
	Event *_self = (Event *) self;
	_self->setString(String(inValue));
}

float spine_event_get_volume(spine_event self) {
	Event *_self = (Event *) self;
	return _self->getVolume();
}

void spine_event_set_volume(spine_event self, float inValue) {
	Event *_self = (Event *) self;
	_self->setVolume(inValue);
}

float spine_event_get_balance(spine_event self) {
	Event *_self = (Event *) self;
	return _self->getBalance();
}

void spine_event_set_balance(spine_event self, float inValue) {
	Event *_self = (Event *) self;
	_self->setBalance(inValue);
}
