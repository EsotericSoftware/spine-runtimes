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
using System.Collections.Generic;

namespace Spine {
	/// <summary>Stores attachments by slot index and attachment name.</summary>
	public class Skin : IEqualityComparer<KeyValuePair<int, String>> {
		internal String name;

		public String Name { get { return name; } }
		private Dictionary<KeyValuePair<int, String>, Attachment> attachments;

		public Skin (String name) {
			if (name == null) throw new ArgumentNullException("name cannot be null.");
			this.attachments = new Dictionary<KeyValuePair<int, String>, Attachment>(this);
			this.name = name;
		}

		public void AddAttachment (int slotIndex, String name, Attachment attachment) {
			if (attachment == null) throw new ArgumentNullException("attachment cannot be null.");
			attachments.Add(new KeyValuePair<int, String>(slotIndex, name), attachment);
		}

		/// <returns>May be null.</returns>
		public Attachment GetAttachment (int slotIndex, String name) {
			Attachment attachment;
			attachments.TryGetValue(new KeyValuePair<int, String>(slotIndex, name), out attachment);
			return attachment;
		}

		public void FindNamesForSlot (int slotIndex, List<String> names) {
			if (names == null) throw new ArgumentNullException("names cannot be null.");
			foreach (KeyValuePair<int, String> key in attachments.Keys)
				if (key.Key == slotIndex) names.Add(key.Value);
		}

		public void FindAttachmentsForSlot (int slotIndex, List<Attachment> attachments) {
			if (attachments == null) throw new ArgumentNullException("attachments cannot be null.");
			foreach (KeyValuePair<KeyValuePair<int, String>, Attachment> entry in this.attachments)
				if (entry.Key.Key == slotIndex) attachments.Add(entry.Value);
		}

		override public String ToString () {
			return name;
		}

		/// <summary>Attach all attachments from this skin if the corresponding attachment from the old skin is currently attached.</summary>
		internal void AttachAll (Skeleton skeleton, Skin oldSkin) {
			foreach (KeyValuePair<KeyValuePair<int, String>, Attachment> entry in oldSkin.attachments) {
				int slotIndex = entry.Key.Key;
				Slot slot = skeleton.slots[slotIndex];
				if (slot.attachment == entry.Value) {
					Attachment attachment = GetAttachment(slotIndex, entry.Key.Value);
					if (attachment != null) slot.Attachment = attachment;
				}
			}
		}

		bool IEqualityComparer<KeyValuePair<int, string>>.Equals(KeyValuePair<int, string> x, KeyValuePair<int, string> y)
		{
			return (x.Key == y.Key) && (x.Value == y.Value);
		}

		int IEqualityComparer<KeyValuePair<int, string>>.GetHashCode(KeyValuePair<int, string> obj)
		{
			// KeyValuePair picks the hashcode for the first item in the struct, so we do it here and avoid boxing.
			return obj.Key.GetHashCode();
		}
	}
}
