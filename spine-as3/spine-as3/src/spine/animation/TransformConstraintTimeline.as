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
	import spine.TransformConstraintData;
	import spine.Event;
	import spine.Skeleton;
	import spine.TransformConstraint;

	public class TransformConstraintTimeline extends CurveTimeline {
		static internal const ENTRIES : int = 7;
		static internal const ROTATE : int = 1, X : int = 2, Y : int = 3, SCALEX : int = 4, SCALEY : int = 5, SHEARY : int = 6;

		/** The index of the transform constraint slot in {@link Skeleton#transformConstraints} that will be changed. */
		public var transformConstraintIndex : int;

		public function TransformConstraintTimeline(frameCount : int, bezierCount : int, transformConstraintIndex : int) {
			super(frameCount, bezierCount, [
				Property.transformConstraint + "|" + transformConstraintIndex
			]);
			this.transformConstraintIndex = transformConstraintIndex;
		}

		public override function getFrameEntries() : int {
			return ENTRIES;
		}

		/** The time in seconds, rotate mix, translate mix, scale mix, and shear mix for the specified key frame. */
		public function setFrame (frame : int, time : Number, mixRotate: Number, mixX: Number, mixY: Number, mixScaleX: Number, mixScaleY: Number, mixShearY: Number) : void {
			frame *= ENTRIES;
			frames[frame] = time;
			frames[frame + ROTATE] = mixRotate;
			frames[frame + X] = mixX;
			frames[frame + Y] = mixY;
			frames[frame + SCALEX] = mixScaleX;
			frames[frame + SCALEY] = mixScaleY;
			frames[frame + SHEARY] = mixShearY;
		}

		public override function apply (skeleton : Skeleton, lastTime : Number, time : Number, events : Vector.<Event>, alpha : Number, blend : MixBlend, direction : MixDirection) : void {
			var constraint : TransformConstraint = skeleton.transformConstraints[transformConstraintIndex];
			if (!constraint.active) return;

			var data : TransformConstraintData;

			var frames : Vector.<Number> = this.frames;
			if (time < frames[0]) {
				data = constraint.data;
				switch (blend) {
				case MixBlend.setup:
					constraint.mixRotate = data.mixRotate;
					constraint.mixX = data.mixX;
					constraint.mixY = data.mixY;
					constraint.mixScaleX = data.mixScaleX;
					constraint.mixScaleY = data.mixScaleY;
					constraint.mixShearY = data.mixShearY;
					return;
				case MixBlend.first:
					constraint.mixRotate += (data.mixRotate - constraint.mixRotate) * alpha;
					constraint.mixX += (data.mixX - constraint.mixX) * alpha;
					constraint.mixY += (data.mixY - constraint.mixY) * alpha;
					constraint.mixScaleX += (data.mixScaleX - constraint.mixScaleX) * alpha;
					constraint.mixScaleY += (data.mixScaleY - constraint.mixScaleY) * alpha;
					constraint.mixShearY += (data.mixShearY - constraint.mixShearY) * alpha;
				}
				return;
			}

			var rotate : Number, x : Number, y : Number, scaleX : Number, scaleY : Number, shearY : Number;
			var i : int = search(frames, time, ENTRIES);
			var curveType : Number = curves[i / ENTRIES];
			switch (curveType) {
			case LINEAR:
				var before : Number = frames[i];
				rotate = frames[i + ROTATE];
				x = frames[i + X];
				y = frames[i + Y];
				scaleX = frames[i + SCALEX];
				scaleY = frames[i + SCALEY];
				shearY = frames[i + SHEARY];
				var t : Number = (time - before) / (frames[i + ENTRIES] - before);
				rotate += (frames[i + ENTRIES + ROTATE] - rotate) * t;
				x += (frames[i + ENTRIES + X] - x) * t;
				y += (frames[i + ENTRIES + Y] - y) * t;
				scaleX += (frames[i + ENTRIES + SCALEX] - scaleX) * t;
				scaleY += (frames[i + ENTRIES + SCALEY] - scaleY) * t;
				shearY += (frames[i + ENTRIES + SHEARY] - shearY) * t;
				break;
			case STEPPED:
				rotate = frames[i + ROTATE];
				x = frames[i + X];
				y = frames[i + Y];
				scaleX = frames[i + SCALEX];
				scaleY = frames[i + SCALEY];
				shearY = frames[i + SHEARY];
				break;
			default:
				rotate = getBezierValue(time, i, ROTATE, curveType - BEZIER);
				x = getBezierValue(time, i, X, curveType + BEZIER_SIZE - BEZIER);
				y = getBezierValue(time, i, Y, curveType + BEZIER_SIZE * 2 - BEZIER);
				scaleX = getBezierValue(time, i, SCALEX, curveType + BEZIER_SIZE * 3 - BEZIER);
				scaleY = getBezierValue(time, i, SCALEY, curveType + BEZIER_SIZE * 4 - BEZIER);
				shearY = getBezierValue(time, i, SHEARY, curveType + BEZIER_SIZE * 5 - BEZIER);
			}

			if (blend == MixBlend.setup) {
				data = constraint.data;
				constraint.mixRotate = data.mixRotate + (rotate - data.mixRotate) * alpha;
				constraint.mixX = data.mixX + (x - data.mixX) * alpha;
				constraint.mixY = data.mixY + (y - data.mixY) * alpha;
				constraint.mixScaleX = data.mixScaleX + (scaleX - data.mixScaleX) * alpha;
				constraint.mixScaleY = data.mixScaleY + (scaleY - data.mixScaleY) * alpha;
				constraint.mixShearY = data.mixShearY + (shearY - data.mixShearY) * alpha;
			} else {
				constraint.mixRotate += (rotate - constraint.mixRotate) * alpha;
				constraint.mixX += (x - constraint.mixX) * alpha;
				constraint.mixY += (y - constraint.mixY) * alpha;
				constraint.mixScaleX += (scaleX - constraint.mixScaleX) * alpha;
				constraint.mixScaleY += (scaleY - constraint.mixScaleY) * alpha;
				constraint.mixShearY += (shearY - constraint.mixShearY) * alpha;
			}
		}
	}
}
