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

/// Fired by EventTimeline when specific animation times are reached.
///
/// See Timeline::apply(), AnimationStateListener::event(), and
/// https://esotericsoftware.com/spine-events Events in the Spine User Guide.
@objc(SpineEvent)
@objcMembers
public class Event: NSObject {
    public let _ptr: UnsafeMutableRawPointer

    public init(fromPointer ptr: spine_event) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        super.init()
    }

    public convenience init(_ time: Float, _ data: EventData) {
        let ptr = spine_event_create(time, data._ptr.assumingMemoryBound(to: spine_event_data_wrapper.self))
        self.init(fromPointer: ptr!)
    }

    /// The event's setup pose data.
    public var data: EventData {
        let result = spine_event_get_data(_ptr.assumingMemoryBound(to: spine_event_wrapper.self))
        return EventData(fromPointer: result!)
    }

    /// The animation time this event was keyed, or -1 for the setup pose.
    public var time: Float {
        let result = spine_event_get_time(_ptr.assumingMemoryBound(to: spine_event_wrapper.self))
        return result
    }

    /// The integer payload for this event.
    public var intValue: Int32 {
        get {
            let result = spine_event_get_int(_ptr.assumingMemoryBound(to: spine_event_wrapper.self))
            return result
        }
        set {
            spine_event_set_int(_ptr.assumingMemoryBound(to: spine_event_wrapper.self), newValue)
        }
    }

    /// The float payload for this event.
    public var floatValue: Float {
        get {
            let result = spine_event_get_float(_ptr.assumingMemoryBound(to: spine_event_wrapper.self))
            return result
        }
        set {
            spine_event_set_float(_ptr.assumingMemoryBound(to: spine_event_wrapper.self), newValue)
        }
    }

    /// The string payload for this event.
    public var stringValue: String {
        get {
            let result = spine_event_get_string(_ptr.assumingMemoryBound(to: spine_event_wrapper.self))
            return String(cString: result!)
        }
        set {
            spine_event_set_string(_ptr.assumingMemoryBound(to: spine_event_wrapper.self), newValue)
        }
    }

    /// If an audio path is set, the volume for the audio.
    public var volume: Float {
        get {
            let result = spine_event_get_volume(_ptr.assumingMemoryBound(to: spine_event_wrapper.self))
            return result
        }
        set {
            spine_event_set_volume(_ptr.assumingMemoryBound(to: spine_event_wrapper.self), newValue)
        }
    }

    /// If an audio path is set, the left/right balance for the audio.
    public var balance: Float {
        get {
            let result = spine_event_get_balance(_ptr.assumingMemoryBound(to: spine_event_wrapper.self))
            return result
        }
        set {
            spine_event_set_balance(_ptr.assumingMemoryBound(to: spine_event_wrapper.self), newValue)
        }
    }

    public func dispose() {
        spine_event_dispose(_ptr.assumingMemoryBound(to: spine_event_wrapper.self))
    }
}
