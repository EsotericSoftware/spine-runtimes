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

/** Stores the setup pose for a {@link PhysicsConstraint}.
 * <p>
 * See <a href="https://esotericsoftware.com/spine-physics-constraints">Physics constraints</a> in the Spine User Guide. */
public class PhysicsConstraintData extends ConstraintData<PhysicsConstraint, PhysicsConstraintPose> {
	BoneData bone;
	float x, y, rotate, scaleX, shearX, limit, step;
	boolean inertiaGlobal, strengthGlobal, dampingGlobal, massGlobal, windGlobal, gravityGlobal, mixGlobal;

	public PhysicsConstraintData (String name) {
		super(name, new PhysicsConstraintPose());
	}

	public PhysicsConstraint create (Skeleton skeleton) {
		return new PhysicsConstraint(this, skeleton);
	}

	/** The bone constrained by this physics constraint. */
	public BoneData getBone () {
		return bone;
	}

	public void setBone (BoneData bone) {
		this.bone = bone;
	}

	/** The time in milliseconds required to advanced the physics simulation one step. */
	public float getStep () {
		return step;
	}

	public void setStep (float step) {
		this.step = step;
	}

	/** Physics influence on x translation, 0-1. */
	public float getX () {
		return x;
	}

	public void setX (float x) {
		this.x = x;
	}

	/** Physics influence on y translation, 0-1. */
	public float getY () {
		return y;
	}

	public void setY (float y) {
		this.y = y;
	}

	/** Physics influence on rotation, 0-1. */
	public float getRotate () {
		return rotate;
	}

	public void setRotate (float rotate) {
		this.rotate = rotate;
	}

	/** Physics influence on scaleX, 0-1. */
	public float getScaleX () {
		return scaleX;
	}

	public void setScaleX (float scaleX) {
		this.scaleX = scaleX;
	}

	/** Physics influence on shearX, 0-1. */
	public float getShearX () {
		return shearX;
	}

	public void setShearX (float shearX) {
		this.shearX = shearX;
	}

	/** Movement greater than the limit will not have a greater affect on physics. */
	public float getLimit () {
		return limit;
	}

	public void setLimit (float limit) {
		this.limit = limit;
	}

	/** True when this constraint's inertia is controlled by global slider timelines. */
	public boolean getInertiaGlobal () {
		return inertiaGlobal;
	}

	public void setInertiaGlobal (boolean inertiaGlobal) {
		this.inertiaGlobal = inertiaGlobal;
	}

	/** True when this constraint's strength is controlled by global slider timelines. */
	public boolean getStrengthGlobal () {
		return strengthGlobal;
	}

	public void setStrengthGlobal (boolean strengthGlobal) {
		this.strengthGlobal = strengthGlobal;
	}

	/** True when this constraint's damping is controlled by global slider timelines. */
	public boolean getDampingGlobal () {
		return dampingGlobal;
	}

	public void setDampingGlobal (boolean dampingGlobal) {
		this.dampingGlobal = dampingGlobal;
	}

	/** True when this constraint's mass is controlled by global slider timelines. */
	public boolean getMassGlobal () {
		return massGlobal;
	}

	public void setMassGlobal (boolean massGlobal) {
		this.massGlobal = massGlobal;
	}

	/** True when this constraint's wind is controlled by global slider timelines. */
	public boolean getWindGlobal () {
		return windGlobal;
	}

	public void setWindGlobal (boolean windGlobal) {
		this.windGlobal = windGlobal;
	}

	/** True when this constraint's gravity is controlled by global slider timelines. */
	public boolean getGravityGlobal () {
		return gravityGlobal;
	}

	public void setGravityGlobal (boolean gravityGlobal) {
		this.gravityGlobal = gravityGlobal;
	}

	/** True when this constraint's mix is controlled by global slider timelines. */
	public boolean getMixGlobal () {
		return mixGlobal;
	}

	public void setMixGlobal (boolean mixGlobal) {
		this.mixGlobal = mixGlobal;
	}
}
