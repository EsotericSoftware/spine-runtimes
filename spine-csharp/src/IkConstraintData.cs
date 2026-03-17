/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2026, Esoteric Software LLC
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
	public class IkConstraintData : ConstraintData<IkConstraint, IkConstraintPose> {
		internal ExposedList<BoneData> bones = new ExposedList<BoneData>(2);
		internal BoneData target;
		internal bool uniform;

		public IkConstraintData (string name)
			: base(name, new IkConstraintPose()) {
		}

		override public IConstraint Create (Skeleton skeleton) {
			return new IkConstraint(this, skeleton);
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
		/// When true and <see cref="IkConstraintPose.Compress"/> or <see cref="IkConstraintPose.Stretch"/> is used, the bone is scaled
		/// on both the X and Y axes.
		/// </summary>
		public bool Uniform {
			get { return uniform; }
			set { uniform = value; }
		}
	}
}
