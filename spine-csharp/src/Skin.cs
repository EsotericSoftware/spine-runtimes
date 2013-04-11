
using System;
using System.Collections.Generic;

namespace Spine {
	/** Stores attachments by slot index and attachment name. */
	public class Skin {
		public String Name { get; private set; }
		private Dictionary<Tuple<int, String>, Attachment> attachments = new Dictionary<Tuple<int, String>, Attachment>();

		public Skin (String name) {
			if (name == null) throw new ArgumentNullException("name cannot be null.");
			Name = name;
		}

		public void AddAttachment (int slotIndex, String name, Attachment attachment) {
			if (attachment == null) throw new ArgumentNullException("attachment cannot be null.");
			attachments.Add(Tuple.Create<int, String>(slotIndex, name), attachment);
		}

		/** @return May be null. */
		public Attachment GetAttachment (int slotIndex, String name) {
			return attachments[Tuple.Create<int, String>(slotIndex, name)];
		}

		public void FindNamesForSlot (int slotIndex, List<String> names) {
			if (names == null) throw new ArgumentNullException("names cannot be null.");
			foreach (Tuple<int, String> key in attachments.Keys)
				if (key.Item1 == slotIndex) names.Add(key.Item2);
		}

		public void FindAttachmentsForSlot (int slotIndex, List<Attachment> attachments) {
			if (attachments == null) throw new ArgumentNullException("attachments cannot be null.");
			foreach (KeyValuePair<Tuple<int, String>, Attachment> entry in this.attachments)
				if (entry.Key.Item1 == slotIndex) attachments.Add(entry.Value);
		}

		override public String ToString () {
			return Name;
		}

		/** Attach all attachments from this skin if the corresponding attachment from the old skin is currently attached. */
		internal void AttachAll (Skeleton skeleton, Skin oldSkin) {
			foreach (KeyValuePair<Tuple<int, String>, Attachment> entry in oldSkin.attachments) {
				int slotIndex = entry.Key.Item1;
				Slot slot = skeleton.Slots[slotIndex];
				if (slot.Attachment == entry.Value) {
					Attachment attachment = GetAttachment(slotIndex, entry.Key.Item2);
					if (attachment != null) slot.Attachment = attachment;
				}
			}
		}
	}
}
