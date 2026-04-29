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

/// Constrained property for a TransformConstraint.
@objc(SpineToProperty)
@objcMembers
open class ToProperty: NSObject {
    public let _ptr: UnsafeMutableRawPointer

    public init(fromPointer ptr: spine_to_property) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        super.init()
    }

    public var rtti: Rtti {
        let result = spine_to_property_get_rtti(_ptr.assumingMemoryBound(to: spine_to_property_wrapper.self))
        return Rtti(fromPointer: result!)
    }

    public var offset: Float {
        get {
            let result = spine_to_property_get__offset(_ptr.assumingMemoryBound(to: spine_to_property_wrapper.self))
        return result
        }
        set {
            spine_to_property_set__offset(_ptr.assumingMemoryBound(to: spine_to_property_wrapper.self), newValue)
        }
    }

    public var max: Float {
        get {
            let result = spine_to_property_get__max(_ptr.assumingMemoryBound(to: spine_to_property_wrapper.self))
        return result
        }
        set {
            spine_to_property_set__max(_ptr.assumingMemoryBound(to: spine_to_property_wrapper.self), newValue)
        }
    }

    public var scale: Float {
        get {
            let result = spine_to_property_get__scale(_ptr.assumingMemoryBound(to: spine_to_property_wrapper.self))
        return result
        }
        set {
            spine_to_property_set__scale(_ptr.assumingMemoryBound(to: spine_to_property_wrapper.self), newValue)
        }
    }

    /// Reads the mix for this property from the specified pose.
    public func mix(_ pose: TransformConstraintPose) -> Float {
        let result = spine_to_property_mix(_ptr.assumingMemoryBound(to: spine_to_property_wrapper.self), pose._ptr.assumingMemoryBound(to: spine_transform_constraint_pose_wrapper.self))
        return result
    }

    /// Applies the value to this property.
    public func apply(_ skeleton: Skeleton, _ pose: TransformConstraintPose, _ bone: BonePose, _ value: Float, _ local: Bool, _ additive: Bool) {
        spine_to_property_apply(_ptr.assumingMemoryBound(to: spine_to_property_wrapper.self), skeleton._ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), pose._ptr.assumingMemoryBound(to: spine_transform_constraint_pose_wrapper.self), bone._ptr.assumingMemoryBound(to: spine_bone_pose_wrapper.self), value, local, additive)
    }

    public static func rttiStatic() -> Rtti {
        let result = spine_to_property_rtti()
        return Rtti(fromPointer: result!)
    }

}