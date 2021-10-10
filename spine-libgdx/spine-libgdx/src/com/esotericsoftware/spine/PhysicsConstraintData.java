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

import static com.esotericsoftware.spine.utils.SpineUtils.*;

import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.Null;

/** Stores the setup pose for a {@link PhysicsConstraint}.
 * <p>
 * See <a href="http://esotericsoftware.com/spine-physics-constraints">Physics constraints</a> in the Spine User Guide. */
public class PhysicsConstraintData extends ConstraintData {
	final Array<NodeData> nodes = new Array();
	final Array<SpringData> springs = new Array();
	float mix, length, strength, damping, gravity, wind;

	public PhysicsConstraintData (String name) {
		super(name);
	}

	public Array<NodeData> getNodes () {
		return nodes;
	}

	public Array<SpringData> getSprings () {
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

	static public class NodeData {
		public int parentBone = -1;
		public int[] bones;
		public float x, y;
	}

	static public class Node {
		public final NodeData data;
		public final @Null Bone parentBone;
		public final Bone[] bones;
		public float x, y, px, py, ax, ay;

		public Node (NodeData data, Skeleton skeleton) {
			this.data = data;

			parentBone = data.parentBone == -1 ? null : skeleton.bones.get(data.parentBone);

			bones = new Bone[data.bones.length];
			for (int i = 0, n = bones.length; i < n; i++)
				bones[i] = skeleton.bones.get(data.bones[i]);

			setToSetupPose();
		}

		public Node (Node node) {
			this.data = node.data;
			parentBone = node.parentBone;
			bones = new Bone[node.bones.length];
			arraycopy(node.bones, 0, bones, 0, bones.length);
			x = node.x;
			y = node.y;
			px = node.px;
			py = node.py;
			ax = node.ax;
			ay = node.ay;
		}

		public void setToSetupPose () {
			x = data.x;
			y = data.y;
			px = x;
			py = y;
			ax = 0;
			ay = 0;
		}
	}

	static public class SpringData {
		public int node1, node2;
		public int[] bones;
		public float length, strength, damping;
		public boolean rope, stretch;
	}

	static public class Spring {
		public final SpringData data;
		public final Node node1, node2;
		public final Bone[] bones;
		public float length, strength, damping;

		public Spring (SpringData data, PhysicsConstraint constraint, Skeleton skeleton) {
			this.data = data;

			node1 = constraint.nodes.get(data.node1);
			node2 = constraint.nodes.get(data.node2);

			bones = new Bone[data.bones.length];
			for (int i = 0, n = bones.length; i < n; i++)
				bones[i] = skeleton.bones.get(data.bones[i]);

			setToSetupPose();
		}

		public Spring (Spring spring, PhysicsConstraint constraint) {
			this.data = spring.data;
			node1 = constraint.nodes.get(data.node1);
			node2 = constraint.nodes.get(data.node2);
			bones = new Bone[spring.bones.length];
			arraycopy(spring.bones, 0, bones, 0, bones.length);
			length = spring.length;
			strength = spring.strength;
			damping = spring.damping;
		}

		public void setToSetupPose () {
			length = data.length;
			strength = data.strength;
			damping = data.damping;
		}
	}
}
