#include "skeleton_bounds.h"
#include <spine/spine.h>

using namespace spine;

spine_skeleton_bounds spine_skeleton_bounds_create(void) {
	return (spine_skeleton_bounds) new (__FILE__, __LINE__) SkeletonBounds();
}

void spine_skeleton_bounds_dispose(spine_skeleton_bounds self) {
	delete (SkeletonBounds *) self;
}

void spine_skeleton_bounds_update(spine_skeleton_bounds self, spine_skeleton skeleton, bool updateAabb) {
	SkeletonBounds *_self = (SkeletonBounds *) self;
	_self->update(*((Skeleton *) skeleton), updateAabb);
}

bool spine_skeleton_bounds_aabb_contains_point(spine_skeleton_bounds self, float x, float y) {
	SkeletonBounds *_self = (SkeletonBounds *) self;
	return _self->aabbContainsPoint(x, y);
}

bool spine_skeleton_bounds_aabb_intersects_segment(spine_skeleton_bounds self, float x1, float y1, float x2, float y2) {
	SkeletonBounds *_self = (SkeletonBounds *) self;
	return _self->aabbIntersectsSegment(x1, y1, x2, y2);
}

bool spine_skeleton_bounds_aabb_intersects_skeleton(spine_skeleton_bounds self, spine_skeleton_bounds bounds) {
	SkeletonBounds *_self = (SkeletonBounds *) self;
	return _self->aabbIntersectsSkeleton(*((SkeletonBounds *) bounds));
}

bool spine_skeleton_bounds_contains_point_1(spine_skeleton_bounds self, spine_polygon polygon, float x, float y) {
	SkeletonBounds *_self = (SkeletonBounds *) self;
	return _self->containsPoint(*((Polygon *) polygon), x, y);
}

/*@null*/ spine_bounding_box_attachment spine_skeleton_bounds_contains_point_2(spine_skeleton_bounds self, float x, float y) {
	SkeletonBounds *_self = (SkeletonBounds *) self;
	return (spine_bounding_box_attachment) _self->containsPoint(x, y);
}

/*@null*/ spine_bounding_box_attachment spine_skeleton_bounds_intersects_segment_1(spine_skeleton_bounds self, float x1, float y1, float x2,
																				   float y2) {
	SkeletonBounds *_self = (SkeletonBounds *) self;
	return (spine_bounding_box_attachment) _self->intersectsSegment(x1, y1, x2, y2);
}

bool spine_skeleton_bounds_intersects_segment_2(spine_skeleton_bounds self, spine_polygon polygon, float x1, float y1, float x2, float y2) {
	SkeletonBounds *_self = (SkeletonBounds *) self;
	return _self->intersectsSegment(*((Polygon *) polygon), x1, y1, x2, y2);
}

/*@null*/ spine_polygon spine_skeleton_bounds_get_polygon(spine_skeleton_bounds self, /*@null*/ spine_bounding_box_attachment attachment) {
	SkeletonBounds *_self = (SkeletonBounds *) self;
	return (spine_polygon) _self->getPolygon((BoundingBoxAttachment *) attachment);
}

/*@null*/ spine_bounding_box_attachment spine_skeleton_bounds_get_bounding_box(spine_skeleton_bounds self, /*@null*/ spine_polygon polygon) {
	SkeletonBounds *_self = (SkeletonBounds *) self;
	return (spine_bounding_box_attachment) _self->getBoundingBox((Polygon *) polygon);
}

spine_array_polygon spine_skeleton_bounds_get_polygons(spine_skeleton_bounds self) {
	SkeletonBounds *_self = (SkeletonBounds *) self;
	return (spine_array_polygon) &_self->getPolygons();
}

spine_array_bounding_box_attachment spine_skeleton_bounds_get_bounding_boxes(spine_skeleton_bounds self) {
	SkeletonBounds *_self = (SkeletonBounds *) self;
	return (spine_array_bounding_box_attachment) &_self->getBoundingBoxes();
}

float spine_skeleton_bounds_get_min_x(spine_skeleton_bounds self) {
	SkeletonBounds *_self = (SkeletonBounds *) self;
	return _self->getMinX();
}

float spine_skeleton_bounds_get_min_y(spine_skeleton_bounds self) {
	SkeletonBounds *_self = (SkeletonBounds *) self;
	return _self->getMinY();
}

float spine_skeleton_bounds_get_max_x(spine_skeleton_bounds self) {
	SkeletonBounds *_self = (SkeletonBounds *) self;
	return _self->getMaxX();
}

float spine_skeleton_bounds_get_max_y(spine_skeleton_bounds self) {
	SkeletonBounds *_self = (SkeletonBounds *) self;
	return _self->getMaxY();
}

float spine_skeleton_bounds_get_width(spine_skeleton_bounds self) {
	SkeletonBounds *_self = (SkeletonBounds *) self;
	return _self->getWidth();
}

float spine_skeleton_bounds_get_height(spine_skeleton_bounds self) {
	SkeletonBounds *_self = (SkeletonBounds *) self;
	return _self->getHeight();
}
