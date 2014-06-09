/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software (typically granted by licensing Spine), you
 * may not (a) modify, translate, adapt or otherwise create derivative works,
 * improvements of the Software or develop new applications using the Software
 * or (b) remove, delete, alter or obscure any trademarks or any copyright,
 * trademark, patent or other intellectual property or proprietary rights
 * notices on or in the Software, including any copy thereof. Redistributions
 * in binary or source form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;

namespace Spine {
	public class Slot {
		internal SlotData data;
		internal Bone bone;
		internal Skeleton skeleton;
		internal float r, g, b, a;
		internal Attachment attachment;
		internal float attachmentTime;
		internal float[] attachmentVertices = new float[0];
		internal int attachmentVerticesCount;

		public SlotData Data { get { return data; } }
		public Bone Bone { get { return bone; } }
		public Skeleton Skeleton { get { return skeleton; } }
		public float R { get { return r; } set { r = value; } }
		public float G { get { return g; } set { g = value; } }
		public float B { get { return b; } set { b = value; } }
		public float A { get { return a; } set { a = value; } }

		/// <summary>May be null.</summary>
		public Attachment Attachment {
			get {
				return attachment;
			}
			set {
				attachment = value;
				attachmentTime = skeleton.time;
				attachmentVerticesCount = 0;
			}
		}

		public float AttachmentTime {
			get {
				return skeleton.time - attachmentTime;
			}
			set {
				attachmentTime = skeleton.time - value;
			}
		}

		public float[] AttachmentVertices { get { return attachmentVertices; } set { attachmentVertices = value; } }
		public int AttachmentVerticesCount { get { return attachmentVerticesCount; } set { attachmentVerticesCount = value; } }

		public Slot (SlotData data, Skeleton skeleton, Bone bone) {
			if (data == null) throw new ArgumentNullException("data cannot be null.");
			if (skeleton == null) throw new ArgumentNullException("skeleton cannot be null.");
			if (bone == null) throw new ArgumentNullException("bone cannot be null.");
			this.data = data;
			this.skeleton = skeleton;
			this.bone = bone;
			SetToSetupPose();
		}

		internal void SetToSetupPose (int slotIndex) {
			r = data.r;
			g = data.g;
			b = data.b;
			a = data.a;
			Attachment = data.attachmentName == null ? null : skeleton.GetAttachment(slotIndex, data.attachmentName);
		}

		public void SetToSetupPose () {
			SetToSetupPose(skeleton.data.slots.IndexOf(data));
		}

		override public String ToString () {
			return data.name;
		}
	}
}
