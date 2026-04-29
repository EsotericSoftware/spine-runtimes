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

/// Base class for frames that use an interpolation bezier curve.
@objc(SpineCurveTimeline)
@objcMembers
open class CurveTimeline: Timeline {
    @nonobjc
    public init(fromPointer ptr: spine_curve_timeline) {
        super.init(fromPointer: UnsafeMutableRawPointer(ptr).assumingMemoryBound(to: spine_timeline_wrapper.self))
    }

    public var linear: Int {
        get { fatalError("Setter-only property") }
        set(newValue) {
            spine_curve_timeline_set_linear(_ptr.assumingMemoryBound(to: spine_curve_timeline_wrapper.self), newValue)
        }
    }

    public var stepped: Int {
        get { fatalError("Setter-only property") }
        set(newValue) {
            spine_curve_timeline_set_stepped(_ptr.assumingMemoryBound(to: spine_curve_timeline_wrapper.self), newValue)
        }
    }

    public var curves: ArrayFloat {
        let result = spine_curve_timeline_get_curves(_ptr.assumingMemoryBound(to: spine_curve_timeline_wrapper.self))
        return ArrayFloat(fromPointer: result!)
    }

    public func setBezier(_ bezier: Int, _ frame: Int, _ value: Float, _ time1: Float, _ value1: Float, _ cx1: Float, _ cy1: Float, _ cx2: Float, _ cy2: Float, _ time2: Float, _ value2: Float) {
        spine_curve_timeline_set_bezier(_ptr.assumingMemoryBound(to: spine_curve_timeline_wrapper.self), bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2)
    }

    public func getBezierValue(_ time: Float, _ frame: Int, _ valueOffset: Int, _ i: Int) -> Float {
        let result = spine_curve_timeline_get_bezier_value(_ptr.assumingMemoryBound(to: spine_curve_timeline_wrapper.self), time, frame, valueOffset, i)
        return result
    }

}