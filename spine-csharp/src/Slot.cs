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
	public class Slot {
		internal SlotData data;
		internal Bone bone;
		internal float r, g, b, a;
		internal Attachment attachment;
		internal float attachmentTime;
		internal ExposedList<float> attachmentVertices = new ExposedList<float>();

		public SlotData Data { get { return data; } }
		public Bone Bone { get { return bone; } }
		public Skeleton Skeleton { get { return bone.skeleton; } }
		public float R { get { return r; } set { r = value; } }
		public float G { get { return g; } set { g = value; } }
		public float B { get { return b; } set { b = value; } }
		public float A { get { return a; } set { a = value; } }

		/// <summary>May be null.</summary>
		public Attachment Attachment {
			get { return attachment; }
			set {
				if (attachment == value) return;
				attachment = value;
				attachmentTime = bone.skeleton.time;
				attachmentVertices.Clear(false);
			}
		}

		public float AttachmentTime {
			get { return bone.skeleton.time - attachmentTime; }
			set { attachmentTime = bone.skeleton.time - value; }
		}

		public ExposedList<float> AttachmentVertices { get { return attachmentVertices; } set { attachmentVertices = value; } }

		public Slot (SlotData data, Bone bone) {
			if (data == null) throw new ArgumentNullException("data", "data cannot be null.");
			if (bone == null) throw new ArgumentNullException("bone", "bone cannot be null.");
			this.data = data;
			this.bone = bone;
			SetToSetupPose();
		}

		public void SetToSetupPose () {
			r = data.r;
			g = data.g;
			b = data.b;
			a = data.a;
			if (data.attachmentName == null)
				Attachment = null;
			else {
				attachment = null;
				Attachment = bone.skeleton.GetAttachment(data.index, data.attachmentName);
			}
		}

		override public String ToString () {
			return data.name;
		}
	}
}
