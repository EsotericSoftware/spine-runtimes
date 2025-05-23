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

using System;

namespace Spine {
	/// <summary>
	/// Stores the setup pose for a <see cref="TransformConstraint"/>.
	/// </summary>
	/// <remarks>
	/// See <a href="https://esotericsoftware.com/spine-transform-constraints">Transform constraints</a> in the Spine User Guide.
	/// </remarks>
	public class TransformConstraintData : ConstraintData {
		internal ExposedList<BoneData> bones = new ExposedList<BoneData>();
		internal BoneData target;
		internal float mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY;
		internal float offsetRotation, offsetX, offsetY, offsetScaleX, offsetScaleY, offsetShearY;
		internal bool relative, local;

		/// <summary>The bones that will be modified by this transform constraint.</summary>
		public ExposedList<BoneData> Bones { get { return bones; } }
		/// <summary>The target bone whose world transform will be copied to the constrained bones.</summary>
		public BoneData Target { get { return target; } set { target = value; } }
		/// <summary>A percentage (0-1) that controls the mix between the constrained and unconstrained rotation.</summary>
		public float MixRotate { get { return mixRotate; } set { mixRotate = value; } }
		/// <summary>A percentage (0-1) that controls the mix between the constrained and unconstrained translation X.</summary>
		public float MixX { get { return mixX; } set { mixX = value; } }
		/// <summary>A percentage (0-1) that controls the mix between the constrained and unconstrained translation Y.</summary>
		public float MixY { get { return mixY; } set { mixY = value; } }
		/// <summary>A percentage (0-1) that controls the mix between the constrained and unconstrained scale X.</summary>
		public float MixScaleX { get { return mixScaleX; } set { mixScaleX = value; } }
		/// <summary>A percentage (0-1) that controls the mix between the constrained and unconstrained scale Y.</summary>
		public float MixScaleY { get { return mixScaleY; } set { mixScaleY = value; } }
		/// <summary>A percentage (0-1) that controls the mix between the constrained and unconstrained shear Y.</summary>
		public float MixShearY { get { return mixShearY; } set { mixShearY = value; } }

		/// <summary>An offset added to the constrained bone rotation.</summary>
		public float OffsetRotation { get { return offsetRotation; } set { offsetRotation = value; } }
		/// <summary>An offset added to the constrained bone X translation.</summary>
		public float OffsetX { get { return offsetX; } set { offsetX = value; } }
		/// <summary>An offset added to the constrained bone Y translation.</summary>
		public float OffsetY { get { return offsetY; } set { offsetY = value; } }
		/// <summary>An offset added to the constrained bone scaleX.</summary>
		public float OffsetScaleX { get { return offsetScaleX; } set { offsetScaleX = value; } }
		/// <summary>An offset added to the constrained bone scaleY.</summary>
		public float OffsetScaleY { get { return offsetScaleY; } set { offsetScaleY = value; } }
		/// <summary>An offset added to the constrained bone shearY.</summary>
		public float OffsetShearY { get { return offsetShearY; } set { offsetShearY = value; } }

		public bool Relative { get { return relative; } set { relative = value; } }
		public bool Local { get { return local; } set { local = value; } }

		public TransformConstraintData (string name) : base(name) {
		}
	}
}
