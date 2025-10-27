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

import { BoneLocal } from "./BoneLocal.js";
import { PosedData } from "./PosedData.js";
import type { Skeleton } from "./Skeleton.js";
import { Color } from "./Utils.js";

/** The setup pose for a bone. */
export class BoneData extends PosedData<BoneLocal> {
	/** The index of the bone in {@link Skeleton.getBones}. */
	index: number = 0;

	/** @returns May be null. */
	parent: BoneData | null = null;

	/** The bone's length. */
	length: number = 0;

	// Nonessential.
	/** The color of the bone as it was in Spine. Available only when nonessential data was exported. Bones are not usually
	 * rendered at runtime. */
	readonly color = new Color();

	/** The bone icon as it was in Spine, or null if nonessential data was not exported. */
	icon?: string;

	/** False if the bone was hidden in Spine and nonessential data was exported. Does not affect runtime rendering. */
	visible = false;

	constructor (index: number, name: string, parent: BoneData | null) {
		super(name, new BoneLocal());
		if (index < 0) throw new Error("index must be >= 0.");
		if (!name) throw new Error("name cannot be null.");
		this.index = index;
		this.parent = parent;
	}

	copy (parent: BoneData | null): BoneData {
		const copy = new BoneData(this.index, this.name, parent);
		copy.length = this.length;
		copy.setup.set(this.setup);
		return copy;
	}
}

/** Determines how a bone inherits world transforms from parent bones. */
export enum Inherit { Normal, OnlyTranslation, NoRotationOrReflection, NoScale, NoScaleOrReflection }
