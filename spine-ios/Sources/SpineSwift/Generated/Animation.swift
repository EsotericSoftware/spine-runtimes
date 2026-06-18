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

/// Stores a list of timelines to animate a skeleton's pose over time.
///
/// See Applying Animations in the Spine Runtimes Guide.
@objc(SpineAnimation)
@objcMembers
public class Animation: NSObject {
    public let _ptr: UnsafeMutableRawPointer

    public init(fromPointer ptr: spine_animation) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        super.init()
    }

    /// Creates a new animation. The timelines must be set before use.
    public convenience init(_ name: String) {
        let ptr = spine_animation_create(name)
        self.init(fromPointer: ptr!)
    }

    /// If this list or the timelines it contains are modified, the timelines and bones must be set
    /// again to recompute the animation's bone indices and timeline property IDs.
    ///
    /// See setTimelines().
    public var timelines: ArrayTimeline {
        let result = spine_animation_get_timelines(_ptr.assumingMemoryBound(to: spine_animation_wrapper.self))
        return ArrayTimeline(fromPointer: result!)
    }

    /// The duration of the animation in seconds, which is usually the highest time of all frames in
    /// the timeline. The duration is used to know when it has completed and when it should loop
    /// back to the start.
    public var duration: Float {
        get {
            let result = spine_animation_get_duration(_ptr.assumingMemoryBound(to: spine_animation_wrapper.self))
        return result
        }
        set {
            spine_animation_set_duration(_ptr.assumingMemoryBound(to: spine_animation_wrapper.self), newValue)
        }
    }

    /// The animation's name, which is unique across all animations in the skeleton.
    public var name: String {
        let result = spine_animation_get_name(_ptr.assumingMemoryBound(to: spine_animation_wrapper.self))
        return String(cString: result!)
    }

    /// The Skeleton::getBones() indices affected by this animation.
    ///
    /// See setTimelines() and BoneTimeline::getBoneIndex().
    public var bones: ArrayInt {
        let result = spine_animation_get_bones(_ptr.assumingMemoryBound(to: spine_animation_wrapper.self))
        return ArrayInt(fromPointer: result!)
    }

    /// The color of the animation as it was in Spine, or a default color if nonessential data was
    /// not exported.
    public var color: Color {
        let result = spine_animation_get_color(_ptr.assumingMemoryBound(to: spine_animation_wrapper.self))
        return Color(fromPointer: result!)
    }

    /// Sets the timelines and bone indices.
    public func setTimelines(_ timelines: ArrayTimeline, _ bones: ArrayInt) {
        spine_animation_set_timelines(_ptr.assumingMemoryBound(to: spine_animation_wrapper.self), timelines._ptr.assumingMemoryBound(to: spine_array_timeline_wrapper.self), bones._ptr.assumingMemoryBound(to: spine_array_int_wrapper.self))
    }

    /// Returns true if this animation contains a timeline with any of the specified property IDs.
    public func hasTimeline(_ ids: ArrayPropertyId) -> Bool {
        let result = spine_animation_has_timeline(_ptr.assumingMemoryBound(to: spine_animation_wrapper.self), ids._ptr.assumingMemoryBound(to: spine_array_property_id_wrapper.self))
        return result
    }

    /// Applies the animation's timelines to the specified skeleton.
    ///
    /// See Timeline::apply() and Applying Animations in the Spine Runtimes Guide.
    ///
    /// - Parameter skeleton: The skeleton the animation is applied to. This provides access to the bones, slots, and other skeleton components the timelines may change.
    /// - Parameter lastTime: The last time in seconds this animation was applied. Some timelines trigger only at discrete times, in which case all keys are triggered between lastTime (exclusive) and time (inclusive). Pass -1 the first time an animation is applied to ensure frame 0 is triggered.
    /// - Parameter time: The time in seconds the skeleton is being posed for. Timelines find the frame before and after this time and interpolate between the frame values.
    /// - Parameter loop: True if time beyond the animation duration repeats the animation, else the last frame is used.
    /// - Parameter events: If any events are fired, they are added to this list. Can be NULL to ignore fired events or if no timelines fire events.
    /// - Parameter alpha: 0 applies setup or current values (depending on from), 1 uses timeline values, and intermediate values interpolate between them. Adjusting alpha over time can mix an animation in or out.
    /// - Parameter from: If true, alpha transitions between setup and timeline values, setup values are used before the first frame (current values are not used). If false, alpha transitions between current and timeline values, no change is made before the first frame.
    /// - Parameter add: If true, for timelines that support it, their values are added to the setup or current values (depending on from).
    /// - Parameter out: True when the animation is mixing out, else it is mixing in. Used by timelines that perform instant transitions.
    /// - Parameter appliedPose: True to modify getAppliedPose(), else the unconstrained pose is modified.
    public func apply(_ skeleton: Skeleton, _ lastTime: Float, _ time: Float, _ loop: Bool, _ events: ArrayEvent?, _ alpha: Float, _ from: MixFrom, _ add: Bool, _ out: Bool, _ appliedPose: Bool) {
        spine_animation_apply(_ptr.assumingMemoryBound(to: spine_animation_wrapper.self), skeleton._ptr.assumingMemoryBound(to: spine_skeleton_wrapper.self), lastTime, time, loop, events?._ptr.assumingMemoryBound(to: spine_array_event_wrapper.self), alpha, spine_mix_from(rawValue: UInt32(from.rawValue)), add, out, appliedPose)
    }

    /// - Parameter target: After the first and before the last entry.
    public static func search(_ values: ArrayFloat, _ target: Float) -> Int32 {
        let result = spine_animation_search_1(values._ptr.assumingMemoryBound(to: spine_array_float_wrapper.self), target)
        return result
    }

    public static func search2(_ values: ArrayFloat, _ target: Float, _ step: Int32) -> Int32 {
        let result = spine_animation_search_2(values._ptr.assumingMemoryBound(to: spine_array_float_wrapper.self), target, step)
        return result
    }

    public func dispose() {
        spine_animation_dispose(_ptr.assumingMemoryBound(to: spine_animation_wrapper.self))
    }
}