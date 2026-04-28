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

/// AlphaTimeline wrapper
@objc(SpineAlphaTimeline)
@objcMembers
public class AlphaTimeline: CurveTimeline1, SlotTimeline {
    @nonobjc
    public init(fromPointer ptr: spine_alpha_timeline) {
        super.init(fromPointer: UnsafeMutableRawPointer(ptr).assumingMemoryBound(to: spine_curve_timeline1_wrapper.self))
    }

    public convenience init(_ frameCount: Int, _ bezierCount: Int, _ slotIndex: Int32) {
        let ptr = spine_alpha_timeline_create(frameCount, bezierCount, slotIndex)
        self.init(fromPointer: ptr!)
    }

    public var slotIndex: Int32 {
        get {
            let result = spine_alpha_timeline_get_slot_index(_ptr.assumingMemoryBound(to: spine_alpha_timeline_wrapper.self))
            return result
        }
        set {
            spine_alpha_timeline_set_slot_index(_ptr.assumingMemoryBound(to: spine_alpha_timeline_wrapper.self), newValue)
        }
    }

    public func dispose() {
        spine_alpha_timeline_dispose(_ptr.assumingMemoryBound(to: spine_alpha_timeline_wrapper.self))
    }
}
