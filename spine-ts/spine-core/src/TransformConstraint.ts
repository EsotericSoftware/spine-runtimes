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

import { Bone } from "./Bone.js";
import { Physics, Skeleton } from "./Skeleton.js";
import { TransformConstraintData } from "./TransformConstraintData.js";
import { Updatable } from "./Updatable.js";
import { Vector2, MathUtils } from "./Utils.js";


/** Stores the current pose for a transform constraint. A transform constraint adjusts the world transform of the constrained
 * bones to match that of the source bone.
 *
 * See [Transform constraints](https://esotericsoftware.com/spine-transform-constraints) in the Spine User Guide. */
export class TransformConstraint implements Updatable {

	/** The transform constraint's setup pose data. */
	data: TransformConstraintData;

	/** The bones that will be modified by this transform constraint. */
	bones: Array<Bone>;

	/** The bone whose world transform will be copied to the constrained bones. */
	source: Bone;

	mixRotate = 0; mixX = 0; mixY = 0; mixScaleX = 0; mixScaleY = 0; mixShearY = 0;

	temp = new Vector2();
	active = false;

	constructor (data: TransformConstraintData, skeleton: Skeleton) {
		if (!data) throw new Error("data cannot be null.");
		if (!skeleton) throw new Error("skeleton cannot be null.");
		this.data = data;

		this.bones = new Array<Bone>();
		for (let i = 0; i < data.bones.length; i++) {
			let bone = skeleton.findBone(data.bones[i].name);
			if (!bone) throw new Error(`Couldn't find bone ${data.bones[i].name}.`);
			this.bones.push(bone);
		}
		let target = skeleton.findBone(data.source.name);
		if (!target) throw new Error(`Couldn't find target bone ${data.source.name}.`);
		this.source = target;

		this.mixRotate = data.mixRotate;
		this.mixX = data.mixX;
		this.mixY = data.mixY;
		this.mixScaleX = data.mixScaleX;
		this.mixScaleY = data.mixScaleY;
		this.mixShearY = data.mixShearY;
	}

	isActive () {
		return this.active;
	}

	setToSetupPose () {
		const data = this.data;
		this.mixRotate = data.mixRotate;
		this.mixX = data.mixX;
		this.mixY = data.mixY;
		this.mixScaleX = data.mixScaleX;
		this.mixScaleY = data.mixScaleY;
		this.mixShearY = data.mixShearY;
	}

	update (physics: Physics) {
		if (this.mixRotate == 0 && this.mixX == 0 && this.mixY == 0 && this.mixScaleX == 0 && this.mixScaleY == 0 && this.mixShearY == 0) return;

		const data = this.data;
		const localFrom = data.localSource, localTarget = data.localTarget, additive = data.additive, clamp = data.clamp;
		const source = this.source;
		const fromItems = data.properties;
		const fn = data.properties.length;
		const bones = this.bones;
		for (let i = 0, n = this.bones.length; i < n; i++) {
			const bone = bones[i];
			for (let f = 0; f < fn; f++) {
				const from = fromItems[f];
				const value = from.value(data, source, localFrom) - from.offset;
				const toItems = from.to;
				for (let t = 0, tn = from.to.length; t < tn; t++) {
					var to = toItems[t];
					if (to.mix(this) != 0) {
						let clamped = to.offset + value * to.scale;
						if (clamp) {
							if (to.offset < to.max)
								clamped = MathUtils.clamp(clamped, to.offset, to.max);
							else
								clamped = MathUtils.clamp(clamped, to.max, to.offset);
						}
						to.apply(this, bone, clamped, localTarget, additive);
					}
				}
			}
			if (localTarget)
				bone.update(null);
			else
				bone.updateAppliedTransform();
		}
	}
}
