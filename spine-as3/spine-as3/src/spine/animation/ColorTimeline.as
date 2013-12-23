/******************************************************************************
 * Spine Runtime Software License - Version 1.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Essential, Professional, Enterprise, or Education License must
 *    be purchased from Esoteric Software and the license must remain valid:
 *    http://esotericsoftware.com/
 * 2. Redistributions of source code must retain this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer.
 * 3. Redistributions in binary form must reproduce this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer, in the documentation and/or other materials provided with the
 *    distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine.animation {
import spine.Event;
import spine.Skeleton;
import spine.Slot;

public class ColorTimeline extends CurveTimeline {
	static private const PREV_FRAME_TIME:int = -5;
	static private const FRAME_R:int = 1;
	static private const FRAME_G:int = 2;
	static private const FRAME_B:int = 3;
	static private const FRAME_A:int = 4;

	public var slotIndex:int;
	public var frames:Vector.<Number> = new Vector.<Number>(); // time, r, g, b, a, ...

	public function ColorTimeline (frameCount:int) {
		super(frameCount);
		frames.length = frameCount * 5;
	}

	/** Sets the time and value of the specified keyframe. */
	public function setFrame (frameIndex:int, time:Number, r:Number, g:Number, b:Number, a:Number) : void {
		frameIndex *= 5;
		frames[frameIndex] = time;
		frames[int(frameIndex + 1)] = r;
		frames[int(frameIndex + 2)] = g;
		frames[int(frameIndex + 3)] = b;
		frames[int(frameIndex + 4)] = a;
	}

	override public function apply (skeleton:Skeleton, lastTime:Number, time:Number, firedEvents:Vector.<Event>, alpha:Number) : void {
		if (time < frames[0])
			return; // Time is before first frame.

		var slot:Slot = skeleton.slots[slotIndex];

		if (time >= frames[int(frames.length - 5)]) { // Time is after last frame.
			var i:int = frames.length - 1;
			slot.r = frames[int(i - 3)];
			slot.g = frames[int(i - 2)];
			slot.b = frames[int(i - 1)];
			slot.a = frames[i];
			return;
		}

		// Interpolate between the previous frame and the current frame.
		var frameIndex:int = Animation.binarySearch(frames, time, 5);
		var prevFrameR:Number = frames[int(frameIndex - 4)];
		var prevFrameG:Number = frames[int(frameIndex - 3)];
		var prevFrameB:Number = frames[int(frameIndex - 2)];
		var prevFrameA:Number = frames[int(frameIndex - 1)];
		var frameTime:Number = frames[frameIndex];
		var percent:Number = 1 - (time - frameTime) / (frames[int(frameIndex + PREV_FRAME_TIME)] - frameTime);
		percent = getCurvePercent(frameIndex / 5 - 1, percent < 0 ? 0 : (percent > 1 ? 1 : percent));

		var r:Number = prevFrameR + (frames[int(frameIndex + FRAME_R)] - prevFrameR) * percent;
		var g:Number = prevFrameG + (frames[int(frameIndex + FRAME_G)] - prevFrameG) * percent;
		var b:Number = prevFrameB + (frames[int(frameIndex + FRAME_B)] - prevFrameB) * percent;
		var a:Number = prevFrameA + (frames[int(frameIndex + FRAME_A)] - prevFrameA) * percent;
		if (alpha < 1) {
			slot.r += (r - slot.r) * alpha;
			slot.g += (g - slot.g) * alpha;
			slot.b += (b - slot.b) * alpha;
			slot.a += (a - slot.a) * alpha;
		} else {
			slot.r = r;
			slot.g = g;
			slot.b = b;
			slot.a = a;
		}
	}
}

}
