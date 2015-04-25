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
	public class SkeletonData {
		internal String name;
		internal List<BoneData> bones = new List<BoneData>();
		internal List<SlotData> slots = new List<SlotData>();
		internal List<Skin> skins = new List<Skin>();
		internal Skin defaultSkin;
		internal List<EventData> events = new List<EventData>();
		internal List<Animation> animations = new List<Animation>();
		internal List<IkConstraintData> ikConstraints = new List<IkConstraintData>();
		internal float width, height;
		internal String version, hash, imagesPath;

		public String Name { get { return name; } set { name = value; } }
		public List<BoneData> Bones { get { return bones; } } // Ordered parents first.
		public List<SlotData> Slots { get { return slots; } } // Setup pose draw order.
		public List<Skin> Skins { get { return skins; } set { skins = value; } }
		/// <summary>May be null.</summary>
		public Skin DefaultSkin { get { return defaultSkin; } set { defaultSkin = value; } }
		public List<EventData> Events { get { return events; } set { events = value; } }
		public List<Animation> Animations { get { return animations; } set { animations = value; } }
		public List<IkConstraintData> IkConstraints { get { return ikConstraints; } set { ikConstraints = value; } }
		public float Width { get { return width; } set { width = value; } }
		public float Height { get { return height; } set { height = value; } }
		/// <summary>The Spine version used to export this data.</summary>
		public String Version { get { return version; } set { version = value; } }
		public String Hash { get { return hash; } set { hash = value; } }

		// --- Bones.

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
		
		/// <returns>May be null.</returns>
		public Skin FindSkin (String skinName) {
			if (skinName == null) throw new ArgumentNullException("skinName cannot be null.");
			foreach (Skin skin in skins)
				if (skin.name == skinName) return skin;
			return null;
		}

		// --- Events.

		/// <returns>May be null.</returns>
		public EventData FindEvent (String eventDataName) {
			if (eventDataName == null) throw new ArgumentNullException("eventDataName cannot be null.");
			foreach (EventData eventData in events)
				if (eventData.name == eventDataName) return eventData;
			return null;
		}

		// --- Animations.
		
		/// <returns>May be null.</returns>
		public Animation FindAnimation (String animationName) {
			if (animationName == null) throw new ArgumentNullException("animationName cannot be null.");
			List<Animation> animations = this.animations;
			for (int i = 0, n = animations.Count; i < n; i++) {
				Animation animation = animations[i];
				if (animation.name == animationName) return animation;
			}
			return null;
		}

		// --- IK constraints.

		/// <returns>May be null.</returns>
		public IkConstraintData FindIkConstraint (String ikConstraintName) {
			if (ikConstraintName == null) throw new ArgumentNullException("ikConstraintName cannot be null.");
			List<IkConstraintData> ikConstraints = this.ikConstraints;
			for (int i = 0, n = ikConstraints.Count; i < n; i++) {
				IkConstraintData ikConstraint = ikConstraints[i];
				if (ikConstraint.name == ikConstraintName) return ikConstraint;
			}
			return null;
		}

		// ---

		override public String ToString () {
			return name ?? base.ToString();
		}
	}
}
