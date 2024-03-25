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

package com.esotericsoftware.spine;

/** Stores the setup pose for a {@link PhysicsConstraint}.
 * <p>
 * See <a href="http://esotericsoftware.com/spine-physics-constraints">Physics constraints</a> in the Spine User Guide. */
public class PhysicsConstraintData extends ConstraintData {
	BoneData bone;
	float x, y, rotate, scaleX, shearX, limit;
	float step, inertia, strength, damping, massInverse, wind, gravity, mix;
	boolean inertiaGlobal, strengthGlobal, dampingGlobal, massGlobal, windGlobal, gravityGlobal, mixGlobal;

	public PhysicsConstraintData (String name) {
		super(name);
	}

	/** The bone constrained by this physics constraint. */
	public BoneData getBone () {
		return bone;
	}

	public void setBone (BoneData bone) {
		this.bone = bone;
	}

	public float getStep () {
		return step;
	}

	public void setStep (float step) {
		this.step = step;
	}

	public float getX () {
		return x;
	}

	public void setX (float x) {
		this.x = x;
	}

	public float getY () {
		return y;
	}

	public void setY (float y) {
		this.y = y;
	}

	public float getRotate () {
		return rotate;
	}

	public void setRotate (float rotate) {
		this.rotate = rotate;
	}

	public float getScaleX () {
		return scaleX;
	}

	public void setScaleX (float scaleX) {
		this.scaleX = scaleX;
	}

	public float getShearX () {
		return shearX;
	}

	public void setShearX (float shearX) {
		this.shearX = shearX;
	}

	public float getLimit () {
		return limit;
	}

	public void setLimit (float limit) {
		this.limit = limit;
	}

	public float getInertia () {
		return inertia;
	}

	public void setInertia (float inertia) {
		this.inertia = inertia;
	}

	public float getStrength () {
		return strength;
	}

	public void setStrength (float strength) {
		this.strength = strength;
	}

	public float getDamping () {
		return damping;
	}

	public void setDamping (float damping) {
		this.damping = damping;
	}

	public float getMassInverse () {
		return massInverse;
	}

	public void setMassInverse (float massInverse) {
		this.massInverse = massInverse;
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

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained poses. */
	public float getMix () {
		return mix;
	}

	public void setMix (float mix) {
		this.mix = mix;
	}

	public boolean getInertiaGlobal () {
		return inertiaGlobal;
	}

	public void setInertiaGlobal (boolean inertiaGlobal) {
		this.inertiaGlobal = inertiaGlobal;
	}

	public boolean getStrengthGlobal () {
		return strengthGlobal;
	}

	public void setStrengthGlobal (boolean strengthGlobal) {
		this.strengthGlobal = strengthGlobal;
	}

	public boolean getDampingGlobal () {
		return dampingGlobal;
	}

	public void setDampingGlobal (boolean dampingGlobal) {
		this.dampingGlobal = dampingGlobal;
	}

	public boolean getMassGlobal () {
		return massGlobal;
	}

	public void setMassGlobal (boolean massGlobal) {
		this.massGlobal = massGlobal;
	}

	public boolean getWindGlobal () {
		return windGlobal;
	}

	public void setWindGlobal (boolean windGlobal) {
		this.windGlobal = windGlobal;
	}

	public boolean getGravityGlobal () {
		return gravityGlobal;
	}

	public void setGravityGlobal (boolean gravityGlobal) {
		this.gravityGlobal = gravityGlobal;
	}

	public boolean getMixGlobal () {
		return mixGlobal;
	}

	public void setMixGlobal (boolean mixGlobal) {
		this.mixGlobal = mixGlobal;
	}
}
