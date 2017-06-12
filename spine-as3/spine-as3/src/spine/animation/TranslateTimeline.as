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
	import spine.Bone;
	import spine.Event;
	import spine.Skeleton;

	public class TranslateTimeline extends CurveTimeline {
		static public const ENTRIES : int = 3;
		static internal const PREV_TIME : int = -3, PREV_X : int = -2, PREV_Y : int = -1;
		static internal const X : int = 1, Y : int = 2;
		public var boneIndex : int;
		public var frames : Vector.<Number>; // time, value, value, ...

		public function TranslateTimeline(frameCount : int) {
			super(frameCount);
			frames = new Vector.<Number>(frameCount * ENTRIES, true);
		}

		override public function getPropertyId() : int {
			return (TimelineType.translate.ordinal << 24) + boneIndex;
		}

		/** Sets the time and value of the specified keyframe. */
		public function setFrame(frameIndex : int, time : Number, x : Number, y : Number) : void {
			frameIndex *= ENTRIES;
			frames[frameIndex] = time;
			frames[int(frameIndex + X)] = x;
			frames[int(frameIndex + Y)] = y;
		}

		override public function apply(skeleton : Skeleton, lastTime : Number, time : Number, firedEvents : Vector.<Event>, alpha : Number, pose : MixPose, direction : MixDirection) : void {
			var frames : Vector.<Number> = this.frames;

			var bone : Bone = skeleton.bones[boneIndex];
			if (time < frames[0]) {
				switch (pose) {
				case MixPose.setup:
					bone.x = bone.data.x;
					bone.y = bone.data.y;
					return;
				case MixPose.current:
					bone.x += (bone.data.x - bone.x) * alpha;
					bone.y += (bone.data.y - bone.y) * alpha;
				}
				return;
			}

			var x : Number, y : Number;
			if (time >= frames[frames.length - ENTRIES]) { // Time is after last frame.
				x = frames[frames.length + PREV_X];
				y = frames[frames.length + PREV_Y];
			} else {
				// Interpolate between the previous frame and the current frame.
				var frame : int = Animation.binarySearch(frames, time, ENTRIES);
				x = frames[frame + PREV_X];
				y = frames[frame + PREV_Y];
				var frameTime : Number = frames[frame];
				var percent : Number = getCurvePercent(frame / ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

				x += (frames[frame + X] - x) * percent;
				y += (frames[frame + Y] - y) * percent;
			}
			if (pose == MixPose.setup) {
				bone.x = bone.data.x + x * alpha;
				bone.y = bone.data.y + y * alpha;
			} else {
				bone.x += (bone.data.x + x - bone.x) * alpha;
				bone.y += (bone.data.y + y - bone.y) * alpha;
			}
		}
	}
}