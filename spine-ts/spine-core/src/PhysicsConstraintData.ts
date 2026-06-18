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
import type { BonePose } from "./BonePose.js";
import { ConstraintData, ScaleYMode } from "./ConstraintData.js";
import { PhysicsConstraint } from "./PhysicsConstraint.js";
import { PhysicsConstraintPose } from "./PhysicsConstraintPose.js";
import type { Skeleton } from "./Skeleton.js";


/** Stores the setup pose for a {@link PhysicsConstraint}.
 *
 * See <a href="http://esotericsoftware.com/spine-physics-constraints">Physics constraints</a> in the Spine User Guide. */
export class PhysicsConstraintData extends ConstraintData<PhysicsConstraint, PhysicsConstraintPose> {
	/** The bone constrained by this physics constraint. */
	public set bone (boneData: BoneData) { this._bone = boneData; }
	public get bone () {
		if (!this._bone) throw new Error("BoneData not set.")
		else return this._bone;
	}
	private _bone: BoneData | null = null;

	/** Physics influence on x translation, 0-1. */
	x = 0;

	/** Physics influence on y translation, 0-1. */
	y = 0;

	/** Physics influence on rotation, 0-1. */
	rotate = 0;

	/** Physics influence on scaleX, 0-1. */
	scaleX = 0;

	/** Physics influence on shearX, 0-1. */
	shearX = 0;

	/** Movement greater than the limit will not have a greater affect on physics. */
	limit = 0;

	/** The time in milliseconds required to advanced the physics simulation one step. */
	step = 0;

	/** True when this constraint's inertia is controlled by global slider timelines. */
	inertiaGlobal = false;

	/** True when this constraint's strength is controlled by global slider timelines. */
	strengthGlobal = false;

	/** True when this constraint's damping is controlled by global slider timelines. */
	dampingGlobal = false;

	/** True when this constraint's mass is controlled by global slider timelines. */
	massGlobal = false;

	/** True when this constraint's wind is controlled by global slider timelines. */
	windGlobal = false;

	/** True when this constraint's gravity is controlled by global slider timelines. */
	gravityGlobal = false;

	/** True when this constraint's mix is controlled by global slider timelines. */
	mixGlobal = false;

	/** Determines how the {@link BonePose.scaleY} changes when {@link BonePose.scaleX} sets
	 * {@link BonePose.scaleX}. */
	private _scaleYMode = ScaleYMode.None;
	get scaleYMode (): ScaleYMode {
		return this._scaleYMode;
	}
	set scaleYMode (scaleYMode: ScaleYMode) {
		if (scaleYMode == null) throw new Error("scaleYMode cannot be null.");
		this._scaleYMode = scaleYMode;
	}

	constructor (name: string) {
		super(name, new PhysicsConstraintPose());
	}

	public create (skeleton: Skeleton) {
		return new PhysicsConstraint(this, skeleton);
	}
}
