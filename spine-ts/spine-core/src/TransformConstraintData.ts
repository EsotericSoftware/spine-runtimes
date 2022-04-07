/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

import { ConstraintData } from "./ConstraintData";
import { BoneData } from "./BoneData";

/** Stores the setup pose for a {@link TransformConstraint}.
 *
 * See [Transform constraints](http://esotericsoftware.com/spine-transform-constraints) in the Spine User Guide. */
export class TransformConstraintData extends ConstraintData {

	/** The bones that will be modified by this transform constraint. */
	bones = new Array<BoneData>();

	/** The target bone whose world transform will be copied to the constrained bones. */
	private _target: BoneData | null = null;
	public set target (boneData: BoneData) { this._target = boneData; }
	public get target () {
		if (!this._target) throw new Error("BoneData not set.")
		else return this._target;
	}

	mixRotate = 0;
	mixX = 0;
	mixY = 0;
	mixScaleX = 0;
	mixScaleY = 0;
	mixShearY = 0;

	/** An offset added to the constrained bone rotation. */
	offsetRotation = 0;

	/** An offset added to the constrained bone X translation. */
	offsetX = 0;

	/** An offset added to the constrained bone Y translation. */
	offsetY = 0;

	/** An offset added to the constrained bone scaleX. */
	offsetScaleX = 0;

	/** An offset added to the constrained bone scaleY. */
	offsetScaleY = 0;

	/** An offset added to the constrained bone shearY. */
	offsetShearY = 0;

	relative = false;
	local = false;

	constructor (name: string) {
		super(name, 0, false);
	}
}
