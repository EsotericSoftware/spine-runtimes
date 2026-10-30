#include "slider_pose.h"
#include <spine/spine.h>

using namespace spine;

spine_slider_pose spine_slider_pose_create(void) {
	return (spine_slider_pose) new (__FILE__, __LINE__) SliderPose();
}

void spine_slider_pose_dispose(spine_slider_pose self) {
	delete (SliderPose *) self;
}

void spine_slider_pose_set(spine_slider_pose self, spine_slider_pose pose) {
	SliderPose *_self = (SliderPose *) self;
	_self->set(*((SliderPose *) pose));
}

float spine_slider_pose_get_time(spine_slider_pose self) {
	SliderPose *_self = (SliderPose *) self;
	return _self->getTime();
}

void spine_slider_pose_set_time(spine_slider_pose self, float time) {
	SliderPose *_self = (SliderPose *) self;
	_self->setTime(time);
}

float spine_slider_pose_get_mix(spine_slider_pose self) {
	SliderPose *_self = (SliderPose *) self;
	return _self->getMix();
}

void spine_slider_pose_set_mix(spine_slider_pose self, float mix) {
	SliderPose *_self = (SliderPose *) self;
	_self->setMix(mix);
}
