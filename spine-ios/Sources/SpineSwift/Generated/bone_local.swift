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

/// Stores a bone's local pose.
@objc(SpineBoneLocal)
@objcMembers
public class BoneLocal: NSObject {
    public let _ptr: UnsafeMutableRawPointer

    public init(fromPointer ptr: spine_bone_local) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        super.init()
    }

    public override convenience init() {
        let ptr = spine_bone_local_create()
        self.init(fromPointer: ptr!)
    }

    /// The local x translation.
    public var x: Float {
        get {
            let result = spine_bone_local_get_x(_ptr.assumingMemoryBound(to: spine_bone_local_wrapper.self))
        return result
        }
        set {
            spine_bone_local_set_x(_ptr.assumingMemoryBound(to: spine_bone_local_wrapper.self), newValue)
        }
    }

    /// The local y translation.
    public var y: Float {
        get {
            let result = spine_bone_local_get_y(_ptr.assumingMemoryBound(to: spine_bone_local_wrapper.self))
        return result
        }
        set {
            spine_bone_local_set_y(_ptr.assumingMemoryBound(to: spine_bone_local_wrapper.self), newValue)
        }
    }

    /// The local rotation in degrees, counter clockwise.
    public var rotation: Float {
        get {
            let result = spine_bone_local_get_rotation(_ptr.assumingMemoryBound(to: spine_bone_local_wrapper.self))
        return result
        }
        set {
            spine_bone_local_set_rotation(_ptr.assumingMemoryBound(to: spine_bone_local_wrapper.self), newValue)
        }
    }

    /// The local scaleX.
    public var scaleX: Float {
        get {
            let result = spine_bone_local_get_scale_x(_ptr.assumingMemoryBound(to: spine_bone_local_wrapper.self))
        return result
        }
        set {
            spine_bone_local_set_scale_x(_ptr.assumingMemoryBound(to: spine_bone_local_wrapper.self), newValue)
        }
    }

    /// The local scaleY.
    public var scaleY: Float {
        get {
            let result = spine_bone_local_get_scale_y(_ptr.assumingMemoryBound(to: spine_bone_local_wrapper.self))
        return result
        }
        set {
            spine_bone_local_set_scale_y(_ptr.assumingMemoryBound(to: spine_bone_local_wrapper.self), newValue)
        }
    }

    /// The local shearX.
    public var shearX: Float {
        get {
            let result = spine_bone_local_get_shear_x(_ptr.assumingMemoryBound(to: spine_bone_local_wrapper.self))
        return result
        }
        set {
            spine_bone_local_set_shear_x(_ptr.assumingMemoryBound(to: spine_bone_local_wrapper.self), newValue)
        }
    }

    /// The local shearY.
    public var shearY: Float {
        get {
            let result = spine_bone_local_get_shear_y(_ptr.assumingMemoryBound(to: spine_bone_local_wrapper.self))
        return result
        }
        set {
            spine_bone_local_set_shear_y(_ptr.assumingMemoryBound(to: spine_bone_local_wrapper.self), newValue)
        }
    }

    /// Determines how parent world transforms affect this bone.
    public var inherit: Inherit {
        get {
            let result = spine_bone_local_get_inherit(_ptr.assumingMemoryBound(to: spine_bone_local_wrapper.self))
        return Inherit(rawValue: Int32(result.rawValue))!
        }
        set {
            spine_bone_local_set_inherit(_ptr.assumingMemoryBound(to: spine_bone_local_wrapper.self), spine_inherit(rawValue: UInt32(newValue.rawValue)))
        }
    }

    /// Sets local scaleX and scaleY to the same value.
    public var scale2: Float {
        get { fatalError("Setter-only property") }
        set(newValue) {
            spine_bone_local_set_scale_2(_ptr.assumingMemoryBound(to: spine_bone_local_wrapper.self), newValue)
        }
    }

    public func set(_ pose: BoneLocal) {
        spine_bone_local_set(_ptr.assumingMemoryBound(to: spine_bone_local_wrapper.self), pose._ptr.assumingMemoryBound(to: spine_bone_local_wrapper.self))
    }

    /// Sets local x and y translation.
    public func setPosition(_ x: Float, _ y: Float) {
        spine_bone_local_set_position(_ptr.assumingMemoryBound(to: spine_bone_local_wrapper.self), x, y)
    }

    /// Sets local scaleX and scaleY.
    public func setScale(_ scaleX: Float, _ scaleY: Float) {
        spine_bone_local_set_scale_1(_ptr.assumingMemoryBound(to: spine_bone_local_wrapper.self), scaleX, scaleY)
    }

    public func dispose() {
        spine_bone_local_dispose(_ptr.assumingMemoryBound(to: spine_bone_local_wrapper.self))
    }
}