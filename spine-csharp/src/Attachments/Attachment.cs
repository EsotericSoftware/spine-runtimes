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

	/// <summary>The base class for all attachments. Multiple <see cref="Skeleton"/> instances, slots, or skins can use the same
	/// attachments.</summary>
	abstract public class Attachment {
		static private readonly int[] Empty = new int[0];

		internal Attachment timelineAttachment;
		internal int[] timelineSlots = Empty;

		/// <summary>The attachment's name.</summary>
		public string Name { get; }

		/// <summary>Timelines for the timeline attachment are also applied to this attachment.
		/// May be null if no attachment-specific timelines should be applied.</summary>
		public Attachment TimelineAttachment { get { return timelineAttachment; } set { timelineAttachment = value; } }

		/// <summary>
		/// Slots that can have attachments whose <see cref="timelineAttachment"/> is this attachment.
		/// </summary>
		public int[] TimelineSlots { get { return timelineSlots; } set { timelineSlots = value; } }

		protected Attachment (string name) {
			if (name == null) throw new ArgumentNullException("name", "name cannot be null");
			this.Name = name;
			timelineAttachment = this;
		}

		/// <summary>Copy constructor.</summary>
		protected Attachment (Attachment other) {
			Name = other.Name;
			timelineAttachment = other.timelineAttachment;
			timelineSlots = other.timelineSlots;
		}

		/// <summary>
		/// Returns true if the <c>slotIndex</c> or any <see cref="timelineSlots"/> have an attachment whose <see cref="timelineAttachment"/> is
		/// this attachment.
		/// </summary>
		/// <param name="slots">The <see cref="Skeleton.Slots"/>.</param>
		/// <param name="slotIndex">The timeline's primary slot index.</param>
		public bool IsTimelineActive (Slot[] slots, int slotIndex, bool appliedPose) {
			Slot slot = slots[slotIndex];
			if (slot.Bone.Active) {
				Attachment other = (appliedPose ? slot.AppliedPose : slot.Pose).Attachment;
				if (other != null && other.timelineAttachment == this) return true;
			}
			for (int i = 0, n = timelineSlots.Length; i < n; i++) {
				slot = slots[timelineSlots[i]];
				if (!slot.Bone.Active) continue;
				Attachment other = (appliedPose ? slot.AppliedPose : slot.Pose).Attachment;
				if (other != null && other.timelineAttachment == this) return true;
			}
			return false;
		}

		override public string ToString () {
			return Name;
		}

		/// <summary>Returns a copy of the attachment.</summary>
		public abstract Attachment Copy ();
	}
}
