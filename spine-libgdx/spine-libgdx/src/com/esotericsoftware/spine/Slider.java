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

package com.esotericsoftware.spine;

import com.esotericsoftware.spine.Animation.MixFrom;
import com.esotericsoftware.spine.Animation.ConstraintTimeline;
import com.esotericsoftware.spine.Animation.DrawOrderFolderTimeline;
import com.esotericsoftware.spine.Animation.DrawOrderTimeline;
import com.esotericsoftware.spine.Animation.PhysicsConstraintTimeline;
import com.esotericsoftware.spine.Animation.SlotTimeline;
import com.esotericsoftware.spine.Animation.Timeline;

/** Applies an animation based on either the slider's {@link SliderPose#time} or a bone's transform property.
 * <p>
 * See <a href="https://esotericsoftware.com/spine-sliders">Sliders</a> in the Spine User Guide. */
public class Slider extends Constraint<Slider, SliderData, SliderPose> {
	static private final float[] offsets = new float[6];

	Bone bone;

	public Slider (SliderData data, Skeleton skeleton) {
		super(data, new SliderPose(), new SliderPose());
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");

		if (data.bone != null) bone = skeleton.bones.items[data.bone.index];
	}

	public Slider copy (Skeleton skeleton) {
		var copy = new Slider(data, skeleton);
		copy.pose.set(pose);
		return copy;
	}

	public void update (Skeleton skeleton, Physics physics) {
		SliderPose p = appliedPose;
		if (p.mix == 0) return;

		Animation animation = data.animation;
		if (bone != null) {
			if (!bone.active) return;
			if (data.local) bone.appliedPose.validateLocalTransform(skeleton);
			p.time = data.offset
				+ (data.property.value(skeleton, bone.appliedPose, data.local, offsets) - data.property.offset) * data.scale;
			if (data.loop)
				p.time = animation.duration + (p.time % animation.duration);
			else
				p.time = Math.max(0, p.time);
		}

		Bone[] bones = skeleton.bones.items;
		int[] indices = animation.bones.items;
		for (int i = 0, n = animation.bones.size; i < n; i++)
			bones[indices[i]].appliedPose.modifyLocal(skeleton);

		animation.apply(skeleton, p.time, p.time, data.loop, null, p.mix, MixFrom.current, data.additive, false, true);
	}

	void sort (Skeleton skeleton) {
		if (bone != null && !data.local) skeleton.sortBone(bone);
		skeleton.updateCache.add(this);

		Bone[] bones = skeleton.bones.items;
		int[] indices = data.animation.bones.items;
		for (int i = 0, n = data.animation.bones.size; i < n; i++) {
			Bone bone = bones[indices[i]];
			bone.sorted = false;
			skeleton.sortReset(bone.children);
			skeleton.constrained(bone);
		}

		Timeline[] timelines = data.animation.timelines.items;
		Slot[] slots = skeleton.slots.items;
		Constraint[] constraints = skeleton.constraints.items;
		PhysicsConstraint[] physics = skeleton.physics.items;
		int physicsCount = skeleton.physics.size;
		for (int i = 0, n = data.animation.timelines.size; i < n; i++) {
			Timeline t = timelines[i];
			if (t instanceof SlotTimeline timeline)
				skeleton.constrained(slots[timeline.getSlotIndex()]);
			else if (t instanceof DrawOrderTimeline || t instanceof DrawOrderFolderTimeline)
				skeleton.drawOrder.constrained();
			else if (t instanceof PhysicsConstraintTimeline timeline) {
				if (timeline.constraintIndex == -1) {
					for (int ii = 0; ii < physicsCount; ii++)
						skeleton.constrained(physics[ii]);
				} else
					skeleton.constrained(constraints[timeline.constraintIndex]);
			} else if (t instanceof ConstraintTimeline timeline) {
				int constraintIndex = timeline.getConstraintIndex();
				if (constraintIndex != -1) skeleton.constrained(constraints[constraintIndex]);
			}
		}
	}

	/** When set, the bone's transform property is used to set the slider's {@link SliderPose#time}. */
	public Bone getBone () {
		return bone;
	}

	public void setBone (Bone bone) {
		this.bone = bone;
	}
}
