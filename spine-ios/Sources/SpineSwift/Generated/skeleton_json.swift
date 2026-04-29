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

/// SkeletonJson wrapper
@objc(SpineSkeletonJson)
@objcMembers
public class SkeletonJson: NSObject {
    public let _ptr: UnsafeMutableRawPointer

    public init(fromPointer ptr: spine_skeleton_json) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        super.init()
    }

    public convenience init(_ atlas: Atlas) {
        let ptr = spine_skeleton_json_create(atlas._ptr.assumingMemoryBound(to: spine_atlas_wrapper.self))
        self.init(fromPointer: ptr!)
    }

    public static func variant2(_ attachmentLoader: AttachmentLoader, _ ownsLoader: Bool) -> SkeletonJson {
        let ptr = spine_skeleton_json_create2(attachmentLoader._ptr.assumingMemoryBound(to: spine_attachment_loader_wrapper.self), ownsLoader)
        return SkeletonJson(fromPointer: ptr!)
    }

    public var scale: Float {
        get { fatalError("Setter-only property") }
        set(newValue) {
            spine_skeleton_json_set_scale(_ptr.assumingMemoryBound(to: spine_skeleton_json_wrapper.self), newValue)
        }
    }

    public var error: String {
        let result = spine_skeleton_json_get_error(_ptr.assumingMemoryBound(to: spine_skeleton_json_wrapper.self))
        return String(cString: result!)
    }

    public func readSkeletonDataFile(_ path: String) -> SkeletonData? {
        let result = spine_skeleton_json_read_skeleton_data_file(_ptr.assumingMemoryBound(to: spine_skeleton_json_wrapper.self), path)
        return result.map { SkeletonData(fromPointer: $0) }
    }

    public func readSkeletonData(_ json: String) -> SkeletonData? {
        let result = spine_skeleton_json_read_skeleton_data(_ptr.assumingMemoryBound(to: spine_skeleton_json_wrapper.self), json)
        return result.map { SkeletonData(fromPointer: $0) }
    }

    public func dispose() {
        spine_skeleton_json_dispose(_ptr.assumingMemoryBound(to: spine_skeleton_json_wrapper.self))
    }
}