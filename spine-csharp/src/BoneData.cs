/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;

namespace Spine {
	public class BoneData {
		internal int index;
		internal string name;
		internal BoneData parent;
		internal float length;
		internal float x, y, rotation, scaleX = 1, scaleY = 1, shearX, shearY;
		internal TransformMode transformMode = TransformMode.Normal;

		/// <summary>The index of the bone in Skeleton.Bones</summary>
		public int Index { get { return index; } }

		/// <summary>The name of the bone, which is unique within the skeleton.</summary>
		public string Name { get { return name; } }

		/// <summary>May be null.</summary>
		public BoneData Parent { get { return parent; } }

		public float Length { get { return length; } set { length = value; } }

		/// <summary>Local X translation.</summary>
		public float X { get { return x; } set { x = value; } }

		/// <summary>Local Y translation.</summary>
		public float Y { get { return y; } set { y = value; } }

		/// <summary>Local rotation.</summary>
		public float Rotation { get { return rotation; } set { rotation = value; } }

		/// <summary>Local scaleX.</summary>
		public float ScaleX { get { return scaleX; } set { scaleX = value; } }

		/// <summary>Local scaleY.</summary>
		public float ScaleY { get { return scaleY; } set { scaleY = value; } }

		/// <summary>Local shearX.</summary>
		public float ShearX { get { return shearX; } set { shearX = value; } }

		/// <summary>Local shearY.</summary>
		public float ShearY { get { return shearY; } set { shearY = value; } }

		/// <summary>The transform mode for how parent world transforms affect this bone.</summary>
		public TransformMode TransformMode { get { return transformMode; } set { transformMode = value; } }

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

	[Flags]
	public enum TransformMode {
		//0000 0 Flip Scale Rotation
		Normal = 0, // 0000
		OnlyTranslation = 7, // 0111
		NoRotationOrReflection = 1, // 0001
		NoScale = 2, // 0010
		NoScaleOrReflection = 6, // 0110
	}
}
