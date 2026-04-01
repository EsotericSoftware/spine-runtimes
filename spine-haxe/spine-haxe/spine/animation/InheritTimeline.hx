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

import spine.Bone;
import spine.Event;
import spine.Skeleton;

/** Changes a bone's spine.Bone.inherit. */
class InheritTimeline extends Timeline implements BoneTimeline {
	public static inline var ENTRIES:Int = 2;
	private static inline var INHERIT:Int = 1;

	private var boneIndex:Int = 0;

	public function new(frameCount:Int, boneIndex:Int) {
		super(frameCount, Property.inherit + "|" + boneIndex);
		this.boneIndex = boneIndex;
		this.instant = true;
	}

	public function getBoneIndex() {
		return boneIndex;
	}

	public override function getFrameEntries():Int {
		return ENTRIES;
	}

	/** Sets the transform mode for the specified frame.
	 * @param frame Between 0 and frameCount, inclusive.
	 * @param time The frame time in seconds. */
	public function setFrame(frame:Int, time:Float, inherit:Inherit):Void {
		frame *= ENTRIES;
		frames[frame] = time;
		frames[frame + INHERIT] = inherit.ordinal;
	}

	public function apply(skeleton:Skeleton, lastTime:Float, time:Float, events:Array<Event>, alpha:Float, fromSetup:Bool, add:Bool, out:Bool,
			appliedPose:Bool):Void {
		var bone:Bone = skeleton.bones[boneIndex];
		if (!bone.active)
			return;
		var pose = appliedPose ? bone.appliedPose : bone.pose;

		if (out) {
			if (fromSetup)
				pose.inherit = bone.data.setupPose.inherit;
		} else {
			var frames:Array<Float> = frames;
			if (time < frames[0]) {
				if (fromSetup)
					pose.inherit = bone.data.setupPose.inherit;
			} else
				pose.inherit = Inherit.values[Std.int(frames[Timeline.search(frames, time, ENTRIES) + INHERIT])];
		}
	}
}
