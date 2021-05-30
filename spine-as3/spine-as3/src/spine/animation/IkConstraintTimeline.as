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
	import spine.IkConstraint;
	import spine.Skeleton;

	public class IkConstraintTimeline extends CurveTimeline {
		static internal const ENTRIES : int = 6;
		static internal const MIX : int = 1, SOFTNESS : int = 2, BEND_DIRECTION : int = 3, COMPRESS : int = 4, STRETCH : int = 5;

		/** The index of the IK constraint slot in {@link Skeleton#ikConstraints} that will be changed. */
		public var ikConstraintIndex : int;

		public function IkConstraintTimeline(frameCount : int, bezierCount : int, ikConstraintIndex : int) {
			super(frameCount, bezierCount, [
				Property.ikConstraint + "|" + ikConstraintIndex
			]);
			this.ikConstraintIndex = ikConstraintIndex;
		}

		public override function getFrameEntries() : int {
			return ENTRIES;
		}

		/** Sets the time in seconds, mix, softness, bend direction, compress, and stretch for the specified key frame. */
		public function setFrame (frame : int, time : Number, mix : Number, softness : Number, bendDirection : int, compress: Boolean, stretch : Boolean) : void {
			frame *= ENTRIES;
			frames[frame] = time;
			frames[frame + MIX] = mix;
			frames[frame + SOFTNESS] = softness;
			frames[frame + BEND_DIRECTION] = bendDirection;
			frames[frame + COMPRESS] = compress ? 1 : 0;
			frames[frame + STRETCH] = stretch ? 1 : 0;
		}

		public override function apply (skeleton : Skeleton, lastTime : Number, time : Number, events : Vector.<Event>, alpha : Number, blend : MixBlend, direction : MixDirection) : void {
			var constraint : IkConstraint = skeleton.ikConstraints[ikConstraintIndex];
			if (!constraint.active) return;

			var frames : Vector.<Number> = this.frames;
			if (time < frames[0]) {
				switch (blend) {
				case MixBlend.setup:
					constraint.mix = constraint.data.mix;
					constraint.softness = constraint.data.softness;
					constraint.bendDirection = constraint.data.bendDirection;
					constraint.compress = constraint.data.compress;
					constraint.stretch = constraint.data.stretch;
					return;
				case MixBlend.first:
					constraint.mix += (constraint.data.mix - constraint.mix) * alpha;
					constraint.softness += (constraint.data.softness - constraint.softness) * alpha;
					constraint.bendDirection = constraint.data.bendDirection;
					constraint.compress = constraint.data.compress;
					constraint.stretch = constraint.data.stretch;
				}
				return;
			}

			var mix : Number = 0, softness : Number = 0;
			var i : int = search2(frames, time, ENTRIES)
			var curveType : Number = curves[i / ENTRIES];
			switch (curveType) {
			case LINEAR:
				var before : Number = frames[i];
				mix = frames[i + MIX];
				softness = frames[i + SOFTNESS];
				var t : Number = (time - before) / (frames[i + ENTRIES] - before);
				mix += (frames[i + ENTRIES + MIX] - mix) * t;
				softness += (frames[i + ENTRIES + SOFTNESS] - softness) * t;
				break;
			case STEPPED:
				mix = frames[i + MIX];
				softness = frames[i + SOFTNESS];
				break;
			default:
				mix = getBezierValue(time, i, MIX, curveType - BEZIER);
				softness = getBezierValue(time, i, SOFTNESS, curveType + BEZIER_SIZE - BEZIER);
			}

			if (blend == MixBlend.setup) {
				constraint.mix = constraint.data.mix + (mix - constraint.data.mix) * alpha;
				constraint.softness = constraint.data.softness + (softness - constraint.data.softness) * alpha;

				if (direction == MixDirection.mixOut) {
					constraint.bendDirection = constraint.data.bendDirection;
					constraint.compress = constraint.data.compress;
					constraint.stretch = constraint.data.stretch;
				} else {
					constraint.bendDirection = frames[i + BEND_DIRECTION];
					constraint.compress = frames[i + COMPRESS] != 0;
					constraint.stretch = frames[i + STRETCH] != 0;
				}
			} else {
				constraint.mix += (mix - constraint.mix) * alpha;
				constraint.softness += (softness - constraint.softness) * alpha;
				if (direction == MixDirection.mixIn) {
					constraint.bendDirection = frames[i + BEND_DIRECTION];
					constraint.compress = frames[i + COMPRESS] != 0;
					constraint.stretch = frames[i + STRETCH] != 0;
				}
			}
		}
	}
}
