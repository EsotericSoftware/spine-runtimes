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

/// EventQueueEntry wrapper
@objc(SpineEventQueueEntry)
@objcMembers
public class EventQueueEntry: NSObject {
    public let _ptr: UnsafeMutableRawPointer

    public init(fromPointer ptr: spine_event_queue_entry) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        super.init()
    }

    public convenience init(_ eventType: EventType, _ trackEntry: TrackEntry?, _ event: Event?) {
        let ptr = spine_event_queue_entry_create(
            spine_event_type(rawValue: UInt32(eventType.rawValue)), trackEntry?._ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self),
            event?._ptr.assumingMemoryBound(to: spine_event_wrapper.self))
        self.init(fromPointer: ptr!)
    }

    public var type: EventType {
        get {
            let result = spine_event_queue_entry_get__type(_ptr.assumingMemoryBound(to: spine_event_queue_entry_wrapper.self))
            return EventType(rawValue: Int32(result.rawValue))!
        }
        set {
            spine_event_queue_entry_set__type(
                _ptr.assumingMemoryBound(to: spine_event_queue_entry_wrapper.self), spine_event_type(rawValue: UInt32(newValue.rawValue)))
        }
    }

    public var entry: TrackEntry? {
        get {
            let result = spine_event_queue_entry_get__entry(_ptr.assumingMemoryBound(to: spine_event_queue_entry_wrapper.self))
            return result.map { TrackEntry(fromPointer: $0) }
        }
        set {
            spine_event_queue_entry_set__entry(
                _ptr.assumingMemoryBound(to: spine_event_queue_entry_wrapper.self),
                newValue?._ptr.assumingMemoryBound(to: spine_track_entry_wrapper.self))
        }
    }

    public var event: Event? {
        get {
            let result = spine_event_queue_entry_get__event(_ptr.assumingMemoryBound(to: spine_event_queue_entry_wrapper.self))
            return result.map { Event(fromPointer: $0) }
        }
        set {
            spine_event_queue_entry_set__event(
                _ptr.assumingMemoryBound(to: spine_event_queue_entry_wrapper.self), newValue?._ptr.assumingMemoryBound(to: spine_event_wrapper.self))
        }
    }

    public func dispose() {
        spine_event_queue_entry_dispose(_ptr.assumingMemoryBound(to: spine_event_queue_entry_wrapper.self))
    }
}
