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

import type { Pose } from "./Pose.js";
import type { PosedData } from "./PosedData.js";

export abstract class Posed<
	D extends PosedData<P>,
	P extends Pose<P>,
	A extends P> {

	/** The constraint's setup pose data. */
	readonly data: D;
	readonly pose: A;
	readonly constrained: A;
	applied: A;

	constructor (data: D, pose: A, constrained: A) {
		if (data == null) throw new Error("data cannot be null.");
		this.data = data;
		this.pose = pose;
		this.constrained = constrained;
		this.applied = pose;
	}

	public setupPose (): void {
		this.pose.set(this.data.setup);
	}

	/** The constraint's setup pose data. */
	public getData (): D {
		return this.data;
	}

	public getPose (): P {
		return this.pose;
	}

	public getAppliedPose (): A {
		return this.applied;
	}

	usePose () { // Port: usePose - reference runtime:  pose()
		this.applied = this.pose;
	}

	useConstrained () { // Port: useConstrained - reference runtime:  constrained()
		this.applied = this.constrained;
	}

	resetConstrained () { // Port: resetConstrained - reference runtime:  reset()
		this.constrained.set(this.pose);
	}

}
