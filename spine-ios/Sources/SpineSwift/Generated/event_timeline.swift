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

/// Fires an Event when specific animation times are reached.
@objc(SpineEventTimeline)
@objcMembers
public class EventTimeline: Timeline {
    @nonobjc
    public init(fromPointer ptr: spine_event_timeline) {
        super.init(fromPointer: UnsafeMutableRawPointer(ptr).assumingMemoryBound(to: spine_timeline_wrapper.self))
    }

    public convenience init(_ frameCount: Int) {
        let ptr = spine_event_timeline_create(frameCount)
        self.init(fromPointer: ptr!)
    }

    /// The event for each frame.
    public var events: ArrayEvent {
        let result = spine_event_timeline_get_events(_ptr.assumingMemoryBound(to: spine_event_timeline_wrapper.self))
        return ArrayEvent(fromPointer: result!)
    }

    /// Sets the time and event for the specified frame.
    ///
    /// - Parameter frame: Between 0 and frameCount, inclusive.
    public func setFrame(_ frame: Int, _ event: Event) {
        spine_event_timeline_set_frame(_ptr.assumingMemoryBound(to: spine_event_timeline_wrapper.self), frame, event._ptr.assumingMemoryBound(to: spine_event_wrapper.self))
    }

    public func dispose() {
        spine_event_timeline_dispose(_ptr.assumingMemoryBound(to: spine_event_timeline_wrapper.self))
    }
}