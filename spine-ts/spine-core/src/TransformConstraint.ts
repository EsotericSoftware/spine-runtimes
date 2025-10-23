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

import type { Bone } from "./Bone.js";
import type { BonePose } from "./BonePose.js";
import { Constraint } from "./Constraint.js";
import type { Physics } from "./Physics.js";
import type { Skeleton } from "./Skeleton.js";
import type { TransformConstraintData } from "./TransformConstraintData.js";
import { TransformConstraintPose } from "./TransformConstraintPose.js";
import { MathUtils } from "./Utils.js";


/** Stores the current pose for a transform constraint. A transform constraint adjusts the world transform of the constrained
 * bones to match that of the source bone.
 *
 * See [Transform constraints](http://esotericsoftware.com/spine-transform-constraints) in the Spine User Guide. */
export class TransformConstraint extends Constraint<TransformConstraint, TransformConstraintData, TransformConstraintPose> {

	/** The bones that will be modified by this transform constraint. */
	bones: Array<BonePose>;

	/** The bone whose world transform will be copied to the constrained bones. */
	source: Bone;

	constructor (data: TransformConstraintData, skeleton: Skeleton) {
		super(data, new TransformConstraintPose(), new TransformConstraintPose());
		if (!skeleton) throw new Error("skeleton cannot be null.");

		this.bones = [] as BonePose[];
		for (const boneData of data.bones)
			this.bones.push(skeleton.bones[boneData.index].constrained);

		const source = skeleton.bones[data.source.index];
		if (source == null) throw new Error("source cannot be null.");
		this.source = source;
	}

	public copy (skeleton: Skeleton) {
		var copy = new TransformConstraint(this.data, skeleton);
		copy.pose.set(this.pose);
		return copy;
	}

	update (skeleton: Skeleton, physics: Physics) {
		const p = this.applied;
		if (p.mixRotate === 0 && p.mixX === 0 && p.mixY === 0 && p.mixScaleX === 0 && p.mixScaleY === 0 && p.mixShearY === 0) return;

		const data = this.data;
		const localSource = data.localSource, localTarget = data.localTarget, additive = data.additive, clamp = data.clamp;
		const offsets = data.offsets;
		const source = this.source.applied;
		if (localSource) source.validateLocalTransform(skeleton);
		const fromItems = data.properties;
		const fn = data.properties.length, update = skeleton._update;
		const bones = this.bones;
		for (let i = 0, n = this.bones.length; i < n; i++) {
			const bone = bones[i];
			if (localTarget)
				bone.modifyLocal(skeleton);
			else
				bone.modifyWorld(update);
			for (let f = 0; f < fn; f++) {
				const from = fromItems[f];
				const value = from.value(skeleton, source, localSource, offsets) - from.offset;
				const toItems = from.to;
				for (let t = 0, tn = from.to.length; t < tn; t++) {
					const to = toItems[t];
					if (to.mix(p) !== 0) {
						let clamped = to.offset + value * to.scale;
						if (clamp) {
							if (to.offset < to.max)
								clamped = MathUtils.clamp(clamped, to.offset, to.max);
							else
								clamped = MathUtils.clamp(clamped, to.max, to.offset);
						}
						to.apply(skeleton, p, bone, clamped, localTarget, additive);
					}
				}
			}
		}
	}

	sort (skeleton: Skeleton) {
		if (!this.data.localSource) skeleton.sortBone(this.source);
		const bones = this.bones;
		const boneCount = this.bones.length;
		const worldTarget = !this.data.localTarget;
		if (worldTarget) {
			for (let i = 0; i < boneCount; i++)
				skeleton.sortBone(bones[i].bone);
		}
		skeleton._updateCache.push(this);
		for (let i = 0; i < boneCount; i++) {
			const bone = bones[i].bone;
			skeleton.sortReset(bone.children);
			skeleton.constrained(bone);
		}
		for (let i = 0; i < boneCount; i++)
			bones[i].bone.sorted = worldTarget;
	}

	isSourceActive () {
		return this.source.active;
	}

}
