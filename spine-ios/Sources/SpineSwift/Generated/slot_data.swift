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

/// Stores the setup pose for a Slot.
@objc(SpineSlotData)
@objcMembers
public class SlotData: PosedData {
    @nonobjc
    public init(fromPointer ptr: spine_slot_data) {
        super.init(fromPointer: UnsafeMutableRawPointer(ptr).assumingMemoryBound(to: spine_posed_data_wrapper.self))
    }

    public convenience init(_ index: Int32, _ name: String, _ boneData: BoneData) {
        let ptr = spine_slot_data_create(index, name, boneData._ptr.assumingMemoryBound(to: spine_bone_data_wrapper.self))
        self.init(fromPointer: ptr!)
    }

    /// The Skeleton::getSlots() index for this slot.
    public var index: Int32 {
        let result = spine_slot_data_get_index(_ptr.assumingMemoryBound(to: spine_slot_data_wrapper.self))
        return result
    }

    /// The bone this slot belongs to.
    public var boneData: BoneData {
        let result = spine_slot_data_get_bone_data(_ptr.assumingMemoryBound(to: spine_slot_data_wrapper.self))
        return BoneData(fromPointer: result!)
    }

    /// The name of the attachment that is visible for this slot in the setup pose, or empty if no
    /// attachment is visible.
    public var attachmentName: String {
        get {
            let result = spine_slot_data_get_attachment_name(_ptr.assumingMemoryBound(to: spine_slot_data_wrapper.self))
        return String(cString: result!)
        }
        set {
            spine_slot_data_set_attachment_name(_ptr.assumingMemoryBound(to: spine_slot_data_wrapper.self), newValue)
        }
    }

    /// The blend mode for drawing the slot's attachment.
    public var blendMode: BlendMode {
        get {
            let result = spine_slot_data_get_blend_mode(_ptr.assumingMemoryBound(to: spine_slot_data_wrapper.self))
        return BlendMode(rawValue: Int32(result.rawValue))!
        }
        set {
            spine_slot_data_set_blend_mode(_ptr.assumingMemoryBound(to: spine_slot_data_wrapper.self), spine_blend_mode(rawValue: UInt32(newValue.rawValue)))
        }
    }

    /// False if the slot was hidden in Spine and nonessential data was exported. Does not affect
    /// runtime rendering.
    public var visible: Bool {
        get {
            let result = spine_slot_data_get_visible(_ptr.assumingMemoryBound(to: spine_slot_data_wrapper.self))
        return result
        }
        set {
            spine_slot_data_set_visible(_ptr.assumingMemoryBound(to: spine_slot_data_wrapper.self), newValue)
        }
    }

    /// The setup pose that most animations are relative to.
    public var setupPose: SlotPose {
        let result = spine_slot_data_get_setup_pose(_ptr.assumingMemoryBound(to: spine_slot_data_wrapper.self))
        return SlotPose(fromPointer: result!)
    }

    public override func dispose() {
        spine_slot_data_dispose(_ptr.assumingMemoryBound(to: spine_slot_data_wrapper.self))
    }
}