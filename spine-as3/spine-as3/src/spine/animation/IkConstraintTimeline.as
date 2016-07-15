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
	static public const ENTRIES:int = 3;
	static internal const PREV_TIME:int = -3, PREV_MIX:int = -2, PREV_BEND_DIRECTION:int = -1;
	static internal const MIX:int = 1, BEND_DIRECTION:int = 2;

	public var ikConstraintIndex:int;
	public var frames:Vector.<Number>; // time, mix, bendDirection, ...

	public function IkConstraintTimeline (frameCount:int) {
		super(frameCount);
		frames = new Vector.<Number>(frameCount * ENTRIES, true);
	}

	/** Sets the time, mix and bend direction of the specified keyframe. */
	public function setFrame (frameIndex:int, time:Number, mix:Number, bendDirection:int) : void {
		frameIndex *= ENTRIES;
		frames[frameIndex] = time;
		frames[int(frameIndex + MIX)] = mix;
		frames[int(frameIndex + BEND_DIRECTION)] = bendDirection;
	}

	override public function apply (skeleton:Skeleton, lastTime:Number, time:Number, firedEvents:Vector.<Event>, alpha:Number) : void {
		if (time < frames[0]) return; // Time is before first frame.

		var constraint:IkConstraint = skeleton.ikConstraints[ikConstraintIndex];

		if (time >= frames[int(frames.length - ENTRIES)]) { // Time is after last frame.
			constraint.mix += (frames[int(frames.length + PREV_MIX)] - constraint.mix) * alpha;
			constraint.bendDirection = int(frames[int(frames.length + PREV_BEND_DIRECTION)]);
			return;
		}

		// Interpolate between the previous frame and the current frame.
		var frame:int = Animation.binarySearch(frames, time, ENTRIES);
		var mix:Number = frames[int(frame + PREV_MIX)];
		var frameTime:Number = frames[frame];
		var percent:Number = getCurvePercent(frame / ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

		constraint.mix += (mix + (frames[frame + MIX] - mix) * percent - constraint.mix) * alpha;
		constraint.bendDirection = int(frames[frame + PREV_BEND_DIRECTION]);
	}
}

}
