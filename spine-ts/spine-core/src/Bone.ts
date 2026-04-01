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

import type { BoneData } from "./BoneData.js";
import { BonePose } from "./BonePose.js";
import { PosedActive } from "./PosedActive.js";
import type { Skeleton } from "./Skeleton.js";

/** A node in a skeleton's hierarchy with a transform that affects its children and their attachments. A bone has a number of
 * poses:
 * - {@link data}: The setup pose.
 * - {@link pose}: The unconstrained local pose. Set by animations and application code.
 * - {@link appliedPose}: The local pose to use for rendering. Possibly modified by constraints.
 * - World transform: the local pose combined with the parent world transform. Computed on a pose by
 * {@link BonePose.updateWorldTransform} and {@link Skeleton.updateWorldTransform}.
 */
export class Bone extends PosedActive<BoneData, BonePose> {
	/** The parent bone, or null if this is the root bone. */
	parent: Bone | null = null;

	/** The immediate children of this bone. */
	children = [] as Bone[];

	sorted = false;

	constructor (data: BoneData, parent: Bone | null) {
		super(data, new BonePose(), new BonePose());
		this.parent = parent;
		this.appliedPose.bone = this;
		this.constrainedPose.bone = this;
	}

	/** Copy constructor. Does not copy the {@link children} bones. */
	copy (parent: Bone | null): Bone {
		const copy = new Bone(this.data, parent);
		copy.pose.set(this.pose);
		return copy;
	}

}
