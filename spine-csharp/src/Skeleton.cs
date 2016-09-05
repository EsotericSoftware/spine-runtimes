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
	public class Skeleton {
		internal SkeletonData data;
		internal ExposedList<Bone> bones;
		internal ExposedList<Slot> slots;
		internal ExposedList<Slot> drawOrder;
		internal ExposedList<IkConstraint> ikConstraints, ikConstraintsSorted;
		internal ExposedList<TransformConstraint> transformConstraints;
		internal ExposedList<PathConstraint> pathConstraints;
		internal ExposedList<IUpdatable> updateCache = new ExposedList<IUpdatable>();
		internal Skin skin;
		internal float r = 1, g = 1, b = 1, a = 1;
		internal float time;
		internal bool flipX, flipY;
		internal float x, y;

		public SkeletonData Data { get { return data; } }
		public ExposedList<Bone> Bones { get { return bones; } }
		public ExposedList<IUpdatable> UpdateCacheList { get { return updateCache; } }
		public ExposedList<Slot> Slots { get { return slots; } }
		public ExposedList<Slot> DrawOrder { get { return drawOrder; } }
		public ExposedList<IkConstraint> IkConstraints { get { return ikConstraints; } }
		public ExposedList<PathConstraint> PathConstraints { get { return pathConstraints; } }
		public ExposedList<TransformConstraint> TransformConstraints { get { return transformConstraints; } }
		public Skin Skin { get { return skin; } set { skin = value; } }
		public float R { get { return r; } set { r = value; } }
		public float G { get { return g; } set { g = value; } }
		public float B { get { return b; } set { b = value; } }
		public float A { get { return a; } set { a = value; } }
		public float Time { get { return time; } set { time = value; } }
		public float X { get { return x; } set { x = value; } }
		public float Y { get { return y; } set { y = value; } }
		public bool FlipX { get { return flipX; } set { flipX = value; } }
		public bool FlipY { get { return flipY; } set { flipY = value; } }

		public Bone RootBone {
			get { return bones.Count == 0 ? null : bones.Items[0]; }
		}

		public Skeleton (SkeletonData data) {
			if (data == null) throw new ArgumentNullException("data", "data cannot be null.");
			this.data = data;

			bones = new ExposedList<Bone>(data.bones.Count);
			foreach (BoneData boneData in data.bones) {
				Bone bone;
				if (boneData.parent == null) {
					bone = new Bone(boneData, this, null);				
				} else {
					Bone parent = bones.Items[boneData.parent.index];
					bone = new Bone(boneData, this, parent);
					parent.children.Add(bone);
				}
				bones.Add(bone);
			}

			slots = new ExposedList<Slot>(data.slots.Count);
			drawOrder = new ExposedList<Slot>(data.slots.Count);
			foreach (SlotData slotData in data.slots) {
				Bone bone = bones.Items[slotData.boneData.index];
				Slot slot = new Slot(slotData, bone);
				slots.Add(slot);
				drawOrder.Add(slot);
			}

			ikConstraints = new ExposedList<IkConstraint>(data.ikConstraints.Count);
			ikConstraintsSorted = new ExposedList<IkConstraint>(data.ikConstraints.Count);
			foreach (IkConstraintData ikConstraintData in data.ikConstraints)
				ikConstraints.Add(new IkConstraint(ikConstraintData, this));

			transformConstraints = new ExposedList<TransformConstraint>(data.transformConstraints.Count);
			foreach (TransformConstraintData transformConstraintData in data.transformConstraints)
				transformConstraints.Add(new TransformConstraint(transformConstraintData, this));

			pathConstraints = new ExposedList<PathConstraint> (data.pathConstraints.Count);
			foreach (PathConstraintData pathConstraintData in data.pathConstraints)
				pathConstraints.Add(new PathConstraint(pathConstraintData, this));

			UpdateCache();
			UpdateWorldTransform();
		}

		/// <summary>Caches information about bones and constraints. Must be called if bones, constraints or weighted path attachments are added
		/// or removed.</summary>
		public void UpdateCache () {
			ExposedList<IUpdatable> updateCache = this.updateCache;
			updateCache.Clear();

			ExposedList<Bone> bones = this.bones;
			for (int i = 0, n = bones.Count; i < n; i++)
				bones.Items[i].sorted = false;

			ExposedList<IkConstraint> ikConstraints = this.ikConstraintsSorted;
			ikConstraints.Clear();
			ikConstraints.AddRange(this.ikConstraints);
			int ikCount = ikConstraints.Count;
			for (int i = 0, level, n = ikCount; i < n; i++) {
				IkConstraint ik = ikConstraints.Items[i];
				Bone bone = ik.bones.Items[0].parent;
				for (level = 0; bone != null; level++)
					bone = bone.parent;
				ik.level = level;
			}
			for (int i = 1, ii; i < ikCount; i++) {
				IkConstraint ik = ikConstraints.Items[i];
				int level = ik.level;
				for (ii = i - 1; ii >= 0; ii--) {
					IkConstraint other = ikConstraints.Items[ii];
					if (other.level < level) break;
					ikConstraints.Items[ii + 1] = other;
				}
				ikConstraints.Items[ii + 1] = ik;
			}
			for (int i = 0, n = ikConstraints.Count; i < n; i++) {
				IkConstraint constraint = ikConstraints.Items[i];
				Bone target = constraint.target;
				SortBone(target);

				ExposedList<Bone> constrained = constraint.bones;
				Bone parent = constrained.Items[0];
				SortBone(parent);

				updateCache.Add(constraint);

				SortReset(parent.children);
				constrained.Items[constrained.Count - 1].sorted = true;
			}

			ExposedList<PathConstraint> pathConstraints = this.pathConstraints;
			for (int i = 0, n = pathConstraints.Count; i < n; i++) {
				PathConstraint constraint = pathConstraints.Items[i];

				Slot slot = constraint.target;
				int slotIndex = slot.data.index;
				Bone slotBone = slot.bone;
				if (skin != null) SortPathConstraintAttachment(skin, slotIndex, slotBone);
				if (data.defaultSkin != null && data.defaultSkin != skin)
					SortPathConstraintAttachment(data.defaultSkin, slotIndex, slotBone);
				for (int ii = 0, nn = data.skins.Count; ii < nn; ii++)
					SortPathConstraintAttachment(data.skins.Items[ii], slotIndex, slotBone);

				PathAttachment attachment = slot.Attachment as PathAttachment;
				if (attachment != null) SortPathConstraintAttachment(attachment, slotBone);

				ExposedList<Bone> constrained = constraint.bones;
				int boneCount = constrained.Count;
				for (int ii = 0; ii < boneCount; ii++)
					SortBone(constrained.Items[ii]);

				updateCache.Add(constraint);

				for (int ii = 0; ii < boneCount; ii++)
					SortReset(constrained.Items[ii].children);
				for (int ii = 0; ii < boneCount; ii++)
					constrained.Items[ii].sorted = true;
			}

			ExposedList<TransformConstraint> transformConstraints = this.transformConstraints;
			for (int i = 0, n = transformConstraints.Count; i < n; i++) {
				TransformConstraint constraint = transformConstraints.Items[i];

				SortBone(constraint.target);

				ExposedList<Bone> constrained = constraint.bones;
				int boneCount = constrained.Count;
				for (int ii = 0; ii < boneCount; ii++)
					SortBone(constrained.Items[ii]);

				updateCache.Add(constraint);

				for (int ii = 0; ii < boneCount; ii++)
					SortReset(constrained.Items[ii].children);
				for (int ii = 0; ii < boneCount; ii++)
					constrained.Items[ii].sorted = true;
			}

			for (int i = 0, n = bones.Count; i < n; i++)
				SortBone(bones.Items[i]);
		}

		private void SortPathConstraintAttachment (Skin skin, int slotIndex, Bone slotBone) {
			foreach (var entry in skin.Attachments)
				if (entry.Key.slotIndex == slotIndex) SortPathConstraintAttachment(entry.Value, slotBone);
		}

		private void SortPathConstraintAttachment (Attachment attachment, Bone slotBone) {
			var pathAttachment = attachment as PathAttachment;
			if (pathAttachment == null) return;
			int[] pathBones = pathAttachment.bones;
			if (pathBones == null)
				SortBone(slotBone);
			else {
				var bones = this.bones;
				for (int i = 0, n = pathBones.Length; i < n; i++)
					SortBone(bones.Items[pathBones[i]]);
			}
		}

		private void SortBone (Bone bone) {
			if (bone.sorted) return;
			Bone parent = bone.parent;
			if (parent != null) SortBone(parent);
			bone.sorted = true;
			updateCache.Add(bone);
		}

		private void SortReset (ExposedList<Bone> bones) {
			var bonesItems = bones.Items;
			for (int i = 0, n = bones.Count; i < n; i++) {
				Bone bone = bonesItems[i];
				if (bone.sorted) SortReset(bone.children);
				bone.sorted = false;
			}
		}

		/// <summary>Updates the world transform for each bone and applies constraints.</summary>
		public void UpdateWorldTransform () {
			var updateItems = this.updateCache.Items;
			for (int i = 0, n = updateCache.Count; i < n; i++)
				updateItems[i].Update();
		}

		/// <summary>Sets the bones, constraints, and slots to their setup pose values.</summary>
		public void SetToSetupPose () {
			SetBonesToSetupPose();
			SetSlotsToSetupPose();
		}

		/// <summary>Sets the bones and constraints to their setup pose values.</summary>
		public void SetBonesToSetupPose () {
			var bonesItems = this.bones.Items;
			for (int i = 0, n = bones.Count; i < n; i++)
				bonesItems[i].SetToSetupPose();

			var ikConstraintsItems = this.ikConstraints.Items;
			for (int i = 0, n = ikConstraints.Count; i < n; i++) {
				IkConstraint constraint = ikConstraintsItems[i];
				constraint.bendDirection = constraint.data.bendDirection;
				constraint.mix = constraint.data.mix;
			}

			var transformConstraintsItems = this.transformConstraints.Items;
			for (int i = 0, n = transformConstraints.Count; i < n; i++) {
				TransformConstraint constraint = transformConstraintsItems[i];
				TransformConstraintData data = constraint.data;
				constraint.rotateMix = data.rotateMix;
				constraint.translateMix = data.translateMix;
				constraint.scaleMix = data.scaleMix;
				constraint.shearMix = data.shearMix;
			}

			var pathConstraintItems = this.pathConstraints.Items;
			for (int i = 0, n = pathConstraints.Count; i < n; i++) {
				PathConstraint constraint = pathConstraintItems[i];
				PathConstraintData data = constraint.data;
				constraint.position = data.position;
				constraint.spacing = data.spacing;
				constraint.rotateMix = data.rotateMix;
				constraint.translateMix = data.translateMix;
			}
		}

		public void SetSlotsToSetupPose () {
			var slots = this.slots;
			var slotsItems = slots.Items;
			drawOrder.Clear();
			for (int i = 0, n = slots.Count; i < n; i++)
				drawOrder.Add(slotsItems[i]);

			for (int i = 0, n = slots.Count; i < n; i++)
				slotsItems[i].SetToSetupPose();
		}

		/// <returns>May be null.</returns>
		public Bone FindBone (String boneName) {
			if (boneName == null) throw new ArgumentNullException("boneName", "boneName cannot be null.");
			var bones = this.bones;
			var bonesItems = bones.Items;
			for (int i = 0, n = bones.Count; i < n; i++) {
				Bone bone = bonesItems[i];
				if (bone.data.name == boneName) return bone;
			}
			return null;
		}

		/// <returns>-1 if the bone was not found.</returns>
		public int FindBoneIndex (String boneName) {
			if (boneName == null) throw new ArgumentNullException("boneName", "boneName cannot be null.");
			var bones = this.bones;
			var bonesItems = bones.Items;
			for (int i = 0, n = bones.Count; i < n; i++)
				if (bonesItems[i].data.name == boneName) return i;
			return -1;
		}

		/// <returns>May be null.</returns>
		public Slot FindSlot (String slotName) {
			if (slotName == null) throw new ArgumentNullException("slotName", "slotName cannot be null.");
			var slots = this.slots;
			var slotsItems = slots.Items;
			for (int i = 0, n = slots.Count; i < n; i++) {
				Slot slot = slotsItems[i];
				if (slot.data.name == slotName) return slot;
			}
			return null;
		}

		/// <returns>-1 if the bone was not found.</returns>
		public int FindSlotIndex (String slotName) {
			if (slotName == null) throw new ArgumentNullException("slotName", "slotName cannot be null.");
			var slots = this.slots;
			var slotsItems = slots.Items;
			for (int i = 0, n = slots.Count; i < n; i++)
				if (slotsItems[i].data.name.Equals(slotName)) return i;
			return -1;
		}

		/// <summary>Sets a skin by name (see SetSkin).</summary>
		public void SetSkin (String skinName) {
			Skin skin = data.FindSkin(skinName);
			if (skin == null) throw new ArgumentException("Skin not found: " + skinName, "skinName");
			SetSkin(skin);
		}

		/// <summary>Sets the skin used to look up attachments before looking in the {@link SkeletonData#getDefaultSkin() default 
		/// skin}. Attachmentsfrom the new skin are attached if the corresponding attachment from the old skin was attached. If 
		/// there was no old skin, each slot's setup mode attachment is attached from the new skin.</summary>
		/// <param name="newSkin">May be null.</param>
		public void SetSkin (Skin newSkin) {
			if (newSkin != null) {
				if (skin != null)
					newSkin.AttachAll(this, skin);
				else {
					ExposedList<Slot> slots = this.slots;
					for (int i = 0, n = slots.Count; i < n; i++) {
						Slot slot = slots.Items[i];
						String name = slot.data.attachmentName;
						if (name != null) {
							Attachment attachment = newSkin.GetAttachment(i, name);
							if (attachment != null) slot.Attachment = attachment;
						}
					}
				}
			}
			skin = newSkin;
		}

		/// <returns>May be null.</returns>
		public Attachment GetAttachment (String slotName, String attachmentName) {
			return GetAttachment(data.FindSlotIndex(slotName), attachmentName);
		}

		/// <returns>May be null.</returns>
		public Attachment GetAttachment (int slotIndex, String attachmentName) {
			if (attachmentName == null) throw new ArgumentNullException("attachmentName", "attachmentName cannot be null.");
			if (skin != null) {
				Attachment attachment = skin.GetAttachment(slotIndex, attachmentName);
				if (attachment != null) return attachment;
			}
			if (data.defaultSkin != null) return data.defaultSkin.GetAttachment(slotIndex, attachmentName);
			return null;
		}

		/// <param name="attachmentName">May be null.</param>
		public void SetAttachment (String slotName, String attachmentName) {
			if (slotName == null) throw new ArgumentNullException("slotName", "slotName cannot be null.");
			ExposedList<Slot> slots = this.slots;
			for (int i = 0, n = slots.Count; i < n; i++) {
				Slot slot = slots.Items[i];
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
			
		/// <returns>May be null.</returns>
		public IkConstraint FindIkConstraint (String constraintName) {
			if (constraintName == null) throw new ArgumentNullException("constraintName", "constraintName cannot be null.");
			ExposedList<IkConstraint> ikConstraints = this.ikConstraints;
			for (int i = 0, n = ikConstraints.Count; i < n; i++) {
				IkConstraint ikConstraint = ikConstraints.Items[i];
				if (ikConstraint.data.name == constraintName) return ikConstraint;
			}
			return null;
		}

		/// <returns>May be null.</returns>
		public TransformConstraint FindTransformConstraint (String constraintName) {
			if (constraintName == null) throw new ArgumentNullException("constraintName", "constraintName cannot be null.");
			ExposedList<TransformConstraint> transformConstraints = this.transformConstraints;
			for (int i = 0, n = transformConstraints.Count; i < n; i++) {
				TransformConstraint transformConstraint = transformConstraints.Items[i];
				if (transformConstraint.data.name == constraintName) return transformConstraint;
			}
			return null;
		}

		/// <returns>May be null.</returns>
		public PathConstraint FindPathConstraint (String constraintName) {
			if (constraintName == null) throw new ArgumentNullException("constraintName", "constraintName cannot be null.");
			ExposedList<PathConstraint> pathConstraints = this.pathConstraints;
			for (int i = 0, n = pathConstraints.Count; i < n; i++) {
				PathConstraint constraint = pathConstraints.Items[i];
				if (constraint.data.name.Equals(constraintName)) return constraint;
			}
			return null;
		}

		public void Update (float delta) {
			time += delta;
		}
	}
}
