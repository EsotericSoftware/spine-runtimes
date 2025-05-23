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
	/// Stores the setup pose for a <see cref="Bone"/>.
	/// </summary>
	public class BoneData {
		internal int index;
		internal string name;
		internal BoneData parent;
		internal float length;
		internal float x, y, rotation, scaleX = 1, scaleY = 1, shearX, shearY;
		internal Inherit inherit = Inherit.Normal;
		internal bool skinRequired;

		/// <summary>The index of the bone in <see cref="Skeleton.Bones"/>.</summary>
		public int Index { get { return index; } }

		/// <summary>The name of the bone, which is unique across all bones in the skeleton.</summary>
		public string Name { get { return name; } }

		/// <summary>May be null.</summary>
		public BoneData Parent { get { return parent; } }

		/// <summary>The bone's length.</summary>
		public float Length { get { return length; } set { length = value; } }

		/// <summary>The local x translation.</summary>
		public float X { get { return x; } set { x = value; } }

		/// <summary>The local y translation.</summary>
		public float Y { get { return y; } set { y = value; } }

		/// <summary>The local rotation in degrees, counter clockwise.</summary>
		public float Rotation { get { return rotation; } set { rotation = value; } }

		/// <summary>The local scaleX.</summary>
		public float ScaleX { get { return scaleX; } set { scaleX = value; } }

		/// <summary>The local scaleY.</summary>
		public float ScaleY { get { return scaleY; } set { scaleY = value; } }

		/// <summary>The local shearX.</summary>
		public float ShearX { get { return shearX; } set { shearX = value; } }

		/// <summary>The local shearY.</summary>
		public float ShearY { get { return shearY; } set { shearY = value; } }

		/// <summary>Determines how parent world transforms affect this bone.</summary>
		public Inherit Inherit { get { return inherit; } set { inherit = value; } }

		/// <summary>When true, <see cref="Skeleton.UpdateWorldTransform(Skeleton.Physics)"/> only updates this bone if the <see cref="Skeleton.Skin"/> contains
		/// this bone.</summary>
		/// <remarks>
		/// See <see cref="Skin.Bones"/>.
		/// </remarks>
		public bool SkinRequired { get { return skinRequired; } set { skinRequired = value; } }

		/// <summary>
		/// Initializes a new instance of the BoneData class.
		/// </summary>
		/// <param name="index">The index of the bone.</param>
		/// <param name="name">The name of the bone.</param>
		/// <param name="parent">May be null.</param>
		public BoneData (int index, string name, BoneData parent) {
			if (index < 0) throw new ArgumentException("index must be >= 0", "index");
			if (name == null) throw new ArgumentNullException("name", "name cannot be null.");
			this.index = index;
			this.name = name;
			this.parent = parent;
		}

		override public string ToString () {
			return name;
		}
	}

	/// <summary>
	/// Determines how a bone inherits world transforms from parent bones.
	/// </summary>
	public enum Inherit {
		Normal,
		OnlyTranslation,
		NoRotationOrReflection,
		NoScale,
		NoScaleOrReflection
	}

	public class InheritEnum {
		public static readonly Inherit[] Values = {
			Inherit.Normal,
			Inherit.OnlyTranslation,
			Inherit.NoRotationOrReflection,
			Inherit.NoScale,
			Inherit.NoScaleOrReflection
		};
	}
}
