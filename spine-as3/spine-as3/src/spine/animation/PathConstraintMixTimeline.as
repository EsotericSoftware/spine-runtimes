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
	import spine.PathConstraint;

	public class PathConstraintMixTimeline extends CurveTimeline {
		static public const ENTRIES : int = 3;
		static internal const PREV_TIME : int = -3, PREV_ROTATE : int = -2, PREV_TRANSLATE : int = -1;
		static internal const ROTATE : int = 1, TRANSLATE : int = 2;
		public var pathConstraintIndex : int;
		public var frames : Vector.<Number>; // time, rotate mix, translate mix, ...

		public function PathConstraintMixTimeline(frameCount : int) {
			super(frameCount);
			frames = new Vector.<Number>(frameCount * ENTRIES, true);
		}

		override public function getPropertyId() : int {
			return (TimelineType.pathConstraintMix.ordinal << 24) + pathConstraintIndex;
		}

		/** Sets the time and mixes of the specified keyframe. */
		public function setFrame(frameIndex : int, time : Number, rotateMix : Number, translateMix : Number) : void {
			frameIndex *= ENTRIES;
			frames[frameIndex] = time;
			frames[frameIndex + ROTATE] = rotateMix;
			frames[frameIndex + TRANSLATE] = translateMix;
		}

		override public function apply(skeleton : Skeleton, lastTime : Number, time : Number, firedEvents : Vector.<Event>, alpha : Number, pose : MixPose, direction : MixDirection) : void {
			var constraint : PathConstraint = skeleton.pathConstraints[pathConstraintIndex];
			if (time < frames[0]) {
				switch (pose) {
				case MixPose.setup:
					constraint.rotateMix = constraint.data.rotateMix;
					constraint.translateMix = constraint.data.translateMix;
					return;
				case MixPose.current:
					constraint.rotateMix += (constraint.data.rotateMix - constraint.rotateMix) * alpha;
					constraint.translateMix += (constraint.data.translateMix - constraint.translateMix) * alpha;
				}
				return;
			}

			var rotate : Number, translate : Number;
			if (time >= frames[frames.length - ENTRIES]) { // Time is after last frame.
				rotate = frames[frames.length + PREV_ROTATE];
				translate = frames[frames.length + PREV_TRANSLATE];
			} else {
				// Interpolate between the previous frame and the current frame.
				var frame : int = Animation.binarySearch(frames, time, ENTRIES);
				rotate = frames[frame + PREV_ROTATE];
				translate = frames[frame + PREV_TRANSLATE];
				var frameTime : Number = frames[frame];
				var percent : Number = getCurvePercent(frame / ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

				rotate += (frames[frame + ROTATE] - rotate) * percent;
				translate += (frames[frame + TRANSLATE] - translate) * percent;
			}

			if (pose == MixPose.setup) {
				constraint.rotateMix = constraint.data.rotateMix + (rotate - constraint.data.rotateMix) * alpha;
				constraint.translateMix = constraint.data.translateMix + (translate - constraint.data.translateMix) * alpha;
			} else {
				constraint.rotateMix += (rotate - constraint.rotateMix) * alpha;
				constraint.translateMix += (translate - constraint.translateMix) * alpha;
			}
		}
	}
}