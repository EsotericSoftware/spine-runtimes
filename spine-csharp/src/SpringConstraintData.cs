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
	/// Stores the setup pose for a <see cref="SpringConstraint"/>.
	/// <para>
	/// See <a href="http://esotericsoftware.com/spine-spring-constraints">Spring constraints</a> in the Spine User Guide.</para>
	/// </summary>
	public class SpringConstraintData : ConstraintData {
		internal ExposedList<BoneData> bones = new ExposedList<BoneData>();
		internal float mix, friction, gravity, wind, stiffness, damping;
		internal bool rope, stretch;

		public SpringConstraintData (string name) : base(name) {
		}

		/// <summary>The bones that are constrained by this spring constraint.</summary>
		public ExposedList<BoneData> Bones { get { return bones; } }

		/// <summary>A percentage (0-1) that controls the mix between the constrained and unconstrained poses.</summary>
		public float Mix { get { return mix; } set { mix = value; } }
		public float Friction { get { return friction; } set { friction = value; } }
		public float Gravity { get { return gravity; } set { gravity = value; } }
		public float Wind { get { return wind; } set { wind = value; } }
		public float Stiffness { get { return stiffness; } set { stiffness = value; } }
		public float Damping { get { return damping; } set { damping = value; } }
		public bool Rope { get { return rope; } set { rope = value; } }
		public bool Stretch { get { return stretch; } set { stretch = value; } }
	}
}
