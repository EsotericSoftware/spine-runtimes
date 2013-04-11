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
	public class Skeleton {
		public SkeletonData Data { get; private set; }
		public List<Bone> Bones { get; private set; }
		public List<Slot> Slots { get; private set; }
		public List<Slot> DrawOrder { get; private set; }
		public Skin Skin { get; set; }
		public float R { get; set; }
		public float G { get; set; }
		public float B { get; set; }
		public float A { get; set; }
		public float Time { get; set; }
		public bool FlipX { get; set; }
		public bool FlipY { get; set; }
		public Bone RootBone {
			get {
				return Bones.Count == 0 ? null : Bones[0];
			}
		}

		public Skeleton (SkeletonData data) {
			if (data == null) throw new ArgumentNullException("data cannot be null.");
			Data = data;

			Bones = new List<Bone>(Data.Bones.Count);
			foreach (BoneData boneData in Data.Bones) {
				Bone parent = boneData.Parent == null ? null : Bones[Data.Bones.IndexOf(boneData.Parent)];
				Bones.Add(new Bone(boneData, parent));
			}

			Slots = new List<Slot>(Data.Slots.Count);
			DrawOrder = new List<Slot>(Data.Slots.Count);
			foreach (SlotData slotData in Data.Slots) {
				Bone bone = Bones[Data.Bones.IndexOf(slotData.BoneData)];
				Slot slot = new Slot(slotData, this, bone);
				Slots.Add(slot);
				DrawOrder.Add(slot);
			}

			R = 1;
			G = 1;
			B = 1;
			A = 1;
		}

		/** Updates the world transform for each bone. */
		public void UpdateWorldTransform () {
			bool flipX = FlipX;
			bool flipY = FlipY;
			List<Bone> bones = Bones;
			for (int i = 0, n = bones.Count; i < n; i++)
				bones[i].UpdateWorldTransform(flipX, flipY);
		}

		/** Sets the bones and slots to their bind pose values. */
		public void SetToBindPose () {
			SetBonesToBindPose();
			SetSlotsToBindPose();
		}

		public void SetBonesToBindPose () {
			List<Bone> bones = this.Bones;
			for (int i = 0, n = bones.Count; i < n; i++)
				bones[i].SetToBindPose();
		}

		public void SetSlotsToBindPose () {
			List<Slot> slots = this.Slots;
			for (int i = 0, n = slots.Count; i < n; i++)
				slots[i].SetToBindPose(i);
		}

		/** @return May be null. */
		public Bone FindBone (String boneName) {
			if (boneName == null) throw new ArgumentNullException("boneName cannot be null.");
			List<Bone> bones = this.Bones;
			for (int i = 0, n = bones.Count; i < n; i++) {
				Bone bone = bones[i];
				if (bone.Data.Name == boneName) return bone;
			}
			return null;
		}

		/** @return -1 if the bone was not found. */
		public int FindBoneIndex (String boneName) {
			if (boneName == null) throw new ArgumentNullException("boneName cannot be null.");
			List<Bone> bones = this.Bones;
			for (int i = 0, n = bones.Count; i < n; i++)
				if (bones[i].Data.Name == boneName) return i;
			return -1;
		}

		/** @return May be null. */
		public Slot FindSlot (String slotName) {
			if (slotName == null) throw new ArgumentNullException("slotName cannot be null.");
			List<Slot> slots = this.Slots;
			for (int i = 0, n = slots.Count; i < n; i++) {
				Slot slot = slots[i];
				if (slot.Data.Name == slotName) return slot;
			}
			return null;
		}

		/** @return -1 if the bone was not found. */
		public int FindSlotIndex (String slotName) {
			if (slotName == null) throw new ArgumentNullException("slotName cannot be null.");
			List<Slot> slots = this.Slots;
			for (int i = 0, n = slots.Count; i < n; i++)
				if (slots[i].Data.Name.Equals(slotName)) return i;
			return -1;
		}

		/** Sets a skin by name.
		 * @see #setSkin(Skin) */
		public void SetSkin (String skinName) {
			Skin skin = Data.FindSkin(skinName);
			if (skin == null) throw new ArgumentException("Skin not found: " + skinName);
			SetSkin(skin);
		}

		/** Sets the skin used to look up attachments not found in the {@link SkeletonData#getDefaultSkin() default skin}. Attachments
	 * from the new skin are attached if the corresponding attachment from the old skin was attached.
	 * @param newSkin May be null. */
		public void SetSkin (Skin newSkin) {
			if (Skin != null && newSkin != null) newSkin.AttachAll(this, Skin);
			Skin = newSkin;
		}

		/** @return May be null. */
		public Attachment GetAttachment (String slotName, String attachmentName) {
			return GetAttachment(Data.FindSlotIndex(slotName), attachmentName);
		}

		/** @return May be null. */
		public Attachment GetAttachment (int slotIndex, String attachmentName) {
			if (attachmentName == null) throw new ArgumentNullException("attachmentName cannot be null.");
			if (Skin != null) {
				Attachment attachment = Skin.GetAttachment(slotIndex, attachmentName);
				if (attachment != null) return attachment;
			}
			if (Data.DefaultSkin != null) return Data.DefaultSkin.GetAttachment(slotIndex, attachmentName);
			return null;
		}

		/** @param attachmentName May be null. */
		public void SetAttachment (String slotName, String attachmentName) {
			if (slotName == null) throw new ArgumentNullException("slotName cannot be null.");
			List<Slot> slots = Slots;
			for (int i = 0, n = slots.Count; i < n; i++) {
				Slot slot = slots[i];
				if (slot.Data.Name == slotName) {
					Attachment attachment = null;
					if (attachmentName != null) {
						attachment = GetAttachment(i, attachmentName);
						if (attachment == null) throw new ArgumentNullException("Attachment not found: " + attachmentName + ", for slot: " + slotName);
					}
					slot.Attachment = attachment;
					return;
				}
			}
			throw new Exception("Slot not found: " + slotName);
		}

		public void Update (float delta) {
			Time += delta;
		}
	}
}
