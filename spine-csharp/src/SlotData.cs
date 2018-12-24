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
	public class SlotData {
		internal int index;
		internal string name;
		internal BoneData boneData;
		internal float r = 1, g = 1, b = 1, a = 1;
		internal float r2 = 0, g2 = 0, b2 = 0;
		internal bool hasSecondColor = false;
		internal string attachmentName;
		internal BlendMode blendMode;

		public int Index { get { return index; } }
		public string Name { get { return name; } }
		public BoneData BoneData { get { return boneData; } }
		public float R { get { return r; } set { r = value; } }
		public float G { get { return g; } set { g = value; } }
		public float B { get { return b; } set { b = value; } }
		public float A { get { return a; } set { a = value; } }

		public float R2 { get { return r2; } set { r2 = value; } }
		public float G2 { get { return g2; } set { g2 = value; } }
		public float B2 { get { return b2; } set { b2 = value; } }
		public bool HasSecondColor { get { return hasSecondColor; } set { hasSecondColor = value; } }

		/// <summary>May be null.</summary>
		public String AttachmentName { get { return attachmentName; } set { attachmentName = value; } }
		public BlendMode BlendMode { get { return blendMode; } set { blendMode = value; } }

		public SlotData (int index, String name, BoneData boneData) {
			if (index < 0) throw new ArgumentException ("index must be >= 0.", "index");
			if (name == null) throw new ArgumentNullException("name", "name cannot be null.");
			if (boneData == null) throw new ArgumentNullException("boneData", "boneData cannot be null.");
			this.index = index;
			this.name = name;
			this.boneData = boneData;
		}

		override public string ToString () {
			return name;
		}
	}
}
