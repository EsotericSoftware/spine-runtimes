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

/// Stores a pose for a path constraint.
@objc(SpinePathConstraintPose)
@objcMembers
public class PathConstraintPose: NSObject {
    public let _ptr: UnsafeMutableRawPointer

    public init(fromPointer ptr: spine_path_constraint_pose) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        super.init()
    }

    public override convenience init() {
        let ptr = spine_path_constraint_pose_create()
        self.init(fromPointer: ptr!)
    }

    /// The position along the path.
    public var position: Float {
        get {
            let result = spine_path_constraint_pose_get_position(_ptr.assumingMemoryBound(to: spine_path_constraint_pose_wrapper.self))
        return result
        }
        set {
            spine_path_constraint_pose_set_position(_ptr.assumingMemoryBound(to: spine_path_constraint_pose_wrapper.self), newValue)
        }
    }

    /// The spacing between bones.
    public var spacing: Float {
        get {
            let result = spine_path_constraint_pose_get_spacing(_ptr.assumingMemoryBound(to: spine_path_constraint_pose_wrapper.self))
        return result
        }
        set {
            spine_path_constraint_pose_set_spacing(_ptr.assumingMemoryBound(to: spine_path_constraint_pose_wrapper.self), newValue)
        }
    }

    /// A percentage (0-1) that controls the mix between the constrained and unconstrained rotation.
    public var mixRotate: Float {
        get {
            let result = spine_path_constraint_pose_get_mix_rotate(_ptr.assumingMemoryBound(to: spine_path_constraint_pose_wrapper.self))
        return result
        }
        set {
            spine_path_constraint_pose_set_mix_rotate(_ptr.assumingMemoryBound(to: spine_path_constraint_pose_wrapper.self), newValue)
        }
    }

    /// A percentage (0-1) that controls the mix between the constrained and unconstrained
    /// translation X.
    public var mixX: Float {
        get {
            let result = spine_path_constraint_pose_get_mix_x(_ptr.assumingMemoryBound(to: spine_path_constraint_pose_wrapper.self))
        return result
        }
        set {
            spine_path_constraint_pose_set_mix_x(_ptr.assumingMemoryBound(to: spine_path_constraint_pose_wrapper.self), newValue)
        }
    }

    /// A percentage (0-1) that controls the mix between the constrained and unconstrained
    /// translation Y.
    public var mixY: Float {
        get {
            let result = spine_path_constraint_pose_get_mix_y(_ptr.assumingMemoryBound(to: spine_path_constraint_pose_wrapper.self))
        return result
        }
        set {
            spine_path_constraint_pose_set_mix_y(_ptr.assumingMemoryBound(to: spine_path_constraint_pose_wrapper.self), newValue)
        }
    }

    public func set(_ pose: PathConstraintPose) {
        spine_path_constraint_pose_set(_ptr.assumingMemoryBound(to: spine_path_constraint_pose_wrapper.self), pose._ptr.assumingMemoryBound(to: spine_path_constraint_pose_wrapper.self))
    }

    public func dispose() {
        spine_path_constraint_pose_dispose(_ptr.assumingMemoryBound(to: spine_path_constraint_pose_wrapper.self))
    }
}