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
	import spine.MathUtils;

	public class ScaleYTimeline extends CurveTimeline1 implements BoneTimeline {
		private var boneIndex : int;

		public function ScaleYTimeline(frameCount : int, bezierCount : int, boneIndex : int) {
			super(frameCount, bezierCount, Property.scaleY + "|" + boneIndex);
			this.boneIndex = boneIndex;
		}

		public function getBoneIndex() : int {
			return boneIndex;
		}

		public override function apply (skeleton : Skeleton, lastTime : Number, time : Number, events : Vector.<Event>, alpha : Number, blend : MixBlend, direction : MixDirection) : void {
			var bone : Bone = skeleton.bones[boneIndex];
			if (!bone.active) return;

			var frames : Vector.<Number> = this.frames;
			if (time < frames[0]) {
				switch (blend) {
				case MixBlend.setup:
					bone.scaleY = bone.data.scaleY;
					return;
				case MixBlend.first:
					bone.scaleY += (bone.data.scaleY - bone.scaleY) * alpha;
				}
				return;
			}

			var y : Number = getCurveValue(time) * bone.data.scaleY;
			if (alpha == 1) {
				if (blend == MixBlend.add)
					bone.scaleY += y - bone.data.scaleY;
				else
					bone.scaleY = y;
			} else {
				// Mixing out uses sign of setup or current pose, else use sign of key.
				var by : Number = 0;
				if (direction == MixDirection.mixOut) {
					switch (blend) {
					case MixBlend.setup:
						by = bone.data.scaleY;
						bone.scaleY = by + (Math.abs(y) * MathUtils.signum(by) - by) * alpha;
						break;
					case MixBlend.first:
					case MixBlend.replace:
						by = bone.scaleY;
						bone.scaleY = by + (Math.abs(y) * MathUtils.signum(by) - by) * alpha;
						break;
					case MixBlend.add:
						by = bone.scaleY;
						bone.scaleY = by + (Math.abs(y) * MathUtils.signum(by) - bone.data.scaleY) * alpha;
					}
				} else {
					switch (blend) {
					case MixBlend.setup:
						by = Math.abs(bone.data.scaleY) * MathUtils.signum(y);
						bone.scaleY = by + (y - by) * alpha;
						break;
					case MixBlend.first:
					case MixBlend.replace:
						by = Math.abs(bone.scaleY) * MathUtils.signum(y);
						bone.scaleY = by + (y - by) * alpha;
						break;
					case MixBlend.add:
						by = MathUtils.signum(y);
						bone.scaleY = Math.abs(bone.scaleY) * by + (y - Math.abs(bone.data.scaleY) * by) * alpha;
					}
				}
			}
		}
	}
}
