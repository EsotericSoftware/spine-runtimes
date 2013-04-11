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
	public class SkeletonData {
		public String Name { get; set; }
		public List<BoneData> Bones { get; private set; } // Ordered parents first.
		public List<SlotData> Slots { get; private set; } // Bind pose draw order.
		public List<Skin> Skins { get; private set; }
		/** May be null. */
		public Skin DefaultSkin;
		public List<Animation> Animations { get; private set; }

		public SkeletonData () {
			Bones = new List<BoneData>();
			Slots = new List<SlotData>();
			Skins = new List<Skin>();
			Animations = new List<Animation>();
		}

		// --- Bones.

		public void AddBone (BoneData bone) {
			if (bone == null) throw new ArgumentNullException("bone cannot be null.");
			Bones.Add(bone);
		}


		/** @return May be null. */
		public BoneData FindBone (String boneName) {
			if (boneName == null) throw new ArgumentNullException("boneName cannot be null.");
			for (int i = 0, n = Bones.Count; i < n; i++) {
				BoneData bone = Bones[i];
				if (bone.Name == boneName) return bone;
			}
			return null;
		}

		/** @return -1 if the bone was not found. */
		public int FindBoneIndex (String boneName) {
			if (boneName == null) throw new ArgumentNullException("boneName cannot be null.");
			for (int i = 0, n = Bones.Count; i < n; i++)
				if (Bones[i].Name == boneName) return i;
			return -1;
		}

		// --- Slots.

		public void AddSlot (SlotData slot) {
			if (slot == null) throw new ArgumentNullException("slot cannot be null.");
			Slots.Add(slot);
		}

		/** @return May be null. */
		public SlotData FindSlot (String slotName) {
			if (slotName == null) throw new ArgumentNullException("slotName cannot be null.");
			for (int i = 0, n = Slots.Count; i < n; i++) {
				SlotData slot = Slots[i];
				if (slot.Name == slotName) return slot;
			}
			return null;
		}

		/** @return -1 if the bone was not found. */
		public int FindSlotIndex (String slotName) {
			if (slotName == null) throw new ArgumentNullException("slotName cannot be null.");
			for (int i = 0, n = Slots.Count; i < n; i++)
				if (Slots[i].Name == slotName) return i;
			return -1;
		}

		// --- Skins.

		public void AddSkin (Skin skin) {
			if (skin == null) throw new ArgumentNullException("skin cannot be null.");
			Skins.Add(skin);
		}

		/** @return May be null. */
		public Skin FindSkin (String skinName) {
			if (skinName == null) throw new ArgumentNullException("skinName cannot be null.");
			foreach (Skin skin in Skins)
				if (skin.Name == skinName) return skin;
			return null;
		}

		// --- Animations.

		public void AddAnimation (Animation animation) {
			if (animation == null) throw new ArgumentNullException("animation cannot be null.");
			Animations.Add(animation);
		}

		/** @return May be null. */
		public Animation FindAnimation (String animationName) {
			if (animationName == null) throw new ArgumentNullException("animationName cannot be null.");
			for (int i = 0, n = Animations.Count; i < n; i++) {
				Animation animation = Animations[i];
				if (animation.Name == animationName) return animation;
			}
			return null;
		}

		// ---

		override public String ToString () {
			return Name != null ? Name : base.ToString();
		}
	}
}
