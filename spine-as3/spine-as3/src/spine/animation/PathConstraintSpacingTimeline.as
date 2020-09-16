/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine.animation {
	import spine.Skeleton;
	import spine.Event;
	import spine.PathConstraint;

	public class PathConstraintSpacingTimeline extends PathConstraintPositionTimeline {
		public function PathConstraintSpacingTimeline(frameCount : int) {
			super(frameCount);
		}

		override public function getPropertyId() : int {
			return (TimelineType.pathConstraintSpacing.ordinal << 24) + pathConstraintIndex;
		}

		override public function apply(skeleton : Skeleton, lastTime : Number, time : Number, firedEvents : Vector.<Event>, alpha : Number, blend : MixBlend, direction : MixDirection) : void {
			var constraint : PathConstraint = skeleton.pathConstraints[pathConstraintIndex];
			if (!constraint.active) return;
			if (time < frames[0]) {
				switch (blend) {
				case MixBlend.setup:
					constraint.spacing = constraint.data.spacing;
					return;
				case MixBlend.first:
					constraint.spacing += (constraint.data.spacing - constraint.spacing) * alpha;
				}
				return;
			}

			var spacing : Number;
			if (time >= frames[frames.length - ENTRIES]) // Time is after last frame.
				spacing = frames[frames.length + PREV_VALUE];
			else {
				// Interpolate between the previous frame and the current frame.
				var frame : int = Animation.binarySearch(frames, time, ENTRIES);
				spacing = frames[frame + PREV_VALUE];
				var frameTime : Number = frames[frame];
				var percent : Number = getCurvePercent(frame / ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

				spacing += (frames[frame + VALUE] - spacing) * percent;
			}

			if (blend == MixBlend.setup)
				constraint.spacing = constraint.data.spacing + (spacing - constraint.data.spacing) * alpha;
			else
				constraint.spacing += (spacing - constraint.spacing) * alpha;
		}
	}
}
