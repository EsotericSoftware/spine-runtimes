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

/// Stores mix (crossfade) durations to be applied when AnimationState animations are changed on the
/// same track.
@objc(SpineAnimationStateData)
@objcMembers
public class AnimationStateData: NSObject {
    public let _ptr: UnsafeMutableRawPointer

    public init(fromPointer ptr: spine_animation_state_data) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        super.init()
    }

    public convenience init(_ skeletonData: SkeletonData) {
        let ptr = spine_animation_state_data_create(skeletonData._ptr.assumingMemoryBound(to: spine_skeleton_data_wrapper.self))
        self.init(fromPointer: ptr!)
    }

    /// The SkeletonData to look up animations when they are specified by name.
    public var skeletonData: SkeletonData {
        let result = spine_animation_state_data_get_skeleton_data(_ptr.assumingMemoryBound(to: spine_animation_state_data_wrapper.self))
        return SkeletonData(fromPointer: result!)
    }

    /// The mix duration to use when no mix duration has been specifically defined between two
    /// animations.
    public var defaultMix: Float {
        get {
            let result = spine_animation_state_data_get_default_mix(_ptr.assumingMemoryBound(to: spine_animation_state_data_wrapper.self))
        return result
        }
        set {
            spine_animation_state_data_set_default_mix(_ptr.assumingMemoryBound(to: spine_animation_state_data_wrapper.self), newValue)
        }
    }

    /// Returns the mix duration to use when changing from the specified animation to the other on
    /// the same track, or the default mix if no mix duration has been set.
    public func getMix(_ from: Animation, _ to: Animation) -> Float {
        let result = spine_animation_state_data_get_mix(_ptr.assumingMemoryBound(to: spine_animation_state_data_wrapper.self), from._ptr.assumingMemoryBound(to: spine_animation_wrapper.self), to._ptr.assumingMemoryBound(to: spine_animation_wrapper.self))
        return result
    }

    /// Removes all mixes and sets the default mix to 0.
    public func clear() {
        spine_animation_state_data_clear(_ptr.assumingMemoryBound(to: spine_animation_state_data_wrapper.self))
    }

    /// Sets a mix duration by animation names.
    public func setMix(_ fromName: String, _ toName: String, _ duration: Float) {
        spine_animation_state_data_set_mix_1(_ptr.assumingMemoryBound(to: spine_animation_state_data_wrapper.self), fromName, toName, duration)
    }

    /// Sets a mix duration when changing from the specified animation to the other. See
    /// TrackEntry.MixDuration.
    public func setMix2(_ from: Animation, _ to: Animation, _ duration: Float) {
        spine_animation_state_data_set_mix_2(_ptr.assumingMemoryBound(to: spine_animation_state_data_wrapper.self), from._ptr.assumingMemoryBound(to: spine_animation_wrapper.self), to._ptr.assumingMemoryBound(to: spine_animation_wrapper.self), duration)
    }

    public func dispose() {
        spine_animation_state_data_dispose(_ptr.assumingMemoryBound(to: spine_animation_state_data_wrapper.self))
    }
}