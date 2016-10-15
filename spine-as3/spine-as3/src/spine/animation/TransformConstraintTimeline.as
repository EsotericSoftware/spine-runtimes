/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine.animation {
import spine.Event;
import spine.Skeleton;
import spine.TransformConstraint;

public class TransformConstraintTimeline extends CurveTimeline {
	static public const ENTRIES:int = 5;
	static internal const PREV_TIME:int = -5, PREV_ROTATE:int = -4, PREV_TRANSLATE:int = -3, PREV_SCALE:int = -2, PREV_SHEAR:int = -1;
	static internal const ROTATE:int = 1, TRANSLATE:int = 2, SCALE:int = 3, SHEAR:int = 4;

	public var transformConstraintIndex:int;
	public var frames:Vector.<Number>; // time, rotate mix, translate mix, scale mix, shear mix, ...

	public function TransformConstraintTimeline (frameCount:int) {
		super(frameCount);
		frames = new Vector.<Number>(frameCount * ENTRIES, true);
	}

	/** Sets the time and mixes of the specified keyframe. */
	public function setFrame (frameIndex:int, time:Number, rotateMix:Number, translateMix:Number, scaleMix:Number, shearMix:Number) : void {
		frameIndex *= ENTRIES;
		frames[frameIndex] = time;
		frames[frameIndex + ROTATE] = rotateMix;
		frames[frameIndex + TRANSLATE] = translateMix;
		frames[frameIndex + SCALE] = scaleMix;
		frames[frameIndex + SHEAR] = shearMix;
	}

	override public function apply (skeleton:Skeleton, lastTime:Number, time:Number, firedEvents:Vector.<Event>, alpha:Number) : void {
		if (time < frames[0]) return; // Time is before first frame.

		var constraint:TransformConstraint = skeleton.transformConstraints[transformConstraintIndex];

		if (time >= frames[frames.length - ENTRIES]) { // Time is after last frame.
			var i:int = frames.length;
			constraint.rotateMix += (frames[i + PREV_ROTATE] - constraint.rotateMix) * alpha;
			constraint.translateMix += (frames[i + PREV_TRANSLATE] - constraint.translateMix) * alpha;
			constraint.scaleMix += (frames[i + PREV_SCALE] - constraint.scaleMix) * alpha;
			constraint.shearMix += (frames[i + PREV_SHEAR] - constraint.shearMix) * alpha;
			return;
		}

		// Interpolate between the previous frame and the current frame.
		var frame:int = Animation.binarySearch(frames, time, ENTRIES);
		var frameTime:Number = frames[frame];
		var percent:Number = getCurvePercent(frame / ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

		var rotate:Number = frames[frame + PREV_ROTATE];
		var translate:Number = frames[frame + PREV_TRANSLATE];
		var scale:Number = frames[frame + PREV_SCALE];
		var shear:Number = frames[frame + PREV_SHEAR];
		constraint.rotateMix += (rotate + (frames[frame + ROTATE] - rotate) * percent - constraint.rotateMix) * alpha;
		constraint.translateMix += (translate + (frames[frame + TRANSLATE] - translate) * percent - constraint.translateMix)
			* alpha;
		constraint.scaleMix += (scale + (frames[frame + SCALE] - scale) * percent - constraint.scaleMix) * alpha;
		constraint.shearMix += (shear + (frames[frame + SHEAR] - shear) * percent - constraint.shearMix) * alpha;
	}
}
}
