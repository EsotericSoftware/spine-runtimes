/******************************************************************************
 * Spine Runtime Software License - Version 1.0
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Single User License or Spine Professional License must be
 *    purchased from Esoteric Software and the license must remain valid:
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
	public class Skeleton {
		internal SkeletonData data;
		internal List<Bone> bones;
		internal List<Slot> slots;
		internal List<Slot> drawOrder;
		internal Skin skin;
		internal float r = 1, g = 1, b = 1, a = 1;
		internal float time;
		internal bool flipX, flipY;
		internal float x, y;

		public SkeletonData Data { get { return data; } }
		public List<Bone> Bones { get { return bones; } }
		public List<Slot> Slots { get { return slots; } }
		public List<Slot> DrawOrder { get { return drawOrder; } }
		public Skin Skin { get { return skin; } set { skin = value; } }
		public float R { get { return r; } set { r = value; } }
		public float G { get { return g; } set { g = value; } }
		public float B { get { return b; } set { b = value; } }
		public float A { get { return a; } set { a = value; } }
		public float Time { get { return time; } set { time = value; } }
		public bool FlipX { get { return flipX; } set { flipX = value; } }
		public bool FlipY { get { return flipY; } set { flipY = value; } }
		public float X { get { return x; } set { x = value; } }
		public float Y { get { return y; } set { y = value; } }

		public Bone RootBone {
			get {
				return bones.Count == 0 ? null : bones[0];
			}
		}

		public Skeleton (SkeletonData data) {
			if (data == null) throw new ArgumentNullException("data cannot be null.");
			this.data = data;

			bones = new List<Bone>(data.bones.Count);
			foreach (BoneData boneData in data.bones) {
				Bone parent = boneData.parent == null ? null : bones[data.bones.IndexOf(boneData.parent)];
				bones.Add(new Bone(boneData, parent));
			}

			slots = new List<Slot>(data.slots.Count);
			drawOrder = new List<Slot>(data.slots.Count);
			foreach (SlotData slotData in data.slots) {
				Bone bone = bones[data.bones.IndexOf(slotData.boneData)];
				Slot slot = new Slot(slotData, this, bone);
				slots.Add(slot);
				drawOrder.Add(slot);
			}
		}

		/** Updates the world transform for each bone. */
		public void UpdateWorldTransform () {
			bool flipX = this.flipX;
			bool flipY = this.flipY;
			List<Bone> bones = this.bones;
			for (int i = 0, n = bones.Count; i < n; i++)
				bones[i].UpdateWorldTransform(flipX, flipY);
		}

		/** Sets the bones and slots to their setup pose values. */
		public void SetToSetupPose () {
			SetBonesToSetupPose();
			SetSlotsToSetupPose();
		}

		public void SetBonesToSetupPose () {
			List<Bone> bones = this.bones;
			for (int i = 0, n = bones.Count; i < n; i++)
				bones[i].SetToSetupPose();
		}

		public void SetSlotsToSetupPose () {
			List<Slot> slots = this.slots;
			for (int i = 0, n = slots.Count; i < n; i++)
				slots[i].SetToSetupPose(i);
		}

		/** @return May be null. */
		public Bone FindBone (String boneName) {
			if (boneName == null) throw new ArgumentNullException("boneName cannot be null.");
			List<Bone> bones = this.bones;
			for (int i = 0, n = bones.Count; i < n; i++) {
				Bone bone = bones[i];
				if (bone.data.name == boneName) return bone;
			}
			return null;
		}

		/** @return -1 if the bone was not found. */
		public int FindBoneIndex (String boneName) {
			if (boneName == null) throw new ArgumentNullException("boneName cannot be null.");
			List<Bone> bones = this.bones;
			for (int i = 0, n = bones.Count; i < n; i++)
				if (bones[i].data.name == boneName) return i;
			return -1;
		}

		/** @return May be null. */
		public Slot FindSlot (String slotName) {
			if (slotName == null) throw new ArgumentNullException("slotName cannot be null.");
			List<Slot> slots = this.slots;
			for (int i = 0, n = slots.Count; i < n; i++) {
				Slot slot = slots[i];
				if (slot.data.name == slotName) return slot;
			}
			return null;
		}

		/** @return -1 if the bone was not found. */
		public int FindSlotIndex (String slotName) {
			if (slotName == null) throw new ArgumentNullException("slotName cannot be null.");
			List<Slot> slots = this.slots;
			for (int i = 0, n = slots.Count; i < n; i++)
				if (slots[i].data.name.Equals(slotName)) return i;
			return -1;
		}

		/** Sets a skin by name.
		 * @see #setSkin(Skin) */
		public void SetSkin (String skinName) {
			Skin skin = data.FindSkin(skinName);
			if (skin == null) throw new ArgumentException("Skin not found: " + skinName);
			SetSkin(skin);
		}

		/** Sets the skin used to look up attachments not found in the {@link SkeletonData#getDefaultSkin() default skin}. Attachments
	 * from the new skin are attached if the corresponding attachment from the old skin was attached.
	 * @param newSkin May be null. */
		public void SetSkin (Skin newSkin) {
			if (skin != null && newSkin != null) newSkin.AttachAll(this, skin);
			skin = newSkin;
		}

		/** @return May be null. */
		public Attachment GetAttachment (String slotName, String attachmentName) {
			return GetAttachment(data.FindSlotIndex(slotName), attachmentName);
		}

		/** @return May be null. */
		public Attachment GetAttachment (int slotIndex, String attachmentName) {
			if (attachmentName == null) throw new ArgumentNullException("attachmentName cannot be null.");
			if (skin != null) {
				Attachment attachment = skin.GetAttachment(slotIndex, attachmentName);
				if (attachment != null) return attachment;
			}
			if (data.defaultSkin != null) return data.defaultSkin.GetAttachment(slotIndex, attachmentName);
			return null;
		}

		/** @param attachmentName May be null. */
		public void SetAttachment (String slotName, String attachmentName) {
			if (slotName == null) throw new ArgumentNullException("slotName cannot be null.");
			List<Slot> slots = this.slots;
			for (int i = 0, n = slots.Count; i < n; i++) {
				Slot slot = slots[i];
				if (slot.data.name == slotName) {
					Attachment attachment = null;
					if (attachmentName != null) {
						attachment = GetAttachment(i, attachmentName);
						if (attachment == null) throw new Exception("Attachment not found: " + attachmentName + ", for slot: " + slotName);
					}
					slot.Attachment = attachment;
					return;
				}
			}
			throw new Exception("Slot not found: " + slotName);
		}

		public void Update (float delta) {
			time += delta;
		}
	}
}
