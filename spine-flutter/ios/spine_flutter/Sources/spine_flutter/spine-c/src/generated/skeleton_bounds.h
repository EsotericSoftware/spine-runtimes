#ifndef SPINE_SPINE_SKELETON_BOUNDS_H
#define SPINE_SPINE_SKELETON_BOUNDS_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_skeleton_bounds spine_skeleton_bounds_create(void);

SPINE_C_API void spine_skeleton_bounds_dispose(spine_skeleton_bounds self);

/**
 * Clears any previous polygons, finds all visible bounding box attachments, and
 * computes the world vertices for each bounding box's polygon.
 *
 * @param skeleton The skeleton.
 * @param updateAabb If true, the axis aligned bounding box containing all the polygons is computed. If false, the SkeletonBounds AABB methods will always return true.
 */
SPINE_C_API void spine_skeleton_bounds_update(spine_skeleton_bounds self, spine_skeleton skeleton, bool updateAabb);
/**
 * Returns true if the axis aligned bounding box contains the point.
 */
SPINE_C_API bool spine_skeleton_bounds_aabb_contains_point(spine_skeleton_bounds self, float x, float y);
/**
 * Returns true if the axis aligned bounding box intersects the line segment.
 */
SPINE_C_API bool spine_skeleton_bounds_aabb_intersects_segment(spine_skeleton_bounds self, float x1, float y1, float x2, float y2);
/**
 * Returns true if the axis aligned bounding box intersects the axis aligned
 * bounding box of the specified bounds.
 */
SPINE_C_API bool spine_skeleton_bounds_aabb_intersects_skeleton(spine_skeleton_bounds self, spine_skeleton_bounds bounds);
/**
 * Returns true if the polygon contains the point.
 */
SPINE_C_API bool spine_skeleton_bounds_contains_point_1(spine_skeleton_bounds self, spine_polygon polygon, float x, float y);
/**
 * Returns the first bounding box attachment that contains the point, or null.
 * When doing many checks, it is usually more efficient to only call this method
 * if aabbContainsPoint(float, float) returns true.
 */
SPINE_C_API /*@null*/ spine_bounding_box_attachment spine_skeleton_bounds_contains_point_2(spine_skeleton_bounds self, float x, float y);
/**
 * Returns the first bounding box attachment that contains any part of the line
 * segment, or null. When doing many checks, it is usually more efficient to
 * only call this method if aabbIntersectsSegment(float, float, float, float)
 * returns true.
 */
SPINE_C_API /*@null*/ spine_bounding_box_attachment spine_skeleton_bounds_intersects_segment_1(spine_skeleton_bounds self, float x1, float y1,
																							   float x2, float y2);
/**
 * Returns true if the polygon contains any part of the line segment.
 */
SPINE_C_API bool spine_skeleton_bounds_intersects_segment_2(spine_skeleton_bounds self, spine_polygon polygon, float x1, float y1, float x2,
															float y2);
/**
 * Returns the polygon for the given bounding box attachment or null if no
 * polygon can be found for the attachment. Requires a call to update() first.
 */
SPINE_C_API /*@null*/ spine_polygon spine_skeleton_bounds_get_polygon(spine_skeleton_bounds self, /*@null*/ spine_bounding_box_attachment attachment);
/**
 * Returns the bounding box for the given polygon or null. Requires a call to
 * update() first.
 */
SPINE_C_API /*@null*/ spine_bounding_box_attachment spine_skeleton_bounds_get_bounding_box(spine_skeleton_bounds self,
																						   /*@null*/ spine_polygon polygon);
/**
 * Returns all polygons or an empty array. Requires a call to update() first.
 */
SPINE_C_API spine_array_polygon spine_skeleton_bounds_get_polygons(spine_skeleton_bounds self);
/**
 * Returns all bounding boxes. Requires a call to update() first.
 */
SPINE_C_API spine_array_bounding_box_attachment spine_skeleton_bounds_get_bounding_boxes(spine_skeleton_bounds self);
/**
 * The left edge of the axis aligned bounding box.
 */
SPINE_C_API float spine_skeleton_bounds_get_min_x(spine_skeleton_bounds self);
/**
 * The bottom edge of the axis aligned bounding box.
 */
SPINE_C_API float spine_skeleton_bounds_get_min_y(spine_skeleton_bounds self);
/**
 * The right edge of the axis aligned bounding box.
 */
SPINE_C_API float spine_skeleton_bounds_get_max_x(spine_skeleton_bounds self);
/**
 * The top edge of the axis aligned bounding box.
 */
SPINE_C_API float spine_skeleton_bounds_get_max_y(spine_skeleton_bounds self);
/**
 * The width of the axis aligned bounding box.
 */
SPINE_C_API float spine_skeleton_bounds_get_width(spine_skeleton_bounds self);
/**
 * The height of the axis aligned bounding box.
 */
SPINE_C_API float spine_skeleton_bounds_get_height(spine_skeleton_bounds self);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_SKELETON_BOUNDS_H */
