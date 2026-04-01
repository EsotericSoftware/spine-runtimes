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

/// Holds texture regions, UVs, and vertex offsets for rendering a region or mesh attachment.
/// Regions must be populated and update() called before use.
@objc(SpineSequence)
@objcMembers
public class Sequence: NSObject {
    public let _ptr: UnsafeMutableRawPointer

    public init(fromPointer ptr: spine_sequence) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        super.init()
    }

    /// - Parameter count: The number of texture regions this sequence will display.
    /// - Parameter pathSuffix: If true, getPath(String, int) has a numeric suffix. If false, all regions will use the same path, so count should be 1.
    public convenience init(_ count: Int32, _ pathSuffix: Bool) {
        let ptr = spine_sequence_create(count, pathSuffix)
        self.init(fromPointer: ptr!)
    }

    /// Copy constructor.
    public static func from(_ other: Sequence) -> Sequence {
        let ptr = spine_sequence_create2(other._ptr.assumingMemoryBound(to: spine_sequence_wrapper.self))
        return Sequence(fromPointer: ptr!)
    }

    /// The list of texture regions this sequence will display.
    public var regions: ArrayTextureRegion {
        let result = spine_sequence_get_regions(_ptr.assumingMemoryBound(to: spine_sequence_wrapper.self))
        return ArrayTextureRegion(fromPointer: result!)
    }

    /// The starting number for the numeric getPath(String, int) suffix.
    public var start: Int32 {
        get {
            let result = spine_sequence_get_start(_ptr.assumingMemoryBound(to: spine_sequence_wrapper.self))
            return result
        }
        set {
            spine_sequence_set_start(_ptr.assumingMemoryBound(to: spine_sequence_wrapper.self), newValue)
        }
    }

    /// The minimum number of digits in the numeric getPath(String, int) suffix, for zero padding. 0
    /// for no zero padding.
    public var digits: Int32 {
        get {
            let result = spine_sequence_get_digits(_ptr.assumingMemoryBound(to: spine_sequence_wrapper.self))
            return result
        }
        set {
            spine_sequence_set_digits(_ptr.assumingMemoryBound(to: spine_sequence_wrapper.self), newValue)
        }
    }

    /// The index of the region to show for the setup pose.
    public var setupIndex: Int32 {
        get {
            let result = spine_sequence_get_setup_index(_ptr.assumingMemoryBound(to: spine_sequence_wrapper.self))
            return result
        }
        set {
            spine_sequence_set_setup_index(_ptr.assumingMemoryBound(to: spine_sequence_wrapper.self), newValue)
        }
    }

    /// Returns true if getPath(String, int) has a numeric suffix.
    public var hasPathSuffix: Bool {
        let result = spine_sequence_has_path_suffix(_ptr.assumingMemoryBound(to: spine_sequence_wrapper.self))
        return result
    }

    /// Returns a unique ID for this sequence.
    public var id: Int32 {
        let result = spine_sequence_get_id(_ptr.assumingMemoryBound(to: spine_sequence_wrapper.self))
        return result
    }

    /// Returns the getRegions() index for SlotPose::getSequenceIndex().
    public func resolveIndex(_ pose: SlotPose) -> Int32 {
        let result = spine_sequence_resolve_index(
            _ptr.assumingMemoryBound(to: spine_sequence_wrapper.self), pose._ptr.assumingMemoryBound(to: spine_slot_pose_wrapper.self))
        return result
    }

    /// Returns the texture region from getRegions() for the specified index.
    public func getRegion(_ index: Int32) -> TextureRegion? {
        let result = spine_sequence_get_region(_ptr.assumingMemoryBound(to: spine_sequence_wrapper.self), index)
        return result.map { TextureRegion(fromPointer: $0) }
    }

    /// Returns the UVs for the specified index. getRegions() must be populated and update() called
    /// before calling this method.
    public func getUVs(_ index: Int32) -> ArrayFloat {
        let result = spine_sequence_get_u_vs(_ptr.assumingMemoryBound(to: spine_sequence_wrapper.self), index)
        return ArrayFloat(fromPointer: result!)
    }

    /// Returns vertex offsets from the center of a RegionAttachment. Invalid to call for a
    /// MeshAttachment.
    public func getOffsets(_ index: Int32) -> ArrayFloat {
        let result = spine_sequence_get_offsets(_ptr.assumingMemoryBound(to: spine_sequence_wrapper.self), index)
        return ArrayFloat(fromPointer: result!)
    }

    /// Returns the specified base path with an optional numeric suffix for the specified index.
    public func getPath(_ basePath: String, _ index: Int32) -> String {
        let result = spine_sequence_get_path(_ptr.assumingMemoryBound(to: spine_sequence_wrapper.self), basePath, index)
        return String(cString: result!)
    }

    /// Computes UVs and offsets for the specified attachment. Must be called if the regions or
    /// attachment properties are changed.
    public func update(_ attachment: RegionAttachment) {
        spine_sequence_update_1(
            _ptr.assumingMemoryBound(to: spine_sequence_wrapper.self), attachment._ptr.assumingMemoryBound(to: spine_region_attachment_wrapper.self))
    }

    public func update2(_ attachment: MeshAttachment) {
        spine_sequence_update_2(
            _ptr.assumingMemoryBound(to: spine_sequence_wrapper.self), attachment._ptr.assumingMemoryBound(to: spine_mesh_attachment_wrapper.self))
    }

    public func dispose() {
        spine_sequence_dispose(_ptr.assumingMemoryBound(to: spine_sequence_wrapper.self))
    }
}
