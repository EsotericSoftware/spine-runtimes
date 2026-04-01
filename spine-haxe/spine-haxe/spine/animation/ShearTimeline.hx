/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

package spine.animation;

/** Changes a bone's local spine.Bone.shearX and spine.Bone.shearY. */
class ShearTimeline extends BoneTimeline2 {
	public function new(frameCount:Int, bezierCount:Int, boneIndex:Int) {
		super(frameCount, bezierCount, boneIndex, Property.shearX, Property.shearY);
	}

	public function apply1(pose:BonePose, setup:BonePose, time:Float, alpha:Float, fromSetup:Bool, add:Bool, out:Bool) {
		if (time < frames[0]) {
			if (fromSetup) {
				pose.shearX = setup.shearX;
				pose.shearY = setup.shearY;
			}
			return;
		}

		var x:Float = 0, y:Float = 0;
		var i:Int = Timeline.search(frames, time, BoneTimeline2.ENTRIES);
		var curveType:Int = Std.int(curves[Std.int(i / BoneTimeline2.ENTRIES)]);
		switch (curveType) {
			case CurveTimeline.LINEAR:
				var before:Float = frames[i];
				x = frames[i + BoneTimeline2.VALUE1];
				y = frames[i + BoneTimeline2.VALUE2];
				var t:Float = (time - before) / (frames[i + BoneTimeline2.ENTRIES] - before);
				x += (frames[i + BoneTimeline2.ENTRIES + BoneTimeline2.VALUE1] - x) * t;
				y += (frames[i + BoneTimeline2.ENTRIES + BoneTimeline2.VALUE2] - y) * t;
			case CurveTimeline.STEPPED:
				x = frames[i + BoneTimeline2.VALUE1];
				y = frames[i + BoneTimeline2.VALUE2];
			default:
				x = getBezierValue(time, i, BoneTimeline2.VALUE1, curveType - CurveTimeline.BEZIER);
				y = getBezierValue(time, i, BoneTimeline2.VALUE2, curveType + CurveTimeline.BEZIER_SIZE - CurveTimeline.BEZIER);
		}

		if (fromSetup) {
			pose.shearX = setup.shearX + x * alpha;
			pose.shearY = setup.shearY + y * alpha;
		} else if (add) {
			pose.shearX += x * alpha;
			pose.shearY += y * alpha;
		} else {
			pose.shearX += (setup.shearX + x - pose.shearX) * alpha;
			pose.shearY += (setup.shearY + y - pose.shearY) * alpha;
		}
	}
}
