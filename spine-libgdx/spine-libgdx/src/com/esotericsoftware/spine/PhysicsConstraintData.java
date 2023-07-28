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

/** Stores the setup pose for a {@link PhysicsConstraint}.
 * <p>
 * See <a href="http://esotericsoftware.com/spine-physics-constraints">Physics constraints</a> in the Spine User Guide. */
public class PhysicsConstraintData extends ConstraintData {
	final Array<BoneData> bones = new Array();
	float speed = 1, mass = 1;
	float strength, friction, damping, inertia, wind, gravity, mix;
	boolean translate, rotate, scale, shear;

	public PhysicsConstraintData (String name) {
		super(name);
	}

	/** The bones that are constrained by this physics constraint. */
	public Array<BoneData> getBones () {
		return bones;
	}

	public float getSpeed () {
		return speed;
	}

	public void setSpeed (float speed) {
		this.speed = speed;
	}

	public float getMass () {
		return mass;
	}

	public void setMass (float mass) {
		this.mass = mass;
	}

	public float getStrength () {
		return strength;
	}

	public void setStrength (float strength) {
		this.strength = strength;
	}

	public float getFriction () {
		return friction;
	}

	public void setFriction (float friction) {
		this.friction = friction;
	}

	public float getDamping () {
		return damping;
	}

	public void setDamping (float damping) {
		this.damping = damping;
	}

	public float getInertia () {
		return inertia;
	}

	public void setInertia (float inertia) {
		this.inertia = inertia;
	}

	public float getWind () {
		return wind;
	}

	public void setWind (float wind) {
		this.wind = wind;
	}

	public float getGravity () {
		return gravity;
	}

	public void setGravity (float gravity) {
		this.gravity = gravity;
	}

	public boolean getTranslate () {
		return translate;
	}

	public void setTranslate (boolean translate) {
		this.translate = translate;
	}

	public boolean getRotate () {
		return rotate;
	}

	public void setRotate (boolean rotate) {
		this.rotate = rotate;
	}

	public boolean getScale () {
		return scale;
	}

	public void setScale (boolean scale) {
		this.scale = scale;
	}

	public boolean getShear () {
		return shear;
	}

	public void setShear (boolean shear) {
		this.shear = shear;
	}

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained poses. */
	public float getMix () {
		return mix;
	}

	public void setMix (float mix) {
		this.mix = mix;
	}
}
