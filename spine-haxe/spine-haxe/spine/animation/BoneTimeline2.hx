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

/** The base class for a spine.animation.CurveTimeline that is a spine.animation.BoneTimeline and sets two properties. */
abstract class BoneTimeline2 extends CurveTimeline implements BoneTimeline {
	private static inline var ENTRIES:Int = 3;
	private static inline var VALUE1:Int = 1;
	private static inline var VALUE2:Int = 2;

	public final boneIndex:Int;

	/** @param bezierCount The maximum number of Bezier curves. See spine.animation.CurveTimeline.shrink(). */
	public function new(frameCount:Int, bezierCount:Int, boneIndex:Int, property1:Property, property2:Property) {
		super(frameCount, bezierCount, property1 + "|" + boneIndex, property2 + "|" + boneIndex);
		this.boneIndex = boneIndex;
		this.additive = true;
	}

	public override function getFrameEntries():Int {
		return ENTRIES;
	}

	/** Sets the time and values for the specified frame.
	 * @param frame Between 0 and frameCount, inclusive.
	 * @param time The frame time in seconds. */
	public function setFrame(frame:Int, time:Float, value1:Float, value2:Float):Void {
		frame *= ENTRIES;
		frames[frame] = time;
		frames[frame + VALUE1] = value1;
		frames[frame + VALUE2] = value2;
	}

	public function getBoneIndex() {
		return boneIndex;
	}

	public function apply(skeleton:Skeleton, lastTime:Float, time:Float, events:Array<Event>, alpha:Float, from:MixFrom, add:Bool, out:Bool, appliedPose:Bool) {
		var bone = skeleton.bones[boneIndex];
		if (bone.active)
			apply1(appliedPose ? bone.appliedPose : bone.pose, bone.data.setupPose, time, alpha, from, add, out);
	}

	abstract function apply1(pose:BonePose, setup:BonePose, time:Float, alpha:Float, from:MixFrom, add:Bool, out:Bool):Void;
}
