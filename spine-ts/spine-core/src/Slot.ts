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
import { Posed } from "./Posed.js";
import type { Skeleton } from "./Skeleton.js";
import type { SlotData } from "./SlotData.js";
import { SlotPose } from "./SlotPose.js";
import { Color } from "./Utils.js";

/** Stores a slot's current pose. Slots organize attachments for {@link Skeleton#drawOrder} purposes and provide a place to store
 * state for an attachment. State cannot be stored in an attachment itself because attachments are stateless and may be shared
 * across multiple skeletons. */
export class Slot extends Posed<SlotData, SlotPose, SlotPose> {
	readonly skeleton: Skeleton;

	/** The bone this slot belongs to. */
	readonly bone: Bone;

	attachmentState: number = 0;

	constructor (data: SlotData, skeleton: Skeleton) {
		super(data, new SlotPose(), new SlotPose());
		if (!skeleton) throw new Error("skeleton cannot be null.");
		this.skeleton = skeleton;
		this.bone = skeleton.bones[data.boneData.index];
		if (data.setup.darkColor != null) {
			this.pose.darkColor = new Color();
			this.constrained.darkColor = new Color();
		}
		this.setupPose();
	}

	setupPose () {
		this.pose.color.setFromColor(this.data.setup.color);
		// biome-ignore lint/style/noNonNullAssertion: reference runtime
		if (this.pose.darkColor) this.pose.darkColor.setFromColor(this.data.setup.darkColor!);
		this.pose.sequenceIndex = this.data.setup.sequenceIndex;
		if (!this.data.attachmentName)
			this.pose.setAttachment(null);
		else {
			this.pose.attachment = null;
			this.pose.setAttachment(this.skeleton.getAttachment(this.data.index, this.data.attachmentName));
		}
	}
}
