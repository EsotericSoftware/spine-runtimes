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
import spine.Bone;
import spine.Event;
import spine.Skeleton;

public class TranslateTimeline extends CurveTimeline {
	static internal const PREV_FRAME_TIME:int = -3;
	static internal const FRAME_X:int = 1;
	static internal const FRAME_Y:int = 2;

	public var boneIndex:int;
	public var frames:Vector.<Number>; // time, value, value, ...

	public function TranslateTimeline (frameCount:int) {
		super(frameCount);
		frames = new Vector.<Number>(frameCount * 3, true);
	}

	/** Sets the time and value of the specified keyframe. */
	public function setFrame (frameIndex:int, time:Number, x:Number, y:Number) : void {
		frameIndex *= 3;
		frames[frameIndex] = time;
		frames[int(frameIndex + 1)] = x;
		frames[int(frameIndex + 2)] = y;
	}

	override public function apply (skeleton:Skeleton, lastTime:Number, time:Number, firedEvents:Vector.<Event>, alpha:Number) : void {
		if (time < frames[0])
			return; // Time is before first frame.

		var bone:Bone = skeleton.bones[boneIndex];

		if (time >= frames[int(frames.length - 3)]) { // Time is after last frame.
			bone.x += (bone.data.x + frames[int(frames.length - 2)] - bone.x) * alpha;
			bone.y += (bone.data.y + frames[int(frames.length - 1)] - bone.y) * alpha;
			return;
		}

		// Interpolate between the previous frame and the current frame.
		var frameIndex:int = Animation.binarySearch(frames, time, 3);
		var prevFrameX:Number = frames[int(frameIndex - 2)];
		var prevFrameY:Number = frames[int(frameIndex - 1)];
		var frameTime:Number = frames[frameIndex];
		var percent:Number = 1 - (time - frameTime) / (frames[int(frameIndex + PREV_FRAME_TIME)] - frameTime);
		percent = getCurvePercent(frameIndex / 3 - 1, percent < 0 ? 0 : (percent > 1 ? 1 : percent));

		bone.x += (bone.data.x + prevFrameX + (frames[int(frameIndex + FRAME_X)] - prevFrameX) * percent - bone.x) * alpha;
		bone.y += (bone.data.y + prevFrameY + (frames[int(frameIndex + FRAME_Y)] - prevFrameY) * percent - bone.y) * alpha;
	}
}

}
