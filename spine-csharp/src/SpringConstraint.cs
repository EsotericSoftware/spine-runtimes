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

using System;

namespace Spine {
	/// <summary>
	/// Stores the current pose for a spring constraint. A spring constraint applies physics to bones.
	/// <para>
	/// See <a href="http://esotericsoftware.com/spine-spring-constraints">Spring constraints</a> in the Spine User Guide.</para>
	/// </summary>
	public class SpringConstraint : IUpdatable {
		internal readonly SpringConstraintData data;
		internal readonly ExposedList<Bone> bones;
		// BOZO! - stiffness -> strength. stiffness, damping, rope, stretch -> move to spring.
		internal float mix, friction, gravity, wind, stiffness, damping;
		internal bool rope, stretch;

		internal bool active;

		public SpringConstraint (SpringConstraintData data, Skeleton skeleton) {
			if (data == null) throw new ArgumentNullException("data", "data cannot be null.");
			if (skeleton == null) throw new ArgumentNullException("skeleton", "skeleton cannot be null.");
			this.data = data;
			mix = data.mix;
			friction = data.friction;
			gravity = data.gravity;
			wind = data.wind;
			stiffness = data.stiffness;
			damping = data.damping;
			rope = data.rope;
			stretch = data.stretch;

			bones = new ExposedList<Bone>(data.Bones.Count);
			foreach (BoneData boneData in data.bones)
				bones.Add(skeleton.bones.Items[boneData.index]);
		}

		/// <summary>Copy constructor.</summary>
		public SpringConstraint (SpringConstraint constraint, Skeleton skeleton) {
			if (constraint == null) throw new ArgumentNullException("constraint", "constraint cannot be null.");
			if (skeleton == null) throw new ArgumentNullException("skeleton", "skeleton cannot be null.");
			data = constraint.data;
			bones = new ExposedList<Bone>(constraint.bones.Count);
			foreach (Bone bone in constraint.bones)
				bones.Add(skeleton.bones.Items[bone.data.index]);
			mix = constraint.mix;
			friction = constraint.friction;
			gravity = constraint.gravity;
			wind = constraint.wind;
			stiffness = constraint.stiffness;
			damping = constraint.damping;
			rope = constraint.rope;
			stretch = constraint.stretch;
		}

		/// <summary>Applies the constraint to the constrained bones.</summary>
		public void Update () {

		}

		/// <summary>A percentage (0-1) that controls the mix between the constrained and unconstrained poses.</summary>
		public float Mix { get { return mix; } set { mix = value; } }
		public float Friction { get { return friction; } set { friction = value; } }
		public float Gravity { get { return gravity; } set { gravity = value; } }
		public float Wind { get { return wind; } set { wind = value; } }
		public float Stiffness { get { return stiffness; } set { stiffness = value; } }
		public float Damping { get { return damping; } set { damping = value; } }
		public bool Rope { get { return rope; } set { rope = value; } }
		public bool Stretch { get { return stretch; } set { stretch = value; } }
		public bool Active { get { return active; } }
		/// <summary>The spring constraint's setup pose data.</summary>
		public SpringConstraintData Data { get { return data; } }

		override public string ToString () {
			return data.name;
		}
	}
}
