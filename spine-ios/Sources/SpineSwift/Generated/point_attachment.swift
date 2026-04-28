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

/// An attachment which is a single point and a rotation. This can be used to spawn projectiles,
/// particles, etc. A bone can be used in similar ways, but a PointAttachment is slightly less
/// expensive to compute and can be hidden, shown, and placed in a skin.
///
/// See https://esotericsoftware.com/spine-points for Point Attachments in the Spine User Guide.
@objc(SpinePointAttachment)
@objcMembers
public class PointAttachment: Attachment {
    @nonobjc
    public init(fromPointer ptr: spine_point_attachment) {
        super.init(fromPointer: UnsafeMutableRawPointer(ptr).assumingMemoryBound(to: spine_attachment_wrapper.self))
    }

    public convenience init(_ name: String) {
        let ptr = spine_point_attachment_create(name)
        self.init(fromPointer: ptr!)
    }

    /// The local x position.
    public var x: Float {
        get {
            let result = spine_point_attachment_get_x(_ptr.assumingMemoryBound(to: spine_point_attachment_wrapper.self))
            return result
        }
        set {
            spine_point_attachment_set_x(_ptr.assumingMemoryBound(to: spine_point_attachment_wrapper.self), newValue)
        }
    }

    /// The local y position.
    public var y: Float {
        get {
            let result = spine_point_attachment_get_y(_ptr.assumingMemoryBound(to: spine_point_attachment_wrapper.self))
            return result
        }
        set {
            spine_point_attachment_set_y(_ptr.assumingMemoryBound(to: spine_point_attachment_wrapper.self), newValue)
        }
    }

    /// The local rotation in degrees, counter clockwise.
    public var rotation: Float {
        get {
            let result = spine_point_attachment_get_rotation(_ptr.assumingMemoryBound(to: spine_point_attachment_wrapper.self))
            return result
        }
        set {
            spine_point_attachment_set_rotation(_ptr.assumingMemoryBound(to: spine_point_attachment_wrapper.self), newValue)
        }
    }

    public var color: Color {
        let result = spine_point_attachment_get_color(_ptr.assumingMemoryBound(to: spine_point_attachment_wrapper.self))
        return Color(fromPointer: result!)
    }

    /// Computes the world rotation from the local rotation.
    public func computeWorldRotation(_ bone: BonePose) -> Float {
        let result = spine_point_attachment_compute_world_rotation(
            _ptr.assumingMemoryBound(to: spine_point_attachment_wrapper.self), bone._ptr.assumingMemoryBound(to: spine_bone_pose_wrapper.self))
        return result
    }

    public func dispose() {
        spine_point_attachment_dispose(_ptr.assumingMemoryBound(to: spine_point_attachment_wrapper.self))
    }
}
