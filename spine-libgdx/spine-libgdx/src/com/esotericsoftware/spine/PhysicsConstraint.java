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

import static com.badlogic.gdx.math.Interpolation.*;
import static com.esotericsoftware.spine.utils.SpineUtils.*;

import com.badlogic.gdx.math.Vector2;
import com.badlogic.gdx.utils.Array;

import com.esotericsoftware.spine.Skeleton.Physics;

// BOZO - Physics steps/something in metrics view.

/** Stores the current pose for a physics constraint. A physics constraint applies physics to bones.
 * <p>
 * See <a href="http://esotericsoftware.com/spine-physics-constraints">Physics constraints</a> in the Spine User Guide. */
public class PhysicsConstraint implements Updatable {
	final PhysicsConstraintData data;
	final Array<Bone> bones;
	State[] states = {};
	float mix;

	boolean active;

	final Skeleton skeleton;
	float remaining, lastTime;

	public PhysicsConstraint (PhysicsConstraintData data, Skeleton skeleton) {
		if (data == null) throw new IllegalArgumentException("data cannot be null.");
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");
		this.data = data;
		this.skeleton = skeleton;

		bones = new Array(data.bones.size);
		for (BoneData boneData : data.bones)
			bones.add(skeleton.bones.get(boneData.index));

		mix = data.mix;
	}

	/** Copy constructor. */
	public PhysicsConstraint (PhysicsConstraint constraint) {
		if (constraint == null) throw new IllegalArgumentException("constraint cannot be null.");
		data = constraint.data;
		skeleton = constraint.skeleton;
		bones = new Array(constraint.bones);
		mix = constraint.mix;
	}

	/** Caches information about bones. Must be called if {@link PathConstraintData#getBones()} is modified. */
	public void updateBones () {
		int count = 0;
		if (data.x) count = 1;
		if (data.y) count++;
		if (data.rotate) count++;
		if (data.scaleX) count++;
		if (data.shearX) count++;
		count *= bones.size * 2;
		if (states.length != count) {
			states = new State[count];
			for (int i = 0; i < count; i++)
				states[i] = new State();
		}
	}

	public void reset () {
		remaining = 0;
		lastTime = skeleton.time;

		for (int i = 0, n = states.length; i < n; i++) {
			State state = states[i];
			state.last = false;
			state.offset = 0;
			state.velocity = 0;
		}
	}

	public void setToSetupPose () {
		reset();

		PhysicsConstraintData data = this.data;
		mix = data.mix;
	}

	/** Applies the constraint to the constrained bones. */
	public void update (Physics physics) {
		if (mix == 0) return;

		data.rotate = true; // BOZO - Remove.
		updateBones(); // BOZO - Remove.

		Object[] bones = this.bones.items;
		int boneCount = this.bones.size;

		switch (physics) {
		case none:
			return;
		case reset:
			reset();
			// Fall through.
		case update:
			for (int i = 0; i < boneCount; i++) {
				Bone bone = (Bone)bones[i];
				if (data.rotate) {
					Vector2 tip = bone.localToWorld(new Vector2(bone.data.length, 0));
					State state = states[i];
					if (!state.last)
						state.last = true;
					else if (state.x != bone.worldX || state.y != bone.worldY) {
						float angleToOldTip = new Vector2(state.tipx, state.tipy).sub(bone.worldX, bone.worldY).angleDeg()
							+ state.offset - bone.getWorldRotationX();
						angleToOldTip -= (16384 - (int)(16384.499999999996 - angleToOldTip / 360)) * 360;
						state.offset = linear.apply(0, angleToOldTip, data.inertia);
// if (angleToOldTip > 0.0001f || angleToOldTip < -0.0001f) //
// System.out.println(angleToOldTip);
// if (applyShear) {
// if (rotationOffset > 0)
// rotationOffset = Math.max(0, rotationOffset - shearOffset);
// else
// rotationOffset = Math.min(0, rotationOffset - shearOffset);
// }
					}
					tip = bone.localToWorld(new Vector2(bone.data.length, 0));
// if (bone.worldX!=271.64316f)
// System.out.println(bone.worldX);
					if (bone.worldY != 662.5888f) System.out.println(bone.worldY);
// System.out.println(bone.worldY);
					state.x = bone.worldX;
					state.y = bone.worldY;
					state.tipx = tip.x;
					state.tipy = tip.y;
				}
				// BOZO - Update physics x, y, scaleX, shearX.
			}
		}

		boolean angle = data.rotate || data.scaleX || data.shearX;

		remaining += Math.max(skeleton.time - lastTime, 0);
		lastTime = skeleton.time;

		float step = 0.016f; // BOZO - Keep fixed step? Make it configurable?
		float cos = 0, sin = 0;
		while (remaining >= step) {
			remaining -= step;

			for (int i = 0; i < boneCount; i++) {
				Bone bone = (Bone)bones[i];
				if (angle) {
					float r = bone.getWorldRotationX() * degRad;
					cos = (float)Math.cos(r);
					sin = (float)Math.sin(r);
				}
				if (data.rotate) {
					State state = states[i];
					// BOZO - Keep length affecting rotation? Calculate world length?
					float windForce = bone.data.length * 0.5f * (-data.wind * sin - data.gravity * cos);
					float springForce = state.offset * data.strength;
					float frictionForce = data.friction * state.velocity;
					state.velocity += (windForce - springForce - frictionForce) / data.mass;
					state.offset += state.velocity * step;
					state.velocity *= data.damping;
				}
			}
		}

		if (mix == 1) {
			for (int i = 0; i < boneCount; i++) {
				Bone bone = (Bone)bones[i];
				if (angle) {
					float r = bone.getWorldRotationX() * degRad;
					cos = (float)Math.cos(r);
					sin = (float)Math.sin(r);
				}
				if (data.rotate) {
					State state = states[i];
					bone.rotateWorld(state.offset);
					bone.updateAppliedTransform();
				}
			}
		} else {
			// BOZO - PhysicsConstraint mix.
		}
	}

	public void step () {
		// BOZO - PhysicsConstraint#step.
	}

	/** The bones that will be modified by this physics constraint. */
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

	static class State {
		boolean last;
		float x, y, tipx, tipy, offset, velocity;
	}
}
