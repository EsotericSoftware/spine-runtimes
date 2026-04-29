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

/// TextureRegion wrapper
@objc(SpineTextureRegion)
@objcMembers
public class TextureRegion: NSObject {
    public let _ptr: UnsafeMutableRawPointer

    public init(fromPointer ptr: spine_texture_region) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        super.init()
    }

    public override convenience init() {
        let ptr = spine_texture_region_create()
        self.init(fromPointer: ptr!)
    }

    public var rtti: Rtti {
        let result = spine_texture_region_get_rtti(_ptr.assumingMemoryBound(to: spine_texture_region_wrapper.self))
        return Rtti(fromPointer: result!)
    }

    public var u: Float {
        get {
            let result = spine_texture_region_get_u(_ptr.assumingMemoryBound(to: spine_texture_region_wrapper.self))
        return result
        }
        set {
            spine_texture_region_set_u(_ptr.assumingMemoryBound(to: spine_texture_region_wrapper.self), newValue)
        }
    }

    public var v: Float {
        get {
            let result = spine_texture_region_get_v(_ptr.assumingMemoryBound(to: spine_texture_region_wrapper.self))
        return result
        }
        set {
            spine_texture_region_set_v(_ptr.assumingMemoryBound(to: spine_texture_region_wrapper.self), newValue)
        }
    }

    public var u2: Float {
        get {
            let result = spine_texture_region_get_u2(_ptr.assumingMemoryBound(to: spine_texture_region_wrapper.self))
        return result
        }
        set {
            spine_texture_region_set_u2(_ptr.assumingMemoryBound(to: spine_texture_region_wrapper.self), newValue)
        }
    }

    public var v2: Float {
        get {
            let result = spine_texture_region_get_v2(_ptr.assumingMemoryBound(to: spine_texture_region_wrapper.self))
        return result
        }
        set {
            spine_texture_region_set_v2(_ptr.assumingMemoryBound(to: spine_texture_region_wrapper.self), newValue)
        }
    }

    public var regionWidth: Int32 {
        get {
            let result = spine_texture_region_get_region_width(_ptr.assumingMemoryBound(to: spine_texture_region_wrapper.self))
        return result
        }
        set {
            spine_texture_region_set_region_width(_ptr.assumingMemoryBound(to: spine_texture_region_wrapper.self), newValue)
        }
    }

    public var regionHeight: Int32 {
        get {
            let result = spine_texture_region_get_region_height(_ptr.assumingMemoryBound(to: spine_texture_region_wrapper.self))
        return result
        }
        set {
            spine_texture_region_set_region_height(_ptr.assumingMemoryBound(to: spine_texture_region_wrapper.self), newValue)
        }
    }

    public var rendererObject: UnsafeMutableRawPointer? {
        let result = spine_texture_region_get_renderer_object(_ptr.assumingMemoryBound(to: spine_texture_region_wrapper.self))
        return result
    }

    public static func rttiStatic() -> Rtti {
        let result = spine_texture_region_rtti()
        return Rtti(fromPointer: result!)
    }

    public func dispose() {
        spine_texture_region_dispose(_ptr.assumingMemoryBound(to: spine_texture_region_wrapper.self))
    }
}