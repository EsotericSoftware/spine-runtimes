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

package com.esotericsoftware.spine;

import com.badlogic.gdx.utils.Array;

/** Stores the current pose for a spring constraint. A spring constraint applies physics to bones.
 * <p>
 * See <a href="http://esotericsoftware.com/spine-spring-constraints">Spring constraints</a> in the Spine User Guide. */
public class SpringConstraint implements Updatable {
	final SpringConstraintData data;
	final Array<Bone> bones;
	// BOZO! - stiffness -> strength. stiffness, damping, rope, stretch -> move to spring.
	float mix, friction, gravity, wind, stiffness, damping;
	boolean rope, stretch;

	boolean active;

	public SpringConstraint (SpringConstraintData data, Skeleton skeleton) {
		if (data == null) throw new IllegalArgumentException("data cannot be null.");
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");
		this.data = data;
		mix = data.mix;
		friction = data.friction;
		gravity = data.gravity;
		wind = data.wind;
		stiffness = data.stiffness;
		damping = data.damping;
		rope = data.rope;
		stretch = data.stretch;

		bones = new Array(data.bones.size);
		for (BoneData boneData : data.bones)
			bones.add(skeleton.bones.get(boneData.index));
	}

	/** Copy constructor. */
	public SpringConstraint (SpringConstraint constraint, Skeleton skeleton) {
		if (constraint == null) throw new IllegalArgumentException("constraint cannot be null.");
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");
		data = constraint.data;
		bones = new Array(constraint.bones.size);
		for (Bone bone : constraint.bones)
			bones.add(skeleton.bones.get(bone.data.index));
		mix = constraint.mix;
		friction = constraint.friction;
		gravity = constraint.gravity;
		wind = constraint.wind;
		stiffness = constraint.stiffness;
		damping = constraint.damping;
		rope = constraint.rope;
		stretch = constraint.stretch;
	}

	/** Applies the constraint to the constrained bones. */
	public void update () {

	}

	/** The bones that will be modified by this spring constraint. */
	public Array<Bone> getBones () {
		return bones;
	}

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained poses. */
	public float getMix () {
		return mix;
	}

	public void setMix (float mix) {
		this.mix = mix;
	}

	public float getFriction () {
		return friction;
	}

	public void setFriction (float friction) {
		this.friction = friction;
	}

	public float getGravity () {
		return gravity;
	}

	public void setGravity (float gravity) {
		this.gravity = gravity;
	}

	public float getWind () {
		return wind;
	}

	public void setWind (float wind) {
		this.wind = wind;
	}

	public float getStiffness () {
		return stiffness;
	}

	public void setStiffness (float stiffness) {
		this.stiffness = stiffness;
	}

	public float getDamping () {
		return damping;
	}

	public void setDamping (float damping) {
		this.damping = damping;
	}

	public boolean getRope () {
		return rope;
	}

	public void setRope (boolean rope) {
		this.rope = rope;
	}

	public boolean getStretch () {
		return stretch;
	}

	public void setStretch (boolean stretch) {
		this.stretch = stretch;
	}

	public boolean isActive () {
		return active;
	}

	/** The spring constraint's setup pose data. */
	public SpringConstraintData getData () {
		return data;
	}

	public String toString () {
		return data.name;
	}
}
