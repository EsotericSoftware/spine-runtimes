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

import static com.esotericsoftware.spine.utils.SpineUtils.*;

import com.esotericsoftware.spine.Skeleton.Physics;

/** Stores the current pose for a physics constraint. A physics constraint applies physics to bones.
 * <p>
 * See <a href="http://esotericsoftware.com/spine-physics-constraints">Physics constraints</a> in the Spine User Guide. */
public class PhysicsConstraint implements Updatable {
	final PhysicsConstraintData data;
	public Bone bone;

	float mix;

	boolean reset = true;
	float beforeX, beforeY, afterX, afterY, tipX, tipY;
	float xOffset, xVelocity;
	float yOffset, yVelocity;
	float rotateOffset, rotateVelocity;
	float scaleOffset, scaleVelocity;

	boolean active;

	final Skeleton skeleton;
	float remaining, lastTime;

	public PhysicsConstraint (PhysicsConstraintData data, Skeleton skeleton) {
		if (data == null) throw new IllegalArgumentException("data cannot be null.");
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");
		this.data = data;
		this.skeleton = skeleton;

		bone = skeleton.bones.get(data.bone.index);
		mix = data.mix;
	}

	/** Copy constructor. */
	public PhysicsConstraint (PhysicsConstraint constraint) {
		if (constraint == null) throw new IllegalArgumentException("constraint cannot be null.");
		data = constraint.data;
		skeleton = constraint.skeleton;
		bone = constraint.bone;
		mix = constraint.mix;
	}

	public void reset () {
		remaining = 0;
		lastTime = skeleton.time;

		reset = true;
		xOffset = 0;
		xVelocity = 0;
		yOffset = 0;
		yVelocity = 0;
		rotateOffset = 0;
		rotateVelocity = 0;
		scaleOffset = 0;
		scaleVelocity = 0;
	}

	public void setToSetupPose () {
		reset();

		PhysicsConstraintData data = this.data;
		mix = data.mix;
	}

	/** Applies the constraint to the constrained bones. */
	public void update (Physics physics) {
		float mix = this.mix;
		if (mix == 0) return;

		boolean x = data.x, y = data.y, rotateOrShear = data.rotate || data.shearX, scaleX = data.scaleX;
		Bone bone = this.bone;

		switch (physics) {
		case none:
			return;
		case reset:
			reset();
			break;
		case update:
			remaining += Math.max(skeleton.time - lastTime, 0);
			lastTime = skeleton.time;

			float length = bone.data.length, br = atan2(bone.c, bone.a);
			float wind = data.wind, gravity = data.gravity, strength = data.strength, friction = data.friction, mass = data.mass;
			float damping = data.damping, step = data.step;
			boolean angle = rotateOrShear || scaleX;
			float cos = 0, sin = 0;
			while (remaining >= step) {
				remaining -= step;
				if (x) {
					xVelocity += (wind - xOffset * strength - friction * xVelocity) * mass;
					xOffset += xVelocity * step;
					xVelocity *= damping;
				}
				if (y) {
					yVelocity += (-gravity - yOffset * strength - friction * yVelocity) * mass;
					yOffset += yVelocity * step;
					yVelocity *= damping;
				}
				if (angle) {
					float r = br + rotateOffset * degRad;
					cos = cos(r);
					sin = sin(r);
				}
				if (rotateOrShear) {
					rotateVelocity += (length * (-wind * sin - gravity * cos) - rotateOffset * strength - friction * rotateVelocity)
						* mass;
					rotateOffset += rotateVelocity * step;
					rotateVelocity *= damping;
				}
				if (scaleX) {
					scaleVelocity += (wind * cos - gravity * sin - scaleOffset * strength - friction * scaleVelocity) * mass;
					scaleOffset += scaleVelocity * step;
					scaleVelocity *= damping;
				}
			}

			float bx = bone.worldX, by = bone.worldY;
			if (reset)
				reset = false;
			else {
				float i = data.inertia;
				if (x) {
					xOffset += (beforeX - bx) * i;
					bone.worldX += xOffset * mix;
				}
				if (y) {
					yOffset += (beforeY - by) * i;
					bone.worldY += yOffset * mix;
				}
				if (rotateOrShear) {
					if (length == 0)
						rotateOffset = 0;
					else {
						float r = (atan2(afterY - bone.worldY + tipY, afterX - bone.worldX + tipX) - br) * radDeg;
						rotateOffset += (r - (16384 - (int)(16384.499999999996 - r / 360)) * 360) * i;
					}
				}
				if (scaleX) {
					if (length == 0)
						scaleOffset = 0;
					else {
						float r = br + rotateOffset * degRad;
						scaleOffset += ((afterX - bone.worldX) * cos(r) + (afterY - bone.worldY) * sin(r)) * i / length;
					}
				}
			}
			beforeX = bx;
			beforeY = by;
			afterX = bone.worldX;
			afterY = bone.worldY;
			tipX = length * bone.a;
			tipY = length * bone.c;
			break;
		case pose:
			if (x) bone.worldX += xOffset * mix;
			if (y) bone.worldY += yOffset * mix;
		}

		if (rotateOrShear) {
			float r = rotateOffset * mix;
			if (data.shearX) {
				if (data.rotate) {
					r *= 0.5f;
					bone.rotateWorld(r);
				}
				float t = (float)Math.tan(r * degRad);
				bone.b += bone.a * t;
				bone.d += bone.c * t;
			} else
				bone.rotateWorld(r);
		}
		if (scaleX) {
			float s = 1 + scaleOffset * mix;
			bone.a *= s;
			bone.c *= s;
		}
		bone.updateAppliedTransform();
	}

	/** The bone constrained by this physics constraint. */
	public Bone getBone () {
		return bone;
	}

	public void setBone (Bone bone) {
		this.bone = bone;
	}

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained poses. */
	public float getMix () {
		return mix;
	}

	public void setMix (float mix) {
		this.mix = mix;
	}

	public boolean isActive () {
		return active;
	}

	/** The physics constraint's setup pose data. */
	public PhysicsConstraintData getData () {
		return data;
	}

	public String toString () {
		return data.name;
	}
}
