/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine.animation {
import spine.Event;
import spine.Skeleton;
import spine.Slot;

public class ColorTimeline extends CurveTimeline {
	static public const ENTRIES:int = 5;
	static internal const PREV_TIME:int = -5, PREV_R:int = -4, PREV_G:int = -3, PREV_B:int = -2, PREV_A:int = -1;
	static internal const R:int = 1, G:int = 2, B:int = 3, A:int = 4;

	public var slotIndex:int;
	public var frames:Vector.<Number>; // time, r, g, b, a, ...

	public function ColorTimeline (frameCount:int) {
		super(frameCount);
		frames = new Vector.<Number>(frameCount * 5, true);
	}

	/** Sets the time and value of the specified keyframe. */
	public function setFrame (frameIndex:int, time:Number, r:Number, g:Number, b:Number, a:Number) : void {
		frameIndex *= ENTRIES;
		frames[frameIndex] = time;
		frames[int(frameIndex + R)] = r;
		frames[int(frameIndex + G)] = g;
		frames[int(frameIndex + B)] = b;
		frames[int(frameIndex + A)] = a;
	}

	override public function apply (skeleton:Skeleton, lastTime:Number, time:Number, firedEvents:Vector.<Event>, alpha:Number) : void {
		if (time < frames[0])
			return; // Time is before first frame.

		var r:Number, g:Number, b:Number, a:Number;
		if (time >= frames[int(frames.length - ENTRIES)]) {
			// Time is after last frame.
			var i:int = frames.length;
			r = frames[int(i + PREV_R)];
			g = frames[int(i + PREV_G)];
			b = frames[int(i + PREV_B)];
			a = frames[int(i + PREV_A)];
		} else {
			// Interpolate between the previous frame and the current frame.
			var frame:int = Animation.binarySearch(frames, time, ENTRIES);
			r = frames[int(frame + PREV_R)];
			g = frames[int(frame + PREV_G)];
			b = frames[int(frame + PREV_B)];
			a = frames[int(frame + PREV_A)];
			var frameTime:Number = frames[frame];
			var percent:Number = getCurvePercent(frame / ENTRIES - 1,
					1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

			r += (frames[frame + R] - r) * percent;
			g += (frames[frame + G] - g) * percent;
			b += (frames[frame + B] - b) * percent;
			a += (frames[frame + A] - a) * percent;
		}
		var slot:Slot = skeleton.slots[slotIndex];
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
