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

/// Changes a slot's deform to deform a VertexAttachment.
@objc(SpineDeformTimeline)
@objcMembers
public class DeformTimeline: SlotCurveTimeline {
    @nonobjc
    public init(fromPointer ptr: spine_deform_timeline) {
        super.init(fromPointer: UnsafeMutableRawPointer(ptr).assumingMemoryBound(to: spine_slot_curve_timeline_wrapper.self))
    }

    public convenience init(_ frameCount: Int, _ bezierCount: Int, _ slotIndex: Int32, _ attachment: VertexAttachment) {
        let ptr = spine_deform_timeline_create(frameCount, bezierCount, slotIndex, attachment._ptr.assumingMemoryBound(to: spine_vertex_attachment_wrapper.self))
        self.init(fromPointer: ptr!)
    }

    /// The attachment whose vertices will be deformed.
    public var attachment: VertexAttachment {
        get {
            let result = spine_deform_timeline_get_attachment(_ptr.assumingMemoryBound(to: spine_deform_timeline_wrapper.self))
        let rtti = spine_vertex_attachment_get_rtti(result!)
        let rttiClassName = String(cString: spine_rtti_get_class_name(rtti)!)
        switch rttiClassName {
        case "BoundingBoxAttachment":
            let castedPtr = spine_vertex_attachment_cast_to_bounding_box_attachment(result!)
            return BoundingBoxAttachment(fromPointer: castedPtr!)
        case "ClippingAttachment":
            let castedPtr = spine_vertex_attachment_cast_to_clipping_attachment(result!)
            return ClippingAttachment(fromPointer: castedPtr!)
        case "MeshAttachment":
            let castedPtr = spine_vertex_attachment_cast_to_mesh_attachment(result!)
            return MeshAttachment(fromPointer: castedPtr!)
        case "PathAttachment":
            let castedPtr = spine_vertex_attachment_cast_to_path_attachment(result!)
            return PathAttachment(fromPointer: castedPtr!)
        default:
            fatalError("Unknown concrete type: \(rttiClassName) for abstract class VertexAttachment")
        }
        }
        set {
            spine_deform_timeline_set_attachment(_ptr.assumingMemoryBound(to: spine_deform_timeline_wrapper.self), newValue._ptr.assumingMemoryBound(to: spine_vertex_attachment_wrapper.self))
        }
    }

    /// Sets the time and vertices for the specified frame.
    public func setFrame(_ frameIndex: Int32, _ time: Float, _ vertices: ArrayFloat) {
        spine_deform_timeline_set_frame(_ptr.assumingMemoryBound(to: spine_deform_timeline_wrapper.self), frameIndex, time, vertices._ptr.assumingMemoryBound(to: spine_array_float_wrapper.self))
    }

    public func getCurvePercent(_ time: Float, _ frame: Int32) -> Float {
        let result = spine_deform_timeline_get_curve_percent(_ptr.assumingMemoryBound(to: spine_deform_timeline_wrapper.self), time, frame)
        return result
    }

    public func dispose() {
        spine_deform_timeline_dispose(_ptr.assumingMemoryBound(to: spine_deform_timeline_wrapper.self))
    }
}