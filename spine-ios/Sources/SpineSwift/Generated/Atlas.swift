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

/// Atlas wrapper
@objc(SpineAtlas)
@objcMembers
public class Atlas: NSObject {
    public let _ptr: UnsafeMutableRawPointer

    public init(fromPointer ptr: spine_atlas) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        super.init()
    }

    public var pages: ArrayAtlasPage {
        let result = spine_atlas_get_pages(_ptr.assumingMemoryBound(to: spine_atlas_wrapper.self))
        return ArrayAtlasPage(fromPointer: result!)
    }

    public var regions: ArrayAtlasRegion {
        let result = spine_atlas_get_regions(_ptr.assumingMemoryBound(to: spine_atlas_wrapper.self))
        return ArrayAtlasRegion(fromPointer: result!)
    }

    public func flipV() {
        spine_atlas_flip_v(_ptr.assumingMemoryBound(to: spine_atlas_wrapper.self))
    }

    /// Returns the first region found with the specified name. This method uses String comparison
    /// to find the region, so the result should be cached rather than calling this method multiple
    /// times.
    ///
    /// - Returns: The region, or nullptr.
    public func findRegion(_ name: String) -> AtlasRegion? {
        let result = spine_atlas_find_region(_ptr.assumingMemoryBound(to: spine_atlas_wrapper.self), name)
        return result.map { AtlasRegion(fromPointer: $0) }
    }

    public func dispose() {
        spine_atlas_dispose(_ptr.assumingMemoryBound(to: spine_atlas_wrapper.self))
    }
}