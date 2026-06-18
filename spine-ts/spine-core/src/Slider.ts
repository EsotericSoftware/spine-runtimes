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

import { DrawOrderFolderTimeline, DrawOrderTimeline, isConstraintTimeline, isSlotTimeline, MixFrom, PhysicsConstraintTimeline } from "./Animation.js";
import type { Bone } from "./Bone.js";
import { Constraint } from "./Constraint.js";
import type { Physics } from "./Physics.js";
import type { Skeleton } from "./Skeleton.js";
import type { SliderData } from "./SliderData.js";
import { SliderPose } from "./SliderPose.js";

/** Applies an animation based on either the slider's {@link SliderPose.time} or a bone's transform property.
 *
 * See <a href="https://esotericsoftware.com/spine-sliders">Sliders</a> in the Spine User Guide. */
export class Slider extends Constraint<Slider, SliderData, SliderPose> {
	private static readonly offsets = [0, 0, 0, 0, 0, 0];

	/** When set, the bone's transform property is used to set the slider's {@link SliderPose.time}. */
	bone: Bone | null = null;

	constructor (data: SliderData, skeleton: Skeleton) {
		super(data, new SliderPose(), new SliderPose());
		if (!skeleton) throw new Error("skeleton cannot be null.");

		if (data.bone != null) this.bone = skeleton.bones[data.bone.index];
	}

	public copy (skeleton: Skeleton) {
		var copy = new Slider(this.data, skeleton);
		copy.pose.set(this.pose);
		return copy;
	}

	public update (skeleton: Skeleton, physics: Physics) {
		const p = this.appliedPose;
		if (p.mix === 0) return;

		const data = this.data, animation = data.animation, bone = this.bone;
		if (bone !== null) {
			if (!bone.active) return;
			if (data.local) bone.appliedPose.validateLocalTransform(skeleton);
			p.time = data.offset
				+ (data.property.value(skeleton, bone.appliedPose, data.local, Slider.offsets) - data.property.offset) * data.scale;
			if (data.loop)
				p.time = animation.duration + (p.time % animation.duration);
			else
				p.time = Math.max(0, p.time);
		}

		const bones = skeleton.bones;
		const indices = animation.bones;
		for (let i = 0, n = animation.bones.length; i < n; i++)
			bones[indices[i]].appliedPose.modifyLocal(skeleton);

		animation.apply(skeleton, p.time, p.time, data.loop, null, p.mix, MixFrom.current, data.additive, false, true);
	}

	sort (skeleton: Skeleton) {
		const bone = this.bone;
		const data = this.data;
		if (bone && !data.local) skeleton.sortBone(bone);
		skeleton._updateCache.push(this);

		const bones = skeleton.bones;
		const indices = data.animation.bones;
		for (let i = 0, n = data.animation.bones.length; i < n; i++) {
			const bone = bones[indices[i]];
			bone.sorted = false;
			skeleton.sortReset(bone.children);
			skeleton.constrained(bone);
		}

		const timelines = data.animation.timelines;
		const slots = skeleton.slots;
		const constraints = skeleton.constraints;
		const physics = skeleton.physics;
		const physicsCount = skeleton.physics.length;
		for (let i = 0, n = data.animation.timelines.length; i < n; i++) {
			const t = timelines[i];
			if (isSlotTimeline(t))
				skeleton.constrained(slots[t.slotIndex]);
			else if (t instanceof DrawOrderTimeline || t instanceof DrawOrderFolderTimeline)
				skeleton.drawOrder.constrained();
			else if (t instanceof PhysicsConstraintTimeline) {
				if (t.constraintIndex === -1) {
					for (let ii = 0; ii < physicsCount; ii++)
						skeleton.constrained(physics[ii]);
				} else
					skeleton.constrained(constraints[t.constraintIndex]);
			} else if (isConstraintTimeline(t)) {
				const constraintIndex = t.constraintIndex;
				if (constraintIndex !== -1) skeleton.constrained(constraints[constraintIndex]);
			}
		}
	}
}
