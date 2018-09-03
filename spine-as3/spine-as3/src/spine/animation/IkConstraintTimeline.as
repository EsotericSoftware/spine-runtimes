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
	import spine.IkConstraint;
	import spine.Skeleton;

	public class IkConstraintTimeline extends CurveTimeline {
		static public const ENTRIES : int = 5;
		static internal const PREV_TIME : int = -5, PREV_MIX : int = -4, PREV_BEND_DIRECTION : int = -3, PREV_COMPRESS : int = -2, PREV_STRETCH : int = -1;
		static internal const MIX : int = 1, BEND_DIRECTION : int = 2, COMPRESS : int = 3, STRETCH : int = 4;
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
		public function setFrame(frameIndex : int, time : Number, mix : Number, bendDirection : int, compress: Boolean, stretch: Boolean) : void {
			frameIndex *= ENTRIES;
			frames[frameIndex] = time;
			frames[int(frameIndex + MIX)] = mix;
			frames[int(frameIndex + BEND_DIRECTION)] = bendDirection;
			frames[int(frameIndex + COMPRESS)] = compress ? 1 : 0;
			frames[int(frameIndex + STRETCH)] = stretch ? 1 : 0;
		}

		override public function apply(skeleton : Skeleton, lastTime : Number, time : Number, firedEvents : Vector.<Event>, alpha : Number, blend : MixBlend, direction : MixDirection) : void {
			var constraint : IkConstraint = skeleton.ikConstraints[ikConstraintIndex];
			if (time < frames[0]) {
				switch (blend) {
				case MixBlend.setup:
					constraint.mix = constraint.data.mix;
					constraint.bendDirection = constraint.data.bendDirection;
					constraint.compress = constraint.data.compress;
					constraint.stretch = constraint.data.stretch;
					return;
				case MixBlend.first:
					constraint.mix += (constraint.data.mix - constraint.mix) * alpha;
					constraint.bendDirection = constraint.data.bendDirection;
					constraint.compress = constraint.data.compress;
					constraint.stretch = constraint.data.stretch;
				}
				return;
			}

			if (time >= frames[int(frames.length - ENTRIES)]) { // Time is after last frame.
				if (blend == MixBlend.setup) {
					constraint.mix = constraint.data.mix + (frames[frames.length + PREV_MIX] - constraint.data.mix) * alpha;
					
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
			var frameTime : Number = frames[frame];
			var percent : Number = getCurvePercent(frame / ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

			if (blend == MixBlend.setup) {
				constraint.mix = constraint.data.mix + (mix + (frames[frame + MIX] - mix) * percent - constraint.data.mix) * alpha;
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
				if (direction == MixDirection.In) {
					constraint.bendDirection = int(frames[frame + PREV_BEND_DIRECTION]);
					constraint.compress = int(frames[frame + PREV_COMPRESS]) != 0;
					constraint.stretch = int(frames[frame + PREV_STRETCH]) != 0;
				}
			}
		}
	}
}