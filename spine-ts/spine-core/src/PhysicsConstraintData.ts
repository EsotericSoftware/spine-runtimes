/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

import { BoneData } from "./BoneData.js";
import { ConstraintData } from "./ConstraintData.js";


/** Stores the setup pose for a {@link PhysicsConstraint}.
 * <p>
 * See <a href="http://esotericsoftware.com/spine-physics-constraints">Physics constraints</a> in the Spine User Guide. */
export class PhysicsConstraintData extends ConstraintData {
	private _bone: BoneData | null = null;
	/** The bone constrained by this physics constraint. */
	public set bone (boneData: BoneData) { this._bone = boneData; }
	public get bone () {
		if (!this._bone) throw new Error("BoneData not set.")
		else return this._bone;
	}

	x = 0;
	y = 0;
	rotate = 0;
	scaleX = 0;
	shearX = 0;
	limit = 0;
	step = 0;
	inertia = 0;
	strength = 0;
	damping = 0;
	massInverse = 0;
	wind = 0;
	gravity = 0;
	/** A percentage (0-1) that controls the mix between the constrained and unconstrained poses. */
	mix = 0;
	inertiaGlobal = false;
	strengthGlobal = false;
	dampingGlobal = false;
	massGlobal = false;
	windGlobal = false;
	gravityGlobal = false;
	mixGlobal = false;

	constructor (name: string) {
		super(name, 0, false);
	}
}
