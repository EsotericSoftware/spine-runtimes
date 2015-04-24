/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;
using System.Collections.Generic;

namespace Spine {
	/// <summary>Stores attachments by slot index and attachment name.</summary>
	public class Skin {
		internal String name;
		private Dictionary<KeyValuePair<int, String>, Attachment> attachments =
			new Dictionary<KeyValuePair<int, String>, Attachment>(AttachmentComparer.Instance);

		public String Name { get { return name; } }

		public Skin (String name) {
			if (name == null) throw new ArgumentNullException("name cannot be null.");
			this.name = name;
		}

		public void AddAttachment (int slotIndex, String name, Attachment attachment) {
			if (attachment == null) throw new ArgumentNullException("attachment cannot be null.");
			attachments[new KeyValuePair<int, String>(slotIndex, name)] = attachment;
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

		// Avoids boxing in the dictionary.
		private class AttachmentComparer : IEqualityComparer<KeyValuePair<int, String>> {
			internal static readonly AttachmentComparer Instance = new AttachmentComparer();

			bool IEqualityComparer<KeyValuePair<int, string>>.Equals (KeyValuePair<int, string> o1, KeyValuePair<int, string> o2) {
				return o1.Key == o2.Key && o1.Value == o2.Value;
			}

			int IEqualityComparer<KeyValuePair<int, string>>.GetHashCode (KeyValuePair<int, string> o) {
				return o.Key;
			}
		}
	}
}
