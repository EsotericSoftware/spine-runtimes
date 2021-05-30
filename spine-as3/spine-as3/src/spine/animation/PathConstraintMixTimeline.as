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
	import spine.Event;
	import spine.Skeleton;
	import spine.PathConstraint;
	import spine.PathConstraintData;
	
	public class PathConstraintMixTimeline extends CurveTimeline {
		static internal const ENTRIES : int = 4;
		static internal const ROTATE : int = 1, X : int = 2, Y : int = 3;

		/** The index of the path constraint slot in {@link Skeleton#getPathConstraints()} that will be changed. */
		public var pathConstraintIndex : int;

		public function PathConstraintMixTimeline (frameCount : int, bezierCount : int, pathConstraintIndex : int) {
			super(frameCount, bezierCount, [
				Property.pathConstraintMix + "|" + pathConstraintIndex
			]);
			this.pathConstraintIndex = pathConstraintIndex;
		}

		public override function getFrameEntries() : int {
			return ENTRIES;
		}

		public function setFrame (frame : int, time : Number, mixRotate : Number, mixX : Number, mixY : Number) : void {
			frame <<= 2;
			frames[frame] = time;
			frames[frame + ROTATE] = mixRotate;
			frames[frame + X] = mixX;
			frames[frame + Y] = mixY;
		}

		public override function apply (skeleton : Skeleton, lastTime : Number, time : Number, events : Vector.<Event>, alpha : Number, blend : MixBlend, direction : MixDirection) : void {
			var constraint : PathConstraint = skeleton.pathConstraints[pathConstraintIndex];
			if (!constraint.active) return;

			var data : PathConstraintData;

			var frames : Vector.<Number> = this.frames;
			if (time < frames[0]) {
				data = constraint.data;
				switch (blend) {
				case MixBlend.setup:
					constraint.mixRotate = data.mixRotate;
					constraint.mixX = data.mixX;
					constraint.mixY = data.mixY;
					return;
				case MixBlend.first:
					constraint.mixRotate += (data.mixRotate - constraint.mixRotate) * alpha;
					constraint.mixX += (data.mixX - constraint.mixX) * alpha;
					constraint.mixY += (data.mixY - constraint.mixY) * alpha;
				}
				return;
			}

			var rotate : Number, x : Number, y : Number;
			var i : int = search2(frames, time, ENTRIES);
			var curveType : Number = curves[i >> 2];
			switch (curveType) {
			case LINEAR:
				var before : Number = frames[i];
				rotate = frames[i + ROTATE];
				x = frames[i + X];
				y = frames[i + Y];
				var t : Number = (time - before) / (frames[i + ENTRIES] - before);
				rotate += (frames[i + ENTRIES + ROTATE] - rotate) * t;
				x += (frames[i + ENTRIES + X] - x) * t;
				y += (frames[i + ENTRIES + Y] - y) * t;
				break;
			case STEPPED:
				rotate = frames[i + ROTATE];
				x = frames[i + X];
				y = frames[i + Y];
				break;
			default:
				rotate = getBezierValue(time, i, ROTATE, curveType - BEZIER);
				x = getBezierValue(time, i, X, curveType + BEZIER_SIZE - BEZIER);
				y = getBezierValue(time, i, Y, curveType + BEZIER_SIZE * 2 - BEZIER);
			}

			if (blend == MixBlend.setup) {
				data = constraint.data;
				constraint.mixRotate = data.mixRotate + (rotate - data.mixRotate) * alpha;
				constraint.mixX = data.mixX + (x - data.mixX) * alpha;
				constraint.mixY = data.mixY + (y - data.mixY) * alpha;
			} else {
				constraint.mixRotate += (rotate - constraint.mixRotate) * alpha;
				constraint.mixX += (x - constraint.mixX) * alpha;
				constraint.mixY += (y - constraint.mixY) * alpha;
			}
		}
	}
}
