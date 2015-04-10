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
            foreach(BoneData boneData in bones)
            {
                if(String.Compare(boneData.name, boneName, true) == 0) return boneData;
            }
            return null;
		}

		/// <returns>-1 if the bone was not found.</returns>
		public int FindBoneIndex (String boneName) {
			if (boneName == null) throw new ArgumentNullException("boneName cannot be null.");
			List<BoneData> bones = this.bones;
			for (int i = 0, n = bones.Count; i < n; i++)
            {
                BoneData boneData = bones[i];
                if(String.Compare(boneData.name, boneName, true) == 0) return i;
            }
			return -1;
		}

		// --- Slots.

		/// <returns>May be null.</returns>
		public SlotData FindSlot (String slotName) {
			if (slotName == null) throw new ArgumentNullException("slotName cannot be null.");
            foreach(SlotData slotData in slots)
            {
                if(String.Compare(slotData.name, slotName, true) == 0) return slotData;
            }
            return null;
		}

		/// <returns>-1 if the bone was not found.</returns>
		public int FindSlotIndex (String slotName) {
			if (slotName == null) throw new ArgumentNullException("slotName cannot be null.");
			List<SlotData> slots = this.slots;
			for (int i = 0, n = slots.Count; i < n; i++)
            {
                SlotData slotData = slots[i];
                if(String.Compare(slotData.name, slotName, true) == 0) return i;
            }
			return -1;
		}

		// --- Skins.
		
		/// <returns>May be null.</returns>
		public Skin FindSkin (String skinName) {
			if (skinName == null) throw new ArgumentNullException("skinName cannot be null.");
			foreach (Skin skin in skins)
            {
                if(String.Compare(skin.name, skinName, true) == 0) return skin;
            }
			return null;
		}

		// --- Events.

		/// <returns>May be null.</returns>
		public EventData FindEvent (String eventDataName) {
			if (eventDataName == null) throw new ArgumentNullException("eventDataName cannot be null.");
			foreach (EventData eventData in events)
            {
                if(String.Compare(eventData.name, eventDataName, true) == 0) return eventData;
            }
			return null;
		}

		// --- Animations.
		
		/// <returns>May be null.</returns>
		public Animation FindAnimation (String animationName) {
			if (animationName == null) throw new ArgumentNullException("animationName cannot be null.");
            foreach(Animation animation in this.animations)
            {
                if(String.Compare(animation.name, animationName, true) == 0) return animation;
            }
			return null;
		}

		// --- IK constraints.

		/// <returns>May be null.</returns>
		public IkConstraintData FindIkConstraint (String ikConstraintName) {
			if (ikConstraintName == null) throw new ArgumentNullException("ikConstraintName cannot be null.");
            foreach(IkConstraintData ikConstraint in this.ikConstraints)
            {
                if(String.Compare(ikConstraint.name, ikConstraintName, true) == 0) return ikConstraint;
            }
            return null;
		}

		// ---

		override public String ToString () {
			return name ?? base.ToString();
		}
	}
}
