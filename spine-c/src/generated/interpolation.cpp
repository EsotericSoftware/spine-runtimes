#include "interpolation.h"
#include <spine/spine.h>

using namespace spine;

float spine_interpolation_apply_1(spine_interpolation self, float a) {
	Interpolation *_self = (Interpolation *) self;
	return _self->apply(a);
}

float spine_interpolation_apply_2(spine_interpolation self, float start, float end, float a) {
	Interpolation *_self = (Interpolation *) self;
	return _self->apply(start, end, a);
}

spine_interpolation spine_interpolation_linear(void) {
	return (spine_interpolation) &Interpolation::linear();
}

spine_interpolation spine_interpolation_smooth(void) {
	return (spine_interpolation) &Interpolation::smooth();
}

spine_interpolation spine_interpolation_slow_fast(void) {
	return (spine_interpolation) &Interpolation::slowFast();
}

spine_interpolation spine_interpolation_fast_slow(void) {
	return (spine_interpolation) &Interpolation::fastSlow();
}

spine_interpolation spine_interpolation_circle(void) {
	return (spine_interpolation) &Interpolation::circle();
}
