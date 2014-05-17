/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software (typically granted by licensing Spine), you
 * may not (a) modify, translate, adapt or otherwise create derivative works,
 * improvements of the Software or develop new applications using the Software
 * or (b) remove, delete, alter or obscure any trademarks or any copyright,
 * trademark, patent or other intellectual property or proprietary rights
 * notices on or in the Software, including any copy thereof. Redistributions
 * in binary or source form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
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
	public class SkeletonData {
		internal String name;
		internal List<BoneData> bones = new List<BoneData>();
		internal List<SlotData> slots = new List<SlotData>();
		internal List<Skin> skins = new List<Skin>();
		internal Skin defaultSkin;
		internal List<EventData> events = new List<EventData>();
		internal List<Animation> animations = new List<Animation>();

		public String Name { get { return name; } set { name = value; } }
		public List<BoneData> Bones { get { return bones; } } // Ordered parents first.
		public List<SlotData> Slots { get { return slots; } } // Setup pose draw order.
		public List<Skin> Skins { get { return skins; } set { skins = value; } }
		/// <summary>May be null.</summary>
		public Skin DefaultSkin { get { return defaultSkin; } set { defaultSkin = value; } }
		public List<EventData> Events { get { return events; } set { events = value; } }
		public List<Animation> Animations { get { return animations; } set { animations = value; } }

		// --- Bones.

		public void AddBone (BoneData bone) {
			if (bone == null) throw new ArgumentNullException("bone cannot be null.");
			bones.Add(bone);
		}


		/// <returns>May be null.</returns>
		public BoneData FindBone (String boneName) {
			if (boneName == null) throw new ArgumentNullException("boneName cannot be null.");
			List<BoneData> bones = this.bones;
			for (int i = 0, n = bones.Count; i < n; i++) {
				BoneData bone = bones[i];
				if (bone.name == boneName) return bone;
			}
			return null;
		}

		/// <returns>-1 if the bone was not found.</returns>
		public int FindBoneIndex (String boneName) {
			if (boneName == null) throw new ArgumentNullException("boneName cannot be null.");
			List<BoneData> bones = this.bones;
			for (int i = 0, n = bones.Count; i < n; i++)
				if (bones[i].name == boneName) return i;
			return -1;
		}

		// --- Slots.

		public void AddSlot (SlotData slot) {
			if (slot == null) throw new ArgumentNullException("slot cannot be null.");
			slots.Add(slot);
		}

		/// <returns>May be null.</returns>
		public SlotData FindSlot (String slotName) {
			if (slotName == null) throw new ArgumentNullException("slotName cannot be null.");
			List<SlotData> slots = this.slots;
			for (int i = 0, n = slots.Count; i < n; i++) {
				SlotData slot = slots[i];
				if (slot.name == slotName) return slot;
			}
			return null;
		}

		/// <returns>-1 if the bone was not found.</returns>
		public int FindSlotIndex (String slotName) {
			if (slotName == null) throw new ArgumentNullException("slotName cannot be null.");
			List<SlotData> slots = this.slots;
			for (int i = 0, n = slots.Count; i < n; i++)
				if (slots[i].name == slotName) return i;
			return -1;
		}

		// --- Skins.

		public void AddSkin (Skin skin) {
			if (skin == null) throw new ArgumentNullException("skin cannot be null.");
			skins.Add(skin);
		}

		/// <returns>May be null.</returns>
		public Skin FindSkin (String skinName) {
			if (skinName == null) throw new ArgumentNullException("skinName cannot be null.");
			foreach (Skin skin in skins)
				if (skin.name == skinName) return skin;
			return null;
		}

		// --- Events.

		public void AddEvent (EventData eventData) {
			if (eventData == null) throw new ArgumentNullException("eventData cannot be null.");
			events.Add(eventData);
		}

		/// <returns>May be null.</returns>
		public EventData FindEvent (String eventDataName) {
			if (eventDataName == null) throw new ArgumentNullException("eventDataName cannot be null.");
			foreach (EventData eventData in events)
				if (eventData.Name == eventDataName) return eventData;
			return null;
		}

		// --- Animations.

		public void AddAnimation (Animation animation) {
			if (animation == null) throw new ArgumentNullException("animation cannot be null.");
			animations.Add(animation);
		}

		/// <returns>May be null.</returns>
		public Animation FindAnimation (String animationName) {
			if (animationName == null) throw new ArgumentNullException("animationName cannot be null.");
			List<Animation> animations = this.animations;
			for (int i = 0, n = animations.Count; i < n; i++) {
				Animation animation = animations[i];
				if (animation.Name == animationName) return animation;
			}
			return null;
		}

		// ---

		override public String ToString () {
			return name ?? base.ToString();
		}
	}
}
