#include "color.h"
#include <spine/spine.h>

using namespace spine;

spine_color spine_color_create(void) {
	return (spine_color) new (__FILE__, __LINE__) Color();
}

spine_color spine_color_create2(float r, float g, float b, float a) {
	return (spine_color) new (__FILE__, __LINE__) Color(r, g, b, a);
}

void spine_color_dispose(spine_color self) {
	delete (Color *) self;
}

spine_color spine_color_set_1(spine_color self, float _r, float _g, float _b, float _a) {
	Color *_self = (Color *) self;
	return (spine_color) &_self->set(_r, _g, _b, _a);
}

spine_color spine_color_set_2(spine_color self, float _r, float _g, float _b) {
	Color *_self = (Color *) self;
	return (spine_color) &_self->set(_r, _g, _b);
}

spine_color spine_color_set_3(spine_color self, spine_color other) {
	Color *_self = (Color *) self;
	return (spine_color) &_self->set(*((const Color *) other));
}

spine_color spine_color_add_1(spine_color self, float _r, float _g, float _b, float _a) {
	Color *_self = (Color *) self;
	return (spine_color) &_self->add(_r, _g, _b, _a);
}

spine_color spine_color_add_2(spine_color self, float _r, float _g, float _b) {
	Color *_self = (Color *) self;
	return (spine_color) &_self->add(_r, _g, _b);
}

spine_color spine_color_add_3(spine_color self, spine_color other) {
	Color *_self = (Color *) self;
	return (spine_color) &_self->add(*((const Color *) other));
}

spine_color spine_color_clamp(spine_color self) {
	Color *_self = (Color *) self;
	return (spine_color) &_self->clamp();
}

float spine_color_parse_hex(/*@null*/ const char *value, size_t index) {
	return Color::parseHex(value, index);
}

void spine_color_rgba8888_to_color(spine_color color, int value) {
	Color::rgba8888ToColor(*((Color *) color), value);
}

void spine_color_rgb888_to_color(spine_color color, int value) {
	Color::rgb888ToColor(*((Color *) color), value);
}

float spine_color_get_r(spine_color self) {
	Color *_self = (Color *) self;
	return _self->r;
}

void spine_color_set_r(spine_color self, float value) {
	Color *_self = (Color *) self;
	_self->r = value;
}

float spine_color_get_g(spine_color self) {
	Color *_self = (Color *) self;
	return _self->g;
}

void spine_color_set_g(spine_color self, float value) {
	Color *_self = (Color *) self;
	_self->g = value;
}

float spine_color_get_b(spine_color self) {
	Color *_self = (Color *) self;
	return _self->b;
}

void spine_color_set_b(spine_color self, float value) {
	Color *_self = (Color *) self;
	_self->b = value;
}

float spine_color_get_a(spine_color self) {
	Color *_self = (Color *) self;
	return _self->a;
}

void spine_color_set_a(spine_color self, float value) {
	Color *_self = (Color *) self;
	_self->a = value;
}
