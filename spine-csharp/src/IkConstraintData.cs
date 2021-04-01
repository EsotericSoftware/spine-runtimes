/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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
using System.Collections.Generic;

namespace Spine {
	/// <summary>Stores the setup pose for an IkConstraint.</summary>
	public class IkConstraintData : ConstraintData {
		internal ExposedList<BoneData> bones = new ExposedList<BoneData>();
		internal BoneData target;
		internal int bendDirection = 1;
		internal bool compress, stretch, uniform;
		internal float mix = 1, softness;

		public IkConstraintData (string name) : base(name) {
		}

		/// <summary>The bones that are constrained by this IK Constraint.</summary>
		public ExposedList<BoneData> Bones {
			get { return bones; }
		}

		/// <summary>The bone that is the IK target.</summary>
		public BoneData Target {
			get { return target; }
			set { target = value; }
		}

		/// <summary>
		/// A percentage (0-1) that controls the mix between the constrained and unconstrained rotation.
		/// <para>
		/// For two bone IK: if the parent bone has local nonuniform scale, the child bone's local Y translation is set to 0.
		/// </para></summary>
		public float Mix {
			get { return mix; }
			set { mix = value; }
		}

		/// <summary>For two bone IK, the target bone's distance from the maximum reach of the bones where rotation begins to slow. The bones
		/// will not straighten completely until the target is this far out of range.</summary>
		public float Softness {
			get { return softness; }
			set { softness = value; }
		}

		/// <summary>For two bone IK, controls the bend direction of the IK bones, either 1 or -1.</summary>
		public int BendDirection {
			get { return bendDirection; }
			set { bendDirection = value; }
		}

		/// <summary>For one bone IK, when true and the target is too close, the bone is scaled to reach it.</summary>
		public bool Compress {
			get { return compress; }
			set { compress = value; }
		}

		/// <summary>When true and the target is out of range, the parent bone is scaled to reach it.
		/// <para>
		/// For two bone IK: 1) the child bone's local Y translation is set to 0,
		/// 2) stretch is not applied if <see cref="Softness"/> is > 0,
		/// and 3) if the parent bone has local nonuniform scale, stretch is not applied.</para></summary>
		public bool Stretch {
			get { return stretch; }
			set { stretch = value; }
		}

		/// <summary>
		/// When true and <see cref="Compress"/> or <see cref="Stretch"/> is used, the bone is scaled on both the X and Y axes.
		/// </summary>
		public bool Uniform {
			get { return uniform; }
			set { uniform = value; }
		}
	}
}
