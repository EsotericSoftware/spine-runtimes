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

import type { Slot } from "./Slot";
import { Utils } from "./Utils";

/** Stores the skeleton's draw order, which is the order that each slot's attachment is rendered. */
export class DrawOrder {
	readonly _setupPose: Slot[];

	/** The unconstrained draw order, set by animations and application code. */
	readonly pose: Slot[];
	readonly constrainedPose: Slot[];

	/** The constrained draw order for rendering. If no constraints modify the draw order, this is the same as {@link pose}.
	 * Otherwise it is a copy of {@link pose} modified by constraints. */
	appliedPose: Slot[];

	constructor (setupPose: Slot[]) {
		this._setupPose = setupPose;
		this.pose = [...setupPose];
		this.constrainedPose = [];
		this.appliedPose = this.pose;
	}

	/** Sets the unconstrained draw order to the setup pose order. */
	setupPose () {
		this.pose.length = this._setupPose.length;
		Utils.arrayCopy(this._setupPose, 0, this.pose, 0, this._setupPose.length);
	}

	/** Sets the applied pose to the unconstrained pose, for when no constraints will modify the draw order. */
	unconstrained () {
		this.appliedPose = this.pose;
	}

	/** Sets the applied pose to the constrained pose, in anticipation of the applied pose being modified by constraints. */
	constrained () {
		this.appliedPose = this.constrainedPose;
	}

	/** Copies the unconstrained pose to the constrained pose, as a starting point for constraints to be applied. */
	resetConstrained () {
		this.constrainedPose.length = this.pose.length;
		Utils.arrayCopy(this.pose, 0, this.constrainedPose, 0, this.pose.length);
	}
}
