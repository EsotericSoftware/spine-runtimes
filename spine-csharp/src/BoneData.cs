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

	/// <summary>
	/// The setup pose for a bone.
	/// </summary>
	public class BoneData : PosedData<BonePose> {
		internal int index;
		internal BoneData parent;
		internal float length;

		/// <param name="parent">May be null.</param>
		public BoneData (int index, string name, BoneData parent)
			: base(name, new BonePose()) {

			if (index < 0) throw new ArgumentException("index must be >= 0", "index");
			if (name == null) throw new ArgumentNullException("name", "name cannot be null.");
			this.index = index;
			this.parent = parent;
		}

		/// <summary>Copy constructor.</summary>
		/// <param name="parent">May be null.</param>
		public BoneData (BoneData data, BoneData parent)
			: this(data.index, data.name, parent) {
			length = data.length;
			setupPose.Set(data.setupPose);
		}

		/// <summary>The <see cref="Skeleton.Bones"/> index.</summary>
		public int Index { get { return index; } }

		/// <summary>May be null.</summary>
		public BoneData Parent { get { return parent; } }

		public float Length { get { return length; } set { length = value; } }
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
