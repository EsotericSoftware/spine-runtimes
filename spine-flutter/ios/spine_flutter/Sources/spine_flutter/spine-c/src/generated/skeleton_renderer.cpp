#include "skeleton_renderer.h"
#include <spine/spine.h>

using namespace spine;

spine_skeleton_renderer spine_skeleton_renderer_create(void) {
	return (spine_skeleton_renderer) new (__FILE__, __LINE__) SkeletonRenderer();
}

void spine_skeleton_renderer_dispose(spine_skeleton_renderer self) {
	delete (SkeletonRenderer *) self;
}

/*@null*/ spine_render_command spine_skeleton_renderer_render(spine_skeleton_renderer self, spine_skeleton skeleton) {
	SkeletonRenderer *_self = (SkeletonRenderer *) self;
	return (spine_render_command) _self->render(*((Skeleton *) skeleton));
}
