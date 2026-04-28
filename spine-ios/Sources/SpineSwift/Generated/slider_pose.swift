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

/// Stores a pose for a slider.
@objc(SpineSliderPose)
@objcMembers
public class SliderPose: NSObject {
    public let _ptr: UnsafeMutableRawPointer

    public init(fromPointer ptr: spine_slider_pose) {
        self._ptr = UnsafeMutableRawPointer(ptr)
        super.init()
    }

    public override convenience init() {
        let ptr = spine_slider_pose_create()
        self.init(fromPointer: ptr!)
    }

    /// The time in SliderData::getAnimation() to apply the animation.
    public var time: Float {
        get {
            let result = spine_slider_pose_get_time(_ptr.assumingMemoryBound(to: spine_slider_pose_wrapper.self))
            return result
        }
        set {
            spine_slider_pose_set_time(_ptr.assumingMemoryBound(to: spine_slider_pose_wrapper.self), newValue)
        }
    }

    /// A percentage (unbounded) that controls the mix between the constrained and unconstrained
    /// poses.
    public var mix: Float {
        get {
            let result = spine_slider_pose_get_mix(_ptr.assumingMemoryBound(to: spine_slider_pose_wrapper.self))
            return result
        }
        set {
            spine_slider_pose_set_mix(_ptr.assumingMemoryBound(to: spine_slider_pose_wrapper.self), newValue)
        }
    }

    public func set(_ pose: SliderPose) {
        spine_slider_pose_set(
            _ptr.assumingMemoryBound(to: spine_slider_pose_wrapper.self), pose._ptr.assumingMemoryBound(to: spine_slider_pose_wrapper.self))
    }

    public func dispose() {
        spine_slider_pose_dispose(_ptr.assumingMemoryBound(to: spine_slider_pose_wrapper.self))
    }
}
