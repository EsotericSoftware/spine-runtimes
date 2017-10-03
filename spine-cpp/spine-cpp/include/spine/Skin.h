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

#ifndef Spine_Skin_h
#define Spine_Skin_h

namespace Spine
{
    /// Stores attachments by slot index and attachment name.
    /// See SkeletonData::getDefaultSkin, Skeleton::getSkin, and
    /// http://esotericsoftware.com/spine-runtime-skins in the Spine Runtimes Guide.
    class Skin
    {
    public:
        public std::string Name { get { return name; } }
        public Dictionary<AttachmentKeyTuple, Attachment> Attachments { get { return attachments; } }
        
        public Skin (std::string name) {
            if (name == null) throw new ArgumentNullException("name", "name cannot be null.");
            this.name = name;
        }
        
        /// Adds an attachment to the skin for the specified slot index and name. If the name already exists for the slot, the previous value is replaced.
        public void AddAttachment (int slotIndex, std::string name, Attachment attachment) {
            if (attachment == null) throw new ArgumentNullException("attachment", "attachment cannot be null.");
            attachments[new AttachmentKeyTuple(slotIndex, name)] = attachment;
        }
        
        /// Returns the attachment for the specified slot index and name, or null.
        /// <returns>May be null.</returns>
        public Attachment GetAttachment (int slotIndex, std::string name) {
            Attachment attachment;
            attachments.TryGetValue(new AttachmentKeyTuple(slotIndex, name), out attachment);
            return attachment;
        }
        
        /// Finds the skin keys for a given slot. The results are added to the passed List(names).
        /// <param name="slotIndex">The target slotIndex. To find the slot index, use <see cref="Spine.Skeleton.FindSlotIndex"/> or <see cref="Spine.SkeletonData.FindSlotIndex"/>
        /// <param name="names">Found skin key names will be added to this list.</param>
        public void FindNamesForSlot (int slotIndex, List<std::string> names) {
            if (names == null) throw new ArgumentNullException("names", "names cannot be null.");
            foreach (AttachmentKeyTuple key in attachments.Keys)
            if (key.slotIndex == slotIndex) names.Add(key.name);
        }
        
        /// Finds the attachments for a given slot. The results are added to the passed List(Attachment).
        /// <param name="slotIndex">The target slotIndex. To find the slot index, use <see cref="Spine.Skeleton.FindSlotIndex"/> or <see cref="Spine.SkeletonData.FindSlotIndex"/>
        /// <param name="attachments">Found Attachments will be added to this list.</param>
        public void FindAttachmentsForSlot (int slotIndex, List<Attachment> attachments) {
            if (attachments == null) throw new ArgumentNullException("attachments", "attachments cannot be null.");
            foreach (KeyValuePair<AttachmentKeyTuple, Attachment> entry in this.attachments)
            if (entry.Key.slotIndex == slotIndex) attachments.Add(entry.Value);
        }
        
        override public std::string ToString () {
            return name;
        }
        
    private:
        internal std::string name;
        Dictionary<AttachmentKeyTuple, Attachment> attachments = new Dictionary<AttachmentKeyTuple, Attachment>(AttachmentKeyTupleComparer.Instance);
        
        /// Attach all attachments from this skin if the corresponding attachment from the old skin is currently attached.
        internal void AttachAll (Skeleton skeleton, Skin oldSkin) {
            foreach (KeyValuePair<AttachmentKeyTuple, Attachment> entry in oldSkin.attachments) {
                int slotIndex = entry.Key.slotIndex;
                Slot slot = skeleton.slots.Items[slotIndex];
                if (slot.Attachment == entry.Value) {
                    Attachment attachment = GetAttachment(slotIndex, entry.Key.name);
                    if (attachment != null) slot.Attachment = attachment;
                }
            }
        }
        
        public struct AttachmentKeyTuple {
            public readonly int slotIndex;
            public readonly std::string name;
            internal readonly int nameHashCode;
            
            public AttachmentKeyTuple (int slotIndex, std::string name) {
                this.slotIndex = slotIndex;
                this.name = name;
                nameHashCode = this.name.GetHashCode();
            }
        }
        
        // Avoids boxing in the dictionary.
        class AttachmentKeyTupleComparer : IEqualityComparer<AttachmentKeyTuple> {
            internal static readonly AttachmentKeyTupleComparer Instance = new AttachmentKeyTupleComparer();
            
            bool IEqualityComparer<AttachmentKeyTuple>.Equals (AttachmentKeyTuple o1, AttachmentKeyTuple o2) {
                return o1.slotIndex == o2.slotIndex && o1.nameHashCode == o2.nameHashCode && o1.name == o2.name;
            }
            
            int IEqualityComparer<AttachmentKeyTuple>.GetHashCode (AttachmentKeyTuple o) {
                return o.slotIndex;
            }
        }
    };
}

#endif /* Spine_Skin_h */
