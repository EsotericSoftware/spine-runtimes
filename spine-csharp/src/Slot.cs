/******************************************************************************
 * Spine Runtime Software License - Version 1.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Essential, Professional, Enterprise, or Education License must
 *    be purchased from Esoteric Software and the license must remain valid:
 *    http://esotericsoftware.com/
 * 2. Redistributions of source code must retain this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer.
 * 3. Redistributions in binary form must reproduce this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer, in the documentation and/or other materials provided with the
 *    distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;

namespace Spine {
	public class Slot {
		internal SlotData data;
		internal Bone bone;
		internal Skeleton skeleton;
		internal float r, g, b, a;
		internal float attachmentTime;
		internal Attachment attachment;

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
