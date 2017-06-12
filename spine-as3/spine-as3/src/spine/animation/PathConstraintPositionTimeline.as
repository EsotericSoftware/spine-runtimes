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
	import spine.PathConstraint;
	import spine.Event;
	import spine.Skeleton;

	public class PathConstraintPositionTimeline extends CurveTimeline {
		static public const ENTRIES : int = 2;
		static internal const PREV_TIME : int = -2, PREV_VALUE : int = -1;
		static internal const VALUE : int = 1;
		public var pathConstraintIndex : int;
		public var frames : Vector.<Number>; // time, position, ...

		public function PathConstraintPositionTimeline(frameCount : int) {
			super(frameCount);
			frames = new Vector.<Number>(frameCount * ENTRIES, true);
		}

		override public function getPropertyId() : int {
			return (TimelineType.pathConstraintPosition.ordinal << 24) + pathConstraintIndex;
		}

		/** Sets the time and value of the specified keyframe. */
		public function setFrame(frameIndex : int, time : Number, value : Number) : void {
			frameIndex *= ENTRIES;
			frames[frameIndex] = time;
			frames[frameIndex + VALUE] = value;
		}

		override public function apply(skeleton : Skeleton, lastTime : Number, time : Number, firedEvents : Vector.<Event>, alpha : Number, pose : MixPose, direction : MixDirection) : void {
			var constraint : PathConstraint = skeleton.pathConstraints[pathConstraintIndex];
			if (time < frames[0]) {
				switch (pose) {
				case MixPose.setup:
					constraint.position = constraint.data.position;
					return;
				case MixPose.current:
					constraint.position += (constraint.data.position - constraint.position) * alpha;
				}
				return;
			}

			var position : Number;
			if (time >= frames[frames.length - ENTRIES]) // Time is after last frame.
				position = frames[frames.length + PREV_VALUE];
			else {
				// Interpolate between the previous frame and the current frame.
				var frame : int = Animation.binarySearch(frames, time, ENTRIES);
				position = frames[frame + PREV_VALUE];
				var frameTime : Number = frames[frame];
				var percent : Number = getCurvePercent(frame / ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

				position += (frames[frame + VALUE] - position) * percent;
			}
			if (pose == MixPose.setup)
				constraint.position = constraint.data.position + (position - constraint.data.position) * alpha;
			else
				constraint.position += (position - constraint.position) * alpha;
		}
	}
}