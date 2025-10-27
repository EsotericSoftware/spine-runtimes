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
import { PosedData } from "./PosedData.js";
import type { Skeleton } from "./Skeleton.js";
import { SlotPose } from "./SlotPose.js";

/** Stores the setup pose for a {@link Slot}. */
export class SlotData extends PosedData<SlotPose> {
	/** The index of the slot in {@link Skeleton.getSlots()}. */
	index: number = 0;

	/** The bone this slot belongs to. */
	boneData: BoneData;

	/** The name of the attachment that is visible for this slot in the setup pose, or null if no attachment is visible. */
	attachmentName: string | null = null;

	/** The blend mode for drawing the slot's attachment. */
	blendMode: BlendMode = BlendMode.Normal;

	// Nonessential.
	/** False if the slot was hidden in Spine and nonessential data was exported. Does not affect runtime rendering. */
	visible = true;

	constructor (index: number, name: string, boneData: BoneData) {
		super(name, new SlotPose());
		if (index < 0) throw new Error("index must be >= 0.");
		if (!boneData) throw new Error("boneData cannot be null.");
		this.index = index;
		this.boneData = boneData;
	}
}

/** Determines how images are blended with existing pixels when drawn. */
export enum BlendMode { Normal, Additive, Multiply, Screen }
