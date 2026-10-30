#include "bone_timeline.h"
#include <spine/spine.h>

using namespace spine;

void spine_bone_timeline_dispose(spine_bone_timeline self) {
	delete (BoneTimeline *) self;
}

spine_rtti spine_bone_timeline_get_rtti(spine_bone_timeline self) {
	BoneTimeline *_self = (BoneTimeline *) self;
	return (spine_rtti) &_self->getRTTI();
}

int spine_bone_timeline_get_bone_index(spine_bone_timeline self) {
	BoneTimeline *_self = (BoneTimeline *) self;
	return _self->getBoneIndex();
}

void spine_bone_timeline_set_bone_index(spine_bone_timeline self, int inValue) {
	BoneTimeline *_self = (BoneTimeline *) self;
	_self->setBoneIndex(inValue);
}

spine_rtti spine_bone_timeline_rtti(void) {
	return (spine_rtti) &BoneTimeline::rtti;
}
