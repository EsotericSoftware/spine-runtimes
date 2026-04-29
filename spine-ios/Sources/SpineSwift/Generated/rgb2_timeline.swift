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

/// Changes RGB for a slot's light and dark colors for two color tinting.
@objc(SpineRgb2Timeline)
@objcMembers
public class Rgb2Timeline: SlotCurveTimeline {
    @nonobjc
    public init(fromPointer ptr: spine_rgb2_timeline) {
        super.init(fromPointer: UnsafeMutableRawPointer(ptr).assumingMemoryBound(to: spine_slot_curve_timeline_wrapper.self))
    }

    public convenience init(_ frameCount: Int, _ bezierCount: Int, _ slotIndex: Int32) {
        let ptr = spine_rgb2_timeline_create(frameCount, bezierCount, slotIndex)
        self.init(fromPointer: ptr!)
    }

    /// Sets the time, light color, and dark color for the specified frame.
    ///
    /// - Parameter frame: Between 0 and frameCount, inclusive.
    /// - Parameter time: The frame time in seconds.
    public func setFrame(_ frame: Int32, _ time: Float, _ r: Float, _ g: Float, _ b: Float, _ r2: Float, _ g2: Float, _ b2: Float) {
        spine_rgb2_timeline_set_frame(_ptr.assumingMemoryBound(to: spine_rgb2_timeline_wrapper.self), frame, time, r, g, b, r2, g2, b2)
    }

    public func dispose() {
        spine_rgb2_timeline_dispose(_ptr.assumingMemoryBound(to: spine_rgb2_timeline_wrapper.self))
    }
}