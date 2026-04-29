//
// Spine Runtimes License Agreement
// Last updated April 5, 2025. Replaces all prior versions.
//
// Copyright (c) 2013-2025, Esoteric Software LLC
//
// Integration of the Spine Runtimes into software or otherwise creating
// derivative works of the Spine Runtimes is permitted under the terms and
// conditions of Section 2 of the Spine Editor License Agreement:
// http://esotericsoftware.com/spine-editor-license
//
// Otherwise, it is permitted to integrate the Spine Runtimes into software
// or otherwise create derivative works of the Spine Runtimes (collectively,
// "Products"), provided that each user of the Products must obtain their own
// Spine Editor license and redistribution of the Products in any form must
// include this license and copyright notice.
//
// THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
// BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

// AUTO GENERATED FILE, DO NOT EDIT.

import Foundation
import SpineC

/// Collects each BoundingBoxAttachment that is visible and computes the world vertices for its
/// polygon. The polygon vertices are provided along with convenience methods for doing hit
/// detection.
@objc(SpineSkeletonBounds)
@objcMembers
public class SkeletonBounds: NSObject {
    public let _ptr: UnsafeMutableRawPointer

    public init(fromPointer ptr: spine_skeleton_bounds) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        super.init()
    }

    public override convenience init() {
        let ptr = spine_skeleton_bounds_create()
        self.init(fromPointer: ptr!)
    }

    /// Returns all polygons or an empty array. Requires a call to update() first.
    public var polygons: ArrayPolygon {
        let result = spine_skeleton_bounds_get_polygons(_ptr.assumingMemoryBound(to: spine_skeleton_bounds_wrapper.self))
        return ArrayPolygon(fromPointer: result!)
    }

    /// Returns all bounding boxes. Requires a call to update() first.
    public var boundingBoxes: ArrayBoundingBoxAttachment {
        let result = spine_skeleton_bounds_get_bounding_boxes(_ptr.assumingMemoryBound(to: spine_skeleton_bounds_wrapper.self))
        return ArrayBoundingBoxAttachment(fromPointer: result!)
    }

    /// The left edge of the axis aligned bounding box.
    public var minX: Float {
        let result = spine_skeleton_bounds_get_min_x(_ptr.assumingMemoryBound(to: spine_skeleton_bounds_wrapper.self))
        return result
    }

    /// The bottom edge of the axis aligned bounding box.
    public var minY: Float {
        let result = spine_skeleton_bounds_get_min_y(_ptr.assumingMemoryBound(to: spine_skeleton_bounds_wrapper.self))
        return result
    }

    /// The right edge of the axis aligned bounding box.
    public var maxX: Float {
        let result = spine_skeleton_bounds_get_max_x(_ptr.assumingMemoryBound(to: spine_skeleton_bounds_wrapper.self))
        return result
    }

    /// The top edge of the axis aligned bounding box.
    public var maxY: Float {
        let result = spine_skeleton_bounds_get_max_y(_ptr.assumingMemoryBound(to: spine_skeleton_bounds_wrapper.self))
        return result
    }

    /// The width of the axis aligned bounding box.
    public var width: Float {
        let result = spine_skeleton_bounds_get_width(_ptr.assumingMemoryBound(to: spine_skeleton_bounds_wrapper.self))
        return result
    }

    /// The height of the axis aligned bounding box.
    public var height: Float {
        let result = spine_skeleton_bounds_get_height(_ptr.assumingMemoryBound(to: spine_skeleton_bounds_wrapper.self))
        return result
    }

    /// Clears any previous polygons, finds all visible bounding box attachments, and computes the
    /// world vertices for each bounding box's polygon.
    ///
    /// - Parameter skeleton: The skeleton.
    /// - Parameter updateAabb: If true, the axis aligned bounding box containing all the polygons is computed. If false, the SkeletonBounds AABB methods will always return true.
    public func update(_ skeleton: Skeleton, _ updateAabb: Bool) {
        spine_skeleton_bounds_update(_ptr.assumingMemoryBound(to: spine_skeleton_bounds_wrapper.self), skeleton._ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), updateAabb)
    }

    /// Returns true if the axis aligned bounding box contains the point.
    public func aabbContainsPoint(_ x: Float, _ y: Float) -> Bool {
        let result = spine_skeleton_bounds_aabb_contains_point(_ptr.assumingMemoryBound(to: spine_skeleton_bounds_wrapper.self), x, y)
        return result
    }

    /// Returns true if the axis aligned bounding box intersects the line segment.
    public func aabbIntersectsSegment(_ x1: Float, _ y1: Float, _ x2: Float, _ y2: Float) -> Bool {
        let result = spine_skeleton_bounds_aabb_intersects_segment(_ptr.assumingMemoryBound(to: spine_skeleton_bounds_wrapper.self), x1, y1, x2, y2)
        return result
    }

    /// Returns true if the axis aligned bounding box intersects the axis aligned bounding box of
    /// the specified bounds.
    public func aabbIntersectsSkeleton(_ bounds: SkeletonBounds) -> Bool {
        let result = spine_skeleton_bounds_aabb_intersects_skeleton(_ptr.assumingMemoryBound(to: spine_skeleton_bounds_wrapper.self), bounds._ptr.assumingMemoryBound(to: spine_skeleton_bounds_wrapper.self))
        return result
    }

    /// Returns the polygon for the given bounding box attachment or null if no polygon can be found
    /// for the attachment. Requires a call to update() first.
    public func getPolygon(_ attachment: BoundingBoxAttachment?) -> Polygon? {
        let result = spine_skeleton_bounds_get_polygon(_ptr.assumingMemoryBound(to: spine_skeleton_bounds_wrapper.self), attachment?._ptr.assumingMemoryBound(to: spine_bounding_box_attachment_wrapper.self))
        return result.map { Polygon(fromPointer: $0) }
    }

    /// Returns the bounding box for the given polygon or null. Requires a call to update() first.
    public func getBoundingBox(_ polygon: Polygon?) -> BoundingBoxAttachment? {
        let result = spine_skeleton_bounds_get_bounding_box(_ptr.assumingMemoryBound(to: spine_skeleton_bounds_wrapper.self), polygon?._ptr.assumingMemoryBound(to: spine_polygon_wrapper.self))
        return result.map { BoundingBoxAttachment(fromPointer: $0) }
    }

    /// Returns true if the polygon contains the point.
    public func containsPoint(_ polygon: Polygon, _ x: Float, _ y: Float) -> Bool {
        let result = spine_skeleton_bounds_contains_point_1(_ptr.assumingMemoryBound(to: spine_skeleton_bounds_wrapper.self), polygon._ptr.assumingMemoryBound(to: spine_polygon_wrapper.self), x, y)
        return result
    }

    /// Returns the first bounding box attachment that contains the point, or null. When doing many
    /// checks, it is usually more efficient to only call this method if aabbContainsPoint(float,
    /// float) returns true.
    public func containsPoint2(_ x: Float, _ y: Float) -> BoundingBoxAttachment? {
        let result = spine_skeleton_bounds_contains_point_2(_ptr.assumingMemoryBound(to: spine_skeleton_bounds_wrapper.self), x, y)
        return result.map { BoundingBoxAttachment(fromPointer: $0) }
    }

    /// Returns the first bounding box attachment that contains any part of the line segment, or
    /// null. When doing many checks, it is usually more efficient to only call this method if
    /// aabbIntersectsSegment(float, float, float, float) returns true.
    public func intersectsSegment(_ x1: Float, _ y1: Float, _ x2: Float, _ y2: Float) -> BoundingBoxAttachment? {
        let result = spine_skeleton_bounds_intersects_segment_1(_ptr.assumingMemoryBound(to: spine_skeleton_bounds_wrapper.self), x1, y1, x2, y2)
        return result.map { BoundingBoxAttachment(fromPointer: $0) }
    }

    /// Returns true if the polygon contains any part of the line segment.
    public func intersectsSegment2(_ polygon: Polygon, _ x1: Float, _ y1: Float, _ x2: Float, _ y2: Float) -> Bool {
        let result = spine_skeleton_bounds_intersects_segment_2(_ptr.assumingMemoryBound(to: spine_skeleton_bounds_wrapper.self), polygon._ptr.assumingMemoryBound(to: spine_polygon_wrapper.self), x1, y1, x2, y2)
        return result
    }

    public func dispose() {
        spine_skeleton_bounds_dispose(_ptr.assumingMemoryBound(to: spine_skeleton_bounds_wrapper.self))
    }
}