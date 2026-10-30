#include "polygon.h"
#include <spine/spine.h>

using namespace spine;

spine_polygon spine_polygon_create(void) {
	return (spine_polygon) new (__FILE__, __LINE__) Polygon();
}

void spine_polygon_dispose(spine_polygon self) {
	delete (Polygon *) self;
}

spine_array_float spine_polygon_get__vertices(spine_polygon self) {
	Polygon *_self = (Polygon *) self;
	return (spine_array_float) &_self->_vertices;
}

void spine_polygon_set__vertices(spine_polygon self, spine_array_float value) {
	Polygon *_self = (Polygon *) self;
	_self->_vertices = *((Array<float> *) value);
}

int spine_polygon_get__count(spine_polygon self) {
	Polygon *_self = (Polygon *) self;
	return _self->_count;
}

void spine_polygon_set__count(spine_polygon self, int value) {
	Polygon *_self = (Polygon *) self;
	_self->_count = value;
}
