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
using System.Collections;
using System.Collections.Generic;

namespace Spine {
	/// <summary>Stores attachments by slot index and placeholder name. Multiple <see cref="Skeleton"/> instances can use the same skins.
	/// <para>See <see cref="Spine.SkeletonData.DefaultSkin"/>, <see cref="Spine.Skeleton.Skin"/>, and
	/// <a href="http://esotericsoftware.com/spine-runtime-skins">Runtime skins</a> in the Spine Runtimes Guide.</para>
	/// </summary>
	public class Skin {
		internal string name;
		// Difference to reference implementation: using Dictionary<SkinKey, SkinEntry> instead of HashSet<SkinEntry>.
		// Reason is that there is no efficient way to replace or access an already added element, losing any benefits.
		private Dictionary<SkinKey, SkinEntry> attachments = new Dictionary<SkinKey, SkinEntry>(SkinKeyComparer.Instance);
		internal readonly ExposedList<BoneData> bones = new ExposedList<BoneData>();
		internal readonly ExposedList<IConstraintData> constraints = new ExposedList<IConstraintData>();

		/// <summary>The skin's name, unique across all skins in the skeleton.
		/// <para>See <see cref="SkeletonData.FindSkin(string)"/>.</para></summary>
		public string Name { get { return name; } }
		/// <summary>Returns all attachments contained in this skin.</summary>
		public ICollection<SkinEntry> Attachments { get { return attachments.Values; } }
		public ExposedList<BoneData> Bones { get { return bones; } }
		public ExposedList<IConstraintData> Constraints { get { return constraints; } }

		public Skin (string name) {
			if (name == null) throw new ArgumentNullException("name", "name cannot be null.");
			this.name = name;
		}

		/// <summary>Adds an attachment to the skin for the specified slot index and placeholder name.</summary>
		public void SetAttachment (int slotIndex, string placeholderName, Attachment attachment) {
			if (attachment == null) throw new ArgumentNullException("attachment", "attachment cannot be null.");
			attachments[new SkinKey(slotIndex, placeholderName)] = new SkinEntry(slotIndex, placeholderName, attachment);
		}

		/// <summary>Adds all attachments, bones, and constraints from the specified skin to this skin.</summary>
		public void AddSkin (Skin skin) {
			foreach (BoneData data in skin.bones)
				if (!bones.Contains(data)) bones.Add(data);

			foreach (IConstraintData data in skin.constraints)
				if (!constraints.Contains(data)) constraints.Add(data);

			foreach (KeyValuePair<SkinKey, SkinEntry> item in skin.attachments) {
				SkinEntry entry = item.Value;
				SetAttachment(entry.slotIndex, entry.placeholderName, entry.attachment);
			}
		}

		/// <summary>Adds all attachments from the specified skin to this skin. Attachments are deep copied.</summary>
		public void CopySkin (Skin skin) {
			foreach (BoneData data in skin.bones)
				if (!bones.Contains(data)) bones.Add(data);

			foreach (IConstraintData data in skin.constraints)
				if (!constraints.Contains(data)) constraints.Add(data);

			foreach (KeyValuePair<SkinKey, SkinEntry> item in skin.attachments) {
				SkinEntry entry = item.Value;
				if (entry.attachment is MeshAttachment) {
					SetAttachment(entry.slotIndex, entry.placeholderName,
					   entry.attachment != null ? ((MeshAttachment)entry.attachment).NewLinkedMesh() : null);
				} else
					SetAttachment(entry.slotIndex, entry.placeholderName, entry.attachment != null ? entry.attachment.Copy() : null);
			}
		}

		/// <summary>Returns the attachment for the specified slot index and placeholder name, or null.</summary>
		/// <returns>May be null.</returns>
		public Attachment GetAttachment (int slotIndex, string placeholderName) {
			SkinEntry entry;
			bool containsKey = attachments.TryGetValue(new SkinKey(slotIndex, placeholderName), out entry);
			return containsKey ? entry.attachment : null;
		}

		/// <summary> Removes the attachment in the skin for the specified slot index and placeholder name, if any.</summary>
		public void RemoveAttachment (int slotIndex, string placeholderName) {
			attachments.Remove(new SkinKey(slotIndex, placeholderName));
		}

		/// <summary>Returns all attachments in this skin for the specified slot index.</summary>
		/// <param name="slotIndex">The target slotIndex. To find the slot index, use <see cref="Spine.SkeletonData.FindSlot"/> and <see cref="Spine.SlotData.Index"/></param>.
		public void GetAttachments (int slotIndex, List<SkinEntry> attachments) {
			if (slotIndex < 0) throw new ArgumentException("slotIndex must be >= 0.");
			if (attachments == null) throw new ArgumentNullException("attachments", "attachments cannot be null.");
			foreach (KeyValuePair<SkinKey, SkinEntry> item in this.attachments) {
				SkinEntry entry = item.Value;
				if (entry.slotIndex == slotIndex) attachments.Add(entry);
			}
		}

		/// <summary>Clears all attachments, bones, and constraints.</summary>
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
			Slot[] slots = skeleton.slots.Items;
			foreach (KeyValuePair<SkinKey, SkinEntry> item in oldSkin.attachments) {
				SkinEntry entry = item.Value;
				SlotPose slot = slots[entry.slotIndex].pose;
				if (slot.Attachment == entry.attachment) {
					Attachment attachment = GetAttachment(entry.slotIndex, entry.placeholderName);
					if (attachment != null) slot.Attachment = attachment;
				}
			}
		}

		// Difference to reference implementation: using Dictionary<SkinKey, SkinEntry> instead of HashSet<SkinEntry>.
		/// <summary>Stores an entry in the skin consisting of the slot index, placeholder name and attachment.</summary>
		public struct SkinEntry {
			internal readonly int slotIndex;
			internal readonly string placeholderName;
			internal readonly Attachment attachment;

			public SkinEntry (int slotIndex, string placeholderName, Attachment attachment) {
				this.slotIndex = slotIndex;
				this.placeholderName = placeholderName;
				this.attachment = attachment;
			}

			/// <summary>The <see cref="Skeleton.Slots"/> index.</summary>
			public int SlotIndex {
				get {
					return slotIndex;
				}
			}

			/// <summary>The placeholder name that the attachment is associated with.</summary>
			public string PlaceholderName {
				get {
					return placeholderName;
				}
			}

			/// <summary>The attachment for this skin entry.</summary>
			public Attachment Attachment {
				get {
					return attachment;
				}
			}
		}

		private struct SkinKey {
			internal readonly int slotIndex;
			internal readonly string placeholderName;
			internal readonly int hashCode;

			public SkinKey (int slotIndex, string placeholderName) {
				if (slotIndex < 0) throw new ArgumentException("slotIndex must be >= 0.");
				if (placeholderName == null) throw new ArgumentNullException("placeholderName", "placeholderName cannot be null");
				this.slotIndex = slotIndex;
				this.placeholderName = placeholderName;
				this.hashCode = placeholderName.GetHashCode() + slotIndex * 37;
			}
		}

		class SkinKeyComparer : IEqualityComparer<SkinKey> {
			internal static readonly SkinKeyComparer Instance = new SkinKeyComparer();

			bool IEqualityComparer<SkinKey>.Equals (SkinKey e1, SkinKey e2) {
				return e1.slotIndex == e2.slotIndex && string.Equals(e1.placeholderName, e2.placeholderName, StringComparison.Ordinal);
			}

			int IEqualityComparer<SkinKey>.GetHashCode (SkinKey e) {
				return e.hashCode;
			}
		}
	}
}
