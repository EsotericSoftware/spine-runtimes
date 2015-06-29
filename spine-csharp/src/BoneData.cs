/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;

namespace Spine {
	public class BoneData {
		internal BoneData parent;
		internal String name;
		internal float length, x, y, rotation, scaleX = 1, scaleY = 1;
		internal bool flipX, flipY;
		internal bool inheritScale = true, inheritRotation = true;

		/// <summary>May be null.</summary>
		public BoneData Parent { get { return parent; } }
		public String Name { get { return name; } }
		public float Length { get { return length; } set { length = value; } }
		public float X { get { return x; } set { x = value; } }
		public float Y { get { return y; } set { y = value; } }
		public float Rotation { get { return rotation; } set { rotation = value; } }
		public float ScaleX { get { return scaleX; } set { scaleX = value; } }
		public float ScaleY { get { return scaleY; } set { scaleY = value; } }
		public bool FlipX { get { return flipX; } set { flipX = value; } }
		public bool FlipY { get { return flipY; } set { flipY = value; } }
		public bool InheritScale { get { return inheritScale; } set { inheritScale = value; } }
		public bool InheritRotation { get { return inheritRotation; } set { inheritRotation = value; } }

		/// <param name="parent">May be null.</param>
		public BoneData (String name, BoneData parent) {
			if (name == null) throw new ArgumentNullException("name cannot be null.");
			this.name = name;
			this.parent = parent;
		}

		override public String ToString () {
			return name;
		}
	}
}
