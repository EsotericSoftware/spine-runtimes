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
	/// Stores the setup pose for a <see cref="PathConstraint"/>.
	/// </summary>
	/// <remarks>
	/// See <a href="https://esotericsoftware.com/spine-path-constraints">Path constraints</a> in the Spine User Guide.
	/// </remarks>
	public class PathConstraintData : ConstraintData {
		internal ExposedList<BoneData> bones = new ExposedList<BoneData>();
		internal SlotData target;
		internal PositionMode positionMode;
		internal SpacingMode spacingMode;
		internal RotateMode rotateMode;
		internal float offsetRotation;
		internal float position, spacing, mixRotate, mixX, mixY;

		public PathConstraintData (string name) : base(name) {
		}

		/// <summary>
		/// The bones that will be modified by this path constraint.
		/// </summary>
		public ExposedList<BoneData> Bones { get { return bones; } }
		/// <summary>
		/// The slot whose path attachment will be used to constrain the bones.
		/// </summary>
		public SlotData Target { get { return target; } set { target = value; } }
		/// <summary>
		/// The mode for positioning the first bone on the path.
		/// </summary>
		public PositionMode PositionMode { get { return positionMode; } set { positionMode = value; } }
		/// <summary>
		/// The mode for positioning the bones after the first bone on the path.
		/// </summary>
		public SpacingMode SpacingMode { get { return spacingMode; } set { spacingMode = value; } }
		/// <summary>
		/// The mode for adjusting the rotation of the bones.
		/// </summary>
		public RotateMode RotateMode { get { return rotateMode; } set { rotateMode = value; } }
		/// <summary>
		/// An offset added to the constrained bone rotation.
		/// </summary>
		public float OffsetRotation { get { return offsetRotation; } set { offsetRotation = value; } }
		/// <summary>
		/// The position along the path.
		/// </summary>
		public float Position { get { return position; } set { position = value; } }
		/// <summary>
		/// The spacing between bones.
		/// </summary>
		public float Spacing { get { return spacing; } set { spacing = value; } }
		/// <summary> A percentage (0-1) that controls the mix between the constrained and unconstrained rotation.</summary>
		public float RotateMix { get { return mixRotate; } set { mixRotate = value; } }
		/// <summary> A percentage (0-1) that controls the mix between the constrained and unconstrained translation X.</summary>
		public float MixX { get { return mixX; } set { mixX = value; } }
		/// <summary> A percentage (0-1) that controls the mix between the constrained and unconstrained translation Y.</summary>
		public float MixY { get { return mixY; } set { mixY = value; } }
	}

	/// <summary>
	/// Controls how the first bone is positioned along the path.
	/// </summary>
	/// <remarks>
	/// See <a href="https://esotericsoftware.com/spine-path-constraints#Position-mode">Position mode</a> in the Spine User Guide.
	/// </remarks>
	public enum PositionMode {
		Fixed, Percent
	}

	/// <summary>
	/// Controls how bones after the first bone are positioned along the path.
	/// </summary>
	/// <remarks>
	/// See <a href="https://esotericsoftware.com/spine-path-constraints#Spacing-mode">Spacing mode</a> in the Spine User Guide.
	/// </remarks>
	public enum SpacingMode {
		Length, Fixed, Percent, Proportional
	}

	/// <summary>
	/// Controls how bones are rotated, translated, and scaled to match the path.
	/// </summary>
	/// <remarks>
	/// See <a href="https://esotericsoftware.com/spine-path-constraints#Rotate-mode">Rotate mode</a> in the Spine User Guide.
	/// </remarks>
	public enum RotateMode {
		Tangent, Chain,
		/// <summary>
		/// When chain scale, constrained bones should all have the same parent. That way when the path constraint scales a bone, it doesn't affect other constrained bones.
		/// </summary>
		ChainScale
	}
}
