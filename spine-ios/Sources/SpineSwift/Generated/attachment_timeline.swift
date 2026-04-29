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

/// AttachmentTimeline wrapper
@objc(SpineAttachmentTimeline)
@objcMembers
public class AttachmentTimeline: Timeline, SlotTimeline {
    @nonobjc
    public init(fromPointer ptr: spine_attachment_timeline) {
        super.init(fromPointer: UnsafeMutableRawPointer(ptr).assumingMemoryBound(to: spine_timeline_wrapper.self))
    }

    public convenience init(_ frameCount: Int, _ slotIndex: Int32) {
        let ptr = spine_attachment_timeline_create(frameCount, slotIndex)
        self.init(fromPointer: ptr!)
    }

    public var slotIndex: Int32 {
        get {
            let result = spine_attachment_timeline_get_slot_index(_ptr.assumingMemoryBound(to: spine_attachment_timeline_wrapper.self))
        return result
        }
        set {
            spine_attachment_timeline_set_slot_index(_ptr.assumingMemoryBound(to: spine_attachment_timeline_wrapper.self), newValue)
        }
    }

    /// Sets the time and attachment name for the specified frame.
    ///
    /// - Parameter frame: Between 0 and frameCount, inclusive.
    /// - Parameter time: The frame time in seconds.
    public func setFrame(_ frame: Int32, _ time: Float, _ attachmentName: String) {
        spine_attachment_timeline_set_frame(_ptr.assumingMemoryBound(to: spine_attachment_timeline_wrapper.self), frame, time, attachmentName)
    }

    public func dispose() {
        spine_attachment_timeline_dispose(_ptr.assumingMemoryBound(to: spine_attachment_timeline_wrapper.self))
    }
}