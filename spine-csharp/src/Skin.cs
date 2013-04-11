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
