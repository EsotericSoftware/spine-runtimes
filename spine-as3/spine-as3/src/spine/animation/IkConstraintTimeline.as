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
import spine.IkConstraint;
import spine.Skeleton;

public class IkConstraintTimeline extends CurveTimeline {
	static private const PREV_FRAME_TIME:int = -3;
	static private const PREV_FRAME_MIX:int = -2;
	static private const PREV_FRAME_BEND_DIRECTION:int = -1;
	static private const FRAME_MIX:int = 1;

	public var ikConstraintIndex:int;
	public var frames:Vector.<Number>; // time, mix, bendDirection, ...

	public function IkConstraintTimeline (frameCount:int) {
		super(frameCount);
		frames = new Vector.<Number>(frameCount * 3, true);
	}

	/** Sets the time, mix and bend direction of the specified keyframe. */
	public function setFrame (frameIndex:int, time:Number, mix:Number, bendDirection:int) : void {
		frameIndex *= 3;
		frames[frameIndex] = time;
		frames[int(frameIndex + 1)] = mix;
		frames[int(frameIndex + 2)] = bendDirection;
	}

	override public function apply (skeleton:Skeleton, lastTime:Number, time:Number, firedEvents:Vector.<Event>, alpha:Number) : void {
		if (time < frames[0]) return; // Time is before first frame.

		var ikConstraint:IkConstraint = skeleton.ikConstraints[ikConstraintIndex];

		if (time >= frames[int(frames.length - 3)]) { // Time is after last frame.
			ikConstraint.mix += (frames[int(frames.length - 2)] - ikConstraint.mix) * alpha;
			ikConstraint.bendDirection = int(frames[int(frames.length - 1)]);
			return;
		}

		// Interpolate between the previous frame and the current frame.
		var frameIndex:int = Animation.binarySearch(frames, time, 3);
		var prevFrameMix:Number = frames[int(frameIndex + PREV_FRAME_MIX)];
		var frameTime:Number = frames[frameIndex];
		var percent:Number = 1 - (time - frameTime) / (frames[int(frameIndex + PREV_FRAME_TIME)] - frameTime);
		percent = getCurvePercent(frameIndex / 3 - 1, percent < 0 ? 0 : (percent > 1 ? 1 : percent));

		var mix:Number = prevFrameMix + (frames[int(frameIndex + FRAME_MIX)] - prevFrameMix) * percent;
		ikConstraint.mix += (mix - ikConstraint.mix) * alpha;
		ikConstraint.bendDirection = int(frames[int(frameIndex + PREV_FRAME_BEND_DIRECTION)]);
	}
}

}
