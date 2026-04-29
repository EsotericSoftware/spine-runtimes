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

/// RenderCommand wrapper
@objc(SpineRenderCommand)
@objcMembers
public class RenderCommand: NSObject {
    public let _ptr: UnsafeMutableRawPointer

    public init(fromPointer ptr: spine_render_command) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        super.init()
    }

    public var positions: UnsafeMutablePointer<Float>? {
        let result = spine_render_command_get_positions(_ptr.assumingMemoryBound(to: spine_render_command_wrapper.self))
        return result
    }

    public var uvs: UnsafeMutablePointer<Float>? {
        let result = spine_render_command_get_uvs(_ptr.assumingMemoryBound(to: spine_render_command_wrapper.self))
        return result
    }

    public var colors: UnsafeMutablePointer<UInt32>? {
        let result = spine_render_command_get_colors(_ptr.assumingMemoryBound(to: spine_render_command_wrapper.self))
        return result
    }

    public var darkColors: UnsafeMutablePointer<UInt32>? {
        let result = spine_render_command_get_dark_colors(_ptr.assumingMemoryBound(to: spine_render_command_wrapper.self))
        return result
    }

    public var numVertices: Int32 {
        let result = spine_render_command_get_num_vertices(_ptr.assumingMemoryBound(to: spine_render_command_wrapper.self))
        return result
    }

    public var indices: UnsafeMutablePointer<UInt16>? {
        let result = spine_render_command_get_indices(_ptr.assumingMemoryBound(to: spine_render_command_wrapper.self))
        return result
    }

    public var numIndices: Int32 {
        let result = spine_render_command_get_num_indices(_ptr.assumingMemoryBound(to: spine_render_command_wrapper.self))
        return result
    }

    public var blendMode: BlendMode {
        let result = spine_render_command_get_blend_mode(_ptr.assumingMemoryBound(to: spine_render_command_wrapper.self))
        return BlendMode(rawValue: Int32(result.rawValue))!
    }

    public var texture: UnsafeMutableRawPointer? {
        let result = spine_render_command_get_texture(_ptr.assumingMemoryBound(to: spine_render_command_wrapper.self))
        return result
    }

    public var next: RenderCommand? {
        let result = spine_render_command_get_next(_ptr.assumingMemoryBound(to: spine_render_command_wrapper.self))
        return result.map { RenderCommand(fromPointer: $0) }
    }

    public func dispose() {
        spine_render_command_dispose(_ptr.assumingMemoryBound(to: spine_render_command_wrapper.self))
    }
}