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

import com.badlogic.gdx.math.Vector2;
import com.badlogic.gdx.utils.Array;

import com.esotericsoftware.spine.PhysicsConstraintData.Node;
import com.esotericsoftware.spine.PhysicsConstraintData.NodeData;
import com.esotericsoftware.spine.PhysicsConstraintData.Spring;
import com.esotericsoftware.spine.PhysicsConstraintData.SpringData;

/** Stores the current pose for a physics constraint. A physics constraint applies physics to bones.
 * <p>
 * See <a href="http://esotericsoftware.com/spine-physics-constraints">Physics constraints</a> in the Spine User Guide. */
public class PhysicsConstraint implements Updatable {
	final PhysicsConstraintData data;
	final Array<Node> nodes;
	final Array<Spring> springs;
	float mix, length, strength, damping, gravity, wind;

	boolean active;

	final Skeleton skeleton;
	float remaining, lastTime;
	final Vector2 temp = new Vector2();

	public PhysicsConstraint (PhysicsConstraintData data, Skeleton skeleton) {
		if (data == null) throw new IllegalArgumentException("data cannot be null.");
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");
		this.data = data;
		this.skeleton = skeleton;

		nodes = new Array(data.nodes.size);
		for (NodeData nodeData : data.nodes)
			nodes.add(new Node(nodeData, skeleton));

		springs = new Array(data.springs.size);
		for (SpringData springData : data.springs)
			springs.add(new Spring(springData, this, skeleton));

		mix = data.mix;
		length = data.length;
		strength = data.strength;
		damping = data.damping;
		gravity = data.gravity;
		wind = data.wind;
	}

	/** Copy constructor. */
	public PhysicsConstraint (PhysicsConstraint constraint) {
		if (constraint == null) throw new IllegalArgumentException("constraint cannot be null.");
		data = constraint.data;
		skeleton = constraint.skeleton;

		nodes = new Array(constraint.nodes.size);
		for (Node node : constraint.nodes)
			nodes.add(new Node(node));

		springs = new Array(constraint.springs.size);
		for (Spring spring : constraint.springs)
			springs.add(new Spring(spring, this));

		mix = constraint.mix;
		length = constraint.length;
		strength = constraint.strength;
		damping = constraint.damping;
		gravity = constraint.gravity;
		wind = constraint.wind;
	}

	public void setToSetupPose () {
		remaining = 0;
		lastTime = skeleton.time;

		Object[] nodes = this.nodes.items;
		for (int i = 0, n = this.nodes.size; i < n; i++)
			((Node)nodes[i]).setToSetupPose();

		Object[] springs = this.springs.items;
		for (int i = 0, n = this.springs.size; i < n; i++)
			((Spring)springs[i]).setToSetupPose();

		PhysicsConstraintData data = this.data;
		mix = data.mix;
		length = data.length;
		strength = data.strength;
		damping = data.damping;
		gravity = data.gravity;
		wind = data.wind;
	}

	/** Applies the constraint to the constrained bones. */
	public void update () {
		Object[] nodes = this.nodes.items;
		int nodeCount = this.nodes.size;
		Vector2 temp = this.temp;
		for (int i = 0; i < nodeCount; i++) {
			Node node = (Node)nodes[i];
			if (node.parentBone == null) continue;
			node.parentBone.localToWorld(temp.set(node.data.x, node.data.y));
			node.x = temp.x;
			node.y = temp.y;
		}

		Object[] springs = this.springs.items;
		int springCount = this.springs.size;

		remaining += Math.max(skeleton.time - lastTime, 0);
		lastTime = skeleton.time;
		while (remaining >= 0.016f) {
			remaining -= 0.016f;
			for (int i = 0; i < springCount; i++)
				((Spring)springs[i]).update();
			for (int i = 0; i < nodeCount; i++)
				((Node)nodes[i]).update(this);
		}

		for (int i = 0; i < nodeCount; i++) {
			Node node = (Node)nodes[i];
			Object[] bones = node.bones;
			int ii = 0, nn = bones.length;
			if (mix == 1) {
				for (; ii < nn; ii++) {
					Bone bone = (Bone)bones[ii];
					bone.worldX = node.x;
					bone.worldY = node.y;
					bone.worldToLocal(temp.set(bone.worldX, bone.worldY));
					bone.ax = temp.x;
					bone.ay = temp.y;
				}
			} else {
				for (; ii < nn; ii++) {
					Bone bone = (Bone)bones[ii];
					bone.worldX = bone.worldX + (node.x - bone.worldX) * mix;
					bone.worldY = bone.worldY + (node.y - bone.worldY) * mix;
					bone.worldToLocal(temp.set(bone.worldX, bone.worldY));
					bone.ax = temp.x;
					bone.ay = temp.y;
				}
			}
		}
	}

	public Array<Node> getNodes () {
		return nodes;
	}

	public Array<Spring> getSprings () {
		return springs;
	}

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained poses. */
	public float getMix () {
		return mix;
	}

	public void setMix (float mix) {
		this.mix = mix;
	}

	public float getLength () {
		return length;
	}

	public void setLength (float length) {
		this.length = length;
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
