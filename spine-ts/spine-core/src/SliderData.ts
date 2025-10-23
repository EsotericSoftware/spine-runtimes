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

import type { Animation } from "./Animation.js";
import type { BoneData } from "./BoneData.js";
import { ConstraintData } from "./ConstraintData.js";
import type { Skeleton } from "./Skeleton.js";
import { Slider } from "./Slider.js";
import { SliderPose } from "./SliderPose.js";
import type { FromProperty } from "./TransformConstraintData.js";

/** Stores the setup pose for a {@link SliderConstraint}.
 *
 * See <a href="https://esotericsoftware.com/spine-slider-constraints">Slider constraints</a> in the Spine User Guide. */
export class SliderData extends ConstraintData<Slider, SliderPose> {
	animation!: Animation;
	additive = false;
	loop = false;
	bone: BoneData | null = null;
	property!: FromProperty;
	scale = 0;
	offset = 0;
	local = false;

	constructor (name: string) {
		super(name, new SliderPose());
	}

	public create (skeleton: Skeleton) {
		return new Slider(this, skeleton);
	}
}
