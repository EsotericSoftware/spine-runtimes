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

/// AtlasPage wrapper
@objc(SpineAtlasPage)
@objcMembers
public class AtlasPage: NSObject {
    public let _ptr: UnsafeMutableRawPointer

    public init(fromPointer ptr: spine_atlas_page) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        super.init()
    }

    public convenience init(_ inName: String) {
        let ptr = spine_atlas_page_create(inName)
        self.init(fromPointer: ptr!)
    }

    public var name: String {
        get {
            let result = spine_atlas_page_get_name(_ptr.assumingMemoryBound(to: spine_atlas_page_wrapper.self))
            return String(cString: result!)
        }
        set {
            spine_atlas_page_set_name(_ptr.assumingMemoryBound(to: spine_atlas_page_wrapper.self), newValue)
        }
    }

    public var texturePath: String {
        get {
            let result = spine_atlas_page_get_texture_path(_ptr.assumingMemoryBound(to: spine_atlas_page_wrapper.self))
            return String(cString: result!)
        }
        set {
            spine_atlas_page_set_texture_path(_ptr.assumingMemoryBound(to: spine_atlas_page_wrapper.self), newValue)
        }
    }

    public var format: Format {
        get {
            let result = spine_atlas_page_get_format(_ptr.assumingMemoryBound(to: spine_atlas_page_wrapper.self))
            return Format(rawValue: Int32(result.rawValue))!
        }
        set {
            spine_atlas_page_set_format(
                _ptr.assumingMemoryBound(to: spine_atlas_page_wrapper.self), spine_format(rawValue: UInt32(newValue.rawValue)))
        }
    }

    public var minFilter: TextureFilter {
        get {
            let result = spine_atlas_page_get_min_filter(_ptr.assumingMemoryBound(to: spine_atlas_page_wrapper.self))
            return TextureFilter(rawValue: Int32(result.rawValue))!
        }
        set {
            spine_atlas_page_set_min_filter(
                _ptr.assumingMemoryBound(to: spine_atlas_page_wrapper.self), spine_texture_filter(rawValue: UInt32(newValue.rawValue)))
        }
    }

    public var magFilter: TextureFilter {
        get {
            let result = spine_atlas_page_get_mag_filter(_ptr.assumingMemoryBound(to: spine_atlas_page_wrapper.self))
            return TextureFilter(rawValue: Int32(result.rawValue))!
        }
        set {
            spine_atlas_page_set_mag_filter(
                _ptr.assumingMemoryBound(to: spine_atlas_page_wrapper.self), spine_texture_filter(rawValue: UInt32(newValue.rawValue)))
        }
    }

    public var uWrap: TextureWrap {
        get {
            let result = spine_atlas_page_get_u_wrap(_ptr.assumingMemoryBound(to: spine_atlas_page_wrapper.self))
            return TextureWrap(rawValue: Int32(result.rawValue))!
        }
        set {
            spine_atlas_page_set_u_wrap(
                _ptr.assumingMemoryBound(to: spine_atlas_page_wrapper.self), spine_texture_wrap(rawValue: UInt32(newValue.rawValue)))
        }
    }

    public var vWrap: TextureWrap {
        get {
            let result = spine_atlas_page_get_v_wrap(_ptr.assumingMemoryBound(to: spine_atlas_page_wrapper.self))
            return TextureWrap(rawValue: Int32(result.rawValue))!
        }
        set {
            spine_atlas_page_set_v_wrap(
                _ptr.assumingMemoryBound(to: spine_atlas_page_wrapper.self), spine_texture_wrap(rawValue: UInt32(newValue.rawValue)))
        }
    }

    public var width: Int32 {
        get {
            let result = spine_atlas_page_get_width(_ptr.assumingMemoryBound(to: spine_atlas_page_wrapper.self))
            return result
        }
        set {
            spine_atlas_page_set_width(_ptr.assumingMemoryBound(to: spine_atlas_page_wrapper.self), newValue)
        }
    }

    public var height: Int32 {
        get {
            let result = spine_atlas_page_get_height(_ptr.assumingMemoryBound(to: spine_atlas_page_wrapper.self))
            return result
        }
        set {
            spine_atlas_page_set_height(_ptr.assumingMemoryBound(to: spine_atlas_page_wrapper.self), newValue)
        }
    }

    public var pma: Bool {
        get {
            let result = spine_atlas_page_get_pma(_ptr.assumingMemoryBound(to: spine_atlas_page_wrapper.self))
            return result
        }
        set {
            spine_atlas_page_set_pma(_ptr.assumingMemoryBound(to: spine_atlas_page_wrapper.self), newValue)
        }
    }

    public var index: Int32 {
        get {
            let result = spine_atlas_page_get_index(_ptr.assumingMemoryBound(to: spine_atlas_page_wrapper.self))
            return result
        }
        set {
            spine_atlas_page_set_index(_ptr.assumingMemoryBound(to: spine_atlas_page_wrapper.self), newValue)
        }
    }

    public var texture: UnsafeMutableRawPointer? {
        let result = spine_atlas_page_get_texture(_ptr.assumingMemoryBound(to: spine_atlas_page_wrapper.self))
        return result
    }

    public func dispose() {
        spine_atlas_page_dispose(_ptr.assumingMemoryBound(to: spine_atlas_page_wrapper.self))
    }
}
