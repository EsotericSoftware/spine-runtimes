/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*****************************************************************************/

package spine.animation;

import spine.Bone;
import spine.Event;
import spine.MathUtils;
import spine.Skeleton;

class ScaleTimeline extends CurveTimeline2 implements BoneTimeline {
	private var boneIndex:Int = 0;

	public function new(frameCount:Int, bezierCount:Int, boneIndex:Int) {
		super(frameCount, bezierCount, [Property.scaleX + "|" + boneIndex, Property.scaleY + "|" + boneIndex]);
		this.boneIndex = boneIndex;
	}

	public function getBoneIndex():Int {
		return boneIndex;
	}

	override public function apply(skeleton:Skeleton, lastTime:Float, time:Float, events:Array<Event>, alpha:Float, blend:MixBlend,
			direction:MixDirection):Void {
		var bone:Bone = skeleton.bones[boneIndex];
		if (!bone.active)
			return;
		if (time < frames[0]) {
			switch (blend) {
				case MixBlend.setup:
					bone.scaleX = bone.data.scaleX;
					bone.scaleY = bone.data.scaleY;
				case MixBlend.first:
					bone.scaleX += (bone.data.scaleX - bone.scaleX) * alpha;
					bone.scaleY += (bone.data.scaleY - bone.scaleY) * alpha;
			}
			return;
		}

		var x:Float = 0, y:Float = 0;
		var i:Int = Timeline.search(frames, time, CurveTimeline2.ENTRIES);
		var curveType:Int = Std.int(curves[Std.int(i / CurveTimeline2.ENTRIES)]);
		switch (curveType) {
			case CurveTimeline.LINEAR:
				var before:Float = frames[i];
				x = frames[i + CurveTimeline2.VALUE1];
				y = frames[i + CurveTimeline2.VALUE2];
				var t:Float = (time - before) / (frames[i + CurveTimeline2.ENTRIES] - before);
				x += (frames[i + CurveTimeline2.ENTRIES + CurveTimeline2.VALUE1] - x) * t;
				y += (frames[i + CurveTimeline2.ENTRIES + CurveTimeline2.VALUE2] - y) * t;
			case CurveTimeline.STEPPED:
				x = frames[i + CurveTimeline2.VALUE1];
				y = frames[i + CurveTimeline2.VALUE2];
			default:
				x = getBezierValue(time, i, CurveTimeline2.VALUE1, curveType - CurveTimeline.BEZIER);
				y = getBezierValue(time, i, CurveTimeline2.VALUE2, curveType + CurveTimeline.BEZIER_SIZE - CurveTimeline.BEZIER);
		}
		x *= bone.data.scaleX;
		y *= bone.data.scaleY;

		if (alpha == 1) {
			if (blend == MixBlend.add) {
				bone.scaleX += x - bone.data.scaleX;
				bone.scaleY += y - bone.data.scaleY;
			} else {
				bone.scaleX = x;
				bone.scaleY = y;
			}
		} else {
			var bx:Float = 0, by:Float = 0;
			if (direction == MixDirection.mixOut) {
				switch (blend) {
					case MixBlend.setup:
						bx = bone.data.scaleX;
						by = bone.data.scaleY;
						bone.scaleX = bx + (Math.abs(x) * MathUtils.signum(bx) - bx) * alpha;
						bone.scaleY = by + (Math.abs(y) * MathUtils.signum(by) - by) * alpha;
					case MixBlend.first, MixBlend.replace:
						bx = bone.scaleX;
						by = bone.scaleY;
						bone.scaleX = bx + (Math.abs(x) * MathUtils.signum(bx) - bx) * alpha;
						bone.scaleY = by + (Math.abs(y) * MathUtils.signum(by) - by) * alpha;
					case MixBlend.add:
						bone.scaleX = (x - bone.data.scaleX) * alpha;
						bone.scaleY = (y - bone.data.scaleY) * alpha;
				}
			} else {
				switch (blend) {
					case MixBlend.setup:
						bx = Math.abs(bone.data.scaleX) * MathUtils.signum(x);
						by = Math.abs(bone.data.scaleY) * MathUtils.signum(y);
						bone.scaleX = bx + (x - bx) * alpha;
						bone.scaleY = by + (y - by) * alpha;
					case MixBlend.first, MixBlend.replace:
						bx = Math.abs(bone.scaleX) * MathUtils.signum(x);
						by = Math.abs(bone.scaleY) * MathUtils.signum(y);
						bone.scaleX = bx + (x - bx) * alpha;
						bone.scaleY = by + (y - by) * alpha;
					case MixBlend.add:
						bone.scaleX += (x - bone.data.scaleX) * alpha;
						bone.scaleY += (y - bone.data.scaleY) * alpha;
				}
			}
		}
	}
}
