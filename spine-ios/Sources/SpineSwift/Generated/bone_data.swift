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

/// BoneData wrapper
@objc(SpineBoneData)
@objcMembers
public class BoneData: PosedData {
    @nonobjc
    public init(fromPointer ptr: spine_bone_data) {
        super.init(fromPointer: UnsafeMutableRawPointer(ptr).assumingMemoryBound(to: spine_posed_data_wrapper.self))
    }

    public convenience init(_ index: Int32, _ name: String, _ parent: BoneData?) {
        let ptr = spine_bone_data_create(index, name, parent?._ptr.assumingMemoryBound(to: spine_bone_data_wrapper.self))
        self.init(fromPointer: ptr!)
    }

    /// The Skeleton::getBones() index for this bone.
    public var index: Int32 {
        let result = spine_bone_data_get_index(_ptr.assumingMemoryBound(to: spine_bone_data_wrapper.self))
        return result
    }

    /// The parent bone, or NULL if this bone is the root.
    public var parent: BoneData? {
        let result = spine_bone_data_get_parent(_ptr.assumingMemoryBound(to: spine_bone_data_wrapper.self))
        return result.map { BoneData(fromPointer: $0) }
    }

    public var length: Float {
        get {
            let result = spine_bone_data_get_length(_ptr.assumingMemoryBound(to: spine_bone_data_wrapper.self))
        return result
        }
        set {
            spine_bone_data_set_length(_ptr.assumingMemoryBound(to: spine_bone_data_wrapper.self), newValue)
        }
    }

    public var color: Color {
        let result = spine_bone_data_get_color(_ptr.assumingMemoryBound(to: spine_bone_data_wrapper.self))
        return Color(fromPointer: result!)
    }

    /// The bone icon name as it was in Spine, or empty if nonessential data was not exported.
    public var icon: String {
        get {
            let result = spine_bone_data_get_icon(_ptr.assumingMemoryBound(to: spine_bone_data_wrapper.self))
        return String(cString: result!)
        }
        set {
            spine_bone_data_set_icon(_ptr.assumingMemoryBound(to: spine_bone_data_wrapper.self), newValue)
        }
    }

    /// The bone icon's display size scale, or 1 if nonessential data was not exported.
    public var iconSize: Float {
        get {
            let result = spine_bone_data_get_icon_size(_ptr.assumingMemoryBound(to: spine_bone_data_wrapper.self))
        return result
        }
        set {
            spine_bone_data_set_icon_size(_ptr.assumingMemoryBound(to: spine_bone_data_wrapper.self), newValue)
        }
    }

    /// The bone icon's display rotation in degrees, or 0 if nonessential data was not exported.
    public var iconRotation: Float {
        get {
            let result = spine_bone_data_get_icon_rotation(_ptr.assumingMemoryBound(to: spine_bone_data_wrapper.self))
        return result
        }
        set {
            spine_bone_data_set_icon_rotation(_ptr.assumingMemoryBound(to: spine_bone_data_wrapper.self), newValue)
        }
    }

    public var visible: Bool {
        get {
            let result = spine_bone_data_get_visible(_ptr.assumingMemoryBound(to: spine_bone_data_wrapper.self))
        return result
        }
        set {
            spine_bone_data_set_visible(_ptr.assumingMemoryBound(to: spine_bone_data_wrapper.self), newValue)
        }
    }

    /// The setup pose that most animations are relative to.
    public var setupPose: BonePose {
        let result = spine_bone_data_get_setup_pose(_ptr.assumingMemoryBound(to: spine_bone_data_wrapper.self))
        return BonePose(fromPointer: result!)
    }

    public override func dispose() {
        spine_bone_data_dispose(_ptr.assumingMemoryBound(to: spine_bone_data_wrapper.self))
    }
}