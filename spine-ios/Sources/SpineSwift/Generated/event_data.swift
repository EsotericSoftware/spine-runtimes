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

/// Stores the setup pose values for an Event.
@objc(SpineEventData)
@objcMembers
public class EventData: NSObject {
    public let _ptr: UnsafeMutableRawPointer

    public init(fromPointer ptr: spine_event_data) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        super.init()
    }

    public convenience init(_ name: String) {
        let ptr = spine_event_data_create(name)
        self.init(fromPointer: ptr!)
    }

    /// The name of the event, unique across all events in the skeleton.
    public var name: String {
        let result = spine_event_data_get_name(_ptr.assumingMemoryBound(to: spine_event_data_wrapper.self))
        return String(cString: result!)
    }

    /// Path to an audio file relative to the audio folder as defined in Spine.
    public var audioPath: String {
        get {
            let result = spine_event_data_get_audio_path(_ptr.assumingMemoryBound(to: spine_event_data_wrapper.self))
            return String(cString: result!)
        }
        set {
            spine_event_data_set_audio_path(_ptr.assumingMemoryBound(to: spine_event_data_wrapper.self), newValue)
        }
    }

    /// The setup values that are shared by all events with this data.
    public var getSetupPose: Event {
        let result = spine_event_data_get_setup_pose_1(_ptr.assumingMemoryBound(to: spine_event_data_wrapper.self))
        return Event(fromPointer: result!)
    }

    public var setupPose2: Event {
        let result = spine_event_data_get_setup_pose_2(_ptr.assumingMemoryBound(to: spine_event_data_wrapper.self))
        return Event(fromPointer: result!)
    }

    public func dispose() {
        spine_event_data_dispose(_ptr.assumingMemoryBound(to: spine_event_data_wrapper.self))
    }
}
