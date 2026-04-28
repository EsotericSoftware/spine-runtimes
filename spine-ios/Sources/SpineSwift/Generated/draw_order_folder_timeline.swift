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

/// Changes a subset of the Skeleton::getDrawOrder() draw order.
@objc(SpineDrawOrderFolderTimeline)
@objcMembers
public class DrawOrderFolderTimeline: Timeline {
    @nonobjc
    public init(fromPointer ptr: spine_draw_order_folder_timeline) {
        super.init(fromPointer: UnsafeMutableRawPointer(ptr).assumingMemoryBound(to: spine_timeline_wrapper.self))
    }

    public convenience init(_ frameCount: Int, _ slots: ArrayInt, _ slotCount: Int) {
        let ptr = spine_draw_order_folder_timeline_create(frameCount, slots._ptr.assumingMemoryBound(to: spine_array_int_wrapper.self), slotCount)
        self.init(fromPointer: ptr!)
    }

    /// The Skeleton::getSlots() indices that this timeline affects, in setup order.
    public var slots: ArrayInt {
        let result = spine_draw_order_folder_timeline_get_slots(_ptr.assumingMemoryBound(to: spine_draw_order_folder_timeline_wrapper.self))
        return ArrayInt(fromPointer: result!)
    }

    /// Sets the time and draw order for the specified frame.
    ///
    /// - Parameter frame: Between 0 and frameCount, inclusive.
    /// - Parameter time: The frame time in seconds.
    /// - Parameter drawOrder: Ordered getSlots() indices, or null to use setup pose order.
    public func setFrame(_ frame: Int, _ time: Float, _ drawOrder: ArrayInt?) {
        spine_draw_order_folder_timeline_set_frame(
            _ptr.assumingMemoryBound(to: spine_draw_order_folder_timeline_wrapper.self), frame, time,
            drawOrder?._ptr.assumingMemoryBound(to: spine_array_int_wrapper.self))
    }

    public func dispose() {
        spine_draw_order_folder_timeline_dispose(_ptr.assumingMemoryBound(to: spine_draw_order_folder_timeline_wrapper.self))
    }
}
