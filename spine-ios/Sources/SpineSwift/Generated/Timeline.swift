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

/// The base class for all timelines.
///
/// See Applying Animations in the Spine Runtimes Guide.
@objc(SpineTimeline)
@objcMembers
open class Timeline: NSObject {
    public let _ptr: UnsafeMutableRawPointer

    public init(fromPointer ptr: spine_timeline) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        super.init()
    }

    public var rtti: Rtti {
        let result = spine_timeline_get_rtti(_ptr.assumingMemoryBound(to: spine_timeline_wrapper.self))
        return Rtti(fromPointer: result!)
    }

    /// True if this timeline supports additive blending.
    public var additive: Bool {
        let result = spine_timeline_get_additive(_ptr.assumingMemoryBound(to: spine_timeline_wrapper.self))
        return result
    }

    /// True if this timeline sets values instantaneously and does not support interpolation between
    /// frames.
    public var instant: Bool {
        let result = spine_timeline_get_instant(_ptr.assumingMemoryBound(to: spine_timeline_wrapper.self))
        return result
    }

    public var frameEntries: Int {
        let result = spine_timeline_get_frame_entries(_ptr.assumingMemoryBound(to: spine_timeline_wrapper.self))
        return result
    }

    public var frameCount: Int {
        let result = spine_timeline_get_frame_count(_ptr.assumingMemoryBound(to: spine_timeline_wrapper.self))
        return result
    }

    public var frames: ArrayFloat {
        let result = spine_timeline_get_frames(_ptr.assumingMemoryBound(to: spine_timeline_wrapper.self))
        return ArrayFloat(fromPointer: result!)
    }

    public var duration: Float {
        let result = spine_timeline_get_duration(_ptr.assumingMemoryBound(to: spine_timeline_wrapper.self))
        return result
    }

    public var propertyIds: ArrayPropertyId {
        let result = spine_timeline_get_property_ids(_ptr.assumingMemoryBound(to: spine_timeline_wrapper.self))
        return ArrayPropertyId(fromPointer: result!)
    }

    /// Applies this timeline to the skeleton.
    ///
    /// See Applying Animations in the Spine Runtimes Guide.
    ///
    /// - Parameter skeleton: The skeleton the timeline is applied to. This provides access to the bones, slots, and other skeleton components the timelines may change.
    /// - Parameter lastTime: The last time in seconds this timeline was applied. Some timelines trigger only at discrete times, in which case all keys are triggered between lastTime (exclusive) and time (inclusive). Pass -1 the first time a timeline is applied to ensure frame 0 is triggered.
    /// - Parameter time: The time in seconds the skeleton is being posed for. Timelines find the frame before and after this time and interpolate between the frame values.
    /// - Parameter events: If any events are fired, they are added to this list. Can be NULL to ignore fired events or if no timelines fire events.
    /// - Parameter alpha: 0 applies setup or current values (depending on from), 1 uses timeline values, and intermediate values interpolate between them. Adjusting alpha over time can mix a timeline in or out.
    /// - Parameter from: If true, alpha transitions between setup and timeline values, setup values are used before the first frame (current values are not used). If false, alpha transitions between current and timeline values, no change is made before the first frame.
    /// - Parameter add: If true, for timelines that support it, their values are added to the setup or current values (depending on from).
    /// - Parameter out: True when the animation is mixing out, else it is mixing in. Used by timelines that perform instant transitions.
    /// - Parameter appliedPose: True to modify getAppliedPose(), else getPose() is modified.
    public func apply(_ skeleton: Skeleton, _ lastTime: Float, _ time: Float, _ events: ArrayEvent?, _ alpha: Float, _ from: MixFrom, _ add: Bool, _ out: Bool, _ appliedPose: Bool) {
        spine_timeline_apply(_ptr.assumingMemoryBound(to: spine_timeline_wrapper.self), skeleton._ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), lastTime, time, events?._ptr.assumingMemoryBound(to: spine_array_event_wrapper.self), alpha, spine_mix_from(rawValue: UInt32(from.rawValue)), add, out, appliedPose)
    }

    public static func rttiStatic() -> Rtti {
        let result = spine_timeline_rtti()
        return Rtti(fromPointer: result!)
    }

}