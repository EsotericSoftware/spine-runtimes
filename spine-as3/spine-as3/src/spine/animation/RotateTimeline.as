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
	import spine.Bone;
	import spine.Event;
	import spine.Skeleton;

	public class RotateTimeline extends CurveTimeline {
		static public const ENTRIES : int = 2;
		static public const PREV_TIME : int = -2, PREV_ROTATION : int = -1;
		static public const ROTATION : int = 1;
		public var boneIndex : int;
		public var frames : Vector.<Number>; // time, value, ...

		public function RotateTimeline(frameCount : int) {
			super(frameCount);
			frames = new Vector.<Number>(frameCount * 2, true);
		}

		override public function getPropertyId() : int {
			return (TimelineType.rotate.ordinal << 24) + boneIndex;
		}

		/** Sets the time and angle of the specified keyframe. */
		public function setFrame(frameIndex : int, time : Number, degrees : Number) : void {
			frameIndex <<= 1;
			frames[frameIndex] = time;
			frames[int(frameIndex + ROTATION)] = degrees;
		}

		override public function apply(skeleton : Skeleton, lastTime : Number, time : Number, firedEvents : Vector.<Event>, alpha : Number, blend : MixBlend, direction : MixDirection) : void {
			var frames : Vector.<Number> = this.frames;

			var bone : Bone = skeleton.bones[boneIndex];
			if (!bone.active) return;
			var r : Number;
			if (time < frames[0]) {
				switch (blend) {
				case MixBlend.setup:
					bone.rotation = bone.data.rotation;
					return;
				case MixBlend.first:
					r = bone.data.rotation - bone.rotation;
					bone.rotation += (r - (16384 - int((16384.499999999996 - r / 360))) * 360) * alpha;
				}
				return;
			}

			if (time >= frames[frames.length - ENTRIES]) { // Time is after last frame.
				r = frames[frames.length + PREV_ROTATION];
				switch (blend) {
					case MixBlend.setup:
						bone.rotation = bone.data.rotation + r * alpha;
						break;
					case MixBlend.first:
					case MixBlend.replace:
						r += bone.data.rotation - bone.rotation;
						r -= (16384 - int((16384.499999999996 - r / 360))) * 360; // Wrap within -180 and 180.
					case MixBlend.add:
						bone.rotation += r * alpha;
				}
				return;
			}

			// Interpolate between the previous frame and the current frame.
			var frame : int = Animation.binarySearch(frames, time, ENTRIES);
			var prevRotation : Number = frames[frame + PREV_ROTATION];
			var frameTime : Number = frames[frame];
			var percent : Number = getCurvePercent((frame >> 1) - 1, 1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

			r = frames[frame + ROTATION] - prevRotation;
			r = prevRotation + (r - (16384 - int((16384.499999999996 - r / 360))) * 360) * percent;
			switch (blend) {
				case MixBlend.setup:
					bone.rotation = bone.data.rotation + (r - (16384 - int((16384.499999999996 - r / 360))) * 360) * alpha;
					break;
				case MixBlend.first:
				case MixBlend.replace:
					r += bone.data.rotation - bone.rotation;
				case MixBlend.add:
					bone.rotation += (r - (16384 - int((16384.499999999996 - r / 360))) * 360) * alpha;
			}
		}
	}
}
