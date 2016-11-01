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

using System;

namespace Spine {
	public class SkeletonData {
		internal string name;
		internal ExposedList<BoneData> bones = new ExposedList<BoneData>();
		internal ExposedList<SlotData> slots = new ExposedList<SlotData>();
		internal ExposedList<Skin> skins = new ExposedList<Skin>();
		internal Skin defaultSkin;
		internal ExposedList<EventData> events = new ExposedList<EventData>();
		internal ExposedList<Animation> animations = new ExposedList<Animation>();
		internal ExposedList<IkConstraintData> ikConstraints = new ExposedList<IkConstraintData>();
		internal ExposedList<TransformConstraintData> transformConstraints = new ExposedList<TransformConstraintData>();
		internal ExposedList<PathConstraintData> pathConstraints = new ExposedList<PathConstraintData>();
		internal float width, height;
		internal string version, hash;

		// Nonessential.
		internal float fps;
		internal string imagesPath;

		public String Name { get { return name; } set { name = value; } }
		public ExposedList<BoneData> Bones { get { return bones; } } // Ordered parents first.
		public ExposedList<SlotData> Slots { get { return slots; } } // Setup pose draw order.
		public ExposedList<Skin> Skins { get { return skins; } set { skins = value; } }
		/// <summary>May be null.</summary>
		public Skin DefaultSkin { get { return defaultSkin; } set { defaultSkin = value; } }
		public ExposedList<EventData> Events { get { return events; } set { events = value; } }
		public ExposedList<Animation> Animations { get { return animations; } set { animations = value; } }
		public ExposedList<IkConstraintData> IkConstraints { get { return ikConstraints; } set { ikConstraints = value; } }
		public ExposedList<TransformConstraintData> TransformConstraints { get { return transformConstraints; } set { transformConstraints = value; } }
		public ExposedList<PathConstraintData> PathConstraints { get { return pathConstraints; } set { pathConstraints = value; } }

		public float Width { get { return width; } set { width = value; } }
		public float Height { get { return height; } set { height = value; } }
		/// <summary>The Spine version used to export this data, or null.</summary>
		public string Version { get { return version; } set { version = value; } }
		public string Hash { get { return hash; } set { hash = value; } }
		public string ImagesPath { get { return imagesPath; } set { imagesPath = value; } }
		public float Fps { get { return fps; } set { fps = value; } }

		// --- Bones.

		/// <returns>May be null.</returns>
		public BoneData FindBone (String boneName) {
			if (boneName == null) throw new ArgumentNullException("boneName", "boneName cannot be null.");
			ExposedList<BoneData> bones = this.bones;
			for (int i = 0, n = bones.Count; i < n; i++) {
				BoneData bone = bones.Items[i];
				if (bone.name == boneName) return bone;
			}
			return null;
		}

		/// <returns>-1 if the bone was not found.</returns>
		public int FindBoneIndex (String boneName) {
			if (boneName == null) throw new ArgumentNullException("boneName", "boneName cannot be null.");
			ExposedList<BoneData> bones = this.bones;
			for (int i = 0, n = bones.Count; i < n; i++)
				if (bones.Items[i].name == boneName) return i;
			return -1;
		}

		// --- Slots.

		/// <returns>May be null.</returns>
		public SlotData FindSlot (String slotName) {
			if (slotName == null) throw new ArgumentNullException("slotName", "slotName cannot be null.");
			ExposedList<SlotData> slots = this.slots;
			for (int i = 0, n = slots.Count; i < n; i++) {
				SlotData slot = slots.Items[i];
				if (slot.name == slotName) return slot;
			}
			return null;
		}

		/// <returns>-1 if the slot was not found.</returns>
		public int FindSlotIndex (String slotName) {
			if (slotName == null) throw new ArgumentNullException("slotName", "slotName cannot be null.");
			ExposedList<SlotData> slots = this.slots;
			for (int i = 0, n = slots.Count; i < n; i++)
				if (slots.Items[i].name == slotName) return i;
			return -1;
		}

		// --- Skins.

		/// <returns>May be null.</returns>
		public Skin FindSkin (String skinName) {
			if (skinName == null) throw new ArgumentNullException("skinName", "skinName cannot be null.");
			foreach (Skin skin in skins)
				if (skin.name == skinName) return skin;
			return null;
		}

		// --- Events.

		/// <returns>May be null.</returns>
		public EventData FindEvent (String eventDataName) {
			if (eventDataName == null) throw new ArgumentNullException("eventDataName", "eventDataName cannot be null.");
			foreach (EventData eventData in events)
				if (eventData.name == eventDataName) return eventData;
			return null;
		}

		// --- Animations.

		/// <returns>May be null.</returns>
		public Animation FindAnimation (String animationName) {
			if (animationName == null) throw new ArgumentNullException("animationName", "animationName cannot be null.");
			ExposedList<Animation> animations = this.animations;
			for (int i = 0, n = animations.Count; i < n; i++) {
				Animation animation = animations.Items[i];
				if (animation.name == animationName) return animation;
			}
			return null;
		}

		// --- IK constraints.

		/// <returns>May be null.</returns>
		public IkConstraintData FindIkConstraint (String constraintName) {
			if (constraintName == null) throw new ArgumentNullException("constraintName", "constraintName cannot be null.");
			ExposedList<IkConstraintData> ikConstraints = this.ikConstraints;
			for (int i = 0, n = ikConstraints.Count; i < n; i++) {
				IkConstraintData ikConstraint = ikConstraints.Items[i];
				if (ikConstraint.name == constraintName) return ikConstraint;
			}
			return null;
		}

		// --- Transform constraints.

		/// <returns>May be null.</returns>
		public TransformConstraintData FindTransformConstraint (String constraintName) {
			if (constraintName == null) throw new ArgumentNullException("constraintName", "constraintName cannot be null.");
			ExposedList<TransformConstraintData> transformConstraints = this.transformConstraints;
			for (int i = 0, n = transformConstraints.Count; i < n; i++) {
				TransformConstraintData transformConstraint = transformConstraints.Items[i];
				if (transformConstraint.name == constraintName) return transformConstraint;
			}
			return null;
		}

		// --- Path constraints.

		/// <returns>May be null.</returns>
		public PathConstraintData FindPathConstraint (String constraintName) {
			if (constraintName == null) throw new ArgumentNullException("constraintName", "constraintName cannot be null.");
			ExposedList<PathConstraintData> pathConstraints = this.pathConstraints;
			for (int i = 0, n = pathConstraints.Count; i < n; i++) {
				PathConstraintData constraint = pathConstraints.Items[i];
				if (constraint.name.Equals(constraintName)) return constraint;
			}
			return null;
		}

		/// <returns>-1 if the path constraint was not found.</returns>
		public int FindPathConstraintIndex (String pathConstraintName) {
			if (pathConstraintName == null) throw new ArgumentNullException("pathConstraintName", "pathConstraintName cannot be null.");
			ExposedList<PathConstraintData> pathConstraints = this.pathConstraints;
			for (int i = 0, n = pathConstraints.Count; i < n; i++)
				if (pathConstraints.Items[i].name.Equals(pathConstraintName)) return i;
			return -1;
		}

		// ---

		override public String ToString () {
			return name ?? base.ToString();
		}
	}
}
