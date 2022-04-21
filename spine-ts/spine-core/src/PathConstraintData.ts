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

import { BoneData } from "./BoneData";
import { ConstraintData } from "./ConstraintData";
import { SlotData } from "./SlotData";


/** Stores the setup pose for a {@link PathConstraint}.
 *
 * See [path constraints](http://esotericsoftware.com/spine-path-constraints) in the Spine User Guide. */
export class PathConstraintData extends ConstraintData {

	/** The bones that will be modified by this path constraint. */
	bones = new Array<BoneData>();

	/** The slot whose path attachment will be used to constrained the bones. */
	private _target: SlotData | null = null;
	public set target (slotData: SlotData) { this._target = slotData; }
	public get target () {
		if (!this._target) throw new Error("SlotData not set.")
		else return this._target;
	}

	/** The mode for positioning the first bone on the path. */
	positionMode: PositionMode = PositionMode.Fixed;

	/** The mode for positioning the bones after the first bone on the path. */
	spacingMode: SpacingMode = SpacingMode.Fixed;

	/** The mode for adjusting the rotation of the bones. */
	rotateMode: RotateMode = RotateMode.Chain;

	/** An offset added to the constrained bone rotation. */
	offsetRotation: number = 0;

	/** The position along the path. */
	position: number = 0;

	/** The spacing between bones. */
	spacing: number = 0;

	mixRotate = 0;
	mixX = 0;
	mixY = 0;

	constructor (name: string) {
		super(name, 0, false);
	}
}

/** Controls how the first bone is positioned along the path.
 *
 * See [position](http://esotericsoftware.com/spine-path-constraints#Position) in the Spine User Guide. */
export enum PositionMode { Fixed, Percent }

/** Controls how bones after the first bone are positioned along the path.
 *
 * See [spacing](http://esotericsoftware.com/spine-path-constraints#Spacing) in the Spine User Guide. */
export enum SpacingMode { Length, Fixed, Percent, Proportional }

/** Controls how bones are rotated, translated, and scaled to match the path.
 *
 * See [rotate mix](http://esotericsoftware.com/spine-path-constraints#Rotate-mix) in the Spine User Guide. */
export enum RotateMode { Tangent, Chain, ChainScale }
