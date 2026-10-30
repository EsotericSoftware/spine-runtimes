#include "skeleton_clipping.h"
#include <spine/spine.h>

using namespace spine;

spine_skeleton_clipping spine_skeleton_clipping_create(void) {
	return (spine_skeleton_clipping) new (__FILE__, __LINE__) SkeletonClipping();
}

void spine_skeleton_clipping_dispose(spine_skeleton_clipping self) {
	delete (SkeletonClipping *) self;
}

size_t spine_skeleton_clipping_clip_start(spine_skeleton_clipping self, spine_skeleton skeleton, spine_slot slot,
										  /*@null*/ spine_clipping_attachment clip) {
	SkeletonClipping *_self = (SkeletonClipping *) self;
	return _self->clipStart(*((Skeleton *) skeleton), *((Slot *) slot), (ClippingAttachment *) clip);
}

void spine_skeleton_clipping_clip_end_1(spine_skeleton_clipping self, spine_slot slot) {
	SkeletonClipping *_self = (SkeletonClipping *) self;
	_self->clipEnd(*((Slot *) slot));
}

void spine_skeleton_clipping_clip_end_2(spine_skeleton_clipping self) {
	SkeletonClipping *_self = (SkeletonClipping *) self;
	_self->clipEnd();
}

bool spine_skeleton_clipping_clip_triangles_1(spine_skeleton_clipping self, /*@null*/ float *vertices, /*@null*/ unsigned short *triangles,
											  size_t trianglesLength) {
	SkeletonClipping *_self = (SkeletonClipping *) self;
	return _self->clipTriangles(vertices, triangles, trianglesLength);
}

bool spine_skeleton_clipping_clip_triangles_2(spine_skeleton_clipping self, /*@null*/ float *vertices, /*@null*/ unsigned short *triangles,
											  size_t trianglesLength, /*@null*/ float *uvs, size_t stride) {
	SkeletonClipping *_self = (SkeletonClipping *) self;
	return _self->clipTriangles(vertices, triangles, trianglesLength, uvs, stride);
}

bool spine_skeleton_clipping_clip_triangles_3(spine_skeleton_clipping self, spine_array_float vertices, spine_array_unsigned_short triangles,
											  spine_array_float uvs, size_t stride) {
	SkeletonClipping *_self = (SkeletonClipping *) self;
	return _self->clipTriangles(*((Array<float> *) vertices), *((Array<unsigned short> *) triangles), *((Array<float> *) uvs), stride);
}

bool spine_skeleton_clipping_is_clipping(spine_skeleton_clipping self) {
	SkeletonClipping *_self = (SkeletonClipping *) self;
	return _self->isClipping();
}

spine_array_float spine_skeleton_clipping_get_clipped_vertices(spine_skeleton_clipping self) {
	SkeletonClipping *_self = (SkeletonClipping *) self;
	return (spine_array_float) &_self->getClippedVertices();
}

spine_array_unsigned_short spine_skeleton_clipping_get_clipped_triangles(spine_skeleton_clipping self) {
	SkeletonClipping *_self = (SkeletonClipping *) self;
	return (spine_array_unsigned_short) &_self->getClippedTriangles();
}

spine_array_float spine_skeleton_clipping_get_clipped_u_vs(spine_skeleton_clipping self) {
	SkeletonClipping *_self = (SkeletonClipping *) self;
	return (spine_array_float) &_self->getClippedUVs();
}
