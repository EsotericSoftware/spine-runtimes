/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Collections;

namespace Spine {
	/// <summary>Stores attachments by slot index and attachment name.
	/// <para>See SkeletonData <see cref="Spine.SkeletonData.DefaultSkin"/>, Skeleton <see cref="Spine.Skeleton.Skin"/>, and 
	/// <a href="http://esotericsoftware.com/spine-runtime-skins">Runtime skins</a> in the Spine Runtimes Guide.</para>
	/// </summary>
	public class Skin {
		internal string name;
		private OrderedDictionary<SkinEntry, Attachment> attachments = new OrderedDictionary<SkinEntry, Attachment>(SkinEntryComparer.Instance);
		internal readonly ExposedList<BoneData> bones = new ExposedList<BoneData>();
		internal readonly ExposedList<ConstraintData> constraints = new ExposedList<ConstraintData>();

		public string Name { get { return name; } }
		public OrderedDictionary<SkinEntry, Attachment> Attachments { get { return attachments; } }
		public ExposedList<BoneData> Bones { get { return bones; } }
		public ExposedList<ConstraintData> Constraints { get { return constraints; } }
		
		public Skin (string name) {
			if (name == null) throw new ArgumentNullException("name", "name cannot be null.");
			this.name = name;
		}

		/// <summary>Adds an attachment to the skin for the specified slot index and name.
		/// If the name already exists for the slot, the previous value is replaced.</summary>
		public void SetAttachment (int slotIndex, string name, Attachment attachment) {
			if (attachment == null) throw new ArgumentNullException("attachment", "attachment cannot be null.");
			if (slotIndex < 0) throw new ArgumentNullException("slotIndex", "slotIndex must be >= 0.");
			attachments[new SkinEntry(slotIndex, name, attachment)] = attachment;
		}

		///<summary>Adds all attachments, bones, and constraints from the specified skin to this skin.</summary>
		public void AddSkin (Skin skin) {
			foreach (BoneData data in skin.bones)
				if (!bones.Contains(data)) bones.Add(data);

			foreach (ConstraintData data in skin.constraints)
				if (!constraints.Contains(data)) constraints.Add(data);

			foreach (SkinEntry entry in skin.attachments.Keys)
				SetAttachment(entry.SlotIndex, entry.Name, entry.Attachment);
		}

		///<summary>Adds all attachments from the specified skin to this skin. Attachments are deep copied.</summary>
		public void CopySkin (Skin skin) {
			foreach (BoneData data in skin.bones)
				if (!bones.Contains(data)) bones.Add(data);

			foreach (ConstraintData data in skin.constraints)
				if (!constraints.Contains(data)) constraints.Add(data);

			foreach (SkinEntry entry in skin.attachments.Keys) {
				if (entry.Attachment is MeshAttachment)
					SetAttachment(entry.SlotIndex, entry.Name,
						entry.Attachment != null ? ((MeshAttachment)entry.Attachment).NewLinkedMesh() : null);
				else
					SetAttachment(entry.SlotIndex, entry.Name, entry.Attachment != null ? entry.Attachment.Copy() : null);
			}
		}

		/// <summary>Returns the attachment for the specified slot index and name, or null.</summary>
		/// <returns>May be null.</returns>
		public Attachment GetAttachment (int slotIndex, string name) {
			var lookup = new SkinEntry(slotIndex, name, null);
			Attachment attachment = null;
			bool containsKey = attachments.TryGetValue(lookup, out attachment);
			return containsKey ? attachment : null;
		}

		/// <summary> Removes the attachment in the skin for the specified slot index and name, if any.</summary>
		public void RemoveAttachment (int slotIndex, string name) {
			if (slotIndex < 0) throw new ArgumentOutOfRangeException("slotIndex", "slotIndex must be >= 0");
			var lookup = new SkinEntry(slotIndex, name, null);
			attachments.Remove(lookup);
		}

		///<summary>Returns all attachments contained in this skin.</summary>
		public ICollection<SkinEntry> GetAttachments () {
			return this.attachments.Keys;
		}

		/// <summary>Returns all attachments in this skin for the specified slot index.</summary>
		/// <param name="slotIndex">The target slotIndex. To find the slot index, use <see cref="Spine.Skeleton.FindSlotIndex"/> or <see cref="Spine.SkeletonData.FindSlotIndex"/>
		public void GetAttachments (int slotIndex, List<SkinEntry> attachments) {
			foreach (SkinEntry entry in this.attachments.Keys)
				if (entry.SlotIndex == slotIndex) attachments.Add(entry);
		}

		///<summary>Clears all attachments, bones, and constraints.</summary>
		public void Clear () {
			attachments.Clear();
			bones.Clear();
			constraints.Clear();
		}

		override public string ToString () {
			return name;
		}

		/// <summary>Attach all attachments from this skin if the corresponding attachment from the old skin is currently attached.</summary>
		internal void AttachAll (Skeleton skeleton, Skin oldSkin) {
			foreach (SkinEntry entry in oldSkin.attachments.Keys) {
				int slotIndex = entry.SlotIndex;
				Slot slot = skeleton.slots.Items[slotIndex];
				if (slot.Attachment == entry.Attachment) {
					Attachment attachment = GetAttachment(slotIndex, entry.Name);
					if (attachment != null) slot.Attachment = attachment;
				}
			}
		}

		/// <summary>Stores an entry in the skin consisting of the slot index, name, and attachment.</summary>
		public struct SkinEntry {
			private readonly int slotIndex;
			private readonly string name;
			private readonly Attachment attachment;
			internal readonly int hashCode;

			public SkinEntry (int slotIndex, string name, Attachment attachment) {
				this.slotIndex = slotIndex;
				this.name = name;
				this.attachment = attachment;
				this.hashCode = this.name.GetHashCode() + this.slotIndex * 37;
			}

			public int SlotIndex {
				get {
					return slotIndex;
				}
			}

			/// <summary>The name the attachment is associated with, equivalent to the skin placeholder name in the Spine editor.</summary>
			public String Name {
				get {
					return name;
				}
			}

			public Attachment Attachment {
				get {
					return attachment;
				}
			}
		}
	
		// Avoids boxing in the dictionary and is necessary to omit entry.attachment in the comparison.
		class SkinEntryComparer : IEqualityComparer<SkinEntry> {
			internal static readonly SkinEntryComparer Instance = new SkinEntryComparer();

			bool IEqualityComparer<SkinEntry>.Equals (SkinEntry e1, SkinEntry e2) {
				if (e1.SlotIndex != e2.SlotIndex) return false;
				if (!string.Equals(e1.Name, e2.Name, StringComparison.Ordinal)) return false;
				return true;
			}

			int IEqualityComparer<SkinEntry>.GetHashCode (SkinEntry e) {
				return e.Name.GetHashCode() + e.SlotIndex * 37;
			}
		}
	}
}
