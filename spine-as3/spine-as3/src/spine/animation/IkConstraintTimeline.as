/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine.animation {
	import spine.Event;
	import spine.IkConstraint;
	import spine.Skeleton;

	public class IkConstraintTimeline extends CurveTimeline {
		static public const ENTRIES : int = 6;
		static internal const PREV_TIME : int = -6, PREV_MIX : int = -5, PREV_SOFTNESS : int = -4, PREV_BEND_DIRECTION : int = -3, PREV_COMPRESS : int = -2, PREV_STRETCH : int = -1;
		static internal const MIX : int = 1, SOFTNESS : int = 2, BEND_DIRECTION : int = 3, COMPRESS : int = 4, STRETCH : int = 5;
		public var ikConstraintIndex : int;
		public var frames : Vector.<Number>; // time, mix, bendDirection, compress, stretch, ...

		public function IkConstraintTimeline(frameCount : int) {
			super(frameCount);
			frames = new Vector.<Number>(frameCount * ENTRIES, true);
		}

		override public function getPropertyId() : int {
			return (TimelineType.ikConstraint.ordinal << 24) + ikConstraintIndex;
		}

		/** Sets the time, mix and bend direction of the specified keyframe. */
		public function setFrame(frameIndex : int, time : Number, mix : Number, softness: Number, bendDirection : int, compress: Boolean, stretch: Boolean) : void {
			frameIndex *= ENTRIES;
			frames[frameIndex] = time;
			frames[int(frameIndex + MIX)] = mix;
			frames[int(frameIndex + SOFTNESS)] = softness;
			frames[int(frameIndex + BEND_DIRECTION)] = bendDirection;
			frames[int(frameIndex + COMPRESS)] = compress ? 1 : 0;
			frames[int(frameIndex + STRETCH)] = stretch ? 1 : 0;
		}

		override public function apply(skeleton : Skeleton, lastTime : Number, time : Number, firedEvents : Vector.<Event>, alpha : Number, blend : MixBlend, direction : MixDirection) : void {
			var constraint : IkConstraint = skeleton.ikConstraints[ikConstraintIndex];
			if (!constraint.active) return;
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

			if (time >= frames[int(frames.length - ENTRIES)]) { // Time is after last frame.
				if (blend == MixBlend.setup) {
					constraint.mix = constraint.data.mix + (frames[frames.length + PREV_MIX] - constraint.data.mix) * alpha;
					constraint.softness = constraint.data.softness
						+ (frames[frames.length + PREV_SOFTNESS] - constraint.data.softness) * alpha;
					if (direction == MixDirection.Out) {
						constraint.bendDirection = constraint.data.bendDirection;
						constraint.compress = constraint.data.compress;
						constraint.stretch = constraint.data.stretch;
					} else {
						constraint.bendDirection = int(frames[frames.length + PREV_BEND_DIRECTION]);
						constraint.compress = int(frames[frames.length + PREV_COMPRESS]) != 0;
						constraint.stretch = int(frames[frames.length + PREV_STRETCH]) != 0;
					}					
				} else {
					constraint.mix += (frames[frames.length + PREV_MIX] - constraint.mix) * alpha;
					constraint.softness += (frames[frames.length + PREV_SOFTNESS] - constraint.softness) * alpha;
					if (direction == MixDirection.In) {
						constraint.bendDirection = int(frames[frames.length + PREV_BEND_DIRECTION]);
						constraint.compress = int(frames[frames.length + PREV_COMPRESS]) != 0;
						constraint.stretch = int(frames[frames.length + PREV_STRETCH]) != 0;
					}
				}
				return;
			}

			// Interpolate between the previous frame and the current frame.
			var frame : int = Animation.binarySearch(frames, time, ENTRIES);
			var mix : Number = frames[int(frame + PREV_MIX)];
			var softness : Number = frames[frame + PREV_SOFTNESS];
			var frameTime : Number = frames[frame];
			var percent : Number = getCurvePercent(frame / ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

			if (blend == MixBlend.setup) {
				constraint.mix = constraint.data.mix + (mix + (frames[frame + MIX] - mix) * percent - constraint.data.mix) * alpha;
				constraint.softness = constraint.data.softness
					+ (softness + (frames[frame + SOFTNESS] - softness) * percent - constraint.data.softness) * alpha;
				if (direction == MixDirection.Out) {
					constraint.bendDirection = constraint.data.bendDirection;
					constraint.compress = constraint.data.compress;
					constraint.stretch = constraint.data.stretch;	
				} else {
					constraint.bendDirection = int(frames[frame + PREV_BEND_DIRECTION]);
					constraint.compress = int(frames[frame + PREV_COMPRESS]) != 0;
					constraint.stretch = int(frames[frame + PREV_STRETCH]) != 0;
				}				
			} else {
				constraint.mix += (mix + (frames[frame + MIX] - mix) * percent - constraint.mix) * alpha;
				constraint.softness += (softness + (frames[frame + SOFTNESS] - softness) * percent - constraint.softness) * alpha;
				if (direction == MixDirection.In) {
					constraint.bendDirection = int(frames[frame + PREV_BEND_DIRECTION]);
					constraint.compress = int(frames[frame + PREV_COMPRESS]) != 0;
					constraint.stretch = int(frames[frame + PREV_STRETCH]) != 0;
				}
			}
		}
	}
}
