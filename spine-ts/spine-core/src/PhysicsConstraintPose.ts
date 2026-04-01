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

import type { Pose } from "./Pose"
import type { Skeleton } from "./Skeleton.js";

/** Stores a pose for a physics constraint. */
export class PhysicsConstraintPose implements Pose<PhysicsConstraintPose> {

	/** Controls how much bone movement is converted into physics movement. */
	inertia = 0;

	/** The amount of force used to return properties to the unconstrained value. */
	strength = 0;

	/** Reduces the speed of physics movements, with more of a reduction at higher speeds. */
	damping = 0;

	/** Determines susceptibility to acceleration. */
	massInverse = 0;

	/** Applies a constant force along the {@link Skeleton.windX}, {@link Skeleton.windY} vector. */
	wind = 0;

	/** Applies a constant force along the {@link Skeleton.gravityX}, {@link Skeleton.gravityY} vector. */
	gravity = 0;

	/** A percentage (0+) that controls the mix between the constrained and unconstrained poses. */
	mix = 0;

	public set (pose: PhysicsConstraintPose) {
		this.inertia = pose.inertia;
		this.strength = pose.strength;
		this.damping = pose.damping;
		this.massInverse = pose.massInverse;
		this.wind = pose.wind;
		this.gravity = pose.gravity;
		this.mix = pose.mix;
	}


}
