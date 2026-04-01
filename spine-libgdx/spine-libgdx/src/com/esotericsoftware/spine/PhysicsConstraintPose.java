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

package com.esotericsoftware.spine;

/** Stores a pose for a physics constraint. */
public class PhysicsConstraintPose implements Pose<PhysicsConstraintPose> {
	float inertia, strength, damping, massInverse, wind, gravity, mix;

	public void set (PhysicsConstraintPose pose) {
		inertia = pose.inertia;
		strength = pose.strength;
		damping = pose.damping;
		massInverse = pose.massInverse;
		wind = pose.wind;
		gravity = pose.gravity;
		mix = pose.mix;
	}

	/** Controls how much bone movement is converted into physics movement. */
	public float getInertia () {
		return inertia;
	}

	public void setInertia (float inertia) {
		this.inertia = inertia;
	}

	/** The amount of force used to return properties to the unconstrained value. */
	public float getStrength () {
		return strength;
	}

	public void setStrength (float strength) {
		this.strength = strength;
	}

	/** Reduces the speed of physics movements, with more of a reduction at higher speeds. */
	public float getDamping () {
		return damping;
	}

	public void setDamping (float damping) {
		this.damping = damping;
	}

	/** Determines susceptibility to acceleration. */
	public float getMassInverse () {
		return massInverse;
	}

	public void setMassInverse (float massInverse) {
		this.massInverse = massInverse;
	}

	/** Applies a constant force along the {@link Skeleton#windX}, {@link Skeleton#windY} vector. */
	public float getWind () {
		return wind;
	}

	public void setWind (float wind) {
		this.wind = wind;
	}

	/** Applies a constant force along the {@link Skeleton#gravityX}, {@link Skeleton#gravityY} vector. */
	public float getGravity () {
		return gravity;
	}

	public void setGravity (float gravity) {
		this.gravity = gravity;
	}

	/** A percentage (0+) that controls the mix between the constrained and unconstrained poses. */
	public float getMix () {
		return mix;
	}

	public void setMix (float mix) {
		this.mix = mix;
	}
}
