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

/// AtlasRegion wrapper
@objc(SpineAtlasRegion)
@objcMembers
public class AtlasRegion: TextureRegion {
    @nonobjc
    public init(fromPointer ptr: spine_atlas_region) {
        super.init(fromPointer: UnsafeMutableRawPointer(ptr).assumingMemoryBound(to: spine_texture_region_wrapper.self))
    }

    public convenience init() {
        let ptr = spine_atlas_region_create()
        self.init(fromPointer: ptr!)
    }

    public var page: AtlasPage? {
        get {
            let result = spine_atlas_region_get_page(_ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self))
            return result.map { AtlasPage(fromPointer: $0) }
        }
        set {
            spine_atlas_region_set_page(
                _ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self), newValue?._ptr.assumingMemoryBound(to: spine_atlas_page_wrapper.self))
        }
    }

    public var name: String {
        get {
            let result = spine_atlas_region_get_name(_ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self))
            return String(cString: result!)
        }
        set {
            spine_atlas_region_set_name(_ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self), newValue)
        }
    }

    public var index: Int32 {
        get {
            let result = spine_atlas_region_get_index(_ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self))
            return result
        }
        set {
            spine_atlas_region_set_index(_ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self), newValue)
        }
    }

    public var x: Int32 {
        get {
            let result = spine_atlas_region_get_x(_ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self))
            return result
        }
        set {
            spine_atlas_region_set_x(_ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self), newValue)
        }
    }

    public var y: Int32 {
        get {
            let result = spine_atlas_region_get_y(_ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self))
            return result
        }
        set {
            spine_atlas_region_set_y(_ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self), newValue)
        }
    }

    public var offsetX: Float {
        get {
            let result = spine_atlas_region_get_offset_x(_ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self))
            return result
        }
        set {
            spine_atlas_region_set_offset_x(_ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self), newValue)
        }
    }

    public var offsetY: Float {
        get {
            let result = spine_atlas_region_get_offset_y(_ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self))
            return result
        }
        set {
            spine_atlas_region_set_offset_y(_ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self), newValue)
        }
    }

    public var packedWidth: Int32 {
        get {
            let result = spine_atlas_region_get_packed_width(_ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self))
            return result
        }
        set {
            spine_atlas_region_set_packed_width(_ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self), newValue)
        }
    }

    public var packedHeight: Int32 {
        get {
            let result = spine_atlas_region_get_packed_height(_ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self))
            return result
        }
        set {
            spine_atlas_region_set_packed_height(_ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self), newValue)
        }
    }

    public var originalWidth: Int32 {
        get {
            let result = spine_atlas_region_get_original_width(_ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self))
            return result
        }
        set {
            spine_atlas_region_set_original_width(_ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self), newValue)
        }
    }

    public var originalHeight: Int32 {
        get {
            let result = spine_atlas_region_get_original_height(_ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self))
            return result
        }
        set {
            spine_atlas_region_set_original_height(_ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self), newValue)
        }
    }

    public var rotate: Bool {
        get {
            let result = spine_atlas_region_get_rotate(_ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self))
            return result
        }
        set {
            spine_atlas_region_set_rotate(_ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self), newValue)
        }
    }

    public var degrees: Int32 {
        get {
            let result = spine_atlas_region_get_degrees(_ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self))
            return result
        }
        set {
            spine_atlas_region_set_degrees(_ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self), newValue)
        }
    }

    public var splits: ArrayInt {
        get {
            let result = spine_atlas_region_get_splits(_ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self))
            return ArrayInt(fromPointer: result!)
        }
        set {
            spine_atlas_region_set_splits(
                _ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self), newValue._ptr.assumingMemoryBound(to: spine_array_int_wrapper.self))
        }
    }

    public var pads: ArrayInt {
        get {
            let result = spine_atlas_region_get_pads(_ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self))
            return ArrayInt(fromPointer: result!)
        }
        set {
            spine_atlas_region_set_pads(
                _ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self), newValue._ptr.assumingMemoryBound(to: spine_array_int_wrapper.self))
        }
    }

    public var values: ArrayFloat {
        get {
            let result = spine_atlas_region_get_values(_ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self))
            return ArrayFloat(fromPointer: result!)
        }
        set {
            spine_atlas_region_set_values(
                _ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self), newValue._ptr.assumingMemoryBound(to: spine_array_float_wrapper.self))
        }
    }

    public override func dispose() {
        spine_atlas_region_dispose(_ptr.assumingMemoryBound(to: spine_atlas_region_wrapper.self))
    }
}
