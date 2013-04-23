/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
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
 ******************************************************************************/

using System;

namespace Spine {
	public class Slot {
		public SlotData Data { get; private set; }
		public Bone Bone { get; private set; }
		public Skeleton Skeleton { get; private set; }
		public float R { get; set; }
		public float G { get; set; }
		public float B { get; set; }
		public float A { get; set; }

		/** May be null. */
		private Attachment attachment;
		public Attachment Attachment {
			get {
				return attachment;
			}
			set {
				attachment = value;
				attachmentTime = Skeleton.Time;
			}
		}

		private float attachmentTime;
		public float AttachmentTime {
			get {
				return Skeleton.Time - attachmentTime;
			}
			set {
				attachmentTime = Skeleton.Time - value;
			}
		}

		public Slot (SlotData data, Skeleton skeleton, Bone bone) {
			if (data == null) throw new ArgumentNullException("data cannot be null.");
			if (skeleton == null) throw new ArgumentNullException("skeleton cannot be null.");
			if (bone == null) throw new ArgumentNullException("bone cannot be null.");
			Data = data;
			Skeleton = skeleton;
			Bone = bone;
			SetToBindPose();
		}

		internal void SetToBindPose (int slotIndex) {
			R = Data.R;
			G = Data.G;
			B = Data.B;
			A = Data.A;
			Attachment = Data.AttachmentName == null ? null : Skeleton.GetAttachment(slotIndex, Data.AttachmentName);
		}

		public void SetToBindPose () {
			SetToBindPose(Skeleton.Data.Slots.IndexOf(Data));
		}

		override public String ToString () {
			return Data.Name;
		}
	}
}
