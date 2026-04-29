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

/// PathAttachment wrapper
@objc(SpinePathAttachment)
@objcMembers
public class PathAttachment: VertexAttachment {
    @nonobjc
    public init(fromPointer ptr: spine_path_attachment) {
        super.init(fromPointer: UnsafeMutableRawPointer(ptr).assumingMemoryBound(to: spine_vertex_attachment_wrapper.self))
    }

    public convenience init(_ name: String) {
        let ptr = spine_path_attachment_create(name)
        self.init(fromPointer: ptr!)
    }

    /// The length in the setup pose from the start of the path to the end of each curve.
    public var lengths: ArrayFloat {
        get {
            let result = spine_path_attachment_get_lengths(_ptr.assumingMemoryBound(to: spine_path_attachment_wrapper.self))
        return ArrayFloat(fromPointer: result!)
        }
        set {
            spine_path_attachment_set_lengths(_ptr.assumingMemoryBound(to: spine_path_attachment_wrapper.self), newValue._ptr.assumingMemoryBound(to: spine_array_float_wrapper.self))
        }
    }

    public var closed: Bool {
        get {
            let result = spine_path_attachment_get_closed(_ptr.assumingMemoryBound(to: spine_path_attachment_wrapper.self))
        return result
        }
        set {
            spine_path_attachment_set_closed(_ptr.assumingMemoryBound(to: spine_path_attachment_wrapper.self), newValue)
        }
    }

    /// If true, additional calculations are performed to make computing positions along the path
    /// more accurate so movement along the path has a constant speed.
    public var constantSpeed: Bool {
        get {
            let result = spine_path_attachment_get_constant_speed(_ptr.assumingMemoryBound(to: spine_path_attachment_wrapper.self))
        return result
        }
        set {
            spine_path_attachment_set_constant_speed(_ptr.assumingMemoryBound(to: spine_path_attachment_wrapper.self), newValue)
        }
    }

    public var color: Color {
        let result = spine_path_attachment_get_color(_ptr.assumingMemoryBound(to: spine_path_attachment_wrapper.self))
        return Color(fromPointer: result!)
    }

    public func dispose() {
        spine_path_attachment_dispose(_ptr.assumingMemoryBound(to: spine_path_attachment_wrapper.self))
    }
}