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

namespace Spine {
	using FromProperty = TransformConstraintData.FromProperty;
	using ToProperty = TransformConstraintData.ToProperty;

	/// <summary>
	/// Stores a pose for a transform constraint.
	/// </summary>
	public class TransformConstraintPose : IPose<TransformConstraintPose> {
		internal float mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY;

		public void Set (TransformConstraintPose pose) {
			mixRotate = pose.mixRotate;
			mixX = pose.mixX;
			mixY = pose.mixY;
			mixScaleX = pose.mixScaleX;
			mixScaleY = pose.mixScaleY;
			mixShearY = pose.mixShearY;
		}

		/// <summary>A percentage that controls the mix between the constrained and unconstrained rotation.</summary>
		public float MixRotate { get { return mixRotate; } set { mixRotate = value; } }
		/// <summary>A percentage that controls the mix between the constrained and unconstrained translation X.</summary>
		public float MixX { get { return mixX; } set { mixX = value; } }
		/// <summary>A percentage that controls the mix between the constrained and unconstrained translation Y.</summary>
		public float MixY { get { return mixY; } set { mixY = value; } }
		/// <summary>A percentage that controls the mix between the constrained and unconstrained scale X.</summary>
		public float MixScaleX { get { return mixScaleX; } set { mixScaleX = value; } }
		/// <summary>A percentage that controls the mix between the constrained and unconstrained scale X.</summary>
		public float MixScaleY { get { return mixScaleY; } set { mixScaleY = value; } }
		/// <summary>A percentage that controls the mix between the constrained and unconstrained shear Y.</summary>
		public float MixShearY { get { return mixShearY; } set { mixShearY = value; } }
	}
}
