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

#if UNITY_5_3_OR_NEWER
#define IS_UNITY
#endif

using System;

namespace Spine {
	public class SlotData : PosedData<SlotPose> {
		internal int index;
		internal BoneData boneData;
		internal string attachmentName;
		internal BlendMode blendMode;

		// Nonessential.
		// bool visible = true;

		public SlotData (int index, String name, BoneData boneData)
			: base(name, new SlotPose()) {
			if (index < 0) throw new ArgumentException("index must be >= 0.", "index");
			if (boneData == null) throw new ArgumentNullException("boneData", "boneData cannot be null.");
			this.index = index;
			this.boneData = boneData;
		}

		/// <summary>The <see cref="Skeleton.Slots"/> index.</summary>
		public int Index { get { return index; } }

		/// <summary>The bone this slot belongs to.</summary>
		public BoneData BoneData { get { return boneData; } }

		/// <summary>The name of the attachment that is visible for this slot in the setup pose, or null if no attachment is visible.</summary>
		public String AttachmentName { get { return attachmentName; } set { attachmentName = value; } }
		/// <summary>The blend mode for drawing the slot's attachment.</summary>
		public BlendMode BlendMode { get { return blendMode; } set { blendMode = value; } }
	}
}
